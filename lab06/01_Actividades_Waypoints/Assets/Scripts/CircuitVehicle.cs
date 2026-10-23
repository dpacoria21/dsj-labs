using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ProgressTracker))]
public class CircuitVehicle : MonoBehaviour
{
    [Min(0)] public float speed = 14f;
    [Min(.1f)] public float rotSpeed = 5f;
    public Color vehicleColor = Color.cyan;
    public bool ManualSimulation { get; set; }
    public int CollisionCount { get; private set; }
    public ProgressTracker Tracker { get; private set; }
    public Rigidbody Body { get; private set; }
    Vector3 spawn; Quaternion heading; bool initialized;
    void Start() { Initialize(); }
    public void Initialize()
    {
        if (initialized) return;
        initialized = true; Body = GetComponent<Rigidbody>(); Tracker = GetComponent<ProgressTracker>();
        spawn = transform.position; heading = transform.rotation; Tracker.Initialize();
    }
    void FixedUpdate() { if (!ManualSimulation) Step(Time.fixedDeltaTime); }
    public void Step(float dt)
    {
        Initialize();
        if (dt <= 0 || Tracker.circuit == null || !Tracker.circuit.IsValid) return;
        Tracker.Advance(Body.position, speed, dt);
        if (Tracker.Target == null) return;
        Vector3 delta = Tracker.Target.position - Body.position; delta.y = 0;
        if (delta.sqrMagnitude < .04f) return;
        Vector3 direction = delta.normalized;
        var rotation = Quaternion.Slerp(Body.rotation, Quaternion.LookRotation(direction), rotSpeed * dt);
        Body.MoveRotation(rotation);
        Vector3 forward = rotation * Vector3.forward;
        float alignment = Mathf.Clamp01(Vector3.Dot(forward, direction));
        float desiredSpeed = Mathf.Max(0, speed) * Mathf.Lerp(.2f, 1f, alignment) * Mathf.Clamp01(delta.magnitude / 1.8f);
        Vector3 velocity = Body.linearVelocity; velocity.y = 0;
        // Fuerza de aceleracion: conserva colisiones y desplazamientos de PhysX.
        Body.AddForce(Vector3.ClampMagnitude((forward * desiredSpeed - velocity) * 5f, 45f), ForceMode.Acceleration);
    }
    public void Nudge() { Initialize(); Body.AddForce(transform.right * 8f, ForceMode.VelocityChange); }
    public void ResetVehicle()
    {
        Initialize(); Body.position = spawn; Body.rotation = heading;
        Body.linearVelocity = Body.angularVelocity = Vector3.zero;
        Tracker.ResetTracker(); CollisionCount = 0;
    }
    void OnCollisionEnter(Collision hit) { if (hit.gameObject.GetComponent<CircuitVehicle>() != null) CollisionCount++; }
}
