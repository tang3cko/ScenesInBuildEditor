# Scenes In Build Editor

## Purpose

Unity editor extension for managing the "Scenes In Build" list in Build Settings. Provides a simple interface with checkboxes for adding and removing scenes, and drag-and-drop for reordering.

---

## Features

- Display all scenes in the project
- Add or remove scenes from Build Settings with checkboxes
- Reorder build scenes with drag-and-drop
- Search filter for finding scenes
- Right-click context menu (Open / Open Additive)
- Double-click to reveal scene in Project window
- Clear Missing button to remove invalid scene references
- Current scene indicator

---

## Installation

### Unity Package Manager (Git URL)

1. Open `Window > Package Manager`
2. Click the `+` button and select `Add package from git URL...`
3. Enter the following URL:

```
https://github.com/tang3cko/ScenesInBuildEditor.git?path=Packages/com.tang3cko.scenes-in-build-editor
```

### Edit manifest.json directly

Add the following to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tang3cko.scenes-in-build-editor": "https://github.com/tang3cko/ScenesInBuildEditor.git?path=Packages/com.tang3cko.scenes-in-build-editor"
  }
}
```

---

## Usage

Open the window from `Window > Scenes In Build Editor`.

| Action | Description |
|--------|-------------|
| Drag handle | Reorder build scenes |
| Checkbox | Add or remove from Build Settings |
| Double-click | Reveal scene in Project window |
| Right-click | Open scene menu (Open / Open Additive) |

| Button | Description |
|--------|-------------|
| Refresh | Reload scene list |
| Clear Missing | Remove invalid scene references from build |

Changes apply immediately to Build Settings.

---

## Requirements

- Unity 2021.3 or later

---

## License

MIT
