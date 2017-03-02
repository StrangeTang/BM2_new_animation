using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CinemaDirector;

[CutsceneItemControlAttribute(typeof(CinemaMultiActorCurveClip))]
public class CinemaMultiActorCurveControl : CinemaCurveControl
{
    public override void UpdateCurveWrappers(CinemaClipCurveWrapper clipWrapper)
    {
        CinemaMultiActorCurveClip clipCurve = clipWrapper.Behaviour as CinemaMultiActorCurveClip;
        if (clipCurve == null) return;

        for (int i = 0; i < clipCurve.CurveData.Count; i++)
        {
            MemberClipCurveData member = clipCurve.CurveData[i];

            CinemaMemberCurveWrapper memberWrapper = null;
            if (!clipWrapper.TryGetValue(member.PropertyType.ToString(), member.PropertyName, out memberWrapper))
            {
                memberWrapper = new CinemaMemberCurveWrapper();
                memberWrapper.Type = member.PropertyType.ToString();
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
                    memberWrapper.AnimationCurves[j].Color = UnityPropertyTypeInfo.GetCurveColor(j);
                    memberWrapper.AnimationCurves[j].Label = UnityPropertyTypeInfo.GetCurveName(member.PropertyType, j);
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
                if (member.PropertyType.ToString() == cw.Type && member.PropertyName == cw.PropertyName)
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
