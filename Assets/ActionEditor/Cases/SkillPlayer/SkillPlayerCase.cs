using System;
using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class SkillPlayerCase : MonoBehaviour
    {
        [ContextMenu("验证 SkillPlayer 播放控制")]
        public void ValidateSkillPlayer()
        {
            Run();
        }

        public static void Run()
        {
            var skill = new CombatSkillAsset
            {
                skillId = "case.runtime.skill_player",
                duration = 1f
            };
            skill.Validate();

            var player = new SkillPlayer(skill);
            var completedCount = 0;
            var loopedCount = 0;
            var evaluatedCount = 0;
            var interruptedReason = string.Empty;

            player.Completed += _ => completedCount++;
            player.Looped += _ => loopedCount++;
            player.TimeEvaluated += (_, _, _) => evaluatedCount++;
            player.Interrupted += (_, reason) => interruptedReason = reason;

            Require(player.Play(), "播放命令没有启动播放器。");
            Require(player.Tick(0.25f), "播放状态没有推进时间。");
            Require(Approximately(player.CurrentTime, 0.25f), "时间推进结果错误。");

            Require(player.Pause(), "暂停命令失败。");
            player.Tick(0.5f);
            Require(Approximately(player.CurrentTime, 0.25f), "暂停后时间仍在推进。");

            Require(player.Seek(0.5f), "跳转命令失败。");
            Require(Approximately(player.CurrentTime, 0.5f), "跳转时间错误。");
            Require(player.Play(), "暂停后恢复播放失败。");
            player.Tick(0.5f);
            Require(player.State == SkillPlaybackState.Completed, "播放结束后状态不正确。");
            Require(completedCount == 1, "完成事件触发次数不正确。");

            Require(player.Play(), "完成后重新播放失败。");
            player.Tick(0.2f);
            Require(player.Interrupt("case interrupt"), "打断命令失败。");
            Require(player.State == SkillPlaybackState.Interrupted, "打断状态不正确。");
            Require(interruptedReason == "case interrupt", "打断原因没有传递给事件。");

            Require(player.Stop(), "停止命令失败。");
            Require(player.State == SkillPlaybackState.Stopped && Approximately(player.CurrentTime, 0f),
                "停止后没有重置状态和时间。");

            player.Loop = true;
            Require(player.Play(), "循环播放启动失败。");
            player.Tick(2.25f);
            Require(player.State == SkillPlaybackState.Playing, "循环播放意外结束。");
            Require(loopedCount == 2, "循环事件触发次数不正确。");
            Require(Approximately(player.CurrentTime, 0.25f), "跨多轮循环后的时间错误。");
            Require(evaluatedCount >= 7, "连续时间段没有被完整求值。");

            player.Stop();
            Debug.Log("SkillPlayer 运行时播放控制验证通过。");
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
