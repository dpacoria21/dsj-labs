using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Automatiza los pasos de la guía que se hacen en el editor, para que el proyecto se pueda reconstruir igual.
// Menú Lab09 o, en batchmode: -executeMethod Lab09Setup.RunAll
public static class Lab09Setup
{
    public const string ScenePath = "Assets/Scenes/SolarAR.unity";
    private const string SolarRoot = "Assets/Solar System";
    private const string AnimatedRoot = "Assets/Animados";

    // Orden del sistema solar; la escena los coloca así dentro del ImageTarget.
    public static readonly string[] Planets =
        { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };

    // Assets animados del ejercicio 1, añadidos al final del carrusel.
    public static readonly string[] AnimatedAssets = { "Tierra y Luna", "Sistema Orbital", "Cohete" };

    private const float PlanetHeight = 0.08f;

    [MenuItem("Lab09/Reconstruir todo")]
    public static void RunAll()
    {
        ConfigureAndroid();
        Lab09Vuforia.Configure();
        CreateAnimatedAssets();
        BuildScene();
        Lab09DSJRetro.BuildScene();
    }

    // ---------------------------------------------------------------- Android (pasos 1-4 de "Cambiar de plataforma")

    [MenuItem("Lab09/1. Configurar Android")]
    public static void ConfigureAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        PlayerSettings.companyName = "UNSA DSJ";
        PlayerSettings.productName = "Solar";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.unsa.dsj.solar");

        // Graphics API sin Vulkan: solo OpenGLES3.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        // Minimum API Level: la guía pide Android 6.0 (API 23), pero Vuforia 11.4 exige Android 10 (API 29)
        // y bloquea el build con un valor menor. Sin Vuforia, Unity 6.4 acepta desde API 25.
        PlayerSettings.Android.minSdkVersion = Lab09Vuforia.IsInstalled
            ? AndroidSdkVersions.AndroidApiLevel29
            : AndroidSdkVersions.AndroidApiLevel25;

        // Vuforia y Google Play necesitan ARM64, que requiere IL2CPP.
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

