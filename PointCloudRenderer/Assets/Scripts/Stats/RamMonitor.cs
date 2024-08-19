using UnityEngine;
using UnityEngine.Profiling;

namespace BlocInBloc.Stats {
    public class RamMonitor : MonoBehaviour {
        public float AllocatedRam { get { return _allocatedRam; } }
        public float ReservedRam { get { return _reservedRam; } }
        public float MonoRam { get { return _monoRam; } }

        private float _allocatedRam = 0;
        private float _reservedRam = 0;
        private float _monoRam = 0;

        private void Update () {
            _allocatedRam = Profiler.GetTotalAllocatedMemoryLong () / 1048576f;
            _reservedRam = Profiler.GetTotalReservedMemoryLong () / 1048576f;
            _monoRam = Profiler.GetMonoUsedSizeLong () / 1048576f;
        }
    }
}