using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Lab08Verification
{
    private const string SessionKey = "Lab08Verification.Running";
    private static readonly List<string> results = new();
    private static int failures;
    private static int runtimeErrors;
    private static IEnumerator scenario;
    private static int lastFrame = -1;
    private static double deadline;

    static Lab08Verification()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Run()
    {
        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            SessionState.SetBool(SessionKey, true);
            EditorApplication.isPlaying = true;
        };
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(SessionKey, false))
            return;

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Time.captureFramerate = 60;
            UnityEngine.Random.InitState(808);
            deadline = EditorApplication.timeSinceStartup + 120;
            Application.logMessageReceived += CaptureErrors;
            scenario = CheckScene();
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            int exitCode = SessionState.GetInt(SessionKey + ".ExitCode", 1);
            SessionState.EraseBool(SessionKey);
            EditorApplication.Exit(exitCode);
        }
    }

    private static void CaptureErrors(string message, string stackTrace, LogType type)
    {
        if (type is LogType.Error or LogType.Exception or LogType.Assert)
            runtimeErrors++;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup > deadline)
        {
            Check("Play Mode timeout", false);
            Finish();
            return;
        }

        if (lastFrame == Time.frameCount)
            return;

        lastFrame = Time.frameCount;
        try
        {
            if (!scenario.MoveNext())
                Finish();
        }
        catch (Exception error)
        {
            Check(error.ToString(), false);
            Finish();
        }
    }

    private static void Check(string name, bool passed, string details = "")
    {
        string result = (passed ? "PASS " : "FAIL ") + name;
        if (!string.IsNullOrEmpty(details))
            result += " | " + details;
        results.Add(result);
        if (!passed)
            failures++;
        Debug.Log(result);
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= CaptureErrors;
        Check("No runtime errors", runtimeErrors == 0, $"errors={runtimeErrors}");
        string output = OutputDirectory();
        Directory.CreateDirectory(output);
        results.Insert(0, $"Unity {Application.unityVersion} | {GraphicsDevice()}");
        File.WriteAllLines(Path.Combine(output, "resultados.txt"), results);
        SessionState.SetInt(SessionKey + ".ExitCode", failures == 0 ? 0 : 1);
        EditorApplication.isPlaying = false;
    }

    private static string OutputDirectory()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-lab08Output");
        return flag >= 0 && flag + 1 < args.Length ? args[flag + 1]
            : Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/Lab08"));
    }

    private static GraphicsDeviceType GraphicsDevice() => SystemInfo.graphicsDeviceType;

    private static void CheckRendering()
    {
        int missingScripts = 0;
        int missingMeshes = 0;
        int invalidMaterials = 0;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
        }

        foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            if (filter.sharedMesh == null)
                missingMeshes++;

        foreach (SkinnedMeshRenderer renderer in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None))
            if (renderer.sharedMesh == null)
                missingMeshes++;

        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null || material.shader == null ||
                    material.shader.name == "Hidden/InternalErrorShader" ||
                    (GraphicsDevice() != GraphicsDeviceType.Null &&
                        (!material.shader.isSupported || ShaderUtil.ShaderHasError(material.shader))))
                    invalidMaterials++;
            }
        }

        foreach (Terrain terrain in Terrain.activeTerrains)
        {
            Material material = terrain.materialTemplate;
            if (material == null || material.shader == null || material.shader.name == "Hidden/InternalErrorShader" ||
                (GraphicsDevice() != GraphicsDeviceType.Null &&
                    (!material.shader.isSupported || ShaderUtil.ShaderHasError(material.shader))))
                invalidMaterials++;
        }

        Check("Scene references", missingScripts == 0 && missingMeshes == 0 && invalidMaterials == 0,
            $"missing scripts={missingScripts}, missing meshes={missingMeshes}, invalid materials={invalidMaterials}");
    }

    private static void CaptureCamera()
    {
        if (GraphicsDevice() == GraphicsDeviceType.Null)
            return;

        const int width = 1280;
        const int height = 720;
        Camera camera = Camera.main;
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
        RenderTexture previous = RenderTexture.active;
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            string output = OutputDirectory();
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, "play-mode.jpg"), image.EncodeToJPG(90));
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.Destroy(image);
        }
    }

    private static IEnumerator CheckScene()
    {
        yield return null;
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        Check("Player and thirteen enemies are active", player != null && enemies.Length == 13,
            $"enemies={enemies.Length}");
        Check("Enemy layer has a trigger for every agent", Array.TrueForAll(enemies,
            enemy => enemy.gameObject.layer == LayerMask.NameToLayer("Enemy") &&
                enemy.TryGetComponent(out CapsuleCollider trigger) && trigger.isTrigger));
        CheckRendering();

        Vector3 start = player.transform.position;
        Vector3 initialCameraOffset = Camera.main.transform.position - start;
        float initialDistance = MeanDistance(enemies, start);
        for (int frame = 0; frame < 300; frame++)
            yield return null;
        float settledDistance = MeanDistance(enemies, player.transform.position);
        float minimumGap = MinimumGap(enemies);
        Check("Enemies approach and spread around the stationary player",
            settledDistance < initialDistance * 0.55f && minimumGap > 0.65f,
            $"distance {initialDistance:F2} to {settledDistance:F2} m, nearest pair={minimumGap:F2} m");
        CaptureCamera();

        Vector3[] destinations = {start + new Vector3(-6, 0, -4), start + new Vector3(-1, 0, 3)};
        foreach (Vector3 destination in destinations)
        {
            player.SetDestination(destination);
            for (int frame = 0; frame < 300; frame++)
                yield return null;

            float arrival = Vector3.Distance(player.transform.position, destination);
            float distance = MeanDistance(enemies, player.transform.position);
            minimumGap = MinimumGap(enemies);
            Check("Player reaches waypoint and enemies follow after the turn",
                arrival < 0.1f && distance < 5 && minimumGap > 0.55f,
                $"destination={destination}, player error={arrival:F2} m, mean distance={distance:F2} m, nearest pair={minimumGap:F2} m");
        }

        Vector3 finalCameraOffset = Camera.main.transform.position - player.transform.position;
        float offsetChange = Vector3.Distance(initialCameraOffset, finalCameraOffset);
        Check("Camera follows the player", offsetChange < 0.01f,
            $"offset change={offsetChange:F3} m");
    }

    private static float MeanDistance(EnemyController[] enemies, Vector3 target)
    {
        float total = 0;
        foreach (EnemyController enemy in enemies)
            total += Vector3.Distance(enemy.transform.position, target);
        return total / enemies.Length;
    }

    private static float MinimumGap(EnemyController[] enemies)
    {
        float closest = float.PositiveInfinity;
        for (int first = 0; first < enemies.Length; first++)
            for (int second = first + 1; second < enemies.Length; second++)
                closest = Mathf.Min(closest, Vector3.Distance(enemies[first].transform.position,
                    enemies[second].transform.position));
        return closest;
    }

}
