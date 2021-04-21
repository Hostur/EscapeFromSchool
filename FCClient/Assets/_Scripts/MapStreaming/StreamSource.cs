using FCData.Chunks;
using UnityEngine;
#pragma warning disable 649

namespace MapStreaming
{
	public class StreamSource : MonoBehaviour
	{
		[SerializeField] public int SourceId;
		[SerializeField] private StreamableMap _streamableMap;
		[SerializeField] private int _chunkSize;
		[SerializeField] private Chunk _currentChunk;

		private void Awake()
		{
			InvokeRepeating(nameof(RecalculateChunk), 1, 0.2f);
		}

		private void OnEnable()
		{
			if (_streamableMap == null)
				_streamableMap = FindObjectOfType<StreamableMap>();
			
			_streamableMap.Remove(_currentChunk, this);
			_streamableMap.Add(PositionToChunk(), this);
		}

		private void OnDisable()
		{
			_streamableMap.Remove(_currentChunk, this);
		}

		private Chunk PositionToChunk()
		{
			var position = transform.position.ToSerializableVector3();
			var chunk = ChunksUtils.ConvertToChunk(in position, in _chunkSize);
			return chunk;
		}

		private void RecalculateChunk()
		{
			var tmpChunk = PositionToChunk();
			if (tmpChunk != _currentChunk)
			{
				UnityEngine.Debug.Log($"Remove from {_currentChunk} and add to {tmpChunk}");
				_streamableMap.Remove(_currentChunk, this);
				_streamableMap.Add(tmpChunk, this);
				_currentChunk = tmpChunk;
			}
		}
	}
}
