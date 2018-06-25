using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class CopyColliderFromTarget : MonoBehaviour
    {
        [HeaderAttribute("_Src")]
        public GameObject _src;
        [HeaderAttribute("_Des")]
        public GameObject _des;

        [HeaderAttribute("Clear Collider")]
        public GameObject _clearTarget;
        public static void ClearCollider(GameObject clearTarget)
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


        public static void CopyFromTargetWithAvatar(GameObject src, GameObject des)
        {
            if (src == null || des == null)
            {
                Debug.LogError("_src == null || _des == null");
                return;
            }

            Animator animator01 = src.GetComponent<Animator>();
            Animator animator02 = des.GetComponent<Animator>();
            if (animator01 == null || animator02 == null)
            {
                Debug.LogError("animator01 == null || animator02 == null");
                return;
            }

            int boneCount = (int)HumanBodyBones.LastBone - 1;
            for (int i = 0; i < boneCount; i++)
            {
                HumanBodyBones bone = (HumanBodyBones)i;
                Transform trs01 = animator01.GetBoneTransform(bone);
                Transform trsDes = animator02.GetBoneTransform(bone);
                if (trs01 == null)
                    continue;
                if (trs01 != null && trsDes == null)
                {
                    Debug.LogError("两个模型的人形骨骼不一致，请检查");
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

        public static void CopyFromComponent(GameObject src, GameObject des)
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
                        Debug.LogError("复制失败，src和des的节点结构不一致，请检查");
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
    }
}
