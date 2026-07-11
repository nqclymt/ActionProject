using System;
using System.Collections.Generic;
using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class FrameEvaluationCase : MonoBehaviour
    {
        [ContextMenu("验证逐帧求值")]
        public void ValidateFrameEvaluation()
        {
            Run();
        }

        public static void Run()
        {
            Require(SkillFrameUtility.GetFrameCount(0.95f, 10) == 10,
                "非整帧技能的结尾帧计算错误。");
            Require(Approximately(SkillFrameUtility.FrameToTime(10, 10, 0.95f), 0.95f),
                "最后一帧没有精确落在技能结束时间。");

            var forwardFrames = new List<int>();
            SkillFrameUtility.EvaluateRange(0f, 0.35f, 10, 1f,
                sample => forwardFrames.Add(sample.Frame));
            Require(IsSequence(forwardFrames, 1, 2, 3), "正向区间没有逐帧求值。");

            var backwardFrames = new List<int>();
            SkillFrameUtility.EvaluateRange(0.3f, 0f, 10, 1f,
                sample => backwardFrames.Add(sample.Frame));
            Require(IsSequence(backwardFrames, 2, 1, 0), "反向区间没有逐帧求值。");

            var skill = new CombatSkillAsset { duration = 1f, frameRate = 10 };
            skill.Validate();
            var player = new SkillPlayer(skill);
            var evaluatedFrames = new List<int>();
            player.FrameEvaluated += (_, _, frame) => evaluatedFrames.Add(frame);

            player.Play();
            player.Tick(0.04f);
            Require(player.CurrentFrame == 0 && evaluatedFrames.Count == 0,
                "不足一帧的增量提前触发了求值。");

            player.Tick(0.06f);
            Require(player.CurrentFrame == 1 && Approximately(player.CurrentTime, 0.1f),
                "累积到一帧后没有触发正确帧。");

            player.Tick(0.25f);
            Require(player.CurrentFrame == 3 && Approximately(player.CurrentTime, 0.3f),
                "跨多帧推进后的结果错误。");
            Require(IsSequence(evaluatedFrames, 1, 2, 3),
                "SkillPlayer 跨多帧时没有逐帧发出事件。");

            var stoppedPlayer = new SkillPlayer(skill);
            stoppedPlayer.Play();
            stoppedPlayer.Tick(0.04f);
            stoppedPlayer.Stop();
            stoppedPlayer.Play();
            stoppedPlayer.Tick(0.06f);
            Require(stoppedPlayer.CurrentFrame == 0,
                "Stop 没有清除不足一帧的内部累积时间。");

            Debug.Log("逐帧求值验证通过。");
        }

        private static bool IsSequence(List<int> values, params int[] expected)
        {
            if (values.Count != expected.Length)
                return false;

            for (var i = 0; i < expected.Length; i++)
            {
                if (values[i] != expected[i])
                    return false;
            }

            return true;
        }

        private static bool Approximately(float left, float right)
        {
            return Math.Abs(left - right) < 0.0001f;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
