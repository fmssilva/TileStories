# LaTeX Work Plan — Grande Panorama AR / TileStories_App
## Dissertation Plan (mscplan) — FCT NOVA

> **AI Agent Instructions**: Read this file fully before starting. Execute each task in order.  
> After each task, verify that the file compiles (or at minimum has no obvious LaTeX syntax errors).  
> The **main file to compile** is: `template.tex` (root of the `report/` folder).  
> Compile command: `pdflatex -interaction=nonstopmode template.tex` (run twice for cross-references).

---

## Project Context

- **Project**: Grande Panorama AR — AR companion app for the Grande Panorama de Lisboa (~1700, 23m panel, Museu Nacional do Azulejo)
- **Author**: Francisco Silva | Student Number: 70698 | fmso.silva@campus.fct.unl.pt
- **Supervisor**: Prof. Teresa Romão (tir@fct.unl.pt) — Full Professor, FCT NOVA
- **Co-Supervisor**: Prof. Fernando Birra (fpb@fct.unl.pt) — Associate Professor, FCT NOVA
- **Degree**: MSc in Computer Science and Engineering (Engenharia Informática)
- **Department**: Informática (Computer Science), FCT NOVA
- **Document type**: `mscplan` (Dissertation Plan, not the final thesis)
- **Date**: March 2026
- **Title (EN)**: *Augmented Reality Companion for the Grande Panorama de Lisboa: An Interactive Museum Experience*
- **Title (PT)**: *Realidade Aumentada como Companheiro do Grande Panorama de Lisboa: Uma Experiência Interativa em Museu*
- **Report folder**: `C:\Users\franc\Desktop\TileStories_App\report\`

---

## File Structure (report/ folder)

```
report/
  template.tex                  ← COMPILE THIS. Do not edit.
  0-Config/
    0_memoir.tex                ← Do not edit
    1_novathesis.tex            ← TASK 1: change doctype, language settings
    2_biblatex.tex              ← Do not edit (keep default)
    3_cover.tex                 ← TASK 2: author, title, supervisors, degree
    4_files.tex                 ← TASK 3: register the 4 chapter files
    5_packages.tex              ← Do not edit unless adding packages
    6_list_of.tex               ← Do not edit
    7-aidisclose.tex            ← TASK 9: AI disclosure configuration
    9_nova_fct.tex              ← TASK 2b: uncomment correct department
  1-FrontMatter/
    abstract-en.tex             ← TASK 4: write English abstract bullet-points
    abstract-pt.tex             ← TASK 5: write Portuguese abstract bullet-points
    acknowledgements.tex        ← Leave as template placeholder
    acronyms.tex                ← TASK 8: add project acronyms
    dedication.tex              ← Leave as template placeholder
    glossary.tex                ← Leave as template placeholder
    quote.tex                   ← Leave as template placeholder
    symbols.tex                 ← Leave as template placeholder
    aidisclosure.tex            ← Leave (manual fallback, not used)
  2-MainMatter/
    chapter-introduction.tex    ← TASK A: Chapter 1 (REPLACE existing file)
    chapter-relatedwork.tex     ← TASK B: Chapter 2 (NEW file)
    chapter-proposed-solution.tex ← TASK C: Chapter 3 (NEW file)
    chapter-workplan.tex        ← TASK D: Chapter 4 (NEW file)
    [all other demo files]      ← Keep as-is (not referenced after TASK 3)
  3-BackMatter/
    appendix1.tex               ← Keep template placeholder
    appendix2.tex               ← Keep template placeholder
    annex1.tex                  ← Keep template placeholder
  4-Bibliography/
    bibliography.bib            ← TASK 7: add initial references
  5-Figures/                    ← Add figure files here when needed
```

---

## TASK 1 — `0-Config/1_novathesis.tex`: Set document type and language

**Change**:
```tex
% FROM:
\ntsetup{doctype=msc}
% TO:
\ntsetup{doctype=mscplan}
```

**Also uncomment** the language line:
```tex
% FROM:
% \ntsetup{lang=en}
% TO:
\ntsetup{lang=en}
```

**Also** make sure this line is active (it should already be):
```tex
\ntsetup{school=nova/fct}
```

**Also** configure the AI disclosure to use the aidisclose package:
```tex
\ntsetup{print/aidisclosure=aidisclose}
```
(This is the default but confirm it is not set to `false`.)

---

## TASK 2 — `0-Config/3_cover.tex`: Author, title, supervisors, degree

**Title** (replace existing dummy title lines):
```tex
\nttitle(main,en){Augmented Reality Companion for the\\Grande Panorama de Lisboa}%
\nttitle(main,pt){Companheiro de Realidade Aumentada\\para o Grande Panorama de Lisboa}%

