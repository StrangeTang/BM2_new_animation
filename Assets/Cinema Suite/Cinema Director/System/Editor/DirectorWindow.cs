using System;
using UnityEditor;
using UnityEngine;
using CinemaDirector;
using System.Timers;

/// <summary>
/// Timeline window that displays and allows edits to cutscenes
/// </summary>
public class DirectorWindow : EditorWindow
{
    private DirectorControl directorControl;
    private Cutscene cutscene;
    private CutsceneWrapper cutsceneWrapper;
    private DateTime previousTime;
    private bool isSnappingEnabled = false;
    private Cutscene[] cachedCutscenes;
    private static Timer timer;

    #region Language
    private const string URL = "http://www.cinema-suite.com";
    private const string PREVIEW_MODE = "Preview Mode";
    private const string CREATE = "Create";
    private GUIContent new_cutscene = new GUIContent("New Cutscene");

    const string TITLE = "Director";
    const string MENU_ITEM = "Window/Cinema Suite/Cinema Director/Director %#d";
    #endregion

    #region UI
    private Texture rescaleImage = null;
    private Texture zoomInImage = null;
    private Texture zoomOutImage = null;
    private Texture snapImage = null;
    private Texture rollingEditImage = null;
    private Texture rippleEditImage = null;
    private Texture pickerImage = null;
    private Texture refreshImage = null;

    private const float TOOLBAR_HEIGHT = 17f;
    private const string PRO_SKIN = "Director_LightSkin";
    private const string FREE_SKIN = "Director_DarkSkin";

    private const float FRAME_LIMITER = 1 / 100f;
    private double accumulatedTime = 0f;
    int popupSelection = 0;
    #endregion

    /// <summary>
    /// Sets the window title and minimum pane size
    /// </summary>
    public void Awake()
    {
        base.title = TITLE;
        this.minSize = new Vector2(400f, 250f);
        loadTextures();

        previousTime = DateTime.Now;
        accumulatedTime = 0;
    }

    /// <summary>
    /// Update the current cutscene
    /// </summary>
    private void DirectorUpdate()
    {
        // Restrict the Repaint rate
        double delta = (DateTime.Now - previousTime).TotalSeconds;
        previousTime = System.DateTime.Now;
        if (delta > 0)
        {
            accumulatedTime += delta;
        }
        if (accumulatedTime >= FRAME_LIMITER)
        {
            base.Repaint();
            accumulatedTime -= FRAME_LIMITER;
        }

        if (cutscene != null)
        {
            if (!Application.isPlaying && cutscene.State == Cutscene.CutsceneState.PreviewPlaying)
            {
                cutscene.UpdateCutscene((float)delta);
            }
        }
    }

