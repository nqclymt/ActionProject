using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullSerializer;
using UnityEngine;

namespace PKC.ActionEditor
{
    [Serializable]
    public abstract class Asset : IDirector
    {
        [HideInInspector] public List<Group> groups = new();
        [SerializeField] private float length = 5f;
        [SerializeField] private float viewTimeMin;
        [SerializeField] private float viewTimeMax = 5f;

        [SerializeField] private float rangeMin;
        [SerializeField] private float rangeMax = 5f;

        public Asset()
        {
            Init();
        }


        [fsIgnore] public List<IDirectable> directables { get; private set; }

        public float Length
        {
            get => length;
            set => length = Mathf.Max(value, 0.1f);
        }

        public float ViewTimeMin
        {
            get => viewTimeMin;
            set
            {
                if (ViewTimeMax > 0) viewTimeMin = Mathf.Min(value, ViewTimeMax - 0.25f);
            }
        }

        public float ViewTimeMax
        {
            get => viewTimeMax;
            set => viewTimeMax = Mathf.Max(value, ViewTimeMin + 0.25f, 0);
        }


        public float ViewTime => ViewTimeMax - ViewTimeMin;

        public float RangeMin
        {
            get => rangeMin;
            set
            {
                rangeMin = Mathf.Clamp(value, 0, Length);
                if (rangeMax < rangeMin) rangeMax = rangeMin;
            }
        }
        

        public float RangeMax
        {
            get => rangeMax;
            set
            {
                rangeMax = Mathf.Clamp(value, RangeMin, Length);
            }
        }

        protected virtual float MinimumLength => 0.1f;

        protected virtual void OnValidateAsset()
        {
        }


        public void UpdateMaxTime()
        {
            groups ??= new List<Group>();
            var previousLength = Length;
            var rangeCoveredWholeAsset = rangeMax <= rangeMin + 0.0001f ||
                                         rangeMax >= previousLength - 0.0001f;
            var t = 0f;
            foreach (var group in groups.Where(group => group != null))
            {
                if (!group.IsActive) continue;
                foreach (var track in group.Tracks)
                {
                    if (!track.IsActive) continue;
                    foreach (var clip in track.Clips)
                        if (clip.EndTime > t)
                            t = clip.EndTime;
                }
            }

            Length = Mathf.Max(t, MinimumLength);
            RangeMin = rangeMin;
            RangeMax = rangeCoveredWholeAsset ? Length : rangeMax;
        }

        public void DeleteGroup(Group group)
        {
            groups.Remove(group);
            Validate();
        }

        public void Validate()
        {
            OnValidateAsset();
            groups ??= new List<Group>();
            groups.RemoveAll(group => group == null);
            directables = new List<IDirectable>();
            foreach (IDirectable group in groups.AsEnumerable().Reverse())
            {
                directables.Add(group);
                try
                {
                    group.Validate(this, null);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                foreach (var track in group.Children.Where(track => track != null).Reverse())
                {
                    directables.Add(track);
                    try
                    {
                        track.Validate(this, group);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    foreach (var clip in track.Children.Where(clip => clip != null))
                    {
                        directables.Add(clip);
                        try
                        {
                            clip.Validate(this, track);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }

            if (directables != null)
                foreach (var d in directables)
                    d.OnAfterDeserialize();

            UpdateMaxTime();
        }

        public Group AddGroup(Type type)
        {
            if (!typeof(Group).IsAssignableFrom(type)) return null;
            var newGroup = Activator.CreateInstance(type) as Group;
            if (newGroup != null)
            {
                newGroup.Name = "New Group";
                groups.Add(newGroup);
                Validate();
            }

            return newGroup;
        }

        public T AddGroup<T>(string name = "") where T : Group, new()
        {
            var newGroup = new T();
            if (string.IsNullOrEmpty(name))
            {
                name = newGroup.GetType().Name;
            }

            newGroup.Name = name;
            groups.Add(newGroup);
            Validate();
            return newGroup;
        }


        public void Init()
        {
            Validate();
        }

        public void OnBeforeSerialize()
        {
            if (directables != null)
                foreach (var d in directables)
                    d.OnBeforeSerialize();

            // groupStr = FullSerializerExtensions.Serialize(typeof(List<Group>), groups);
        }

        public void OnAfterDeserialize()
        {
            // if (!string.IsNullOrEmpty(groupStr))
            // {
            //     var obj = FullSerializerExtensions.Deserialize(typeof(List<Group>), groupStr);
            //     if (obj is List<Group> list)
            //     {
            //         groups = list;
            //     }
            // }
        }
    }
}
