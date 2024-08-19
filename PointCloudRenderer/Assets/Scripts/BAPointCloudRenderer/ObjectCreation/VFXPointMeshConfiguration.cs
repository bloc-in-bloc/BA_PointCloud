using System;
using System.Collections.Generic;
using System.Linq;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using UnityEngine;
using UnityEngine.VFX;

namespace BAPointCloudRenderer.ObjectCreation {
    // Inspired by : https://www.youtube.com/watch?v=P5BgrdXis68&t=425s
    public class VFXPointMeshConfiguration : MeshConfiguration {
        public VisualEffect visualEffect;
        public VFXPointMesh vfxPointMeshPrefab;
        public uint resolution = 2048;
        public float particleSize = 0.1f;

        private static HashSet<VFXPointMesh> _allVFXPointMesh;
        private bool _updatePointCloud = false;

        public void Start () {
            _allVFXPointMesh = new HashSet<VFXPointMesh> ();
        }

        public void FixedUpdate () {
            if (_updatePointCloud) {
                UpdateParticles ();
                _updatePointCloud = false;
            }
        }

        public override GameObject CreateGameObject (string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            VFXPointMesh vfxPointMesh = GameObject.Instantiate (vfxPointMeshPrefab);
            vfxPointMesh.name = name;
            GameObject go = vfxPointMesh.gameObject;

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

            vfxPointMesh.Init (vertexData, colorData);

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
            Vector3[] positions = _allVFXPointMesh.SelectMany (pm => pm.positions.Select (p => pm.transform.TransformPoint (p))).ToArray ();
            Color[] colors = _allVFXPointMesh.SelectMany (pm => pm.colors).ToArray ();
            SetParticles (positions, colors);
        }

        private void SetParticles (Vector3[] positions, Color[] colors) {
            Texture2D texColor = new Texture2D (positions.Length > (int) resolution ? (int) resolution : positions.Length, Mathf.Clamp (positions.Length / (int) resolution, 1, (int) resolution), TextureFormat.RGBAFloat, false);
            Texture2D texPosScale = new Texture2D (positions.Length > (int) resolution ? (int) resolution : positions.Length, Mathf.Clamp (positions.Length / (int) resolution, 1, (int) resolution), TextureFormat.RGBAFloat, false);

            int texWidth = texColor.width;
            int texHeight = texColor.height;

            for (int y = 0; y < texHeight; y++) {
                for (int x = 0; x < texWidth; x++) {
                    int index = x + y * texWidth;
                    texColor.SetPixel (x, y, colors[index]);
                    var data = new Color (positions[index].x, positions[index].y, positions[index].z, particleSize);
                    texPosScale.SetPixel (x, y, data);
                }
            }

            texColor.Apply ();
            texPosScale.Apply ();

            uint particleCount = (uint) positions.Length;

            visualEffect.Reinit ();
            visualEffect.SetUInt (Shader.PropertyToID ("ParticleCount"), particleCount);
            visualEffect.SetTexture (Shader.PropertyToID ("TexColor"), texColor);
            visualEffect.SetTexture (Shader.PropertyToID ("TexPosScale"), texPosScale);
            visualEffect.SetUInt (Shader.PropertyToID ("Resolution"), resolution);
        }
    }
}