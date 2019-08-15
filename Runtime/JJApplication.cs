using System.Collections;
using System.Collections.Generic;
using JJFramework.Runtime.Extension;
using UnityEngine;

namespace JJFramework.Runtime
{
	public class JJApplication : MonoSingleton<JJApplication>
	{
		private readonly List<ExMonoBehaviour> _updateList = new List<ExMonoBehaviour>(0);
		private readonly List<ExMonoBehaviour> _lateUpdateList = new List<ExMonoBehaviour>(0);

		public void RegisterUpdate(ExMonoBehaviour behaviour)
		{
			_updateList.Add(behaviour);
		}

		public void UnregisterUpdate(ExMonoBehaviour behaviour)
		{
			_updateList.Remove(behaviour);
		}

		public void RegisterLateUpdate(ExMonoBehaviour behaviour)
		{
			_lateUpdateList.Add(behaviour);
		}

		public void UnregisterLateUpdate(ExMonoBehaviour behaviour)
		{
			_lateUpdateList.Remove(behaviour);
		}

		// Update is called once per frame
		private void Update()
		{
			foreach (var item in _updateList)
			{
				item.ManagedUpdate();
			}
		}

		private void LateUpdate()
		{
			foreach (var item in _lateUpdateList)
			{
				item.ManagedLateUpdate();
			}
		}

		private void OnDestroy()
		{
			_updateList.Clear();
			_lateUpdateList.Clear();
		}
	}
}
