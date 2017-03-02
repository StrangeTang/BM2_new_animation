using UnityEngine;
using System.Collections;
using UnityEditor;
using CinemaDirector;

[CutsceneItemControlAttribute(typeof(CinemaAudio))]
public class CinemaAudioControl : ActionFixedItemControl
{
    private string audioItemName = string.Empty;
    private Texture2D texture = null;

    public CinemaAudioControl()
    {
        base.AlterFixedAction += CinemaAudioControl_AlterFixedAction;
    }

    void CinemaAudioControl_AlterFixedAction(object sender, ActionFixedItemEventArgs e)
    {
        CinemaAudio audioItem = e.actionItem as CinemaAudio;
        if (audioItem == null) return;

        if (e.duration <= 0)
        {
            deleteItem(audioItem);
        }
        else
        {
            Undo.RecordObject(e.actionItem, string.Format("Change {0}", audioItem.name));
            audioItem.Firetime = e.firetime;
            audioItem.Duration = e.duration;
            audioItem.InTime = e.inTime;
            audioItem.OutTime = e.outTime;
        }
    }

    public override void Draw(DirectorControlState state)
    {
        CinemaAudio audioItem = Wrapper.Behaviour as CinemaAudio;
        if (audioItem == null) return;
        AudioSource audioSource = audioItem.GetComponent<AudioSource>();
        
        if (Selection.activeGameObject == audioItem.gameObject)
        {
            GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.AudioTrackItemSelectedStyle);
        }
        else
        {
            GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.AudioTrackItemStyle);
        }

        if (audioSource != null && audioSource.clip != null)
        {
            GUILayout.BeginArea(controlPosition);
            if (texture == null || audioItemName != audioSource.clip.name)
            {
                audioItemName = audioSource.clip.name;
                texture = AssetPreview.GetAssetPreview(audioSource.clip);
            }

            float inTimeOffset = (audioItem.InTime) * state.Scale.x;
            float outTimeOffset = (audioItem.ItemLength - audioItem.OutTime) * state.Scale.x;
            Rect texturePosition = new Rect(-inTimeOffset + 2, 0, controlPosition.width - 4 + inTimeOffset + outTimeOffset, controlPosition.height);

            if (texture != null)
            {
                GUI.DrawTexture(texturePosition, texture, ScaleMode.StretchToFill);
            }
            GUILayout.EndArea();
        }
    }
}
