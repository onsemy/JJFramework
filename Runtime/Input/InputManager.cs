using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public interface IInputManagerInterface
    {
        void BindButtonEvent(in string inBindName, in object inTarget, in string inMethodName);
        void UnBindButtonEvent(in string inBindName);
    }

    public class InputManager : IInputManagerInterface
    {
        static public IInputManagerInterface instance
        {
            get
            {
                return GetInstance();
            }
        }

        // Begin IInputManagerInterface Interface
        public void BindButtonEvent(in string inBindName, in object inTarget, in string inMethodName)
        {
            if (m_inputReceiver == null || inTarget == null)
                return;

            InputBindParam newParam = new InputBindParam(inTarget, inMethodName);
            m_inputReceiver.BindButtonEvent(inBindName, newParam);
        }

        public void UnBindButtonEvent(in string inBindName)
        {
            if (m_inputReceiver == null)
                return;
            m_inputReceiver.UnBindButtonEvent(inBindName);
        }
        // ~~End IInputManagerInterface Interface

        private InputManager()
        {
            Debug.Log("Create Input Receiver");
            m_inputReceiver = new GameObject().AddComponent<InputReceiver>();
        }

        ~InputManager()
        {
            m_inputReceiver = null;
        }

        static InputManager GetInstance()
        {
            if(m_instance == null)
            {
                m_instance = new InputManager();
            }
            return m_instance;
        }

        static InputManager m_instance = null;
        InputReceiver m_inputReceiver = null;
    }
}