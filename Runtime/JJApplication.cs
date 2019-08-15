using System.Collections;
using System.Collections.Generic;
using JJFramework.Runtime.Extension;
using UnityEngine;

namespace JJFramework.Runtime
{
	public class JJApplication : MonoSingleton<JJApplication>
	{
		private readonly List<ExMonoBehaviour> _behaviourList = new List<ExMonoBehaviour>(0);

		public void RegisterBehaviour(ExMonoBehaviour behaviour)
		{
			_behaviourList.Add(behaviour);
		}

		public void UnregisterBehaviour(ExMonoBehaviour behaviour)
		{
			_behaviourList.Remove(behaviour);
		}

		// Update is called once per frame
		private void Update()
		{
			foreach (var behaviour in _behaviourList)
			{
				behaviour.ManagedUpdate();
			}
		}
	}
}
