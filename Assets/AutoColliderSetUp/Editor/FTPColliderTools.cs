using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class FTPColliderTools
    {
        public static void CopyLineSphereColliderWithAvatar(GameObject src, GameObject des)
        {
            if (src == null || des == null)
            {
                Debug.LogError("_src == null || _des == null");
                return;
            }

            Animator animatorSrc = src.GetComponent<Animator>();
            Animator animatorDes = des.GetComponent<Animator>();
            if (animatorSrc == null || animatorDes == null)
            {
                Debug.LogError("animator src == null || animator src == null");
                return;
            }

            int boneCount = (int)HumanBodyBones.LastBone - 1;
            for (int i = 0; i < boneCount; i++)
            {
                HumanBodyBones bone = (HumanBodyBones)i;
                Transform trs01 = animatorSrc.GetBoneTransform(bone);
                Transform trsDes = animatorDes.GetBoneTransform(bone);
                if (trs01 == null)
                    continue;
                if (trs01 != null && trsDes == null)
                {
                    Debug.LogError("Two humanoid does have the same bone please Check");
                    return;
                }

                LineSphereCollider[] colliders = trs01.GetComponents<LineSphereCollider>();

                RootColliderManager smanager = trsDes.GetComponent<RootColliderManager>();
                if (smanager == null)
                    smanager = trsDes.gameObject.AddComponent<RootColliderManager>();

                for (int index = 0; index < colliders.Length; index++)
                {
                    LineSphereCollider sp = trsDes.gameObject.AddComponent<LineSphereCollider>();
                    LineSphereCollider s1 = colliders[index];

                    sp.A = s1.A;
                    sp.B = s1.B;
                    sp.RadiusA = s1.RadiusA;
                    sp.RadiusB = s1.RadiusB;

                    sp.scaleAFactor = s1.scaleAFactor;
                    sp.scaleBFactor = s1.scaleBFactor;

                    sp.EditorAFactor = s1.EditorAFactor;
                    sp.EditorBFactor = s1.EditorBFactor;
                    sp.lineIndex = s1.lineIndex;
                    sp._enableCollider = s1._enableCollider;
                    sp._startBone = s1._startBone;
                    sp._endBone = s1._endBone;

                    smanager._lineSphereColliders.Add(sp);
                }
            }
        }
        public static void CopyLineSphereColliderWithSameHierarchy(GameObject src, GameObject des)
        {
            if (src == null || des == null)
            {
                Debug.LogError("_src == null || _des == null");
                return;
            }
            else
            {
                LineSphereCollider[] scripts = src.GetComponentsInChildren<LineSphereCollider>(true);
                for (int i = 0; i < scripts.Length; i++)
                {
                    Transform trs = scripts[i].transform;
                    string path = AnimationUtility.CalculateTransformPath(trs, src.transform);

                    Transform desTrs = des.transform.Find(path);
                    if (desTrs == null)
                    {
                        Debug.LogError("Fail: src and des dismatch in Hierarchy...");
                        return;
                    }
                    RootColliderManager smanager = desTrs.GetComponent<RootColliderManager>();
                    if (smanager == null)
                        smanager = desTrs.gameObject.AddComponent<RootColliderManager>();

                    LineSphereCollider sp = desTrs.gameObject.AddComponent<LineSphereCollider>();
                    LineSphereCollider s1 = scripts[i];

                    sp.A = s1.A;
                    sp.B = s1.B;
                    sp.RadiusA = s1.RadiusA;
                    sp.RadiusB = s1.RadiusB;

                    sp.scaleAFactor = s1.scaleAFactor;
                    sp.scaleBFactor = s1.scaleBFactor;

                    sp.EditorAFactor = s1.EditorAFactor;
                    sp.EditorBFactor = s1.EditorBFactor;
                    sp.lineIndex = s1.lineIndex;
                    sp._enableCollider = s1._enableCollider;
                    sp._startBone = s1._startBone;
                    sp._endBone = s1._endBone;

                    smanager._lineSphereColliders.Add(sp);
                }
            }
        }

        public static void ClearLineSphereCollider(GameObject clearTarget)
        {
            if (clearTarget != null)
            {
                LineSphereCollider[] scripts = clearTarget.GetComponentsInChildren<LineSphereCollider>(true);
                RootColliderManager[] managers = clearTarget.GetComponentsInChildren<RootColliderManager>(true);
                for (int i = 0; i < scripts.Length; i++)
                {
                    GameObject.DestroyImmediate(scripts[i]);
                }

                for (int i = 0; i < managers.Length; i++)
                {
                    GameObject.DestroyImmediate(managers[i]);
                }
            }
        }

        public static void ClearNormalCollider(GameObject clearTarget)
        {

        }
    }
}
