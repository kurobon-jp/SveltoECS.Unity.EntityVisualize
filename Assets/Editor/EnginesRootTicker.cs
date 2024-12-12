using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Internal;
using SveltoECS.Unity.EntityVisualize.Models;
using UnityEngine;

namespace SveltoECS.Unity.EntityVisualize.Editor
{
    /// <summary>
    /// The engines root ticker class
    /// </summary>
    public class EnginesRootTicker
    {
        /// <summary>
        /// The _groupEntityComponentsDB fieldInfo
        /// </summary>
        private static readonly FieldInfo GroupEntityComponentsDBField = typeof(EnginesRoot).GetField(
            "_groupEntityComponentsDB",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// The TryGetValue params
        /// </summary>
        private static readonly object[] TryGetValueParams = new object[2];

        /// <summary>
        /// The TryGetValue method cache
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> MethodCache = new();

        /// <summary>
        /// The group entity components db
        /// </summary>
        private FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
            _groupEntityComponentsDB;

        /// <summary>
        /// Gets the value of the engines root info
        /// </summary>
        public EnginesRootInfo EnginesRootInfo { get; } = new();

        public void Bind(EnginesRoot enginesRoot)
        {
            _groupEntityComponentsDB =
                (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>)
                GroupEntityComponentsDBField.GetValue(enginesRoot);
            EnginesRootInfo.Clear();
        }

        /// <summary>
        /// Ticks this instance
        /// </summary>
        public void Tick()
        {
            var frameCount = Time.frameCount;
            foreach (var entityComponents in _groupEntityComponentsDB)
            {
                if (!EnginesRootInfo.TryGet(entityComponents.key, out var groupInfo))
                {
                    groupInfo = new EntityGroupInfo(entityComponents.key);
                    EnginesRootInfo.Add(groupInfo);
                }

                foreach (var componentEntry in entityComponents.value)
                {
                    componentEntry.value.KeysEvaluator(entityId =>
                    {
                        if (!groupInfo.TryGet(entityId, out var entityInfo))
                        {
                            entityInfo = new EntityInfo(entityId);
                            groupInfo.Add(entityInfo);
                        }

                        entityInfo.LastTick = frameCount;
                        var dictionaryType = componentEntry.value.GetType();
                        if (!MethodCache.TryGetValue(dictionaryType, out var method))
                        {
                            method = dictionaryType.GetMethod("TryGetValue");
                            if (method == null) return;
                            MethodCache[dictionaryType] = method;
                        }

                        TryGetValueParams[0] = entityId;
                        TryGetValueParams[1] = default;
                        if (!Equals(method.Invoke(componentEntry.value, TryGetValueParams), true)) return;
                        var component = TryGetValueParams[1];
                        if (!entityInfo.TryGet(componentEntry.key, out var componentInfo))
                        {
                            componentInfo = new ComponentInfo(componentEntry.key, component.GetType().Name);
                            entityInfo.Add(componentInfo);
                        }

                        componentInfo.Component = component;
                    });
                }
            }

            EnginesRootInfo.Cleanup(frameCount);
        }
    }
}