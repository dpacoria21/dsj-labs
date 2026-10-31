using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Lab07ExerciseVerification : MonoBehaviour
{
    [Serializable] public class Check {public string name;public bool passed;public string detail;}
    [Serializable] public class Report {public List<Check> checks=new List<Check>();public int failures;}
    Report report=new Report();
    void Verify(string name,bool passed,string detail="")
    {
        report.checks.Add(new Check{name=name,passed=passed,detail=detail});if(!passed)report.failures++;
        Debug.Log("LAB07_EX1_CHECK "+name+" "+passed+" "+detail);
    }
    IEnumerator Start()
    {
        var args=Environment.GetCommandLineArgs();
        if(Array.IndexOf(args,"--lab07-exercise-verify")<0)yield break;
        var lab=GetComponent<Lab07Dashboard>();lab.player.automated=true;
        yield return new WaitForSeconds(1);
        var agents=new[]{lab.enemy,lab.secondEnemy};
        var walls=GameObject.Find("Environment").GetComponentsInChildren<BoxCollider2D>();
        Verify("12 paredes con collider y capa Environment",walls.Length==12 && Array.TrueForAll(walls,w=>w.gameObject.layer==8),"count="+walls.Length);
        Verify("Dos agentes AIPath",FindObjectsByType<AIPath>(FindObjectsSortMode.None).Length==2);
        foreach(var agent in agents)
        {
            Verify(agent.name+" destino Player",agent.GetComponent<AIDestinationSetter>().target==lab.player.transform);
            Verify(agent.name+" orientacion Graphics",agent.transform.Find("Graphics").GetComponent<EnemyMovement>().aiPath==agent);
        }
        var graph=AstarPath.active.data.gridGraph;
        foreach(var wall in walls)
            Verify(wall.name+" bloquea nodos",!graph.GetNearest(wall.transform.position).node.Walkable);
        int overlaps=0,crossings=0;float[] travelled={0,0};int[] paths={0,0};
        agents[0].GetComponent<Seeker>().pathCallback+=p=>{if(!p.error)paths[0]++;};
        agents[1].GetComponent<Seeker>().pathCallback+=p=>{if(!p.error)paths[1]++;};
        var targets=new[]{new Vector2(2,-1),new Vector2(-9,-4),new Vector2(9,4)};
        for(int i=0;i<targets.Length;i++)
        {
            var body=lab.player.GetComponent<Rigidbody2D>();body.position=targets[i];body.linearVelocity=Vector2.zero;
            Physics2D.SyncTransforms();foreach(var a in agents)a.SearchPath();
            var previous=new[]{agents[0].position,agents[1].position};float deadline=Time.time+40;
            while(Time.time<deadline && (Vector2.Distance(agents[0].position,targets[i])>.7f || Vector2.Distance(agents[1].position,targets[i])>.7f))
            {
                yield return new WaitForFixedUpdate();
                for(int j=0;j<agents.Length;j++)
                {
                    var current=agents[j].position;travelled[j]+=Vector3.Distance(previous[j],current);
                    if(Physics2D.OverlapCircle(current,.24f,1<<8))overlaps++;
                    if(Physics2D.Linecast(previous[j],current,1<<8).collider)crossings++;
                    previous[j]=current;
                }
            }
            foreach(var a in agents)Verify(a.name+" alcanza destino "+i,Vector2.Distance(a.position,targets[i])<.75f,"distance="+Vector2.Distance(a.position,targets[i]));
        }
        Verify("Ambos evitan las paredes",overlaps==0 && crossings==0,"overlaps="+overlaps+", crossings="+crossings);
        for(int j=0;j<2;j++)Verify(agents[j].name+" recorre y recalcula",travelled[j]>30 && paths[j]>5,"distance="+travelled[j]+", paths="+paths[j]);
        var output=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../../Verificacion/Ejercicio1"));
        int index=Array.IndexOf(args,"--lab-output");if(index>=0 && index+1<args.Length)output=args[index+1];
        Directory.CreateDirectory(output);File.WriteAllText(System.IO.Path.Combine(output,"resultados.json"),JsonUtility.ToJson(report,true));
        Application.Quit(report.failures==0?0:1);
    }
}
