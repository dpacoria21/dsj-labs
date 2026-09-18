using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneCompatibility
{
    public static void PrepareScene()
    {
        const string scenePath = "Assets/PetZombie/PetZombie.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        Transform[] transforms = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        int removed = transforms.Sum(transform => GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject));

        GameObject player = transforms.Select(transform => transform.gameObject).FirstOrDefault(item => item.name == "Player");
        GameObject zombie = transforms.Select(transform => transform.gameObject).FirstOrDefault(item => item.name == "Zombie");
        if (player == null || zombie == null)
            throw new InvalidOperationException("The Player or Zombie object could not be found in PetZombie.unity.");

        ZombieFollower follower = zombie.GetComponent<ZombieFollower>();
        if (follower == null)
            follower = zombie.AddComponent<ZombieFollower>();
        follower.goal = player;

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"PRESENTATION_COMPATIBILITY_OK removedMissingScripts={removed} follower={zombie.name} goal={player.name} scene={scenePath}");
    }
}
