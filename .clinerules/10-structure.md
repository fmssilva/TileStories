
## 1. Project Structure

### 1.1 Two top-level areas: Framework and Apps

- `Assets/Framework/` — everything that behaves identically no matter which heritage
  wall is loaded. This includes AR session bootstrap, tracking abstractions, the POI data
  model and rendering, UI shells, content-card rendering, analytics, and the guide
  character system.
- `Assets/Apps/<WallName>/` — one self-contained folder per wall (e.g. `Panorama`,
  `Chafariz`, `Mural`, and any local development wall used for fast iteration). Each
  contains only that wall's data: its POI list, category taxonomy, map/localization
  files, and media (images, audio, 3D models, video).
- A system moves from an app-specific folder into Framework only once a **second**
  wall needs the exact same thing, unchanged. If only one wall currently needs it, it
  stays local to that wall's folder, even if it looks reusable. Do not generalize ahead
  of actual need.
- Nothing in `Framework/` may ever reference anything inside a specific `Apps/<WallName>/`
  folder. If Framework code needs wall-specific information, that information is passed
  in through a data contract (a ScriptableObject base class or interface) that the wall
  folder implements — never a direct reference the other way.
- No wall folder references another wall folder. If two walls need the same thing, that
  thing belongs in Framework, not copy-pasted between wall folders.

### 1.2 Editor code is physically separated from Runtime code

- `Assets/Framework/Runtime/` — code that ships in the built app.
- `Assets/Framework/Editor/` — code that only runs inside the Unity Editor (custom
  inspectors, menu-item tools, validation scripts, wall-setup wizards). This code must
  never end up in a device build.
- Do this separation using **Assembly Definition files (.asmdef)**, not just folder
  naming. Folder naming is a convention an editor mistake can silently violate; an
  assembly reference rule is enforced by the compiler and fails loudly if violated.
- The Runtime assembly must never reference the Editor assembly. If a runtime script
  needs to call editor-only functionality, that is a sign the code belongs in the Editor
  assembly instead, wired through a build step or menu tool, not a runtime call.
- Wrap any code that must exist in a runtime file but only makes sense in the editor
  in `#if UNITY_EDITOR` — but prefer physically moving it to the Editor assembly whenever
  possible, since that is caught at compile time rather than relying on a preprocessor
  directive someone might forget.

### 1.3 Domain-centered folders, not type-centered folders

- Group files by what they do (`Tracking/`, `POI/`, `Content/`, `Analytics/`), never by
  generic technical category (`Scripts/`, `Prefabs/`, `Managers/`). A reader should be
  able to look at the folder tree alone and understand what the project does, without
  reading a single file.
- Never create a `Utils.cs`, `Helpers.cs`, or `Common.cs` file that accumulates unrelated
  static methods over time. If a file starts doing more than one clearly nameable job,
  split it into separate files named after each job.
- Test folders live alongside the code they test (`Framework/Tests/EditMode/`,
  `Framework/Tests/PlayMode/`), not in a separate top-level `tests/` tree disconnected
  from the code.

---

## 1.4. Complete Project File Structure

  Read in the global project guide in the file: 
  C:\Users\franc\Desktop\TileStories\_0_work_plan.md
  the section "## A. Complete Project File Structure", with the project structure that we should follow as base when implementing things. 


---
