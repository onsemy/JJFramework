using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace JJFramework.Runtime.Extension
{
    [DisallowMultipleComponent]
    public class ExMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            this.LoadComponents();
        }
    }
}
