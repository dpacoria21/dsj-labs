using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Path = System.IO.Path;

// Opt-in checks exercise the real agent, graph and Physics2D in the built player.
public class Lab07Verification : MonoBehaviour
{
    [Serializable] public class Check { public string name; public bool passed; public string detail; }
    [Serializable] public class Report { public List<Check> checks=new List<Check>(); public int failures; }
    Report report=new Report(); string output; Lab07Dashboard lab;
    void CheckResult(string name,bool passed,string detail="")
    {
        report.checks.Add(new Check {name=name,passed=passed,detail=detail});
        if(!passed)report.failures++;
        Debug.Log("LAB07_CHECK "+name+" "+passed+" "+detail);
    }
    IEnumerator Start()
    {
        var args=Environment.GetCommandLineArgs();
        if(Array.IndexOf(args,"--lab07-verify")<0)yield break;
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Verificacion"));
        int index=Array.IndexOf(args,"--lab-output"); if(index>=0 && index+1<args.Length)output=args[index+1];
        Directory.CreateDirectory(output); lab=GetComponent<Lab07Dashboard>(); lab.player.automated=true;
        yield return new WaitForSeconds(1);
        var grid=AstarPath.active.data.gridGraph;
        CheckResult("Grid 2D y fisica 2D",grid!=null && grid.is2D && grid.collision.use2D);
        CheckResult("Capa Environment",grid.collision.mask==(1<<8));
        int blocked=0,walkable=0; grid.GetNodes(n=>{if(n.Walkable)walkable++;else blocked++;});
        CheckResult("Escaneo detecta paredes y suelo",blocked>0 && walkable>0,"blocked="+blocked+", walkable="+walkable);
        CheckResult("Destino Player",lab.enemy.GetComponent<AIDestinationSetter>().target==lab.player.transform);
        CheckResult("Graphics orientado por EnemyMovement",lab.enemy.transform.Find("Graphics").GetComponent<EnemyMovement>().aiPath==lab.enemy);
        Capture("01_escena_y_ruta");
        var targets=new[]{new Vector2(2,-1),new Vector2(9,-4),new Vector2(-9,-4),new Vector2(0,5)};
        int overlaps=0,wallCrossings=0;float distanceTravelled=0;
        for(int i=0;i<targets.Length;i++)
        {
            lab.player.GetComponent<Rigidbody2D>().position=targets[i];
            lab.player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            Physics2D.SyncTransforms(); lab.enemy.SearchPath();
            float deadline=Time.time+22;Vector3 previous=lab.enemy.position; bool captured=false;
            while(Time.time<deadline && Vector2.Distance(lab.enemy.position,targets[i])>.65f)
            {
                yield return new WaitForFixedUpdate();
                var current=lab.enemy.position;distanceTravelled+=Vector3.Distance(previous,current);
                if(Physics2D.OverlapCircle(current,.24f,1<<8))overlaps++;
                if(Physics2D.Linecast(previous,current,1<<8).collider!=null)wallCrossings++;
                previous=current;
                if(i==1 && !captured && Time.time>deadline-20) {Capture("02_persecucion");captured=true;}
            }
            CheckResult("Llegada destino "+i,Vector2.Distance(lab.enemy.position,targets[i])<.7f,"distance="+Vector2.Distance(lab.enemy.position,targets[i]));
            Capture("destino_"+i);
        }
        CheckResult("Enemigo no atraviesa paredes",overlaps==0 && wallCrossings==0,"overlaps="+overlaps+", crossings="+wallCrossings);
        CheckResult("Agente se desplaza y recalcula",distanceTravelled>25 && lab.pathCount>10,"distance="+distanceTravelled+", paths="+lab.pathCount);
        var playerBody=lab.player.GetComponent<Rigidbody2D>();playerBody.position=new Vector2(10,0);lab.player.testInput=Vector2.right;
        yield return new WaitForSeconds(2);lab.player.testInput=Vector2.zero;
        CheckResult("Jugador respeta pared derecha",playerBody.position.x<11.5f && playerBody.position.x>10.8f,"x="+playerBody.position.x);
        lab.ResetPositions();yield return new WaitForSeconds(.5f);Capture("03_estado_final");
        File.WriteAllText(Path.Combine(output,"resultados.json"),JsonUtility.ToJson(report,true));
        Application.Quit(report.failures==0?0:1);
    }
    void Capture(string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var camera=Camera.main;var rt=new RenderTexture(1400,900,24);camera.targetTexture=rt;
        var oldRect=camera.rect;camera.rect=new Rect(0,0,1,1);camera.Render();
        var old=RenderTexture.active;RenderTexture.active=rt;
        var image=new Texture2D(1400,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,900),0,0);image.Apply();
        File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());
        camera.targetTexture=null;camera.rect=oldRect;RenderTexture.active=old;Destroy(image);Destroy(rt);
    }
}
