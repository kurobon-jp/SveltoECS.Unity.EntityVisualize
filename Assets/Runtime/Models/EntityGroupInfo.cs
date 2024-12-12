using System.Collections.Generic;
using Svelto.ECS;

namespace SveltoECS.Unity.EntityVisualize.Models
{
    /// <summary>
    /// The entity group info class
    /// </summary>
    public class EntityGroupInfo
    {
        /// <summary>
        /// The entities
        /// </summary>
        private readonly List<EntityInfo> _entities = new();

        /// <summary>
        /// Gets the value of the group struct
        /// </summary>
        public ExclusiveGroupStruct GroupStruct { get; }

        /// <summary>
        /// Gets the value of the entities
        /// </summary>
        public List<EntityInfo> Entities => _entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityGroupInfo"/> class
        /// </summary>
        /// <param name="groupStruct">The group struct</param>
        public EntityGroupInfo(ExclusiveGroupStruct groupStruct)
        {
            GroupStruct = groupStruct;
        }

        /// <summary>
        /// Describes whether this instance try get
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>The bool</returns>
        public bool TryGet(uint key, out EntityInfo value)
        {
            value = null;
            foreach (var entity in _entities)
            {
                if (entity.EntityId == key)
                {
                    value = entity;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the value
        /// </summary>
        /// <param name="value">The value</param>
        public void Add(EntityInfo value)
        {
            _entities.Add(value);
        }

        /// <summary>
        /// Cleanups the frame count
        /// </summary>
        /// <param name="frameCount">The frame count</param>
        public void Cleanup(int frameCount)
        {
            _entities.RemoveAll(x => x.LastTick < frameCount);
        }

        /// <summary>
        /// Returns the string
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return GroupStruct.ToString();
        }
    }
}