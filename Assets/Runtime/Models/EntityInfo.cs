using System.Collections.Generic;
using Svelto.ECS;

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
        public EGID EGID { get; }

        /// <summary>
        /// Gets the value of the components
        /// </summary>
        public List<ComponentInfo> Components { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EGID"/> class
        /// </summary>
        /// <param name="egid">The entity group id</param>
        public EntityInfo(EGID egid)
        {
            EGID = egid;
        }

        /// <summary>
        /// Adds the value
        /// </summary>
        /// <param name="value">The value</param>
        public void Add(ComponentInfo value)
        {
            Components.Add(value);
        }

        /// <summary>
        /// Returns the string
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return $"Entity id:{EGID.entityID}";
        }
    }
}