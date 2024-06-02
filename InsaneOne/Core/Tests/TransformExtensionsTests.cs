using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
    public class TransformExtensionsTests
    {
        void Prepare2D(out Vector2 lookPosition, out Transform transform)
        {
            lookPosition = new Vector2(0, 2);
            var go = new GameObject("Test Object");
            transform = go.transform;
        }

        [Test]
        public void TestLookAngle2D_ForwardIsUp()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = false;
            Prepare2D(out var lookPosition, out var transform);

            var lookAngle = transform.GetAngleLook2D(lookPosition);

            Assert.AreEqual(0, lookAngle);
        }

        [Test]
        public void TestLookAngle2D_ForwardIsRight()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = true;
            Prepare2D(out var lookPosition, out var transform);

            var lookAngle = transform.GetAngleLook2D(lookPosition);

            Assert.AreEqual(90, lookAngle);
        }

        [Test]
        public void TestLookAt2D_ForwardIsUp()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = false;
            Prepare2D(out var lookPosition, out var transform);

            var result = transform.GetLook2D(lookPosition);
            var lookAngle = result.eulerAngles.z;

            Assert.AreEqual(0, lookAngle);
        }

        [Test]
        public void TestLookAt2D_ForwardIsRight()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = true;
            Prepare2D(out var lookPosition, out var transform);

            var result = transform.GetLook2D(lookPosition);
            var lookAngle = result.eulerAngles.z;

            Assert.AreEqual(90, lookAngle);
        }

        [Test]
        public void TestGetDirectionFromAngle2D_ForwardIsRight()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = true;
            Prepare2D(out var lookPosition, out var transform);

            var lookAngle = transform.GetAngleLook2D(lookPosition);
            transform.Set2DRotation(lookAngle);

            var direction = TransformExtensions.GetDirectionFromAngle2D(lookAngle);
            var angleDiff = Vector2.Angle(direction, Vector2.up);

            Assert.AreEqual(0, angleDiff);
        }

        // same to previous (only axis in TransformExtensions changed), because same direction should be returned in same conditions
        [Test]
        public void TestGetDirectionFromAngle2D_ForwardIsUp()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = false;
            Prepare2D(out var lookPosition, out var transform);

            var lookAngle = transform.GetAngleLook2D(lookPosition);
            transform.Set2DRotation(lookAngle);

            var direction = TransformExtensions.GetDirectionFromAngle2D(lookAngle);
            var angleDiff = Vector2.Angle(direction, Vector2.up);

            Assert.AreEqual(0, angleDiff);
        }

        void PrepareTwoTransforms(out Transform transformA, out Transform transformB)
        {
            var goA = new GameObject("Test Object A");
            var goB = new GameObject("Test Object B");

            transformA = goA.transform;
            transformB = goB.transform;
        }

        [Test]
        public void TestIsLookingAt2D_ForwardIsUp()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = false;
            PrepareTwoTransforms(out var transformA, out var transformB);
            transformB.position = new Vector2(0, 2);

            Assert.IsTrue(transformA.IsLookingAt2D(transformB));

            transformB.position = new Vector2(2, 0);

            Assert.IsFalse(transformA.IsLookingAt2D(transformB));
        }

        [Test]
        public void TestIsLookingAt2D_ForwardIsRight()
        {
            TransformExtensions.RightAxisIsForwardInTwoD = true;
            PrepareTwoTransforms(out var transformA, out var transformB);
            transformB.position = new Vector2(2, 0);

            Assert.IsTrue(transformA.IsLookingAt2D(transformB));

            transformB.position = new Vector2(0, 2);

            Assert.IsFalse(transformA.IsLookingAt2D(transformB));
        }

        [Test]
        public void TestIsLookingAt3D()
        {
            PrepareTwoTransforms(out var transformA, out var transformB);
            transformB.position = new Vector3(0, 0, 2);

            Assert.IsTrue(transformA.IsLookingAt(transformB));

            transformB.position = new Vector3(2, 0, 0);

            Assert.IsFalse(transformA.IsLookingAt(transformB));
        }
    }
}