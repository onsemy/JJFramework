using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public interface IInputManager
    {
        void BindButtonEvent(string bindName, DelegateOnButton newEvent);
        void BindAxisEvent(string bindName, DelegateOnAxis newEvent);
        void UnBindButtonEvent(string bindName);
    }

    public class InputManager : IInputManager
    {
        static InputManager _instance = null;
        InputReceiver _inputReceiver = null;

        public static IInputManager instance
        {
            get
            {
                return GetInstance();
            }
        }

        // Begin IInputManagerInterface Interface
        public void BindButtonEvent(string bindName, DelegateOnButton newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            InputBindParam newParam = new InputBindParam(newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void BindAxisEvent(string bindName, DelegateOnAxis newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            InputBindParam newParam = new InputBindParam(newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void UnBindButtonEvent(string bindName)
        {
            if (this._inputReceiver == null)
                return;
            this._inputReceiver.UnBindButtonEvent(bindName);
        }
        // ~~End IInputManagerInterface Interface

        private InputManager()
        {
            this._inputReceiver = new GameObject().AddComponent<InputReceiver>();
        }

        ~InputManager()
        {
            this._inputReceiver = null;
        }

        static InputManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new InputManager();
            }
            return _instance;
        }
    }
}