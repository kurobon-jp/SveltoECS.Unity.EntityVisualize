#if UNITY_EDITOR
using System.Collections.Generic;
using Svelto.ECS;

namespace SveltoECS.Unity.EntityVisualize
{
    /// <summary>
    /// The entity visualizer class
    /// </summary>
    public static class EntityVisualizer
    {
        public static Dictionary<string, EnginesRoot> EnginesRoots { get; } = new();
        
        /// <summary>
        /// Registers the name
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="enginesRoot">The engines root</param>
        public static void Register(string name, EnginesRoot enginesRoot)
        {
            EnginesRoots[name] = enginesRoot;
        }

        /// <summary>
        /// Uns the register using the specified name
        /// </summary>
        /// <param name="name">The name</param>
        public static void UnRegister(string name)
        {
            EnginesRoots.Remove(name);
        }
    }
}
#endif