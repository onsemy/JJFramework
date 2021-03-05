using System.Collections.Generic;
using UnityEngine;
using Debug = JJFramework.Runtime.Extension.Debug;

namespace JJFramework.Runtime.Input
{
    public class InputReceiver : MonoBehaviour
    {
        private Dictionary<string, IInputBindParam> _bindInfoGroup;

        public void BindButtonEvent(string bindName, IInputBindParam bindParam)
        {
            if(this._bindInfoGroup.ContainsKey(bindName) == true)
            {
                Debug.LogWarning($"Already Binding Info Bind Name : {bindName}");
                return;
            }
            this._bindInfoGroup.Add(bindName, bindParam);
        }

        public void UnBindButtonEvent(string bindName)
        {
            if (this._bindInfoGroup.ContainsKey(bindName) == false)
            {
                Debug.LogWarning($"Not Binding Info Bind Name : {bindName}");
                return;
            }
            this._bindInfoGroup.Remove(bindName);
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            name = "InputReceiver";
            this._bindInfoGroup = new Dictionary<string, IInputBindParam>();
        }

        void OnDestroy()
        {
            this._bindInfoGroup.Clear();
        }

        void Update()
        {
            foreach(KeyValuePair<string, IInputBindParam> element in this._bindInfoGroup)
                element.Value?.Execute();
        }
    }
}
