using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Ejercicio 2: escena DSJRetroAR. La cancha, los jugadores y el balón del proyecto del curso (DSJRetro, Godot)
// aparecen sobre el mismo marcador SolarSystem.jpg y se juega una tanda de penales con botones.
// Los sprites se exportan antes con Herramientas/exportar_sprites_dsjretro.py.
public static class Lab09DSJRetro
{
    public const string ScenePath = "Assets/Scenes/DSJRetroAR.unity";
    public const string SceneName = "DSJRetroAR";
    private const string Root = "Assets/DSJRetro";
    private const string Sprites = Root + "/Sprites";
    private const string Materials = Root + "/Materiales";

    // La cancha mide 850 x 360 píxeles; en el marcador ocupa 0.96 de ancho (el marcador mide 1).
    private const float PitchWidth = 0.96f;
    private const float PitchPixel = PitchWidth / 850f;
    // Los personajes se dibujan al doble de escala que la cancha para que se vean bien en el celular.
    private const float CharacterPixel = PitchPixel * 2f;
    private const float Frame = 32f * CharacterPixel;

    // Posiciones en píxeles de pitch-lines.png (origen arriba a la izquierda).
    private static Vector3 PitchPoint(float px, float py, float height = 0f) =>
        new((px - 425f) * PitchPixel, height, (180f - py) * PitchPixel);

    [MenuItem("Lab09/4. Construir escena DSJRetro AR (ejercicio 2)")]
    public static void BuildScene()
    {
        ConfigureTextures();
        Lab09Setup.EnsureFolder(Materials);
        Material pitch = TextureMaterial("Cancha", "Unlit/Texture");
        Material p1 = TextureMaterial("JugadorP1", "Unlit/Transparent Cutout");
        Material cpu = TextureMaterial("JugadorCPU", "Unlit/Transparent Cutout");
        Material ball = TextureMaterial("Balon", "Unlit/Transparent Cutout");
        Material shadow = TextureMaterial("Sombra", "Unlit/Transparent");
        Material tag1P = TextureMaterial("Indicador1P", "Unlit/Transparent Cutout");
        Material tagCpu = TextureMaterial("IndicadorCPU", "Unlit/Transparent Cutout");
        Material net = TextureMaterial("Red", "Unlit/Transparent");
        Material post = ColorMaterial("Palo", new Color(0.97f, 0.97f, 0.95f));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        bool vuforia = Lab09Vuforia.IsInstalled;
        if (vuforia)
            Lab09Vuforia.CreateARCamera();
        else
            Lab09Setup.CreatePreviewCamera();
        GameObject target = vuforia ? Lab09Vuforia.CreateImageTarget() : new GameObject("ImageTarget");

        var game = new GameObject("DSJRetro").transform;
        game.SetParent(target.transform, false);

        // Cancha plana sobre el marcador.
        GameObject field = Quad("Cancha", game, new Vector3(0f, 0.0005f, 0f), new Vector2(PitchWidth, 360f * PitchPixel), pitch);
        field.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Arco derecho: línea de gol en x = 798 px, centro del área chica en y = 185 px.
        Vector3 goalCenter = PitchPoint(798f, 185f);
        const float goalWidth = 0.064f, goalHeight = 0.04f, goalDepth = 0.025f;
        var goal = new GameObject("Arco").transform;
        goal.SetParent(game, false);
        goal.localPosition = goalCenter;
        BuildGoal(goal, goalWidth, goalHeight, goalDepth, post, net);

        // Jugadores: el que patea en el punto penal, el arquero en la línea y dos jugadores más de adorno.
        SpriteHoja kicker = Player("P1 Pateador", game, PitchPoint(700f, 185f), p1, false, shadow, tag1P);
        SpriteHoja keeper = Player("CPU Arquero", game, goalCenter + new Vector3(-0.012f, 0f, 0f), cpu, true, shadow, tagCpu);
        SpriteHoja mate = Player("P1 Companero", game, PitchPoint(560f, 110f), p1, false, shadow, null);
        mate.Reproducir(new[] { 6, 7, 8, 9, 10, 11 }, 10f, true);
        SpriteHoja defender = Player("CPU Defensa", game, PitchPoint(610f, 285f), cpu, true, shadow, null);
        defender.cuadros = PenalesAR.Quieto;

        // Balón delante del pie del pateador (soccer-ball.png: 4 cuadros de 10 x 10).
        float ballSize = 10f * CharacterPixel;
        GameObject ballObject = Quad("Balon", game, PitchPoint(700f, 185f, ballSize / 2f) + new Vector3(0.022f, 0f, 0f),
            Vector2.one * ballSize, ball);
        ballObject.AddComponent<MiraCamara>();
        var ballSheet = ballObject.AddComponent<SpriteHoja>();
        ballSheet.columnas = 4;
        ballSheet.filas = 1;
        ballSheet.cuadros = new[] { 0 };
        Shadow(ballObject.transform, shadow, ballSize / 2f, 1.1f);

        var penalties = game.gameObject.AddComponent<PenalesAR>();
        penalties.pateador = kicker;
        penalties.arquero = keeper;
        penalties.balon = ballObject.transform;
        penalties.balonHoja = ballSheet;
        penalties.arco = goal;
        penalties.anchoArco = goalWidth;
        BuildInterface(penalties);

        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterScenes();
        AssetDatabase.SaveAssets();
        Debug.Log($"[Lab09] Escena DSJRetro guardada en {ScenePath}");
    }

