using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    public class ParticulesNode : MonoBehaviour {
        public Vector3[] positions;
        public Color[] colors;
        public int numberOfPoints;

        public void Init (Vector3[] positions, Color[] colors, float particleSize) {
            this.positions = positions;
            this.colors = colors;
            this.numberOfPoints = this.positions.Length;
        }
    }
}