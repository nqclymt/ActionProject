
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor
{
    public class CreateAssetWindow : PopupWindowContent
    {
        
        private static Rect _myRect ;
        private string _selectType ;
        private string _createName = string.Empty;

        public static void Show()
        {
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            _myRect = new Rect(mousePos.x, mousePos.y, 400, 150);
            PopupWindow.Show(new Rect(_myRect.x,_myRect.y,0,0),new CreateAssetWindow());
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(_myRect.width, _myRect.height);
        }

        public override void OnGUI(Rect rect)
        {
            if (string.IsNullOrEmpty(_selectType) && Prefs.AssetNames.Count > 0)
            {
                _selectType = Prefs.AssetNames[0];
            }

            GUILayout.BeginVertical("box");
            
            GUI.color = new Color(0, 0, 0, 0.3f);
            
            GUILayout.BeginHorizontal(Styles.HeaderBoxStyle);
            GUI.color = Color.white;
            GUILayout.Label($"<size=22><b>{Lan.CreateAsset}</b></size>");
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            
            GUILayout.BeginVertical("box");
            
            _selectType = EditorTools.CleanPopup(Lan.CreateAsset, _selectType, Prefs.AssetNames);
            _createName = EditorGUILayout.TextField(new GUIContent(Lan.CreateAsset, Lan.CreateAssetFileName),
                _createName);
            if (Prefs.AssetNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No concrete ActionEditor Asset type was found.", MessageType.Warning);
            }

            GUI.enabled = Prefs.AssetNames.Count > 0;
            GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
            if (GUILayout.Button(new GUIContent(Lan.CreateAssetConfirm)))
            {
                CreateConfirm();
            }
            
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(new GUIContent(Lan.CreateAssetReset)))
            {
                _selectType = Prefs.AssetNames.Count > 0 ? Prefs.AssetNames[0] : null;
                _createName = string.Empty;
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            
            GUILayout.EndVertical();
            
        }

        void CreateConfirm()
        {
            if (string.IsNullOrEmpty(_createName))
            {
                EditorUtility.DisplayDialog(Lan.TipsTitle, Lan.CreateAssetTipsNameNull, Lan.TipsConfirm);
            }
            else if (_createName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                     !string.Equals(_createName, Path.GetFileName(_createName), StringComparison.Ordinal))
            {
                EditorUtility.DisplayDialog(Lan.TipsTitle, Lan.CreateAssetTipsNameNull, Lan.TipsConfirm);
            }
            else if (!Prefs.AssetTypes.TryGetValue(_selectType, out var assetType))
            {
                EditorUtility.DisplayDialog(Lan.TipsTitle,
                    "No concrete ActionEditor Asset type is available.", Lan.TipsConfirm);
            }
            else if (string.IsNullOrEmpty(Prefs.savePath) ||
                     !(string.Equals(Prefs.savePath.TrimEnd('/', '\\'), "Assets", StringComparison.Ordinal) ||
                       Prefs.savePath.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal)))
            {
                EditorUtility.DisplayDialog(Lan.TipsTitle,
                    "The ActionEditor save path must be inside Assets.", Lan.TipsConfirm);
            }
            else
            {
                var folder = Prefs.savePath.TrimEnd('/', '\\');
                var path = $"{folder}/{_createName}.json";
                if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null)
                {
                    EditorUtility.DisplayDialog(Lan.TipsTitle, Lan.CreateAssetTipsRepetitive, Lan.TipsConfirm);
                    return;
                }

                Directory.CreateDirectory(Path.GetFullPath(folder));
                var inst = Activator.CreateInstance(assetType);
                if (inst == null) return;

                var json = Json.Serialize(inst);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"ActionEditor could not serialize a new '{assetType.Name}' asset.");
                    return;
                }

                File.WriteAllText(path, json);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (textAsset != null)
                {
                    App.OnObjectPickerConfig(textAsset);
                }

                editorWindow.Close();
            }
        }
    }
}
