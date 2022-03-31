using Replica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Replica.Editor {
    [CustomEditor(typeof(NetworkBehaviour), true)]
    public class NetworkBehaviourViewer : UnityEditor.Editor {
        protected object InspectedNetVar { get; set; }
        protected List<PropertyInfo> NetVars { get; set; }

        object MakeFieldForType(Type type, string label, object val) {
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.label.hover.textColor = Color.yellow;
            EditorStyles.label.fontStyle = FontStyle.Bold;

            var options = GUILayout.MinWidth(300);

            if (type == typeof(int)) {
                val = EditorGUILayout.IntField(label, (int)val, options);
            } else if (type == typeof(uint)) {
                val = (uint)EditorGUILayout.LongField(label, (long)((uint)val), options);
            } else if (type == typeof(short)) {
                val = (short)EditorGUILayout.IntField(label, (int)((short)val), options);
            } else if (type == typeof(ushort)) {
                val = (ushort)EditorGUILayout.IntField(label, (int)((ushort)val), options);
            } else if (type == typeof(sbyte)) {
                val = (sbyte)EditorGUILayout.IntField(label, (int)((sbyte)val), options);
            } else if (type == typeof(byte)) {
                val = (byte)EditorGUILayout.IntField(label, (int)((byte)val), options);
            } else if (type == typeof(long)) {
                val = EditorGUILayout.LongField(label, (long)val, options);
            } else if (type == typeof(ulong)) {
                val = (ulong)EditorGUILayout.LongField(label, (long)((ulong)val), options);
            } else if (type == typeof(bool)) {
                val = EditorGUILayout.Toggle(label, (bool)val, options);
            } else if (type == typeof(string)) {
                val = EditorGUILayout.TextField(label, (string)val, options);
            } else if (type == typeof(float)) {
                val = EditorGUILayout.FloatField(label, (float)val, options);
            } else if (type == typeof(double)) {
                val = EditorGUILayout.DoubleField(label, (double)val, options);
            } else {
                EditorGUILayout.LabelField($"Type not renderable {type.Name}");
            }

            return val;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            InspectedNetVar = serializedObject.targetObject;

            NetVars = target.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetCustomAttribute<NetVar>() != null)
                .Where(property => (property.SetMethod?.IsPublic).GetValueOrDefault())
                .ToList();

            EditorGUILayout.Separator();

            GUIStyle header = new GUIStyle("box") {
                margin = new RectOffset(-5, -5, -5, -5),
                padding = new RectOffset(6, 6, 6, 6),
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            header.normal.textColor = Color.white;
            ColorUtility.TryParseHtmlString("#242424", out Color cx);
            header.normal.background = MakeTex(100, 100, cx);

            GUIStyle container = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(0, 0, 0, 10)
            };

            GUILayout.BeginHorizontal(container);
            GUILayout.BeginVertical();

            GUILayout.Label("NETWORKED PROPERTIES", header);
            EditorGUILayout.Separator();

            foreach (PropertyInfo property in NetVars) {
                GUILayout.BeginHorizontal();
                string label = $"{property.Name.Replace("Network", "")}";
                property.SetValue(InspectedNetVar, MakeFieldForType(property.PropertyType, label, property.GetValue(InspectedNetVar)));
                ColorUtility.TryParseHtmlString("#bcbcbc", out Color c);
                ColorUtility.TryParseHtmlString("#7ba9ee", out Color cc);
                EditorStyles.label.normal.textColor = c;
                EditorStyles.label.hover.textColor = cc;
                EditorStyles.label.fontStyle = FontStyle.Normal;
                GUILayout.Label(property.PropertyType.Name, GUILayout.MinWidth(100));
                GUILayout.EndHorizontal();
                Repaint();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            Color32[] pix = new Color32[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels32(pix);
            result.Apply();
            return result;
        }

    }
}

