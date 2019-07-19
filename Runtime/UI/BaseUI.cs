using UnityEngine;
using JJFramework.Runtime.Extension;

namespace JJFramework.Runtime.UI
{
    [DisallowMultipleComponent]
    public abstract class BaseUI : ExMonoBehaviour
    {
        public int id
        {
            get;
            private set;
        }

        //[ComponentPath] protected Button btnClose;

        public bool isCalledCloseAction { get; protected set; }
        private System.Action<BaseUI> _preCloseAction;

        public virtual void CloseAction()
        {
            isCalledCloseAction = true;
            _preCloseAction?.Invoke(this);

            this.Hide();

            GameObject.Destroy(gameObject);
        }

        public void RegistPreCloseAction(System.Action<BaseUI> action)
        {
            _preCloseAction = action;
        }

        protected override void Awake()
        {
            base.Awake();

            id = this.GetHashCode(); // TODO(jjo): GenerateUID Function
            
            //btnClose.OnClickAsObservable().Subscribe(_ => CloseAction());
        }

        public virtual BaseUI Show()
        {
            gameObject.SetActive(true);

            return this;
        }

        public virtual BaseUI Hide()
        {
            gameObject.SetActive(false);

            return this;
        }
    }
}
