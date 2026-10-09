using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lab05Verification
{
    static int checks;
    static void Check(bool ok, string description) { checks++; if (!ok) throw new Exception("LAB05: " + description); }
    [MenuItem("Laboratorio 05/Verificar configuracion y ecuaciones")]
    public static void Run()
    {
        checks = 0;
        foreach (string name in Lab05ProjectBuilder.SceneNames)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            Check(gm != null && gm.ball != null && gm.trajectory != null, name + " referencias");
            var rb = gm.ball.GetComponent<Rigidbody2D>();
            Check(Mathf.Approximately(rb.mass, 1) && rb.gravityScale == 1 && rb.linearDamping == 0, "Ball masa/gravedad/damping");
            Check(rb.sharedMaterial != null && Mathf.Approximately(rb.sharedMaterial.friction, .5f) && Mathf.Approximately(rb.sharedMaterial.bounciness, .6f), "material .5/.6");
            Check(gm.ball.GetComponent<CircleCollider2D>() != null, "CircleCollider2D");
            Check(UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None).Length >= 3, "walls collider");
            foreach (var collider in UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
            {
                if (collider.isTrigger) continue;
                Check(Vector2.Distance(collider.size, collider.GetComponent<SpriteRenderer>().size) < .0001f, "wall visible coincide collider");
            }
            var cups = UnityEngine.Object.FindObjectsByType<Cup>(FindObjectsSortMode.None);
            Check(cups.Length == (gm.scoreEnabled ? 2 : 1), "numero de objetivos");
            foreach (var cup in cups)
            {
                var body = cup.GetComponent<Rigidbody2D>();
                Check(body.bodyType == RigidbodyType2D.Dynamic && body.mass == 1.5f, "Cup Dynamic masa1.5");
                Check(cup.GetComponent<EdgeCollider2D>() != null && cup.sensor != null && cup.sensor.cup == cup, "Cup Edge/sensor");
            }
            Check(gm.trajectory.dotPrefab != null && gm.trajectory.dotsParent != null, "dots referencias");
        }
        Check(Vector2.Distance(Trajectory.PositionAtTime(Vector2.zero, new Vector2(3, 5), new Vector2(0, -10), 1), new Vector2(3, 0)) < 1e-5f, "ecuacion continua");
        Check(Vector2.Distance(Trajectory.PhysicsPositionAtTime(Vector2.zero, new Vector2(3, 5), new Vector2(0, -10), 1, .02f), new Vector2(3, -.1f)) < 1e-5f, "correccion integrador");
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/resultado-editor.txt", "Unity " + Application.unityVersion + "\nComprobaciones: " + checks + "\nErrores: 0\n");
        Debug.Log("LAB05_EDITOR_OK checks=" + checks);
        EditorSceneManager.OpenScene("Assets/Scenes/" + Lab05ProjectBuilder.SceneNames[0] + ".unity");
    }
}
