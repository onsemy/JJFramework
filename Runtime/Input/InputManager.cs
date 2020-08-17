using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JJFramework.Runtime.Input
{
    public interface IInputManager
    {
        void BindButtonEvent(string bindName, DelegateOnButton newEvent);
        void BindAxisEvent(string bindName, DelegateOnAxis newEvent);
        void BindAndroidBackEvent(string bindName, UnityAction newEvent);
        void BindMouseMoveEvent(in string bindName, in DelegateMousePosition newEvent);
        void UnBindButtonEvent(string bindName);
    }

    public class InputManager : IInputManager
    {
        static readonly string AndroidBackBindName = "AndroidBackButton";

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

            IInputBindParam newParam = new InputBindParamButton(bindName, newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void BindAxisEvent(string bindName, DelegateOnAxis newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            IInputBindParam newParam = new InputBindParamAxis(bindName, newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);        
        }

        public void BindAndroidBackEvent(string bindName, UnityAction newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            IInputBindParam newParam = new InputBindParamAndroidBack(newEvent);
            this._inputReceiver.BindButtonEvent(bindName, newParam);
        }

        public void BindMouseMoveEvent(in string bindName, in DelegateMousePosition newEvent)
        {
            if (this._inputReceiver == null || newEvent == null)
                return;

            IInputBindParam newParam = new InputBindParamMousePosition(newEvent);
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
