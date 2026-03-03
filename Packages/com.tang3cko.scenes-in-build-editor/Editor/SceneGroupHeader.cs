using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScenesInBuildEditor
{
    /// <summary>
    /// Collapsible group header for directory-based scene grouping.
    /// </summary>
    internal class SceneGroupHeader : VisualElement
    {
        private const string FoldStatePrefsKeyPrefix = "ScenesInBuildEditor.FoldState.";

        private readonly VisualElement childContainer;
        private readonly Label arrowLabel;
        private bool isFolded;

        public VisualElement ChildContainer => childContainer;

        public SceneGroupHeader(string directoryPath, int sceneCount)
        {
            var normalizedPath = directoryPath.Replace('\\', '/');
            var prefsKey = FoldStatePrefsKeyPrefix + normalizedPath;
            isFolded = EditorPrefs.GetBool(prefsKey, false);

            style.flexDirection = FlexDirection.Column;

            // Header row
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.height = 24;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.paddingLeft = 8;
            headerRow.style.paddingRight = 8;
            headerRow.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

            // Hover effects
            headerRow.RegisterCallback<PointerEnterEvent>(_ =>
            {
                headerRow.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            });
            headerRow.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                headerRow.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            });

            // Arrow
            arrowLabel = new Label(isFolded ? "\u25b6" : "\u25bc");
            arrowLabel.style.width = 16;
            arrowLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            arrowLabel.style.fontSize = 10;
            headerRow.Add(arrowLabel);

            // Directory path
            var pathLabel = new Label(normalizedPath);
            pathLabel.style.flexGrow = 1;
            pathLabel.style.overflow = Overflow.Hidden;
            pathLabel.style.textOverflow = TextOverflow.Ellipsis;
            headerRow.Add(pathLabel);

            // Scene count
            var countLabel = new Label($"({sceneCount})");
            countLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            headerRow.Add(countLabel);

            // Click to toggle
            headerRow.RegisterCallback<ClickEvent>(_ =>
            {
                isFolded = !isFolded;
                EditorPrefs.SetBool(prefsKey, isFolded);
                arrowLabel.text = isFolded ? "\u25b6" : "\u25bc";
                childContainer.style.display = isFolded ? DisplayStyle.None : DisplayStyle.Flex;
            });

            Add(headerRow);

            // Child container
            childContainer = new VisualElement();
            childContainer.style.flexDirection = FlexDirection.Column;
            childContainer.style.display = isFolded ? DisplayStyle.None : DisplayStyle.Flex;
            Add(childContainer);
        }
    }
}
