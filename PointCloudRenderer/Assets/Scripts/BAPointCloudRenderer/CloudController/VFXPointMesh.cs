using System;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    public class VFXPointMesh : MonoBehaviour {
        public Vector3[] positions;
        public Color[] positionsAsColor;
        public Color[] colors;
        public int numberOfPoints;

        public void Init (Vector3[] positions, Color[] colors, float particleSize) {
            this.positions = positions;
            this.positionsAsColor = Array.ConvertAll (this.positions, p => new Color (p.x, p.y, p.z, particleSize));
            this.colors = colors;
            this.numberOfPoints = this.positions.Length;
        }
    }
}