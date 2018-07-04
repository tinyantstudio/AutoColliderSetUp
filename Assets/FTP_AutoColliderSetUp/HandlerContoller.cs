using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class HandlerContoller : MonoBehaviour
    {
        public HumanBodyBones _targetBones = HumanBodyBones.LastBone;
        private void OnDrawGizmos()
        {
            Color color = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.03f);
            Gizmos.color = color;
        }
    }
}
