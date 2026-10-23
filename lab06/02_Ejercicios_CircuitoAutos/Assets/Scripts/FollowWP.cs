using UnityEngine;

// Actividades 7-15: version comentada del algoritmo de Unity Learn.
public class FollowWP : MonoBehaviour
{
    public GameObject[] waypoints;
    [Min(0)] public float speed = 10f;
    [Min(0)] public float rotSpeed = 10f;
    [Min(.1f)] public float accuracy = 3f;
    public int startWaypoint;
    public Color vehicleColor = Color.green;
    public int CurrentWP { get; private set; }
    public int PassedWaypoints { get; private set; }
    public int Laps { get; private set; }
    public bool ManualSimulation { get; set; }
    Vector3 initialPosition;
    Quaternion initialRotation;
    bool initialized;

    void Start() { Initialize(); }
    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        initialPosition = transform.position; initialRotation = transform.rotation;
        CurrentWP = waypoints != null && waypoints.Length > 0 ? Mathf.Clamp(startWaypoint, 0, waypoints.Length - 1) : 0;
    }
    void Update() { if (!ManualSimulation) Step(Time.deltaTime); }
    public void Step(float dt)
    {
        Initialize();
        if (dt <= 0 || waypoints == null || waypoints.Length < 2 || waypoints[CurrentWP] == null) return;
        Vector3 goal = waypoints[CurrentWP].transform.position; goal.y = transform.position.y;
        if (Vector3.Distance(transform.position, goal) < accuracy) {
            CurrentWP = (CurrentWP + 1) % waypoints.Length;
            PassedWaypoints++;
            if (PassedWaypoints % waypoints.Length == 0) Laps++;
            if (waypoints[CurrentWP] == null) return;
            goal = waypoints[CurrentWP].transform.position; goal.y = transform.position.y;
        }
        Vector3 direction = goal - transform.position;
        if (direction.sqrMagnitude < .0001f) return;
        var lookAtWP = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAtWP, Mathf.Max(0, rotSpeed) * dt);
        transform.Translate(0, 0, Mathf.Max(0, speed) * dt);
    }
    public void ResetVehicle()
    {
        Initialize(); transform.SetPositionAndRotation(initialPosition, initialRotation);
        CurrentWP = waypoints != null && waypoints.Length > 0 ? Mathf.Clamp(startWaypoint, 0, waypoints.Length - 1) : 0;
        PassedWaypoints = Laps = 0;
    }
}