    public static void RegisterScenes()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            { new(Lab09Setup.ScenePath, true) };
        if (File.Exists(ScenePath))
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void BuildGoal(Transform goal, float width, float height, float depth, Material post, Material net)
    {
        const float thickness = 0.004f;
        foreach (float side in new[] { -1f, 1f })
        {
            Bar("Palo", goal, new Vector3(0f, height / 2f, side * width / 2f), new Vector3(thickness, height, thickness), post);
            Bar("Soporte", goal, new Vector3(depth / 2f, height, side * width / 2f), new Vector3(depth, thickness, thickness), post);
            GameObject sideNet = Quad("RedLateral", goal, new Vector3(depth / 2f, height / 2f, side * width / 2f),
                new Vector2(depth, height), net);
            sideNet.transform.localRotation = Quaternion.identity;
        }
        Bar("Travesano", goal, new Vector3(0f, height, 0f), new Vector3(thickness, thickness, width + thickness), post);
        GameObject back = Quad("RedFondo", goal, new Vector3(depth, height / 2f, 0f), new Vector2(width, height), net);
        back.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        GameObject top = Quad("RedTecho", goal, new Vector3(depth / 2f, height, 0f), new Vector2(depth, width), net);
        top.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private static SpriteHoja Player(string name, Transform parent, Vector3 feet, Material sheet, bool facesLeft,
        Material shadow, Material tag)
    {
        GameObject body = Quad(name, parent, feet + Vector3.up * (Frame / 2f), Vector2.one * Frame, sheet);
        body.AddComponent<MiraCamara>();
        var animation = body.AddComponent<SpriteHoja>();
        animation.voltear = facesLeft;
        animation.cuadros = PenalesAR.Quieto;
        Shadow(body.transform, shadow, Frame / 2f, 0.5f);
        if (tag != null)
        {
            // Indicador 1P / CPU sobre la cabeza, como en el partido.
            GameObject marker = Quad("Indicador", body.transform, new Vector3(0f, 0.62f, 0f),
                new Vector2(15f, 11f) / 32f, tag);
            marker.transform.localRotation = Quaternion.identity;
        }
        return animation;
    }

    private static void Shadow(Transform owner, Material material, float heightAboveGround, float size)
    {
        // Hijo del sprite: queda a ras de suelo aunque el sprite gire hacia la cámara.
        GameObject shadow = Quad("Sombra", owner, new Vector3(0f, -heightAboveGround / owner.localScale.y + 0.08f, 0f),
            new Vector2(size, size * 0.5f), material);
        shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private static GameObject Quad(string name, Transform parent, Vector3 position, Vector2 size, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = position;
        // z = y: los hijos girados 90° (sombras) heredan la misma escala en profundidad.
        quad.transform.localScale = new Vector3(size.x, size.y, size.y);
        var renderer = quad.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return quad;
    }

    private static void Bar(string name, Transform parent, Vector3 position, Vector3 size, Material material)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = name;
        Object.DestroyImmediate(bar.GetComponent<Collider>());
        bar.transform.SetParent(parent, false);
        bar.transform.localPosition = position;
        bar.transform.localScale = size;
        bar.GetComponent<Renderer>().sharedMaterial = material;
    }

    // ---------------------------------------------------------------- Interfaz

    private static void BuildInterface(PenalesAR penalties)
    {
        Transform canvas = Lab09Setup.CreateCanvas();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Label("Titulo", canvas, "DSJRetro AR · Penales", new Vector2(0f, -60f), 56, Color.white, font);
        penalties.marcador = Label("Marcador", canvas, "P1  0 - 0  CPU", new Vector2(0f, -150f), 48,
            new Color(1f, 0.92f, 0.3f), font);
        Text message = Label("Mensaje", canvas, "Elige dónde patear", new Vector2(0f, 230f), 44, Color.white, font);
        var messageRect = (RectTransform)message.transform;
        messageRect.anchorMin = messageRect.anchorMax = messageRect.pivot = new Vector2(0.5f, 0f);
        messageRect.anchoredPosition = new Vector2(0f, 220f);
        penalties.mensaje = message;

        // Cartel GOAL! de DSJRetro (assets/art/ui/goal.png).
        var banner = new GameObject("CartelGol", typeof(RectTransform));
        banner.layer = canvas.gameObject.layer;
        banner.transform.SetParent(canvas, false);
        var bannerRect = (RectTransform)banner.transform;
        bannerRect.anchoredPosition = new Vector2(0f, 80f);
        bannerRect.sizeDelta = new Vector2(133f, 50f) * 4f;
        var image = banner.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + "/Gol.png");
        image.preserveAspect = true;
        penalties.cartelGol = banner;
        banner.SetActive(false); // solo aparece al marcar

        (string name, string caption, float x, int direction)[] shots =
        {
            ("ButtonArriba", "Arriba", -340f, PenalesAR.Arriba),
            ("ButtonCentro", "Centro", 0f, PenalesAR.Centro),
            ("ButtonAbajo", "Abajo", 340f, PenalesAR.Abajo),
        };
        foreach (var (name, caption, x, direction) in shots)
        {
            Button button = Lab09Setup.CreateButton(name, caption, new Vector2(x, 90f), canvas, font);
            UnityEventTools.AddIntPersistentListener(button.onClick, penalties.Patear, direction);
        }

        Button restart = Lab09Setup.CreateButton("ButtonReiniciar", "Reiniciar", Vector2.zero, canvas, font);
        Corner((RectTransform)restart.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f));
        UnityEventTools.AddVoidPersistentListener(restart.onClick, penalties.Reiniciar);

