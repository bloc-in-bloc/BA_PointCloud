using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlocInBloc.Stats {
    public class FpsText : MonoBehaviour {
        public TMP_Text currentFPSValue;
        public TMP_Text currentEstimateRenderFrameRate;
        public TMP_Text requestRenderFrameInterval;
        public TMP_Text minFPSValue;
        public TMP_Text maxFPSValue;
        public TMP_Text avgFPSValue;
        public int goodFPSThreshold = 60;
        public Color goodFPSColor;
        public int cautionFPSThreshold = 30;
        public Color cautionFPSColor;
        public Color criticalFPSColor;

        private FpsMonitor _fpsMonitor = null;
        private int _updateRate = 4; // 4 updates per sec.
        private int _frameCount = 0;
        private float _deltaTime = 0f;
        private float _fps = 0f;
        private const int _minFps = 0;
        private const int _maxFps = 10000;

        void Awake () {
            Init ();
        }

        void OnDestroy () {
            IntString.Dispose ();
        }

        private void Update () {
            _deltaTime += Time.unscaledDeltaTime;
            _frameCount++;

            // Only update texts 'm_updateRate' times per second
            if (_deltaTime > 1f / _updateRate) {
                _fps = _frameCount / _deltaTime;

                // Update fps
                currentFPSValue.text = Mathf.Clamp (Mathf.RoundToInt (_fps), 0, 2000).ToStringNonAlloc ();
                SetFpsRelatedTextColor (currentFPSValue, _fps);

                // Update min fps
                minFPSValue.text = Mathf.Clamp (((int)_fpsMonitor.MinFPS), 0, 2000).ToStringNonAlloc ();
                SetFpsRelatedTextColor (minFPSValue, _fpsMonitor.MinFPS);

                // Update max fps
                maxFPSValue.text = Mathf.Clamp (((int)_fpsMonitor.MaxFPS), 0, 2000).ToStringNonAlloc ();
                SetFpsRelatedTextColor (maxFPSValue, _fpsMonitor.MaxFPS);

                // Update avg fps
                avgFPSValue.text = Mathf.Clamp (((int)_fpsMonitor.AverageFPS), 0, 2000).ToStringNonAlloc ();
                SetFpsRelatedTextColor (avgFPSValue, _fpsMonitor.AverageFPS);

                currentEstimateRenderFrameRate.text = OnDemandRendering.effectiveRenderFrameRate.ToString ();
                requestRenderFrameInterval.text = OnDemandRendering.renderFrameInterval.ToString ();
                // Reset variables
                _deltaTime = 0f;
                _frameCount = 0;
            }
        }

        void SetFpsRelatedTextColor (TMP_Text text, float fps) {
            if (fps > goodFPSThreshold) {
                text.color = goodFPSColor;
            } else if (fps > cautionFPSThreshold) {
                text.color = cautionFPSColor;
            } else {
                text.color = criticalFPSColor;
            }
        }

        void Init () {
            IntString.Init (0, 2000);
            _fpsMonitor = FpsMonitor.Instance;
        }
    }
}