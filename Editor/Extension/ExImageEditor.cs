using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJFramework.Runtime.Extension;
using UniRx;
using UnityEditor;

namespace JJFramework.Editor.Extension
{
    [CustomEditor(typeof(ExImage))]
    [CanEditMultipleObjects]
    public class ExImageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var component = (ExImage) target;
            component.alpha = EditorGUILayout.Slider("Alpha", component.alpha, 0f, 1f);
            if (GUILayout.Button("Set Native Size"))
            {
                component.SetNativeSize();
            }
        }
    }
}
