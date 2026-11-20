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
        Flock[] groups = Object.FindObjectsByType<Flock>(FindObjectsSortMode.None);
        FlockUnit[] fish = Object.FindObjectsByType<FlockUnit>(FindObjectsSortMode.None);
        Terrain terrain = Terrain.activeTerrain;
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        Check("Two flocks with 100 fish each", groups.Length == 2 && fish.Length == 200 &&
            Array.TrueForAll(groups, group => group.allUnits?.Length == 100));
        Check("Terrain collider on Obstacle layer", terrain != null && terrain.gameObject.layer == obstacleLayer &&
            terrain.TryGetComponent(out TerrainCollider collider) && collider.enabled);
        Check("Free camera installed", Camera.main != null &&
            Camera.main.TryGetComponent(out UnityTemplateProjects.SimpleCameraController _));
        CheckRendering();

        Vector3[] previous = Array.ConvertAll(fish, unit => unit.transform.position);
        float travel = 0;
        float minimumClearance = float.PositiveInfinity;
        bool valid = true;
        for (int frame = 0; frame < 360; frame++)
        {
            yield return null;
            for (int i = 0; i < fish.Length; i++)
            {
                Vector3 point = fish[i].transform.position;
                valid &= float.IsFinite(point.x) && float.IsFinite(point.y) && float.IsFinite(point.z);
                minimumClearance = Mathf.Min(minimumClearance,
                    point.y - terrain.SampleHeight(point) - terrain.GetPosition().y);
                travel += Vector3.Distance(previous[i], point);
                previous[i] = point;
            }
        }

        float farthest = 0;
        foreach (Flock group in groups)
            foreach (FlockUnit unit in group.allUnits)
                farthest = Mathf.Max(farthest, Vector3.Distance(unit.transform.position, group.transform.position));

        Check("Collective movement for six simulated seconds", valid && travel / fish.Length > 6 && farthest < 24,
            $"fish={fish.Length}, mean travel={travel / fish.Length:F2} m, farthest from spawn center={farthest:F2} m");
        Check("Fish stay above the terrain in the scene run", minimumClearance > 0.12f,
            $"minimum clearance={minimumClearance:F2} m");
        CaptureCamera();

        FlockUnit probe = groups[0].allUnits[0];
        Vector3 probePosition = groups[0].transform.position;
        probePosition.y = terrain.SampleHeight(probePosition) + terrain.GetPosition().y + 1.5f;
        probe.transform.SetPositionAndRotation(probePosition, Quaternion.LookRotation(Vector3.down, Vector3.forward));
        bool detected = Physics.Raycast(probePosition, Vector3.down, 2, 1 << obstacleLayer);
        bool steered = false;
        float probeClearance = float.PositiveInfinity;
        for (int frame = 0; frame < 120; frame++)
        {
            yield return null;
            steered |= probe.IsAvoidingObstacle;
            Vector3 point = probe.transform.position;
            probeClearance = Mathf.Min(probeClearance,
                point.y - terrain.SampleHeight(point) - terrain.GetPosition().y);
        }
        Check("Fish turns away from a direct approach to the terrain", detected && steered && probeClearance > 0.1f,
            $"ray hit={detected}, avoidance activated={steered}, minimum clearance={probeClearance:F2} m");
    }

}
