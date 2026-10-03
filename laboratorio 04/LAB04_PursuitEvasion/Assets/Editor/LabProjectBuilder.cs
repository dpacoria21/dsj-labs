using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LabProjectBuilder
{
    const string SceneFolder = "Assets/Scenes";
    [MenuItem("Laboratorio 04/Regenerar escenas (reemplaza las ocho escenas)")]
    public static void CreateScenes()
    {
        Directory.CreateDirectory(SceneFolder);
        Directory.CreateDirectory("Assets/Materials");
        EditorSettings.serializationMode = SerializationMode.ForceText;
        PlayerSettings.companyName = "DSJ";
        PlayerSettings.productName = "LAB04 Pursuit Evasion";
        PlayerSettings.defaultScreenWidth = 1440;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        // La guia usa UnityEngine.Input. Se selecciona el Input Manager clasico.
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue = 0;
        settings.ApplyModifiedPropertiesWithoutUndo();
        for (int i = 0; i < LabSession.SceneNames.Length; i++) CreateScene(i);
        EditorBuildSettings.scenes = LabSession.SceneNames.Select(n =>
            new EditorBuildSettingsScene(SceneFolder + "/" + n + ".unity", true)).ToArray();
        EditorSceneManager.OpenScene(SceneFolder + "/" + LabSession.SceneNames[0] + ".unity");
        AssetDatabase.SaveAssets();
        Debug.Log("LAB04: ocho escenas generadas y referencias configuradas.");
    }

    static Material Mat(string name, Color color, bool unlit = false)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find(unlit ? "Unlit/Color" : "Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.2f);
        return material;
    }

    static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    static void Trail(GameObject agent, Color color)
    {
        var t = agent.AddComponent<TrailRenderer>();
        t.sharedMaterial = Mat("Trail" + agent.name, color, true);
        t.time = 7f;
        t.startWidth = 0.07f; t.endWidth = 0.025f;
        t.minVertexDistance = 0.1f;
        t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        t.receiveShadows = false;
    }

    static void CreateScene(int index)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var blue = Mat("RunnerBlue", new Color(0.08f, 0.43f, 1f));
        var red = Mat("PredatorRed", new Color(1f, 0.10f, 0.12f));
        var floor = Mat("Arena", new Color(0.10f, 0.16f, 0.22f));
        Shape("Arena", PrimitiveType.Plane, Vector3.zero, new Vector3(3, 1, 3), floor);
        var grid = new GameObject("Cuadricula visual");
        var gridMat = Mat("Grid", new Color(0.16f, 0.24f, 0.31f), true);
        for (int v = -14; v <= 14; v += 2)
        {
            var x = Shape("X " + v, PrimitiveType.Cube, new Vector3(v, 0.015f, 0), new Vector3(0.02f, 0.015f, 30), gridMat);
            var z = Shape("Z " + v, PrimitiveType.Cube, new Vector3(0, 0.015f, v), new Vector3(30, 0.015f, 0.02f), gridMat);
            UnityEngine.Object.DestroyImmediate(x.GetComponent<Collider>());
            UnityEngine.Object.DestroyImmediate(z.GetComponent<Collider>());
            x.transform.SetParent(grid.transform); z.transform.SetParent(grid.transform);
        }
        var runnerObj = Shape("Runner", PrimitiveType.Capsule, new Vector3(6, 0.5f, 0), Vector3.one, blue);
        var predatorObj = Shape("Predator", PrimitiveType.Capsule, new Vector3(-6, 0.5f, 0), Vector3.one, red);
        var runner = runnerObj.AddComponent<EvaderController>();
        var predator = predatorObj.AddComponent<PursuerAgent>();
        runner.pursuer = predator;
        predator.target = runner;
        Trail(runnerObj, new Color(0.20f, 0.55f, 1f));
        Trail(predatorObj, new Color(1f, 0.22f, 0.25f));
        var pursuitMarker = Shape("PursuitPredictionMarker", PrimitiveType.Sphere, new Vector3(6, 0.15f, 0), Vector3.one * 0.25f, Mat("Cyan", Color.cyan, true));
        var evasionMarker = Shape("EvasionPredictionMarker", PrimitiveType.Sphere, new Vector3(-6, 0.15f, 0), Vector3.one * 0.25f, Mat("Magenta", Color.magenta, true));
        pursuitMarker.GetComponent<Collider>().enabled = false;
        evasionMarker.GetComponent<Collider>().enabled = false;
        predator.predictionMarker = pursuitMarker.transform;
        runner.predictionMarker = evasionMarker.transform;
        predator.mode = index == 0 || index == 5 ? PursuerAgent.PursuitMode.Seek : PursuerAgent.PursuitMode.Pursuit;
        runner.mode = index >= 2 && index <= 4 ? EvaderController.EvaderMode.Evasion : EvaderController.EvaderMode.Manual;
        evasionMarker.SetActive(runner.mode == EvaderController.EvaderMode.Evasion);
        if (index == 3) { runner.predictionFactor = predator.predictionFactor = 0.03f; runner.maxPrediction = predator.maxPrediction = 0.25f; }
        if (index == 4) { runner.predictionFactor = predator.predictionFactor = 0.35f; runner.maxPrediction = predator.maxPrediction = 3f; }
        if (index == 5 || index == 6) { runner.maxSpeed = 5.5f; predator.maxSpeed = 5f; }
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 20, 0);
        camera.transform.rotation = Quaternion.Euler(90, 0, 0);
        camera.orthographic = true; camera.orthographicSize = 17;
        camera.rect = new Rect(0.23f, 0, 0.77f, 1);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.06f, 0.085f);
        camera.gameObject.AddComponent<AudioListener>();
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.3f;
        light.transform.rotation = Quaternion.Euler(60, -30, 0);
        RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.65f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        var lab = new GameObject("Laboratorio").AddComponent<LabSession>();
        lab.runner = runner; lab.predator = predator;
        var visuals = lab.gameObject.AddComponent<PredictionVisuals>();
        visuals.runner = runner; visuals.predator = predator;
        string[] titles = { "A / Seek", "B / Pursuit", "C / Evasion", "C / Reactiva", "C / Anticipada", "V.1 / Seek", "V.1 / Pursuit", "V.2 / Captura" };
        lab.experimentTitle = titles[index];
        lab.instructions = index >= 2 && index <= 4
            ? "Evasion automatica. Observa los puntos futuros y compara los limites de prediccion."
            : index == 7 ? "No muevas al Runner para comprobar la captura. Luego repite usando WASD."
            : "Mueve el Runner con WASD en curvas amplias. Repite un recorrido similar para comparar Seek y Pursuit.";
        if (index == 7)
        {
            lab.capture = lab.gameObject.AddComponent<CaptureDetector>();
            lab.capture.runner = runner; lab.capture.predator = predator;
        }
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), SceneFolder + "/" + LabSession.SceneNames[index] + ".unity");
    }

    public static void BuildAndVerify()
    {
        CreateScenes();
        LabVerification.Run();
        Directory.CreateDirectory("Builds/Windows");
        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/Windows/LAB04.exe",
            BuildTarget.StandaloneWindows64, BuildOptions.Development);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build fallido: " + report.summary.result);
        Debug.Log("LAB04_BUILD_OK bytes=" + report.summary.totalSize);
    }
}
