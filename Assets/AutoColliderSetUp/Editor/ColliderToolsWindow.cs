using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    //
    // Copy Line-Sphere and clear Colliders of Character
    //
    public class ColliderToolsWindow : EditorWindow
    {
        [MenuItem("FTP_Tools/ColliderTools", false, 2001)]
        public static void DoWindow()
        {
            var window = GetWindow<ColliderToolsWindow>("FTP Collider Tools");
            window.minSize = new Vector2(200, 300);
            window.Show();
        }

        public GameObject m_Src;
        public GameObject m_Des;

        public GameObject m_ClearHumanoidColliderTarget;

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Copy , Clear Humanoid Collider", MessageType.Info);
            EditorTools.DrawLabelWithColorInBox("Copy", Color.green);

            m_Src = EditorGUILayout.ObjectField("Source", m_Src, typeof(GameObject), true) as GameObject;
            m_Des = EditorGUILayout.ObjectField("Destination", m_Des, typeof(GameObject), true) as GameObject;

            // If your humanoid character has same "bone" name
            if (GUILayout.Button("Copy Line-Sphere With Same Hierarchy", GUILayout.MinHeight(25)))
            {
                if (m_Src == null || m_Des == null)
                {
                    EditorTools.ShowMessage("Src or Des is Null...");
                }
                else
                {
                    FTPColliderTools.CopyLineSphereColliderWithSameHierarchy(m_Src, m_Des);
                }
            }
            // If your humanoid character has unsame "bone" name you can use Avatar to copy Collider
            if (GUILayout.Button("Copy Line-Sphere with Avatar Settings", GUILayout.MinHeight(25)))
            {
                if (m_Src == null || m_Des == null)
                {
                    EditorTools.ShowMessage("Src or Des is Null...");
                }
                else
                {
                    FTPColliderTools.CopyLineSphereColliderWithAvatar(m_Src, m_Des);
                }
            }

            EditorTools.DrawSpace(4);
            EditorTools.DrawLabelWithColorInBox("Clear", Color.green);
            m_ClearHumanoidColliderTarget = EditorGUILayout.ObjectField("Clear Target", m_ClearHumanoidColliderTarget, typeof(GameObject), true) as GameObject;
            // clear line-sphere collider
            if (GUILayout.Button("Clear Line-Sphere Collider", GUILayout.MinHeight(25)))
            {
                FTPColliderTools.ClearLineSphereCollider(m_ClearHumanoidColliderTarget);
            }

            // clear normal box or other real collider
            if (GUILayout.Button("Clear Normal Collider", GUILayout.MinHeight(25)))
            {
            }
        }
    }
}
