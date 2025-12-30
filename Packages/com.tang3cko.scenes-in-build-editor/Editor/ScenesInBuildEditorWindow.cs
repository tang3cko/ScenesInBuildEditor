using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScenesInBuildEditor
{
    /// <summary>
    /// Editor window for managing scenes in Build Settings.
    /// </summary>
    public class ScenesInBuildEditorWindow : EditorWindow
    {
        // Fields
        private List<SceneEntry> allScenes = new();
        private VisualElement sceneContainer;
        private TextField searchField;
        private Label footerLabel;
        private SceneItem draggedItem;
        private int dragStartIndex;
        private VisualElement dropIndicator;

        // Properties
        internal IReadOnlyList<SceneEntry> AllScenes => allScenes;
        internal VisualElement SceneContainer => sceneContainer;

        // Unity Lifecycle Methods
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            CreateToolbar(root);
            CreateSceneList(root);
            CreateDropIndicator(root);
            CreateFooter(root);

            RefreshAndRebuildList();
        }

        private void OnFocus()
        {
            if (sceneContainer != null)
            {
                RefreshAndRebuildList();
            }
        }

        // Public Methods
        [MenuItem("Window/Scenes In Build Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScenesInBuildEditorWindow>("Scenes In Build Editor");
            window.minSize = new Vector2(400, 300);
        }

        // Internal Methods - Called by SceneItem
        internal void HandleSceneToggled(SceneEntry scene, bool isInBuild)
        {
            if (isInBuild && !scene.IsInBuild)
            {
                scene.IsInBuild = true;
                scene.BuildIndex = allScenes.Count(s => s.IsInBuild) - 1;
            }
            else if (!isInBuild && scene.IsInBuild)
            {
                scene.IsInBuild = false;
                scene.BuildIndex = -1;

                int index = 0;
                foreach (var s in allScenes.Where(s => s.IsInBuild).OrderBy(s => s.BuildIndex))
                {
                    s.BuildIndex = index++;
                }
            }

            ApplyChangesToBuildSettings();
            RefreshAndRebuildList();
        }

        internal void StartDrag(SceneItem item, int index)
        {
            if (!item.Scene.IsInBuild) return;

            draggedItem = item;
            dragStartIndex = index;
            item.AddToClassList("dragging");
        }

        internal void UpdateDrag(Vector2 localPosition)
        {
            if (draggedItem == null) return;

            int dropIndex = CalculateDropIndex(localPosition);
            ShowDropIndicator(dropIndex);
        }

        internal void EndDrag(Vector2 localPosition)
        {
            if (draggedItem == null) return;

            int dropIndex = CalculateDropIndex(localPosition);
            if (dropIndex != dragStartIndex && dropIndex >= 0)
            {
                MoveScene(dragStartIndex, dropIndex);
            }

            draggedItem.RemoveFromClassList("dragging");
            draggedItem = null;
            dropIndicator.style.display = DisplayStyle.None;
        }

        // Private Methods - UI Creation
        private void CreateToolbar(VisualElement root)
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.height = 22;
            toolbar.style.paddingLeft = 4;
            toolbar.style.paddingRight = 4;
            toolbar.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
            toolbar.style.borderBottomWidth = 1;
            toolbar.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f);

            var refreshButton = new Button(RefreshAndRebuildList) { text = "Refresh" };
            refreshButton.style.width = 60;
            toolbar.Add(refreshButton);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            searchField = new TextField();
            searchField.style.width = 200;
            searchField.RegisterValueChangedCallback(_ => RebuildList());
            toolbar.Add(searchField);

            root.Add(toolbar);
        }

        private void CreateSceneList(VisualElement root)
        {
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            sceneContainer = new VisualElement();
            sceneContainer.style.flexDirection = FlexDirection.Column;
            scrollView.Add(sceneContainer);

            root.Add(scrollView);
        }

        private void CreateDropIndicator(VisualElement root)
        {
            dropIndicator = new VisualElement();
            dropIndicator.style.height = 2;
            dropIndicator.style.backgroundColor = new Color(0.2f, 0.6f, 1f);
            dropIndicator.style.position = Position.Absolute;
            dropIndicator.style.left = 0;
            dropIndicator.style.right = 0;
            dropIndicator.style.display = DisplayStyle.None;
            root.Add(dropIndicator);
        }

        private void CreateFooter(VisualElement root)
        {
            footerLabel = new Label();
            footerLabel.style.height = 20;
            footerLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            footerLabel.style.paddingRight = 8;
            footerLabel.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
            footerLabel.style.borderTopWidth = 1;
            footerLabel.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f);
            root.Add(footerLabel);
        }

        // Private Methods - Data Management
        private void RefreshAndRebuildList()
        {
            RefreshSceneList();
            RebuildList();
        }

        private void RefreshSceneList()
        {
            var buildScenes = EditorBuildSettings.scenes;
            var buildPaths = buildScenes.Select(s => s.path).ToList();
            var allSceneGuids = AssetDatabase.FindAssets("t:Scene");

            allScenes.Clear();

            for (int i = 0; i < buildScenes.Length; i++)
            {
                var path = buildScenes[i].path;
                allScenes.Add(new SceneEntry
                {
                    Path = path,
                    Name = System.IO.Path.GetFileNameWithoutExtension(path),
                    IsInBuild = true,
                    BuildIndex = i
                });
            }

            foreach (var guid in allSceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!buildPaths.Contains(path))
                {
                    allScenes.Add(new SceneEntry
                    {
                        Path = path,
                        Name = System.IO.Path.GetFileNameWithoutExtension(path),
                        IsInBuild = false,
                        BuildIndex = -1
                    });
                }
            }
        }

        private void RebuildList()
        {
            sceneContainer.Clear();

            var searchText = searchField?.value?.ToLower() ?? "";

            foreach (var scene in allScenes)
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !scene.Name.ToLower().Contains(searchText) &&
                    !scene.Path.ToLower().Contains(searchText))
                {
                    continue;
                }

                var item = new SceneItem(scene, this);
                sceneContainer.Add(item);
            }

            UpdateFooter();
        }

        private void UpdateFooter()
        {
            var inBuildCount = allScenes.Count(s => s.IsInBuild);
            footerLabel.text = $"Total: {allScenes.Count} | In Build: {inBuildCount}";
        }

        private void ApplyChangesToBuildSettings()
        {
            var newScenes = allScenes
                .Where(s => s.IsInBuild)
                .OrderBy(s => s.BuildIndex)
                .Select(s => new EditorBuildSettingsScene(s.Path, true))
                .ToArray();

            EditorBuildSettings.scenes = newScenes;
        }

        // Private Methods - Scene Operations
        private void MoveScene(int fromIndex, int toIndex)
        {
            var buildScenes = allScenes.Where(s => s.IsInBuild).OrderBy(s => s.BuildIndex).ToList();

            if (fromIndex < 0 || fromIndex >= buildScenes.Count) return;
            if (toIndex < 0) toIndex = 0;
            if (toIndex > buildScenes.Count) toIndex = buildScenes.Count;

            var scene = buildScenes[fromIndex];
            buildScenes.RemoveAt(fromIndex);

            if (toIndex > fromIndex) toIndex--;
            buildScenes.Insert(toIndex, scene);

            for (int i = 0; i < buildScenes.Count; i++)
            {
                buildScenes[i].BuildIndex = i;
            }

            ApplyChangesToBuildSettings();
            RefreshAndRebuildList();
        }

        // Private Methods - Drag and Drop
        private int CalculateDropIndex(Vector2 localPosition)
        {
            int buildCount = allScenes.Count(s => s.IsInBuild);
            if (buildCount == 0) return -1;

            float y = localPosition.y;
            int index = 0;

            foreach (var child in sceneContainer.Children())
            {
                if (child is SceneItem sceneItem && sceneItem.Scene.IsInBuild)
                {
                    var rect = child.worldBound;
                    var localRect = sceneContainer.WorldToLocal(rect.position);
                    float midY = localRect.y + rect.height / 2;

                    if (y < midY)
                    {
                        return index;
                    }
                    index++;
                }
            }

            return buildCount;
        }

        private void ShowDropIndicator(int index)
        {
            if (index < 0)
            {
                dropIndicator.style.display = DisplayStyle.None;
                return;
            }

            int currentIndex = 0;
            foreach (var child in sceneContainer.Children())
            {
                if (child is SceneItem sceneItem && sceneItem.Scene.IsInBuild)
                {
                    if (currentIndex == index)
                    {
                        var rect = child.worldBound;
                        dropIndicator.style.top = sceneContainer.WorldToLocal(rect.position).y;
                        dropIndicator.style.display = DisplayStyle.Flex;
                        return;
                    }
                    currentIndex++;
                }
            }

            var lastBuildItem = sceneContainer.Children()
                .OfType<SceneItem>()
                .LastOrDefault(s => s.Scene.IsInBuild);

            if (lastBuildItem != null)
            {
                var rect = lastBuildItem.worldBound;
                dropIndicator.style.top = sceneContainer.WorldToLocal(rect.position).y + rect.height;
                dropIndicator.style.display = DisplayStyle.Flex;
            }
        }
    }
}
