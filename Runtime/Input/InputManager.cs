using System.Collections.Generic;
using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public interface IInputManagerInterface
    {
        void SetController(in IControllerInterface inController);
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
        public void SetController(in IControllerInterface inController)
        {
            if (m_inputReceiver == null)
                return;

            m_inputReceiver.SetController(inController);
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