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
   4. **Feed back.** Fold the developer's answers into the spec, as decisions with a date.
4. **Result:**
   - the group's spec section;
   - the live Lab link and its record copy;
   - the questions for the developer.

   **Limits:**
   - A prototype is the reference for organisation, look and behaviour, **not code to port**.
   - Values the developer tries in it are experiments, not defaults (`00-process.md`, "Defaults are judged in the real scene").
   - No framework code is written in this skill.
