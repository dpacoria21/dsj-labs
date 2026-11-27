using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Integración con Vuforia (pasos 9-20). Usa reflexión para que el proyecto también compile sin el paquete
// de Vuforia (que no se sube al repositorio por su tamaño).
public static class Lab09Vuforia
{
    // Licencia personal fuera del repositorio: lab09/Solar/vuforia-license.txt (ignorado por git).
    public const string LicenseFile = "vuforia-license.txt";
    public const string TargetImage = "Assets/Solar System/ImageTargets/SolarSystem.jpg";
    // Tamaño del lado mayor del marcador en unidades de escena (valor por defecto de Vuforia).
    // Los planetas miden 0.1, es decir, una décima parte del ancho de la imagen.
    public const float TargetSize = 1f;
    private const BindingFlags Members =BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static bool IsInstalled => FindType("Vuforia.VuforiaBehaviour") != null;

    // ---------------------------------------------------------------- Pasos 14-16: configuración de Vuforia

    [MenuItem("Lab09/Configurar Vuforia (licencia y ARCore)")]
    public static void Configure()
    {
        if (!IsInstalled)
        {
            Debug.LogWarning("[Lab09] Vuforia no está instalado; se omite su configuración.");
            return;
        }

        Type configType = FindType("Vuforia.VuforiaConfiguration");
        var config = (ScriptableObject)configType.GetProperty("Instance", Members)!.GetValue(null);
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(config)))
        {
            Lab09Setup.EnsureFolder("Assets/Resources");
            AssetDatabase.CreateAsset(config, "Assets/Resources/VuforiaConfiguration.asset");
        }

        // Paso 16: App License Key.
        string key = File.Exists(LicenseFile) ? File.ReadAllText(LicenseFile).Trim() : "";
        object generic = configType.GetProperty("Vuforia", Members)!.GetValue(config);
        if (key.Length > 0)
            SetProperty(generic, "LicenseKey", key);
        else
            Debug.LogWarning($"[Lab09] No se encontró {LicenseFile}: pega la licencia en Window > Vuforia Configuration.");

        // Paso 16: desactivar Include ARCore Library.
        object deviceTracker = configType.GetProperty("DeviceTracker", Members)!.GetValue(config);
        SetProperty(deviceTracker, "AutoImportArcoreSetting", false);

        // Play Mode con el simulador de Vuforia: funciona sin webcam.
        object playMode = configType.GetProperty("PlayMode", Members)!.GetValue(config);
        SetProperty(playMode, "PlayModeType", Enum.Parse(FindType("Vuforia.PlayModeType"), "SIMULATOR"));

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Lab09] Vuforia configurado: licencia={(key.Length > 0 ? "sí" : "no")}, ARCore=no, Play Mode=Simulator");
    }

    public static string LicenseKey()
    {
        object config = FindType("Vuforia.VuforiaConfiguration").GetProperty("Instance", Members)!.GetValue(null);
        object generic = config.GetType().GetProperty("Vuforia", Members)!.GetValue(config);
        return (string)generic.GetType().GetProperty("LicenseKey", Members)!.GetValue(generic);
    }

    public static bool IncludesARCore()
    {
        object config = FindType("Vuforia.VuforiaConfiguration").GetProperty("Instance", Members)!.GetValue(null);
        object tracker = config.GetType().GetProperty("DeviceTracker", Members)!.GetValue(config);
        return (bool)tracker.GetType().GetProperty("AutoImportArcoreSetting", Members)!.GetValue(tracker);
    }

    // ---------------------------------------------------------------- Paso 9: AR Camera

    public static GameObject CreateARCamera()
    {
        GameObject camera = CreateFromMenu("GameObject/Vuforia Engine/AR Camera", "ARCamera");
        camera.tag = "MainCamera";
        // Pose inicial del simulador: mirando el marcador desde arriba y de frente.
        camera.transform.position = new Vector3(0f, 0.45f, -0.45f);
        camera.transform.LookAt(new Vector3(0f, 0.06f, 0f));
        return camera;
    }

    // ---------------------------------------------------------------- Pasos 19-20: Image Target con SolarSystem.jpg

    public static GameObject CreateImageTarget()
    {
        GameObject target = CreateFromMenu("GameObject/Vuforia Engine/Image Target", "ImageTarget");
        Type behaviourType = FindType("Vuforia.ImageTargetBehaviour");
        Component behaviour = target.GetComponent(behaviourType);

        // Lo mismo que hace el Inspector al arrastrar la imagen: ajustar la importación de la textura
        // (Read/Write, sin compresión), elegir el tipo "From Image" y calcular tamaño y nombre del target.
        Editor editor = Editor.CreateEditor(behaviour); // su OnEnable prepara el estado que usa UpdateImageTargetData
        try
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(TargetImage);
            editor.GetType().GetMethod("SetRequiredInstantImageTargetTextureImportSettings", Members)!
                .Invoke(editor, new object[] { importer });
            importer.SaveAndReimport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetImage);
            object wrapper = editor.GetType().GetField("mSerializedObject", Members)!.GetValue(editor);
            editor.serializedObject.Update();
            // Igual que ImageTargetEditor.DrawRunTimeTargetInspectorUI al soltar una imagen en el campo Image:
            // el lado mayor mide TargetSize unidades de escena y el otro conserva la proporción de la imagen.
            float longest = Mathf.Max(texture.width, texture.height);
            using (var edit = (IDisposable)wrapper.GetType().GetMethod("Edit", Members)!.Invoke(wrapper, null))
            {
                SetProperty(wrapper, "ImageTargetType", Enum.Parse(FindType("Vuforia.ImageTargetType"), "INSTANT"));
                SetProperty(wrapper, "TrackableName", texture.name);
                SetProperty(wrapper, "InstantImageTargetTexture", texture);
                SetProperty(wrapper, "Width", TargetSize * texture.width / longest);
                SetProperty(wrapper, "Height", TargetSize * texture.height / longest);
                SetProperty(wrapper, "AspectRatio", (float)texture.height / texture.width);
            }
            editor.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Type sceneManager = FindTypeByName("VuforiaSceneManager");
            object manager = sceneManager?.GetProperty("Instance", Members)?.GetValue(null);
            sceneManager?.GetMethod("ApplyDataSetProperties", Members)?.Invoke(manager, null);
        }
        finally
        {
            Object.DestroyImmediate(editor);
        }

        Vector2 size = (Vector2)behaviourType.GetMethod("GetSize", Members)!.Invoke(behaviour, null);
        Debug.Log($"[Lab09] ImageTarget desde {Path.GetFileName(TargetImage)}: {size.x:F3} x {size.y:F3}");
        return target;
    }

    public static string DescribeImageTarget(GameObject target)
    {
        Type behaviourType = FindType("Vuforia.ImageTargetBehaviour");
        Component behaviour = target.GetComponent(behaviourType);
        if (behaviour == null)
            return null;
        object type = behaviourType.GetProperty("ImageTargetType", Members)!.GetValue(behaviour);
        var texture = (Texture2D)behaviourType.GetMethod("GetRuntimeTargetTexture", Members)!.Invoke(behaviour, null);
        Vector2 size = (Vector2)behaviourType.GetMethod("GetSize", Members)!.Invoke(behaviour, null);
        return $"{type} {(texture != null ? texture.name : "sin imagen")} {size.x:F3}x{size.y:F3}";
    }

    public static string TargetStatus(GameObject target)
    {
        Component behaviour = target.GetComponent(FindType("Vuforia.ObserverBehaviour"));
        object status = behaviour?.GetType().GetProperty("TargetStatus", Members)!.GetValue(behaviour);
        return status?.GetType().GetProperty("Status", Members)?.GetValue(status)?.ToString() ?? "desconocido";
    }

    // Hace lo mismo que Vuforia al reconocer el marcador (activa los renderers de sus hijos). Solo se usa para
    // las capturas automáticas: en batchmode el simulador no recibe imágenes y el marcador nunca se detecta.
    public static bool ShowTargetContent(GameObject target)
    {
        Component handler = target.GetComponent(FindType("DefaultObserverEventHandler"));
        MethodInfo found = handler?.GetType().GetMethod("OnTrackingFound", Members);
        found?.Invoke(handler, null);
        return found != null;
    }

    public static bool IsEngineRunning()
    {
        Type application = FindType("Vuforia.VuforiaApplication");
        object instance = application?.GetProperty("Instance", Members)?.GetValue(null);
        return instance != null && (bool)application.GetProperty("IsRunning", Members)!.GetValue(instance);
    }

    // ---------------------------------------------------------------- Utilidades

    private static GameObject CreateFromMenu(string menu, string name)
    {
        Selection.activeObject = null; // si hay algo seleccionado, Vuforia crea el objeto como hijo
        if (!EditorApplication.ExecuteMenuItem(menu))
            throw new InvalidOperationException("No se pudo ejecutar el menú " + menu);
        GameObject created = Selection.activeGameObject != null ? Selection.activeGameObject : GameObject.Find(name);
        if (created == null)
            throw new InvalidOperationException("El menú " + menu + " no creó " + name);
        created.name = name;
        Selection.activeObject = null;
        return created;
    }

    private static void SetProperty(object target, string name, object value)
    {
        PropertyInfo property = target.GetType().GetProperty(name, Members)
                                ?? throw new MissingMemberException(target.GetType().Name, name);
        property.SetValue(target, value);
    }

    private static Type FindTypeByName(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.GetName().Name.Contains("Vuforia"))
                continue;
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException error) { types = error.Types; }
            foreach (Type type in types)
                if (type != null && type.Name == name)
                    return type;
        }
        return null;
    }

    public static Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(fullName, false);
            if (type != null)
                return type;
        }
        return null;
    }
}
