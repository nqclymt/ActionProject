using System;
using System.Collections.Generic;
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
            set
            {
                currentTime = Asset == null
                    ? 0f
                    : SkillFrameUtility.QuantizeTime(value, Asset.EvaluationFrameRate, Length);
            }
        }

        public int CurrentFrame => Asset == null
            ? 0
            : SkillFrameUtility.GetEvaluationFrame(currentTime, Asset.EvaluationFrameRate, Length);
        
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

            var targetTime = SkillFrameUtility.QuantizeTime(time, Asset.EvaluationFrameRate, Length);
            if ((targetTime == 0 || targetTime == Length) && previousTime == targetTime)
            {
                currentTime = targetTime;
                return;
            }
            
            if (!preInitialized && targetTime > 0 && previousTime == 0)
            {
                InitializePreviewPointers();
            }

            if (timePointers != null)
            {
                SkillFrameUtility.EvaluateRange(previousTime, targetTime,
                    Asset.EvaluationFrameRate, Length, sample =>
                    {
                        currentTime = sample.Time;
                        InternalSamplePointers(sample.Time, sample.PreviousTime);
                    });
            }

            currentTime = targetTime;
            previousTime = targetTime;
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

            AnimationPreviewCoordinator.Evaluate(initializedAsset ?? Asset, previousTime, currentTime);
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

            var previews = new Dictionary<IDirectable, PreviewBase>();
            var unsupportedDirectables = new HashSet<IDirectable>();
            var executionEvents = DirectableExecutionOrder.Build(Asset);
            foreach (var executionEvent in executionEvents)
            {
                var directable = executionEvent.Directable;
                if (directable == null || !directable.IsActive || unsupportedDirectables.Contains(directable))
                    continue;

                if (!previews.TryGetValue(directable, out var preview))
                {
                    if (!typeDic.TryGetValue(directable.GetType(), out var previewType))
                    {
                        unsupportedDirectables.Add(directable);
                        continue;
                    }

                    preview = CreatePreview(previewType, directable);
                    if (preview == null)
                    {
                        unsupportedDirectables.Add(directable);
                        continue;
                    }

                    previews.Add(directable, preview);
                }

                if (executionEvent.EventType == DirectableExecutionEventType.Start)
                {
                    var evaluationEndTime = SkillFrameUtility.QuantizeTime(directable.EndTime,
                        Asset.EvaluationFrameRate, Asset.Length, SkillFrameRounding.Nearest);
                    var startPointer = new StartTimePointer(preview, executionEvent.Time, evaluationEndTime);
                    timePointers.Add(startPointer);
                    unsortedStartTimePointers.Add(startPointer);
                }
                else
                {
                    timePointers.Add(new EndTimePointer(preview, executionEvent.Time));
                }
            }

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
                var asset = initializedAsset ?? Asset;
                if (asset != null)
                {
                    SkillFrameUtility.EvaluateRange(previousTime, 0f,
                        asset.EvaluationFrameRate, asset.Length, sample =>
                        {
                            currentTime = sample.Time;
                            InternalSamplePointers(sample.Time, sample.PreviousTime);
                        });
                }
            }

            timePointers = null;
            unsortedStartTimePointers = null;
            AnimationPreviewCoordinator.Reset();
            preInitialized = false;
            initializedAsset = null;
        }
    }
}
