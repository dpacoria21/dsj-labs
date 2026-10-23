using UnityEngine;

// El orden de este arreglo define el circuito; el ultimo punto enlaza con el primero.
public class WaypointCircuit : MonoBehaviour
{
    public Transform[] points;
    public bool showRoute = true;
    public Color routeColor = new Color(.2f, .9f, .85f, .7f);
    public int Count => points == null ? 0 : points.Length;
    public Vector3 Point(int index) => points[index % Count].position;
    public bool IsValid {
        get { if (Count < 2) return false; foreach (var p in points) if (p == null) return false; return true; }
    }
    void Start() { SetVisible(showRoute); }
    public void SetVisible(bool value)
    {
        showRoute = value;
        var line = GetComponent<LineRenderer>();
        if (line != null) line.enabled = value;
    }
    void OnDrawGizmos()
    {
        if (!IsValid) return;
        Gizmos.color = routeColor;
        for (int i = 0; i < Count; i++) {
            Gizmos.DrawWireSphere(Point(i), .65f);
            Gizmos.DrawLine(Point(i), Point((i + 1) % Count));
        }
    }
}
