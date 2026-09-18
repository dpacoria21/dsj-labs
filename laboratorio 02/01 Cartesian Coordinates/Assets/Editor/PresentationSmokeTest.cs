using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PresentationSmokeTest
{
    private const string ActiveKey = "PresentationSmokeTest.Active";
    private const string PlayEnteredKey = "PresentationSmokeTest.PlayEntered";
    private const string ExitRequestedKey = "PresentationSmokeTest.ExitRequested";
    private const string ErrorCountKey = "PresentationSmokeTest.ErrorCount";
    private static double enteredPlayAt;

    static PresentationSmokeTest()
    {
        if (SessionState.GetBool(ActiveKey, false))
        {
            AttachCallbacks();
            EditorApplication.delayCall += ResumeAfterDomainReload;
        }
    }

    public static void Run()
    {
        string scene = EditorBuildSettings.scenes
            .Where(item => item.enabled)
            .Select(item => item.path)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(scene))
            throw new InvalidOperationException("No enabled scene is configured in Build Settings.");

        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(PlayEnteredKey, false);
        SessionState.SetBool(ExitRequestedKey, false);
        SessionState.SetInt(ErrorCountKey, 0);
        EditorSceneManager.OpenScene(scene);
        AttachCallbacks();
        Debug.Log($"PRESENTATION_SMOKE_START scene={scene}");
        EditorApplication.EnterPlaymode();
    }

    private static void ResumeAfterDomainReload()
    {
        if (!SessionState.GetBool(ActiveKey, false))
            return;

        if (EditorApplication.isPlaying)
        {
            if (!SessionState.GetBool(PlayEnteredKey, false))
            {
                SessionState.SetBool(PlayEnteredKey, true);
                enteredPlayAt = EditorApplication.timeSinceStartup;
                Debug.Log("PRESENTATION_PLAY_MODE_ENTERED");
            }
        }
        else if (SessionState.GetBool(PlayEnteredKey, false) &&
                 SessionState.GetBool(ExitRequestedKey, false))
        {
            Finish();
        }
    }

    private static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            SessionState.SetInt(ErrorCountKey, SessionState.GetInt(ErrorCountKey, 0) + 1);
    }

    private static void OnUpdate()
    {
        if (!SessionState.GetBool(ActiveKey, false))
            return;

        if (EditorApplication.isPlaying && SessionState.GetBool(PlayEnteredKey, false) &&
            !SessionState.GetBool(ExitRequestedKey, false) &&
            EditorApplication.timeSinceStartup - enteredPlayAt >= 5.0)
        {
            SessionState.SetBool(ExitRequestedKey, true);
            Debug.Log($"PRESENTATION_PLAY_MODE_OK errors={SessionState.GetInt(ErrorCountKey, 0)}");
            EditorApplication.ExitPlaymode();
        }
        else if (!EditorApplication.isPlaying &&
                 SessionState.GetBool(PlayEnteredKey, false) &&
                 SessionState.GetBool(ExitRequestedKey, false))
        {
            Finish();
        }
    }

    private static void AttachCallbacks()
    {
        Application.logMessageReceived -= OnLogMessage;
        Application.logMessageReceived += OnLogMessage;
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    private static void Finish()
    {
        int errors = SessionState.GetInt(ErrorCountKey, 0);
        Application.logMessageReceived -= OnLogMessage;
        EditorApplication.update -= OnUpdate;
        SessionState.EraseBool(ActiveKey);
        SessionState.EraseBool(PlayEnteredKey);
        SessionState.EraseBool(ExitRequestedKey);
        SessionState.EraseInt(ErrorCountKey);
        Debug.Log($"PRESENTATION_SMOKE_FINISHED errors={errors}");
        EditorApplication.Exit(errors == 0 ? 0 : 2);
    }
}
