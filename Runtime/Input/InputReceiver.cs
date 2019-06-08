using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public class InputReceiver : MonoBehaviour
    {
        private Dictionary<string, InputBindParam> _bindInfoGroup;

        public void BindButtonEvent(string bindName, InputBindParam bindParam)
        {
            if(this._bindInfoGroup.ContainsKey(bindName) == true)
            {
                Debug.LogWarningFormat("Already Binding Info Bind Name : {0}", bindName);
                return;
            }
            this._bindInfoGroup.Add(bindName, bindParam);
        }

        public void UnBindButtonEvent(string bindName)
        {
            if (this._bindInfoGroup.ContainsKey(bindName) == true)
            {
                Debug.LogWarningFormat("Not Binding Info Bind Name : {0}", bindName);
                return;
            }
            this._bindInfoGroup.Remove(bindName);
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            name = "InputReceiver";
            this._bindInfoGroup = new Dictionary<string, InputBindParam>();
        }

        void OnDestroy()
        {
            this._bindInfoGroup.Clear();
        }

        void Update()
        {
            foreach(KeyValuePair<string, InputBindParam> element in this._bindInfoGroup)
            {
                string bindName = element.Key;
                InputBindParam currentParam = element.Value;
                if (currentParam.isAxisEvent == true && currentParam.isValidAxisEvent == true)
                {
                    float localValue = UnityEngine.Input.GetAxis(bindName);
                    currentParam.axisEvent(localValue);
                }
                else if (currentParam.isButtonEvent == true && currentParam.isValidButtonEvent == true)
                {                    
                    if (UnityEngine.Input.GetButtonDown(bindName) == true)
                    {
                        currentParam.buttonEvent(true);
                    }
                    else if (UnityEngine.Input.GetButtonUp(bindName) == true)
                    {
                        currentParam.buttonEvent(false);
                    }
                }
            }
        }
    }
}