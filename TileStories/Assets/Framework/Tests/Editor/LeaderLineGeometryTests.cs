using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The pure geometry of a leader line (MarkerLeaderLine): where an elbow bends and where the line stops
    // at a label's text edge. The drawn line on real markers is covered by DisplacementRealMarkerTests.
    public class LeaderLineGeometryTests
    {
        [Test]
        public void ElbowPoint_GoesAlongScreenUpFirst_ThenAcross_ForAnyCameraTurn()
        {
            // - a camera rolled 30 degrees: "up on screen" is no longer world up
            Vector3 camUp = Quaternion.Euler(0f, 0f, 30f) * Vector3.up;
            var start = new Vector3(0f, 0f, 2f);
            var end = new Vector3(1f, 0.5f, 2f);
            Vector3 bend = MarkerLeaderLine.ElbowPoint(start, end, camUp);

            Assert.AreEqual(0f, Vector3.Cross(bend - start, camUp).magnitude, 1e-5f, "first leg runs along screen up");
            Assert.AreEqual(0f, Vector3.Dot(end - bend, camUp), 1e-5f, "second leg runs across (perpendicular to screen up)");
        }

        [Test]
        public void EdgeDistance_StopsAtTheSideOrTheTop_WhicheverComesFirst()
        {
            // - a label 0.4 wide and 0.1 tall: straight across hits the side, straight down hits the bottom
            Assert.AreEqual(0.2f, MarkerLeaderLine.EdgeDistance(Vector3.right, Vector3.right, Vector3.up, 0.2f, 0.05f), 1e-5f);
            Assert.AreEqual(0.05f, MarkerLeaderLine.EdgeDistance(Vector3.down, Vector3.right, Vector3.up, 0.2f, 0.05f), 1e-5f);
            // - 45 degrees: the flat top/bottom edge is reached first
            Vector3 diagonal = new Vector3(1f, 1f, 0f).normalized;
            Assert.AreEqual(0.05f / diagonal.y, MarkerLeaderLine.EdgeDistance(diagonal, Vector3.right, Vector3.up, 0.2f, 0.05f), 1e-5f);
        }

        [Test]
        public void EdgeDistance_OfAZeroSizedRectangle_IsZero()
        {
            Assert.AreEqual(0f, MarkerLeaderLine.EdgeDistance(Vector3.right, Vector3.right, Vector3.up, 0f, 0f), 1e-6f);
        }
    }
}
