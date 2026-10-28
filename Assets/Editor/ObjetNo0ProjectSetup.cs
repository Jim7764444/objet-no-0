using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ObjetNo0ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/ObjetNo0.unity";

    [MenuItem("Objet No 0/Create or repair prototype scene")]
    public static void CreateOrRepairPrototypeScene()
    {
        ConfigureProjectIdentity();

        if (File.Exists(ScenePath))
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        Scene scene = SceneManager.GetActiveScene();
        bool changed = false;
        if (Object.FindFirstObjectByType<Camera>() == null)
        {
            var cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.067f, 0.067f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
            changed = true;
        }

        if (Object.FindFirstObjectByType<ObjetNo0Game>() == null)
        {
            var gameObject = new GameObject("Objet N°0 - Bureau");
            gameObject.AddComponent<ObjetNo0Game>();
            changed = true;
        }

        if (changed)
            EditorSceneManager.SaveScene(scene, ScenePath);

        EnsureBuildScene();
        AssetDatabase.Refresh();
        Debug.Log("Objet N°0 prototype scene is ready.");
    }

    [MenuItem("Objet No 0/Validate prototype")]
    public static void ValidatePrototype()
    {
        ConfigureProjectIdentity();
        CreateOrRepairPrototypeScene();

        if (!ObjetNo0Game.ValidateCaseCatalog(out string catalogMessage))
            throw new System.InvalidOperationException(catalogMessage);

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (sceneAsset == null)
            throw new System.InvalidOperationException("The prototype scene could not be loaded.");

        if (Object.FindFirstObjectByType<ObjetNo0Game>() == null)
            throw new System.InvalidOperationException("The prototype scene does not contain its game controller.");

        bool sceneInBuild = false;
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == ScenePath && scenes[i].enabled)
                sceneInBuild = true;
        }

        if (!sceneInBuild)
            throw new System.InvalidOperationException("The prototype scene is not enabled in Build Settings.");

        RunClassificationLoopSmokeTest();
        Debug.Log("Objet N°0 validation passed. " + catalogMessage + " Scene and service-flow smoke test passed.");
    }

    private static void ConfigureProjectIdentity()
    {
        PlayerSettings.companyName = "Quantum Pixel Labs";
        PlayerSettings.productName = "Objet N°0";

        string versionPath = Path.Combine(Directory.GetCurrentDirectory(), "VERSION");
        if (!File.Exists(versionPath))
            throw new FileNotFoundException("VERSION is required to configure PlayerSettings.", versionPath);

        string version = File.ReadAllText(versionPath).Trim();
        if (string.IsNullOrEmpty(version))
            throw new System.InvalidOperationException("VERSION cannot be empty.");

        PlayerSettings.bundleVersion = version;
    }

    private static void EnsureBuildScene()
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < currentScenes.Length; i++)
        {
            if (currentScenes[i].path == ScenePath)
            {
                if (!currentScenes[i].enabled)
                {
                    currentScenes[i].enabled = true;
                    EditorBuildSettings.scenes = currentScenes;
                }
                return;
            }
        }

        var updatedScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
        for (int i = 0; i < currentScenes.Length; i++)
            updatedScenes[i] = currentScenes[i];
        updatedScenes[currentScenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = updatedScenes;
    }

    private static void RunClassificationLoopSmokeTest()
    {
        string archiveKey = GetPrivateConstant("ArchiveKey");
        string bestScoreKey = GetPrivateConstant("BestScoreKey");
        string runsKey = GetPrivateConstant("RunsKey");
        bool hadArchive = PlayerPrefs.HasKey(archiveKey);
        bool hadBestScore = PlayerPrefs.HasKey(bestScoreKey);
        bool hadRuns = PlayerPrefs.HasKey(runsKey);
        int previousArchive = PlayerPrefs.GetInt(archiveKey, 0);
        int previousBestScore = PlayerPrefs.GetInt(bestScoreKey, 0);
        int previousRuns = PlayerPrefs.GetInt(runsKey, 0);
        var testObject = new GameObject("Objet No 0 - Temporary Smoke Test")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        try
        {
            ObjetNo0Game game = testObject.AddComponent<ObjetNo0Game>();
            InvokePrivate(game, "BeginShift");
            if (GetPrivateField<int>(game, "focus") != 2)
                throw new System.InvalidOperationException("A new file should start with two focus points.");

            InvokePrivate(game, "RevealEvidence", 0);
            if (GetPrivateField<int>(game, "focus") != 1)
                throw new System.InvalidOperationException("Inspecting evidence should spend one focus point.");

            for (int fileNumber = 0; fileNumber < 4; fileNumber++)
            {
                ObjetNo0Game.Disposition correct = GetCurrentDisposition(game);
                InvokePrivate(game, "ResolveCase", correct);
                if (GetPrivateField<int>(game, "correctFiles") != fileNumber + 1)
                    throw new System.InvalidOperationException("A correct ruling was not recorded.");

                InvokePrivate(game, "AdvanceAfterFile");
                if (GetPrivateField<object>(game, "screen").ToString() == "Draft")
                    InvokePrivate(game, "SelectUpgrade", fileNumber < 2 ? 0 : 1);
            }

            if (GetPrivateField<object>(game, "screen").ToString() != "Intermission")
                throw new System.InvalidOperationException("Completing four files should reach the bank-or-risk decision.");

            int scoreBeforeAnnex = GetPrivateField<int>(game, "runScore");
            InvokePrivate(game, "OpenAnnex");
            ObjetNo0Game.Disposition annexAnswer = GetCurrentDisposition(game);
            ObjetNo0Game.Disposition wrongAnswer = annexAnswer == ObjetNo0Game.Disposition.Restituer
                ? ObjetNo0Game.Disposition.Archiver
                : ObjetNo0Game.Disposition.Restituer;
            InvokePrivate(game, "ResolveCase", wrongAnswer);

            if (GetPrivateField<int>(game, "stability") != 0 ||
                GetPrivateField<int>(game, "runScore") >= scoreBeforeAnnex)
            {
                throw new System.InvalidOperationException("A failed annex should end the service and reduce the score.");
            }

            InvokePrivate(game, "AdvanceAfterFile");
            if (GetPrivateField<object>(game, "screen").ToString() != "Report")
                throw new System.InvalidOperationException("A failed annex should end on the service report.");
        }
        finally
        {
            Object.DestroyImmediate(testObject);
            RestorePlayerPref(archiveKey, hadArchive, previousArchive);
            RestorePlayerPref(bestScoreKey, hadBestScore, previousBestScore);
            RestorePlayerPref(runsKey, hadRuns, previousRuns);
            PlayerPrefs.Save();
        }
    }

    private static ObjetNo0Game.Disposition GetCurrentDisposition(ObjetNo0Game game)
    {
        PropertyInfo currentCase = typeof(ObjetNo0Game).GetProperty(
            "CurrentCase",
            BindingFlags.Instance | BindingFlags.NonPublic);
        object file = currentCase.GetValue(game);
        PropertyInfo disposition = file.GetType().GetProperty("CorrectDisposition");
        return (ObjetNo0Game.Disposition)disposition.GetValue(file);
    }

    private static string GetPrivateConstant(string name)
    {
        FieldInfo field = typeof(ObjetNo0Game).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
        return (string)field.GetRawConstantValue();
    }

    private static object InvokePrivate(object target, string name, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
            throw new System.InvalidOperationException("Unable to find game-flow method: " + name);
        return method.Invoke(target, arguments);
    }

    private static T GetPrivateField<T>(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            throw new System.InvalidOperationException("Unable to find game-flow state: " + name);
        return (T)field.GetValue(target);
    }

    private static void RestorePlayerPref(string key, bool existed, int value)
    {
        if (existed)
            PlayerPrefs.SetInt(key, value);
        else
            PlayerPrefs.DeleteKey(key);
    }
}
