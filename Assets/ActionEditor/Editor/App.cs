using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PKC.ActionEditor
{
    public delegate void CallbackFunction();

    public delegate void OpenAssetFunction(Asset asset);
    public static class App
    {
        private static TextAsset _textAsset;

        public static CallbackFunction OnInitialize;
        public static CallbackFunction OnDisable;
        public static OpenAssetFunction OnOpenAsset;

        public static Asset AssetData { get; private set; } = null;

        public static TextAsset TextAsset
        {
            get => _textAsset;
            set
            {
                if (_textAsset == value)
                {
                    return;
                }

                Stop();
                Select(null);
                _textAsset = value;
                AssetData = null;
                if (value == null)
                {
                    Refresh();
                }
                else
                {
                    var obj = Json.Deserialize(typeof(Asset), _textAsset.text);
                    if (obj is Asset asset)
                    {
                        AssetData = asset;
                        asset.Init();
                        Selection.activeObject = CurrentInspectorPreviewAsset;
                        EditorUtility.SetDirty(CurrentInspectorPreviewAsset);
                        OnOpenAsset?.Invoke(AssetData);
                        _lastSaveTime = DateTime.Now;
                        Refresh();
                    }
                    else
                    {
                        Debug.LogError($"ActionEditor could not open '{value.name}' because it does not contain valid action data.");
                    }
                }
            }
        }

        public static EditorWindow Window;

        public static long Frame;

        public static float Width;

        public static void OnObjectPickerConfig(Object obj)
        {
            if (obj is TextAsset textAsset)
            {
                TextAsset = textAsset;
            }
        }

        public static void SaveAsset()
        {
            if (AssetData == null || TextAsset == null) return;
            AssetData.OnBeforeSerialize();
            var path = AssetDatabase.GetAssetPath(TextAsset);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("ActionEditor could not save because the selected asset has no valid path.");
                return;
            }

            var json = Json.Serialize(AssetData);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"ActionEditor could not serialize '{TextAsset.name}'. The existing file was not changed.");
                return;
            }

            File.WriteAllText(Path.GetFullPath(path), json);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        public static void OnGUIEnd()
        {
            if (Frame > NeedForceRefreshFrame)
            {
                NeedForceRefresh = false;
            }

            Frame++;
            if (Frame >= long.MaxValue)
            {
                Frame = 0;
            }
        }

        public static void OnUpdate()
        {
            TryAutoSave();
            PlayerUpdate();
        }
        
        #region AutoSave

        public static DateTime LastSaveTime => _lastSaveTime;

        private static DateTime _lastSaveTime = DateTime.Now;

        /// <summary>
        /// 尝试自动保存
        /// </summary>
        public static void TryAutoSave()
        {
            var interval = Prefs.autoSaveSeconds;
            if (AssetData == null || interval <= 0)
            {
                return;
            }

            var elapsed = DateTime.Now - _lastSaveTime;
            if (elapsed.TotalSeconds >= interval)
            {
                AutoSave();
            }
        }

        public static void AutoSave()
        {
            _lastSaveTime = DateTime.Now;
            SaveAsset();
        }

        #endregion
        
        #region Copy&Cut

        public static IDirectable CopyAsset { get; set; }
        public static bool IsCut { get; set; }

        #endregion
        
         #region Select

        public static IDirectable[] SelectItems => _selectList.ToArray();
        public static int SelectCount => _selectList.Count;
        private static readonly List<IDirectable> _selectList = new List<IDirectable>();

        public static IDirectable FistSelect => _selectList.Count > 0 ? _selectList.First() : null;

        public static bool CanMultipleSelect { get; set; }

        [System.NonSerialized] private static InspectorPreviewAsset _currentInspectorPreviewAsset;

        public static InspectorPreviewAsset CurrentInspectorPreviewAsset
        {
            get
            {
                if (_currentInspectorPreviewAsset == null)
                {
                    _currentInspectorPreviewAsset = ScriptableObject.CreateInstance<InspectorPreviewAsset>();
                }

                return _currentInspectorPreviewAsset;
            }
        }

        public static void Select(params IDirectable[] objs)
        {
            var change = false;
            if (objs == null)
            {
                if (_selectList.Count > 0) change = true;
            }
            else
            {
                if (objs.Length != _selectList.Count) change = true;
                else
                {
                    var pickCount = 0;
                    foreach (var obj in objs)
                    {
                        if (_selectList.Contains(obj)) pickCount++;
                    }

                    if (pickCount != objs.Length)
                    {
                        change = true;
                    }
                }
            }

            if (!change) return;
            _selectList.Clear();
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    _selectList.Add(obj);
                }

                Selection.activeObject = CurrentInspectorPreviewAsset;
                EditorUtility.SetDirty(CurrentInspectorPreviewAsset);

                // DirectorUtility.selectedObject = FistSelect;
            }

            if (_selectList.Count == 1 && _selectList[0] is not Clip)
            {
                CanMultipleSelect = true;
            }
            else
            {
                CanMultipleSelect = false;
            }
        }

        public static bool IsSelect(IDirectable directable)
        {
            return _selectList.Contains(directable);
        }

        #endregion
        #region Refresh

        public static bool NeedForceRefresh { get; private set; }
        public static long NeedForceRefreshFrame { get; private set; }

        public static void Refresh(bool previewDataChanged = false)
        {
            if (previewDataChanged)
            {
                _player.Invalidate();
            }

            NeedForceRefresh = true;
            NeedForceRefreshFrame = Frame;
        }

        public static void NotifyDataChanged()
        {
            AssetData?.Validate();
            Refresh(true);
            Repaint();
        }


        public static void Repaint()
        {
            if (Window != null)
            {
                Window.Repaint();
            }
        }

        #endregion
        
        #region 播放相关

        public static CallbackFunction OnPlay;
        public static CallbackFunction OnStop;

        private static AssetPlayer _player => AssetPlayer.Inst;

        public static bool IsPlay { get; private set; }
        public static bool IsPause { get; private set; }

        public static bool IsRange { get; set; }

        private static float _editorPreviousTime;

        public static void Play(Action callback = null)
        {
            if (Application.isPlaying || AssetData == null)
            {
                return;
            }

            OnPlay?.Invoke();
            IsPlay = true;
            IsPause = false;
            _editorPreviousTime = Time.realtimeSinceStartup;
            var playStart = GetPlayStart();
            var playEnd = GetPlayEnd();
            if (_player.CurrentTime < playStart || _player.CurrentTime >= playEnd)
            {
                _player.Sample(playStart);
            }
            callback?.Invoke();
        }

        public static void Pause(bool pause = true)
        {
            IsPause = IsPlay && pause;
        }

        public static void Stop()
        {
            _player.Reset();

            OnStop?.Invoke();
            IsPlay = false;
            IsPause = false;
        }

        public static void StepForward()
        {
            if (Math.Abs(_player.CurrentTime - _player.Length) < 0.00001f)
            {
                _player.Sample(0);
                return;
            }

            _player.Sample(_player.CurrentTime + Prefs.SnapInterval);
        }

        public static void StepBackward()
        {
            if (_player.CurrentTime == 0)
            {
                _player.Sample(_player.Length);
                return;
            }

            _player.Sample(_player.CurrentTime - Prefs.SnapInterval);
        }


        private static void PlayerUpdate()
        {
            var now = Time.realtimeSinceStartup;
            var delta = Mathf.Max(0, now - _editorPreviousTime) * Mathf.Max(0, Time.timeScale);
            _editorPreviousTime = now;

            if (AssetData == null)
            {
                return;
            }

            if (!IsPlay || IsPause)
            {
                _player.Sample();
                return;
            }

            var playStart = GetPlayStart();
            var playEnd = GetPlayEnd();
            var playLength = playEnd - playStart;
            if (playLength <= Mathf.Epsilon)
            {
                return;
            }

            if (_player.CurrentTime < playStart || _player.CurrentTime >= playEnd)
            {
                _player.Sample(playStart);
            }

            var nextTime = _player.CurrentTime + delta;
            if (nextTime >= playEnd)
            {
                _player.Sample(playEnd);
                _player.Sample(playStart);
                nextTime = playStart + (nextTime - playStart) % playLength;
            }

            _player.Sample(nextTime);
            Repaint();
        }

        private static float GetPlayStart()
        {
            return IsRange ? Mathf.Clamp(AssetData.RangeMin, 0, _player.Length) : 0;
        }

        private static float GetPlayEnd()
        {
            if (!IsRange)
            {
                return _player.Length;
            }

            return Mathf.Clamp(AssetData.RangeMax, GetPlayStart(), _player.Length);
        }

        public static void Shutdown()
        {
            Stop();
            Select(null);
            if (_currentInspectorPreviewAsset != null)
            {
                Object.DestroyImmediate(_currentInspectorPreviewAsset);
                _currentInspectorPreviewAsset = null;
            }
        }

        #endregion
    }
}
