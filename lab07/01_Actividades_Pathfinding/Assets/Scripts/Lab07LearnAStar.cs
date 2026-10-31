using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Adaptación 2D del ejemplo Unity Learn The A* Algorithm (S0407).
// Conserva marcadores, G/H/F, abiertos/cerrados y padres; añade agotamiento seguro.
public class Lab07LearnAStar : MonoBehaviour
{
 public class Node { public Vector2Int p; public float g,h; public float f=>g+h; public Node parent; }
 public const int W=17,H=13;
 public bool[,] walls=new bool[W,H];
 public List<Node> open=new List<Node>(),closed=new List<Node>();
 public List<Vector2Int> path=new List<Vector2Int>();
 public Vector2Int start,goal; public bool done,found,showPath,automatic;
 public int seed=7; float tick; System.Random rng;
 public string status="Listo";
 static readonly Vector2Int[] dirs={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
 void Start(){Generate();Begin(new Vector2Int(1,1),new Vector2Int(14,11));}
 public void Generate(){rng=new System.Random(seed); for(int x=0;x<W;x++)for(int y=0;y<H;y++)walls[x,y]=x==0||y==0||x==W-1||y==H-1;for(int x=3;x<W-1;x+=3)for(int y=1;y<H-1;y++)walls[x,y]=y!=(x%2==0?2:10);}
 public void RandomEndpoints(){var free=new List<Vector2Int>();for(int x=1;x<W-1;x++)for(int y=1;y<H-1;y++)if(!walls[x,y])free.Add(new Vector2Int(x,y));int a=rng.Next(free.Count),b;do{b=rng.Next(free.Count);}while(a==b);Begin(free[a],free[b]);}
 public void Begin(Vector2Int a,Vector2Int b){start=a;goal=b;open.Clear();closed.Clear();path.Clear();done=found=showPath=automatic=false;open.Add(new Node{p=a,g=0,h=Vector2Int.Distance(a,b)});status="Inicio y meta seleccionados";}
 public void Step(){if(done)return;if(open.Count==0){done=true;status="Sin camino: lista abierta vacía";automatic=false;return;}var current=open.OrderBy(n=>n.f).ThenBy(n=>n.h).First();open.Remove(current);closed.Add(current);if(current.p==goal){done=found=true;automatic=false;for(var n=current;n!=null;n=n.parent)path.Add(n.p);path.Reverse();status="Meta encontrada · coste "+current.g;return;}foreach(var d in dirs){var p=current.p+d;if(p.x<0||p.x>=W||p.y<0||p.y>=H||walls[p.x,p.y]||closed.Any(n=>n.p==p))continue;float g=current.g+1;var old=open.Find(n=>n.p==p);if(old==null)open.Add(new Node{p=p,g=g,h=Vector2Int.Distance(p,goal),parent=current});else if(g<old.g){old.g=g;old.parent=current;}}status="Explorando el menor F, desempate H";}
 void Update(){if(Input.GetKeyDown(KeyCode.P))RandomEndpoints();if(Input.GetKeyDown(KeyCode.C))Step();if(Input.GetKeyDown(KeyCode.M)&&found)showPath=true;if(Input.GetKeyDown(KeyCode.Space))automatic=!automatic;if(automatic&&Time.unscaledTime>tick){Step();tick=Time.unscaledTime+.08f;}}
 void OnGUI(){float s=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.Scale(new Vector3(s,s,1));var text=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};var title=new GUIStyle(text){fontSize=29,fontStyle=FontStyle.Bold};GUI.Box(new Rect(10,10,330,880),"");GUILayout.BeginArea(new Rect(30,30,285,830));GUILayout.Label("LABORATORIO 07",text);GUILayout.Label("Ejercicio 2\nA* paso a paso",title);GUILayout.Space(15);GUILayout.Label("Adaptación 2D de\nUnity Learn · The A* Algorithm",text);GUILayout.Space(20);if(GUILayout.Button("P · Nuevos inicio y meta",GUILayout.Height(40)))RandomEndpoints();if(GUILayout.Button("C · Un paso",GUILayout.Height(40)))Step();if(GUILayout.Button("Espacio · Auto / pausa",GUILayout.Height(40)))automatic=!automatic;if(GUILayout.Button("M · Reconstruir ruta",GUILayout.Height(40))&&found)showPath=true;GUILayout.Space(20);GUILayout.Label(status,text);GUILayout.Label("Abiertos: "+open.Count+"\nCerrados: "+closed.Count+"\nRuta: "+(found?(path.Count-1)+" pasos":"pendiente"),text);GUILayout.Space(20);GUILayout.Label("Verde: inicio\nRojo: meta\nAzul: abiertos\nNaranja: cerrados\nAmarillo: ruta",text);GUILayout.Space(15);GUILayout.Label("G: coste acumulado\nH: distancia euclídea\nF = G + H\n4 vecinos · coste 1",text);GUILayout.EndArea();var small=new GUIStyle(GUI.skin.label){fontSize=10,normal={textColor=Color.white}};for(int x=0;x<W;x++)for(int y=0;y<H;y++){var p=new Vector2Int(x,y);var n=open.Find(a=>a.p==p)??closed.Find(a=>a.p==p);Color c=walls[x,y]?new Color(.55f,.6f,.65f):new Color(.1f,.14f,.2f);if(n!=null)c=open.Contains(n)?new Color(.1f,.4f,.7f):new Color(.62f,.29f,.08f);if(showPath&&path.Contains(p))c=new Color(.7f,.58f,.05f);if(p==start)c=new Color(.1f,.6f,.2f);if(p==goal)c=new Color(.75f,.15f,.2f);var r=new Rect(360+x*62,40+(H-1-y)*62,60,60);GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;if(n!=null)GUI.Label(new Rect(r.x+3,r.y+2,58,56),"G "+n.g.ToString("0")+"\nH "+n.h.ToString("0.0")+"\nF "+n.f.ToString("0.0"),small);if(p==start||p==goal)GUI.Label(new Rect(r.x+3,r.y+42,58,20),p==start?"INICIO":"META",small);}}
}

