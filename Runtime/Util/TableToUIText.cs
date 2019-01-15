//using UnityEngine;
//using UnityEngine.UI;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UniRx;
//using JJFramework.Runtime.Extension;

//namespace JJFramework.Runtime.Util
//{
//    [DisallowMultipleComponent]
//    public class TableToUIText : MonoBehaviour
//    {
//        private Text __text;

//        [SerializeField]
//        private int __tableIndex = -1;

//        void Awake()
//        {
//            if (__text == null)
//            {
//                __text = __text.GetComponentEx<Text>();
//            }
//        }

//        void Start()
//        {
//            if (__tableIndex == -1)
//            {
//                this.SetActive(false);
//                return;
//            }

//            SetText(__tableIndex);
//        }

//        public void SetText(int index)
//        {
//            // TODO(jjo): set text from table
//            SetText("");
//        }

//        public void SetText(string text)
//        {
//            // TODO(jjo): set text from table
//            this.__text.text = text;
//        }
//    }
//}
