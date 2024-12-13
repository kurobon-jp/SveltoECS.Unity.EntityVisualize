namespace SveltoECS.Unity.EntityVisualize.Models
{
    /// <summary>
    /// The component info class
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Gets the value of the component id
        /// </summary>
        public uint ComponentId { get; }

        /// <summary>
        /// Gets the value of the component name
        /// </summary>
        public string ComponentName { get; }

        /// <summary>
        /// Gets or sets the value of the component
        /// </summary>
        public object Component { get; }

        /// <summary>
        /// Gets or sets the value of the foldout
        /// </summary>
        public bool Foldout { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentInfo"/> class
        /// </summary>
        /// <param name="componentId">The component id</param>
        /// <param name="componentName">The component name</param>
        public ComponentInfo(uint componentId, string componentName, object component)
        {
            ComponentId = componentId;
            ComponentName = componentName;
            Component = component;
        }

        /// <summary>
        /// Returns the string
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return $"{ComponentName}({ComponentId})";
        }
    }
}