using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lab05ProjectBuilder
{
    public static readonly string[] SceneNames = { "01_Prediccion_Base", "02_Dos_Objetivos_Puntuacion" };
    const string Folder = "Assets/Scenes/";
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + name + ".png");

    [MenuItem("Laboratorio 05/Regenerar las dos escenas (reemplaza las existentes)")]
    public static void CreateScenes()
    {
        Directory.CreateDirectory(Folder);
        EditorSettings.serializationMode = SerializationMode.ForceText;
        PlayerSettings.companyName = "DSJ - UNSA";
        PlayerSettings.productName = "LAB05 Prediccion de Trayectoria";
        PlayerSettings.defaultScreenWidth = 1440;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue = 0;
        settings.ApplyModifiedPropertiesWithoutUndo();
        Physics2D.gravity = new Vector2(0, -9.81f);
        Time.fixedDeltaTime = .02f;
        Time.maximumDeltaTime = .1f;
        // box.png usa 180 px/unidad en upstream. Sliced evita estirar el borde
        // y hace coincidir sus dimensiones visibles con las del collider.
        var boxImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/box.png");
        var textureSettings = new TextureImporterSettings();
        boxImporter.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        boxImporter.SetTextureSettings(textureSettings);
        boxImporter.spriteBorder = Vector4.one * 14;
        boxImporter.SaveAndReimport();
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Physics/bounciness.physicsMaterial2D");
        if (material == null)
        {
            material = new PhysicsMaterial2D("bounciness");
            AssetDatabase.CreateAsset(material, "Assets/Physics/bounciness.physicsMaterial2D");
        }
        material.friction = .5f;
        material.bounciness = .6f;
        EditorUtility.SetDirty(material);
        for (int i = 0; i < SceneNames.Length; i++) CreateScene(i, material);
        EditorBuildSettings.scenes = SceneNames.Select(n => new EditorBuildSettingsScene(Folder + n + ".unity", true)).ToArray();
        EditorSceneManager.OpenScene(Folder + SceneNames[0] + ".unity");
        AssetDatabase.SaveAssets();
        Debug.Log("LAB05_SCENES_OK: 2 escenas, sprites upstream y referencias asignadas.");
    }

    static GameObject Image(string name, string sprite, Vector2 position, Vector2 scale, Color color, int order = 0)
    {
        var obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite(sprite);
        renderer.color = color;
        renderer.sortingOrder = order;
        return obj;
    }
    static GameObject Wall(string name, Vector2 position, Vector2 size)
    {
        var obj = Image(name, "box", position, Vector2.one, new Color(.58f, .04f, .90f));
        var renderer = obj.GetComponent<SpriteRenderer>();
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        obj.AddComponent<BoxCollider2D>().size = size;
        return obj;
    }
    static TextMesh Label(string text, Vector2 pos, Color color)
    {
        var obj = new GameObject(text);
        obj.transform.position = new Vector3(pos.x, pos.y, -1);
        obj.transform.localScale = Vector3.one * .08f;
        var mesh = obj.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = 46;
        mesh.characterSize = 1;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.color = color;
        obj.GetComponent<MeshRenderer>().sortingOrder = 20;
        return mesh;
    }
    static Cup Target(string id, Vector2 position, Color color)
    {
        var obj = Image("Cup " + id, "soap_cup", position, Vector2.one * .6f, color, 5);
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = 1.5f;
        rb.gravityScale = 1;
        rb.linearDamping = .5f;
        rb.angularDamping = .5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // El objetivo conserva cuerpo Dynamic; se bloquea la rotacion para mantener la boca legible.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var edge = obj.AddComponent<EdgeCollider2D>();
        edge.points = new[] { new Vector2(-.98f, 1.06f), new Vector2(-.69f, -1.12f), new Vector2(.69f, -1.12f), new Vector2(.98f, 1.06f) };
        edge.edgeRadius = .03f;
        var cup = obj.AddComponent<Cup>();
        cup.targetId = id;
        var sensorObj = new GameObject("Sensor interior " + id);
        sensorObj.transform.SetParent(obj.transform, false);
        sensorObj.transform.localPosition = new Vector3(0, .45f, 0);
        var box = sensorObj.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.20f, .3f);
        cup.sensor = sensorObj.AddComponent<CupSensor>();
        cup.sensor.cup = cup;
        Label("OBJETIVO " + id, position + new Vector2(0, 1.1f), new Color(.25f, .26f, .36f));
        return cup;
    }
    static Material Flat(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(material, path); }
        material.color = color;
        return material;
    }
    static void CreateScene(int index, PhysicsMaterial2D bounciness)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var environment = new GameObject("Environment");
        Wall("wall - suelo", new Vector2(0, -3.7f), new Vector2(11.2f, .4f)).transform.SetParent(environment.transform);
        Wall("wall - derecha", new Vector2(5.4f, 0), new Vector2(.4f, 7.4f)).transform.SetParent(environment.transform);
        Wall("wall - soporte A", new Vector2(.7f, -2.825f), new Vector2(1.35f, 1.35f)).transform.SetParent(environment.transform);
        Target("A", new Vector2(.7f, -1.46f), Color.white);
        if (index == 1)
        {
            Wall("wall - soporte B", new Vector2(3.0f, -3.4f), new Vector2(1.35f, .20f)).transform.SetParent(environment.transform);
            Target("B", new Vector2(3.0f, -2.61f), new Color(.55f, .90f, 1));
        }
        var obj = Image("Ball", "ball", new Vector2(-3.8f, 1.1f), Vector2.one * .48f, Color.white, 10);
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = 1;
        rb.gravityScale = 1;
        rb.linearDamping = 0;
        rb.angularDamping = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sharedMaterial = bounciness;
        obj.AddComponent<CircleCollider2D>().radius = .645f;
        var ball = obj.AddComponent<Ball>();
        var trail = obj.AddComponent<TrailRenderer>();
        trail.sharedMaterial = Flat("Recorrido real", new Color(.12f, .65f, .2f, .75f));
        trail.time = 4;
        trail.startWidth = .055f;
        trail.endWidth = .02f;
        trail.minVertexDistance = .025f;
        trail.sortingOrder = 9;
        var trajObj = new GameObject("Trajectory");
        var trajectory = trajObj.AddComponent<Trajectory>();
        trajectory.ball = ball;
        trajectory.dotsParent = new GameObject("Dots");
        trajectory.dotsParent.transform.SetParent(trajObj.transform);
        trajectory.dotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TrajectoryDot.prefab");
        trajectory.dotsNumber = 45;
        trajectory.dotSpacing = .05f;
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        gm.ball = ball;
        gm.trajectory = trajectory;
        gm.scoreEnabled = index == 1;
        gm.experimentTitle = index == 0 ? "Prediccion de trayectoria" : "Dos objetivos y puntuacion";
        Label("LAB05  /  " + gm.experimentTitle, new Vector2(0, 4.1f), new Color(.25f, .26f, .36f));
        gm.worldScore = Label(index == 0 ? "ENTRADAS: 0" : "PUNTOS: 0", new Vector2(-3.5f, -1.75f), new Color(.25f, .26f, .36f));
        gm.worldStatus = Label("TIRO: 0 / LISTO", new Vector2(-3.5f, -2.22f), new Color(.25f, .26f, .36f));
        var line = gm.gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = Flat("Direccion de arrastre", new Color(.02f, .62f, .74f));
        line.startWidth = line.endWidth = .035f;
        line.sortingOrder = 8;
        line.useWorldSpace = true;
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, .20f, -10);
        camera.orthographic = true;
        camera.orthographicSize = 4.75f;
        camera.rect = new Rect(0, .10f, 1, .76f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.98f, .985f, 1f);
        camera.gameObject.AddComponent<AudioListener>();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Folder + SceneNames[index] + ".unity");
    }

    public static void BuildAndVerify()
    {
        CreateScenes();
        Lab05Verification.Run();
        Directory.CreateDirectory("Builds/Windows");
        var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/Windows/LAB05.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build fallo: " + report.summary.result);
        Debug.Log("LAB05_BUILD_OK bytes=" + report.summary.totalSize);
    }
}