        Lab09Setup.SceneButton("ButtonSistemaSolar", "Sistema Solar", Path.GetFileNameWithoutExtension(Lab09Setup.ScenePath),
            new Vector2(0f, 1f), new Vector2(30f, -30f), canvas, font);
    }

    public static void Corner(RectTransform rect, Vector2 corner, Vector2 offset)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = corner;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(260f, 80f);
    }

    private static Text Label(string name, Transform canvas, string content, Vector2 position, int size, Color color, Font font)
    {
        var label = new GameObject(name, typeof(RectTransform));
        label.layer = canvas.gameObject.layer;
        label.transform.SetParent(canvas, false);
        var rect = (RectTransform)label.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(900f, 80f);
        var text = label.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        label.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);
        return text;
    }

    // ---------------------------------------------------------------- Texturas y materiales

    private static void ConfigureTextures()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Sprites }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = path.EndsWith("Gol.png") ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; // pixel art nítido
            importer.mipmapEnabled = false;
            importer.wrapMode = path.EndsWith("Red.png") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Material TextureMaterial(string texture, string shader)
    {
        Material material = LoadOrCreate(texture, shader);
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Sprites}/{texture}.png");
        if (material.HasProperty("_Cutoff"))
            material.SetFloat("_Cutoff", 0.5f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material ColorMaterial(string name, Color color)
    {
        Material material = LoadOrCreate(name, "Standard");
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material LoadOrCreate(string name, string shader)
    {
        string path = $"{Materials}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = Shader.Find(shader);
        return material;
    }
}
