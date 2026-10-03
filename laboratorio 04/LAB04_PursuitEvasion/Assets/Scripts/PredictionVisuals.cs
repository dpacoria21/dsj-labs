using UnityEngine;

// Lineas visibles tambien en Game/build; complementan Debug.DrawLine de la guia.
public class PredictionVisuals : MonoBehaviour
{
    public PursuerAgent predator;
    public EvaderController runner;
    LineRenderer pursuit, direction, evasion;

    void Awake()
    {
        pursuit = MakeLine("Prediccion del Runner", Color.cyan);
        direction = MakeLine("Objetivo del Predator", Color.yellow);
        evasion = MakeLine("Prediccion del Predator", Color.magenta);
    }
    LineRenderer MakeLine(string label, Color color)
    {
        var obj = new GameObject(label);
        obj.transform.SetParent(transform);
        var line = obj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = 0.045f;
        line.positionCount = 2;
        return line;
    }
    void LateUpdate()
    {
        Draw(pursuit, runner.transform.position, predator.TargetPoint, true);
        Draw(direction, predator.transform.position, predator.TargetPoint, true);
        Draw(evasion, predator.transform.position, runner.PredictedPursuer,
            runner.mode == EvaderController.EvaderMode.Evasion);
    }
    void Draw(LineRenderer line, Vector3 a, Vector3 b, bool visible)
    {
        line.enabled = visible;
        a.y = b.y = 0.12f;
        line.SetPosition(0, a);
        line.SetPosition(1, b);
    }
    void OnDestroy()
    {
        foreach (var line in new[] { pursuit, direction, evasion })
            if (line != null) Destroy(line.material);
    }
}
