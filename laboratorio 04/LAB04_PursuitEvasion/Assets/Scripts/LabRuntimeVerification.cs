using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Se activa exclusivamente con --lab-verify. No altera una sesion normal.
[DefaultExecutionOrder(-100)]
public class LabRuntimeVerification : MonoBehaviour
{
    static LabRuntimeVerification instance;
    readonly List<string> rows = new List<string>();
    readonly List<string> errors = new List<string>();
    LabSession lab;
    string output;
    float elapsed, minDistance, sumLead;
    int samples, captures;
    bool driving;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--lab-verify") < 0 || instance != null) return;
        instance = new GameObject("Prueba automatizada explicita").AddComponent<LabRuntimeVerification>();
        DontDestroyOnLoad(instance.gameObject);
    }

    IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        int ix = Array.IndexOf(args, "--lab-output");
        output = ix >= 0 && ix + 1 < args.Length ? Path.GetFullPath(args[ix + 1]) : Path.Combine(Application.persistentDataPath, "LabVerification");
        Directory.CreateDirectory(output);
        Application.logMessageReceived += OnLog;
        Time.captureFramerate = 60;
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        rows.Add("escena,segundos,distancia_minima,distancia_final,adelanto_promedio,capturas,entrada");
        for (int i = 0; i < LabSession.SceneNames.Length; i++)
        {
            yield return SceneManager.LoadSceneAsync(LabSession.SceneNames[i]);
            yield return null;
            lab = FindFirstObjectByType<LabSession>();
            elapsed = 0; samples = 0; sumLead = 0; captures = 0; minDistance = float.MaxValue;
            lab.runner.UseTestInput = true;
            lab.TestStatus = "PRUEBA AUTOMATIZADA\nEntrada reproducible; no es WASD humano.";
            lab.runner.transform.position = new Vector3(6, .5f, 0);
            lab.predator.transform.position = new Vector3(-6, .5f, 0);
            lab.runner.ResetMotion();
            lab.predator.ResetMotion();
            foreach (var trail in FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None)) trail.Clear();
            if (i == 0)
            {
                CaptureCamera(Path.Combine(output, "00_Escena_base.png"));
            }
            bool midCaptured = false;
            driving = true;
            while (elapsed < 8f)
            {
                yield return null;
                float distance = Vector3.Distance(lab.runner.transform.position, lab.predator.transform.position);
                minDistance = Mathf.Min(minDistance, distance);
                sumLead += lab.runner.Velocity.magnitude * lab.predator.PredictionTime;
                samples++;
                if (!midCaptured && elapsed >= 3f)
                {
                    yield return new WaitForEndOfFrame();
                    CaptureCamera(Path.Combine(output, LabSession.SceneNames[i] + "_t03.png"));
                    midCaptured = true;
                }
                if (!IsFinite(distance) || lab.runner.Velocity.magnitude > lab.runner.maxSpeed + .001f ||
                    lab.predator.Velocity.magnitude > lab.predator.maxSpeed + .001f)
                    errors.Add("Invariante de movimiento: " + LabSession.SceneNames[i]);
            }
            driving = false;
            // Render real de la camara del juego. Funciona tambien con ventana oculta.
            yield return new WaitForEndOfFrame();
            CaptureCamera(Path.Combine(output, LabSession.SceneNames[i] + ".png"));
            yield return null;
            float final = Vector3.Distance(lab.runner.transform.position, lab.predator.transform.position);
            rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4:F4},{5},{6}",
                LabSession.SceneNames[i], elapsed, minDistance, final, sumLead / Mathf.Max(1, samples), captures,
                i == 7 ? "Runner inmovil" : lab.runner.mode == EvaderController.EvaderMode.Manual ? "direccion circular sintetica" : "Evasion autonoma"));
            if (i == 7 && (captures != 1 || !lab.capture.HasCaptured)) errors.Add("Se esperaba exactamente una captura.");
        }
        yield return null;
        File.WriteAllLines(Path.Combine(output, "resultados-runtime.csv"), rows);
        File.WriteAllText(Path.Combine(output, "resultado-runtime.txt"),
            "Unity " + Application.unityVersion + "\nUTC " + DateTime.UtcNow.ToString("O") +
            "\n8 escenas x 8 segundos simulados; deltaTime=1/60.\nErrores: " + errors.Count +
            "\n" + string.Join("\n", errors));
        Application.logMessageReceived -= OnLog;
        Debug.Log("LAB04_RUNTIME_FINISHED errors=" + errors.Count);
        Application.Quit(errors.Count == 0 ? 0 : 2);
    }

    void Update()
    {
        if (!driving || lab == null) return;
        elapsed += Time.deltaTime;
        if (SceneManager.GetActiveScene().name == LabSession.SceneNames[7]) lab.runner.TestInput = Vector3.zero;
        else lab.runner.TestInput = new Vector3(-Mathf.Sin(elapsed * .65f), 0, Mathf.Cos(elapsed * .65f));
    }
    void OnLog(string message, string stack, LogType type)
    {
        if (message == "Runner captured") captures++;
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message);
    }
    static bool IsFinite(float n) { return !float.IsNaN(n) && !float.IsInfinity(n); }

    static void CaptureCamera(string path)
    {
        // No incluye Inspector, Console ni el panel IMGUI. No se compone una imagen ficticia.
        Camera camera = Camera.main;
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        Rect previousRect = camera.rect;
        var target = new RenderTexture(1200, 1200, 24);
        var texture = new Texture2D(1200, 1200, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.rect = new Rect(0, 0, 1, 1);
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, 1200, 1200), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            camera.rect = previousRect;
            RenderTexture.active = previousActive;
            target.Release();
            Destroy(target);
            Destroy(texture);
        }
    }
}