\nttitle(sub,en){An Interactive Museum Experience}%
\nttitle(sub,pt){Uma Experiência Interativa em Museu}%
```

**Author** (replace dummy author):
```tex
\ntauthorname(m){Francisco Manuel Sousa de Oliveira Silva}{Francisco Silva}
```

**Author's previous degree**:
```tex
\ntauthordegree(en){Bachelor in Computer Science and Engineering}
\ntauthordegree(pt){Licenciatura em Engenharia Informática}
```

**Submission date**:
```tex
\ntdate(submission){2026-03}
```

**Advisers** (replace dummy advisers — note gender f/m):
```tex
\ntaddperson{adviser}(a,f){Teresa Isabel Lopes Romão, Full Professor, NOVA School of Science and Technology}
\ntaddperson{adviser}(c,m){Fernando José Pina Birra, Associate Professor, NOVA School of Science and Technology}
```

**Remove or comment out** the dummy committee lines (they are not needed for mscplan working status).

---

## TASK 2b — `0-Config/9_nova_fct.tex`: Uncomment correct department and degree

Find and **uncomment** the Informática department block:
```tex
\ntdepartment(pt){Informática}
\ntdepartment(en){Computer Science}
```

Find and **uncomment** the MSc in Computer Science and Engineering degree:
```tex
% Look for "Engenharia Informática" or "Computer Science and Engineering"
% Uncomment the appropriate \ntdegreename lines
```

---

## TASK 3 — `0-Config/4_files.tex`: Register the 4 chapter files

**Replace** the existing chapter block:
```tex
% FROM (remove these lines):
\ntaddfile{chapter}{chapter-introduction}
\ntaddfile{chapter}{chapter-manual}
\ntaddfile{chapter}{chapter-aidisclose}
\ntaddfile{chapter}{chapter-latex}

% TO (replace with):
\ntaddfile{chapter}{chapter-introduction}
\ntaddfile{chapter}{chapter-relatedwork}
\ntaddfile{chapter}{chapter-proposed-solution}
\ntaddfile{chapter}{chapter-workplan}
```

**Remove or comment** the appendix entries (not needed for mscplan):
```tex
% \ntaddfile{appendix}{appendix1}
% \ntaddfile{appendix}{appendix2}
% \ntaddfile{annex}{annex1}
```

Keep bibliography, abstracts, glossaries as-is.

---

## TASK 4 — `1-FrontMatter/abstract-en.tex`: English Abstract

**Overwrite entirely** with bullet-point content structure (see below).  
The abstract must be ≤ 1 page. Write in bullet-point / short sentence style.

Content to include:
1. **Context**: The Grande Panorama de Lisboa (~1700, Gabriel del Barco, 23m panel, Museu Nacional do Azulejo). Static cultural artefact with high historical value but limited interactivity for visitors.
2. **Problem**: Museum visitors lack contextual tools to identify, explore, and engage with the 150+ historical buildings depicted in the panorama. Traditional museum displays (panels, labels) do not scale to this level of detail.
3. **Proposed solution**: A mobile AR application (iOS + Android) that overlays interactive markers over the physical panel, enabling visitors to identify buildings, explore historical epochs (pre-1755 / earthquake / Pombaline / today), follow themed circuits, and access an AI conversational guide.
4. **Key contributions**: (a) AR tracking of a large-scale (23m) flat painting using mobile devices; (b) temporal layer system across 4 historical epochs; (c) personalized visitor profiles; (d) data-driven evaluation with real museum visitors.
5. **Keywords**: Augmented Reality, Cultural Heritage, Museum Experience, Mobile Applications, Historical Visualization, Lisbon

---

## TASK 5 — `1-FrontMatter/abstract-pt.tex`: Portuguese Abstract

**Overwrite entirely** with Portuguese version of the same content as TASK 4.  
Portuguese keywords: Realidade Aumentada, Património Cultural, Experiência em Museu, Aplicações Móveis, Visualização Histórica, Lisboa

---

## TASK 6 (= TASK A) — `2-MainMatter/chapter-introduction.tex`: Chapter 1

**Overwrite entirely** with Chapter 1. Structure:

```
\chapter{Introduction}

