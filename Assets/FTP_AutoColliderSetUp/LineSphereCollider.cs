using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class LineSphereCollider : MonoBehaviour
    {
        [SerializeField]
        public Vector3 A = Vector3.zero;
        [SerializeField]
        public Vector3 B = new Vector3(0, -0.2f, 0);
        [SerializeField]
        public float RadiusA = 0.1f;
        [SerializeField]
        public float RadiusB = 0.1f;

        public float scaleAFactor = 1.0f;
        public float scaleBFactor = 1.0f;

        public float EditorAFactor = 1.0f;
        public float EditorBFactor = 1.0f;

        // Line Index used for define get target line sphere
        public int lineIndex; // 100 101 102 103 ...... 199

        // Collider Status
        public bool _enableCollider = true;
        public HumanBodyBones _startBone = HumanBodyBones.LastBone;
        public HumanBodyBones _endBone = HumanBodyBones.LastBone;

        public Vector3 WorldA
        {
            set { A = transform.InverseTransformPoint(value); }
            get { return transform.TransformPoint(A); }
        }

        public Vector3 WorldB
        {
            set { B = transform.InverseTransformPoint(value); }
            get { return transform.TransformPoint(B); }
        }

        public float WorldRadiusA
        {
            set { RadiusA = value / Scale / scaleAFactor; }
            get { return RadiusA * Scale * scaleAFactor * EditorAFactor; }
        }

        public float WorldRadiusB
        {
            set { RadiusB = value / Scale / scaleBFactor; }
            get { return RadiusB * Scale * scaleBFactor * EditorBFactor; }
        }

        private float Scale
        {
            get { return Mathf.Max(Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), transform.lossyScale.z); }
        }

        public void EnableColliderWithRootBone(HumanBodyBones bone, bool enable)
        {
            if (_startBone == bone)
                _enableCollider = enable;
        }

        public void SetBoneScaleFactor(HumanBodyBones bone, float factor, bool isEditorFactor = true)
        {
            if (_startBone != bone && _endBone != bone)
                return;

            if (_startBone == bone)
            {
                if (isEditorFactor)
                    EditorAFactor = factor;
                else
                    RadiusA = factor;
            }

            if (_endBone == bone)
            {
                if (isEditorFactor)
                    EditorBFactor = factor;
                else
                    RadiusB = factor;
            }
        }

        public void SetBonePosition(HumanBodyBones bone, Vector3 pos)
        {
            if (_startBone != bone && _endBone != bone)
                return;

            if (_startBone == bone) { WorldA = pos; }
            if (_endBone == bone) { WorldB = pos; }
        }

        public void RefreshRealFactor()
        {
            scaleAFactor *= EditorAFactor;
            scaleBFactor *= EditorBFactor;
            EditorAFactor = EditorBFactor = 1.0f;
        }

        private void OnDrawGizmos()
        {
            if (!_enableCollider)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(WorldA, WorldRadiusA);
            Gizmos.DrawWireSphere(WorldB, WorldRadiusB);

            var dir = Vector3.Normalize(WorldA - WorldB);
            var up = Vector3.Cross(dir, new Vector3(dir.z, dir.y, -dir.x)).normalized;

            var angle = Mathf.PI / 10;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);
            var q = new Quaternion(cos * dir.x, cos * dir.y, cos * dir.z, sin);

            var identity = Quaternion.identity;

            for (var i = 0; i < 5; i++)
            {
                identity *= q;

                var mA = Matrix4x4.TRS(WorldA, identity, Vector3.one * WorldRadiusA);
                var mB = Matrix4x4.TRS(WorldB, identity, Vector3.one * WorldRadiusB);

                var p1 = mA.MultiplyPoint3x4(up);
                var p2 = mB.MultiplyPoint3x4(up);

                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}
