using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneCompatibility
{
    public static void RemoveMissingScripts()
    {
        const string scenePath = "Assets/Moving.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        int removed = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Sum(transform => GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject));
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"PRESENTATION_COMPATIBILITY_OK removedMissingScripts={removed} scene={scenePath}");
    }
}
