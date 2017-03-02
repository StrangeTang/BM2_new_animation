using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CutsceneItemControlAttribute(typeof(CinemaClipCurve))]
public class CinemaCurveControl : CinemaCurveClipItemControl
{
    protected bool hasUndoRedoBeenPerformed = false;

    public CinemaCurveControl()
    {
        base.AlterDuration += CinemaCurveControl_AlterDuration;
        base.AlterFiretime += CinemaCurveControl_AlterFiretime;
        base.TranslateCurveClipItem += CinemaCurveControl_TranslateCurveClipItem;
        base.SnapScrubber += CinemaCurveControl_SnapScrubber;
    }

    void CinemaCurveControl_SnapScrubber(object sender, CurveClipScrubberEventArgs e)
    {
        CinemaClipCurve curveClip = e.curveClipItem as CinemaClipCurve;
        if (curveClip == null) return;

        curveClip.Cutscene.SetRunningTime(e.time);
    }

    void CinemaCurveControl_TranslateCurveClipItem(object sender, CurveClipItemEventArgs e)
    {
        CinemaClipCurve curveClip = e.curveClipItem as CinemaClipCurve;
        if (curveClip == null) return;

        Undo.RecordObject(e.curveClipItem, string.Format("Changed {0}", curveClip.name));
        curveClip.TranslateCurves(e.firetime - curveClip.Firetime);
        EditorUtility.SetDirty(e.curveClipItem);
    }

    void CinemaCurveControl_AlterFiretime(object sender, CurveClipItemEventArgs e)
    {
        CinemaClipCurve curveClip = e.curveClipItem as CinemaClipCurve;
        if (curveClip == null) return;

        Undo.RecordObject(e.curveClipItem, string.Format("Changed {0}", curveClip.name));
        curveClip.AlterFiretime(e.firetime, e.duration);

        if (e.duration <= 0)
        {
            GUIUtility.hotControl = 0;
            deleteItem(e.curveClipItem);
        }
        EditorUtility.SetDirty(e.curveClipItem);
    }

    void CinemaCurveControl_AlterDuration(object sender, CurveClipItemEventArgs e)
    {
        CinemaClipCurve curveClip = e.curveClipItem as CinemaClipCurve;
        if (curveClip == null) return;

        Undo.RecordObject(e.curveClipItem, string.Format("Changed {0}", curveClip.name));
        curveClip.AlterDuration(e.duration);

        if (e.duration <= 0)
        {
            GUIUtility.hotControl = 0;
            deleteItem(e.curveClipItem);
        }
        EditorUtility.SetDirty(e.curveClipItem);
    }

    public override void UpdateCurveWrappers(CinemaClipCurveWrapper clipWrapper)
    {
        CinemaClipCurve clipCurve = clipWrapper.Behaviour as CinemaClipCurve;
        if (clipCurve == null) return;
        
        for (int i = 0; i < clipCurve.CurveData.Count; i++)
        {
            MemberClipCurveData member = clipCurve.CurveData[i];

            CinemaMemberCurveWrapper memberWrapper = null;
            if (!clipWrapper.TryGetValue(member.Type, member.PropertyName, out memberWrapper))
            {
                memberWrapper = new CinemaMemberCurveWrapper();
                memberWrapper.Type = member.Type;
                memberWrapper.PropertyName = member.PropertyName;
                memberWrapper.Texture = EditorGUIUtility.ObjectContent(null, UnityPropertyTypeInfo.GetUnityType(member.Type)).image;
                ArrayUtility.Add<CinemaMemberCurveWrapper>(ref clipWrapper.MemberCurves, memberWrapper);

                int showingCurves = UnityPropertyTypeInfo.GetCurveCount(member.PropertyType);
                memberWrapper.AnimationCurves = new CinemaAnimationCurveWrapper[showingCurves];

                for (int j = 0; j < showingCurves; j++)
                {
                    memberWrapper.AnimationCurves[j] = new CinemaAnimationCurveWrapper();

                    memberWrapper.AnimationCurves[j].Id = j;
                    memberWrapper.AnimationCurves[j].Curve = member.GetCurve(j);
                    memberWrapper.AnimationCurves[j].Label = UnityPropertyTypeInfo.GetCurveName(member.PropertyType, j);
                    memberWrapper.AnimationCurves[j].Color = UnityPropertyTypeInfo.GetCurveColor(member.Type, member.PropertyName, memberWrapper.AnimationCurves[j].Label, j);
                }
            }
        }

        // Remove missing track items
        List<CinemaMemberCurveWrapper> itemRemovals = new List<CinemaMemberCurveWrapper>();
        foreach (CinemaMemberCurveWrapper cw in clipWrapper.MemberCurves)
        {
            bool found = false;
            foreach (MemberClipCurveData member in clipCurve.CurveData)
            {
                if (member.Type == cw.Type && member.PropertyName == cw.PropertyName)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                itemRemovals.Add(cw);
            }
        }
        foreach (CinemaMemberCurveWrapper item in itemRemovals)
        {
            ArrayUtility.Remove<CinemaMemberCurveWrapper>(ref clipWrapper.MemberCurves, item);
        }
    }
}
