using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public class InputReceiver : MonoBehaviour
    {
        public void BindButtonEvent(in string inBindName, in InputBindParam inBindParam)
        {
            if(BindInfoGroup.ContainsKey(inBindName) == true)
            {
                Debug.LogWarningFormat("Already Binding Info Bind Name : {0}", inBindName);
                return;
            }
            BindInfoGroup.Add(inBindName, inBindParam);
        }

        public void UnBindButtonEvent(in string inBindName)
        {
            if (BindInfoGroup.ContainsKey(inBindName) == true)
            {
                Debug.LogWarningFormat("Not Binding Info Bind Name : {0}", inBindName);
                return;
            }
            BindInfoGroup.Remove(inBindName);
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            name = "InputReceiver";
            BindInfoGroup = new Dictionary<string, InputBindParam>();
        }

        void OnDestroy()
        {
            BindInfoGroup.Clear();
        }

        void Update()
        {
            foreach(KeyValuePair<string, InputBindParam> element in BindInfoGroup)
            {
                string currentMethodKey = element.Value.methodName;
                string localBindName = element.Key;
                object localTarget = element.Value.target;
                if (localTarget == null)
                    continue;
                System.Reflection.MethodInfo localMethodInfo = localTarget.GetType().GetMethod(currentMethodKey);
                if (localMethodInfo == null)
                    continue;
                System.Reflection.ParameterInfo[] localParameterInfo = localMethodInfo.GetParameters();
                if (localParameterInfo.Length <= 0)
                    continue;
                if (localParameterInfo[0].ParameterType == typeof(float))
                {
                    float localValue = UnityEngine.Input.GetAxis(localBindName);
                    object[] localParams = new object[1];
                    localParams[0] = localValue;
                    localMethodInfo.Invoke(localTarget, localParams);
                }
                else if (localParameterInfo[0].ParameterType == typeof(bool))
                {
                    
                    System.Action<bool> localButtonAction =
                        (bool inButtonDown) =>
                        {
                            object[] localParams = new object[1];
                            localParams[0] = inButtonDown;
                            localMethodInfo.Invoke(localTarget, localParams);
                        };

                    if (UnityEngine.Input.GetButtonDown(localBindName) == true)
                    {
                        localButtonAction(true);
                    }
                    else if (UnityEngine.Input.GetButtonUp(localBindName) == true)
                    {
                        localButtonAction(false);
                    }
                }
            }
        }

        private Dictionary<string, InputBindParam> BindInfoGroup;
    }
}