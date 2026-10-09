using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class TutorialVerification
{
    const string Key = "TutorialVerification.Running";
    static double started;
    static bool highCapture, lowCapture, lowImage, unreachableScenario;
    static int errors, finalShots, finalHighHits, finalLowHits;
    static Vector3 initialPosition, chaseStart;
    static float reloadDisplacement;
    static string Lab => Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;

    static TutorialVerification()
    {
        if (SessionState.GetBool(Key, false))
        {
            Attach();
            EditorApplication.delayCall += () => { started = EditorApplication.timeSinceStartup; };
        }
    }

    public static void Run()
    {
        Require(FireShell.TryCalculateAngle(12f, 0f, 15f, 9.81f, true, out float low), "arco bajo calculable");
        Require(FireShell.TryCalculateAngle(12f, 0f, 15f, 9.81f, false, out float high), "arco alto calculable");
        Require(low < high && Math.Abs(low + high - 90f) < 0.01f, "dos raíces complementarias");
        Require(!FireShell.TryCalculateAngle(100f, 0f, 15f, 9.81f, true, out _), "blanco fuera de alcance");
        EditorSceneManager.OpenScene(TutorialBuild.Scene);
        SessionState.SetBool(Key, true);
        Attach();
        // Dejar terminar inicialización/indexación del Editor antes de Play Mode.
        EditorApplication.delayCall += () => EditorApplication.EnterPlaymode();
    }

    static void Attach()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Application.logMessageReceived -= Log;
        Application.logMessageReceived += Log;
    }

    static void Log(string text, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }

    static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Verificación falló: " + check);
        Debug.Log("TUTORIAL_CHECK_OK " + check);
    }

    static void Capture(string name)
    {
        Camera camera = Camera.main;
        RenderTexture target = new RenderTexture(1440, 900, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        Texture2D pixels = new Texture2D(1440, 900, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0);
        pixels.Apply();
        string folder = Path.Combine(Lab, "evidencias", "tutorial");
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder, name + ".png"), pixels.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(pixels);
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
    }

    static string Number(float value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    static void Tick()
    {
        if (!EditorApplication.isPlaying || !SessionState.GetBool(Key, false)) return;
        FireShell tank = UnityEngine.Object.FindFirstObjectByType<FireShell>();
        if (tank == null) return;
        double elapsed = EditorApplication.timeSinceStartup - started;
        if (!highCapture && elapsed >= 2.1)
        {
            initialPosition = tank.transform.position;
            Capture("01-arco-alto-unity");
            highCapture = true;
        }
        if (!lowCapture && elapsed >= 5.2)
        {
            tank.lowArc = true;
            lowCapture = true;
        }
        if (lowCapture && !lowImage && elapsed >= 6.4)
        {
            Capture("02-arco-bajo-unity");
            lowImage = true;
        }
        if (!unreachableScenario && elapsed >= 8.5)
        {
            finalShots = tank.Shots;
            finalHighHits = tank.HighArcHits;
            finalLowHits = tank.LowArcHits;
            reloadDisplacement = Vector3.Distance(tank.transform.position, initialPosition);
            chaseStart = tank.transform.position;
            tank.enemy.transform.position += new Vector3(100f, 0f, 0f);
            unreachableScenario = true;
        }
        if (unreachableScenario && elapsed >= 9.3)
        {
            float chaseDistance = Vector3.Distance(tank.transform.position, chaseStart);
            bool noUnreachableShots = tank.Shots == finalShots;
            bool passed = errors == 0 && finalHighHits >= 1 && finalLowHits >= 1 && finalShots >= 4 &&
                reloadDisplacement < 0.02f && chaseDistance > 0.5f && noUnreachableShots && !tank.HasSolution;
            string result = "{\n" +
                "  \"unity\": \"" + Application.unityVersion + "\",\n" +
                "  \"durationSeconds\": 9.3,\n" +
                "  \"errors\": " + errors + ",\n" +
                "  \"shots\": " + finalShots + ",\n" +
                "  \"highArcTargetHits\": " + finalHighHits + ",\n" +
                "  \"lowArcTargetHits\": " + finalLowHits + ",\n" +
                "  \"reloadDisplacementMetres\": " + Number(reloadDisplacement) + ",\n" +
                "  \"unreachableChaseMetres\": " + Number(chaseDistance) + ",\n" +
                "  \"noUnreachableShots\": " + noUnreachableShots.ToString().ToLowerInvariant() + ",\n" +
                "  \"passed\": " + passed.ToString().ToLowerInvariant() + "\n}";
            string folder = Path.Combine(Lab, "pruebas", "tutorial");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "resultados.json"), result);
            Debug.Log("TUTORIAL_PLAYMODE_FINISHED " + result);
            SessionState.EraseBool(Key);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(passed ? 0 : 2);
        }
    }
}
