using System;
using System.Collections.Generic;
using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class DeterministicOrderingCase : MonoBehaviour
    {
        [ContextMenu("验证同帧确定性排序")]
        public void ValidateDeterministicOrdering()
        {
            Run();
        }

        public static void Run()
        {
            var input = CreateUnsortedEvents();
            var reversedInput = new List<DirectableExecutionEvent>(input);
            reversedInput.Reverse();

            DirectableExecutionOrder.Sort(input);
            DirectableExecutionOrder.Sort(reversedInput);

            Require(input.Count == 9, "排序案例事件数量错误。");
            for (var i = 0; i < input.Count; i++)
                Require(IsSameKey(input[i], reversedInput[i]), "输入顺序改变了最终执行顺序。");

            Require(Math.Abs(input[0].Time - 0.5f) < 0.0001f, "较早的事件没有优先执行。");
            Require(IsEvent(input[1], DirectableExecutionTargetType.Track,
                    DirectableExecutionEventType.Start, 0, 0, -1),
                "同帧时轨道没有先建立执行上下文。");
            Require(IsEvent(input[2], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.End, 0, 0, 0),
                "同帧时已结束片段没有先退出。");
            Require(IsEvent(input[3], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.Start, 0, 0, 2) && input[3].IsInstant,
                "零长度片段没有按进入阶段执行。");
            Require(IsEvent(input[4], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.End, 0, 0, 2) && input[4].IsInstant,
                "零长度片段没有按退出阶段执行。");
            Require(IsEvent(input[5], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.Start, 0, 0, 1),
                "新片段没有按照片段索引进入。");
            Require(IsEvent(input[6], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.Start, 0, 0, 3),
                "同轨道片段索引顺序不稳定。");
            Require(IsEvent(input[7], DirectableExecutionTargetType.Clip,
                    DirectableExecutionEventType.Start, 1, 0, 0),
                "组索引顺序不稳定。");
            Require(IsEvent(input[8], DirectableExecutionTargetType.Track,
                    DirectableExecutionEventType.End, 0, 0, int.MaxValue),
                "轨道没有在片段之后释放执行上下文。");

            Debug.Log("同帧确定性排序验证通过。");
        }

        private static List<DirectableExecutionEvent> CreateUnsortedEvents()
        {
            return new List<DirectableExecutionEvent>
            {
                Event(DirectableExecutionTargetType.Track, DirectableExecutionEventType.End,
                    1f, 0, 0, int.MaxValue),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.Start,
                    1f, 1, 0, 0),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.End,
                    1f, 0, 0, 2, true),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.Start,
                    1f, 0, 0, 3),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.Start,
                    0.5f, 0, 0, 0),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.End,
                    1f, 0, 0, 0),
                Event(DirectableExecutionTargetType.Track, DirectableExecutionEventType.Start,
                    1f, 0, 0, -1),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.Start,
                    1f, 0, 0, 2, true),
                Event(DirectableExecutionTargetType.Clip, DirectableExecutionEventType.Start,
                    1f, 0, 0, 1)
            };
        }

        private static DirectableExecutionEvent Event(DirectableExecutionTargetType targetType,
            DirectableExecutionEventType eventType, float time, int groupIndex, int trackIndex,
            int itemIndex, bool isInstant = false)
        {
            return new DirectableExecutionEvent(null, targetType, eventType, time,
                groupIndex, trackIndex, itemIndex, isInstant);
        }

        private static bool IsEvent(DirectableExecutionEvent value,
            DirectableExecutionTargetType targetType, DirectableExecutionEventType eventType,
            int groupIndex, int trackIndex, int itemIndex)
        {
            return value.TargetType == targetType && value.EventType == eventType &&
                   value.GroupIndex == groupIndex && value.TrackIndex == trackIndex &&
                   value.ItemIndex == itemIndex;
        }

        private static bool IsSameKey(DirectableExecutionEvent left, DirectableExecutionEvent right)
        {
            return left.TargetType == right.TargetType && left.EventType == right.EventType &&
                   Math.Abs(left.Time - right.Time) < 0.0001f &&
                   left.GroupIndex == right.GroupIndex && left.TrackIndex == right.TrackIndex &&
                   left.ItemIndex == right.ItemIndex && left.IsInstant == right.IsInstant;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
