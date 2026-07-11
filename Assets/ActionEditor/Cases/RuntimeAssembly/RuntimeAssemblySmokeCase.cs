using UnityEngine;

namespace PKC.ActionEditor.Cases
{
    public sealed class RuntimeAssemblySmokeCase : MonoBehaviour
    {
        [SerializeField]
        private bool logOnStart;

        [SerializeField]
        private bool quitAfterStart;

        public string RuntimeAssemblyName => typeof(Asset).Assembly.GetName().Name;

        public void Configure(bool shouldLogOnStart, bool shouldQuitAfterStart)
        {
            logOnStart = shouldLogOnStart;
            quitAfterStart = shouldQuitAfterStart;
        }

        private void Start()
        {
            if (logOnStart)
                Debug.Log($"ActionEditor runtime assembly: {RuntimeAssemblyName}", this);

            if (quitAfterStart)
                Application.Quit();
        }

        [ContextMenu("Log Runtime Assembly")]
        private void LogRuntimeAssembly()
        {
            Debug.Log($"ActionEditor runtime assembly: {RuntimeAssemblyName}", this);
        }
    }
}
