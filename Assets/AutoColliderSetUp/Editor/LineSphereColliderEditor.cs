using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    [CustomEditor(typeof(LineSphereCollider))]
    public class LineSphereColliderEditor : Editor
    {
        private void OnSceneGUI()
        {
            //LineSphereCollider collider = target as LineSphereCollider;
            //if (!collider._inEditing)
            //    return;

            //Vector3 p1 = collider.WorldA;
            //Vector3 p2 = collider.WorldB;

            //EditorGUI.BeginChangeCheck();
            //Handles.color = Color.red;
            //Vector3 newP1 = Handles.PositionHandle(p1, Quaternion.identity);
            //Vector3 newP2 = Handles.PositionHandle(p2, Quaternion.identity);
            //Handles.SphereHandleCap(-1, p1, Quaternion.identity, 0.03f, EventType.Repaint);
            //Handles.SphereHandleCap(-1, p2, Quaternion.identity, 0.03f, EventType.Repaint);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    collider.WorldA = newP1;
            //    collider.WorldB = newP2;
            //}
        }
    }
}
