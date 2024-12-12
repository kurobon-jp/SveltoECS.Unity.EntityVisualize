using System.Collections.Generic;

namespace SveltoECS.Unity.EntityVisualize.Models
{
    /// <summary>
    /// The entity info class
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// Gets the value of the entity id
        /// </summary>
        public uint EntityId { get; }

        /// <summary>
        /// Gets the value of the components
        /// </summary>
        public List<ComponentInfo> Components { get; } = new();

        /// <summary>
        /// Gets or sets the value of the last tick
        /// </summary>
        public int LastTick { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInfo"/> class
        /// </summary>
        /// <param name="entityId">The entity id</param>
        public EntityInfo(uint entityId)
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Describes whether this instance try get
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>The bool</returns>
        public bool TryGet(uint key, out ComponentInfo value)
        {
            value = null;
            foreach (var componentInfo in Components)
            {
                if (componentInfo.ComponentId == key)
                {
                    value = componentInfo;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the value
        /// </summary>
        /// <param name="value">The value</param>
        public void Add(ComponentInfo value)
        {
            Components.Add(value);
            Components.Sort((a, b) => (int)(a.ComponentId - b.ComponentId));
        }

        /// <summary>
        /// Returns the string
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return $"Entity_{EntityId.ToString()}";
        }
    }
}