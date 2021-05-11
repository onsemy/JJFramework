namespace JJFramework.Runtime.Extension
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Linq.Expressions;
    using System.Reflection;
    using JJFramework.Runtime.Attribute;
#if UNITY_2019_1_OR_NEWER
    using UnityEngine.UI.Extensions;
#endif
    
#if UNITY_EDITOR
    using UnityEditor;
    
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviourCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var component = target as MonoBehaviour;
            if (GUILayout.Button("Assign Components"))
            {
                component.LoadComponents(true);
            }
        }
    }
#endif

    public static class ExUnity
    {
        public static void SetPreferredSize(this Text t)
        {
            t.rectTransform.sizeDelta = new Vector2(t.preferredWidth, t.preferredHeight);
        }

        public static void SetActive(this Component c, bool isActive)
        {
            c.gameObject.SetActive(isActive);
        }

        public static void SetScale(this GameObject obj, Vector3 scale)
        {
            obj.transform.localScale = scale;
        }

        public static void SetPosition(this GameObject obj, Vector3 position)
        {
            obj.transform.position = position;
        }

        public static void SetPosition(this GameObject obj, float x, float y, float z)
        {
            obj.transform.position = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(this GameObject obj, Vector3 position)
        {
            obj.transform.localPosition = position;
        }

        public static void SetLocalPosition(this GameObject obj, float x, float y, float z)
        {
            obj.transform.localPosition = new Vector3(x, y, z);
        }

        public static void SetRotation(this GameObject obj, Quaternion rotation)
        {
            obj.transform.rotation = rotation;
        }

        public static void SetRotation(this GameObject obj, Vector3 eulerAngle)
        {
            obj.transform.rotation = Quaternion.Euler(eulerAngle);
        }

        public static void SetLocalRotation(this GameObject obj, Quaternion rotation)
        {
            obj.transform.localRotation = rotation;
        }

        public static void SetLocalRotation(this GameObject obj, Vector3 eulerAngle)
        {
            obj.transform.localRotation = Quaternion.Euler(eulerAngle);
        }

        public static string ToAssetPath(this string fpath)
        {
            // "C:/UnityProj/Assets/a.txt" => "Assets/a.txt";
            return ("Assets/" + fpath.ToUnixPath().Substring(Application.dataPath.ToUnixPath().Length + 1)).ToUnixPath();
        }

        public static Transform FindEx(this Transform tr, string name)
        {
            Transform child = null;
            // NOTE(JJO): 모든 자식들로부터 찾아본다. 
            var childList = tr.GetComponentsInChildren<Transform>(true);
            foreach (var t in childList)
            {
                if (t.name.Equals(name))
                {
                    child = t;
                    break;
                }
            }

            if (child == null)
            {
                Debug.LogError($"can't find child in {name}");
            }

            return child;
        }
        
        public static T GetComponentEx<T>(this GameObject go) where T : Component
        {
            if (go == null)
            {
                Debug.LogError("[ERROR] go is null");
                return null;
            }

            var t = go.GetComponent<T>() ?? go.AddComponent<T>();

            return t;
        }

        public static T GetComponentEx<T>(this Component component) where T : Component
        {
            return component.gameObject.GetComponentEx<T>();
        }

        public static T FindComponent<T>(this MonoBehaviour c, Expression<Func<object>> expression) where T : Component
        {
            return FindComponent<T>(c, (expression.Body as MemberExpression)?.Member.Name);
        }

        public static T FindComponent<T>(this MonoBehaviour c, string name) where T : Component
        {
            return c.transform.FindComponent<T>(name);
        }

        public static T FindComponent<T>(this Component c, string name) where T : Component
        {
            var target = c.transform.Find(name);
            if (target == null)
            {
                Debug.Log("target is NULL");
                return null;
            }

            return target.GetComponentEx<T>();
        }

        public static Color SetAlpha(this Color c, float alpha)
        {
            c = new Color(c.r, c.g, c.b, alpha);
            return c;
        }

        public static void SetSiblingIndex(this Component c, int index)
        {
            c.transform.SetSiblingIndex(index);
        }

        public static int GetSiblingIndex(this Component c)
        {
            return c.transform.GetSiblingIndex();
        }

        public static void SetColor(this Image image, Color color)
        {
            image.color = color;
        }

        public static void SetColor(this Image image, float r = 1f, float g = 1f, float b = 1f, float a = 1f)
        {
            image.color = new Color(r, g, b, a);
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            var color = image.color;
            image.color = new Color(color.r, color.g, color.b, alpha);
        }

        public static void SetNotchPortrait(this RectTransform rectTransform)
        {
#if UNITY_2019_1_OR_NEWER
            var canvas = rectTransform.GetParentCanvas();
            var cam = canvas.worldCamera ?? Camera.main;

            var worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            for (int i = 0; i < worldCorners.Length; ++i)
            {
                worldCorners[i] = cam.WorldToScreenPoint(worldCorners[i]);
            }
            var thisRect = new Rect(worldCorners[0].x, worldCorners[0].y, worldCorners[2].x - worldCorners[1].x, worldCorners[2].y - worldCorners[3].y);

            var scaleFactor = canvas.scaleFactor;
            foreach (var rect in Screen.cutouts)
            {
                if (thisRect.Overlaps(rect))
                {
                    var prevPos = rectTransform.anchoredPosition;
                    prevPos.y -= rect.height / scaleFactor;
            
                    rectTransform.anchoredPosition = prevPos;
                }
            }
#endif
        }
    }

    public static class ExAttributeUnity
    {
        public static void LoadComponents(this MonoBehaviour behaviour, bool isLoadFromEditor = false)
        {
            var behaviourType = behaviour.GetType();
            var componentType = typeof(Component);
            var pathType = typeof(ComponentPathAttribute);

            var members = behaviourType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m =>
                    (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                    && m.GetCustomAttributes(pathType, true).Length == 1
                );

            var componentMembers = members.Where(m => m.GetMemberType().IsSubclassOf(componentType));
            foreach (var item in componentMembers)
            {
                var attribute = item.GetCustomAttributes(pathType, true)[0] as ComponentPathAttribute;
                if (null == attribute
                    || attribute.IsLoadFromEditor != isLoadFromEditor)
                {
                    continue;
                }

                var path = attribute.IsSelf ? item.Name : attribute.path;
                var memberType = item.GetMemberType();

                var child = behaviour.transform.Find(path);
                if (child == null)
                {
                    if (attribute.IsSelf)
                    {
                        // NOTE(JJO): 모든 자식들로부터 찾아본다. 
                        var childList = behaviour.transform.GetComponentsInChildren<Transform>(true);
                        foreach (var t in childList)
                        {
                            if (t.name.Equals(path))
                            {
                                child = t;
                                break;
                            }
                        }
                    }
                    
                    if (child == null)
                    {
                        UnityEngine.Debug.LogError($"can't find child in {path}");
                        continue;
                    }
                }

                var memberComponent = child.GetComponent(memberType);

                if (memberComponent == null)
                    memberComponent = child.gameObject.AddComponent(memberType);

                if (memberComponent == null)
                {
                    UnityEngine.Debug.LogError($"can't find component {path} {memberType}");
                    continue;
                }

                item.SetValue(behaviour, memberComponent);
            }
            
            // NOTE(JJO): List<>를 ComponentPath로 지정한 경우의 처리
            var genericMembers = members.Where(m => m.GetMemberType().IsGenericType);
            foreach (var item in genericMembers)
            {
                var attribute = item.GetCustomAttributes(pathType, true)[0] as ComponentPathAttribute;
                if (null == attribute
                    || attribute.IsLoadFromEditor != isLoadFromEditor)
                {
                    continue;
                }

                var genericType = item.GetMemberType().GenericTypeArguments[0];
                if (!genericType.IsSubclassOf(componentType))
                {
                    continue;
                }
                
                var objectName = attribute.IsSelf ? item.Name : attribute.path;
 
                var listType = typeof(List<>).MakeGenericType(genericType);
                var list = (IList)System.Activator.CreateInstance(listType);
                
                var childList = behaviour.transform.GetComponentsInChildren(genericType, true);
                foreach (var c in childList)
                {
                    if (c.name.Contains(objectName))
                    {
                        list.Add(c);
                    }
                }
                
                item.SetValue(behaviour, list);
            }
            
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            UnityEditor.EditorUtility.SetDirty(behaviour);
#endif
        }
    }
    
    // NOTE(JJO): kimsama님의 gist를 참고하여 작성됨.
    //            https://gist.github.com/kimsama/4123043
    public static class Debug
    {
        public static bool isDebugBuild
        {
            get { return UnityEngine.Debug.isDebugBuild; }
        }

        private static void ConvertLog(object message, string memberName, string filePath, int lineNumber, System.Action<object> logFunction)
        {
            var convertPath = filePath.Replace("/", "\\");
            var lastIndexOf = convertPath.LastIndexOf('\\') + 1;
            var length = filePath.Length - lastIndexOf - 3;
            logFunction.Invoke($"<b>[{filePath.Substring(lastIndexOf, length)}::{memberName}:L{lineNumber}]</b> {message}");
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void Log(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            ConvertLog(message, memberName, filePath, lineNumber, UnityEngine.Debug.Log);
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void LogWarning(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            ConvertLog(message, memberName, filePath, lineNumber, UnityEngine.Debug.LogWarning);
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void LogError(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            ConvertLog(message, memberName, filePath, lineNumber, UnityEngine.Debug.LogError);
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void DrawLine(UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color = default(UnityEngine.Color), float duration = 0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void DrawRay(UnityEngine.Vector3 start, UnityEngine.Vector3 dir, UnityEngine.Color color = default(UnityEngine.Color), float duration = 0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
        }
        
        [System.Diagnostics.Conditional("__DEBUG__")]
        public static void Assert(bool condition, object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var convertPath = filePath.Replace("/", "\\");
            var lastIndexOf = convertPath.LastIndexOf('\\') + 1;
            var length = filePath.Length - lastIndexOf - 3;
            UnityEngine.Debug.Assert(condition, $"<b>[{filePath.Substring(lastIndexOf, length)}::{memberName}:L{lineNumber}]</b> {message}");
        }
    }
}
