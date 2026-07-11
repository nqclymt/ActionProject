using System;
using UnityEngine;

namespace PKC.ActionEditor
{
    public enum SkillFrameRounding
    {
        Floor,
        Nearest,
        Ceiling
    }

    public readonly struct SkillFrameSample
    {
        public SkillFrameSample(int previousFrame, int frame, float previousTime, float time)
        {
            PreviousFrame = previousFrame;
            Frame = frame;
            PreviousTime = previousTime;
            Time = time;
        }

        public int PreviousFrame { get; }
        public int Frame { get; }
        public float PreviousTime { get; }
        public float Time { get; }
    }

    /// <summary>
    /// 编辑器预览和运行时共用的帧转换及逐帧区间求值工具。
    /// </summary>
    public static class SkillFrameUtility
    {
        private const double RoundingEpsilon = 0.00001d;
        private const float TimeEpsilon = 0.00001f;

        public static int NormalizeFrameRate(int frameRate)
        {
            return Mathf.Clamp(frameRate, 1, 1000);
        }

        public static int GetFrameCount(float duration, int frameRate)
        {
            frameRate = NormalizeFrameRate(frameRate);
            return Math.Max(1, (int)Math.Ceiling(Math.Max(0f, duration) * frameRate - RoundingEpsilon));
        }

        public static int TimeToFrame(float time, int frameRate,
            SkillFrameRounding rounding = SkillFrameRounding.Floor)
        {
            frameRate = NormalizeFrameRate(frameRate);
            var scaledTime = Math.Max(0d, time) * frameRate;
            return rounding switch
            {
                SkillFrameRounding.Nearest => Math.Max(0, (int)Math.Round(scaledTime,
                    MidpointRounding.AwayFromZero)),
                SkillFrameRounding.Ceiling => Math.Max(0, (int)Math.Ceiling(scaledTime - RoundingEpsilon)),
                _ => Math.Max(0, (int)Math.Floor(scaledTime + RoundingEpsilon))
            };
        }

        public static float FrameToTime(int frame, int frameRate, float duration)
        {
            frameRate = NormalizeFrameRate(frameRate);
            return Mathf.Min(Mathf.Max(0, frame) / (float)frameRate, Mathf.Max(0f, duration));
        }

        public static int GetEvaluationFrame(float time, int frameRate, float duration,
            SkillFrameRounding rounding = SkillFrameRounding.Floor)
        {
            duration = Mathf.Max(0f, duration);
            if (time >= duration - TimeEpsilon)
                return GetFrameCount(duration, frameRate);

            return Mathf.Min(TimeToFrame(time, frameRate, rounding), GetFrameCount(duration, frameRate));
        }

        public static float QuantizeTime(float time, int frameRate, float duration,
            SkillFrameRounding rounding = SkillFrameRounding.Floor)
        {
            duration = Mathf.Max(0f, duration);
            var clampedTime = Mathf.Clamp(time, 0f, duration);
            var frame = GetEvaluationFrame(clampedTime, frameRate, duration, rounding);
            return FrameToTime(frame, frameRate, duration);
        }

        public static void EvaluateRange(float previousTime, float targetTime, int frameRate,
            float duration, Action<SkillFrameSample> evaluate)
        {
            if (evaluate == null)
                throw new ArgumentNullException(nameof(evaluate));

            frameRate = NormalizeFrameRate(frameRate);
            duration = Mathf.Max(0f, duration);
            var previousFrame = GetEvaluationFrame(previousTime, frameRate, duration);
            var targetFrame = GetEvaluationFrame(targetTime, frameRate, duration);
            if (previousFrame == targetFrame)
                return;

            var step = targetFrame > previousFrame ? 1 : -1;
            var fromFrame = previousFrame;
            var fromTime = FrameToTime(fromFrame, frameRate, duration);

            for (var frame = previousFrame + step;; frame += step)
            {
                var time = FrameToTime(frame, frameRate, duration);
                evaluate(new SkillFrameSample(fromFrame, frame, fromTime, time));
                if (frame == targetFrame)
                    break;

                fromFrame = frame;
                fromTime = time;
            }
        }
    }
}
