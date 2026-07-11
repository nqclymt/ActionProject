using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor.Cases.Editor
{
    public static class FrameEvaluationEditorCase
    {
        private const string TemporaryAssetPath =
            "Assets/ActionEditor/Cases/FrameEvaluation/FrameEvaluationEditorSmoke.json";

        public static void Run()
        {
            try
            {
                var skill = new CombatSkillAsset
                {
                    skillId = "case.editor.frame_evaluation",
                    duration = 1f,
                    frameRate = 10
                };
                skill.Validate();

                File.WriteAllText(Path.GetFullPath(TemporaryAssetPath), Json.Serialize(skill));
                AssetDatabase.ImportAsset(TemporaryAssetPath, ImportAssetOptions.ForceUpdate);
                App.TextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(TemporaryAssetPath);

                AssetPlayer.Inst.Reset();
                AssetPlayer.Inst.Sample(0.35f);

                var runtimePlayer = new SkillPlayer(skill);
                runtimePlayer.Play();
                runtimePlayer.Tick(0.35f);

                Require(AssetPlayer.Inst.CurrentFrame == runtimePlayer.CurrentFrame,
                    "编辑器预览与运行时帧号不一致。");
                Require(Math.Abs(AssetPlayer.Inst.CurrentTime - runtimePlayer.CurrentTime) < 0.0001f,
                    "编辑器预览与运行时时间不一致。");
                Require(AssetPlayer.Inst.CurrentFrame == 3,
                    "0.35 秒没有按 10 FPS 求值到第 3 帧。");

                Debug.Log("编辑器与运行时逐帧求值一致性验证通过。");
            }
            finally
            {
                App.TextAsset = null;
                AssetDatabase.DeleteAsset(TemporaryAssetPath);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
