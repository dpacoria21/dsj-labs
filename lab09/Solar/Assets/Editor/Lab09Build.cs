using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Paso 40: genera el APK sin abrir la ventana Build Profiles.
// Unity.exe -batchmode -quit -projectPath <proyecto> -executeMethod Lab09Build.BuildApk
public static class Lab09Build
{
    public const string ApkPath = "Builds/Solar.apk";

    [MenuItem("Lab09/Generar APK")]
    public static void BuildApk()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ApkPath)!);
        EditorUserBuildSettings.buildAppBundle = false;
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = ApkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[Lab09] Build {summary.result}: {ApkPath} ({summary.totalSize / (1024f * 1024f):F1} MB, " +
                  $"{summary.totalTime.TotalSeconds:F0} s, errores={summary.totalErrors})");
        if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
