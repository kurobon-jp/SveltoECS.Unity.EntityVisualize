using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SveltoECS.Unity.EntityVisualize.Editor
{
    /// <summary>
    /// The entities hierarchy window class
    /// </summary>
    /// <seealso cref="EditorWindow"/>
    public class EntitiesHierarchyWindow : EditorWindow
    {
        /// <summary>
        /// The tree view state
        /// </summary>
        [SerializeField] private TreeViewState _treeViewState;

        /// <summary>
        /// The tree view
        /// </summary>
        private EntitiesTreeView _treeView;

        /// <summary>
        /// The search field
        /// </summary>
        private SearchField _searchField;

        /// <summary>
        /// The selected index
        /// </summary>
        private int _selectedIndex;

        /// <summary>
        /// The is selected
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// The ticker
        /// </summary>
        private EntityCollector _collector;

        /// <summary>
        /// Opens
        /// </summary>
        [MenuItem("Window/Svelto.ECS/EntitiesHierarchy")]
        private static void Open()
        {
            GetWindow<EntitiesHierarchyWindow>("EntitiesHierarchy");
        }

        /// <summary>
        /// Ons the enable
        /// </summary>
        private void OnEnable()
        {
            _treeViewState ??= new TreeViewState();
            _treeView = new EntitiesTreeView(_treeViewState);
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }

        /// <summary>
        /// Ons the gui
        /// </summary>
        void OnGUI()
        {
            var enginesRoots = EntityVisualizer.EnginesRoots;
            if (!EditorApplication.isPlaying || enginesRoots.Count == 0) return;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _treeView.searchString = _searchField.OnToolbarGUI(_treeView.searchString, GUILayout.Width(350f));
                GUILayout.FlexibleSpace();
                var names = enginesRoots.Keys.ToArray();
                var indexies = names.Select((x, i) => i).ToArray();
                var selectedIndex = EditorGUILayout.IntPopup(_selectedIndex, names, indexies, EditorStyles.toolbarPopup,
                    GUILayout.Width(150f));
                if (selectedIndex < 0) return;
                if (selectedIndex != _selectedIndex || _collector == null)
                {
                    _collector ??= new EntityCollector();
                    _treeViewState.selectedIDs = new List<int>();
                    _treeView.OnEntitySelected(default);
                    if (enginesRoots.TryGetValue(names[_selectedIndex], out var enginesRoot))
                    {
                        _selectedIndex = selectedIndex;
                        _collector.Bind(enginesRoot);
                    }
                }

                _treeView.Bind(_collector);
                _treeView.Reload();
                _treeView.OnGUI(GetContentArea());
            }
        }

        /// <summary>
        /// Updates this instance
        /// </summary>
        private void Update()
        {
            Repaint();
        }

        /// <summary>
        /// Gets the content area
        /// </summary>
        /// <returns>The rect</returns>
        private Rect GetContentArea()
        {
            var padding = 3f;
            var height = EditorGUIUtility.singleLineHeight;
            return new Rect(padding, padding + height, position.width - padding * 2f,
                position.height - padding * 2f - height);
        }

        /// <summary>
        /// The entities tree view class
        /// </summary>
        /// <seealso cref="TreeView"/>
        class EntitiesTreeView : TreeView
        {
            /// <summary>
            /// The collector
            /// </summary>
            private EntityCollector _collector;

            /// <summary>
            /// The engines root
            /// </summary>
            private IReadOnlyDictionary<ExclusiveGroupStruct, FasterList<EGID>> _groups;

            /// <summary>
            /// The item cache
            /// </summary>
            private readonly List<object> _itemCache = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="EntitiesTreeView"/> class
            /// </summary>
            /// <param name="treeViewState">The tree view state</param>
            public EntitiesTreeView(TreeViewState treeViewState) : base(treeViewState)
            {
            }

            /// <summary>
            /// Binds the collector
            /// </summary>
            /// <param name="collector">The collector</param>
            public void Bind(EntityCollector collector)
            {
                _collector = collector;
            }

            /// <summary>
            /// Selections the changed using the specified selected ids
            /// </summary>
            /// <param name="selectedIds">The selected ids</param>
            protected override void SelectionChanged(IList<int> selectedIds)
            {
                foreach (var selectedId in selectedIds)
                {
                    if (selectedId >= _itemCache.Count) continue;
                    var item = _itemCache[selectedId];
                    if (item is not EGID egid) continue;
                    OnEntitySelected(egid);
                    return;
                }
            }

            /// <summary>
            /// Ons the entity selected using the specified egid
            /// </summary>
            /// <param name="egid">The egid</param>
            public void OnEntitySelected(EGID egid)
            {
                if (egid == default)
                {
                    EntityInspector.Instance.Bind(null);
                    return;
                }

                var entityInfo = _collector.GetEntityInfo(egid);
                EntityInspector.Instance.Bind(entityInfo);
            }

            /// <summary>
            /// Builds the root
            /// </summary>
            /// <returns>The root</returns>
            protected override TreeViewItem BuildRoot()
            {
                _groups = _collector.CollectGroups();
                _itemCache.Clear();
                var root = new TreeViewItem
                {
                    id = 0, depth = -1, displayName = "Root",
                    children = new List<TreeViewItem>()
                };
                foreach (var group in _groups)
                {
                    var groupItem = new TreeViewItem
                        { id = _itemCache.Count, depth = 0, displayName = group.Key.ToString() };
                    _itemCache.Add(group.Key);
                    foreach (var egid in group.Value)
                    {
                        _itemCache.Add(egid);
                        groupItem.AddChild(new TreeViewItem
                            { id = _itemCache.Count, depth = 1, displayName = egid.ToString() });
                    }

                    root.AddChild(groupItem);
                }

                SetupDepthsFromParentsAndChildren(root);
                return root;
            }

            /// <summary>
            /// Describes whether this instance does item match search
            /// </summary>
            /// <param name="treeViewItem">The tree view item</param>
            /// <param name="search">The search</param>
            /// <returns>The bool</returns>
            protected override bool DoesItemMatchSearch(TreeViewItem treeViewItem, string search)
            {
                // if (_itemCache[treeViewItem.id] is EntityInfo entity)
                // {
                //     var searchLower = search.ToLower();
                //     foreach (var component in entity.Components)
                //     {
                //         if (component.ComponentName.ToLower().Contains(searchLower))
                //         {
                //             return true;
                //         }
                //     }
                // }

                return false;
            }
        }
    }
}