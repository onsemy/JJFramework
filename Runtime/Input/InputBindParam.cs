namespace JJFramework.Runtime.Input
{
    public delegate void DelegateOnButton(bool inButtonDown);
    public delegate void DelegateOnAxis(float inValue);

    public class InputBindParam
    {
        DelegateOnButton _buttonEvent;
        DelegateOnAxis _axisEvent;

        public DelegateOnButton buttonEvent { get { return _buttonEvent; } }
        public DelegateOnAxis axisEvent { get { return _axisEvent; } }
        public bool isButtonEvent { get { return this._axisEvent == null; } }
        public bool isAxisEvent { get { return this._buttonEvent == null; } }
        public bool isValidButtonEvent { get { return this._buttonEvent != null; } }
        public bool isValidAxisEvent { get { return this._axisEvent != null; } }

        public InputBindParam(DelegateOnButton newEvent)
        {
            this._buttonEvent = newEvent;
            this._axisEvent = null;
        }
        public InputBindParam(DelegateOnAxis newEvent)
        {
            this._axisEvent = newEvent;
            this._buttonEvent = null;
        }
    }
}