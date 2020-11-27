using UnityEngine;

namespace JJFramework.Runtime.Attribute
{
    public sealed class ComponentPathAttribute : System.Attribute
    {
        public readonly string path;
        public bool IsSelf
        {
            get;
            private set;
        }

        public bool IsLoadFromEditor
        {
            get;
            private set;
        }

        public ComponentPathAttribute(string path, bool isLoadFromEditor = false)
        {
            this.path = path;
            this.IsSelf = false;
            this.IsLoadFromEditor = isLoadFromEditor;
        }

        public ComponentPathAttribute(bool isLoadFromEditor = false)
        {
            this.path = null;
            this.IsSelf = true;
            this.IsLoadFromEditor = isLoadFromEditor;
        }
    }
}
