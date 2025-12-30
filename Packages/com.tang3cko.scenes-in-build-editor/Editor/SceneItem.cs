using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScenesInBuildEditor
{
    /// <summary>
    /// Visual element representing a single scene item in the list.
    /// </summary>
    internal class SceneItem : VisualElement
    {
        // Fields
        private readonly ScenesInBuildEditorWindow window;
        private bool isDragging;
        private Vector3 dragStartPosition;

        // Properties
        public SceneEntry Scene { get; }

        // Constructor
        public SceneItem(SceneEntry scene, ScenesInBuildEditorWindow window)
        {
            Scene = scene;
            this.window = window;

            SetupLayout();
            CreateDragHandle(scene.IsInBuild);
            CreateIndexLabel(scene);
            CreateToggle(scene);
            CreatePathLabel(scene);
            SetupDragEvents(scene);
            SetupHoverEffects();
        }

        // Private Methods - Setup
        private void SetupLayout()
        {
            style.flexDirection = FlexDirection.Row;
            style.height = 36;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.alignItems = Align.Center;
        }

        private void CreateDragHandle(bool isVisible)
        {
            var container = new VisualElement();
            container.style.width = 16;
            container.style.minWidth = 16;
            container.style.height = 10;
            container.style.minHeight = 10;
            container.style.flexDirection = FlexDirection.Column;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.marginRight = 4;

            if (isVisible)
            {
                for (int i = 0; i < 3; i++)
                {
                    var line = new VisualElement();
                    line.style.width = 12;
                    line.style.height = 2;
                    line.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
                    container.Add(line);
                }
            }

            Add(container);
        }

        private void CreateIndexLabel(SceneEntry scene)
        {
            var indexLabel = new Label(scene.IsInBuild ? $"[{scene.BuildIndex}]" : "[-]");
            indexLabel.style.width = 30;
            indexLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            indexLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            Add(indexLabel);
        }

        private void CreateToggle(SceneEntry scene)
        {
            var toggle = new Toggle();
            toggle.value = scene.IsInBuild;
            toggle.style.marginRight = 4;
            toggle.RegisterValueChangedCallback(evt =>
            {
                window.HandleSceneToggled(scene, evt.newValue);
            });
            Add(toggle);
        }

        private void CreatePathLabel(SceneEntry scene)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.flexDirection = FlexDirection.Column;
            container.style.justifyContent = Justify.Center;
            container.style.overflow = Overflow.Hidden;

            var nameLabel = new Label(scene.Name + ".unity");
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.textOverflow = TextOverflow.Ellipsis;
            container.Add(nameLabel);

            var directory = System.IO.Path.GetDirectoryName(scene.Path);
            var pathLabel = new Label(directory);
            pathLabel.style.fontSize = 10;
            pathLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            pathLabel.style.overflow = Overflow.Hidden;
            pathLabel.style.textOverflow = TextOverflow.Ellipsis;
            container.Add(pathLabel);

            Add(container);
        }

        private void SetupDragEvents(SceneEntry scene)
        {
            RegisterCallback<ClickEvent>(HandleClick);

            if (scene.IsInBuild)
            {
                RegisterCallback<PointerDownEvent>(HandlePointerDown);
                RegisterCallback<PointerMoveEvent>(HandlePointerMove);
                RegisterCallback<PointerUpEvent>(HandlePointerUp);
                RegisterCallback<PointerCaptureOutEvent>(HandlePointerCaptureOut);
            }
        }

        private void SetupHoverEffects()
        {
            RegisterCallback<PointerEnterEvent>(_ =>
            {
                style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            });
            RegisterCallback<PointerLeaveEvent>(_ =>
            {
                style.backgroundColor = Color.clear;
            });
        }

        // Event Handlers
        private void HandlePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            isDragging = false;
            dragStartPosition = evt.localPosition;
            this.CapturePointer(evt.pointerId);
        }

        private void HandlePointerMove(PointerMoveEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId)) return;

            var delta = evt.localPosition - dragStartPosition;
            if (!isDragging && delta.magnitude > 5)
            {
                isDragging = true;
                int index = window.AllScenes
                    .Where(s => s.IsInBuild)
                    .OrderBy(s => s.BuildIndex)
                    .ToList()
                    .IndexOf(Scene);
                window.StartDrag(this, index);
            }

            if (isDragging)
            {
                var containerPos = window.SceneContainer.WorldToLocal(evt.position);
                window.UpdateDrag(containerPos);
            }
        }

        private void HandlePointerUp(PointerUpEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId)) return;

            if (isDragging)
            {
                var containerPos = window.SceneContainer.WorldToLocal(evt.position);
                window.EndDrag(containerPos);
            }

            isDragging = false;
            this.ReleasePointer(evt.pointerId);
        }

        private void HandleClick(ClickEvent evt)
        {
            if (evt.clickCount == 2)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene.Path);
                if (sceneAsset != null)
                {
                    Selection.SetActiveObjectWithContext(sceneAsset, null);
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(sceneAsset);
                }
            }
        }

        private void HandlePointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (isDragging)
            {
                window.EndDrag(Vector2.zero);
            }
            isDragging = false;
        }
    }
}
