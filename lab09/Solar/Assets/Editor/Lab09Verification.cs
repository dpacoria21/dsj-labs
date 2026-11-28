using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Verificación automática en Play Mode:
// Unity.exe -batchmode -force-d3d11 -projectPath <proyecto> -executeMethod Lab09Verification.Run [-lab09Output <carpeta>]
[InitializeOnLoad]
public static class Lab09Verification
{
    private const string SessionKey = "Lab09Verification.Running";
    private const int Fps = 30;
    private static readonly List<string> results = new();
    private static int failures;
    private static int runtimeErrors;
    private static readonly Stack<IEnumerator> stack = new();
    private static int lastFrame = -1;
    private static bool tracked;

    private static double deadline;

    static Lab09Verification()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Lab09/Verificar en Play Mode")]
    public static void Run()
    {
        EditorApplication.delayCall += () =>
        {
            VuforiaDriverPreload.Load();
            EditorSceneManager.OpenScene(Lab09Setup.ScenePath);
            SessionState.SetBool(SessionKey, true);
            EditorApplication.isPlaying = true;
        };
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(SessionKey, false))
            return;

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Time.captureFramerate = Fps;
            deadline = EditorApplication.timeSinceStartup + 120;
            Application.logMessageReceived += CaptureErrors;
            stack.Clear();
            stack.Push(CheckScene());
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            int exitCode = SessionState.GetInt(SessionKey + ".ExitCode", 1);
            SessionState.EraseBool(SessionKey);
            // Se cierra solo si se lanzó desde la línea de comandos (con o sin -batchmode).
            if (Environment.GetCommandLineArgs().Contains("Lab09Verification.Run"))
                EditorApplication.Exit(exitCode);
        }
    }

    private static void CaptureErrors(string message, string stackTrace, LogType type)
    {
        if (type is LogType.Error or LogType.Exception or LogType.Assert)
            runtimeErrors++;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup > deadline)
        {
            Check("Play Mode timeout", false);
            Finish();
            return;
        }

        if (lastFrame == Time.frameCount)
            return;

        lastFrame = Time.frameCount;
        try
        {
            // Ejecuta corrutinas anidadas (yield return OtraCorrutina()) como lo haría StartCoroutine.
            while (stack.Count > 0)
            {
                IEnumerator current = stack.Peek();
                if (!current.MoveNext())
                {
                    stack.Pop();
                    continue;
                }
                if (current.Current is IEnumerator nested)
                {
                    stack.Push(nested);
                    continue;
                }
                return;
            }
            Finish();
        }
        catch (Exception error)
        {
            Check(error.ToString(), false);
            Finish();
        }
    }

    private static void Check(string name, bool passed, string details = "")
    {
        string result = (passed ? "PASS " : "FAIL ") + name;
        if (!string.IsNullOrEmpty(details))
            result += " | " + details;
        results.Add(result);
        if (!passed)
            failures++;
        Debug.Log(result);
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= CaptureErrors;
        Check("No runtime errors", runtimeErrors == 0, $"errors={runtimeErrors}");
        string output = OutputDirectory();
        Directory.CreateDirectory(output);
        results.Insert(0, $"Unity {Application.unityVersion} | {SystemInfo.graphicsDeviceType} | " +
                          $"build target {EditorUserBuildSettings.activeBuildTarget} | Vuforia={Lab09Vuforia.IsInstalled}");
        File.WriteAllLines(Path.Combine(output, "resultados.txt"), results);
        SessionState.SetInt(SessionKey + ".ExitCode", failures == 0 ? 0 : 1);
        EditorApplication.isPlaying = false;
    }

    private static string OutputDirectory()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-lab09Output");
        return flag >= 0 && flag + 1 < args.Length ? args[flag + 1]
            : Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/Lab09"));
    }

    private static string Active(Transform target) =>
        target.Cast<Transform>().Where(child => child.gameObject.activeSelf).Select(child => child.name)
            .DefaultIfEmpty("(ninguno)").Aggregate((a, b) => a + "+" + b);

    private static IEnumerator Wait(float seconds)
    {
        for (int i = 0; i < Mathf.RoundToInt(seconds * Fps); i++)
            yield return null;
    }

    private static IEnumerator CheckScene()
    {
        yield return null;
        GameObject targetObject = GameObject.Find("ImageTarget");
        Check("ImageTarget en la escena", targetObject != null);
        Transform target = targetObject.transform;
        var carousel = target.GetComponent<MultiTargetVuforia>();
        Check("MultiTargetVuforia en el ImageTarget", carousel != null);

        string[] expected = Lab09Setup.Planets.Concat(Lab09Setup.AnimatedAssets).ToArray();
        string[] children = target.Cast<Transform>().Select(child => child.name).ToArray();
        Check("Hijos del ImageTarget en orden", children.SequenceEqual(expected), string.Join(", ", children));
        bool heightOk = Lab09Setup.Planets.All(planet => Mathf.Approximately(target.Find(planet).localPosition.y, 0.08f));
        Check("Planetas en Position Y = 0.08", heightOk);
        Check("Empieza mostrando solo Earth", Active(target) == "Earth", "activo=" + Active(target));
        CheckRendering();

        Button previous = GameObject.Find("ButtonPrevio")?.GetComponent<Button>();
        Button next = GameObject.Find("ButtonSiguiente")?.GetComponent<Button>();
        Check("Botones Previo y Siguiente", previous != null && next != null &&
            previous.onClick.GetPersistentEventCount() == 1 && next.onClick.GetPersistentEventCount() == 1 &&
            previous.onClick.GetPersistentMethodName(0) == "ChangeARModel" &&
            next.onClick.GetPersistentTarget(0) == carousel);

        if (Lab09Vuforia.IsInstalled)
        {
            GameObject arCamera = GameObject.Find("ARCamera");
            Check("ARCamera con VuforiaBehaviour (paso 9)", arCamera != null &&
                arCamera.GetComponent(Lab09Vuforia.FindType("Vuforia.VuforiaBehaviour")) != null &&
                GameObject.Find("Preview Camera") == null);
            string description = Lab09Vuforia.DescribeImageTarget(targetObject);
            Check("Image Target desde SolarSystem.jpg (pasos 19-20)",
                description != null && description.StartsWith("INSTANT SolarSystem"), description);
            Check("Licencia de Vuforia configurada (paso 16)", Lab09Vuforia.LicenseKey().Length > 100);
            Check("Include ARCore Library desactivado (paso 16)", !Lab09Vuforia.IncludesARCore());

            // El simulador procesa cuadros de cámara en otro hilo a tiempo real: se espera por reloj, no por frames.
            double limit = EditorApplication.timeSinceStartup + (Application.isBatchMode ? 5 : 15);
            while (EditorApplication.timeSinceStartup < limit && !(Lab09Vuforia.IsEngineRunning() &&
                       Lab09Vuforia.TargetStatus(targetObject) == "TRACKED"))
                yield return null;
            Check("Vuforia Engine iniciado con la licencia", Lab09Vuforia.IsEngineRunning());
            string status = Lab09Vuforia.TargetStatus(targetObject);
            tracked = status == "TRACKED";
            if (tracked || !Application.isBatchMode)
                Check("El simulador reconoce el Image Target", tracked, "estado=" + status);
            else
                // Sin ventana, Unity no renderiza la ARCamera y el simulador no recibe imágenes que analizar.
                results.Add("INFO El reconocimiento del marcador se prueba en el editor (Play) o en el celular | " +
                            "en batchmode el simulador no recibe imágenes, estado=" + status);
            ShowForCaptures(targetObject);
        }
        else
        {
            tracked = true; // sin Vuforia, la vista previa muestra siempre los modelos
        }

        yield return Capture("01_inicio_tierra.jpg");

        // Recorrido con los botones: cada clic es un onClick.Invoke(), igual que al tocar el botón.
        var steps = new List<string>();
        void Press(Button button)
        {
            button.onClick.Invoke();
            steps.Add((button == next ? "Siguiente→" : "Previo→") + Active(target));
        }

        Press(next);
        bool nextOk = Active(target) == "Mars";
        Press(previous);
        Press(previous);
        bool previousOk = Active(target) == "Venus";
        Press(previous);
        Press(previous);
        bool reachedSun = Active(target) == "Sun";
        Press(previous);
        bool wrapBackOk = reachedSun && Active(target) == expected[^1]; // de Sun pasa al último
        Press(next);
        bool wrapForwardOk = Active(target) == "Sun"; // del último vuelve a Sun
        Check("Siguiente avanza (Earth → Mars)", nextOk);
        Check("Previo retrocede (Mars → Venus)", previousOk);
        Check("Previo en el primero salta al último", wrapBackOk);
        Check("Siguiente en el último vuelve al primero", wrapForwardOk);
        Check("Siempre hay un solo modelo visible", steps.All(step => !step.Contains('+')), string.Join(" | ", steps));

        // Ir a Saturno y comprobar su anillo y la rotación propia.
        while (Active(target) != "Saturn")
            next.onClick.Invoke();
        Transform saturn = target.Find("Saturn");
        Quaternion before = saturn.localRotation;
        yield return Wait(1f);
        Check("Saturno con anillo generado", saturn.GetComponentsInChildren<MeshFilter>()
            .Any(filter => filter.name.Contains("Ring") && filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0));
        Check("Planeta rota sobre su eje", Quaternion.Angle(before, saturn.localRotation) > 10f,
            $"giro en 1 s={Quaternion.Angle(before, saturn.localRotation):F1}°");
        yield return Capture("02_saturno.jpg");

        // Ejercicio 1: assets animados.
        next.onClick.Invoke(); next.onClick.Invoke(); next.onClick.Invoke();
        Check("Siguiente llega a Tierra y Luna", Active(target) == "Tierra y Luna");
        Transform moon = target.Find("Tierra y Luna/PivoteLuna/Orbita/Luna");
        Vector3 moonStart = moon.position;
        yield return Wait(1f);
        float moonAngle = Vector3.Angle(moonStart - moon.parent.parent.position, moon.position - moon.parent.parent.position);
        Check("Animator: la Luna orbita la Tierra", moonAngle > 60f, $"ángulo recorrido en 1 s={moonAngle:F0}° (esperado 90°)");
        yield return Capture("03_tierra_luna.jpg");

        next.onClick.Invoke();
        Transform orbitalEarth = target.Find("Sistema Orbital/OrbitaEarth/Giro/Earth");
        Transform orbitalMercury = target.Find("Sistema Orbital/OrbitaMercury/Giro/Mercury");
        Vector3 center = target.Find("Sistema Orbital").position;
        Vector3 earthStart = orbitalEarth.position - center, mercuryStart = orbitalMercury.position - center;
        yield return Wait(1f);
        float earthAngle = Vector3.Angle(earthStart, orbitalEarth.position - center);
        float mercuryAngle = Vector3.Angle(mercuryStart, orbitalMercury.position - center);
        Check("Animator: los planetas orbitan al Sol a distinta velocidad", earthAngle > 20f && mercuryAngle > earthAngle * 2f,
            $"en 1 s Mercurio={mercuryAngle:F0}° Tierra={earthAngle:F0}°");
        yield return Capture("04_sistema_orbital.jpg");

        next.onClick.Invoke();
        Transform ship = target.Find("Cohete/Nave");
        float startHeight = ship.localPosition.y;
        yield return Wait(1.5f);
        var exhaust = ship.GetComponentInChildren<ParticleSystem>();
        Check("Animator: el cohete despega", ship.localPosition.y - startHeight > 0.05f,
            $"altura a 1.5 s={ship.localPosition.y:F3}");
        Check("Partículas de propulsión activas", exhaust != null && exhaust.particleCount > 5,
            $"partículas={exhaust?.particleCount}");
        yield return Capture("05_cohete.jpg");

        yield return CheckDSJRetro();
    }

    // Sin detección, las capturas usan lo mismo que ejecuta Vuforia al reconocer el marcador.
    private static void ShowForCaptures(GameObject target)
    {
        if (tracked || !Application.isBatchMode || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            return;
        tracked = Lab09Vuforia.ShowTargetContent(target);
        if (tracked && !results.Any(line => line.StartsWith("INFO Capturas")))
            results.Add("INFO Capturas con el contenido del marcador visible | se llamó a OnTrackingFound de " +
                        "DefaultObserverEventHandler, igual que cuando Vuforia detecta la imagen");
    }

    // ---------------------------------------------------------------- Ejercicio 2: DSJRetro AR

    private static string ActiveScene() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    private static IEnumerator CheckDSJRetro()
    {
        Button open = GameObject.Find("ButtonDSJRetro")?.GetComponent<Button>();
        Check("Botón DSJRetro AR en la escena Solar", open != null && open.onClick.GetPersistentEventCount() == 1 &&
            open.onClick.GetPersistentMethodName(0) == "Cargar");
        open.onClick.Invoke();
        for (int i = 0; i < 10 && ActiveScene() != Lab09DSJRetro.SceneName; i++)
            yield return null;
        yield return null;
        Check("El botón carga la escena DSJRetroAR", ActiveScene() == Lab09DSJRetro.SceneName, "escena=" + ActiveScene());

        GameObject targetObject = GameObject.Find("ImageTarget");
        var game = Object.FindAnyObjectByType<PenalesAR>();
        Check("DSJRetro dentro del ImageTarget", targetObject != null && game != null &&
            game.transform.parent == targetObject.transform);
        CheckRendering();
        if (Lab09Vuforia.IsInstalled)
        {
            string description = Lab09Vuforia.DescribeImageTarget(targetObject);
            Check("DSJRetro usa el mismo marcador SolarSystem.jpg", description != null &&
                description.StartsWith("INSTANT SolarSystem"), description);
            tracked = Lab09Vuforia.TargetStatus(targetObject) == "TRACKED";
            ShowForCaptures(targetObject);
        }

        Texture texture = game.transform.Find("Cancha").GetComponent<Renderer>().sharedMaterial.mainTexture;
        SpriteHoja[] sheets = game.GetComponentsInChildren<SpriteHoja>().Where(sheet => sheet.columnas == 6).ToArray();
        Check("Cancha y jugadores de DSJRetro", texture != null && texture.name == "Cancha" && texture.width == 850 &&
            sheets.Length == 4, $"cancha={texture?.name} {texture?.width}x{texture?.height}, jugadores={sheets.Length}");
        Check("Uniformes P1 y CPU", sheets.Count(s => s.GetComponent<Renderer>().sharedMaterial.name == "JugadorP1") == 2 &&
            sheets.Count(s => s.GetComponent<Renderer>().sharedMaterial.name == "JugadorCPU") == 2);

        Button up = GameObject.Find("ButtonArriba")?.GetComponent<Button>();
        Button middle = GameObject.Find("ButtonCentro")?.GetComponent<Button>();
        Button down = GameObject.Find("ButtonAbajo")?.GetComponent<Button>();
        Check("Botones Arriba, Centro y Abajo llaman a PenalesAR.Patear", new[] { up, middle, down }.All(button =>
            button != null && button.onClick.GetPersistentEventCount() == 1 &&
            button.onClick.GetPersistentTarget(0) == game && button.onClick.GetPersistentMethodName(0) == "Patear"));

        int keeperStart = game.arquero.CuadroActual;
        yield return Wait(0.5f);
        Check("Animación de reposo (cuadros 0-3)", game.arquero.CuadroActual != keeperStart &&
            PenalesAR.Quieto.Contains(game.arquero.CuadroActual), $"cuadro {keeperStart} → {game.arquero.CuadroActual}");
        yield return Capture("06_dsjretro_inicio.jpg");

        // Tiro 1: P1 patea arriba y el arquero se lanza abajo → gol.
        Vector3 ballStart = game.balon.localPosition;
        game.AdivinanzaForzada = PenalesAR.Abajo;
        up.onClick.Invoke();
        yield return Wait(0.1f);
        int prepare = game.pateador.CuadroActual;
        yield return Wait(0.25f);
        int kick = game.pateador.CuadroActual;
        Check("Animación de patada (cuadros 48 y 49-51)", prepare == PenalesAR.PrepararPatada &&
            PenalesAR.Patada.Contains(kick), $"preparar={prepare}, patear={kick}");
        middle.onClick.Invoke(); // ignorado: ya hay un tiro en curso
        yield return Wait(0.1f);
        Check("El balón vuela hacia el arco", game.balon.localPosition.x > ballStart.x + 0.02f &&
            game.balon.localPosition.y > ballStart.y,
            $"x={game.balon.localPosition.x:F3}, y={game.balon.localPosition.y:F3}");
        yield return Capture("07_dsjretro_tiro.jpg", PenaltyView, PenaltyFocus);
        yield return Wait(0.4f);
        Vector3 inGoal = game.balon.localPosition - game.arco.localPosition;
        Check("Gol: el balón entra por el lado elegido", game.Goles == 1 && game.UltimoResultado == "GOL" &&
            inGoal.x > 0f && inGoal.z > 0f && Mathf.Abs(inGoal.z) < game.anchoArco / 2f && game.cartelGol.activeSelf,
            $"goles={game.Goles}, balón respecto al arco=({inGoal.x:F3}, {inGoal.y:F3}, {inGoal.z:F3})");
        Check("No se puede patear dos veces a la vez", game.Goles + game.Atajadas == 1);
        yield return Capture("08_dsjretro_gol.jpg", PenaltyView, PenaltyFocus);
        while (game.EnJuego)
            yield return null;
        Check("Después del tiro el balón vuelve al punto penal", Vector3.Distance(game.balon.localPosition, ballStart) < 1e-4f);

        // Tiro 2: P1 patea al centro y el arquero también → atajada.
        game.AdivinanzaForzada = PenalesAR.Centro;
        middle.onClick.Invoke();
        yield return Wait(1.2f);
        Check("Atajada cuando el arquero adivina", game.Atajadas == 1 && game.UltimoResultado == "ATAJADA" &&
            game.balon.localPosition.x < game.arco.localPosition.x && !game.cartelGol.activeSelf,
            $"atajadas={game.Atajadas}, marcador='{game.marcador.text}'");
        yield return Capture("09_dsjretro_atajada.jpg", PenaltyView, PenaltyFocus);
        while (game.EnJuego)
            yield return null;

        // Tiro 3: el arquero elige al azar.
        game.AdivinanzaForzada = null;
        down.onClick.Invoke();
        yield return null;
        while (game.EnJuego)
            yield return null;
        Check("Marcador P1 - CPU", game.Goles + game.Atajadas == 3 &&
            game.marcador.text == $"P1  {game.Goles} - {game.Atajadas}  CPU", $"'{game.marcador.text}'");

        GameObject.Find("ButtonReiniciar").GetComponent<Button>().onClick.Invoke();
        Check("Reiniciar pone el marcador en 0 - 0", game.marcador.text == "P1  0 - 0  CPU");

        GameObject.Find("ButtonSistemaSolar").GetComponent<Button>().onClick.Invoke();
        for (int i = 0; i < 10 && ActiveScene() == Lab09DSJRetro.SceneName; i++)
            yield return null;
        Check("Botón Sistema Solar vuelve a SolarAR", ActiveScene() == "SolarAR", "escena=" + ActiveScene());
    }

    private static void CheckRendering()
    {
        int missingScripts = 0, missingMeshes = 0, invalidMaterials = 0;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
                if (item.TryGetComponent(out MeshFilter filter) && filter.sharedMesh == null && !item.name.Contains("Ring"))
                    missingMeshes++;
                if (item.TryGetComponent(out Renderer renderer))
                    foreach (Material material in renderer.sharedMaterials)
                        if (material == null || material.shader == null || material.shader.name == "Hidden/InternalErrorShader" ||
                            (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null && !material.shader.isSupported))
                            invalidMaterials++;
            }

        Check("Referencias de la escena", missingScripts == 0 && missingMeshes == 0 && invalidMaterials == 0,
            $"missing scripts={missingScripts}, missing meshes={missingMeshes}, invalid materials={invalidMaterials}");
    }

    // Renderiza la cámara con la interfaz incluida (el Canvas pasa temporalmente a Screen Space - Camera).
    // Vista cercana del punto penal y el arco (coordenadas del marcador).
    private static readonly Vector3 PenaltyView = new(0.2f, 0.17f, -0.25f);
    private static readonly Vector3 PenaltyFocus = new(0.37f, 0.02f, 0f);

    private static IEnumerator Capture(string fileName, Vector3? view = null, Vector3? focus = null)
    {
        // Vuforia oculta los modelos mientras no reconoce el marcador: la captura saldría vacía.
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || !tracked)
            yield break;

        yield return null;
        const int width = 1280;
        const int height = 720;
        Camera camera = Camera.main;
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
        RenderTexture previous = RenderTexture.active;
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        Vector3 position = camera.transform.position;
        Quaternion rotation = camera.transform.rotation;
        try
        {
            if (view.HasValue)
            {
                Transform marker = GameObject.Find("ImageTarget").transform;
                camera.transform.position = marker.TransformPoint(view.Value);
                camera.transform.LookAt(marker.TransformPoint(focus ?? Vector3.zero), marker.up);
                foreach (var billboard in Object.FindObjectsByType<MiraCamara>())
                    billboard.SendMessage("LateUpdate");
            }
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane * 2f;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            string output = OutputDirectory();
            Directory.CreateDirectory(output);
            File.WriteAllBytes(Path.Combine(output, fileName), image.EncodeToJPG(90));
        }
        finally
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            camera.transform.SetPositionAndRotation(position, rotation);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.Destroy(image);
        }
    }
}
