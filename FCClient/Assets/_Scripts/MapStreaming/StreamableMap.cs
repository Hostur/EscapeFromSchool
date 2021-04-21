using System.Collections.Generic;
using FCData.Chunks;
using FCData.DataUtils;
using UnityEngine;
#pragma warning disable 649

namespace MapStreaming
{
	public class StreamableMap : FCBehaviour
	{
		[SerializeField] private StreamableChunk _prefab;
		[SerializeField] private int _chunkSize;
		[SerializeField] private int _mapWidth;
		[SerializeField] private int _mapHeight;
		private int _width;
		private int _height;

		[SerializeField] private StreamableChunk[] _chunks;

		protected override void OnAwake()
		{
			base.OnAwake();
			_width = _mapWidth / _chunkSize;
			_height = _mapHeight / _chunkSize;
		}

		public StreamableChunk this[int x, int y]
		{
			get => _chunks[x + (y * _width)];
			set => _chunks[x + (y * _width)] = value;
		}

		public void Rebuild()
		{
			BuildChunks();
			RebuildMap();
		}

		private void BuildChunks()
		{
			FindObjectsOfType<StreamableChunk>().Each(s => DestroyImmediate(s.gameObject));
			_width = _mapWidth / _chunkSize;
			_height = _mapHeight / _chunkSize;
			_chunks = new StreamableChunk[_width * _height];

			for (int y = 0; y < _height; y++)
			for (int x = 0; x < _width; x++)
			{
				var instance = GameObject.Instantiate(_prefab, new Vector3(x * _chunkSize, 0, y * _chunkSize),
					Quaternion.identity);

				instance.Init(x, y, _chunkSize, this);
				instance.name = $"{x},{y}";
				instance.transform.SetParent(transform);
				this[x, y] = instance;
			}

			UnityEngine.Debug.Log($"Built {_chunks.Length} chunks.");
		}

		private void RebuildMap()
		{
			var streamablePrefabs = FindObjectsOfType<StreamablePrefab>();

			foreach (var streamablePrefab in streamablePrefabs)
			{
				var position = streamablePrefab.transform.position.ToSerializableVector3();
				Chunk chunk = ChunksUtils.ConvertToChunk(in position, _chunkSize);
				this[chunk.X, chunk.Y].Add(streamablePrefab);
			}

			UnityEngine.Debug.Log($"Built {streamablePrefabs.Length} streamable prefabs.");
		}

		public bool IsEntityAround(int x, int y)
		{
			if (this[x, y].Members.Count > 0) return true;

			foreach (Chunk neighbor in GetNeighbors(x, y))
			{
				if (this[neighbor.X, neighbor.Y].Members.Count > 0)
					return true;
			}

			return false;
		}

		private IEnumerable<Chunk> GetNeighbors(int x, int y)
		{
			if (x > 0)
				yield return new Chunk(x - 1, y);
			if (y > 0)
				yield return new Chunk(x, y - 1);
			if (x > 0 && y > 0)
				yield return new Chunk(x - 1, y - 1);
			if (x <= _width)
				yield return new Chunk(x + 1, y);
			if (y <= _height)
				yield return new Chunk(x, y + 1);
			if (x <= _width && y <= _height)
				yield return new Chunk(x + 1, y + 1);
			if (y <= _height && x > 0)
				yield return new Chunk(x - 1, y + 1);
			if (y > 0 && x <= _width)
				yield return new Chunk(x + 1, y - 1);
		}

		public void EnableEverything()
		{
			foreach (StreamableChunk streamableChunk in _chunks)
			{
				streamableChunk.Enable();
			}
		}

		public void DisableEverything()
		{
			foreach (StreamableChunk streamableChunk in _chunks)
			{
				streamableChunk.Disable();
			}
		}

		public void Add(Chunk chunk, StreamSource source)
		{
			this[chunk.X, chunk.Y].Add(source.SourceId);

			foreach (Chunk neighbor in GetNeighbors(chunk.X, chunk.Y))
			{
				this[neighbor.X, neighbor.Y].Enable();
			}
		}

		public void Remove(Chunk chunk, StreamSource source)
		{
			this[chunk.X, chunk.Y].Remove(source.SourceId);
		}
	}
}
