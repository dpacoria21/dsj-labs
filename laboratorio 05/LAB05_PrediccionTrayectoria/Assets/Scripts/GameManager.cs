using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Adaptacion del GameManager.cs de hamza herbou (MIT): arrastre, impulso y puntos.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Ball ball;
    public Trajectory trajectory;
    [SerializeField] public float pushForce = 4f;
    public float maxDragDistance = 3f;
    public bool scoreEnabled;
    public string experimentTitle;
    public bool AutomatedVerification;
    public TextMesh worldScore;
    public TextMesh worldStatus;
    public int Score { get; private set; }
    public int Hits { get; private set; }
    public string LastEvent { get; private set; } = "Arrastra desde la bola y suelta para disparar.";
    public Vector2 CurrentImpulse { get; private set; }
    public bool IsDragging { get; private set; }
    public readonly List<string> HitHistory = new List<string>();
    readonly HashSet<string> scoredShots = new HashSet<string>();
    Camera cam;
    Vector2 startPoint;
    LineRenderer dragLine;
    GUIStyle heading, text, small, scoreStyle;

    void Awake() { Instance = this; }
    void Start()
    {
        cam = Camera.main;
        ball.DesactivateRb();
        dragLine = GetComponent<LineRenderer>();
        if (dragLine != null) dragLine.enabled = false;
        RefreshWorldHud();
    }
    void Update()
    {
        if (AutomatedVerification) return;
        if (Input.GetKeyDown(KeyCode.R)) ResetShot();
        if (Input.GetKeyDown(KeyCode.N)) { ResetShot(); ClearScore(); }
        if (Input.GetKeyDown(KeyCode.Alpha1)) SceneManager.LoadScene("01_Prediccion_Base");
        if (Input.GetKeyDown(KeyCode.Alpha2)) SceneManager.LoadScene("02_Dos_Objetivos_Puntuacion");
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string dir = Path.Combine(Application.dataPath, "../Capturas");
            Directory.CreateDirectory(dir);
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, SceneManager.GetActiveScene().name + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png"));
            LastEvent = "Captura guardada en la carpeta Capturas del proyecto.";
        }
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !ball.InFlight && ball.col.OverlapPoint(mouse)) OnDragStart(mouse);
        if (IsDragging) OnDrag(mouse);
        if (Input.GetMouseButtonUp(0) && IsDragging) OnDragEnd();
    }
    public void OnDragStart(Vector2 point)
    {
        if (ball.InFlight) return;
        ball.DesactivateRb();
        startPoint = point;
        CurrentImpulse = Vector2.zero;
        IsDragging = true;
        trajectory.Show();
        LastEvent = "Apuntando: mueve el raton en sentido contrario al lanzamiento.";
        RefreshWorldHud();
        if (dragLine != null) dragLine.enabled = true;
    }
    public void OnDrag(Vector2 point)
    {
        if (!IsDragging) return;
        Vector2 pull = Vector2.ClampMagnitude(startPoint - point, maxDragDistance);
        CurrentImpulse = pull * pushForce;
        trajectory.UpdateDots(ball.pos, CurrentImpulse);
        if (dragLine != null)
        {
            dragLine.positionCount = 2;
            dragLine.SetPosition(0, ball.pos);
            dragLine.SetPosition(1, (Vector2)ball.pos - pull);
        }
        Debug.DrawLine(startPoint, point, Color.cyan);
    }
    public void OnDragEnd()
    {
        if (!IsDragging) return;
        IsDragging = false;
        trajectory.Hide();
        if (dragLine != null) dragLine.enabled = false;
        if (CurrentImpulse.sqrMagnitude < .04f) { LastEvent = "Arrastre muy corto: vuelve a apuntar."; RefreshWorldHud(); return; }
        PushBall(CurrentImpulse);
    }
    public void PushBall(Vector2 impulse)
    {
        ball.ActivateRb();
        ball.Push(impulse);
        LastEvent = "Tiro " + ball.ShotId + ": R para recuperar la bola.";
        RefreshWorldHud();
    }
    public bool RegisterHit(Cup cup, Ball projectile)
    {
        if (!projectile.InFlight) return false;
        string key = projectile.ShotId + ":" + cup.targetId;
        if (!scoredShots.Add(key)) return false;
        Hits++;
        if (scoreEnabled) Score += cup.points;
        LastEvent = "Objetivo " + cup.targetId + " alcanzado" + (scoreEnabled ? "  +" + cup.points + " punto" : "");
        HitHistory.Add("tiro=" + projectile.ShotId + ";objetivo=" + cup.targetId + ";puntos=" + Score);
        RefreshWorldHud();
        Debug.Log("LAB05_HIT " + HitHistory[HitHistory.Count - 1]);
        return true;
    }
    public void ResetShot()
    {
        IsDragging = false;
        CurrentImpulse = Vector2.zero;
        trajectory.Hide();
        if (dragLine != null) dragLine.enabled = false;
        ball.ResetBall();
        foreach (var cup in FindObjectsByType<Cup>(FindObjectsSortMode.None)) cup.ResetCup();
        LastEvent = "Bola lista. La puntuacion se conserva; N inicia una partida.";
        RefreshWorldHud();
    }
    public void ClearScore() { Score = 0; Hits = 0; scoredShots.Clear(); HitHistory.Clear(); RefreshWorldHud(); }
    void RefreshWorldHud()
    {
        if (worldScore != null) worldScore.text = scoreEnabled ? "PUNTOS: " + Score : "ENTRADAS: " + Hits;
        if (worldStatus != null) worldStatus.text = "TIRO: " + ball.ShotId + (IsDragging ? " / APUNTANDO" : ball.InFlight ? " / EN VUELO" : " / LISTO");
    }

    void OnGUI()
    {
        float scale = Mathf.Clamp(Screen.width / 1440f, .65f, 1.5f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        float width = Screen.width / scale;
        if (heading == null)
        {
            heading = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold };
            text = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
            small = new GUIStyle(text) { fontSize = 14 };
            scoreStyle = new GUIStyle(heading) { alignment = TextAnchor.UpperRight };
        }
        GUI.Box(new Rect(0, 0, width, 118), "");
        GUI.Label(new Rect(24, 15, width - 320, 38), "LAB 05  /  " + experimentTitle, heading);
        GUI.Label(new Rect(24, 58, width - 350, 48), LastEvent, text);
        GUI.Label(new Rect(width - 300, 20, 275, 40), scoreEnabled ? "PUNTOS  " + Score : "PREDICCION 2D", scoreStyle);
        GUI.Box(new Rect(0, Screen.height / scale - 88, width, 88), "");
        GUI.Label(new Rect(24, Screen.height / scale - 79, width - 48, 30), "Arrastrar desde Ball  |  Soltar: lanzar  |  R: recuperar  |  N: nueva partida  |  1/2: escenas  |  F12: captura", text);
        GUI.Label(new Rect(24, Screen.height / scale - 47, width - 48, 40), AutomatedVerification ? "PRUEBA AUTOMATIZADA: entrada reproducible; render real de Unity." : "Puntos: prediccion hasta el primer obstaculo. Estela verde: recorrido real. Cada taza puntua una vez por tiro.", small);
        GUI.matrix = Matrix4x4.identity;
    }
}
