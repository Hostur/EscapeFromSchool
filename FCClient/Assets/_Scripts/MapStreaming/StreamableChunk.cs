using System;
using System.Collections.Generic;
using FCData.DataUtils;
using UnityEngine;

namespace MapStreaming
{
	[Serializable]
	public class StreamableChunk : MonoBehaviour
	{
		[SerializeField] public List<int> Members;

		[SerializeField] private int _x;
		[SerializeField] private int _y;
		[SerializeField] private int _chunkSize;
		[SerializeField] public List<StreamablePrefab> _prefabs;
		[SerializeField] private bool _active;
		[SerializeField] private Vector3 _leftDownCorner;
		[SerializeField] private StreamableMap _streamableMap;

		public void Init(int x, int y, int chunkSize, StreamableMap streamableMap)
		{
			_streamableMap = streamableMap;
			_x = x;
			_y = y;
			_chunkSize = chunkSize;

			_leftDownCorner = new Vector3(_x * _chunkSize, 0, _y * _chunkSize);

			Members = new List<int>();
			_prefabs = new List<StreamablePrefab>();
			_active = true;
		}

		public void Add(int entityId)
		{
			if (Members.Contains(entityId)) return;

			Members.Add(entityId);
			Enable();
		}

		public void Remove(int entityId)
		{
			if (Members.Contains(entityId))
			{
				Members.Remove(entityId);
			}

			//if (Members.Count == 0)
			//{
			//	Disable();
			//}
		}

		public void Add(StreamablePrefab prefab)
		{
			_prefabs.Add(prefab);
		}

		public void Enable()
		{
			if (!_active)
			{
				_active = true;
				_prefabs.Each(e => e.Enable());
				// For enabled chunk we want to keep checking if we can disable it.
				InvokeRepeating(nameof(CheckForDisable), 10, 10);
			}
		}

		public void Disable()
		{
			if (_active)
			{
				_active = false;
				_prefabs.Each(e => e.Disable());
				CancelInvoke(nameof(CheckForDisable));
			}
		}

		private void CheckForDisable()
		{
			bool isEntityAround = _streamableMap.IsEntityAround(_x, _y);

			if(!isEntityAround)
				Disable();
		}

		private void OnDrawGizmos()
		{
			if (!_active) return;
#if UNITY_EDITOR
			Gizmos.color = Color.red;

			//Draw the suspension
			Gizmos.DrawLine(
				_leftDownCorner,
				_leftDownCorner + new Vector3(_chunkSize, 0, 0)
			);

			Gizmos.DrawLine(
				_leftDownCorner,
				_leftDownCorner + new Vector3(0, 0, _chunkSize)
			);

			Gizmos.color = Color.white;
#endif
		}
	}
}
