using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using JJFramework.Runtime.Extension;
using JJFramework.Runtime.Resource;

namespace JJFramework.Runtime.UI
{
    [DisallowMultipleComponent]
    public class UIManager
    {
        private IResourceLoader __resourceLoader;

        public class UIPack
        {
            public BaseUI ui { get; private set; }
            public int id { get; private set; }

            public UIPack(int id, BaseUI ui)
            {
                this.id = id;
                this.ui = ui;
            }
        }

        private List<UIPack> __uiStackList = new List<UIPack>();

        //private Stack<BaseUI> __uiStackList = new Stack<BaseUI>();

        private Transform __uiRoot;
        private Canvas __canvas;

        public void Init(IResourceLoader loader, Vector2 screenSize, int sortingOrder = 100)
        {
            __resourceLoader = loader;

            GameObject obj = new GameObject(nameof(UIManager));

            __canvas = obj.AddComponent<Canvas>();
            __canvas.renderMode = RenderMode.ScreenSpaceCamera;
            __canvas.worldCamera = Camera.main;
            __canvas.sortingOrder = sortingOrder;
            // NOTE(JJO): Spine - SkeletonGraphic 덕분에 기본값이 100인 것을 1로 변경함.
            // 추후 SkeletonGraphic 버그(?)가 고쳐지면 변경될 수 있음.
            __canvas.referencePixelsPerUnit = 1f;

            var scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = screenSize;

            var raycaster = obj.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            raycaster.ignoreReversedGraphics = true;

            __uiRoot = obj.transform;
            GameObject.DontDestroyOnLoad(obj);
        }

        public T Generate<T>() where T : BaseUI
        {
            var origin = __resourceLoader.Load<GameObject>(typeof(T).Name);
            if (origin == null)
            {
                Debug.LogError($"{nameof(T)}을(를) 불러오지 못했습니다! - origin이 NULL입니다!");
                return null;
            }

            var uiObject = GameObject.Instantiate<GameObject>(origin, __uiRoot);
            if (uiObject == null)
            {
                Debug.LogError($"{nameof(T)}을(를) 불러오지 못했습니다! - Prefab 생성에 실패했습니다.");
                return null;
            }

            var ui = uiObject.AddComponent<T>();
            ui.RegistPreCloseAction(RemoveFromStack);

            //__uiStackList.Push(ui);
            __uiStackList.Add(new UIPack(ui.id, ui));

            if (__canvas.worldCamera == null)
            {
                __canvas.worldCamera = Camera.main;
            }

            return ui;
        }

        public BaseUI currentDialog => __uiStackList.Count > 0 ? __uiStackList.LastOrDefault()?.ui : null;

        private void RemoveFromStack(BaseUI ui)
        {
            if (__uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            var findUIPack = __uiStackList.Find(x => ReferenceEquals(x.ui, ui));
            if (ReferenceEquals(findUIPack, null) == true)
            {
                Debug.LogError("ui is NOT IN Stack!");
                return;
            }

            __uiStackList.Remove(findUIPack);
        }

        public void CloseWithID(int id)
        {
            if (__uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            var uiPack = __uiStackList.Find(x => x.id == id);
            if (ReferenceEquals(uiPack, null))
            {
                Debug.LogError("Cannot find UIPack!");
                return;
            }

            uiPack.ui.CloseAction();
        }

        /// <summary>
        /// 일반적 형태의 UI Close 함수. 인자를 포함하지 않으면 Stack의 최상단부터 닫는다.
        /// </summary>
        /// <param name="index">기본값은 -1. 이외의 값이 들어간다면 Stack에서 해당 Index의 UIPack을 찾아서 닫는다.</param>
        public void Close(int index = -1)
        {
            if (__uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            BaseUI ui = null;
            if (index < -1 || __uiStackList.Count <= index)
            {
                Debug.LogError($"INVALID index!: {index}");
                return;
            }

            if (index == -1)
            {
                ui = __uiStackList.PopLast()?.ui;
            }
            else
            {
                ui = __uiStackList.Pop(index)?.ui;
            }

            ui?.CloseAction();
        }

        public void CloseAll()
        {
            while (__uiStackList.Count > 0)
            {
                Close();
            }

            // NOTE(JJO): Stack에서 다 지웠음에도 남아있을 수도 있어서 추가로 남겨둠
            foreach (Transform child in __uiRoot)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void OnDestroy()
        {
            CloseAll();

            __uiStackList.Clear();
            __uiStackList = null;

            __uiRoot = null;
        }
    }
}
