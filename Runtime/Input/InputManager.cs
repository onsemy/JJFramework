using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public interface IInputManagerInterface
    {
        void BindButtonEvent(in string bindName, in DelegateOnButton newEvent);
        void BindAxisEvent(in string bindName, in DelegateOnAxis newEvent);
        void UnBindButtonEvent(in string bindName);
    }

    public class InputManager : IInputManagerInterface
    {
        static InputManager _instance = null;
        InputReceiver _inputReceiver = null;

        static public IInputManagerInterface instance
        {
            get
            {
                return GetInstance();
            }
        }

        // Begin IInputManagerInterface Interface
        public void BindButtonEvent(in string bindName, in DelegateOnButton newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            InputBindParam newParam = new InputBindParam(newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void BindAxisEvent(in string bindName, in DelegateOnAxis newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            InputBindParam newParam = new InputBindParam(newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void UnBindButtonEvent(in string bindName)
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
            if(_instance == null)
            {
                _instance = new InputManager();
            }
            return _instance;
        }
    }
}