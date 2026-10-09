using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Solo se activa con --lab05-verify. Ejecuta Box2D real, no sustituye posiciones del tiro.
public class Lab05RuntimeVerification : MonoBehaviour
{
    static Lab05RuntimeVerification instance;
    readonly List<string> errors = new List<string>();
    readonly List<string> rows = new List<string>();
    readonly List<string> events = new List<string>();
    string output;
    GameManager gm;
    int checks;
    const float Step = .02f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--lab05-verify") < 0 || instance != null) return;
        instance = new GameObject("LAB05 prueba automatizada").AddComponent<Lab05RuntimeVerification>();
        DontDestroyOnLoad(instance.gameObject);
    }
    void Check(bool condition, string name) { checks++; if (!condition) errors.Add(name); }
    void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message);
        if (message.StartsWith("LAB05_HIT")) events.Add(message);
    }
    IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        int ix = Array.IndexOf(args, "--lab-output");
        output = ix >= 0 && ix + 1 < args.Length ? Path.GetFullPath(args[ix + 1]) : Path.Combine(Application.persistentDataPath, "Lab05Verification");
        Directory.CreateDirectory(output);
        Application.logMessageReceived += OnLog;
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        Physics2D.simulationMode = SimulationMode2D.Script;
        rows.Add("prueba,masa,gravedad_scale,segundos,error_prediccion_unidades,error_formula_continua_unidades,colisiones,puntos");
        yield return Load("01_Prediccion_Base");
        yield return Capture("01_Escena_base.png");
        foreach (float mass in new[] { 1f, 2f })
        {
            foreach (float gravity in new[] { 1f, .6f })
            {
                gm.ResetShot();
                gm.ball.rb.mass = mass;
                gm.ball.rb.gravityScale = gravity;
                Vector2 origin = gm.ball.rb.position;
                Vector2 impulse = new Vector2(3, 5);
                gm.PushBall(impulse);
                float maximum = 0, analytic = 0;
                for (int tick = 1; tick <= 40; tick++)
                {
                    Physics2D.Simulate(Step);
                    float t = tick * Step;
                    Vector2 acceleration = Physics2D.gravity * gravity;
                    Vector2 predicted = Trajectory.PhysicsPositionAtTime(origin, impulse / mass, acceleration, t, Step);
                    maximum = Mathf.Max(maximum, Vector2.Distance(predicted, gm.ball.rb.position));
                    analytic = Mathf.Max(analytic, Vector2.Distance(Trajectory.PositionAtTime(origin, impulse / mass, acceleration, t), gm.ball.rb.position));
                }
                Check(gm.ball.CollisionCount == 0, "vuelo libre sin colisiones masa=" + mass + " g=" + gravity);
                Check(maximum < .005f, "prediccion fisica error menor .005 masa=" + mass + " g=" + gravity);
                Check(Mathf.Abs(gm.ball.rb.linearVelocity.x - impulse.x / mass) < .0001f, "impulso/masa");
                rows.Add(string.Format(CultureInfo.InvariantCulture, "vuelo_libre,{0},{1},0.8,{2:F7},{3:F7},{4},{5}", mass, gravity, maximum, analytic, gm.ball.CollisionCount, gm.Score));
            }
        }
        gm.ball.rb.mass = 1;
        gm.ball.rb.gravityScale = 1;
        gm.ResetShot();
        yield return Shoot("A", "02_Prediccion_base.png", "03_Entrada_objetivo_base.png");
        Check(gm.Hits == 1 && gm.Score == 0, "base registra entrada sin puntuacion");
        yield return Load("02_Dos_Objetivos_Puntuacion");
        yield return Capture("04_Dos_objetivos.png");
        yield return Shoot("A", "05_Prediccion_objetivo_A.png", "06_Punto_objetivo_A.png");
        Check(gm.Score == 1, "objetivo A puntua 1");
        var cupA = FindCup("A");
        Check(!cupA.Hit(gm.ball) && gm.Score == 1, "duplicado mismo objetivo/tiro no puntua");
        gm.ResetShot();
        Settle();
        Check(gm.Score == 1, "R conserva score");
        yield return Shoot("A", null, "07_Segundo_tiro_A.png");
        Check(gm.Score == 2, "nuevo tiro mismo objetivo suma otra vez");
        gm.ResetShot();
        Settle();
        yield return Shoot("B", "08_Prediccion_objetivo_B.png", "09_Punto_objetivo_B.png");
        Check(gm.Score == 3, "objetivo B suma tercer punto");
        Check(gm.Hits == 3, "tres impactos validos");
        gm.ResetShot();
        gm.ball.rb.position = new Vector2(-3.8f, -1);
        gm.PushBall(new Vector2(0, -2));
        bool bounced = false;
        float ratio = 0;
        for (int tick = 0; tick < 80; tick++)
        {
            float incoming = gm.ball.rb.linearVelocity.y;
            Physics2D.Simulate(Step);
            if (incoming < 0 && gm.ball.rb.linearVelocity.y > 0)
            {
                ratio = gm.ball.rb.linearVelocity.y / -incoming;
                bounced = true;
                break;
            }
        }
        Check(bounced && ratio > .55f && ratio < .65f, "rebote real material .6 ratio=" + ratio);
        Check(gm.Score == 3, "fallo/rebote suelo no suma");
        rows.Add(string.Format(CultureInfo.InvariantCulture, "rebote_suelo,1,1,0,0,0,{0},{1}", gm.ball.CollisionCount, gm.Score));
        gm.ResetShot();
        gm.ClearScore();
        Check(gm.Score == 0 && gm.Hits == 0, "N nueva partida borra score");
        File.WriteAllLines(Path.Combine(output, "resultados-runtime-2d.csv"), rows);
        File.WriteAllLines(Path.Combine(output, "eventos-impactos-2d.txt"), events);
        File.WriteAllText(Path.Combine(output, "resultado-runtime-2d.txt"),
            "Unity " + Application.unityVersion + "\nUTC " + DateTime.UtcNow.ToString("O") +
            "\nBox2D: Physics2D.Simulate(dt=0.02), cuerpos reales, entrada sintetica de arrastre.\n" +
            "Capturas: render offscreen real de Camera con marcador de estado/puntos en la escena.\nNo incluyen interfaz IMGUI, Inspector ni Console; no es una sesion humana de arrastre.\n" +
            "Pruebas: 4 vuelos libres (0.8 s); entrada A base; A/A/B extension; duplicado mismo tiro; R; N; rebote suelo.\n" +
            "Rebote medido: " + ratio.ToString("F5", CultureInfo.InvariantCulture) + "\nComprobaciones: " + checks + "\nErrores: " + errors.Count + "\n" + string.Join("\n", errors));
        Application.logMessageReceived -= OnLog;
        Debug.Log("LAB05_RUNTIME_FINISHED checks=" + checks + " errors=" + errors.Count);
        Application.Quit(errors.Count == 0 ? 0 : 2);
    }
    IEnumerator Load(string scene)
    {
        yield return SceneManager.LoadSceneAsync(scene);
        yield return null;
        gm = FindFirstObjectByType<GameManager>();
        gm.AutomatedVerification = true;
        gm.ball.rb.interpolation = RigidbodyInterpolation2D.None;
        foreach (var cup in FindObjectsByType<Cup>(FindObjectsSortMode.None)) cup.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        Settle();
    }
    void Settle() { for (int tick = 0; tick < 100; tick++) Physics2D.Simulate(Step); }
    Cup FindCup(string id)
    {
        foreach (var cup in FindObjectsByType<Cup>(FindObjectsSortMode.None)) if (cup.targetId == id) return cup;
        throw new Exception("Objetivo no encontrado: " + id);
    }
    IEnumerator Shoot(string targetId, string predictionImage, string impactImage)
    {
        var cup = FindCup(targetId);
        Vector2 origin = gm.ball.rb.position;
        float time = targetId == "A" ? 1.2f : 1.55f;
        Vector2 goal = cup.sensor.transform.position;
        Vector2 velocity = (goal - origin) / time - .5f * Physics2D.gravity * (time + Step);
        Vector2 impulse = velocity * gm.ball.rb.mass;
        gm.OnDragStart(origin);
        gm.OnDrag(origin - impulse / gm.pushForce);
        Check(Vector2.Distance(gm.CurrentImpulse, impulse) < .0001f, "arrastre reproduce impulso objetivo " + targetId);
        Check(gm.trajectory.VisibleDots > 5 && gm.trajectory.VisibleDots < gm.trajectory.dotsNumber, "preview corta primera colision " + targetId);
        if (predictionImage != null) yield return Capture(predictionImage);
        int hitsBefore = gm.Hits;
        gm.OnDragEnd();
        bool captured = false;
        for (int tick = 0; tick < 140; tick++)
        {
            Physics2D.Simulate(Step);
            if (!captured && gm.Hits > hitsBefore)
            {
                yield return Capture(impactImage);
                captured = true;
            }
            // Un frame por dos pasos conserva la estela del movimiento en el render.
            if (tick % 2 == 0) yield return null;
        }
        Check(captured, "entrada fisica sensor " + targetId);
        rows.Add(string.Format(CultureInfo.InvariantCulture, "impacto_{0},1,1,2.8,0,0,{1},{2}", targetId, gm.ball.CollisionCount, gm.Score));
    }
    IEnumerator Capture(string filename)
    {
        yield return new WaitForEndOfFrame();
        // Una ventana oculta no ofrece framebuffer de ScreenCapture. Se renderiza
        // directamente la camara de Unity, incluidos sus textos de estado reales.
        Camera camera = Camera.main;
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        Rect previousRect = camera.rect;
        var target = new RenderTexture(1440, 1080, 24);
        camera.targetTexture = target;
        camera.rect = new Rect(0, 0, 1, 1);
        camera.Render();
        RenderTexture.active = target;
        var texture = new Texture2D(1440, 1080, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1440, 1080), 0, 0);
        texture.Apply();
        File.WriteAllBytes(Path.Combine(output, filename), texture.EncodeToPNG());
        camera.targetTexture = previousTarget;
        camera.rect = previousRect;
        RenderTexture.active = previousActive;
        target.Release();
        Destroy(target);
        Destroy(texture);
    }
}
