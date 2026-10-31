using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Pathfinding;

public static class Lab07Builder
{
    public const string ScenePath = "Assets/Scenes/01_Actividades_Pathfinding.unity";
    [MenuItem("Laboratorio 07/Regenerar escena de actividades")]
    public static void Create()
    {
        Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Generated");
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        EditorSettings.serializationMode = SerializationMode.ForceText;
        PlayerSettings.companyName = "DSJ UNSA"; PlayerSettings.productName = "LAB07 Pathfinding 2D";
        PlayerSettings.defaultScreenWidth=1440; PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed; PlayerSettings.runInBackground=true;
        PlayerSettings.SplashScreen.show=false;
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue=0; settings.ApplyModifiedPropertiesWithoutUndo();
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue="Environment"; tags.ApplyModifiedPropertiesWithoutUndo();
        Physics2D.gravity=Vector2.zero;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/Player.png");
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=195; importer.alphaIsTransparency=true;
        importer.SaveAndReimport();
        var character = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player.png");
        var tex = new Texture2D(1,1); tex.SetPixel(0,0,Color.white); tex.Apply();
        File.WriteAllBytes("Assets/Generated/Square.png",tex.EncodeToPNG()); AssetDatabase.Refresh();
        var si = (TextureImporter)AssetImporter.GetAtPath("Assets/Generated/Square.png");
        si.textureType=TextureImporterType.Sprite; si.spriteImportMode=SpriteImportMode.Single;
        si.spritePixelsPerUnit=1; si.SaveAndReimport();
        var square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Generated/Square.png");
        var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag="MainCamera";
        camera.orthographic=true; camera.orthographicSize=10.3f; camera.transform.position=new Vector3(0,0,-10);
        camera.rect=new Rect(.23f,0,.77f,1); camera.backgroundColor=new Color(.045f,.07f,.11f);
        camera.clearFlags=CameraClearFlags.SolidColor; camera.gameObject.AddComponent<AudioListener>();
        var environment=new GameObject("Environment");
        Wall("wall_Bottom",new Vector2(0,-7),new Vector2(24.5f,.5f),square,environment.transform);
        Wall("wall_Top",new Vector2(0,7),new Vector2(24.5f,.5f),square,environment.transform);
        Wall("wall_Left",new Vector2(-12,0),new Vector2(.5f,14),square,environment.transform);
        Wall("wall_Right",new Vector2(12,0),new Vector2(.5f,14),square,environment.transform);
        Wall("wall_InnerLeft",new Vector2(-6,0),new Vector2(.5f,6),square,environment.transform);
        Wall("wall_InnerRight",new Vector2(6,0),new Vector2(.5f,6),square,environment.transform);
        Wall("wall_InnerTop",new Vector2(0,3.5f),new Vector2(6,.5f),square,environment.transform);
        Wall("wall_InnerBottom",new Vector2(0,-3.5f),new Vector2(6,.5f),square,environment.transform);
        var gridVisual=new GameObject("Cuadricula visual");
        for(int x=-11;x<=11;x++) Visual("GridX",new Vector3(x,0,1),new Vector2(.012f,13.5f),square,new Color(.09f,.14f,.2f),gridVisual.transform);
        for(int y=-6;y<=6;y++) Visual("GridY",new Vector3(0,y,1),new Vector2(23.5f,.012f),square,new Color(.09f,.14f,.2f),gridVisual.transform);
        var player=Actor("Player",new Vector2(2,-1),character,new Color(.3f,.8f,1f),square);
        var body=player.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.freezeRotation=true;
        body.collisionDetectionMode=CollisionDetectionMode2D.Continuous; body.interpolation=RigidbodyInterpolation2D.Interpolate;
        player.AddComponent<CircleCollider2D>().radius=.35f;
        var control=player.AddComponent<PlayerScript>(); control.graphics=player.transform.Find("Graphics");
        var enemy=Actor("Enemy",new Vector2(-9,4),character,new Color(1f,.43f,.18f),square);
        enemy.AddComponent<Seeker>();
        var ai=enemy.AddComponent<AIPath>(); ai.orientation=OrientationMode.YAxisForward; ai.updateRotation=false;
        ai.enableRotation=false; ai.slowWhenNotFacingTarget=false;
        ai.pickNextWaypointDist=.3f; ai.maxAcceleration=25f;
        ai.maxSpeed=2.8f; ai.radius=.35f; ai.height=1; ai.gravity=Vector3.zero; ai.repathRate=.2f;
        ai.endReachedDistance=.35f; ai.slowdownDistance=.8f; ai.constrainInsideGraph=true;
        enemy.AddComponent<AIDestinationSetter>().target=player.transform;
        enemy.transform.Find("Graphics").gameObject.AddComponent<EnemyMovement>().aiPath=ai;
        var astar=new GameObject("PathFinding").AddComponent<AstarPath>();
        astar.ConfigureReferencesInternal();
        astar.data.FindGraphTypes(); astar.data.Awake();
        var graph=astar.data.AddGraph(typeof(GridGraph)) as GridGraph;
        graph.is2D=true; graph.center=Vector3.zero; graph.SetDimensions(98,58,.25f);
        graph.collision.use2D=true; graph.collision.heightCheck=false;
        graph.collision.type=Pathfinding.ColliderType.Sphere; graph.collision.diameter=3.2f;
        graph.collision.mask=1<<8; graph.cutCorners=false;
        Physics2D.SyncTransforms(); astar.Scan();
        astar.data.SetData(astar.data.SerializeGraphs(new Pathfinding.Serialization.SerializeSettings { nodes=true }));
        EditorUtility.SetDirty(astar);
        var route=new GameObject("Ruta A estrella").AddComponent<LineRenderer>();
        route.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Generated/Route.mat");
        if(route.sharedMaterial==null) {
            route.sharedMaterial=new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(route.sharedMaterial,"Assets/Generated/Route.mat");
        }
        route.startColor=new Color(.2f,1f,.65f); route.endColor=new Color(.2f,1f,.65f);
        route.widthMultiplier=.08f; route.sortingOrder=2; route.numCornerVertices=3;
        var hud=new GameObject("Panel del laboratorio").AddComponent<Lab07Dashboard>();
        hud.player=control; hud.enemy=ai; hud.route=route;
        hud.gameObject.AddComponent<Lab07Verification>();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        Selection.activeGameObject=astar.gameObject; SceneView.lastActiveSceneView?.FrameSelected();
        AssetDatabase.SaveAssets(); Debug.Log("LAB07_SCENE_READY");
    }
    static void Wall(string name,Vector2 position,Vector2 size,Sprite sprite,Transform parent)
    {
        var o=Visual(name,position,size,sprite,new Color(.85f,.91f,.95f),parent);
        o.layer=8; o.AddComponent<BoxCollider2D>();
    }
    static GameObject Visual(string name,Vector3 position,Vector2 size,Sprite sprite,Color color,Transform parent)
    {
        var o=new GameObject(name); o.transform.SetParent(parent); o.transform.position=position; o.transform.localScale=size;
        var sr=o.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.color=color; return o;
    }
    static GameObject Actor(string name,Vector2 position,Sprite sprite,Color color,Sprite square)
    {
        var actor=new GameObject(name); actor.transform.position=position;
        var g=Visual("Graphics",position,Vector2.one*1.2f,sprite,Color.white,actor.transform); g.GetComponent<SpriteRenderer>().sortingOrder=4;
        // A colored marker distinguishes the two instances of the supplied character.
        var marker=Visual("Identificador",(Vector3)position+new Vector3(0,.7f,0),new Vector2(.8f,.1f),square,color,actor.transform);
        marker.GetComponent<SpriteRenderer>().sortingOrder=5; return actor;
    }
    public static void Build()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Directory.CreateDirectory("Builds");
        var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Lab07.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(result.summary.result!=BuildResult.Succeeded) throw new Exception("Build failed: "+result.summary.result);
        Debug.Log("LAB07_BUILD_READY");
    }
    public static void CreateAndBuild() { Create(); Build(); }
}