1.1 Context
1.2 Motivation and Problem Definition
1.3 Goals and Research Questions
1.4 Main Expected Contributions
```

### Section 1.1 — Context

Bullet topics to develop:
- The Grande Panorama de Lisboa: painted c. 1700 by Gabriel del Barco; 23m long; depicts pre-earthquake Lisbon; currently housed at the Museu Nacional do Azulejo (MNAz), Lisbon
  - [CITE: Ayres de Carvalho, 1962 — or museum catalogue source]
- The panorama depicts 14 km of the Tagus riverfront; 150+ identifiable buildings; multiple churches, palaces, markets, fortifications
- The 1755 Lisbon earthquake: one of the most destructive seismic events in European history; destroyed ~85% of Lisbon; the panorama is one of the rare detailed records of the pre-earthquake city
  - [CITE: Pereira, 2009 — Terramoto 1755 source]
- Museum visits today: visitors stand in front of the 23m panel without tools to understand what they are looking at; no interactive elements currently available; multilingual signage is limited
- Growing use of mobile technology in museum contexts: smartphones as companion devices; AR as an emerging medium for cultural heritage
  - [CITE: Bekele et al., 2018 — survey of AR in cultural heritage]
- [FIGURE PLACEHOLDER: Photo of the Grande Panorama de Lisboa at MNAz, showing the scale of the panel and a typical visitor perspective. Caption: "The Grande Panorama de Lisboa (c. 1700, Gabriel del Barco) at the Museu Nacional do Azulejo."]

### Section 1.2 — Motivation and Problem Definition

Bullet topics to develop:
- **Problem 1 — Information gap**: visitors cannot identify individual buildings without expert knowledge; the level of historical detail embedded in the painting is inaccessible to the general public
- **Problem 2 — Static experience**: the panorama offers no interactive engagement; visitors spend on average 2–3 minutes and move on [anecdotal observation — quantify with museum stats if available]
- **Problem 3 — No temporal dimension**: the painting depicts one moment in time (c. 1700); visitors have no way to understand what happened after (earthquake, reconstruction, modern city)
- **Problem 4 — Diverse audiences**: museum visitors range from schoolchildren to architecture students to international tourists; a one-size-fits-all experience fails all of them
- Opportunity: AR technology on consumer smartphones (ARCore, ARKit) has matured to a point where large-scale flat-image tracking is feasible without specialized hardware
  - [CITE: Google ARCore documentation — image tracking; Apple ARKit documentation]
- Opportunity: the FCT NOVA — MNAz institutional context creates a research partnership enabling validated user studies with real museum visitors

### Section 1.3 — Goals and Research Questions

Bullet topics to develop:
- **Main goal**: Design, implement, and evaluate a mobile AR application that enhances visitor engagement with the Grande Panorama de Lisboa through interactive, personalized, and historically layered content
- **Research questions**:
  - RQ1: How can mobile AR image tracking be applied reliably to a large-scale (23m) flat painting in a controlled museum environment?
  - RQ2: How does the introduction of AR-enhanced interactive content affect visitor engagement and dwell time compared to the baseline (no app)?
  - RQ3: What personalization strategies (visitor profiles, adaptive content) are most effective for heterogeneous museum audiences?
  - RQ4: How can temporal visualization (4 historical epochs) help visitors understand the historical significance of the panorama?
- **Technical objectives**:
  - Implement AR tracking of the full panorama image using ARCore (Android) and ARKit (iOS)
  - Develop a POI system for 30+ buildings at MVP, scaling to 100+ in later phases
  - Implement a 4-epoch timeline slider with visual overlays
  - Implement visitor profile onboarding and content adaptation
  - Deploy on both iOS App Store and Google Play Store
- **Evaluation objectives**:
  - Conduct user studies with 50+ museum visitors
  - Collect quantitative metrics (dwell time, circuit completion, POI engagement rate)
  - Collect qualitative feedback (interviews, questionnaires)

### Section 1.4 — Main Expected Contributions

Bullet topics to develop:
- **C1 — AR tracking of large-scale flat art**: methodological contribution on how to handle multi-scale image tracking (full panel → individual building) on consumer smartphones; implications for other large-format artworks in museums worldwide
- **C2 — Temporal visualization framework**: a reusable system for overlaying historical epoch data on a static visual artefact; applicable to any cultural heritage context where time is a key dimension
- **C3 — Personalized museum companion**: a visitor profile system that adapts content depth, language, and circuit suggestions based on declared interests and education level
- **C4 — Validated user study data**: empirical data from 50–1000+ real museum visitors (depending on deployment phase), contributing to the HCI/museum-tech research community
- **C5 — Open-source mobile app**: the app (or its core AR framework) may be released as a reference implementation for other Portuguese cultural institutions

---

## TASK 7 (= TASK B) — `2-MainMatter/chapter-relatedwork.tex`: Chapter 2

**Create new file**. Structure:

```
\chapter{Related Work}

