using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    public class VFXPointMesh : MonoBehaviour {
        public Vector3[] positions;
        public Color[] colors;

        public void Init (Vector3[] positions, Color[] colors) {
            this.positions = positions;
            this.colors = colors;
        }
    }
}