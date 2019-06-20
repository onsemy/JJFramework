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

            //__uiStackList.Push(ui);
            __uiStackList.Add(new UIPack(ui.id, ui));

            if (__canvas.worldCamera == null)
            {
                __canvas.worldCamera = Camera.main;
            }

            return ui;
        }

        public BaseUI currentDialog => __uiStackList.Count > 0 ? __uiStackList.LastOrDefault()?.ui : null;

        public BaseUI Close(int index = -1, bool withoutCloseAction = false)
        {
            if (__uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return null;
            }

            BaseUI ui = null;
            if (index == -1)
            {
                if (__uiStackList.LastOrDefault() == null)
                {
                    Debug.LogError("Current Position is NULL!");
                    return null;
                }

                //ui = __uiStackList.Pop();
                ui = __uiStackList.PopLast()?.ui;
            }
            else
            {
                ui = __uiStackList.Pop(index)?.ui;
            }

            if (withoutCloseAction == false &&
                ui != null &&
                ui.isCalledCloseAction == false)
            {
                ui.CloseAction();
            }

            return ui;
        }

        public void CloseAll(bool withoutCloseAction = false, bool doDestroy = false)
        {
            while (__uiStackList.Count > 0)
            {
                var dialog = Close(withoutCloseAction: withoutCloseAction);
                if (doDestroy &&
                    dialog != null)
                {
                    GameObject.Destroy(dialog.gameObject);
                }
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
