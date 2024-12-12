using System.Collections.Generic;
using Svelto.ECS;

namespace SveltoECS.Unity.EntityVisualize.Models
{
    /// <summary>
    /// The engines root info class
    /// </summary>
    public class EnginesRootInfo
    {
        /// <summary>
        /// Gets the value of the groups
        /// </summary>
        public List<EntityGroupInfo> Groups { get; } = new();

        /// <summary>
        /// Describes whether this instance try get
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>The bool</returns>
        public bool TryGet(ExclusiveGroupStruct key, out EntityGroupInfo value)
        {
            value = null;
            foreach (var group in Groups)
            {
                if (group.GroupStruct == key)
                {
                    value = group;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the value
        /// </summary>
        /// <param name="value">The value</param>
        public void Add(EntityGroupInfo value)
        {
            Groups.Add(value);
        }

        /// <summary>
        /// Cleanups the frame count
        /// </summary>
        /// <param name="frameCount">The frame count</param>
        public void Cleanup(int frameCount)
        {
            foreach (var group in Groups)
            {
                group.Cleanup(frameCount);
            }
        }

        /// <summary>
        /// Clears this instance
        /// </summary>
        public void Clear()
        {
            Groups.Clear();
        }
    }
}