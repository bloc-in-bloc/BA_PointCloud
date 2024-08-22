using System.Collections.Generic;
using System.Linq;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using UnityEngine;
using UnityEngine.VFX;

namespace BAPointCloudRenderer.ObjectCreation {
    // Inspired by : https://www.youtube.com/watch?v=y6KwsRkQ86U&t=148s
    public class ParticulesConfiguration : MeshConfiguration {
        public ParticleSystem targetParticleSystem;
        public float particleSize = 0.1f;

        private static List<ParticulesNode> _allParticulesNode;
        private bool _updatePointCloud = false;
        private bool isReady = true;
        private VisualEffect _currentVisualEffect;
        private ParticleSystem.Particle[] _particles;
        private bool _needToUpdateParticles = false;

        public void Start () {
            _allParticulesNode = new List<ParticulesNode> ();
            InvokeRepeating ("CheckUpdatePointCloud", 1f, 1f);
        }

        private void Update () {
            if (_needToUpdateParticles) {
                targetParticleSystem.SetParticles (_particles, _particles.Length);
                _needToUpdateParticles = false;
            }
        }

        public void CheckUpdatePointCloud () {
            if (_updatePointCloud) {
                UpdateParticles ();
                _updatePointCloud = false;
            }
        }

        public override GameObject CreateGameObject (string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject go = new GameObject (name);
            ParticulesNode particuleNode = go.AddComponent<ParticulesNode> ();

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

            particuleNode.Init (vertexData, colorData, particleSize);

            go.AddComponent<BoundingBoxComponent> ().boundingBox = boundingBox;

            _allParticulesNode.Add (particuleNode);

            _updatePointCloud = true;

            return go;
        }

        public override void RemoveGameObject (GameObject gameObject) {
            _allParticulesNode.Remove (gameObject.GetComponent<ParticulesNode> ());
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

        private void SetParticles () {
            int totalNumberOfPoints = _allParticulesNode.Sum ((holder => holder.numberOfPoints));

            _particles = new ParticleSystem.Particle [totalNumberOfPoints];
            
            List<Vector3> positions = new List<Vector3> (totalNumberOfPoints);
            List<Color> colors = new List<Color> (totalNumberOfPoints);

            foreach (ParticulesNode particuleNode in _allParticulesNode) {
                positions.AddRange (particuleNode.positions);
                colors.AddRange (particuleNode.colors);
            }

            for (int i = 0; i < totalNumberOfPoints; i++) {
                _particles[i].position = positions[i];
                _particles[i].startColor = colors[i];
                _particles[i].startSize = particleSize;
            }

            _needToUpdateParticles = true;
        }
    }
}