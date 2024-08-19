using System.Collections.Generic;
using System.Linq;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation {
    public class ComputeBufferGlobalConfiguration : MeshConfiguration {
        public Shader shader;
        private Dictionary<GameObject, ComputeBufferHolder> _gameObjectCollection = null;
        private ComputeBuffer _pointsbuffer;
        private ComputeBuffer _colorsbuffer;
        private Material _material;
        private int _numberOfPoints;

        public bool useTexture = true;
        public Texture2D _pointsTexture;
        public Texture2D _colorsTexture;

        public void Start () {
            
            Debug.Log($"System supports compute shaders {SystemInfo.supportsComputeShaders}");
            _gameObjectCollection = new Dictionary<GameObject, ComputeBufferHolder> ();
            
            if (_material == null) {
                _material = new Material (shader);
                _material.SetFloat ("_PointSize", 10);
                _material.SetBuffer ("_PointsBuffer", _pointsbuffer);
                _material.SetBuffer ("_ColorsBuffer", _colorsbuffer);
                _material.enableInstancing = true;
            }
        }

        public override GameObject CreateGameObject (string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject gameObject = new GameObject (name);

            //Set Translation
            if (version == "2.0") {
                // 20230125: potree v2 vertices have absolute coordinates,
                // hence all gameobjects need to reside at Vector.Zero.
                // And: the position must be set after parenthood has been granted.
                //gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent (parent, false);
                gameObject.transform.localPosition = translationV2.ToFloatVector ();
            } else {
                gameObject.transform.Translate (boundingBox.Min ().ToFloatVector ());
                gameObject.transform.SetParent (parent, false);
            }
            
            ComputeBufferHolder computeBufferHolder = gameObject.AddComponent<ComputeBufferHolder> ();
            computeBufferHolder.Init (shader, vertexData, colorData);

            gameObject.AddComponent<BoundingBoxComponent> ().boundingBox = boundingBox;

            if (_gameObjectCollection != null) {
                _gameObjectCollection.Add (gameObject, computeBufferHolder);
            }
            //Debug.Log ($"{name} / {vertexData.Length}");

            return gameObject;
        }

        public override void RemoveGameObject (GameObject gameObject) {
            if (_gameObjectCollection != null) {
                _gameObjectCollection.Remove (gameObject);
            }
            if (gameObject != null) {
                Destroy (gameObject);
            }
        }

        void LateUpdate () {
            
            int numberOfPoints = _gameObjectCollection.Values.Sum ((holder => holder._nbPoints));
            if(numberOfPoints ==0 ) return;

            if (useTexture) {
                // Dispose of old textures if they exist
                if (_pointsTexture != null) Destroy(_pointsTexture);
                if (_colorsTexture != null) Destroy(_colorsTexture);

                // Determine texture dimensions (choose a square-like aspect ratio for better compatibility)
                int textureWidth = Mathf.CeilToInt(Mathf.Sqrt(numberOfPoints));
                int textureHeight = Mathf.CeilToInt((float)numberOfPoints / textureWidth);

                // Create new textures
                _pointsTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false);
                _colorsTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

                // Prepare arrays to store data
                Color[] pointData = new Color[textureWidth * textureHeight];
                Color[] colorData = new Color[textureWidth * textureHeight];

                int index = 0;
                foreach (ComputeBufferHolder computeBufferHolder in _gameObjectCollection.Values.ToArray()) {
                    for (int i = 0; i < computeBufferHolder._nbPoints; i++) {
                        if (index >= pointData.Length) break;

                        float3 point = computeBufferHolder.vertices[i];
                        uint color = computeBufferHolder.colors[i];

                        // Store point data in RGBA components of Color
                        pointData[index] = new Color(point.x, point.y, point.z, 1.0f);

                        // Decode color as in the shader
                        float r = (color & 0xff) ;
                        float g = ((color >> 8) & 0xff) ;
                        float b = ((color >> 16) & 0xff) ;
                        float a = ((color >> 24) & 0xff) ;
                        colorData[index] = new Color(r, g, b) * a * 16 / (255.0f * 255.0f);

                        index++;
                    }
                }
                // Apply the data to the textures
                _pointsTexture.SetPixels(pointData);
                _pointsTexture.Apply();

                _colorsTexture.SetPixels(colorData);
                _colorsTexture.Apply();
                
                Debug.Log ($"points: {_pointsTexture.width}x{_pointsTexture.height}, colors: {_colorsTexture.width}x{_colorsTexture.height}");
                
                _material.SetInt("_TextureWidth", textureWidth);
                _material.SetInt("_TextureHeight", textureHeight);
            } else {
                if(_pointsbuffer != null) _pointsbuffer.Dispose ();
                if(_colorsbuffer != null) _colorsbuffer.Dispose ();
            
                _pointsbuffer = new ComputeBuffer (numberOfPoints *4, sizeof (float) * 3, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
                _colorsbuffer = new ComputeBuffer (numberOfPoints *4, sizeof (uint), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
            
                int index = 0;
                foreach (ComputeBufferHolder computeBufferHolder in _gameObjectCollection.Values.ToArray ()) {
                    NativeArray<float3> tmpPoints = _pointsbuffer.BeginWrite<float3> (index, computeBufferHolder._nbPoints);
                    tmpPoints.CopyFrom (computeBufferHolder.vertices);
                    _pointsbuffer.EndWrite<float3> (computeBufferHolder._nbPoints);

                    NativeArray<uint> tmpColors = _colorsbuffer.BeginWrite<uint> (index, computeBufferHolder._nbPoints);
                    tmpColors.CopyFrom (computeBufferHolder.colors);
                    _colorsbuffer.EndWrite<uint> (computeBufferHolder._nbPoints);

                    index += computeBufferHolder._nbPoints;
                }

            }

            _numberOfPoints = numberOfPoints;

        }

        void OnRenderObject () {
            if (useTexture) {
                _material.SetTexture("_PointsTexture", _pointsTexture);
                _material.SetTexture("_ColorsTexture", _colorsTexture);
            } else {
                _material.SetBuffer ("_PointsBuffer", _pointsbuffer);
                _material.SetBuffer ("_ColorsBuffer", _colorsbuffer);
            }
            _material.SetPass (0);
            Graphics.DrawProceduralNow (MeshTopology.Points, _numberOfPoints);
        }

        public override int GetMaximumPointsPerMesh () {
            return 10000000;
        }
    }
}