using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using GameRuntime.Core;
using JJFramework.Runtime.Extension;
using JJFramework.Runtime.Resource;

namespace JJFramework.Runtime.UI
{
    [DisallowMultipleComponent]
    public class UIManager
    {
        private static readonly string UI = "ui/Prefab";
        
        private Func<string, string, GameObject> _resourceLoader;

        public class UIPack
        {
            public BaseUI ui { get; private set; }
            public int id { get; private set; }

            public UIPack(int id, BaseUI ui)
            {
                this.id = id;
                this.ui = ui;
            }

            public void Cleanup()
            {
                ui = null;
            }
        }

        private List<UIPack> _uiStackList = new List<UIPack>();

        private Transform _uiRoot;
        private Canvas _canvas;

        public void Init(Func<string, string, GameObject> loader, Vector2 screenSize, int sortingOrder = 100)
        {
            _resourceLoader = loader;

            var obj = new GameObject(nameof(UIManager));

            _canvas = obj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;
            _canvas.sortingOrder = sortingOrder;
            // NOTE(JJO): Spine - SkeletonGraphic 덕분에 기본값이 100인 것을 1로 변경함.
            // 추후 SkeletonGraphic 버그(?)가 고쳐지면 변경될 수 있음.
            _canvas.referencePixelsPerUnit = 1f;

            var scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = screenSize;

            var raycaster = obj.AddComponent<GraphicRaycaster>();
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            raycaster.ignoreReversedGraphics = true;

            _uiRoot = obj.transform;
            GameObject.DontDestroyOnLoad(obj);
        }

        public void Cleanup()
        {
            foreach (var uiPack in _uiStackList)
            {
                uiPack.Cleanup();
            }
            _uiStackList.Clear();
            _uiStackList = null;

            _uiRoot = null;
            _canvas = null;

            _resourceLoader = null;
        }

        public T Generate<T>() where T : BaseUI
        {
            var origin = _resourceLoader?.Invoke(UI, $"{typeof(T).Name}.prefab");
            if (origin == null)
            {
                Debug.LogError($"{nameof(T)}을(를) 불러오지 못했습니다! - origin이 NULL입니다!");
                return null;
            }

            var uiObject = GameObject.Instantiate<GameObject>(origin, _uiRoot);
            if (uiObject == null)
            {
                Debug.LogError($"{nameof(T)}을(를) 불러오지 못했습니다! - Prefab 생성에 실패했습니다.");
                return null;
            }

            var ui = uiObject.AddComponent<T>();
            ui.RegistPreCloseAction(RemoveFromStack);

            //__uiStackList.Push(ui);
            _uiStackList.Add(new UIPack(ui.id, ui));

            if (_canvas.worldCamera == null)
            {
                _canvas.worldCamera = Camera.main;
            }

            return ui;
        }

        public BaseUI currentDialog => _uiStackList.Count > 0 ? _uiStackList.LastOrDefault()?.ui : null;

        private void RemoveFromStack(BaseUI ui)
        {
            if (_uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            var findUIPack = _uiStackList.Find(x => ReferenceEquals(x.ui, ui));
            if (ReferenceEquals(findUIPack, null) == true)
            {
                Debug.LogError("ui is NOT IN Stack!");
                return;
            }

            _uiStackList.Remove(findUIPack);
        }

        public void CloseWithID(int id)
        {
            if (_uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            var uiPack = _uiStackList.Find(x => x.id == id);
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
            if (_uiStackList.Count == 0)
            {
                Debug.LogError("Stack is EMPTY!");
                return;
            }

            BaseUI ui = null;
            if (index < -1 || _uiStackList.Count <= index)
            {
                Debug.LogError($"INVALID index!: {index}");
                return;
            }

            if (index == -1)
            {
                ui = _uiStackList.PopLast()?.ui;
            }
            else
            {
                ui = _uiStackList.Pop(index)?.ui;
            }

            ui?.CloseAction();
        }

        public void CloseAll()
        {
            while (_uiStackList.Count > 0)
            {
                Close();
            }

            // NOTE(JJO): Stack에서 다 지웠음에도 남아있을 수도 있어서 추가로 남겨둠
            foreach (Transform child in _uiRoot)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void OnDestroy()
        {
            CloseAll();

            _uiStackList.Clear();
            _uiStackList = null;

            _uiRoot = null;
        }
    }
}
