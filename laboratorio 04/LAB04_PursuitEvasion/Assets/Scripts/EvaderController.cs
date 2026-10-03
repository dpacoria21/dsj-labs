using UnityEngine;

// Runner: control WASD o huida de la posicion futura del Predator.
public class EvaderController : MonoBehaviour
{
    public enum EvaderMode { Manual, Evasion }
    public EvaderMode mode = EvaderMode.Manual;
    public PursuerAgent pursuer;
    public Transform predictionMarker;
    [Min(0)] public float maxSpeed = 5f;
    [Min(0)] public float maxAcceleration = 10f;
    [Min(0)] public float predictionFactor = 0.12f;
    [Min(0)] public float maxPrediction = 1.5f;
    [Min(0)] public float arenaLimit = 13f;
    public Vector3 Velocity { get; private set; }
    public Vector3 PredictedPursuer { get; private set; }
    public float PredictionTime { get; private set; }

    // Solo la prueba automatizada activa esta entrada. El uso normal lee WASD.
    public bool UseTestInput { get; set; }
    public Vector3 TestInput { get; set; }

    void Update() { Step(Time.deltaTime, UseTestInput ? TestInput : ReadManualInput()); }

    public void Step(float deltaTime, Vector3 manualInput)
    {
        Vector3 desiredVelocity = mode == EvaderMode.Manual
            ? manualInput.normalized * maxSpeed : CalculateEvasion();
        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - Velocity, maxAcceleration);
        Velocity = Vector3.ClampMagnitude(Velocity + steering * deltaTime, maxSpeed);
        transform.position += Velocity * deltaTime;
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, -arenaLimit, arenaLimit);
        p.z = Mathf.Clamp(p.z, -arenaLimit, arenaLimit);
        transform.position = p;
        if (Velocity.sqrMagnitude > 0.01f) transform.forward = Velocity.normalized;
        if (predictionMarker != null) predictionMarker.gameObject.SetActive(mode == EvaderMode.Evasion);
    }

    Vector3 ReadManualInput()
    {
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input.z += 1f;
        if (Input.GetKey(KeyCode.S)) input.z -= 1f;
        if (Input.GetKey(KeyCode.D)) input.x += 1f;
        if (Input.GetKey(KeyCode.A)) input.x -= 1f;
        return input.normalized;
    }

    Vector3 CalculateEvasion()
    {
        if (pursuer == null) return Vector3.zero;
        float distance = Vector3.Distance(transform.position, pursuer.transform.position);
        PredictionTime = Mathf.Min(maxPrediction, distance * predictionFactor);
        PredictedPursuer = pursuer.transform.position + pursuer.Velocity * PredictionTime;
        if (predictionMarker != null)
            predictionMarker.position = new Vector3(PredictedPursuer.x, 0.15f, PredictedPursuer.z);
        Debug.DrawLine(pursuer.transform.position, PredictedPursuer, Color.magenta);
        Vector3 away = transform.position - PredictedPursuer;
        return away.sqrMagnitude < 0.001f ? Vector3.zero : away.normalized * maxSpeed;
    }

    public void ResetMotion() { Velocity = Vector3.zero; }
}
