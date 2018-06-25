using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class HumanBodyBoneReferenceData
{
    public Transform root = null;
    public Dictionary<int, Transform> _dicBones;
    public List<int> _listMappingBoneName;

    public HumanBodyBoneReferenceData()
    {
        _listMappingBoneName = new List<int>()
        {
            (int)HumanBodyBones.Hips,
            (int)HumanBodyBones.Chest,
            (int)HumanBodyBones.Spine,
            (int)HumanBodyBones.Head,

            (int)HumanBodyBones.LeftUpperArm,
            (int)HumanBodyBones.LeftLowerArm,
            (int)HumanBodyBones.LeftHand,

            (int)HumanBodyBones.RightUpperArm,
            (int)HumanBodyBones.RightLowerArm,
            (int)HumanBodyBones.RightHand,

            (int)HumanBodyBones.LeftUpperLeg,
            (int)HumanBodyBones.LeftLowerLeg,
            (int)HumanBodyBones.LeftFoot,

            (int)HumanBodyBones.RightUpperLeg,
            (int)HumanBodyBones.RightLowerLeg,
            (int)HumanBodyBones.RightFoot,

            // Optional Bones
            (int)HumanBodyBones.LeftShoulder,
            (int)HumanBodyBones.RightShoulder,
            (int)HumanBodyBones.Neck,
        };
        _dicBones = new Dictionary<int, Transform>();
        ResetReference();
    }

    public bool CheckBone(HumanBodyBones bone)
    {
        return (_dicBones.ContainsKey((int)bone) && _dicBones[(int)bone] != null);
    }

    public void GetLeftRightBodyPartTransform(HumanBodyBones bone, ref List<Transform> list)
    {
        list.Clear();
        if (_dicBones.ContainsKey((int)bone) && _dicBones[(int)bone] != null)
        {
            if (bone == HumanBodyBones.LeftShoulder)
                list.Add(_dicBones[(int)HumanBodyBones.RightShoulder]);
            else if (bone == HumanBodyBones.LeftUpperArm)
                list.Add(_dicBones[(int)HumanBodyBones.RightUpperArm]);
            else if (bone == HumanBodyBones.LeftLowerArm)
                list.Add(_dicBones[(int)HumanBodyBones.RightLowerArm]);
            else if (bone == HumanBodyBones.LeftHand)
                list.Add(_dicBones[(int)HumanBodyBones.RightHand]);
            else if (bone == HumanBodyBones.LeftUpperLeg)
                list.Add(_dicBones[(int)HumanBodyBones.RightUpperLeg]);
            else if (bone == HumanBodyBones.LeftLowerLeg)
                list.Add(_dicBones[(int)HumanBodyBones.RightLowerLeg]);
            else if (bone == HumanBodyBones.LeftFoot)
                list.Add(_dicBones[(int)HumanBodyBones.RightFoot]);
            list.Add(_dicBones[(int)bone]);
        }
    }

    public bool IsValid()
    {
        if (root == null || _dicBones == null)
            return false;
        foreach (var bone in _dicBones)
        {
            if (bone.Value == null &&
                (bone.Key != (int)HumanBodyBones.Spine) &&
                (bone.Key != (int)HumanBodyBones.Chest) &&
                (bone.Key != (int)HumanBodyBones.Neck) &&
                (bone.Key != (int)HumanBodyBones.UpperChest) &&
                (bone.Key != (int)HumanBodyBones.LeftShoulder) &&
                (bone.Key != (int)HumanBodyBones.RightShoulder)
                )
                return false;
        }
        return true;
    }

    public void ResetReference()
    {
        for (int i = 0; i < _listMappingBoneName.Count; i++)
        {
            int bone = _listMappingBoneName[i];
            _dicBones[bone] = null;
        }
    }

    public void MapHumanAvatarToBoneReferences(Transform root, Animator animator)
    {
        HumanBodyBoneReferenceData reference = this;
        if (animator == null || root == null)
            return;
        reference.root = root;
        for (int i = 0; i < _listMappingBoneName.Count; i++)
        {
            _dicBones[_listMappingBoneName[i]] = animator.GetBoneTransform((HumanBodyBones)_listMappingBoneName[i]);
        }
    }
}
