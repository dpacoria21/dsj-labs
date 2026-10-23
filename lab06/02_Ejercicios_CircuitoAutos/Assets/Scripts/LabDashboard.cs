using UnityEngine;
using UnityEngine.SceneManagement;

// Panel de experimentacion. Los mismos parametros estan disponibles en el Inspector.
public class LabDashboard : MonoBehaviour
{
    public bool activities;
    public string subtitle;
    public WaypointCircuit circuit;
    public FollowWP[] tanks;
    public CircuitVehicle[] vehicles;
    public LabCamera view;
    public int selected;
    bool paused;
    GUIStyle title, label, small, button;
    Texture2D panel;
    public bool AutomatedVerification { get; set; }
    int Count => activities ? tanks.Length : vehicles.Length;
    void Start() { Select(0); }
    void Update()
    {
        if (AutomatedVerification) return;
        if (Input.GetKeyDown(KeyCode.Space)) Pause();
        if (Input.GetKeyDown(KeyCode.R)) ResetAll();
        if (Input.GetKeyDown(KeyCode.C)) view.Toggle();
        if (Input.GetKeyDown(KeyCode.Tab)) Select((selected + 1) % Count);
        if (Input.GetKeyDown(KeyCode.V)) Visuals();
        if (!activities && Input.GetKeyDown(KeyCode.B)) vehicles[selected].Nudge();
    }
    void Select(int i) { selected = i; if (view != null && Count > 0) view.target = activities ? tanks[i].transform : vehicles[i].transform; }
    void Pause() { paused = !paused; Time.timeScale = paused ? 0 : 1; }
    public void ResetAll()
    {
        if (activities) foreach (var t in tanks) t.ResetVehicle();
        else foreach (var v in vehicles) v.ResetVehicle();
        Physics.SyncTransforms();
    }
    void Visuals()
    {
        circuit.SetVisible(!circuit.showRoute);
        if (!activities) foreach (var v in vehicles) v.GetComponent<ProgressTracker>().SetVisible(circuit.showRoute);
    }
    void Styles()
    {
        if (panel != null) return;
        panel = new Texture2D(1,1); panel.SetPixel(0,0,new Color(.035f,.06f,.09f,.95f)); panel.Apply();
        title = new GUIStyle(GUI.skin.label) { fontSize=24, fontStyle=FontStyle.Bold, normal={textColor=Color.white} };
        label = new GUIStyle(GUI.skin.label) { fontSize=15, normal={textColor=new Color(.86f,.92f,.96f)} };
        small = new GUIStyle(label) { fontSize=12, wordWrap=true };
        button = new GUIStyle(GUI.skin.button) { fontSize=13, fixedHeight=28 };
    }
    void OnGUI()
    {
        Styles();
        float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        GUI.DrawTexture(new Rect(22,22,355,590),panel);
        GUILayout.BeginArea(new Rect(42,38,315,555));
        GUILayout.Label("LAB 06  /  WAYPOINTS",small);
        GUILayout.Label(activities ? "Desert patrol" : "Circuit lab",title);
        GUILayout.Label(subtitle,small); GUILayout.Space(15);
        for (int i=0;i<Count;i++) {
            var color = activities ? tanks[i].vehicleColor : vehicles[i].vehicleColor;
            GUI.color=color;
            string name = activities ? tanks[i].name.Replace("Tank", "") : vehicles[i].name;
            if (GUILayout.Button((selected==i ? "●  " : "○  ")+name,button)) Select(i);
            GUI.color=Color.white;
        }
        GUILayout.Space(12);
        float speed = activities ? tanks[selected].speed : vehicles[selected].speed;
        GUILayout.Label("Velocidad   " + speed.ToString("F1") + " u/s",label);
        speed = GUILayout.HorizontalSlider(speed, 0, activities ? 30 : 25);
        if (activities) tanks[selected].speed=speed; else vehicles[selected].speed=speed;
        float rot = activities ? tanks[selected].rotSpeed : vehicles[selected].rotSpeed;
        GUILayout.Label("Giro   " + rot.ToString("F1"),label);
        rot = GUILayout.HorizontalSlider(rot,.5f,15);
        if (activities) tanks[selected].rotSpeed=rot; else vehicles[selected].rotSpeed=rot;
        if (activities) {
            GUILayout.Label("Umbral   " + tanks[selected].accuracy.ToString("F1") + " u",label);
            tanks[selected].accuracy=GUILayout.HorizontalSlider(tanks[selected].accuracy,.5f,10);
            GUILayout.Label("Waypoint " + (tanks[selected].CurrentWP+1) + " / " + circuit.Count + "    Vueltas: " + tanks[selected].Laps,small);
        } else {
            var t=vehicles[selected].GetComponent<ProgressTracker>();
            GUILayout.Label("Anticipación   " + t.lookAhead.ToString("F1") + " u",label);
            t.lookAhead=GUILayout.HorizontalSlider(t.lookAhead,3,12);
            GUILayout.Label("Vueltas del tracker: " + t.Laps + "   Contactos: " + vehicles[selected].CollisionCount,small);
            if (GUILayout.Button("B  ·  Aplicar impulso lateral",button)) vehicles[selected].Nudge();
        }
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(paused?"Continuar":"Pausar",button)) Pause();
        if (GUILayout.Button("Reiniciar",button)) ResetAll();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cámara [C]",button)) view.Toggle();
        if (GUILayout.Button("Ruta [V]",button)) Visuals();
        GUILayout.EndHorizontal();
        if (!activities && SceneManager.sceneCountInBuildSettings>1 && GUILayout.Button("Cambiar: tutorial / autos",button)) {
            Time.timeScale=1; SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex+1)%SceneManager.sceneCountInBuildSettings);
        }
        GUILayout.EndArea();
        GUI.DrawTexture(new Rect(22,840,1396,38),panel);
        GUI.Label(new Rect(40,849,1360,24),"ESPACIO  pausa     R  reiniciar     TAB  vehículo     C  cámara     V  ruta" + (activities ? "     Ajusta velocidad y giro para comparar trayectorias." : "     B  impulso lateral para probar recuperación"),small);
    }
    void OnDestroy() { if(panel!=null) Destroy(panel); Time.timeScale=1; }
}
