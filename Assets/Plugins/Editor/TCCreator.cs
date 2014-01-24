using UnityEditor;
using UnityEngine;

public static class TCParticlesCreator
{
	[MenuItem("GameObject/Create Other/TC Particle system")]
	private static void CreateTcSystem()
	{
		var go = new GameObject();
		go.AddComponent<TCParticleSystem>();
		go.name = "TC Particle system";
		Selection.activeGameObject = go;
		go.transform.position = SceneView.currentDrawingSceneView.pivot;
	}
}