using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PKC.ActionEditor.Cases.Editor
{
    internal static class RuntimeAssemblyPlayerBuildCase
    {
        private const string TemporaryScenePath =
            "Assets/ActionEditor/Cases/RuntimeAssembly/RuntimeAssemblySmokeScene.unity";

        [MenuItem("Tools/ActionEditor Cases/Build Runtime Assembly Smoke Player")]
        private static void BuildFromMenu()
        {
            BuildSmokePlayer(GetOutputPath());
        }

        public static void BuildFromCommandLine()
        {
            BuildSmokePlayer(GetOutputPath());
        }

        private static void BuildSmokePlayer(string outputPath)
        {
            SceneSetup[] previousSceneSetup = null;

            if (!Application.isBatchMode)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            }

            try
            {
                var smokeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var smokeObject = new GameObject("ActionEditor Runtime Assembly Smoke Case");
                var smokeCase = smokeObject.AddComponent<RuntimeAssemblySmokeCase>();
                smokeCase.Configure(true, true);

                if (!EditorSceneManager.SaveScene(smokeScene, TemporaryScenePath))
                    throw new InvalidOperationException("Failed to save the runtime assembly smoke scene.");

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { TemporaryScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Runtime assembly smoke Player build failed: {report.summary.result}");

                Debug.Log($"Runtime assembly smoke Player built at: {outputPath}");
            }
            finally
            {
                if (previousSceneSetup != null)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSceneSetup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                AssetDatabase.DeleteAsset(TemporaryScenePath);
            }
        }

        private static string GetOutputPath()
        {
            var configuredPath = Environment.GetEnvironmentVariable("ACTION_EDITOR_CASE_BUILD_PATH");
            return string.IsNullOrWhiteSpace(configuredPath)
                ? Path.GetFullPath("Temp/ActionEditorCases/RuntimeAssemblySmoke.exe")
                : Path.GetFullPath(configuredPath);
        }
    }
}
