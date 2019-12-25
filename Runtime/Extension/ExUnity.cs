namespace JJFramework.Runtime.Extension
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Linq.Expressions;
    using System.Reflection;
    using JJFramework.Runtime.Attribute;

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

        public static T GetComponentEx<T>(this GameObject go) where T : Component
        {
            if (go == null)
            {
                UnityEngine.Debug.LogErrorFormat("[ERROR] go is null");
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
                UnityEngine.Debug.Log("target is NULL");
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
    }

    public static class ExAttributeUnity
    {
        public static void LoadComponents(this MonoBehaviour behaviour)
        {
            var behaviour_type = behaviour.GetType();
            var comp_type = typeof(Component);
            var path_type = typeof(ComponentPathAttribute);

            var members = behaviour_type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .Where(m =>
                                 (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                                 && m.GetMemberType().IsSubclassOf(comp_type)
                                 && m.GetCustomAttributes(path_type, true).Length == 1
                                );

            foreach (var item in members)
            {
                var attribute = item.GetCustomAttributes(path_type, true)[0] as ComponentPathAttribute;
                var path = (attribute.IsSelf) ? item.Name : attribute.path;
                var member_type = item.GetMemberType();

                var child = behaviour.transform.Find(path);

                if (child == null)
                {
                    if (attribute.IsSelf)
                    {
                        // NOTE(JJO): 모든 자식들로부터 찾아본다. 
                        var childList = behaviour.transform.GetComponentsInChildren<Transform>();
                        foreach (var t in childList)
                        {
                            if (t.name.Equals(path))
                            {
                                child = t;
                                break;
                            }
                        }

                        if (child == null)
                        {
                            UnityEngine.Debug.LogError($"can't find child in {path}");
                            continue;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"can't find child in {path}");
                        continue;
                    }
                }

                var member_comp = child.GetComponent(member_type);

                if (member_comp == null)
                    member_comp = child.gameObject.AddComponent(member_type);

                if (member_comp == null)
                {
                    UnityEngine.Debug.LogError($"can't find componnet {path} {member_type}");
                    continue;
                }

                item.SetValue(behaviour, member_comp);
            }
        }
    }
    /*
    public static class ExDebug
    {
        static readonly ILogger log = UnityEngine.Debug.unityLogger;

        public static void SetEnable(bool enable)
        {
            log.logEnabled = enable;
        }

        /// <summary>I don't care where it was writing</summary>
        public static void Log(string message, [CallerMemberName] string callerName = "")
        {
            Log<object>(message, callerName);
        }

        public static void Log(this ILogger logger, string message, [CallerMemberName] string callerName = "")
        {
            logger.Log(LogType.Log, $"<b>[{callerName}]</b>", message);
        }

        public static void Log<T>(string message, [CallerMemberName] string callerName = "")
        {
            log.Log(LogType.Log, $"<b>[{typeof(T).FullName} | {callerName}]</b> {message}");
        }

        public static void Log(this UnityEngine.Object obj, string message, [CallerMemberName] string callerName = "")
        {
            //UnityEngine.Debug.Log($"<b>[{obj.GetType().FullName} | {callerName}]</b> {message}");
            log.Log(LogType.Log, $"<b>[{obj.GetType().FullName} | {callerName}]</b>", message, obj);
        }

        public static void LogError<T>(string message, [CallerMemberName] string callerName = "")
        {
            log.Log(LogType.Error, $"<b>[{typeof(T).FullName} | {callerName}]</b> {message}");
        }

        public static void LogWarning<T>(object message, [CallerMemberName] string callerName = "")
        {
            log.Log(LogType.Warning, $"<b>[{typeof(T).FullName} | {callerName}]</b> {message}");
        }
    }
    */
}
