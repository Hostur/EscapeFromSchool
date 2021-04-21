using UnityEngine;

namespace MapStreaming
{
	public class StreamablePrefab : MonoBehaviour
	{
		public void Enable() => gameObject.SetActive(true);
		public void Disable() => gameObject.SetActive(false);
	}
}