    /// <summary>
    /// Spools the events and loads the timeline control
    /// </summary>
    protected void OnEnable()
    {
        EditorApplication.update = (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(this.DirectorUpdate));
        EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)System.Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(this.PlaymodeStateChanged));

        GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
        skin = (EditorGUIUtility.isProSkin) ? Resources.Load<GUISkin>(PRO_SKIN) : Resources.Load<GUISkin>(FREE_SKIN);

        directorControl = new DirectorControl();
        directorControl.OnLoad(skin);

        directorControl.PlayCutscene += directorControl_PlayCutscene;
        directorControl.PauseCutscene += directorControl_PauseCutscene;
        directorControl.StopCutscene += directorControl_StopCutscene;
        directorControl.ScrubCutscene += directorControl_ScrubCutscene;
        directorControl.SetCutsceneTime += directorControl_SetCutsceneTime;
        directorControl.EnterPreviewMode += directorControl_EnterPreviewMode;
        directorControl.ExitPreviewMode += directorControl_ExitPreviewMode;

        isSnappingEnabled = directorControl.IsSnappingEnabled;

        previousTime = DateTime.Now;
        accumulatedTime = 0;

        int instanceId = -1;
        if (EditorPrefs.HasKey("DirectorControl.CutsceneID"))
        {
            instanceId = EditorPrefs.GetInt("DirectorControl.CutsceneID");
        }

        if (instanceId >= 0)
        {
            foreach (Cutscene c in GameObject.FindObjectsOfType<Cutscene>())
            {
                if (c.GetInstanceID() == instanceId)
                {
                    cutscene = c;
                }
            }
        }
    }

    #region EventHandlers

    void directorControl_ExitPreviewMode(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.ExitPreviewMode();
        }
    }

    void directorControl_EnterPreviewMode(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.EnterPreviewMode();
        }
    }

    void directorControl_SetCutsceneTime(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.SetRunningTime(e.timeArg);
            cutsceneWrapper.RunningTime = e.timeArg;
        }
    }

    void directorControl_ScrubCutscene(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.ScrubToTime(e.timeArg);
        }
    }

    void directorControl_StopCutscene(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.Stop();
        }
    }

    void directorControl_PauseCutscene(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            c.Pause();
        }
    }

    void directorControl_PlayCutscene(object sender, CinemaDirectorArgs e)
    {
        Cutscene c = e.cutscene as Cutscene;
        if (c != null)
        {
            if (Application.isPlaying)
            {
                c.Play();
            }
            else
            {
                c.PreviewPlay();
            }
        }
    }

    #endregion

    /// <summary>
    /// Called when the object is destroyed
    /// </summary>
    protected void OnDestroy()
    {
    }

    /// <summary>
    /// Draws the GUI for the Timeline Window.
    /// </summary>
    protected void OnGUI()
    {
        Rect toolbarArea = new Rect(0, 0, base.position.width, TOOLBAR_HEIGHT);
        Rect controlArea = new Rect(0, TOOLBAR_HEIGHT, base.position.width, base.position.height - TOOLBAR_HEIGHT);

        updateToolbar(toolbarArea);

        cutsceneWrapper = DirectorHelper.UpdateWrapper(cutscene, cutsceneWrapper);
        directorControl.OnGUI(controlArea, cutsceneWrapper);
        DirectorHelper.ReflectChanges(cutscene, cutsceneWrapper);
    }

    /// <summary>
    /// Draw and update the toolbar for the director control
    /// </summary>
    /// <param name="toolbarArea">The area for the toolbar</param>
    private void updateToolbar(Rect toolbarArea)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // If there are no cutscenes, then only give option to create a new cutscene.
        if (GUILayout.Button(CREATE, EditorStyles.toolbarDropDown, GUILayout.Width(60)))
        {
            GenericMenu createMenu = new GenericMenu();
            createMenu.AddItem(new_cutscene, false, openCutsceneCreatorWindow);

            if (cutscene != null)
            {
                createMenu.AddSeparator(string.Empty);

                foreach (Type type in DirectorHelper.GetAllSubTypes(typeof(TrackGroup)))
                {
                    TrackGroupContextData userData = getContextDataFromType(type);
                    string text = string.Format(userData.Label);
                    createMenu.AddItem(new GUIContent(text), false, new GenericMenu.MenuFunction2(AddTrackGroup), userData);
                }

            }

            createMenu.DropDown(new Rect(5, TOOLBAR_HEIGHT, 0, 0));
        }

        // Cutscene selector
        cachedCutscenes = GameObject.FindObjectsOfType<Cutscene>();
        if (cachedCutscenes != null && cachedCutscenes.Length > 0)
        {
            // Get cutscene names
            GUIContent[] cutsceneNames = new GUIContent[cachedCutscenes.Length];
            for (int i = 0; i < cachedCutscenes.Length; i++)
            {
                cutsceneNames[i] = new GUIContent(cachedCutscenes[i].name);
            }

            // Sort alphabetically
            Array.Sort(cutsceneNames, delegate(GUIContent content1, GUIContent content2)
            {
                return string.Compare(content1.text, content2.text);
            });

            Array.Sort(cachedCutscenes, delegate(Cutscene c1, Cutscene c2)
            {
                return string.Compare(c1.name, c2.name);
            });

            // Find the currently selected cutscene.
            for (int i = 0; i < cachedCutscenes.Length; i++)
            {
                if (cutscene != null && cutscene.GetInstanceID() == cachedCutscenes[i].GetInstanceID())
                {
                    popupSelection = i;
                }
            }

            // Show the popup
            int tempPopup = EditorGUILayout.Popup(popupSelection, cutsceneNames, EditorStyles.toolbarPopup);
            if (tempPopup != popupSelection)
            {
                popupSelection = tempPopup;
                EditorPrefs.SetInt("DirectorControl.CutsceneID", cachedCutscenes[popupSelection].GetInstanceID());
            }
            popupSelection = Math.Min(popupSelection, cachedCutscenes.Length - 1);
            cutscene = cachedCutscenes[popupSelection];

        }
        if (cutscene != null)
        {
            if (GUILayout.Button(pickerImage, EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                Selection.activeObject = cutscene;
            }
            if (GUILayout.Button(refreshImage, EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                cutscene.Refresh();
            }

            if (Event.current.control && Event.current.keyCode == KeyCode.Space)
            {
                cutscene.Refresh();
                Event.current.Use();
            }
        }
        GUILayout.FlexibleSpace();

        bool tempSnapping = GUILayout.Toggle(isSnappingEnabled, snapImage, EditorStyles.toolbarButton, GUILayout.Width(24));
        if (tempSnapping != isSnappingEnabled)
        {
            isSnappingEnabled = tempSnapping;
            directorControl.IsSnappingEnabled = isSnappingEnabled;
        }
        GUILayout.Button(rippleEditImage, EditorStyles.toolbarButton, GUILayout.Width(24));
        GUILayout.Button(rollingEditImage, EditorStyles.toolbarButton, GUILayout.Width(24));
        GUILayout.Space(10f);

        if (GUILayout.Button(rescaleImage, EditorStyles.toolbarButton, GUILayout.Width(24)))
        {
            directorControl.Rescale();
        }
        if (GUILayout.Button(new GUIContent(zoomInImage, "Zoom In"), EditorStyles.toolbarButton, GUILayout.Width(24)))
        {
            directorControl.ZoomIn();
        }
        if (GUILayout.Button(zoomOutImage, EditorStyles.toolbarButton, GUILayout.Width(24)))
        {
            directorControl.ZoomOut();
        }
        GUILayout.Space(10f);

        Color temp = GUI.color;
        GUI.color = directorControl.InPreviewMode ? Color.red : temp;
        directorControl.InPreviewMode = GUILayout.Toggle(directorControl.InPreviewMode, PREVIEW_MODE, EditorStyles.toolbarButton, GUILayout.Width(150));
        GUI.color = temp;
        GUILayout.Space(10);

        if (GUILayout.Button("?", EditorStyles.toolbarButton))
        {
            EditorWindow.GetWindow<DirectorHelpWindow>();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Handle the playmode state changed event.
    /// </summary>
    public void PlaymodeStateChanged()
    {
        directorControl.InPreviewMode = false;
    }

    /// <summary>
    /// Save user preferences
    /// </summary>
    public void OnDisable()
    {
        directorControl.OnDisable();
        if (Application.isEditor && cutscene != null)
        {
            cutscene.ExitPreviewMode();
        }

        if (cutscene != null)
        {
            EditorPrefs.SetInt("DirectorControl.CutsceneID", cutscene.GetInstanceID());
        }
    }

    /// <summary>
    /// Load textures from Resources folder.
    /// </summary>
    private void loadTextures()
    {
        if (rescaleImage == null)
        {
            rescaleImage = Resources.Load<Texture>("Director_HorizontalRescaleIcon_Dark");
        }
        if (rescaleImage == null)
        {
            Debug.Log("Rescale icon missing from Resources folder.");
        }

        if (zoomInImage == null)
        {
            zoomInImage = Resources.Load<Texture>("Director_ZoomInIcon_Dark");
        }
        if (zoomInImage == null)
        {
            Debug.Log("Zoom in icon missing from Resources folder.");
        }

        if (zoomOutImage == null)
        {
            zoomOutImage = Resources.Load<Texture>("Director_ZoomOutIcon_Dark");
        }
        if (zoomOutImage == null)
        {
            Debug.Log("Zoom out icon missing from Resources folder.");
        }

        if (snapImage == null)
        {
            snapImage = Resources.Load<Texture>("Director_Magnet_Dark");
        }
        if (snapImage == null)
        {
            Debug.Log("Magnet icon missing from Resources folder.");
        }

        if (rollingEditImage == null)
        {
            rollingEditImage = Resources.Load<Texture>("Director_RollingIcon");
        }
        if (rollingEditImage == null)
        {
            Debug.Log("Rolling edit icon missing from Resources folder.");
        }

        if (rippleEditImage == null)
        {
            rippleEditImage = Resources.Load<Texture>("Director_RippleIcon");
        }
        if (rippleEditImage == null)
        {
            Debug.Log("Ripple edit icon missing from Resources folder.");
        }

        if (pickerImage == null)
        {
            pickerImage = Resources.Load<Texture>("Director_PickerIcon");
        }
        if (pickerImage == null)
        {
            Debug.Log("Picker icon is missing from Resources folder.");
        }

        if (refreshImage == null)
        {
            refreshImage = Resources.Load<Texture>("Director_RefreshIcon");
        }
        if (refreshImage == null)
        {
            Debug.Log("Refresh icon is missing from Resources folder.");
        }
    }

    /// <summary>
    /// Add a new track group to the current cutscene.
    /// </summary>
    /// <param name="userData">TrackGroupContextData containing track group label and type</param>
    private void AddTrackGroup(object userData)
    {
        TrackGroupContextData data = userData as TrackGroupContextData;
        if (data != null)
        {
            GameObject item = CutsceneItemFactory.CreateTrackGroup(cutscene, data.Type, data.Label).gameObject;
            Undo.RegisterCreatedObjectUndo(item, string.Format("Create {0}", item.name));
        }
    }

    private TrackGroupContextData getContextDataFromType(Type type)
    {
        string label = string.Empty;
        foreach (TrackGroupAttribute attribute in type.GetCustomAttributes(typeof(TrackGroupAttribute), true))
        {
            if (attribute != null)
            {
                label = attribute.Label;
                break;
            }
        }
        TrackGroupContextData userData = new TrackGroupContextData { Type = type, Label = label };
        return userData;
    }

    /// <summary>
    /// Show the TimelineWindow.
    /// </summary>
    [MenuItem(MENU_ITEM, false, 10)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DirectorWindow));
    }

    /// <summary>
    /// Opens the Cutscene creator window.
    /// </summary>
    internal void openCutsceneCreatorWindow()
    {
        EditorWindow.GetWindow<CutsceneCreatorWindow>();
    }

    private class TrackGroupContextData
    {
        public Type Type;
        public string Label;
    }
}

