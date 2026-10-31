using UnityEngine;
using Pathfinding;

public class Lab07Dashboard : MonoBehaviour
{
    public PlayerScript player;
    public AIPath enemy;
    public LineRenderer route;
    public string activityTitle = "ACTIVIDADES 1–17";
    public AIPath secondEnemy;
    public bool showRoute = true;
    Vector3[] points = new Vector3[0];
    bool paused;
    public int pathCount;
    void OnEnable() { enemy.GetComponent<Seeker>().pathCallback += OnPath; }
    void OnDisable() { if (enemy) enemy.GetComponent<Seeker>().pathCallback -= OnPath; Time.timeScale = 1; }
    void OnPath(Path p)
    {
        if (p.error) { points = new Vector3[0]; return; }
        pathCount++;
        points = p.vectorPath.ToArray();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetPositions();
        if (Input.GetKeyDown(KeyCode.V)) showRoute = !showRoute;
        if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
        route.enabled = showRoute;
        route.positionCount = points.Length;
        for (int i=0; i<points.Length; i++) route.SetPosition(i, points[i] + Vector3.back*.15f);
    }
    public void ResetPositions()
    {
        player.GetComponent<Rigidbody2D>().position = new Vector2(2,-1);
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        enemy.Teleport(new Vector3(-9,4,0)); enemy.SearchPath();
        if(secondEnemy) { secondEnemy.Teleport(new Vector3(9,4,0)); secondEnemy.SearchPath(); }
    }
    void TogglePause() { paused = !paused; Time.timeScale = paused ? 0 : 1; }
    void OnGUI()
    {
        float scale = Screen.height / 900f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale,scale,1));
        float width = Screen.width / scale * .22f;
        GUI.Box(new Rect(0,0,width,900), "");
        var title = new GUIStyle(GUI.skin.label) { fontSize=25, fontStyle=FontStyle.Bold, wordWrap=true };
        var text = new GUIStyle(GUI.skin.label) { fontSize=16, wordWrap=true };
        GUILayout.BeginArea(new Rect(24,35,width-48,820));
        GUILayout.Label("LABORATORIO 07", text);
        GUILayout.Space(15); GUILayout.Label("Pathfinding\nA* en 2D", title);
        GUILayout.Space(24); GUILayout.Label(activityTitle,text);
        GUILayout.Space(16); GUILayout.Label(secondEnemy ? "Jugador azul. Los enemigos naranja y violeta buscan rutas hasta Player alrededor de las paredes." : "Tú controlas al jugador azul. El enemigo naranja calcula una ruta para alcanzarte sin atravesar las paredes.",text);
        GUILayout.Space(20); GUILayout.Label("Flechas / WASD  ·  Mover\nV  ·  Mostrar ruta\nR  ·  Reiniciar\nEspacio  ·  Pausar",text);
        GUILayout.Space(24);
        showRoute = GUILayout.Toggle(showRoute," Mostrar ruta A*");
        if(GUILayout.Button("Reiniciar posiciones",GUILayout.Height(36))) ResetPositions();
        if(GUILayout.Button(paused?"Continuar":"Pausar",GUILayout.Height(36))) TogglePause();
        GUILayout.Space(25); GUILayout.Label("Destino: Player\nAlgoritmo: A*\nGrafo: Grid Graph 2D\nObstáculos: Environment",text);
        GUILayout.Space(18); GUILayout.Label("Distancia: " + Vector2.Distance(player.transform.position,enemy.position).ToString("F1") + "\nRutas calculadas: " + pathCount,text);
        if(secondEnemy) GUILayout.Label("Enemigo 2: " + Vector2.Distance(player.transform.position,secondEnemy.position).ToString("F1") + " unidades",text);
        GUILayout.EndArea();
    }
}
