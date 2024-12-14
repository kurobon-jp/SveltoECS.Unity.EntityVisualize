using System.Collections.Generic;
using System.Threading.Tasks;
using Svelto.ECS;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SveltoECS.Unity.EntityVisualize.Editor
{
    /// <summary>
    /// The entities hierarchy window class
    /// </summary>
    /// <seealso cref="EditorWindow"/>
    public sealed class EntitiesHierarchyWindow : EditorWindow
    {
        /// <summary>
        /// The root items
        /// </summary>
        private readonly List<TreeViewItemData<Item>> _rootItems = new();

        /// <summary>
        /// The collector
        /// </summary>
        private EntityCollector _collector;

        /// <summary>
        /// The tree view
        /// </summary>
        private TreeView _treeView;

        /// <summary>
        /// The toolbar menu
        /// </summary>
        private ToolbarMenu _toolbarMenu;

        /// <summary>
        /// The search field
        /// </summary>
        private ToolbarSearchField _searchField;

        /// <summary>
        /// The search text
        /// </summary>
        private string _searchText;

        /// <summary>
        /// The is refreshing
        /// </summary>
        private bool _isRefreshing;

        /// <summary>
        /// Creates the gui
        /// </summary>
        private void CreateGUI()
        {
            _treeView = new TreeView
            {
                viewDataKey = "tree-view",
                focusable = true,
                makeItem = () =>
                {
                    var label = new Label();
                    var doubleClickable = new Clickable(() => OnDoubleClick());
                    doubleClickable.activators.Clear();
                    doubleClickable.activators.Add(new ManipulatorActivationFilter
                    { button = MouseButton.LeftMouse, clickCount = 2 });
                    label.AddManipulator(doubleClickable);
                    return label;
                }
            };
            _treeView.bindItem = (e, i) => e.Q<Label>().text = _treeView.GetItemDataForIndex<Item>(i).name;
            _treeView.selectionChanged += OnSelectionChanged;

            var toolbar = new Toolbar();
            _toolbarMenu = new ToolbarMenu();
            _toolbarMenu.text = "Engines Root";
            _toolbarMenu.variant = ToolbarMenu.Variant.Popup;
            toolbar.Add(_toolbarMenu);
            _searchField = new ToolbarSearchField();
            _searchField.RegisterValueChangedCallback(x => OnSearchTextChanged(x.newValue));
            toolbar.Add(_searchField);
            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_treeView);
        }

        private void OnDoubleClick()
        {
            var inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var inspectorWindow = GetWindow(inspectorWindowType);
            inspectorWindow.Focus();
        }

        /// <summary>
        /// Ons the search text changed using the specified text
        /// </summary>
        /// <param name="text">The text</param>
        private void OnSearchTextChanged(string text)
        {
            _searchText = text;
        }

        /// <summary>
        /// Ons the selection changed using the specified selections
        /// </summary>
        /// <param name="selections">The selections</param>
        private void OnSelectionChanged(IEnumerable<object> selections)
        {
            foreach (var selection in selections)
            {
                if (selection is not Item item) continue;
                OnEntitySelected(item.egid);
                break;
            }
        }

        /// <summary>
        /// Ons the entity selected using the specified egid
        /// </summary>
        /// <param name="egid">The egid</param>
        private void OnEntitySelected(EGID egid)
        {
            EntityInspector.Instance.Bind(_collector, egid);
        }

        /// <summary>
        /// Updates this instance
        /// </summary>
        private void Update()
        {
            if (EntityVisualizer.EnginesRoots.Count == 0) return;
            if (_collector == null)
            {
                _collector = new EntityCollector();
                _toolbarMenu.menu.ClearItems();
                var status = DropdownMenuAction.Status.Checked;
                foreach (var pair in EntityVisualizer.EnginesRoots)
                {
                    _toolbarMenu.menu.AppendAction(pair.Key, _ => { OnSwitchEnginesRoot(pair.Value); }, status: status);
                    if (status != DropdownMenuAction.Status.Checked) continue;
                    OnSwitchEnginesRoot(pair.Value);
                    status = DropdownMenuAction.Status.Normal;
                }
            }

            if (!EditorApplication.isPlaying || _isRefreshing || _collector == null) return;
            _treeView.SetRootItems(_rootItems);
            _treeView.RefreshItems();
            _isRefreshing = true;
            Task.Run(Refresh);
        }

        /// <summary>
        /// Ons the switch engines root using the specified engines root
        /// </summary>
        /// <param name="enginesRoot">The engines root</param>
        private void OnSwitchEnginesRoot(EnginesRoot enginesRoot)
        {
            _collector.Bind(enginesRoot);
        }

        /// <summary>
        /// Refreshes this instance
        /// </summary>
        private void Refresh()
        {
            _rootItems.Clear();
            var groups = _collector.CollectGroups();
            foreach (var group in groups)
            {
                var items = new List<TreeViewItemData<Item>>(group.Value.Count);
                foreach (var egid in group.Value)
                {
                    var entityName = $"{egid.Value}";
                    if (!string.IsNullOrEmpty(_searchText) && !entityName.Contains(_searchText))
                    {
                        continue;
                    }

                    var item = new TreeViewItemData<Item>(entityName.GetHashCode(),
                        new Item { name = entityName, egid = egid.Value });
                    items.Add(item);
                }

                var groupName = $"{group.Key}";
                var rootItem = new TreeViewItemData<Item>(groupName.GetHashCode(),
                    new Item { name = $"{groupName} entities:{group.Value.Count}" }, items);
                _rootItems.Add(rootItem);
            }

            _isRefreshing = false;
        }

        /// <summary>
        /// Shows the window
        /// </summary>
        [MenuItem("Window/Svelto.ECS/Entities Hierarchy")]
        public static void ShowWindow()
        {
            GetWindow<EntitiesHierarchyWindow>("Entities Hierarchy");
        }

        /// <summary>
        /// The item
        /// </summary>
        private struct Item
        {
            /// <summary>
            /// The name
            /// </summary>
            public string name;

            /// <summary>
            /// The egid
            /// </summary>
            public EGID egid;
        }
    }
}