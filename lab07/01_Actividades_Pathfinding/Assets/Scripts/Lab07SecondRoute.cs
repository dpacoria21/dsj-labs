using UnityEngine;
using Pathfinding;

// Each Seeker owns its path. This draws the second agent's independent route.
public class Lab07SecondRoute : MonoBehaviour
{
    public AIPath agent;
    public LineRenderer route;
    public Lab07Dashboard dashboard;
    Vector3[] points = new Vector3[0];
    void OnEnable() { agent.GetComponent<Seeker>().pathCallback += OnPath; }
    void OnDisable() { if(agent) agent.GetComponent<Seeker>().pathCallback -= OnPath; }
    void OnPath(Path p) { points = p.error ? new Vector3[0] : p.vectorPath.ToArray(); }
    void Update()
    {
        route.enabled = dashboard.showRoute;
        route.positionCount = points.Length;
        for(int i=0;i<points.Length;i++) route.SetPosition(i,points[i]+Vector3.back*.2f);
    }
}
