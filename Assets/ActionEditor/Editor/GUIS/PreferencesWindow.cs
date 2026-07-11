using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor
{
    public class PreferencesWindow : PopupWindowContent
    {
        private const float WindowWidth = 420f;
        private const float WindowHeight = 330f;

        private Vector2 _scrollPosition;

        public static void Show(Rect activatorRect)
        {
            PopupWindow.Show(activatorRect, new PreferencesWindow());
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(WindowWidth, WindowHeight);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(8);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label(Lan.PreferencesTitle, titleStyle);
            EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            DrawLanguage();
            GUILayout.Space(6);

            Prefs.timeStepMode = (Prefs.TimeStepMode)EditorGUILayout.EnumPopup(
                new GUIContent(Lan.PreferencesTimeStepMode), Prefs.timeStepMode);

            if (Prefs.timeStepMode == Prefs.TimeStepMode.Frames)
            {
                DrawFrameRate();
            }
            else
            {
                DrawSnapInterval();
            }

            Prefs.MagnetSnapping = EditorGUILayout.Toggle(
                new GUIContent(Lan.PreferencesMagnetSnapping, Lan.PreferencesMagnetSnappingTips),
                Prefs.MagnetSnapping);
            Prefs.scrollWheelZooms = EditorGUILayout.Toggle(
                new GUIContent(Lan.PreferencesScrollWheelZooms, Lan.PreferencesScrollWheelZoomsTips),
                Prefs.scrollWheelZooms);

            var savePath = EditorTools.GUILayoutGetFolderPath(
                Lan.PreferencesSavePath, Lan.PreferencesSavePathTips, Prefs.savePath);
            if (!string.IsNullOrWhiteSpace(savePath))
            {
                Prefs.savePath = savePath.Replace('\\', '/');
            }

            Prefs.autoSaveSeconds = Mathf.Max(0, EditorGUILayout.DelayedIntField(
                new GUIContent(Lan.PreferencesAutoSaveTime, Lan.PreferencesAutoSaveTimeTips),
                Prefs.autoSaveSeconds));

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static void DrawLanguage()
        {
            var languages = System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.OrderBy(Lan.AllLanguages.Keys, name => name));
            if (languages.Length == 0)
            {
                return;
            }

            var currentIndex = System.Array.IndexOf(languages, Lan.Language);
            currentIndex = Mathf.Max(0, currentIndex);
            var selectedIndex = EditorGUILayout.Popup(Lan.PreferencesLanguage, currentIndex, languages);
            if (selectedIndex != currentIndex)
            {
                Lan.SetLanguage(languages[selectedIndex]);
                if (App.Window != null)
                {
                    App.Window.titleContent = new GUIContent(Lan.Title);
                    App.Window.Repaint();
                }
            }
        }

        private static void DrawFrameRate()
        {
            var options = System.Array.ConvertAll(Prefs.frameRates, value => $"{value:0} FPS");
            var index = System.Array.FindIndex(Prefs.frameRates,
                value => Mathf.Approximately(value, Prefs.FrameRate));
            index = Mathf.Max(0, index);
            var selectedIndex = EditorGUILayout.Popup(Lan.PreferencesFrameRate, index, options);
            Prefs.FrameRate = Mathf.RoundToInt(Prefs.frameRates[selectedIndex]);
        }

        private static void DrawSnapInterval()
        {
            var options = System.Array.ConvertAll(Prefs.snapIntervals, value => $"{value:0.###} s");
            var index = System.Array.FindIndex(Prefs.snapIntervals,
                value => Mathf.Approximately(value, Prefs.SnapInterval));
            index = Mathf.Max(0, index);
            var selectedIndex = EditorGUILayout.Popup(Lan.PreferencesSnapInterval, index, options);
            Prefs.SnapInterval = Prefs.snapIntervals[selectedIndex];
        }
    }
}
