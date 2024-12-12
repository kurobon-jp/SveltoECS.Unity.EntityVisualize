using System.Collections.Generic;
using System.Linq;
using SveltoECS.Unity.EntityVisualize.Models;
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
        private EnginesRootTicker _ticker;

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
                if (selectedIndex != _selectedIndex || _ticker == null)
                {
                    _ticker ??= new EnginesRootTicker();
                    _treeViewState.selectedIDs = new List<int>();
                    _treeView.OnEntitySelected(null);
                    if (enginesRoots.TryGetValue(names[_selectedIndex], out var enginesRoot))
                    {
                        _selectedIndex = selectedIndex;
                        _ticker.Bind(enginesRoot);
                    }
                }

                _ticker.Tick();
                _treeView.Bind(_ticker.EnginesRootInfo);
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
            /// The engines root
            /// </summary>
            private EnginesRootInfo _enginesRoot;

            private readonly List<object> _itemCache = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="EntitiesTreeView"/> class
            /// </summary>
            /// <param name="treeViewState">The tree view state</param>
            public EntitiesTreeView(TreeViewState treeViewState) : base(treeViewState)
            {
            }

            /// <summary>
            /// Binds the engines root
            /// </summary>
            /// <param name="enginesRoot">The engines root</param>
            public void Bind(EnginesRootInfo enginesRoot)
            {
                _enginesRoot = enginesRoot;
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
                    if (item is not EntityInfo entity) continue;
                    OnEntitySelected(entity);
                    return;
                }
            }

            /// <summary>
            /// Ons the entity selected using the specified entity
            /// </summary>
            /// <param name="entity">The entity</param>
            public void OnEntitySelected(EntityInfo entity)
            {
                EntityInspector.Instance.Bind(entity);
            }

            /// <summary>
            /// Builds the root
            /// </summary>
            /// <returns>The root</returns>
            protected override TreeViewItem BuildRoot()
            {
                _itemCache.Clear();
                var root = new TreeViewItem
                {
                    id = 0, depth = -1, displayName = "Root",
                    children = new List<TreeViewItem>()
                };
                foreach (var group in _enginesRoot.Groups)
                {
                    var item = CreateEntityGroupItem(group);
                    _itemCache.Add(group);
                    foreach (var entity in group.Entities)
                    {
                        item.AddChild(CreateEntityItem(entity));
                        _itemCache.Add(entity);
                    }

                    root.AddChild(item);
                }

                SetupDepthsFromParentsAndChildren(root);
                return root;
            }

            /// <summary>
            /// Creates the entity group item using the specified group
            /// </summary>
            /// <param name="group">The group</param>
            /// <returns>The tree view item</returns>
            private TreeViewItem CreateEntityGroupItem(EntityGroupInfo group)
            {
                return new TreeViewItem { id = _itemCache.Count, depth = 0, displayName = group.ToString() };
            }

            /// <summary>
            /// Creates the entity item using the specified entity
            /// </summary>
            /// <param name="entity">The entity</param>
            /// <returns>The tree view item</returns>
            private TreeViewItem CreateEntityItem(EntityInfo entity)
            {
                return new TreeViewItem { id = _itemCache.Count, depth = 1, displayName = entity.ToString() };
            }

            protected override bool DoesItemMatchSearch(TreeViewItem treeViewItem, string search)
            {
                if (_itemCache[treeViewItem.id] is EntityInfo entity)
                {
                    var searchLower = search.ToLower();
                    foreach (var component in entity.Components)
                    {
                        if (component.ComponentName.ToLower().Contains(searchLower))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}