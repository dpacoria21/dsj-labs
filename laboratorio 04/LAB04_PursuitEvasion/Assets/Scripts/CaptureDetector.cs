using UnityEngine;

// Ejercicio V.2: distancia estrictamente menor que 0.8, un solo log por ejecucion.
public class CaptureDetector : MonoBehaviour
{
    public PursuerAgent predator;
    public EvaderController runner;
    public bool HasCaptured { get; private set; }
    public float CapturedAt { get; private set; }
    public static bool IsCaptureDistance(float distance) { return distance < 0.8f; }

    void LateUpdate() { CheckCapture(); }

    public void CheckCapture()
    {
        if (HasCaptured || predator == null || runner == null) return;
        if (IsCaptureDistance(Vector3.Distance(predator.transform.position, runner.transform.position)))
        {
            HasCaptured = true;
            CapturedAt = Time.timeSinceLevelLoad;
            Debug.Log("Runner captured");
        }
    }
}
