using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FastToolsPackage.AutoWrapBodyCollider
{
    public class EditorTools
    {
        public static void DrawLabelWithColorInBox(string label, Color color)
        {
            Color preColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
            GUI.color = preColor;
        }

        public static void DrawSpace(int count)
        {
            while (count > 0)
            {
                EditorGUILayout.Space();
                count--;
            }
        }

        public static void ShowMessage(string message)
        {
            EditorUtility.DisplayDialog("Message", message, "OK");
        }
    }
}
