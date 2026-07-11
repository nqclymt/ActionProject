using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor.Cases.Editor
{
    public static class CombatSkillSchemaEditorCase
    {
        private const string TemporaryAssetPath =
            "Assets/ActionEditor/Cases/CombatSkillSchema/CombatSkillSchemaEditorSmoke.json";

        public static void Run()
        {
            try
            {
                Prefs.InitializeAssetTypes();
                Require(Prefs.AssetTypes.TryGetValue("战斗技能", out var assetType),
                    "The create window did not discover Combat Skill.");
                Require(assetType == typeof(CombatSkillAsset),
                    "The Combat Skill entry resolves to the wrong type.");

                var skill = new CombatSkillAsset();
                Require(skill.Length >= skill.duration,
                    "A new combat skill did not initialize its timeline duration.");

                skill.skillId = "case.editor.authoring";
                skill.duration = 1.25f;
                skill.Validate();

                var json = Json.Serialize(skill);

                File.WriteAllText(Path.GetFullPath(TemporaryAssetPath), json);
                AssetDatabase.ImportAsset(TemporaryAssetPath, ImportAssetOptions.ForceUpdate);

                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(TemporaryAssetPath);
                Require(textAsset != null, "The temporary combat skill JSON was not imported.");

                App.TextAsset = textAsset;
                Require(App.AssetData is CombatSkillAsset,
                    "ActionEditor did not open the combat skill JSON as CombatSkillAsset.");
                Require(Selection.activeObject == App.CurrentInspectorPreviewAsset,
                    "Opening a combat skill did not expose its asset inspector.");

                Debug.Log("CombatSkillAsset editor authoring validation passed.");
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
