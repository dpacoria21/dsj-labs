using UnityEngine;
using UnityEngine.SceneManagement;

// Presentación y visualización añadidas para el laboratorio, sin sustituir la física.
public class TutorialPresentation : MonoBehaviour
{
    public FireShell tank;
    public LineRenderer trajectory;
    GUIStyle title, label;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        trajectory.enabled = tank.HasSolution;
        if (!tank.HasSolution) return;
        Vector3 origin = tank.turret.transform.position;
        Vector3 delta = tank.TargetPosition - origin;
        float horizontalSpeed = new Vector2(tank.LaunchVelocity.x, tank.LaunchVelocity.z).magnitude;
        float flightTime = new Vector2(delta.x, delta.z).magnitude / horizontalSpeed;
        trajectory.positionCount = 81;
        for (int i = 0; i < trajectory.positionCount; i++)
        {
            float t = flightTime * i / (trajectory.positionCount - 1);
            trajectory.SetPosition(i, origin + tank.LaunchVelocity * t + 0.5f * Physics.gravity * t * t);
        }
    }

    void OnGUI()
    {
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
            label = new GUIStyle(GUI.skin.label) { fontSize = 17 };
            title.normal.textColor = Color.white;
            label.normal.textColor = Color.white;
        }
        GUI.color = new Color(0.04f, 0.10f, 0.17f, 0.93f);
        GUI.DrawTexture(new Rect(12, 12, 640, 170), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(28, 22, 630, 32), "LAB 05 · Calculating Trajectories", title);
        GUI.Label(new Rect(28, 59, 620, 27), "Tanque IA · tiro parabólico con Rigidbody 3D", label);
        GUI.Label(new Rect(28, 87, 620, 27), tank.HasSolution ?
            $"Arco {(tank.lowArc ? "bajo" : "alto")} · ángulo {tank.Elevation:F1}° · rapidez {tank.speed:F1} m/s" :
            "Objetivo fuera de alcance: el tanque se aproxima", label);
        GUI.Label(new Rect(28, 115, 620, 27), $"Disparos: {tank.Shots}   Impactos en objetivo: {tank.TargetHits}", label);
        GUI.Label(new Rect(28, 143, 620, 27), "WASD / flechas: objetivo · L: arco · R: reiniciar", label);
    }
}
