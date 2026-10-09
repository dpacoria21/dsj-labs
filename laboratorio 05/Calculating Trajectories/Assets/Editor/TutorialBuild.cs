using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class TutorialBuild
{
    public const string Scene = "Assets/Tanks.unity";
    public static void PrepareAndBuild()
    {
        EditorSceneManager.OpenScene(Scene);
        FireShell tank = UnityEngine.Object.FindFirstObjectByType<FireShell>();
        if (tank == null || tank.bullet == null || tank.turretBase == null || tank.enemy == null)
            throw new InvalidOperationException("Referencias de tutorial no conectadas.");
        tank.gameObject.name = "Tanque IA (Unity Learn)";
        tank.enemy.name = "Objetivo - WASD";
        GameObject ground = GameObject.Find("Ground");
        ground.tag = "Untagged";
        Physics.gravity = new Vector3(0f, -9.81f, 0f);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.7f, 0.75f, 0.8f);
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(1f, 12f, -22f);
        camera.transform.LookAt(new Vector3(1f, 6.5f, 3.82f));
        camera.orthographic = true;
        camera.orthographicSize = 9f;
        camera.backgroundColor = new Color(0.08f, 0.14f, 0.22f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        TutorialPresentation presentation = UnityEngine.Object.FindFirstObjectByType<TutorialPresentation>();
        if (presentation == null) presentation = new GameObject("Presentación del laboratorio").AddComponent<TutorialPresentation>();
        presentation.tank = tank;
        LineRenderer line = presentation.GetComponent<LineRenderer>();
        if (line == null) line = presentation.gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Trajectory.mat");
        if (line.sharedMaterial == null) {
            line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(line.sharedMaterial, "Assets/Materials/Trajectory.mat");
        }
        line.startColor = line.endColor = new Color(0.05f, 0.92f, 0.9f);
        line.startWidth = line.endWidth = 0.065f;
        line.positionCount = 0;
        line.useWorldSpace = true;
        presentation.trajectory = line;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
        PlayerSettings.companyName = "DSJ Labs";
        PlayerSettings.productName = "Laboratorio 05 - Calculating Trajectories";
        PlayerSettings.defaultScreenWidth = 1440;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, "Builds", "Windows", "Calculating Trajectories.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new[] { Scene }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException(report.summary.result.ToString());
        Debug.Log("TUTORIAL_BUILD_OK bytes=" + report.summary.totalSize);
    }
}