2.1 Augmented Reality in Cultural Heritage
2.2 Mobile Applications for Museum Experiences
2.3 Temporal and Historical Visualization
2.4 Visitor Engagement and Personalization
2.5 Technical Infrastructure: AR Frameworks and Mobile Development
2.6 Summary and Positioning
```

### Section 2.1 — Augmented Reality in Cultural Heritage

Topics:
- Definition of AR: superimposition of digital information onto the physical world in real time; Milgram and Kishino's Reality-Virtuality Continuum
  - [CITE: Milgram, P. & Kishino, F. (1994). A Taxonomy of Mixed Reality Visual Displays. IEICE Transactions on Information Systems, E77-D(12), 1321–1329.]
- AR in archaeology and heritage sites: outdoor AR for ruins reconstruction (e.g., Pompeii, Colosseum digital reconstructions)
  - [CITE: Bekele, M.K. et al. (2018). A Survey of Augmented, Virtual, and Mixed Reality for Cultural Heritage. J. Comput. Cult. Herit., 11(2), Article 7. DOI:10.1145/3145534]
- AR in indoor museum environments: marker-based vs markerless tracking; image-target tracking for paintings and exhibits
  - [CITE: Chung, N. et al. (2015). The role of augmented reality for experience-influenced environments. J. Travel Res., 54(2), 186–198.]
- Key challenge: tracking stability for large, flat, low-texture artworks — specific problem for the Grande Panorama
  - [CITE: Google ARCore — Augmented Images documentation. https://developers.google.com/ar/develop/augmented-images]
- Examples of AR apps for specific artworks or paintings (search for relevant examples — National Gallery, Rijksmuseum, etc.)
  - [CITE: to be added — search ACM DL for "augmented reality museum painting"]
- [TABLE PLACEHOLDER: Comparison table of AR museum applications: App name | Artwork/Site | AR method | Platform | Key features | Evaluation approach. Include ~6 representative examples from literature.]

### Section 2.2 — Mobile Applications for Museum Experiences

Topics:
- Evolution of museum apps: from audio guides → multimedia kiosks → smartphone apps
  - [CITE: Falk, J.H. & Dierking, L.D. (2012). The Museum Experience Revisited. Left Coast Press.]
- Categories of museum mobile apps: navigation aids, multimedia guides, gamified exploration, social sharing
  - [CITE: Proctor, N. (2010). The museum is mobile. MW2010: Museums and the Web 2010.]
- Key usability challenges: device handling while looking at exhibit, glare, distraction, battery life
  - [CITE: vom Lehn, D. & Heath, C. (2003). Displacing the object: mobile technologies and interpretive resources. Proceedings of ICHIM 03.]
- Notable case studies:
  - Bloomberg Connects (multiple museums) — content distribution platform
  - National Museum of Natural History "Skin & Bones" AR app (Smithsonian)
  - Rijksmuseum Amsterdam app — offline support, high-quality content
  - [CITE: for each — find publication or technical report]
- Success metrics: dwell time, return visits, content completion rate, satisfaction scores
  - [CITE: vom Lehn, D. & Heath, C. 2003; or Falk & Dierking 2012]

### Section 2.3 — Temporal and Historical Visualization

Topics:
- Challenge of representing time in static displays: the "frozen moment" problem in historical artefacts
- Timeline and epoch visualization patterns in digital heritage:
  - Layered map overlays (historic vs modern maps: e.g., Old Maps Online, Google Timelapse)
  - Slider-based temporal navigation (before/after earthquake comparisons)
  - [CITE: Gregory, I. & Geddes, A. (2014). Toward a digital historical atlas. In Toward Spatial Humanities.]
- The 1755 Lisbon earthquake as a unique temporal event: before (Grande Panorama) → during → Pombaline reconstruction → modern Lisbon
  - [CITE: Pereira, A.S. (2009). The opportunity of a disaster: The economic impact of the 1755 Lisbon earthquake. J. Econ. Hist., 69(2), 466–499.]
- AR as a medium for temporal juxtaposition: AR overlays showing "what was here" vs "what is here now"
  - [CITE: Amin, D. et al. (2012). Exploring AR-based Approaches for Heritage Site Visits. IEEE VR 2012.]
- [FIGURE PLACEHOLDER: Diagram showing the 4 temporal epochs of the project: pre-1755 (Grande Panorama) → Earthquake 1755 → Pombaline Reconstruction → Modern Lisbon. Timeline visualization mock-up.]

### Section 2.4 — Visitor Engagement and Personalization

Topics:
- Visitor typology in museums: Falk's identity-related visit motivations (explorer, facilitator, experience seeker, professional/hobbyist, recharger)
  - [CITE: Falk, J.H. (2009). Identity and the Museum Visitor Experience. Left Coast Press.]
- Personalization in museum apps: adapting content depth, language, circuit length to visitor profile
  - [CITE: Ardissono, L. et al. (2012). Personalization in cultural heritage: the road travelled and the one ahead. User Model. User-Adapt. Interact., 22(1–2), 73–99.]
- Gamification in cultural heritage: badges, quizzes, leaderboards; evidence of increased engagement
  - [CITE: Nacher, V. et al. (2015). Enhancing Museum Guidance with Gamification. Interactive Learning Environments.]
- Child-specific design considerations: simplified language, shorter attention spans, game-like interaction patterns
  - [CITE: Druin, A. (1999). Cooperative inquiry: developing new technologies for children with children. CHI '99.]
- Accessibility: language adaptation, font size, contrast, voice narration as assistive technology
  - [CITE: WCAG 2.1 guidelines — W3C, 2018]

### Section 2.5 — Technical Infrastructure

Topics:
- ARCore (Google): plane detection, image tracking, depth API; supported on Android devices with ARCore support
  - [CITE: Google LLC. (2023). ARCore overview. https://developers.google.com/ar]
- ARKit (Apple): scene understanding, image tracking, reality kit; iOS 11+
  - [CITE: Apple Inc. (2023). ARKit documentation. https://developer.apple.com/documentation/arkit]
- Flutter framework: cross-platform mobile development (iOS + Android from single codebase); Dart language; Riverpod state management
  - [CITE: Google LLC. (2024). Flutter documentation. https://flutter.dev]
- `ar_flutter_plugin_plus`: Flutter plugin bridging ARCore/ARKit
  - [CITE: GitHub — ar_flutter_plugin repository]
- Multi-scale tracking challenge: ARCore augmented image tracking — reference image resolution requirements; tracking distance; feature point density
  - [CITE: Google ARCore — best practices for augmented images]
- Flutter + AR architecture patterns: abstraction layers (mock vs real AR), compile-time flags, testability
  - [CITE: reference to the project's own architecture — self-citation if technical paper is published, otherwise omit]

### Section 2.6 — Summary and Positioning

Topics:
- Summary table: what existing work covers and what gaps remain
- Key gap: no existing mobile AR application specifically designed for a large-scale (23m+) flat painting in a museum environment
- Key gap: no work combining AR marker tracking + temporal epoch visualization + personalized visitor profiles in a single integrated system for cultural heritage
- This work's positioning: fills the intersection of (AR for paintings) + (temporal heritage visualization) + (personalized museum companion) + (real-world validated user study in Portuguese museum context)
- [TABLE PLACEHOLDER: Summary table: Related Work | AR support | Temporal layers | Personalization | User study | Large-scale image | Notes. Fill rows with works cited in this chapter.]

---

## TASK 8 (= TASK C) — `2-MainMatter/chapter-proposed-solution.tex`: Chapter 3

**Create new file**. Structure:

```
\chapter{Proposed Solution and Preliminary Work}

