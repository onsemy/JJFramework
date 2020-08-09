using UnityEngine.Events;

namespace JJFramework.Runtime.Input
{
    public delegate void DelegateOnButton(bool inButtonDown);
    public delegate void DelegateOnAxis(float inValue);

    public interface IInputBindParam
    {
        void Execute();
    }

    public class InputBindParamAxis : IInputBindParam
    {
        string bindName;
        DelegateOnAxis axisEvent { get; set; }

        public InputBindParamAxis(string inBindName, DelegateOnAxis newEvent)
        {
            bindName = inBindName;
            axisEvent = newEvent;
        }

        public void Execute()
        {
            float localValue = UnityEngine.Input.GetAxis(bindName);
            axisEvent?.Invoke(localValue);
        }
    }

    public class InputBindParamButton : IInputBindParam
    {
        private string bindName { get; set; }
        private DelegateOnButton buttonEvent { get; set; }
        
        public InputBindParamButton(string inBindName, DelegateOnButton newEvent)
        {
            bindName = inBindName;
            buttonEvent = newEvent;
        }

        public void Execute()
        {
            if (UnityEngine.Input.GetButtonDown(bindName) == true)
            {
                buttonEvent?.Invoke(true);
            }
            else if (UnityEngine.Input.GetButtonUp(bindName) == true)
            {
                buttonEvent?.Invoke(false);
            }
        }
    }

    public class InputBindParamAndroidBack : IInputBindParam
    {
        UnityAction buttonEvent { get; set; }

        public InputBindParamAndroidBack(UnityAction newEvent)
        {
            buttonEvent = newEvent;
        }

        public void Execute()
        {
            if(UnityEngine.Input.GetKey(UnityEngine.KeyCode.Escape))
                buttonEvent?.Invoke();
        }
    }
}
