using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJFramework.Runtime.Extension;
using UniRx;

namespace JJFramework.Runtime
{
    public class ExMonoSingleton<T> : ExMonoBehaviour where T : ExMonoBehaviour
    {
        private static object _lock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
#if !UNITY_EDITOR
				if (_applicationQuit)
				{
					return null;
				}
#endif
                lock (_lock)
                {
                    if (ReferenceEquals(_instance, null))
                    {
                        _instance = (T) FindObjectOfType(typeof(T));

                        if (ReferenceEquals(_instance, null))
                        {
                            GameObject singletonObject = new GameObject($"(ExMonoSingleton) {typeof(T).Name}");
                            _instance = singletonObject.AddComponent<T>();
							
                            DontDestroyOnLoad(singletonObject);
							
                            Debug.Log($"[MonoSingleton] Created instance of {typeof(T).Name}");
                        }
                    }

                    return _instance;
                }
            }
        }

#if !UNITY_EDITOR
		private static bool _applicationQuit = false;

		private void OnApplicationQuit()
		{
			_applicationQuit = true;
		}

		private void OnDestroy()
		{
			_applicationQuit = true;
		}
#endif
    }
}
