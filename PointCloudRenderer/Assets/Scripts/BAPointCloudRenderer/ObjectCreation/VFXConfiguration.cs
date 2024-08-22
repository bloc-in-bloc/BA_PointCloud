using System.Collections.Generic;
using System.Linq;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace BAPointCloudRenderer.ObjectCreation {
    // Inspired by : https://www.youtube.com/watch?v=P5BgrdXis68&t=425s
    public class VFXConfiguration : MeshConfiguration {
        public VisualEffect visualEffectPrefab;
        public uint resolution = 2048;
        public float particleSize = 0.1f;

        private static List<VFXPointMesh> _allVFXPointMesh;
        private bool _updatePointCloud = false;
        private bool isReady = true;
        private VisualEffect _currentVisualEffect;

        public void Start () {
            _allVFXPointMesh = new List<VFXPointMesh> ();
            InvokeRepeating ("CheckUpdatePointCloud", 1f, 1f);
        }

        public void CheckUpdatePointCloud () {
            if (_updatePointCloud) {
                UpdateParticles ();
                _updatePointCloud = false;
            }
        }

        public override GameObject CreateGameObject (string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject go = new GameObject (name);
            VFXPointMesh vfxPointMesh = go.AddComponent<VFXPointMesh> ();

            //Set Translation
            if (version == "2.0") {
                // 20230125: potree v2 vertices have absolute coordinates,
                // hence all gameobjects need to reside at Vector.Zero.
                // And: the position must be set after parenthood has been granted.
                //gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                go.transform.SetParent (parent, false);
                go.transform.localPosition = translationV2.ToFloatVector ();
            } else {
                go.transform.Translate (boundingBox.Min ().ToFloatVector ());
                go.transform.SetParent (parent, false);
            }

            vfxPointMesh.Init (vertexData, colorData, particleSize);

            go.AddComponent<BoundingBoxComponent> ().boundingBox = boundingBox;

            _allVFXPointMesh.Add (vfxPointMesh);

            _updatePointCloud = true;

            return go;
        }

        public override void RemoveGameObject (GameObject gameObject) {
            _allVFXPointMesh.Remove (gameObject.GetComponent<VFXPointMesh> ());
            if (gameObject != null) {
                Destroy (gameObject);
            }
            _updatePointCloud = true;
        }

        public override int GetMaximumPointsPerMesh () {
            return 65535;
        }

        private void UpdateParticles () {
            SetParticles ();
        }

        private async void SetParticles () {
            int totalNumberOfPoints = _allVFXPointMesh.Sum ((holder => holder.numberOfPoints));

            Texture2D texColor = new Texture2D (totalNumberOfPoints > (int) resolution ? (int) resolution : totalNumberOfPoints,
                Mathf.Clamp (totalNumberOfPoints / (int) resolution, 1, (int) resolution),
                TextureFormat.RGBAFloat,
                false);

            Texture2D texPosScale = new Texture2D (totalNumberOfPoints > (int) resolution ? (int) resolution : totalNumberOfPoints,
                Mathf.Clamp (totalNumberOfPoints / (int) resolution, 1, (int) resolution),
                TextureFormat.RGBAFloat,
                false);

            List<Color> positions = new List<Color> (totalNumberOfPoints);
            List<Color> colors = new List<Color> (totalNumberOfPoints);

            foreach (VFXPointMesh mesh in _allVFXPointMesh) {
                positions.AddRange (mesh.positionsAsColor);
                colors.AddRange (mesh.colors);
            }

            texPosScale.SetPixels (positions.ToArray ());
            texColor.SetPixels (colors.ToArray ());

            texColor.Apply ();
            texPosScale.Apply ();

            uint particleCount = (uint) totalNumberOfPoints;

            VisualEffect visualEffect = GameObject.Instantiate (visualEffectPrefab);
            visualEffect.Reinit ();
            visualEffect.SetUInt (Shader.PropertyToID ("ParticleCount"), particleCount);
            visualEffect.SetTexture (Shader.PropertyToID ("TexColor"), texColor);
            visualEffect.SetTexture (Shader.PropertyToID ("TexPosScale"), texPosScale);
            visualEffect.SetUInt (Shader.PropertyToID ("Resolution"), resolution);

            // Prevent visual effect flickering
            await UniTask.WaitForSeconds (0.1f);

            if (_currentVisualEffect != null) {
                GameObject.DestroyImmediate (_currentVisualEffect.gameObject);
            }
            _currentVisualEffect = visualEffect;
        }
    }
}