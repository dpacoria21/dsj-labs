using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
public static class Lab07LearnBuilder {
 [MenuItem("Laboratorio 07/Crear ejercicio 2 Unity Learn")]
 public static void Create(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var c=new GameObject("Main Camera").AddComponent<Camera>();c.tag="MainCamera";c.orthographic=true;c.transform.position=new Vector3(0,0,-10);c.backgroundColor=new Color(.04f,.06f,.1f);c.clearFlags=CameraClearFlags.SolidColor;var demo=new GameObject("A estrella - Unity Learn 2D").AddComponent<Lab07LearnAStar>();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/03_Ejercicio_2_UnityLearn_AStar.unity");Verify(demo);Debug.Log("LAB07_EJ2_READY");}
 static void Verify(Lab07LearnAStar d){d.Generate();int passed=0;for(int i=0;i<100;i++){d.RandomEndpoints();var q=new Queue<Vector2Int>();var distances=new Dictionary<Vector2Int,int>();q.Enqueue(d.start);distances[d.start]=0;while(q.Count>0){var p=q.Dequeue();foreach(var v in new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left}){var n=p+v;if(n.x<0||n.y<0||n.x>=Lab07LearnAStar.W||n.y>=Lab07LearnAStar.H||d.walls[n.x,n.y]||distances.ContainsKey(n))continue;distances[n]=distances[p]+1;q.Enqueue(n);}}int guard=1000;while(!d.done&&guard-->0)d.Step();if(!d.found||d.path.Count-1!=distances[d.goal])throw new System.Exception("A* difiere de BFS");passed++;}d.Begin(new Vector2Int(1,1),new Vector2Int(15,11));d.walls[1,2]=d.walls[2,1]=true;while(!d.done)d.Step();if(d.found)throw new System.Exception("Caso sin ruta");passed++;Directory.CreateDirectory("Verificacion/Ejercicio2");File.WriteAllText("Verificacion/Ejercicio2/resultados.txt",passed+" pruebas correctas: 100 pares comparados con BFS y un caso sin ruta.");}
}
