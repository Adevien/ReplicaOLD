using Replica.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Replica.Editor
{
    [CustomEditor(typeof(NetworkBehaviour), true)]
    public class NetworkBehaviourViewer : UnityEditor.Editor
    { 
        protected object InspectedNetVar { get; set; }
        protected List<PropertyInfo> NetVars { get; set; }

        object MakeFieldForType(Type type, string label, object value)
        {
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.yellow;
            EditorStyles.label.fontStyle = FontStyle.Bold;

            if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, (bool)value);
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, (int)value);
            if (type == typeof(long))
                return EditorGUILayout.LongField(label, (long)value);
            if (type == typeof(float))
                return EditorGUILayout.FloatField(label, (float)value);
            if (type == typeof(double))
                return EditorGUILayout.DoubleField(label, (double)value);
            if (type == typeof(string))
                return EditorGUILayout.TextField(label, (string)value);

            throw new ArgumentException($"Invalid type: {nameof(type)}");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InspectedNetVar = serializedObject.targetObject;

            NetVars = target.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetCustomAttribute<NetVar>() != null)
                .Where(property => (property.SetMethod?.IsPublic).GetValueOrDefault())
                .ToList();


            EditorGUILayout.Separator();

            GUIStyle header = new GUIStyle("box");
            header.margin = new RectOffset(-5,-5,-5,-5);
            header.padding = new RectOffset(6,6,6,6);
            header.richText = true;
            header.alignment = TextAnchor.MiddleCenter;
            header.stretchWidth = true; 
            header.fontStyle = FontStyle.Bold;
            header.fontSize = 14;
            header.normal.textColor = Color.white;
            ColorUtility.TryParseHtmlString("#242424", out Color cx);
            header.normal.background = MakeTex(100, 100, cx);

            GUIStyle container = new GUIStyle(EditorStyles.helpBox);
            container.padding = new RectOffset(0, 0, 0, 10);

            GUILayout.BeginHorizontal(container);
            GUILayout.BeginVertical();

            GUILayout.Label("NETWORKED PROPERTIES", header);
            EditorGUILayout.Separator();

            foreach (PropertyInfo property in NetVars)
            {
                string label = $"{property.Name.Replace("Network", "")}";
                property.SetValue(InspectedNetVar, MakeFieldForType(property.PropertyType, label, property.GetValue(InspectedNetVar)));

                ColorUtility.TryParseHtmlString("#bcbcbc", out Color c);
                ColorUtility.TryParseHtmlString("#7ba9ee", out Color cc);

                EditorStyles.label.normal.textColor = c;
                EditorStyles.label.hover.textColor = cc;
                EditorStyles.label.fontStyle = FontStyle.Normal;

                Repaint();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)     
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

    }
}