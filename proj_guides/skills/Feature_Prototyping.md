# SKILL: Feature Prototyping (Editor Lab)

1. **Name:** "prototype the <feature group> of <domain>". It shows one group of features as it will be configured in the POI
   Editor and as it will look and behave in the app, before any code.
2. **Trigger:** inside `Domain_Planning.md` step 3, once per feature group; in a domain already being built, once per
   component whose look or Editor rows are still open (one that follows an established pattern needs none). Input: the domain's feature map (the groups
   and their options) and a stand-in wall image. If the group has no feature list yet, STOP and write the list first.
3. **Procedure:**
   1. **Spec first.** Write the group's spec section, applying the Framework Completeness rules in `00-process.md`. It
      contains:
      - the group's components;
      - every option with its values, where it lives in the Editor, and how much tracking precision it needs;
      - the fixed rules, with their reasons;
      - which Unity tool authors the content;
      - a mock of the Editor tree and a config sketch;
      - the open questions, each with a recommendation.
   2. **Build one HTML Lab page** with this layout:
      - **Left, the Scene / Game view:** the stand-in wall, the Scene tools the group needs, and ▶ Play (with replay) to show
        Play Mode (reveal, idle, tap).
      - **Right, a Unity-skinned POI Editor** that follows `_5.1_Editor_Tab.md` section 0:
        - the real tabs, with the existing sections shown collapsed and locked ("existing");
        - the new sections marked NEW or EXTENDED;
        - framed foldouts, closed by default except the selected item and the Test sub-foldout;
        - taxonomy tables: a generated hidden key, an editable name, a use count, and a delete that is blocked while the row
          is in use;
        - reference popups that show "(missing)";
        - conditional rows indented;
        - a (?) help on every option;
        - a live `config.json` preview in each tab.
      - **Every option works**, and edits stay live while playing.
      - **The developer performs the real authoring action.** If in Unity they will draw a shape, place a knot or pick a
        prefab, they do that in the Lab too. A Lab that fakes the step (detecting shapes by colour, for example) hides what
        authoring will really be like.
      - **Sample data is complex on purpose:** several items per POI, several POIs, one per level, so that the organisation
        is tested and not only the look.
      - **Every visual has a production path.** The "In Unity" note tags each effect, control and motion with one path:
        - **Native:** a Unity feature as-is (a component, a package).
        - **Build:** code or a shader we write. Name it, and give a rough mobile cost.
        - **Import:** an asset made in another tool and imported (video with alpha, Rive, a PSB rig, GLB, a flipbook). In the
          Lab, this is shown with the same kind of asset, not redrawn in code.
        - **Browser-only:** it cannot be made, or it costs too much on a phone. It is labelled in the Lab and is not judged.

        Claims not verified are marked as such. The limits depend on the target:
        - **UI Toolkit** (the card, screen overlays): no backdrop blur; no USS gradients (a ramp texture or a vertex mesh
          instead); one text-shadow per element; no 3D transforms (a scale stands in); no z-index (draw order = hierarchy
          order).
        - **World content** (URP meshes and shaders on the wall): gradients, glow and 3D are fine. Blur and large
          transparent layers cost fill rate on a phone. There is no camera-wide Bloom over the camera image.
        - **The Editor window** (IMGUI): rows, tables, popups and small previews, laid out as `_5.1` section 0 describes.
        - **Everywhere:** the project renders in linear colour space, so alpha blends look different from the browser.
      - Reference: `skills/feature_prototipe_planning_example.html` / `.png` (the `_4.1` Region Editor Lab).
   3. **Check once and publish.** Take one rendered look (a headless browser screenshot), fix what it shows, then publish it
      as an artifact. Save a record copy and a README entry in `_x.y.N_Prototype/`: the link, what it shows, what to judge.
   4. **Feed back.** Fold each answer and each correction the developer makes in the Lab into the spec the same round, as a rule
      in the section of the part it changes. The spec says the current target, not the round that changed it.
      - For each part, the spec also says what the code has today and where (Built / Partly / Not yet + the class), so the
        implementation starts from the gap.
   5. **Keep a Lab that lives over many rounds in order.** Several prototypes run side by side, so each one is self-contained:
      - **Its own folder:** `_x.y.N_Prototype/`, with its sources in a `<name>_src/` folder there once it has more than one file.
      - **Its own map:** `PROTOTYPE_STRUCTURE.md` beside the sources. It starts with "the code of this prototype, not the Unity
        project" (the project's structure is `10-structure.md`). It lists each module, the features it owns, the Editor
        sections it fills and the tests that prove it. Create it with the prototype; update it in the same round as every change.
      - **One home per feature:** a feature's state and defaults, its drawing, its Editor rows, each row's demo and its test live
        in one module. A change edits that module; a new module is only for a new feature. Never add a "newest layer" that
        wraps the old one.
      - **Extension points, not wrappers:** the shared parts (the Editor, the phone, the tour, the voice) call named
        registries that each module joins. Never `var fOld=f; f=function(){...fOld()...}`, never a second declaration of
        a function.
      - **No silent fallback:** when one source stands in for another (a recorded line missing, so the device voice speaks),
        the Editor shows it at that moment.
      - **Every feature is in the Editor:** each thing the visitor can see or do (a button, a gesture, a notice, a behaviour)
        has its place in the Editor, in the section of the part it belongs to: at least an On / Off row, and options where a
        choice is useful. A feature with no row is not finished. The developer must be able to read the Editor and know every
        feature the app has.
      - **Each Editor row names its demo:** the view, the part it changes, the state, and an action (play a sample, open the
        state). A change, or a press on the row's name, shows that demo at once, so its effect is seen. The self-test fails a
        row without one, and the tests press each row and check that its part is shown and changed.
      - **The build only joins the modules and embeds the assets.** It never rewrites code. It refuses a wrapper, a second
        declaration or a default set twice.
   6. **Audit against the code before the final round.** Walk every Editor row, config value and runtime UI element the code
      has today in the group's area, and give each one a verdict against the Lab: same, add, merge (two versions of one option
      become one list with every good choice), or drop (with a one-line reason). Options neither side has may be added, marked
      PROPOSED for the developer to judge. Split the walk by area when it is large; each part reports a table, and the merged
      decisions go into the Lab and the spec in one pass. The self-test then proves every new option reaches the screen.
4. **Result:**
   - the group's spec section;
   - the live Lab link and its record copy;
   - the questions for the developer.

   **Limits:**
   - A prototype is the reference for organisation, look and behaviour, **not code to port**.
   - Values the developer tries in it are experiments, not defaults (`00-process.md`, "Defaults are judged in the real scene").
   - No framework code is written in this skill.
