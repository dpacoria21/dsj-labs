using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class Lab06Builder
{
    const string Activity = "Assets/Scenes/01_Actividades_TankRacing.unity";
    const string Tutorial = "Assets/Scenes/01_Tutorial_CircuitoTracker.unity";
    const string Cars = "Assets/Scenes/02_Circuito_Autos.unity";
    static bool Exercises => Application.dataPath.Contains("02_Ejercicios");
    static readonly Color[] Colors = { new Color(.12f,.83f,.66f),new Color(.24f,.58f,1f),new Color(1f,.3f,.23f),new Color(1f,.78f,.15f) };
    static readonly Vector3[] DesertPoints = {
        new Vector3(-38,0,-24), new Vector3(-42,0,5), new Vector3(-29,0,28),
        new Vector3(-2,0,34), new Vector3(28,0,25), new Vector3(42,0,6),
        new Vector3(34,0,-21), new Vector3(8,0,-32), new Vector3(-17,0,-32)
    };
    [MenuItem("Laboratorio 06/Regenerar escenas del laboratorio")]
    public static void Create()
    {
        Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Generated");
        Directory.CreateDirectory("Assets/Prefabs"); Directory.CreateDirectory("Verificacion");
        EditorSettings.serializationMode=SerializationMode.ForceText;
        EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode3D;
        PlayerSettings.productName=Exercises?"LAB06 Circuito de Autos":"LAB06 Waypoints Tanques";
        PlayerSettings.companyName="DSJ UNSA";
        PlayerSettings.defaultScreenWidth=1440; PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed; PlayerSettings.runInBackground=true;
        PlayerSettings.SplashScreen.show=false; PlayerSettings.colorSpace=ColorSpace.Linear;
        var ps = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        ps.FindProperty("activeInputHandler").intValue=0; ps.ApplyModifiedPropertiesWithoutUndo();
        QualitySettings.antiAliasing=4; QualitySettings.shadowDistance=180;
        QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowResolution=ShadowResolution.High;
        Physics.gravity=new Vector3(0,-9.81f,0); Time.fixedDeltaTime=.02f;
        CreateDesert(Exercises);
        if (Exercises) CreateCars();
        EditorBuildSettings.scenes=(Exercises?new[]{Cars,Tutorial}:new[]{Activity}).Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
        EditorSceneManager.OpenScene(Exercises?Cars:Activity);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("LAB06_SCENES_READY "+(Exercises?"autos + tutorial tracker":"actividades"));
    }
    static Material Mat(string name,Color color,float metallic=0)
    {
        string path="Assets/Generated/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",metallic>0?.55f:.12f);
        EditorUtility.SetDirty(m);return m;
    }
    static GameObject Shape(string name,PrimitiveType type,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool solid=false)
    {
        var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);
        o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solid) UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
        return o;
    }
    static void Environment()
    {
        foreach(var old in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(old.gameObject);
        foreach(var old in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(old.gameObject);
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.65f,.76f,.87f);
        RenderSettings.ambientEquatorColor=new Color(.44f,.47f,.5f);
        RenderSettings.ambientGroundColor=new Color(.27f,.28f,.26f);
        RenderSettings.fog=false; RenderSettings.skybox=null;
        var sun=new GameObject("Sol").AddComponent<Light>();sun.type=LightType.Directional;
        sun.intensity=1.15f;sun.color=new Color(1,.94f,.83f);sun.shadows=LightShadows.Soft;
        sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadowStrength=.65f;
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.52f,.67f,.75f);
        camera.transform.position=new Vector3(65,92,-80);camera.transform.LookAt(new Vector3(0,0,0));
        camera.fieldOfView=52;camera.farClipPlane=400;camera.nearClipPlane=.3f;
        camera.rect=new Rect(.27f,0,.73f,1);
        camera.gameObject.AddComponent<AudioListener>();camera.gameObject.AddComponent<LabCamera>();
    }
    static WaypointCircuit Route(Vector3[] positions,string name,bool palms)
    {
        var route=new GameObject(name).AddComponent<WaypointCircuit>();route.points=new Transform[positions.Length];
        var palm=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/PalmTree.fbx");
        for(int i=0;i<positions.Length;i++){
            GameObject p=palms?(GameObject)PrefabUtility.InstantiatePrefab(palm):new GameObject();
            p.name=palms?"PalmTree_WP_"+i.ToString("00"):"WP_"+i.ToString("00");
            p.transform.SetParent(route.transform);p.transform.position=positions[i];
            if(palms){p.transform.localScale=Vector3.one*.75f;foreach(var c in p.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);}
            route.points[i]=p.transform;
        }
        var line=route.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.loop=true;
        line.widthMultiplier=.16f;line.positionCount=positions.Length;
        line.SetPositions(positions.Select(p=>p+Vector3.up*.16f).ToArray());
        line.sharedMaterial=Mat("Ruta",new Color(.2f,.9f,.85f));
        return route;
    }
    static LabDashboard Dashboard(bool activities,string subtitle,WaypointCircuit circuit)
    {
        var hud=new GameObject("Panel del laboratorio").AddComponent<LabDashboard>();hud.activities=activities;
        hud.subtitle=subtitle;hud.circuit=circuit;hud.view=Camera.main.GetComponent<LabCamera>();
        return hud;
    }
    static void CreateDesert(bool tracker)
    {
        EditorSceneManager.OpenScene("Assets/TankRacing.unity");
        Environment();
        var ground=GameObject.Find("Cube");ground.name="Suelo del desierto";
        ground.transform.position=new Vector3(0,-.55f,0);ground.transform.localScale=new Vector3(125,1,108);
        var oldPalm=GameObject.Find("PalmTree");if(oldPalm!=null)UnityEngine.Object.DestroyImmediate(oldPalm);
        var route=Route(DesertPoints,"Circuito de 9 palmeras",true);
        var hud=Dashboard(!tracker,tracker?"Ejercicio 1 · Física y objetivo auxiliar":"Actividades 1–15 · Tanques y palmeras",route);
        var names=new[]{"GreenTank","BlueTank","RedTank","YellowTank"};
        var follows=new FollowWP[4];var drivers=new CircuitVehicle[4];
        for(int i=0;i<4;i++){
            var tank=GameObject.Find(names[i]);
            if(tank==null)throw new Exception("Falta recurso original "+names[i]);
            int start=i*2;
            var forward=(DesertPoints[(start+1)%9]-DesertPoints[start]).normalized;
            tank.transform.SetPositionAndRotation(DesertPoints[start]+Vector3.up*.06f,Quaternion.LookRotation(forward));
            if(tracker){
                var body=tank.AddComponent<Rigidbody>();body.mass=800;body.linearDamping=.15f;
                body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
                body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                var box=tank.AddComponent<BoxCollider>();box.center=new Vector3(0,.85f,0);box.size=new Vector3(2.6f,1.7f,3.6f);
                var t=tank.AddComponent<ProgressTracker>();t.circuit=route;t.startWaypoint=(start+1)%9;t.lookAhead=5;
                var v=tank.AddComponent<CircuitVehicle>();v.speed=10+i*1.3f;v.rotSpeed=6;v.vehicleColor=Colors[i];drivers[i]=v;
            }else{
                var f=tank.AddComponent<FollowWP>();f.waypoints=route.points.Select(p=>p.gameObject).ToArray();
                f.startWaypoint=(start+1)%9;f.speed=10+i*1.5f;f.rotSpeed=6+i;f.accuracy=3;f.vehicleColor=Colors[i];follows[i]=f;
            }
        }
        hud.tanks=follows;hud.vehicles=drivers;
        // Senalizacion minima del circuito, sin colliders en las palmeras.
        for(int i=0;i<route.Count;i++)WorldText("WP "+(i+1),DesertPoints[i]+new Vector3(2,.2f,0),1.4f,new Color(.15f,.2f,.18f));
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),tracker?Tutorial:Activity);
    }
    static Vector3[] RoadPoints()
    {
        Vector3[] control={new Vector3(-40,0,-24),new Vector3(-42,0,15),new Vector3(-20,0,32),new Vector3(19,0,28),new Vector3(43,0,12),new Vector3(38,0,-20),new Vector3(8,0,-31),new Vector3(-19,0,-27)};
        var result=new Vector3[control.Length*8];
        for(int i=0;i<control.Length;i++)for(int j=0;j<8;j++){
            float t=j/8f;var a=control[(i+7)%8];var b=control[i];var c=control[(i+1)%8];var d=control[(i+2)%8];
            result[i*8+j]=.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t)+Vector3.up*.1f;
        }
        return result;
    }
    static void Road(Vector3[] points)
    {
        var road=new GameObject("Asfalto del circuito");var mesh=new Mesh {name="Circuito cerrado"};
        int n=points.Length;var vertices=new Vector3[(n+1)*2];var uv=new Vector2[vertices.Length];var tri=new int[n*6];
        var red=Mat("Piano rojo",new Color(.85f,.12f,.09f));var white=Mat("Blanco",new Color(.94f,.95f,.89f));
        var dark=Mat("Asfalto",new Color(.095f,.12f,.15f));
        var curb=new GameObject("Pianos rojo y blanco").transform;
        var markings=new GameObject("Marcas de pista").transform;
        for(int i=0;i<=n;i++){
            int k=i%n;var tangent=(points[(k+1)%n]-points[(k+n-1)%n]).normalized;
            var side=Vector3.Cross(Vector3.up,tangent).normalized;
            vertices[i*2]=points[k]+side*6;vertices[i*2+1]=points[k]-side*6;
            uv[i*2]=new Vector2(0,i);uv[i*2+1]=new Vector2(1,i);
            if(i==n)continue;
            int s=i*2;int t=i*6;tri[t]=s;tri[t+1]=s+1;tri[t+2]=s+2;tri[t+3]=s+1;tri[t+4]=s+3;tri[t+5]=s+2;
            float length=Vector3.Distance(points[k],points[(k+1)%n]);
            var middle=(points[k]+points[(k+1)%n])/2;var rotation=Quaternion.LookRotation(points[(k+1)%n]-points[k]);
            foreach(int sign in new[]{-1,1}){
                var strip=Shape("Piano "+i,PrimitiveType.Cube,middle+side*(6.25f*sign),new Vector3(.7f,.12f,length+.12f),i%2==0?red:white,curb);
                strip.transform.rotation=rotation;
            }
            if(i%2==0){var dash=Shape("Linea discontinua",PrimitiveType.Cube,points[k]+Vector3.up*.02f,new Vector3(.17f,.02f,1.4f),white,markings);dash.transform.rotation=rotation;}
        }
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path="Assets/Generated/Circuito.mesh";if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null)AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh,path);road.AddComponent<MeshFilter>().sharedMesh=mesh;road.AddComponent<MeshRenderer>().sharedMaterial=dark;
        // El suelo es continuo y plano; el asfalto es una capa visual sobre el collider.
        var checker=new GameObject("Linea de salida").transform;
        checker.position=points[1]+Vector3.up*.04f;checker.rotation=Quaternion.LookRotation(points[2]-points[1]);
        for(int x=0;x<12;x++)for(int z=0;z<2;z++)Shape("Cuadro",PrimitiveType.Cube,new Vector3(x-5.5f,0,z-.5f),new Vector3(1,.025f,1),(x+z)%2==0?white:dark,checker);
    }
    static GameObject MakeCar(int index)
    {
        var car=new GameObject(new[]{"01 Jade","02 Azul","03 Coral","04 Ambar"}[index]);
        var root=car.transform;var paint=Mat("Carroceria "+index,Colors[index],.35f);
        var black=Mat("Neumatico",new Color(.025f,.032f,.04f));var glass=Mat("Cristal",new Color(.07f,.16f,.23f),.65f);
        var metal=Mat("Aleacion",new Color(.55f,.6f,.65f),.8f);var white=Mat("Blanco",new Color(.94f,.95f,.89f));
        Shape("Chasis",PrimitiveType.Cube,new Vector3(0,.5f,0),new Vector3(2.25f,.5f,4.25f),paint,root);
        Shape("Capo",PrimitiveType.Cube,new Vector3(0,.83f,1.2f),new Vector3(2.05f,.22f,1.6f),paint,root);
        Shape("Cabina",PrimitiveType.Cube,new Vector3(0,1.02f,-.3f),new Vector3(1.7f,.6f,1.9f),glass,root);
        Shape("Techo",PrimitiveType.Cube,new Vector3(0,1.35f,-.4f),new Vector3(1.76f,.12f,1.4f),paint,root);
        Shape("Franja",PrimitiveType.Cube,new Vector3(0,.955f,1.2f),new Vector3(.3f,.02f,1.58f),white,root);
        Shape("Aleron",PrimitiveType.Cube,new Vector3(0,1.06f,-1.9f),new Vector3(2.45f,.12f,.4f),black,root);
        foreach(int side in new[]{-1,1}){
            Shape("Soporte aleron",PrimitiveType.Cube,new Vector3(side*.75f,.85f,-1.9f),new Vector3(.12f,.4f,.16f),black,root);
            Shape("Faro",PrimitiveType.Cube,new Vector3(side*.75f,.69f,2.14f),new Vector3(.5f,.18f,.05f),white,root);
            Shape("Piloto",PrimitiveType.Cube,new Vector3(side*.75f,.69f,-2.14f),new Vector3(.5f,.18f,.05f),Mat("Luz roja",Color.red),root);
            foreach(float z in new[]{-1.35f,1.35f}){
                var wheel=Shape("Rueda",PrimitiveType.Cylinder,new Vector3(side*1.12f,.43f,z),new Vector3(.86f,.2f,.86f),black,root);wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                var rim=Shape("Llanta",PrimitiveType.Cylinder,new Vector3(side*1.33f,.43f,z),new Vector3(.47f,.025f,.47f),metal,root);rim.transform.localRotation=Quaternion.Euler(0,0,90);
            }
        }
        var box=car.AddComponent<BoxCollider>();box.center=new Vector3(0,.65f,0);box.size=new Vector3(2.35f,1.3f,4.3f);
        var body=car.AddComponent<Rigidbody>();body.mass=1100;body.linearDamping=.1f;body.angularDamping=2;
        body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
        body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        car.AddComponent<ProgressTracker>();var driver=car.AddComponent<CircuitVehicle>();driver.speed=13+index*1.5f;driver.rotSpeed=5;driver.vehicleColor=Colors[index];
        PrefabUtility.SaveAsPrefabAsset(car,"Assets/Prefabs/Auto_"+(index+1)+".prefab");
        return car;
    }
    static void WorldText(string text,Vector3 position,float size,Color color)
    {
        var t=new GameObject(text).AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size*.1f;t.anchor=TextAnchor.MiddleCenter;t.color=color;
        t.transform.position=position;t.transform.rotation=Quaternion.Euler(90,0,0);
    }
    static void CreateCars()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Environment();
        Shape("Terreno",PrimitiveType.Cube,new Vector3(0,-.55f,0),new Vector3(138,1,118),Mat("Cesped",new Color(.23f,.42f,.29f)),null,true);
        Vector3[] points=RoadPoints();Road(points);var route=Route(points,"Circuito 64 waypoints",false);route.showRoute=false;
        route.GetComponent<LineRenderer>().enabled=false;
        var drivers=new CircuitVehicle[4];
        for(int i=0;i<4;i++){
            var car=MakeCar(i);int start=i*16;
            car.transform.position=points[start]+Vector3.up*.05f;car.transform.rotation=Quaternion.LookRotation(points[(start+1)%64]-points[start]);
            var tracker=car.GetComponent<ProgressTracker>();tracker.circuit=route;tracker.startWaypoint=(start+1)%64;tracker.lookAhead=5;
            drivers[i]=car.GetComponent<CircuitVehicle>();
        }
        var dark=Mat("Estructuras",new Color(.12f,.19f,.22f));var pale=Mat("Hormigon",new Color(.62f,.67f,.64f));
        var scene=new GameObject("Entorno del autodromo").transform;
        Shape("Edificio de boxes",PrimitiveType.Cube,new Vector3(3,2,43),new Vector3(45,4,8),pale,scene);
        Shape("Cubierta boxes",PrimitiveType.Cube,new Vector3(3,4.2f,43),new Vector3(47,.4f,9),dark,scene);
        for(int i=0;i<7;i++)Shape("Puerta box",PrimitiveType.Cube,new Vector3(-15+i*6,1.4f,38.9f),new Vector3(4,2.7f,.12f),dark,scene);
        for(int row=0;row<4;row++)Shape("Grada",PrimitiveType.Cube,new Vector3(0,row*.55f,-43-row*1.3f),new Vector3(38,.6f,1.3f),row%2==0?dark:pale,scene);
        var tree=Mat("Arboles",new Color(.1f,.3f,.19f));var wood=Mat("Tronco",new Color(.31f,.23f,.15f));
        for(int i=0;i<18;i++){
            float a=i*Mathf.PI*2/18;var pos=new Vector3(Mathf.Cos(a)*60,0,Mathf.Sin(a)*51);
            if(Mathf.Abs(pos.x)<30&&pos.z>35)continue;
            Shape("Tronco",PrimitiveType.Cylinder,pos+Vector3.up*1.4f,new Vector3(.7f,1.4f,.7f),wood,scene);
            Shape("Copa",PrimitiveType.Sphere,pos+Vector3.up*3.4f,new Vector3(4.1f,4.3f,4.1f),tree,scene);
        }
        WorldText("CIRCUIT LAB",new Vector3(0,.02f,4),6,new Color(.82f,.9f,.78f));
        WorldText("DSJ  /  06",new Vector3(0,.02f,-3),2.5f,new Color(.65f,.77f,.63f));
        var hud=Dashboard(false,"Ejercicio 2 · Circuito de autos con tracker",route);hud.vehicles=drivers;hud.tanks=new FollowWP[0];
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),Cars);
    }
    public static void Build()
    {
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Lab06.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
        });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build: "+report.summary.result);
        Debug.Log("LAB06_BUILD_OK "+report.summary.totalSize);
    }
    public static void CreateAndBuild() { Create(); Build(); }
}