3.1 Domain and Application Context
3.2 Concept Overview
    3.2.1 Core Idea
    3.2.2 Key Features
    3.2.3 User Interaction Flow
3.3 System Architecture
3.4 Preliminary Work
```

### Section 3.1 — Domain and Application Context

Topics:
- Target environment: Museu Nacional do Azulejo (MNAz), Lisbon — permanent gallery hosting the Grande Panorama
- Target users: museum visitors of all ages and backgrounds; primary interaction scenario is on-site in front of the physical panel
- Device target: personal smartphones (iOS 14+, Android with ARCore support); no additional hardware required
- Partnership context: FCT NOVA research project in collaboration with MNAz [note: formal MoU pending — describe as planned partnership]
- Scope of the dissertation plan: the full system as described; MVP (30 POIs, Phase 1) validated by end of thesis; extended features (100+ POIs, earthquake simulation, AI guide) in subsequent phases

### Section 3.2.1 — Core Idea

Topics:
- User points their smartphone camera at the physical 23m panel → ARCore detects the panel as a reference image → digital markers appear overlaid on specific buildings
- Two exploration modes:
  - Live AR mode: camera open, markers floating in 3D space over real physical panel
  - Offline/static mode: same markers on a high-resolution digital copy of the panorama (no camera needed; works without visiting the museum)
- Timeline slider: switch between 4 historical epochs — markers and overlays change to reflect which buildings existed, were destroyed, or were rebuilt
- Tap a marker → information card with building name, historical context, modern location (Google Maps link), historical images
- [FIGURE PLACEHOLDER: Conceptual diagram showing: phone camera → ARCore image tracking → overlay markers on panel. Show the two modes (live AR + static). This is the main system overview figure for the chapter.]

### Section 3.2.2 — Key Features (MVP scope for the dissertation plan)

Present as a feature table (use LaTeX tabular or longtable):

| Feature                    | Description                                               | Phase     |
| -------------------------- | --------------------------------------------------------- | --------- |
| AR image tracking          | Detect 23m panel via ARCore/ARKit; anchor digital content | MVP       |
| 30 POI markers             | Interactive markers for 30 key buildings                  | MVP       |
| POI information cards      | Name, historical text, images, maps link                  | MVP       |
| 4-epoch timeline slider    | Pre-1755, Earthquake, Pombaline, Today                    | MVP       |
| Offline static mode        | Same experience without camera                            | MVP       |
| Visitor profile onboarding | Interest + education level; content adapts                | Phase 2   |
| Audio guide (20 clips)     | Narrated contextual audio per zone                        | Phase 2   |
| 5 themed circuits          | Guided routes through thematic groupings                  | Phase 2   |
| Gamification               | Badges, quizzes, achievements                             | Phase 2   |
| Earthquake simulation      | 3-minute immersive animated event                         | Phase 3   |
| AI conversational guide    | GPT-based Q&A assistant                                   | Phase 3   |
| 100+ POIs                  | Extended building coverage                                | Phase 3   |
| Analytics                  | Heatmaps, dwell time, circuit completion                  | Phase 2/3 |

### Section 3.2.3 — User Interaction Flow

Topics:
- Onboarding: app launch → welcome screen (15-second intro to the panorama) → profile selection (skippable in Phase 1) → mode choice (AR or static)
- AR mode flow: camera permission → point at panel → tracking indicator → buildings light up → tap to explore → timeline slider → circuit selection
- Key UX decisions:
  - AR mode does NOT require internet (ARCore image tracking is on-device)
  - Audio guide can be paused/resumed independently of AR session
  - Offline mode ensures functionality even without museum WiFi
- [FIGURE PLACEHOLDER: User flow diagram (horizontal) showing the main navigation paths from app launch to AR exploration to building detail. Should be a simple flowchart — 8–10 nodes maximum. Do not duplicate information from the architecture figure.]

### Section 3.3 — System Architecture

Topics:
- Flutter cross-platform app (single codebase → iOS + Android)
- Clean domain-driven architecture:
  - `ar_core/` — AR infrastructure (abstract interfaces + ARCore/ARKit implementations)
  - `domains/panorama/` — POI data, markers, AR controller
  - `domains/timeline/` — epoch state and slider widget
  - `domains/onboarding/` — visitor profile
  - `domains/audio_guide/`, `circuits/`, `analytics/` — later phases
- Riverpod state management: providers for AR state, selected POI, active epoch, visitor profile
- Abstraction layer: `kUseRealAR` compile-time flag → same codebase runs with mock AR (development/testing) or real ARCore/ARKit (device)
- Data storage: POI data in JSON (`assets/data/pois.json`); panorama image bundled in app assets (offline-first)
- No cloud dependency for core features; optional cloud features (AI guide, analytics sync) degrade gracefully
- [FIGURE PLACEHOLDER: Architecture diagram showing: Flutter App layers (UI/Widgets → Domain Providers → AR Core Abstractions → Implementations: ARCore | ARKit | Mock). Keep simple, 2-column layered diagram. This is the ONE architecture figure for the thesis — do not add more architecture figures.]

### Section 3.4 — Preliminary Work

Topics:
- What has already been implemented (by dissertation plan submission date, March 2026):
  - Project structure: Flutter app with clean domain architecture fully scaffolded
  - AR abstraction layer: `ARSessionManager`, `ARImageTracker`, `ImageFrameProvider` interfaces + mock implementations fully working
  - Static image mode: `StaticImageFrameProvider` + `InteractiveViewer` panorama browsing working
  - POI data model: `POI` Freezed class with `id`, `name`, `category`, `normalizedPosition`, `epoch`, `description`, `imageUrls` fields
  - Initial POI dataset: 10 buildings with coordinates and historical text
  - `PanoramaARView` widget: renders POI markers on the static panorama
  - Timeline domain: `TimePeriod` enum + `TimelineNotifier` + `TimelineSlider` widget — epoch switching works
  - Design system: typography, spacing tokens, color theme (Material 3)
  - Routing: GoRouter setup with home → panorama AR page navigation
  - i18n: Portuguese, English, Spanish language switching infrastructure
  - Test suite: unit tests for POI repository, timeline provider; widget tests for marker rendering
- What is pending at plan submission and is part of the dissertation work:
  - Real ARCore/ARKit integration testing on physical devices
  - Full 30-POI dataset with historical research
  - User study design and ethics approval
  - All Phase 2/3/4 features listed in the feature table above

---

## TASK 9 (= TASK D) — `2-MainMatter/chapter-workplan.tex`: Chapter 4

**Create new file**. Structure:

```
\chapter{Work Plan}

