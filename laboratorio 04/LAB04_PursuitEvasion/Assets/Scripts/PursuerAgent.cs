using UnityEngine;

// Predator: Seek usa la posicion actual; Pursuit extrapola con velocidad y T.
public class PursuerAgent : MonoBehaviour
{
    public enum PursuitMode { Seek, Pursuit }
    public PursuitMode mode = PursuitMode.Seek;
    public EvaderController target;
    public Transform predictionMarker;
    [Min(0)] public float maxSpeed = 4.6f;
    [Min(0)] public float maxAcceleration = 9f;
    [Min(0)] public float predictionFactor = 0.12f;
    [Min(0)] public float maxPrediction = 1.5f;
    [Min(0)] public float arenaLimit = 13f;
    public Vector3 Velocity { get; private set; }
    public Vector3 TargetPoint { get; private set; }
    public float PredictionTime { get; private set; }

    void Update() { Step(Time.deltaTime); }

    public void Step(float deltaTime)
    {
        if (target == null) return;
        TargetPoint = target.transform.position;
        PredictionTime = 0f;
        if (mode == PursuitMode.Pursuit)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            PredictionTime = Mathf.Min(maxPrediction, distance * predictionFactor);
            TargetPoint += target.Velocity * PredictionTime;
        }
        if (predictionMarker != null)
            predictionMarker.position = new Vector3(TargetPoint.x, 0.15f, TargetPoint.z);
        Debug.DrawLine(target.transform.position, TargetPoint, Color.cyan);
        Debug.DrawLine(transform.position, TargetPoint, Color.yellow);
        Vector3 toTarget = TargetPoint - transform.position;
        if (toTarget.sqrMagnitude < 0.001f) return;
        Vector3 desiredVelocity = toTarget.normalized * maxSpeed;
        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - Velocity, maxAcceleration);
        Velocity = Vector3.ClampMagnitude(Velocity + steering * deltaTime, maxSpeed);
        transform.position += Velocity * deltaTime;
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, -arenaLimit, arenaLimit);
        p.z = Mathf.Clamp(p.z, -arenaLimit, arenaLimit);
        transform.position = p;
        if (Velocity.sqrMagnitude > 0.01f) transform.forward = Velocity.normalized;
    }

    public void ResetMotion() { Velocity = Vector3.zero; }
}
