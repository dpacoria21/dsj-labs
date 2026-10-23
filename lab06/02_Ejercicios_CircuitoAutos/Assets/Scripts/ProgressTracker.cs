using UnityEngine;

// Objetivo auxiliar SIN collider. Avanza exactamente por el circuito y espera al vehiculo.
public class ProgressTracker : MonoBehaviour
{
    public WaypointCircuit circuit;
    [Min(1)] public float lookAhead = 6f;
    public bool visible;
    public int startWaypoint;
    public Transform Target { get; private set; }
    public int CurrentWP { get; private set; }
    public int PassedWaypoints { get; private set; }
    public int Laps => circuit != null && circuit.Count > 0 ? PassedWaypoints / circuit.Count : 0;
    public int WaitSteps { get; private set; }
    Renderer marker;

    public void Initialize()
    {
        if (Target != null || circuit == null || !circuit.IsValid) return;
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "Tracker_" + name;
        var collider = obj.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
        Target = obj.transform; Target.localScale = new Vector3(.65f, .12f, .65f);
        marker = obj.GetComponent<Renderer>();
        var m = new Material(Shader.Find("Standard")); m.color = new Color(1, .68f, .12f);
        marker.material = m; marker.enabled = visible;
        ResetTracker();
    }
    public void ResetTracker()
    {
        if (Target == null) return;
        CurrentWP = Mathf.Clamp(startWaypoint, 0, circuit.Count - 1);
        Target.position = circuit.Point((CurrentWP + circuit.Count - 1) % circuit.Count);
        PassedWaypoints = WaitSteps = 0;
    }
    public void Advance(Vector3 vehiclePosition, float vehicleSpeed, float dt)
    {
        Initialize();
        if (Target == null || dt <= 0 || !circuit.IsValid) return;
        Vector3 here = Target.position; vehiclePosition.y = here.y;
        float gap = Vector3.Distance(here, vehiclePosition);
        // Esta cota tambien limita el paso final: no se adelanta indefinidamente.
        float budget = Mathf.Min((Mathf.Max(0, vehicleSpeed) + 2f) * dt, Mathf.Max(0, lookAhead - gap));
        if (budget < .001f) { WaitSteps++; return; }
        for (int guard = 0; guard < circuit.Count && budget > 0; guard++) {
            Vector3 goal = circuit.Point(CurrentWP);
            float distance = Vector3.Distance(here, goal);
            if (distance <= budget + .0001f) {
                here = goal; budget = Mathf.Max(0, budget - distance);
                CurrentWP = (CurrentWP + 1) % circuit.Count; PassedWaypoints++;
            } else { here = Vector3.MoveTowards(here, goal, budget); budget = 0; }
        }
        Target.position = here;
    }
    public void SetVisible(bool value) { visible = value; if (marker != null) marker.enabled = value; }
    void OnDestroy() { if (marker != null) Destroy(marker.sharedMaterial); if (Target != null) Destroy(Target.gameObject); }
}
