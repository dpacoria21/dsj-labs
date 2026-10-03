using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Panel didactico: configuracion, magnitudes reales y acceso a cada experimento.
public class LabSession : MonoBehaviour
{
    public EvaderController runner;
    public PursuerAgent predator;
    public CaptureDetector capture;
    public string experimentTitle;
    [TextArea] public string instructions;
    public string TestStatus { get; set; }
    GUIStyle titleStyle, textStyle;
    public static readonly string[] SceneNames = {
        "01_A_Seek_Manual", "02_B_Pursuit_Manual", "03_C_Pursuit_Evasion",
        "04_C_Reactiva", "05_C_Anticipada", "06_E1_Seek_RunnerRapido",
        "07_E1_Pursuit_RunnerRapido", "08_E2_Captura"
    };
    static readonly string[] Labels = {
        "A - Seek + Manual", "B - Pursuit + Manual", "C - Pursuit + Evasion",
        "C - Reactiva (T corto)", "C - Anticipada (T largo)", "V.1 - Seek / 5.5 y 5.0",
        "V.1 - Pursuit / 5.5 y 5.0", "V.2 - Captura < 0.8"
    };

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (Input.GetKeyDown(KeyCode.F12)) StartCoroutine(CaptureScreen());
    }

    IEnumerator CaptureScreen()
    {
        yield return new WaitForEndOfFrame();
        string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Capturas"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, SceneManager.GetActiveScene().name + "_" +
            DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Captura real guardada: " + path);
    }

    void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold, wordWrap = true };
            titleStyle.normal.textColor = Color.white;
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            textStyle.normal.textColor = new Color(0.88f, 0.92f, 0.96f);
        }
        float scale = Mathf.Max(0.55f, Screen.height / 900f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        GUI.Box(new Rect(8, 8, 305, 880), GUIContent.none);
        GUILayout.BeginArea(new Rect(24, 24, 273, 850));
        GUILayout.Label("DSJ / LAB 04", textStyle);
        GUILayout.Space(10);
        GUILayout.Label(experimentTitle, titleStyle);
        GUILayout.Space(12);
        GUILayout.Label(instructions, textStyle);
        GUILayout.Space(12);
        GUILayout.Label("Azul: Runner  |  Rojo: Predator\nCian: prediccion de Runner\nMagenta: prediccion de Predator\nAmarillo: direccion de persecucion", textStyle);
        GUILayout.Space(12);
        GUILayout.Label(string.Format("Runner  {0}  / max {1:0.0}\nPredator  {2}  / max {3:0.0}\nDistancia: {4:0.000} u\nT Predator: {5:0.000} s\nT Runner: {6:0.000} s\nFactor / maxT: {7:0.00} / {8:0.00}\nTiempo: {9:0.00} s",
            runner.mode, runner.maxSpeed, predator.mode, predator.maxSpeed,
            Vector3.Distance(runner.transform.position, predator.transform.position),
            predator.PredictionTime, runner.PredictionTime, runner.predictionFactor,
            runner.maxPrediction, Time.timeSinceLevelLoad), textStyle);
        if (capture != null)
            GUILayout.Label(capture.HasCaptured ? "Runner captured\nRegistro unico en Console" : "Captura: pendiente (< 0.8)", textStyle);
        if (!string.IsNullOrEmpty(TestStatus)) GUILayout.Label(TestStatus, textStyle);
        GUILayout.Space(10);
        for (int i = 0; i < SceneNames.Length; i++)
            if (GUILayout.Button(Labels[i], GUILayout.Height(27))) SceneManager.LoadScene(SceneNames[i]);
        GUILayout.Space(10);
        GUILayout.Label("WASD mover / R reiniciar\nF12 guardar captura real", textStyle);
        GUILayout.EndArea();
        GUI.matrix = Matrix4x4.identity;
    }
}
