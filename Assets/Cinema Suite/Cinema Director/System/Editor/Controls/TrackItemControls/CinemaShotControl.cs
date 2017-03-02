using UnityEngine;
using System.Collections;
using UnityEditor;
using CinemaDirector;

[CutsceneItemControlAttribute(typeof(CinemaShot))]
public class CinemaShotControl : CinemaActionControl
{
    #region Language
    private const string MODIFY_CAMERA = "Set Camera/{0}";
    #endregion

    public override void Draw(DirectorControlState state)
    {
        CinemaShot shot = Wrapper.Behaviour as CinemaShot;
        if (shot == null) return;

        Color temp = GUI.color;

        GUI.color = (shot.shotCamera != null) ? GUI.color : Color.red;
        if (Selection.activeGameObject == shot.gameObject)
        {
            GUI.Box(controlPosition, new GUIContent(shot.name), TimelineTrackControl.styles.ShotTrackItemSelectedStyle);
        }
        else
        {
            GUI.Box(controlPosition, new GUIContent(shot.name), TimelineTrackControl.styles.ShotTrackItemStyle);
        }
        GUI.color = temp;
    }

    protected override void showContextMenu(Behaviour behaviour)
    {
        CinemaShot shot = behaviour as CinemaShot;
        if (shot == null) return;

        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
        
        GenericMenu createMenu = new GenericMenu();
        createMenu.AddItem(new GUIContent("Focus"), false, focusShot, shot);
        foreach (Camera c in cameras)
        {
            
            ContextSetCamera arg = new ContextSetCamera();
            arg.shot = shot;
            arg.camera = c;
            createMenu.AddItem(new GUIContent(string.Format(MODIFY_CAMERA, c.gameObject.name)), false, setCamera, arg);
        }
        createMenu.AddSeparator(string.Empty);
        createMenu.AddItem(new GUIContent("Copy"), false, copyItem, behaviour);
        createMenu.AddSeparator(string.Empty);
        createMenu.AddItem(new GUIContent("Clear"), false, deleteItem, shot);
        createMenu.ShowAsContext();
    }

    private void focusShot(object userData)
    {
        CinemaShot shot = userData as CinemaShot;
        if (shot.shotCamera != null)
        {
            SceneView.currentDrawingSceneView.AlignViewToObject(shot.shotCamera.transform);
        }
    }

    private void setCamera(object userData)
    {
        ContextSetCamera arg = userData as ContextSetCamera;
        if (arg != null)
        {
            Undo.RecordObject(arg.shot, "Set Camera");
            arg.shot.shotCamera = arg.camera;
        }
    }

    private class ContextSetCamera
    {
        public Camera camera;
        public CinemaShot shot;
    }
}
