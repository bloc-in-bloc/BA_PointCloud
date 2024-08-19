using System.Globalization;
using TMPro;
using UnityEngine;

namespace BlocInBloc.Stats {
    public class UICustomStats : MonoBehaviour {
        public FpsMonitor fpsMonitor;
        public RamMonitor ramMonitor;

        [Header ("Infos")]
        public TMP_Text versionValue;
        public TMP_Text loadingTimeValue;
        public TMP_Text networkStatusValue;

        [Header ("Assets")]
        public TMP_Text verticesValue;
        public TMP_Text trianglesValue;
        public TMP_Text objectsValue;
        public TMP_Text materialsValue;

        [Header ("AR")]
        public TMP_Text trackingStateValue;
        public TMP_Text notTrackingReasonValue;
        public TMP_Text gnssDistanceValue;

        [Header ("Specs")]
        public TMP_Text resValue;
        public TMP_Text apiValue;
        public TMP_Text osValue;
        public TMP_Text cpuValue;
        public TMP_Text ramValue;
        public TMP_Text gpuValue;
        private bool _needToUpdateTracking = false;

        public void SetActive (bool state) {
            bool isActive = state;
            if (isActive) {
                Init ();
            } else {
                DeInit ();
            }
            gameObject.SetActive (isActive);
        }

        void OnDestroy () {
            DeInit ();
        }

        void Init () {
            InitInfo ();
            InitAssets ();
            InitSpec ();
            UpdateTracking ();
        }

        void Update () {
            // We don't do this directly in "OnARStateChanged" because it cause "UnityEngine.UI.Graphic.SetVerticesDirty" error
            // It need to be called from unity thread
            if (_needToUpdateTracking) {
                UpdateTracking ();
                _needToUpdateTracking = false;
            }
            string networkStatusText = "Offline";
            if (networkStatusValue.text != networkStatusText) {
                networkStatusValue.text = networkStatusText;
            }

            string gnssDistance = "0";
            if (gnssDistanceValue.text != gnssDistance) {
                gnssDistanceValue.text = gnssDistance;
            }
        }

        void UpdateTracking () {
            //trackingStateValue.text = ARManager.isStarted ? ARSession.state.ToString () : "-";
            //notTrackingReasonValue.text = ARManager.isStarted ? ARSession.notTrackingReason.ToString () : "-";
        }

        void DeInit () {
            //ARSession.stateChanged -= OnARStateChanged;
        }

        void InitInfo () {
            /*versionValue.text = BIBVersion.version;
            if (ProjectManager.Instance != null) {
                loadingTimeValue.text = $"{(ProjectManager.Instance.totalLoadingTime / 1000f).ToString ("0.00")} s";
            } else {
                loadingTimeValue.text = "-";
            }*/
        }

        void InitAssets () {
            /*if (ProjectManager.Instance != null) {
                var nfi = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone ();
                nfi.NumberGroupSeparator = " ";
                verticesValue.text = ((Model.Instance != null) ? Model.Instance.nbVertices : -1).ToString ("#,0", nfi);
                trianglesValue.text = ((Model.Instance != null) ? Model.Instance.nbTriangles : -1).ToString ("#,0", nfi);
                materialsValue.text = ((Model.Instance != null) ? Model.Instance.nbMaterials : -1).ToString ("#,0", nfi);
                objectsValue.text = ((Model.Instance != null) ? Model.Instance.nbPrimitives : -1).ToString ("#,0", nfi);
            } else {
                trianglesValue.text = "-";
            }*/
        }

        void InitSpec () {
            Resolution res = Screen.currentResolution;
            resValue.text = $"{res.width}x{res.height}@{res.refreshRateRatio.ToString ()}Hz";
            apiValue.text = SystemInfo.graphicsDeviceVersion;
            osValue.text = $"{SystemInfo.operatingSystem}";
            cpuValue.text = $"{SystemInfo.processorType} ({SystemInfo.processorCount} cores)";
            ramValue.text = $"{SystemInfo.systemMemorySize} Mo";
            gpuValue.text = SystemInfo.graphicsDeviceName;
        }
    }
}