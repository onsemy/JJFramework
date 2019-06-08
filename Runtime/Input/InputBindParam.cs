namespace JJFramework.Runtime.Input
{
    public delegate void DelegateOnButton(in bool inButtonDown);
    public delegate void DelegateOnAixs(in float inValue);

    public class InputBindParam
    {
        DelegateOnButton _buttonEvent;
        DelegateOnAixs _aixsEvent;

        public DelegateOnButton buttonEvent { get { return _buttonEvent; } }
        public DelegateOnAixs aixsEvent { get { return _aixsEvent; } }
        public bool isButtonEvent { get { return this._aixsEvent == null; } }
        public bool isAixsEvent { get { return this._buttonEvent == null; } }
        public bool isValidButtonEvent { get { return this._buttonEvent != null; } }
        public bool isValidAixsEvent { get { return this._aixsEvent != null; } }

        public InputBindParam(in DelegateOnButton newEvent)
        {
            this._buttonEvent = newEvent;
            this._aixsEvent = null;
        }
        public InputBindParam(in DelegateOnAixs newEvent)
        {
            this._aixsEvent = newEvent;
            this._buttonEvent = null;
        }
    }
}