4.1 Methodology
4.2 Phases and Milestones
4.3 Gantt Chart
4.4 Risk Analysis
```

### Section 4.1 — Methodology

Topics:
- Research methodology: iterative Human-Centered Design (HCD) / Design Science Research
  - [CITE: Hevner, A.R. et al. (2004). Design Science in Information Systems Research. MIS Quarterly, 28(1), 75–105.]
- Development methodology: Agile (monthly sprints aligned with phases)
- Evaluation methodology: mixed-methods (quantitative analytics + qualitative user interviews)
- Ethics: user study approval through FCT NOVA ethics board; informed consent; anonymous data collection

### Section 4.2 — Phases and Milestones

Present as a numbered list with deliverables:

**Phase 1 — MVP (Months 1–3, March–May 2026)**
- Deliverable: Working AR app (30 POIs, timeline slider, offline mode) deployed to TestFlight + Play Internal Testing
- Milestone: First in-museum test session with 5–10 volunteer visitors

**Phase 2 — Core Experience (Months 4–6, June–August 2026)**
- Deliverable: Visitor profiles, audio guide (20 clips), 5 themed circuits, gamification (badges + quizzes)
- Milestone: Beta release on App Store + Google Play; 50-user beta test study

**Phase 3 — Advanced Features (Months 7–9, September–November 2026)**
- Deliverable: Earthquake simulation, AI conversational guide (GPT-4o-mini), 100 POIs, analytics system
- Milestone: Full user study (100+ validated participants) with analytics data

**Phase 4 — Polish and Evaluation (Months 10–12, December 2026–February 2027)**
- Deliverable: 150 POIs, accessibility audit (WCAG 2.1 AA), app optimisation (<100MB), full public release
- Milestone: Final user study data (target 1000+ visitors); dissertation writing

### Section 4.3 — Gantt Chart

[GANTT CHART PLACEHOLDER: Create a LaTeX Gantt chart using the `pgfgantt` package.
Rows: Phase 1 / Phase 2 / Phase 3 / Phase 4 — each with sub-tasks.
Months: March 2026 to February 2027 (12 months).
Mark: current status (March 2026 = beginning of Phase 1).
Use `\begin{ganttchart}[...]{1}{12}` syntax.
If pgfgantt is not available, add `\usepackage{pgfgantt}` to 5_packages.tex and show an approximation table instead.]

### Section 4.4 — Risk Analysis

[TABLE PLACEHOLDER: Risk table with columns: Risk | Likelihood | Impact | Mitigation strategy.
Rows to include:
1. AR tracking unstable on physical panel (lighting, distance, glass) | Medium | High | QR code fallback; manual zone selection
2. Museum partnership delayed (MoU not signed) | Low | High | Proceed with public-domain images; formalize later
3. Unity 3D integration too complex for timeline | Medium | Medium | Use Flutter-native 2D overlays as fallback
4. Real-device ARCore testing reveals performance issues | Medium | High | Optimize reference image; fallback to static mode
5. User study recruitment below target | Low | Medium | Coordinate with museum education department for group visits
6. App size exceeds 100MB (assets) | Low | Medium | Tile-based progressive loading; asset compression
]

---

## TASK 10 — `1-FrontMatter/acronyms.tex`: Add project-specific acronyms

Add these acronym entries using `\newacronym{label}{short}{long}` format:

```tex
\newacronym{ar}{AR}{Augmented Reality}
\newacronym{vr}{VR}{Virtual Reality}
\newacronym{mr}{MR}{Mixed Reality}
\newacronym{xr}{XR}{Extended Reality}
\newacronym{poi}{POI}{Point of Interest}
\newacronym{mvp}{MVP}{Minimum Viable Product}
\newacronym{hci}{HCI}{Human-Computer Interaction}
\newacronym{ui}{UI}{User Interface}
\newacronym{ux}{UX}{User Experience}
\newacronym{api}{API}{Application Programming Interface}
\newacronym{mnaz}{MNAz}{Museu Nacional do Azulejo}
\newacronym{gpl}{GPL}{Grande Panorama de Lisboa}
\newacronym{hcd}{HCD}{Human-Centered Design}
\newacronym{sdk}{SDK}{Software Development Kit}
\newacronym{lod}{LOD}{Level of Detail}
\newacronym{gps}{GPS}{Global Positioning System}
\newacronym{tts}{TTS}{Text-to-Speech}
\newacronym{nps}{NPS}{Net Promoter Score}
```

---

## TASK 11 — `4-Bibliography/bibliography.bib`: Initial references

Add BibTeX entries for the following sources (use placeholder DOI/URL if exact details not yet confirmed — mark with TODO comment):

```
1. Milgram & Kishino 1994 — Reality-Virtuality Continuum
2. Bekele et al. 2018 — Survey of AR/VR/MR for Cultural Heritage (ACM JOCCH)
3. Falk & Dierking 2012 — The Museum Experience Revisited
4. Falk 2009 — Identity and the Museum Visitor Experience
5. Ardissono et al. 2012 — Personalization in cultural heritage (UMUAI)
6. Hevner et al. 2004 — Design Science in IS Research (MISQ)
7. Pereira 2009 — 1755 Lisbon earthquake economic impact (JEH)
8. Google ARCore documentation (online, 2023)
9. Apple ARKit documentation (online, 2023)
10. Flutter framework documentation (online, 2024)
11. Chung et al. 2015 — Role of AR for experience-influenced environments
12. Proctor 2010 — The museum is mobile (Museums and the Web)
13. Nacher et al. 2015 — Enhancing museum guidance with gamification
14. Druin 1999 — Cooperative inquiry (CHI '99)
```

---

## TASK 12 — `0-Config/7-aidisclose.tex`: AI Disclosure Configuration

**Replace existing content** with the following configuration:

AI tools used: GitHub Copilot, ChatGPT (GPT-4o), Claude (Anthropic)

Tasks:
- LaTeX formatting and debugging: `s:debug` (Debugging and Repair)
- Text improvement / writing better: `w:poly` (Polishing and Editing)
- Code testing — expand unit/widget/integration tests: `s:auto` (Process automation) + `sup:qa` (Simulated Peer Review)
- Code documentation — add comments: `s:doc` (Code documentation)
- Code UI/UX improvement — iterate from pseudocode to better design: `s:gen` (Code Generation) + `v:edit` (Image Enhancement and Editing)
- AI-generated images/videos for the app: `v:gen` (Synthetic Asset Generation)

---

## COMPILATION INSTRUCTIONS

```bash
# Navigate to report folder
cd C:\Users\franc\Desktop\TileStories_App\report

# Compile (run twice for cross-references and bibliography)
pdflatex -interaction=nonstopmode template.tex
biber template          # or: bibtex template
pdflatex -interaction=nonstopmode template.tex
pdflatex -interaction=nonstopmode template.tex

# Alternatively, use the Makefile:
make
```

**Expected output**: `template.pdf` in the report folder.

**Common errors to check**:
1. `File 'chapter-relatedwork.tex' not found` → make sure file exists in `2-MainMatter/`
2. `Undefined control sequence \ntaddfile` → means template.tex was not compiled (not editing the right file)
3. `I can't find file 'bibliography.bib'` → check that `4-Bibliography/bibliography.bib` exists
4. `Package pgfgantt Error` → add `\usepackage{pgfgantt}` to `0-Config/5_packages.tex`
5. Acronyms not expanding → make sure `\gls{ar}` is used in text and `acronyms.tex` entries are correct

---

## DATA FLOW SUMMARY

```
template.tex (root compile entry)
  └── novathesis.cls
        ├── 0-Config/1_novathesis.tex   → doctype=mscplan, lang=en, school=nova/fct
        ├── 0-Config/3_cover.tex        → title, author, supervisors → printed on cover
        ├── 0-Config/4_files.tex        → declares all content files:
        │     ├── 4-Bibliography/bibliography.bib
        │     ├── 1-FrontMatter/abstract-en.tex
        │     ├── 1-FrontMatter/abstract-pt.tex
        │     ├── 1-FrontMatter/acronyms.tex
        │     ├── 2-MainMatter/chapter-introduction.tex     (Chapter 1)
        │     ├── 2-MainMatter/chapter-relatedwork.tex      (Chapter 2)
        │     ├── 2-MainMatter/chapter-proposed-solution.tex (Chapter 3)
        │     └── 2-MainMatter/chapter-workplan.tex         (Chapter 4)
        ├── 0-Config/7-aidisclose.tex   → AI disclosure tasks → printed in frontmatter
        └── 0-Config/9_nova_fct.tex     → department=Informática, degree=MSc EI
```

---

## NOTES ON INTRODUCTION STRUCTURE

The user asked about two orderings:

**Option A** (original):
```
1.1 Motivation
1.2 Context
1.3 Goals
1.4 Contributions
```

**Option B** (preferred — better academic flow):
```
1.1 Context
1.2 Motivation and Problem Definition
1.3 Goals / Research Questions
1.4 Main Expected Contributions
```

**Recommendation**: Use **Option B**. Academic reports conventionally start with Context (sets the stage, grounds the reader in the domain) before stating the problem (Motivation). Starting with Motivation before Context forces the reader to evaluate importance without knowing what they're evaluating. All 3 reference reports at the FCT NOVA use this Context-first approach.

---

## AI DISCLOSURE — AIDISCLOSE.ORG INSTRUCTIONS

Go to https://aidisclose.org and:
1. Fill **Author name**: Francisco Silva
2. Fill **AI tools used**: GitHub Copilot, ChatGPT (GPT-4o), Claude (Anthropic)
3. **Check the following task boxes** (use these exact labels from the site):

| Category               | Task to check                             | Notes                              |
| ---------------------- | ----------------------------------------- | ---------------------------------- |
| Software Development   | Code documentation and comment generation | Adding comments to code            |
| Software Development   | Debugging and Repair                      | LaTeX formatting and compile debug |
| Software Development   | Code Generation                           | UI/UX improvement from pseudocode  |
| Software Development   | Process automation                        | Expanding test coverage            |
| Visuals and Multimedia | Synthetic Asset Generation                | AI-generated images/videos for app |
| Visuals and Multimedia | Image Enhancement and Editing             | Image post-processing              |
| Writing and Editing    | Polishing and Editing                     | Text improvement / writing better  |
| Quality Assurance      | Simulated Peer Review                     | Code review simulation             |

4. Add **optional comment**: "AI tools were used for code assistance, text polishing, and generating visual assets for the application. All conceptual work, research, and final decisions are the author's own."
5. Select "Generate Only Snippet" → copy output → paste into `0-Config/7-aidisclose.tex`

---

*End of work plan. Execute tasks in order 1 → 14. Each task corresponds to exactly one file edit.*
