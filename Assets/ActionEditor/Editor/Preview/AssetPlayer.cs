using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PKC.ActionEditor
{
    public enum EditorPlaybackState
    {
        Stoped,
        PlayingForwards,
        PlayingBackwards
    }
    public sealed class AssetPlayer
    {
        private static AssetPlayer _inst;

        public static AssetPlayer Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new AssetPlayer();
                }

                return _inst;
            }
        }
        
        private List<IDirectableTimePointer> timePointers;
        
         /// <summary>
        /// 预览器
        /// </summary>
        private List<IDirectableTimePointer> unsortedStartTimePointers;

        private float currentTime;

        public float previousTime { get; private set; }

        private bool preInitialized;
        private Asset initializedAsset;

        public Asset Asset => App.AssetData;

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime
        {
            get => currentTime;
            set => currentTime = Mathf.Clamp(value, 0, Length);
        }
        
        public float Length
        {
            get
            {
                if (Asset != null)
                {
                    return Asset.Length;
                }

                return 0;
            }
        }

        public void Sample()
        {
            Sample(currentTime);
        }

        public void Sample(float time)
        {
            if (Asset == null)
            {
                Reset();
                return;
            }

            if (preInitialized && initializedAsset != Asset)
            {
                Reset();
            }

            CurrentTime = time;
            if ((currentTime == 0 || currentTime == Length) && previousTime == currentTime)
            {
                return;
            }
            
            if (!preInitialized && currentTime > 0 && previousTime == 0)
            {
                InitializePreviewPointers();
            }


            if (timePointers != null)
            {
                InternalSamplePointers(currentTime, previousTime);
            }

            previousTime = currentTime;
        }

        void InternalSamplePointers(float currentTime, float previousTime)
        {
            if (!Application.isPlaying || currentTime > previousTime)
            {
                foreach (var t in timePointers)
                {
                    try
                    {
                        t.TriggerForward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }


            if (!Application.isPlaying || currentTime < previousTime)
            {
                for (var i = timePointers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        timePointers[i].TriggerBackward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (unsortedStartTimePointers != null)
            {
                foreach (var t in unsortedStartTimePointers)
                {
                    try
                    {
                        t.Update(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化时间指针预览器
        /// </summary>
        public void InitializePreviewPointers()
        {
            ReleasePreviewPointers();
            timePointers = new List<IDirectableTimePointer>();
            unsortedStartTimePointers = new List<IDirectableTimePointer>();

            if (Asset == null)
            {
                return;
            }

            Dictionary<Type, Type> typeDic = new Dictionary<Type, Type>();
            var childs = EditorTools.GetTypeMetaDerivedFrom(typeof(PreviewBase));
            foreach (var t in childs)
            {
                var arrs = t.type.GetCustomAttributes(typeof(CustomPreviewAttribute), true);
                foreach (var arr in arrs)
                {
                    if (arr is CustomPreviewAttribute c)
                    {
                        var bindT = c.PreviewType;
                        var iT = t.type;
                        if (!typeDic.ContainsKey(bindT))
                        {
                            if (!iT.IsAbstract) typeDic[bindT] = iT;
                        }
                        else
                        {
                            var old = typeDic[bindT];
                            //如果不是抽象类，且是子类就更新
                            if (!iT.IsAbstract && iT.IsSubclassOf(old))
                            {
                                typeDic[bindT] = iT;
                            }
                        }
                    }
                }
            }

            foreach (var group in Asset.groups.Where(group => group != null).Reverse())
            {
                if (!group.IsActive) continue;
                foreach (var track in group.Tracks.Where(track => track != null).Reverse())
                {
                    if (!track.IsActive) continue;
                    var tType = track.GetType();
                    if (typeDic.TryGetValue(tType, out var t1))
                    {
                        var preview = CreatePreview(t1, track);
                        if (preview != null)
                        {
                            var p3 = new StartTimePointer(preview);
                            timePointers.Add(p3);
                
                            unsortedStartTimePointers.Add(p3);
                            timePointers.Add(new EndTimePointer(preview));
                        }
                    }
                
                    foreach (var clip in track.Clips.Where(clip => clip != null))
                    {
                        var cType = clip.GetType();
                        if (typeDic.TryGetValue(cType, out var t))
                        {
                            var preview = CreatePreview(t, clip);
                            if (preview != null)
                            {
                                var p3 = new StartTimePointer(preview);
                                timePointers.Add(p3);
                
                                unsortedStartTimePointers.Add(p3);
                                timePointers.Add(new EndTimePointer(preview));
                            }
                        }
                    }
                }
            }

            timePointers = timePointers.OrderBy(pointer => pointer.time).ToList();
            preInitialized = true;
            initializedAsset = Asset;
        }

        public void Invalidate()
        {
            var time = currentTime;
            ReleasePreviewPointers();
            currentTime = Mathf.Clamp(time, 0, Length);
            previousTime = 0;
        }

        public void Reset()
        {
            ReleasePreviewPointers();
            currentTime = 0;
            previousTime = 0;
        }

        private PreviewBase CreatePreview(Type previewType, IDirectable target)
        {
            try
            {
                if (Activator.CreateInstance(previewType) is PreviewBase preview)
                {
                    preview.SetTarget(target);
                    return preview;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        private void ReleasePreviewPointers()
        {
            if (timePointers != null && previousTime > 0)
            {
                InternalSamplePointers(0, previousTime);
            }

            timePointers = null;
            unsortedStartTimePointers = null;
            preInitialized = false;
            initializedAsset = null;
        }
    }
}
