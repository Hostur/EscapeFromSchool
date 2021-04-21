using MapStreaming;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StreamableMap))]
public class StreamableMapEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var script = (StreamableMap)target;

		if(GUILayout.Button("Rebuild map", GUILayout.Height(40)))
		{
			script.Rebuild();
		}

		if (GUILayout.Button("Disable everything", GUILayout.Height(40)))
		{
			script.DisableEverything();
		}

		if (GUILayout.Button("Enable everything", GUILayout.Height(40)))
		{
			script.EnableEverything();
		}
	}
}
