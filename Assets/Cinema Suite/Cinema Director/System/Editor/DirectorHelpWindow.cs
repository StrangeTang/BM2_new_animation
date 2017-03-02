using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class DirectorHelpWindow : EditorWindow
{
    const string TITLE = "Director Help";
    const string MENU_ITEM = "Window/Cinema Suite/Cinema Director/Help";

    #region Language

    const string YOUTUBE_LINK = "http://youtu.be/nyxCGrmHEG8?list=PLkTFhf2jQXOkn0Un8ej8THMVG8ccZH2y8";
    const string UNITY_FORUM_LINK = "http://forum.unity3d.com/threads/cinema-director-released.258242/";
    const string CS_FORUM_LINK = "www.cinema-suite.com/forum";
    const string DOCUMENTATION_LINK = "http://www.cinema-suite.com/Documentation/CinemaDirector/CinemaDirectorDocumentation.pdf";
    const string ASSET_STORE_LINK = "http://u3d.as/8vm";
    #endregion

    private Texture2D cinemaSuiteLogo;
    bool isDirectorInstalled = false;
    string directorProductVersion = string.Empty;

    public void Awake()
    {
        base.title = TITLE;
        this.minSize = new Vector2(450, 512f);
        
        // Load resources
        cinemaSuiteLogo = Resources.Load("Cinema Suite Monochrome") as Texture2D;
        if (cinemaSuiteLogo == null)
        {
            UnityEngine.Debug.LogWarning("Cinema Suite.png missing from the Cinema Suite/About/Resources folder.");
        }

        isDirectorInstalled = System.IO.File.Exists("Assets/Cinema Suite/Cinema Director/System/Editor/DirectorControl/DirectorEditor.dll");
        if (isDirectorInstalled)
        {
            directorProductVersion = FileVersionInfo.GetVersionInfo("Assets/Cinema Suite/Cinema Director/System/Editor/DirectorControl/DirectorEditor.dll").ProductVersion;
        }
    }

    protected void OnGUI()
    {
        GUILayout.BeginVertical();

        GUIStyle bigFont = new GUIStyle();
        bigFont.fontSize = 20;
        bigFont.wordWrap = true;
        bigFont.normal.textColor = GUI.contentColor;
        bigFont.alignment = TextAnchor.UpperCenter;

        // Display Logo
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        GUILayout.Label(cinemaSuiteLogo, bigFont, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        // Display links and emails
        
        GUILayout.Space(15f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Thank you for purchasing Cinema Director!", bigFont);
        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();
        bigFont.fontSize = 10;
        GUILayout.Label("This is version " + directorProductVersion, bigFont);
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        bigFont.fontSize = 16;
        bigFont.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(5f);
        if (GUILayout.Button("YouTube", GUILayout.Height(64), GUILayout.Width(160)))
            Application.OpenURL(YOUTUBE_LINK);
        GUILayout.Label("Visit our YouTube tutorial series for a quick overview.", bigFont);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(5f);
        if (GUILayout.Button("Unity3D Forums", GUILayout.Height(64), GUILayout.Width(160)))
            Application.OpenURL(UNITY_FORUM_LINK);
        GUILayout.Label("Participate in the further development of Director in the official Unity forums.", bigFont);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(5f);
        if (GUILayout.Button("Cinema Suite Forums", GUILayout.Height(64), GUILayout.Width(160)))
            Application.OpenURL(CS_FORUM_LINK);
        GUILayout.Label("Chat with the Cinema Suite team and other users.", bigFont);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(5f);
        if (GUILayout.Button("Documentation", GUILayout.Height(64), GUILayout.Width(160)))
            Application.OpenURL(DOCUMENTATION_LINK);
        GUILayout.Label("Find out how everything works in our Documentation.", bigFont);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.Space(5f);
        if (GUILayout.Button("Asset Store", GUILayout.Height(64), GUILayout.Width(160)))
            Application.OpenURL(ASSET_STORE_LINK);
        GUILayout.Label("If you have a moment, please leave us a review on the Asset Store.", bigFont);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        //GUILayout.Toggle(false, "Don't show this to me anymore");
        GUILayout.EndVertical();
    }

    [MenuItem(MENU_ITEM)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DirectorHelpWindow));
    }
}