        // Active Input Handling = Both: el simulador de Vuforia usa el Input System nuevo y los botones de la
        // guía usan el StandaloneInputModule clásico.
        var playerSettings = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        playerSettings.FindProperty("activeInputHandler").intValue = 2;
        playerSettings.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        Debug.Log($"[Lab09] Android listo: APIs={string.Join(",", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android))}, " +
                  $"minSdk={PlayerSettings.Android.minSdkVersion}");
    }

    // ---------------------------------------------------------------- Ejercicio 1: assets animados

    [MenuItem("Lab09/2. Crear assets animados")]
    public static void CreateAnimatedAssets()
    {
        EnsureFolder(AnimatedRoot);
        EnsureFolder(AnimatedRoot + "/Animaciones");
        EnsureFolder(AnimatedRoot + "/Materiales");
        EnsureFolder(AnimatedRoot + "/Prefabs");

        Material moon = SolidMaterial("Luna", new Color(0.72f, 0.72f, 0.75f));
        Material rocketBody = SolidMaterial("CoheteCuerpo", new Color(0.92f, 0.93f, 0.95f));
        Material rocketRed = SolidMaterial("CoheteRojo", new Color(0.85f, 0.16f, 0.12f));
        Material window = SolidMaterial("CoheteVentana", new Color(0.2f, 0.55f, 0.95f));
        Material flame = FlameMaterial();

        CreateEarthAndMoon(moon);
        CreateOrbitalSystem();
        CreateRocket(rocketBody, rocketRed, window, flame);
        AssetDatabase.SaveAssets();
        Debug.Log("[Lab09] Assets animados creados en " + AnimatedRoot);
    }

    private static void CreateEarthAndMoon(Material moonMaterial)
    {
        var root = new GameObject("Tierra y Luna");
        GameObject earth = InstantiatePlanet("Earth", root.transform);
        earth.transform.localPosition = Vector3.zero;
        earth.transform.localScale = Vector3.one * 0.1f;
        earth.AddComponent<RotacionPropia>().velocidad = 45f;

        var pivot = new GameObject("PivoteLuna").transform;
        pivot.SetParent(root.transform, false);
        pivot.localRotation = Quaternion.Euler(0f, 0f, 8f); // órbita ligeramente inclinada
        var orbit = new GameObject("Orbita").transform;
        orbit.SetParent(pivot, false);
        GameObject moon = Sphere("Luna", orbit, new Vector3(0.12f, 0f, 0f), 0.03f, moonMaterial);
        moon.AddComponent<RotacionPropia>().velocidad = 30f;

        AnimationClip clip = NewClip("TierraLuna_Orbita", 4f);
        SetEulerY(clip, "PivoteLuna/Orbita", 4f, 360f);
        SavePrefab(root, clip);
    }

    private static void CreateOrbitalSystem()
    {
        var root = new GameObject("Sistema Orbital");
        GameObject sun = InstantiatePlanet("Sun", root.transform);
        sun.transform.localScale = Vector3.one * 0.08f;
        sun.AddComponent<RotacionPropia>().velocidad = 10f;

        // planeta, radio de la órbita, tamaño, segundos por vuelta
        (string planet, float radius, float size, float period)[] orbits =
        {
            ("Mercury", 0.08f, 0.018f, 3f),
            ("Venus", 0.12f, 0.026f, 5f),
            ("Earth", 0.17f, 0.028f, 8f),
            ("Mars", 0.22f, 0.022f, 12f),
        };

        // El clip dura 24 s (mínimo común múltiplo de los periodos) para que todas las órbitas cierren al hacer loop.
        const float length = 24f;
        AnimationClip clip = NewClip("SistemaOrbital_Orbitas", length);
        foreach (var (planet, radius, size, period) in orbits)
        {
            string pivotName = "Orbita" + planet;
            var pivot = new GameObject(pivotName).transform;
            pivot.SetParent(root.transform, false);
            pivot.localRotation = Quaternion.Euler(0f, radius * 900f, 0f); // fases iniciales distintas
            var spin = new GameObject("Giro").transform;
            spin.SetParent(pivot, false);
            GameObject body = InstantiatePlanet(planet, spin);
            body.transform.localPosition = new Vector3(radius, 0f, 0f);
            body.transform.localScale = Vector3.one * size;
            SetEulerY(clip, pivotName + "/Giro", length, 360f * (length / period));
        }

        SavePrefab(root, clip);
    }

    private static void CreateRocket(Material body, Material red, Material window, Material flame)
    {
        var root = new GameObject("Cohete");
        var rocket = new GameObject("Nave").transform;
        rocket.SetParent(root.transform, false);

        GameObject hull = Primitive(PrimitiveType.Cylinder, "Fuselaje", rocket, new Vector3(0f, 0.09f, 0f),
            new Vector3(0.05f, 0.06f, 0.05f), body);
        Primitive(PrimitiveType.Sphere, "Punta", rocket, new Vector3(0f, 0.155f, 0f),
            new Vector3(0.05f, 0.07f, 0.05f), red);
        Primitive(PrimitiveType.Sphere, "Ventana", rocket, new Vector3(0f, 0.11f, -0.023f),
            new Vector3(0.022f, 0.022f, 0.01f), window);
        for (int i = 0; i < 3; i++)
        {
            var fin = new GameObject("Aleta" + i).transform;
            fin.SetParent(rocket, false);
            fin.localRotation = Quaternion.Euler(0f, i * 120f, 0f);
            Primitive(PrimitiveType.Cube, "Placa", fin, new Vector3(0f, 0.04f, 0.033f),
                new Vector3(0.006f, 0.045f, 0.03f), red);
        }

        var exhaust = new GameObject("Propulsion");
        exhaust.transform.SetParent(rocket, false);
        exhaust.transform.localPosition = new Vector3(0f, 0.028f, 0f);
        exhaust.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // emite hacia abajo
        var particles = exhaust.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.35f;
        main.startSpeed = 0.25f;
        main.startSize = 0.025f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.3f), new Color(1f, 0.35f, 0.05f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        var emission = particles.emission;
        emission.rateOverTime = 60f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.008f;
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        exhaust.GetComponent<ParticleSystemRenderer>().sharedMaterial = flame;
        Object.DestroyImmediate(hull.GetComponent<Collider>());

        // Despega, se balancea en el aire y aterriza (4 s en loop).
        AnimationClip clip = NewClip("Cohete_Vuelo", 4f);
        var height = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(1f, 0.06f), new Keyframe(2f, 0.09f),
            new Keyframe(3f, 0.06f), new Keyframe(4f, 0f));
        SetCurve(clip, "Nave", "m_LocalPosition.y", height);
        SetCurve(clip, "Nave", "m_LocalPosition.x", AnimationCurve.Constant(0f, 4f, 0f));
        SetCurve(clip, "Nave", "m_LocalPosition.z", AnimationCurve.Constant(0f, 4f, 0f));
        SetCurve(clip, "Nave", "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, 4f, 0f));
        SetCurve(clip, "Nave", "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, 4f, 360f));
        SetCurve(clip, "Nave", "localEulerAnglesRaw.z", new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(1.5f, 8f), new Keyframe(2.5f, -8f), new Keyframe(4f, 0f)));
        SavePrefab(root, clip);
    }

    private static GameObject InstantiatePlanet(string planet, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{SolarRoot}/Prefabs/{planet}.prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.transform.localPosition = Vector3.zero;
        return instance;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 position, float size, Material material) =>
        Primitive(PrimitiveType.Sphere, name, parent, position, Vector3.one * size, material);

    private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        return item;
    }

    private static AnimationClip NewClip(string name, float length)
    {
        string path = $"{AnimatedRoot}/Animaciones/{name}.anim";
        AssetDatabase.DeleteAsset(path);
        var clip = new AnimationClip { name = name, frameRate = 30f };
        AssetDatabase.CreateAsset(clip, path);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        settings.stopTime = length;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }

    private static void SetEulerY(AnimationClip clip, string path, float length, float degrees)
    {
        SetCurve(clip, path, "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, length, 0f));
        SetCurve(clip, path, "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, length, degrees));
        SetCurve(clip, path, "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, length, 0f));
    }

    private static void SetCurve(AnimationClip clip, string path, string property, AnimationCurve curve)
    {
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
    }

    private static void SavePrefab(GameObject root, AnimationClip clip)
    {
        string controllerPath = $"{AnimatedRoot}/Animaciones/{root.name}.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, clip);
        root.AddComponent<Animator>().runtimeAnimatorController = controller;
        PrefabUtility.SaveAsPrefabAsset(root, $"{AnimatedRoot}/Prefabs/{root.name}.prefab");
        Object.DestroyImmediate(root);
    }

    private static Material SolidMaterial(string name, Color color)
    {
        string path = $"{AnimatedRoot}/Materiales/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Glossiness", 0.35f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material FlameMaterial()
    {
        string path = $"{AnimatedRoot}/Materiales/Llama.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.mainTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- Escena (pasos 9-38)

    [MenuItem("Lab09/3. Construir escena")]
    public static void BuildScene()
    {
        EnsureFolder("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Paso 3: la guía elimina la Main Camera porque la reemplaza el ARCamera de Vuforia.
        // Mientras Vuforia no esté importado, una cámara de vista previa mira el marcador desde arriba.
        bool vuforia = Lab09Vuforia.IsInstalled;
        if (vuforia)
            Lab09Vuforia.CreateARCamera();
        else
            CreatePreviewCamera();

        // Pasos 19-22: ImageTarget con los prefabs como hijos, Y = 0.08 y todos ocultos.
        GameObject target = vuforia ? Lab09Vuforia.CreateImageTarget() : new GameObject("ImageTarget");
        GameObject earth = null;
        foreach (string planet in Planets)
        {
            GameObject instance = InstantiatePlanet(planet, target.transform);
            instance.transform.localPosition = new Vector3(0f, PlanetHeight, 0f);
            instance.AddComponent<RotacionPropia>().velocidad = planet == "Sun" ? 8f : 25f;
            instance.SetActive(false);
            if (planet == "Earth")
                earth = instance;
        }

        // Ejercicio 1: assets animados al final del carrusel.
        foreach (string asset in AnimatedAssets)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AnimatedRoot}/Prefabs/{asset}.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, target.transform);
            // El cohete despega desde el marcador; los sistemas quedan a la altura de los planetas.
            instance.transform.localPosition = new Vector3(0f, asset == "Cohete" ? 0f : PlanetHeight, 0f);
            instance.SetActive(false);
        }

        // Pasos 33-35: script en el ImageTarget, Earth como Start Model y visible.
        var carousel = target.AddComponent<MultiTargetVuforia>();
        var serialized = new SerializedObject(carousel);
        serialized.FindProperty("startModel").objectReferenceValue = earth;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        earth.SetActive(true);

        // Pasos 23-27 y 36-38: botones Previo / Siguiente.
        BuildInterface(carousel, target.transform);

        if (!vuforia)
            CreateTargetPreview();

        EditorSceneManager.SaveScene(scene, ScenePath);
        Lab09DSJRetro.RegisterScenes();
        AssetDatabase.SaveAssets();
        Debug.Log($"[Lab09] Escena guardada en {ScenePath} (Vuforia={(vuforia ? "ARCamera + ImageTarget" : "no, vista previa")})");
    }

    internal static void CreatePreviewCamera()
    {
        var cameraObject = new GameObject("Preview Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.06f, 0.1f);
        camera.nearClipPlane = 0.01f;
        camera.fieldOfView = 50f;
        cameraObject.transform.position = new Vector3(0f, 0.42f, -0.5f);
        cameraObject.transform.LookAt(new Vector3(0f, 0.06f, 0f));
        cameraObject.AddComponent<AudioListener>();
    }

    // Plano con la imagen SolarSystem para ver dónde quedaría el marcador. EditorOnly: no entra al APK.
    private static void CreateTargetPreview()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{SolarRoot}/ImageTargets/SolarSystem.jpg");
        string path = $"{AnimatedRoot}/Materiales/MarcadorVistaPrevia.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Unlit/Texture"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.mainTexture = texture;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Marcador (vista previa)";
        quad.tag = "EditorOnly";
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        float aspect = texture != null ? (float)texture.height / texture.width : 1f;
        quad.transform.localScale = new Vector3(0.6f, 0.6f * aspect, 1f);
        quad.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void BuildInterface(MultiTargetVuforia carousel, Transform target)
    {
        Transform canvasTransform = CreateCanvas();
        GameObject canvasObject = canvasTransform.gameObject;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Button previous = CreateButton("ButtonPrevio", "Previo", new Vector2(-170f, 90f), canvasObject.transform, font);
        Button next = CreateButton("ButtonSiguiente", "Siguiente", new Vector2(170f, 90f), canvasObject.transform, font);
        UnityEventTools.AddIntPersistentListener(previous.onClick, carousel.ChangeARModel, -1);
        UnityEventTools.AddIntPersistentListener(next.onClick, carousel.ChangeARModel, 1);

        // Ejercicio 2: acceso a la escena DSJRetro AR.
        SceneButton("ButtonDSJRetro", "DSJRetro AR", Lab09DSJRetro.SceneName, new Vector2(1f, 1f), new Vector2(-30f, -30f),
            canvasTransform, font);
        AddModelLabel(canvasTransform, target, font);
    }

    internal static Transform CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        return canvasObject.transform;
    }

    // Botón en una esquina que carga otra escena (CambiarEscena.Cargar).
    internal static Button SceneButton(string name, string caption, string scene, Vector2 corner, Vector2 offset,
        Transform canvas, Font font)
    {
        Button button = CreateButton(name, caption, Vector2.zero, canvas, font);
        Lab09DSJRetro.Corner((RectTransform)button.transform, corner, offset);
        var changer = button.gameObject.AddComponent<CambiarEscena>();
        UnityEventTools.AddStringPersistentListener(button.onClick, changer.Cargar, scene);
        return button;
    }

    private static void AddModelLabel(Transform canvasTransform, Transform target, Font font)
    {
        GameObject canvasObject = canvasTransform.gameObject;
        var label = new GameObject("NombreModelo", typeof(RectTransform));
        label.layer = canvasObject.layer;
        label.transform.SetParent(canvasObject.transform, false);
        var rect = (RectTransform)label.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -60f);
        rect.sizeDelta = new Vector2(700f, 90f);
        var text = label.AddComponent<Text>();
        text.font = font;
        text.fontSize = 56;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        label.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        label.AddComponent<EtiquetaModelo>().imageTarget = target;
    }

    internal static Button CreateButton(string name, string caption, Vector2 position, Transform parent, Font font)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform));
        buttonObject.layer = parent.gameObject.layer;
        buttonObject.transform.SetParent(parent, false);
        var rect = (RectTransform)buttonObject.transform;
        // Paso 24: anclaje Bottom Center.
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300f, 100f);

        var image = buttonObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        var textObject = new GameObject("Text (Legacy)", typeof(RectTransform));
        textObject.layer = buttonObject.layer;
        textObject.transform.SetParent(buttonObject.transform, false);
        var textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = caption;
        text.fontSize = 25; // Paso 26
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.2f, 0.2f, 0.2f);
        return button;
    }

    // ---------------------------------------------------------------- Utilidades

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        string parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
