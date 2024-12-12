using System;
using SveltoECS.Unity.EntityVisualize.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SveltoECS.Unity.EntityVisualize.Editor
{
    /// <summary>
    /// The entity inspector class
    /// </summary>
    /// <seealso cref="ScriptableObject"/>
    public class EntityInspector : ScriptableObject
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static EntityInspector _instance;

        /// <summary>
        /// Gets the value of the instance
        /// </summary>
        public static EntityInspector Instance
        {
            get
            {
                _instance ??= (EntityInspector)CreateInstance(typeof(EntityInspector));
                return _instance;
            }
        }

        /// <summary>
        /// The entity info
        /// </summary>
        private EntityInfo _entityInfo;

        /// <summary>
        /// Binds the entity info
        /// </summary>
        /// <param name="entityInfo">The entity info</param>
        public void Bind(EntityInfo entityInfo)
        {
            _entityInfo = entityInfo;
            if (entityInfo != null)
            {
                Selection.activeObject = this;
                var inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
                var inspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
                inspectorWindow.Focus();
            }
            else
            {
                Selection.activeObject = null;
            }
        }

        /// <summary>
        /// The entity inspector editor class
        /// </summary>
        /// <seealso cref="UnityEditor.Editor"/>
        [CustomEditor(typeof(EntityInspector))]
        public class EntityInspectorEditor : UnityEditor.Editor
        {
            private EntityInspector _inspector;

            private void OnEnable()
            {
                _inspector = (EntityInspector)target;
            }

            protected override void OnHeaderGUI()
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.fontSize = 24;
                style.padding = new RectOffset(8, 8, 8, 8);
                GUILayout.Label($"Entity_{_inspector._entityInfo.EntityId}", style);
            }

            /// <summary>
            /// Ons the inspector gui
            /// </summary>
            public override void OnInspectorGUI()
            {
                var defaultColor = GUI.backgroundColor;
                for (var i = 0; i < _inspector._entityInfo.Components.Count; i++)
                {
                    var componentInfo = _inspector._entityInfo.Components[i];
                    var component = componentInfo.Component;
                    GUI.backgroundColor = GetRainbowColor(i);
                    componentInfo.Foldout =
                        EditorGUILayout.BeginFoldoutHeaderGroup(componentInfo.Foldout, componentInfo.ToString());
                    GUI.backgroundColor = defaultColor;
                    if (componentInfo.Foldout)
                    {
                        foreach (var field in component.GetType().GetFields())
                        {
                            var value = field.GetValue(component);
                            if (field.FieldType == typeof(string))
                            {
                                EditorGUILayout.LabelField(field.Name, (string)value);
                            }
                            else if (field.FieldType == typeof(bool))
                            {
                                EditorGUILayout.Toggle(field.Name, (bool)value);
                            }
                            else if (field.FieldType == typeof(byte))
                            {
                                EditorGUILayout.IntField(field.Name, (int)value);
                            }
                            else if (field.FieldType == typeof(short))
                            {
                                EditorGUILayout.IntField(field.Name, (int)value);
                            }
                            else if (field.FieldType == typeof(ushort))
                            {
                                EditorGUILayout.IntField(field.Name, Convert.ToInt16((ushort)value));
                            }
                            else if (field.FieldType == typeof(int))
                            {
                                EditorGUILayout.IntField(field.Name, (int)value);
                            }
                            else if (field.FieldType == typeof(uint))
                            {
                                EditorGUILayout.IntField(field.Name, Convert.ToInt32((uint)value));
                            }
                            else if (field.FieldType == typeof(long))
                            {
                                EditorGUILayout.LongField(field.Name, (long)value);
                            }
                            else if (field.FieldType == typeof(ulong))
                            {
                                EditorGUILayout.LongField(field.Name, Convert.ToInt64((ulong)value));
                            }
                            else if (field.FieldType == typeof(float))
                            {
                                EditorGUILayout.FloatField(field.Name, (float)value);
                            }
                            else if (field.FieldType == typeof(double))
                            {
                                EditorGUILayout.DoubleField(field.Name, (double)value);
                            }
                            else if (field.FieldType == typeof(Color))
                            {
                                EditorGUILayout.ColorField(field.Name, (Color)value);
                            }
                            else if (field.FieldType == typeof(Vector2))
                            {
                                EditorGUILayout.Vector2Field(field.Name, (Vector2)value);
                            }
                            else if (field.FieldType == typeof(Vector2Int))
                            {
                                EditorGUILayout.Vector2IntField(field.Name, (Vector2Int)value);
                            }
                            else if (field.FieldType == typeof(Vector3))
                            {
                                EditorGUILayout.Vector3Field(field.Name, (Vector3)value);
                            }
                            else if (field.FieldType == typeof(Vector3Int))
                            {
                                EditorGUILayout.Vector3IntField(field.Name, (Vector3Int)value);
                            }
                            else if (field.FieldType == typeof(Vector4))
                            {
                                EditorGUILayout.Vector4Field(field.Name, (Vector4)value);
                            }
                            else if (field.FieldType == typeof(Quaternion))
                            {
                                var quaternion = (Quaternion)value;
                                EditorGUILayout.Vector4Field(field.Name,
                                    new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w));
                            }
                            else if (field.FieldType.IsEnum)
                            {
                                EditorGUILayout.EnumPopup(field.Name, (Enum)value);
                            }
                            else if (value.GetType().IsSubclassOf(typeof(Object)))
                            {
                                EditorGUILayout.ObjectField(field.Name, (Object)value,
                                    field.FieldType, true);
                            }
                            else
                            {
                                EditorGUILayout.LabelField(field.Name, value.ToString());
                            }
                        }
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                EditorUtility.SetDirty(target);
            }

            /// <summary>
            /// Gets the rainbow color using the specified index
            /// </summary>
            /// <param name="index">The index</param>
            /// <returns>The color</returns>
            private static Color GetRainbowColor(int index)
            {
                return Color.HSVToRGB(index / 16f % 1f, 1, 1);
            }
        }
    }
}