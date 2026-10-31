using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Pathfinding;

public static class Lab07ExerciseBuilder
{
    public const string ScenePath="Assets/Scenes/02_Ejercicio_1_DosEnemigos.unity";
    [MenuItem("Laboratorio 07/Crear ejercicio 1 y compilar")]
    public static void CreateAndBuild()
    {
        EditorSceneManager.OpenScene(Lab07Builder.ScenePath);
        // Work on a separate scene; the activity scene is retained as the baseline.
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        var env=GameObject.Find("Environment").transform;
        var template=GameObject.Find("wall_InnerLeft");
        var positions=new[]{new Vector2(-9,0),new Vector2(9,0),new Vector2(-3,0),new Vector2(3,0)};
        for(int i=0;i<positions.Length;i++)
        {
            var wall=UnityEngine.Object.Instantiate(template,env);
            wall.name="wall_Extra_"+(i+1); wall.transform.position=positions[i];
            wall.transform.localScale=new Vector3(.5f,i<2?3f:2f,1);
        }
        var original=GameObject.Find("Enemy");
        var second=UnityEngine.Object.Instantiate(original);
        second.name="Enemy_2";second.transform.position=new Vector3(9,4,0);
        second.transform.Find("Identificador").GetComponent<SpriteRenderer>().color=new Color(.85f,.4f,1);
        var ai=second.GetComponent<AIPath>();
        second.GetComponent<AIDestinationSetter>().target=GameObject.Find("Player").transform;
        second.transform.Find("Graphics").GetComponent<EnemyMovement>().aiPath=ai;
        var hud=UnityEngine.Object.FindFirstObjectByType<Lab07Dashboard>();
        hud.activityTitle="EJERCICIO 1\n12 paredes · 2 enemigos";hud.secondEnemy=ai;
        var secondRoute=UnityEngine.Object.Instantiate(hud.route);
        secondRoute.name="Ruta A estrella Enemy 2";
        secondRoute.startColor=secondRoute.endColor=new Color(.85f,.4f,1);
        var drawing=hud.gameObject.AddComponent<Lab07SecondRoute>();
        drawing.agent=ai;drawing.route=secondRoute;drawing.dashboard=hud;
        hud.gameObject.AddComponent<Lab07ExerciseVerification>();
        var astar=UnityEngine.Object.FindFirstObjectByType<AstarPath>();
        astar.ConfigureReferencesInternal();astar.data.FindGraphTypes();astar.data.Awake();
        Physics2D.SyncTransforms();astar.Scan();
        astar.data.SetData(astar.data.SerializeGraphs(new Pathfinding.Serialization.SerializeSettings {nodes=true}));
        EditorUtility.SetDirty(astar);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Lab07Builder.ScenePath,true),new EditorBuildSettingsScene(ScenePath,true)};
        Directory.CreateDirectory("Builds/Ejercicio1");
        var result=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Ejercicio1/Lab07_Ejercicio1.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Exercise build: "+result.summary.result);
        Selection.activeGameObject=second;
        Debug.Log("LAB07_EXERCISE_1_READY");
    }
}
