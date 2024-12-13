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
    /// The entity collector class
    /// </summary>
    public class EntityCollector
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
        /// The groups
        /// </summary>
        private readonly Dictionary<ExclusiveGroupStruct, FasterList<EGID>> _groups = new();

        /// <summary>
        /// The group entity components db
        /// </summary>
        private FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
            _groupEntityComponentsDB;

        /// <summary>
        /// Binds the engines root
        /// </summary>
        /// <param name="enginesRoot">The engines root</param>
        public void Bind(EnginesRoot enginesRoot)
        {
            _groupEntityComponentsDB =
                (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>)
                GroupEntityComponentsDBField.GetValue(enginesRoot);
        }

        /// <summary>
        /// Ticks this instance
        /// </summary>
        public IReadOnlyDictionary<ExclusiveGroupStruct, FasterList<EGID>> CollectGroups()
        {
            _groups.Clear();
            foreach (var entityComponents in _groupEntityComponentsDB)
            {
                var entityIds = new FasterList<EGID>();
                _groups.Add(entityComponents.key, entityIds);
                foreach (var componentEntry in entityComponents.value)
                {
                    componentEntry.value.KeysEvaluator(entityId =>
                    {
                        entityIds.Add(new EGID(entityId, entityComponents.key));
                    });
                }
            }

            return _groups;
        }

        /// <summary>
        /// Gets the entity info using the specified egid
        /// </summary>
        /// <param name="egid">The egid</param>
        /// <returns>The entity info</returns>
        public EntityInfo GetEntityInfo(EGID egid)
        {
            var entityInfo = new EntityInfo(egid);
            if (!_groupEntityComponentsDB.TryGetValue(egid.groupID, out var entityComponents)) return entityInfo;
            foreach (var componentEntry in entityComponents)
            {
                var dictionaryType = componentEntry.value.GetType();
                if (!MethodCache.TryGetValue(dictionaryType, out var method))
                {
                    method = dictionaryType.GetMethod("TryGetValue");
                    if (method == null) continue;
                    MethodCache[dictionaryType] = method;
                }

                TryGetValueParams[0] = egid.entityID;
                TryGetValueParams[1] = default;
                if (!Equals(method.Invoke(componentEntry.value, TryGetValueParams), true)) continue;
                var component = TryGetValueParams[1];
                entityInfo.Add(new ComponentInfo(componentEntry.key, component.GetType().Name, component));
            }

            return entityInfo;
        }
    }
}