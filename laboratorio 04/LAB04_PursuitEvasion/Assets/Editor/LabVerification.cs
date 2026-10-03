using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Comprobaciones de configuracion y propiedades de movimiento, sin paquetes externos.
public static class LabVerification
{
    static readonly List<string> passed = new List<string>();
    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception("LAB04 FAIL: " + label);
        passed.Add("PASS " + label);
    }
    [MenuItem("Laboratorio 04/Verificar configuracion y algoritmos")]
    public static void Run()
    {
        passed.Clear();
        for (int i = 0; i < LabSession.SceneNames.Length; i++)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + LabSession.SceneNames[i] + ".unity");
            var r = UnityEngine.Object.FindFirstObjectByType<EvaderController>();
            var p = UnityEngine.Object.FindFirstObjectByType<PursuerAgent>();
            Check(r != null && p != null && r.pursuer == p && p.target == r, "Referencias " + LabSession.SceneNames[i]);
            Check(!p.predictionMarker.GetComponent<Collider>().enabled && !r.predictionMarker.GetComponent<Collider>().enabled, "Marcadores sin collider " + i);
            Check(r.transform.position == new Vector3(6, .5f, 0) && p.transform.position == new Vector3(-6, .5f, 0), "Posiciones iniciales " + i);
            Check(r.mode == (i >= 2 && i <= 4 ? EvaderController.EvaderMode.Evasion : EvaderController.EvaderMode.Manual), "Modo Runner " + i);
            Check(p.mode == (i == 0 || i == 5 ? PursuerAgent.PursuitMode.Seek : PursuerAgent.PursuitMode.Pursuit), "Modo Predator " + i);
            Check((i == 5 || i == 6) ? r.maxSpeed == 5.5f && p.maxSpeed == 5f : r.maxSpeed == 5f && p.maxSpeed == 4.6f, "Velocidades " + i);
        }
        Check(!CaptureDetector.IsCaptureDistance(0.8f), "Distancia 0.8 NO captura");
        Check(!CaptureDetector.IsCaptureDistance(0.8001f), "Distancia mayor NO captura");
        Check(CaptureDetector.IsCaptureDistance(0.7999f), "Distancia menor SI captura");
        EditorSceneManager.OpenScene("Assets/Scenes/" + LabSession.SceneNames[1] + ".unity");
        var runner = UnityEngine.Object.FindFirstObjectByType<EvaderController>();
        var predator = UnityEngine.Object.FindFirstObjectByType<PursuerAgent>();
        for (int i = 0; i < 120; i++) runner.Step(1f / 60f, new Vector3(1, 0, 1));
        Check(runner.Velocity.magnitude <= runner.maxSpeed + .0001f, "Diagonal respeta velocidad maxima");
        var previous = runner.Velocity;
        runner.Step(.02f, Vector3.left);
        Check((runner.Velocity - previous).magnitude <= runner.maxAcceleration * .02f + .0001f, "Cambio de velocidad respeta aceleracion");
        predator.Step(0f);
        Check(Vector3.Distance(predator.TargetPoint, runner.transform.position + runner.Velocity * predator.PredictionTime) < .0001f, "Pursuit predice con velocidad");
        Check(predator.PredictionTime <= predator.maxPrediction, "T limitado por maxPrediction");
        predator.maxPrediction = 0;
        predator.Step(0f);
        Check(Vector3.Distance(predator.TargetPoint, runner.transform.position) < .0001f, "T=0 equivale a Seek");
        runner.mode = EvaderController.EvaderMode.Evasion;
        runner.ResetMotion(); predator.maxPrediction = 1.5f;
        predator.Step(.1f);
        runner.Step(.1f, Vector3.zero);
        Check(Vector3.Dot(runner.Velocity, runner.transform.position - runner.PredictedPursuer) > 0, "Evasion se aleja del punto futuro");
        runner.transform.position = new Vector3(30, .5f, -30);
        runner.Step(0f, Vector3.zero);
        Check(Mathf.Abs(runner.transform.position.x) <= 13 && Mathf.Abs(runner.transform.position.z) <= 13, "Limites arena");
        Directory.CreateDirectory("../evidencias");
        File.WriteAllLines("../evidencias/verificacion-configuracion.txt", passed);
        EditorSceneManager.OpenScene("Assets/Scenes/" + LabSession.SceneNames[0] + ".unity");
        Debug.Log("LAB04_CHECKS_OK count=" + passed.Count);
    }
}
