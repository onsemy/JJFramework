using UnityEngine;

namespace JJFramework.Runtime.Attribute
{
    sealed class ComponentPathAttribute : System.Attribute
    {
        public readonly string path;
        public bool IsSelf
        {
            get;
            private set;
        }
        public bool IsCustomComponentSetting
        {
            get;
            private set;
        }

        public ComponentPathAttribute(string path, bool is_custom_setting = true)
        {
            this.path = path;
            this.IsSelf = false;
            this.IsCustomComponentSetting = is_custom_setting;
        }

        public ComponentPathAttribute(bool is_custom_setting = true)
        {
            this.path = null;
            this.IsSelf = true;
            this.IsCustomComponentSetting = is_custom_setting;
        }

        // TODO(jjo): 상위 컴포넌트에 대해 되게끔
        //public ComponentPathAttribute(Component component, bool is_custom_setting = true)
        //{
        //    this.path = null;
        //    this.IsSelf = false;
        //    this.IsCustomComponentSetting = is_custom_setting;
        //}
    }
}
