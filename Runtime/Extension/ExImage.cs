using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace JJFramework.Runtime.Extension
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/ExImage")]
    public class ExImage : Image
    {
        public float alpha
        {
            get { return this.color.a; }
            set { this.color = new Color(color.r, color.g, color.b, value); } //.SetAlpha(value); }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/ExImage")]
        public static void CreateImageObject(UnityEditor.MenuCommand command)
        {
            var gameObject = new GameObject("ExImage", typeof(ExImage));
            UnityEditor.GameObjectUtility.SetParentAndAlign(gameObject, command.context as GameObject);
            UnityEditor.Undo.RegisterCreatedObjectUndo(gameObject, $"Create {gameObject.name}");
            UnityEditor.Selection.activeObject = gameObject;
        }
#endif
    }
}
