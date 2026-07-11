using System;
using System.Collections.Generic;

namespace PKC.ActionEditor
{
    public enum DirectableExecutionTargetType
    {
        Track,
        Clip
    }

    public enum DirectableExecutionEventType
    {
        Start,
        End
    }

    /// <summary>
    /// 描述一个时间点事件及其在资源层级中的稳定位置。
    /// </summary>
    public readonly struct DirectableExecutionEvent : IComparable<DirectableExecutionEvent>
    {
        public DirectableExecutionEvent(IDirectable directable, DirectableExecutionTargetType targetType,
            DirectableExecutionEventType eventType, float time, int groupIndex, int trackIndex,
            int itemIndex, bool isInstant)
        {
            Directable = directable;
            TargetType = targetType;
            EventType = eventType;
            Time = time;
            GroupIndex = groupIndex;
            TrackIndex = trackIndex;
            ItemIndex = itemIndex;
            IsInstant = isInstant;
        }

        public IDirectable Directable { get; }
        public DirectableExecutionTargetType TargetType { get; }
        public DirectableExecutionEventType EventType { get; }
        public float Time { get; }
        public int GroupIndex { get; }
        public int TrackIndex { get; }
        public int ItemIndex { get; }
        public bool IsInstant { get; }

        public int CompareTo(DirectableExecutionEvent other)
        {
            var result = Time.CompareTo(other.Time);
            if (result != 0) return result;

            result = GetPhase().CompareTo(other.GetPhase());
            if (result != 0) return result;

            result = GroupIndex.CompareTo(other.GroupIndex);
            if (result != 0) return result;

            result = TrackIndex.CompareTo(other.TrackIndex);
            if (result != 0) return result;

            result = ItemIndex.CompareTo(other.ItemIndex);
            if (result != 0) return result;

            result = TargetType.CompareTo(other.TargetType);
            if (result != 0) return result;

            return EventType.CompareTo(other.EventType);
        }

        private int GetPhase()
        {
            if (TargetType == DirectableExecutionTargetType.Track)
                return EventType == DirectableExecutionEventType.Start ? 0 : 5;
            if (IsInstant)
                return EventType == DirectableExecutionEventType.Start ? 2 : 3;

            return EventType == DirectableExecutionEventType.End ? 1 : 4;
        }
    }

    public static class DirectableExecutionOrder
    {
        private const float InstantEpsilon = 0.00001f;

        public static List<DirectableExecutionEvent> Build(Asset asset, bool includeTracks = true)
        {
            var events = new List<DirectableExecutionEvent>();
            if (asset?.groups == null)
                return events;

            for (var groupIndex = 0; groupIndex < asset.groups.Count; groupIndex++)
            {
                var group = asset.groups[groupIndex];
                if (group == null)
                    continue;

                var tracks = group.Tracks;
                for (var trackIndex = 0; trackIndex < tracks.Count; trackIndex++)
                {
                    var track = tracks[trackIndex];
                    if (track == null)
                        continue;

                    if (includeTracks)
                    {
                        AddPair(events, track, DirectableExecutionTargetType.Track,
                            groupIndex, trackIndex, -1);
                    }

                    var clips = track.Clips;
                    for (var clipIndex = 0; clipIndex < clips.Count; clipIndex++)
                    {
                        var clip = clips[clipIndex];
                        if (clip == null)
                            continue;

                        AddPair(events, clip, DirectableExecutionTargetType.Clip,
                            groupIndex, trackIndex, clipIndex);
                    }
                }
            }

            events.Sort();
            return events;
        }

        public static void Sort(List<DirectableExecutionEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            events.Sort();
        }

        private static void AddPair(List<DirectableExecutionEvent> events, IDirectable directable,
            DirectableExecutionTargetType targetType, int groupIndex, int trackIndex, int itemIndex)
        {
            var isInstant = Math.Abs(directable.EndTime - directable.StartTime) <= InstantEpsilon;
            events.Add(new DirectableExecutionEvent(directable, targetType,
                DirectableExecutionEventType.Start, directable.StartTime,
                groupIndex, trackIndex, itemIndex, isInstant));
            events.Add(new DirectableExecutionEvent(directable, targetType,
                DirectableExecutionEventType.End, directable.EndTime,
                groupIndex, trackIndex, targetType == DirectableExecutionTargetType.Track ? int.MaxValue : itemIndex,
                isInstant));
        }
    }
}
