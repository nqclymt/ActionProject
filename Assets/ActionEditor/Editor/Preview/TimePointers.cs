
namespace PKC.ActionEditor
{
    public interface IDirectableTimePointer
    {
        PreviewBase target { get; }
        float time { get; }
        void TriggerForward(float currentTime, float previousTime);
        void TriggerBackward(float currentTime, float previousTime);
        void Update(float currentTime, float previousTime);
    }

    public struct StartTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        private float lastTargetStartTime;
        private readonly float evaluationStartTime;
        private readonly float evaluationEndTime;
        public PreviewBase target { get; private set; }
        float IDirectableTimePointer.time => evaluationStartTime;

        public StartTimePointer(PreviewBase target, float evaluationStartTime, float evaluationEndTime)
        {
            this.target = target;
            triggered = false;
            lastTargetStartTime = target.directable.StartTime;
            this.evaluationStartTime = evaluationStartTime;
            this.evaluationEndTime = evaluationEndTime;
        }

        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.directable.IsActive) return;
            if (currentTime >= evaluationStartTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    target.Enter();
                    target.Update(target.directable.ToLocalTime(currentTime), 0);
                }
            }
        }

        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            if (!target.directable.IsActive) return;
            if (currentTime >= evaluationStartTime && currentTime < evaluationEndTime &&
                currentTime > 0)
            {
                var deltaMoveClip = target.directable.StartTime - lastTargetStartTime;
                var localCurrentTime = target.directable.ToLocalTime(currentTime);
                var localPreviousTime = target.directable.ToLocalTime(previousTime + deltaMoveClip);

                target.Update(localCurrentTime, localPreviousTime);
                lastTargetStartTime = target.directable.StartTime;
            }
        }

        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.directable.IsActive) return;
            if (currentTime < evaluationStartTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;
                    target.Update(0, target.directable.ToLocalTime(previousTime));
                    target.Reverse();
                }
            }
        }
    }

    public struct EndTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        private readonly float evaluationEndTime;
        public PreviewBase target { get; private set; }
        float IDirectableTimePointer.time => evaluationEndTime;

        public EndTimePointer(PreviewBase target, float evaluationEndTime)
        {
            this.target = target;
            triggered = false;
            this.evaluationEndTime = evaluationEndTime;
        }

        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.directable.IsActive) return;
            if (currentTime >= evaluationEndTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    target.Update(target.directable.GetLength(), target.directable.ToLocalTime(previousTime));
                    target.Exit();
                }
            }
        }


        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            
        }


        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.directable.IsActive) return;
            if (currentTime < evaluationEndTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;
                    target.ReverseEnter();
                    target.Update(target.directable.ToLocalTime(currentTime), target.directable.GetLength());
                }
            }
        }
    }
}
