using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class BodyJointChainsManager
    {
        public class JointConfig
        {
            public HumanBodyBones _rootBone;
            public List<HumanBodyBones> _bones;
        }

        public static void GetLineSpereCollider(Transform trsRoot, ref List<LineSphereCollider> list)
        {
            if (trsRoot == null || list == null)
                return;
            LineSphereCollider[] lscs = trsRoot.gameObject.GetComponents<LineSphereCollider>();
            list.AddRange(lscs);
        }

        public static List<JointConfig> _listJointConfigs007 = new List<JointConfig>
        {
            // Hips
            new JointConfig() { _rootBone = HumanBodyBones.Hips, _bones = new List<HumanBodyBones>() { HumanBodyBones.Spine, HumanBodyBones.RightUpperLeg, HumanBodyBones.LeftUpperLeg } },
            // Spine
            new JointConfig() { _rootBone = HumanBodyBones.Spine, _bones = new List<HumanBodyBones>() { HumanBodyBones.Chest } },
            // Chest
            new JointConfig() { _rootBone = HumanBodyBones.Chest, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightShoulder, HumanBodyBones.LeftShoulder, HumanBodyBones.Neck } },
            // Neck
            new JointConfig() { _rootBone = HumanBodyBones.Neck, _bones = new List<HumanBodyBones>() { HumanBodyBones.Head } },
            // Shoulder
            new JointConfig() { _rootBone = HumanBodyBones.LeftShoulder, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftUpperArm} },
            // Arm
            new JointConfig() { _rootBone = HumanBodyBones.RightShoulder, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightUpperArm} },
            new JointConfig() { _rootBone = HumanBodyBones.RightUpperArm, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightLowerArm} },
            new JointConfig() { _rootBone = HumanBodyBones.RightLowerArm, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightHand} },
            new JointConfig() { _rootBone = HumanBodyBones.LeftShoulder, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftUpperArm} },
            new JointConfig() { _rootBone = HumanBodyBones.LeftUpperArm, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftLowerArm} },
            new JointConfig() { _rootBone = HumanBodyBones.LeftLowerArm, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftHand} },
            // Leg
            new JointConfig() { _rootBone = HumanBodyBones.LeftUpperLeg, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftLowerLeg} },
            new JointConfig() { _rootBone = HumanBodyBones.LeftLowerLeg, _bones = new List<HumanBodyBones>() { HumanBodyBones.LeftFoot} },
            new JointConfig() { _rootBone = HumanBodyBones.RightUpperLeg, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightLowerLeg } },
            new JointConfig() { _rootBone = HumanBodyBones.RightLowerLeg, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightFoot } },
        };

        public static JointConfig optionalSpineWithOutChestJoint = new JointConfig() { _rootBone = HumanBodyBones.Spine, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightShoulder, HumanBodyBones.LeftShoulder, HumanBodyBones.Neck } };
        public static JointConfig optionalSpineWithOutCheckShoulderJoint = new JointConfig() { _rootBone = HumanBodyBones.Spine, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightUpperArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.Neck } };
        public static JointConfig optionalChestWithOutShoulderJoint = new JointConfig() { _rootBone = HumanBodyBones.Chest, _bones = new List<HumanBodyBones>() { HumanBodyBones.RightUpperArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.Neck } };
        public static JointConfig optionalCheckWithOutNeck = new JointConfig() { _rootBone = HumanBodyBones.Chest, _bones = new List<HumanBodyBones>() { HumanBodyBones.Head } };
        public static JointConfig optionaSpineWithOutNeck = new JointConfig() { _rootBone = HumanBodyBones.Spine, _bones = new List<HumanBodyBones>() { HumanBodyBones.Head } };

        public static List<JointConfig> _cacheList = new List<JointConfig>();
        public static void CreateJoints(Transform root, HumanBodyBoneReferenceData r, bool isReal)
        {
            // Hip
            // Spine
            // Chest
            // Shoulder
            // Arm
            // Leg

            // Clear Colliders
            LineSphereCollider[] preColliders = root.gameObject.GetComponentsInChildren<LineSphereCollider>(true);
            for (int i = 0; i < preColliders.Length; i++)
            {
                GameObject.DestroyImmediate(preColliders[i]);
            }
            RootColliderManager[] mgs = root.gameObject.GetComponentsInChildren<RootColliderManager>(true);
            for (int i = 0; i < mgs.Length; i++)
            {
                GameObject.DestroyImmediate(mgs[i]);
            }

            Transform trsLeftUpperArm = r._dicBones[(int)HumanBodyBones.LeftUpperArm];
            Transform trsRightUpperArm = r._dicBones[(int)HumanBodyBones.RightUpperArm];
            Transform trsLeftLowerArm = r._dicBones[(int)HumanBodyBones.LeftLowerArm];
            Transform trsLeftUpperLeg = r._dicBones[(int)HumanBodyBones.LeftUpperLeg];
            Transform trsLeftLowerLeg = r._dicBones[(int)HumanBodyBones.LeftLowerLeg];

            float upperArmDis = Vector3.Distance(trsLeftUpperArm.position, trsLeftLowerArm.position);
            float legDis = Vector3.Distance(trsLeftUpperLeg.position, trsLeftLowerLeg.position);
            Vector3 v2 = trsLeftUpperArm.position - trsRightUpperArm.position;
            float torsoWidth = v2.magnitude;

            // check Optional JointConfig
            bool hasChest = r.CheckBone(HumanBodyBones.Chest);
            bool hasShoulder = r.CheckBone(HumanBodyBones.LeftShoulder);
            bool hasNeck = r.CheckBone(HumanBodyBones.Neck);
            _cacheList.Clear();
            if (!hasChest)
            {
                if (hasShoulder)
                    _cacheList.Add(optionalSpineWithOutChestJoint);
                else
                    _cacheList.Add(optionalSpineWithOutCheckShoulderJoint);
                if (!hasNeck)
                    _cacheList.Add(optionaSpineWithOutNeck);
            }
            else
            {
                if (!hasShoulder)
                {
                    _cacheList.Add(optionalChestWithOutShoulderJoint);
                }
                if (!hasNeck)
                    _cacheList.Add(optionalCheckWithOutNeck);
            }

            _cacheList.AddRange(_listJointConfigs007);
            for (int i = 0; i < _cacheList.Count; i++)
            {
                JointConfig jc = _cacheList[i];
                HumanBodyBones rootBone = jc._rootBone;
                if (!r._dicBones.ContainsKey((int)rootBone) || (r._dicBones[(int)rootBone] == null))
                    continue;
                List<HumanBodyBones> jointBones = jc._bones;
                Transform rootTrs = r._dicBones[(int)rootBone];

                bool hasCollider = false;
                for (int boneIndex = 0; boneIndex < jointBones.Count; boneIndex++)
                {
                    HumanBodyBones bone = jointBones[boneIndex];
                    if (!r._dicBones.ContainsKey((int)jointBones[boneIndex])
                        || (r._dicBones[(int)jointBones[boneIndex]]) == null)
                        continue;
                    Transform boneTrs = r._dicBones[(int)jointBones[boneIndex]];
                    LineSphereCollider collider = rootTrs.gameObject.AddComponent<LineSphereCollider>();
                    collider._startBone = rootBone;
                    collider._endBone = jointBones[boneIndex];
                    collider.WorldA = rootTrs.position;
                    collider.WorldB = boneTrs.position;

                    float a = GetRadius(upperArmDis, legDis, torsoWidth, rootBone);
                    float b = GetRadius(upperArmDis, legDis, torsoWidth, bone);

                    collider.RadiusA = a;
                    collider.RadiusB = b;
                    hasCollider = true;
                }

                if (hasCollider)
                {
                    RootColliderManager mg = rootTrs.gameObject.AddComponent<RootColliderManager>();
                    mg.InitRootColliderManagerWhenCreate();
                }
            }
            // Debug.Log("## Create Joints Point collider over ##");
        }

        private static float GetRadius(float upperArmDis, float legDis, float torsoWidth, HumanBodyBones bone)
        {
            if (bone == HumanBodyBones.Chest || bone == HumanBodyBones.Spine)
                return torsoWidth * 0.75f * 0.5f;
            else if (bone == HumanBodyBones.Hips)
                return torsoWidth * 0.8f * 0.5f;
            else if (bone == HumanBodyBones.LeftShoulder || bone == HumanBodyBones.RightShoulder)
                return upperArmDis * 0.25f * 0.5f;
            else if (bone == HumanBodyBones.LeftUpperArm || bone == HumanBodyBones.RightUpperArm)
                return upperArmDis * 0.3f * 0.5f;
            else if (bone == HumanBodyBones.LeftLowerArm || bone == HumanBodyBones.RightLowerArm)
                return upperArmDis * 0.2f * 0.5f;
            else if (bone == HumanBodyBones.LeftHand || bone == HumanBodyBones.RightHand)
                return upperArmDis * 0.16f * 0.5f;
            else if (bone == HumanBodyBones.Head)
                return upperArmDis * 0.75f * 0.5f;
            else if (bone == HumanBodyBones.Neck)
                return upperArmDis * 0.55f * 0.5f;
            return 0.05f;
        }
    }
}
