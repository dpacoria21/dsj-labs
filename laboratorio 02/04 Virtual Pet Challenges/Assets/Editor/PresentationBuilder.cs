using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PresentationBuilder
{
    public static void BuildWindows()
    {
        string[] scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        if (scenes.Length == 0) throw new InvalidOperationException("No scene was found under Assets.");
        string mainScene = scenes.FirstOrDefault(path => string.Equals(Path.GetFileName(path), "PetZombie.unity", StringComparison.OrdinalIgnoreCase)) ?? scenes[0];
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(mainScene, true) };
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string productName = new DirectoryInfo(projectRoot).Name;
        string outputDirectory = Path.Combine(projectRoot, "Builds", "Windows");
        Directory.CreateDirectory(outputDirectory);
        PlayerSettings.productName = productName;
        string executablePath = Path.Combine(outputDirectory, productName + ".exe");
        BuildReport report = BuildPipeline.BuildPlayer(new[] { mainScene }, executablePath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
        Debug.Log($"PRESENTATION_BUILD_OK scene={mainScene} output={executablePath}");
    }
}
