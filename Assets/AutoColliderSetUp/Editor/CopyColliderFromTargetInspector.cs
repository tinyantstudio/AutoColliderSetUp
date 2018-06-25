using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace FastToolsPackage.AutoWrapBodyCollider
{
    [CustomEditor(typeof(CopyColliderFromTarget))]
    public class CopyColliderFromTargetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CopyColliderFromTarget script = target as CopyColliderFromTarget;
            if (GUILayout.Button("Copy Collider Same Names", GUILayout.MinHeight(20)))
            {
                CopyColliderFromTarget.CopyFromComponent(script._src, script._des);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Copy Collider Un Same Names", GUILayout.MinHeight(20)))
            {
                CopyColliderFromTarget.CopyFromTargetWithAvatar(script._src, script._des);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Clear Collider", GUILayout.MinHeight(20)))
            {
                CopyColliderFromTarget.ClearCollider(script._clearTarget);
            }
        }
    }
}
