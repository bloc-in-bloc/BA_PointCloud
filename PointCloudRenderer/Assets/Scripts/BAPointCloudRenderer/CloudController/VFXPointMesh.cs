using UnityEngine;
using UnityEngine.VFX;

namespace BAPointCloudRenderer.CloudController {
    // Inspired by : https://www.youtube.com/watch?v=P5BgrdXis68&t=425s
    public class VFXPointMesh : MonoBehaviour {
        public VisualEffect visualEffect;
        public uint resolution = 2048;
        public float particleSize = 0.1f;
        
        public void Init (Vector3[] positions, Color[] colors) { 
            Texture2D texColor = new Texture2D(positions.Length > (int)resolution ? (int)resolution : positions.Length, Mathf.Clamp(positions.Length / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
            Texture2D texPosScale = new Texture2D(positions.Length > (int)resolution ? (int)resolution : positions.Length, Mathf.Clamp(positions.Length / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
            
            int texWidth = texColor.width;
            int texHeight = texColor.height;
 
            for (int y = 0; y < texHeight; y++) {
                for (int x = 0; x < texWidth; x++) {
                    int index = x + y * texWidth;
                    texColor.SetPixel(x, y, colors[index]);
                    var data = new Color(positions[index].x, positions[index].y, positions[index].z, particleSize);
                    texPosScale.SetPixel(x, y, data);
                }
            }
            
            texColor.Apply();
            texPosScale.Apply();
            
            uint particleCount = (uint)positions.Length;
            
            visualEffect.Reinit();
            visualEffect.SetUInt(Shader.PropertyToID("ParticleCount"), particleCount);
            visualEffect.SetTexture(Shader.PropertyToID("TexColor"), texColor);
            visualEffect.SetTexture(Shader.PropertyToID("TexPosScale"), texPosScale);
            visualEffect.SetUInt(Shader.PropertyToID("Resolution"), resolution);
        }
    }
}