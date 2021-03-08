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

        /// <summary>
        /// 직접 경로를 지정하여 할당한다.
        /// </summary>
        /// <param name="path">List<>타입을 설정한 경우 경로가 아닌 이름입니다.</param>
        /// <param name="isLoadFromEditor"></param>
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
