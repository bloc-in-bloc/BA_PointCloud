using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BAPointCloudRenderer.CloudController {
    public class ComputeBufferHolder : MonoBehaviour {
        
        public int _nbPoints;
        public NativeArray<float3> vertices;
        public NativeArray<uint> colors;

        public void Init (Shader shader, Vector3[] vertex, Color[] color) {
            if (vertex.Length != color.Length) {
                Debug.Log ($"{vertex.Length} / {color.Length}");
            }
            
            _nbPoints = vertex.Length;
            vertices = new NativeArray<float3> (_nbPoints, Allocator.Persistent); 
            colors = new NativeArray<uint> (_nbPoints, Allocator.Persistent);
            
            for (var i = 0; i < _nbPoints; i++) {
                Vector3 pos = vertex[i] + gameObject.transform.position;
                vertices[i] = new float3 (pos.x, pos.y, pos.z);
                colors[i] = EncodeColor (color[i]);
            }
        }
        
        private void OnDestroy () {
            vertices.Dispose ();
            colors.Dispose ();
        }
        
        static uint EncodeColor (Color c) {
            const float kMaxBrightness = 16;

            var y = Mathf.Max (Mathf.Max (c.r, c.g), c.b);
            y = Mathf.Clamp (Mathf.Ceil (y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3 (c.r, c.g, c.b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint) rgb.x) |
                   ((uint) rgb.y << 8) |
                   ((uint) rgb.z << 16) |
                   ((uint) y << 24);
        }
    }
}