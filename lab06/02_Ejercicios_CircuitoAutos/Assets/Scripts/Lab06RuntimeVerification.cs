using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pruebas opt-in en el ejecutable. Simula PhysX real con pasos de 0.02 s.
public class Lab06RuntimeVerification : MonoBehaviour
{
    readonly List<string> failures=new List<string>();
    readonly List<string> results=new List<string>();
    string output; int checks;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartIfRequested()
    {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--lab06-verify")<0 || FindFirstObjectByType<Lab06RuntimeVerification>()!=null)return;
        var o=new GameObject("Verificacion automatizada");DontDestroyOnLoad(o);o.AddComponent<Lab06RuntimeVerification>();
    }
    void Check(bool ok,string text){ checks++; if(!ok)failures.Add(text); }
    void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)failures.Add(text);}
    IEnumerator Start()
    {
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--lab-output");
        output=i>=0?Path.GetFullPath(args[i+1]):Path.Combine(Application.persistentDataPath,"Verificacion");
        Directory.CreateDirectory(output);Application.logMessageReceived+=Log;
        QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
        Physics.simulationMode=SimulationMode.Script;
        for(int scene=0;scene<SceneManager.sceneCountInBuildSettings;scene++){
            yield return SceneManager.LoadSceneAsync(scene);yield return null;
            var hud=FindFirstObjectByType<LabDashboard>();hud.AutomatedVerification=true;
            var route=hud.circuit;Check(route.IsValid,"Ruta valida "+scene);
            var tanks=FindObjectsByType<FollowWP>(FindObjectsSortMode.None);
            var cars=FindObjectsByType<CircuitVehicle>(FindObjectsSortMode.None);
            Check(tanks.Length+cars.Length==4,"Cuatro vehiculos "+scene);
            foreach(var t in tanks){t.ManualSimulation=true;t.Initialize();Check(t.waypoints.Length==route.Count,"Arreglo de palmeras completo");}
            foreach(var car in cars){car.ManualSimulation=true;car.Initialize();car.Body.interpolation=RigidbodyInterpolation.None;
                Check(!car.Body.isKinematic&&car.GetComponent<BoxCollider>()!=null,"Rigidbody y collider dinamicos");
                Check(car.Tracker.Target.GetComponent<Collider>()==null||!car.Tracker.Target.GetComponent<Collider>().enabled,"Tracker sin colision");}
            Capture("escena-"+scene+"-inicio.png");
            int impulseProgress=0;float maxGap=0;float minY=100;bool recovered=false;
            for(int tick=0;tick<12000;tick++){
                if(tick==1000&&cars.Length>0){impulseProgress=cars[0].Tracker.PassedWaypoints;cars[0].Nudge();}
                foreach(var tank in tanks)tank.Step(.02f);
                foreach(var car in cars)car.Step(.02f);
                Physics.Simulate(.02f);
                foreach(var car in cars){minY=Mathf.Min(minY,car.Body.position.y);Vector3 delta=car.Body.position-car.Tracker.Target.position;delta.y=0;maxGap=Mathf.Max(maxGap,delta.magnitude);}
                if(tick==2500&&cars.Length>0)recovered=cars[0].Tracker.PassedWaypoints>impulseProgress+route.Count/2;
                if(tick==500)Capture("escena-"+scene+"-movimiento.png");
                if(tick%100==0)yield return null;
            }
            foreach(var t in tanks){Check(t.Laps>=2,"Tanque completa circuitos: "+t.name+" vueltas="+t.Laps);results.Add(t.name+" vueltas="+t.Laps+" waypoints="+t.PassedWaypoints);}
            foreach(var car in cars){Check(car.Tracker.Laps>=2,"Vehiculo completa circuitos: "+car.name+" vueltas="+car.Tracker.Laps);results.Add(car.name+" vueltas_tracker="+car.Tracker.Laps+" esperas="+car.Tracker.WaitSteps+" contactos="+car.CollisionCount);}
            if(cars.Length>0){
                Check(recovered,"Recuperacion tras impulso en 30 segundos");Check(minY>-.5f,"No cae a traves del suelo");
                // Contacto forzado para probar que la fisica puede desviar un vehiculo.
                hud.ResetAll();Physics.SyncTransforms();
                cars[1].Body.position=cars[0].Body.position+cars[0].transform.right;
                Physics.SyncTransforms();
                for(int tick=0;tick<30;tick++){foreach(var car in cars)car.Step(.02f);Physics.Simulate(.02f);}
                Check(cars[0].CollisionCount+cars[1].CollisionCount>0,"Colision real vehiculo-vehiculo");
                // Un coche detenido no deja escapar indefinidamente al tracker.
                var tracker=cars[2].Tracker;tracker.ResetTracker();Vector3 still=tracker.Target.position;
                for(int tick=0;tick<1000;tick++)tracker.Advance(still,20,.02f);
                Check(Vector3.Distance(still,tracker.Target.position)<=tracker.lookAhead+.01f,"Limite de anticipacion");
                results.Add("max_distancia_tracker="+maxGap.ToString("F3")+" minY="+minY.ToString("F3")+" recuperacion="+recovered);
            }
            hud.ResetAll();
            foreach(var t in tanks)Check(t.Laps==0&&t.PassedWaypoints==0,"Reinicio de tanque");
            foreach(var car in cars)Check(car.Tracker.Laps==0&&car.Body.linearVelocity.sqrMagnitude<.001f,"Reinicio de vehiculo");
            // Ausencia de ruta: detenerse de forma segura sin excepciones.
            var empty=new GameObject("Prueba ruta vacia").AddComponent<FollowWP>();empty.ManualSimulation=true;empty.Step(.02f);Check(empty.transform.position==Vector3.zero,"Ruta vacia no mueve");Destroy(empty.gameObject);
        }
        File.WriteAllLines(Path.Combine(output,"metricas.txt"),results);
        File.WriteAllText(Path.Combine(output,"resultado.txt"),"Unity "+Application.unityVersion+"\nSimulacion: 240 segundos por escena; dt 0.02; PhysX real.\nCapturas: Camera.Render, sin interfaz IMGUI.\nComprobaciones: "+checks+"\nErrores: "+failures.Count+"\n"+string.Join("\n",failures));
        Application.logMessageReceived-=Log;
        Debug.Log("LAB06_VERIFICATION_FINISHED checks="+checks+" failures="+failures.Count);
        Application.Quit(failures.Count==0?0:2);
    }
    void Capture(string name)
    {
        var cam=Camera.main;var prev=cam.targetTexture;var rect=cam.rect;var active=RenderTexture.active;
        var rt=new RenderTexture(1440,900,24);var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);
        cam.targetTexture=rt;cam.rect=new Rect(0,0,1,1);cam.Render();RenderTexture.active=rt;
        tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());
        cam.targetTexture=prev;cam.rect=rect;RenderTexture.active=active;rt.Release();Destroy(rt);Destroy(tex);
    }
}
