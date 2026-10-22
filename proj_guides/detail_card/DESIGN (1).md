---
name: Iberian Horizon AR
colors:
  surface: '#11131a'
  surface-dim: '#11131a'
  surface-bright: '#373941'
  surface-container-lowest: '#0c0e15'
  surface-container-low: '#191b23'
  surface-container: '#1d1f27'
  surface-container-high: '#282a31'
  surface-container-highest: '#33343c'
  on-surface: '#e2e2ec'
  on-surface-variant: '#c5c5d3'
  inverse-surface: '#e2e2ec'
  inverse-on-surface: '#2e3038'
  outline: '#8f909d'
  outline-variant: '#444651'
  surface-tint: '#b6c4ff'
  primary: '#b6c4ff'
  on-primary: '#05297a'
  primary-container: '#1e3a8a'
  on-primary-container: '#90a8ff'
  inverse-primary: '#4059aa'
  secondary: '#ffb95f'
  on-secondary: '#472a00'
  secondary-container: '#ee9800'
  on-secondary-container: '#5b3800'
  tertiary: '#ffb77d'
  on-tertiary: '#4d2600'
  tertiary-container: '#653400'
  on-tertiary-container: '#fc922b'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#dce1ff'
  primary-fixed-dim: '#b6c4ff'
  on-primary-fixed: '#00164e'
  on-primary-fixed-variant: '#264191'
  secondary-fixed: '#ffddb8'
  secondary-fixed-dim: '#ffb95f'
  on-secondary-fixed: '#2a1700'
  on-secondary-fixed-variant: '#653e00'
  tertiary-fixed: '#ffdcc3'
  tertiary-fixed-dim: '#ffb77d'
  on-tertiary-fixed: '#2f1500'
  on-tertiary-fixed-variant: '#6e3900'
  background: '#11131a'
  on-background: '#e2e2ec'
  surface-variant: '#33343c'
typography:
  headline-xl:
    fontFamily: Playfair Display
    fontSize: 48px
    fontWeight: '600'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-xl-mobile:
    fontFamily: Playfair Display
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg:
    fontFamily: Playfair Display
    fontSize: 36px
    fontWeight: '500'
    lineHeight: 44px
    letterSpacing: -0.015em
  headline-lg-mobile:
    fontFamily: Playfair Display
    fontSize: 26px
    fontWeight: '500'
    lineHeight: 34px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Playfair Display
    fontSize: 24px
    fontWeight: '500'
    lineHeight: 32px
  headline-sm:
    fontFamily: Playfair Display
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0.01em
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 22px
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
  label-lg:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.02em
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.04em
  label-sm:
    fontFamily: Inter
    fontSize: 10px
    fontWeight: '600'
    lineHeight: 14px
    letterSpacing: 0.08em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1rem
  gutter-tablet: 1.5rem
  gutter-desktop: 2rem
  margin: 1rem
  margin-tablet: 2rem
  margin-desktop: 3rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style

This design system establishes an ultra-premium, atmospheric augmented reality interface tailored for historical exploration and archaeological presence. It merges the gravitas of monumental antiquity with precision AR optics. 

### Visual Tone & Aesthetic
- **Atmosphere**: Nocturnal, transcendent, scholarly, and luminous. Interfaces hover over physical ruins and modern vistas like polished obsidian instrumentation.
- **Style Synthesis**: Glassmorphism paired with precision instrument design. Frosted, high-saturation optical panels capture ambient camera feeds, bound by hairline directional rim lighting reminiscent of gilded navigational astrolabes.
- **Audience**: Cultured explorers, architectural historians, and spatial computing users seeking frictionless immersion without visual pollution.

## Colors

The palette is derived from Lisbon's maritime twilight, glazed ceramic heritage, and fortified stone under afternoon warmth.

- **Primary (`#1E3A8A`) — Cobalt Azulejo Glaze**: Used for dominant focal controls, structural anchors, active AR beacons, and primary interactive states.
- **Secondary (`#F59E0B`) — Golden Hour Amber**: Used for active historical reconstructions, relic highlights, curated focal annotations, and temporal scrubbers.
- **Tertiary (`#D97706`) — Warm Terracotta**: Used for supporting metadata, navigational sub-states, and warnings.
- **Neutral (`#0B0D14`) — Deep Iberian Night**: The fundamental canvas underlay, rendering rich obsidian tones with calibrated light transmittance.

### Surface Color Logic
- **Base Canvas**: `#0B0D14` at full opacity for non-AR contexts; mapped to `rgba(11, 13, 20, 0.72)` for standard spatial glass backdrops.
- **Raised Glass (Tier 1)**: `rgba(18, 22, 34, 0.65)` with specular rim borders.
- **Floating HUD (Tier 2)**: `rgba(30, 58, 138, 0.35)` mixed with neutral dark tones to provide tinted, luminous contrast against unpredictable AR camera feeds.

## Typography

The type system creates an intentional juxtaposition between classical narrative authority and utilitarian digital precision.

- **Headlines (Playfair Display)**: Evokes the classical epigraphy, monument engravings, and editorial historiography of the Iberian Peninsula. Used exclusively for points of interest, chronological epochs, and major overlay headings.
- **Body & Labels (Inter)**: Delivers micro-legibility over live, moving high-dynamic-range camera feeds. Neutral, structurally balanced, and high in optical clarity.
- **Numerics & Metadata**: Micro-labels, spatial coordinates, elevation metrics, and temporal years leverage uppercase Inter with expanded letter-spacing (`0.08em`) to act as technical HUD readouts.

## Layout & Spacing

The layout is architected around safe viewing corridors, prioritizing clear vision at the screen center for spatial alignment while anchoring controls along the lower perimeter and dynamic telemetry across the upper periphery.

### Structural Flow & Grid
- **Screen Safe Zones**: A persistent 48px clear margin is preserved from screen top and bottom edges to account for camera optics, physical grip boundaries, and system navigation bars.
- **Adaptive Breakpoints**:
  - **Mobile (< 768px)**: 4-column layout; full-width bottom docking sheet and floating lower thumb controls.
  - **Tablet (768px - 1024px)**: 8-column layout; split HUD presentation with contextual inspector panels hovering in dual glass wings.
  - **Spatial / Desktop (> 1024px)**: 12-column layout; decoupled spatial cards pinned to world-space vectors with floating modular toolbars.

## Elevation & Depth

Visual hierarchy does not use opaque drop shadows. Depth is achieved via chromatic light filtering, blur differentials, and directional boundary illumination.

### Optical Layer System
1. **Camera Feed (Ground)**: Full viewport base layer displaying the live historical environment or real-time spatial mesh.
2. **Ambient HUD (Level 1)**: Floating indicators, orientation reticles, and ground pins. Unbordered, using subtle radial glows (`box-shadow: 0 0 24px rgba(30, 58, 138, 0.45)`).
3. **Glazed Panels (Level 2)**: Secondary metadata sheets, timeline ribbons, and audio tour controllers.
   - Background: `rgba(18, 22, 34, 0.70)`
   - Blur: `backdrop-filter: blur(28px) saturate(180%)`
   - Rim Light: 1px gradient border running from `rgba(255, 255, 255, 0.22)` at the top edge to `rgba(255, 255, 255, 0.03)` at the bottom edge.
4. **Modal Inspectors (Level 3)**: Relic inspection sheets, expanded architectural comparisons, and active dialogues.
   - Background: `rgba(11, 13, 20, 0.85)`
   - Blur: `backdrop-filter: blur(40px) saturate(200%)`
   - Rim Light: 1px gradient border running from `rgba(245, 158, 11, 0.45)` down to `rgba(30, 58, 138, 0.20)`, with an internal glow of `inset 0 1px 0 rgba(255, 255, 255, 0.25)`.

## Shapes

The shape architecture relies on refined geometry that balances soft architectural arches with technical HUD precision.

- **Base Radius (`0.5rem`)**: Applied to micro-tooltips, state chips, technical readout badges, and input elements.
- **Container Radius (`rounded-lg` / `1rem`)**: Applied to spatial cards, contextual inspector drawers, and floating control clusters.
- **Window Radius (`rounded-xl` / `1.5rem`)**: Applied to primary modal sheets, full-height panels, and media overlays.
- **Pill Exception**: Interactive thumb toggles, audio scrubbers, and primary AR mode triggers utilize full continuous capsule radii (`9999px`) to maintain immediate affordance against the rectangular grid of the camera view.

## Components

### Buttons
- **Primary Action (Relic Anchor / Portal)**: Cobalt glazed fill (`#1E3A8A`) overlaid with an internal top-lit specular highlight. Label set in `label-lg` with high-contrast white text (`#FFFFFF`). On hover or focus, an outer aura emerges (`box-shadow: 0 0 16px rgba(59, 130, 246, 0.5)`).
- **Secondary Ghost**: Translucent glass (`rgba(255, 255, 255, 0.06)`) with a 1px border (`rgba(255, 255, 255, 0.15)`). Reverts to `rgba(255, 255, 255, 0.12)` on active touch.
- **Icon / Reticle Triggers**: 44x44px minimum bounds, fully circular with directional rim light and centered vector iconography in Golden Hour Amber or crisp white.

### Chips & Badges
- **Status & Epoch Identifiers**: Capsule pills composed of `rgba(18, 22, 34, 0.65)` glass, 1px perimeter border, containing micro-labels in uppercase `label-sm`. Active chronological periods (e.g., "MOORISH PERIOD — 11TH C.") display a pulsing Amber beacon dot (`#F59E0B`) to the left of the label.

### Cards & Inspector Panels
- Frosted composite construction with `backdrop-filter: blur(28px) saturate(180%)`. 
- Top-lit hairline boundary (`border-top: 1px solid rgba(255, 255, 255, 0.20)`, `border-bottom: 1px solid rgba(255, 255, 255, 0.04)`).
- Content layout enforces clear separation: a `Playfair Display` title block, accompanied by technical coordinate metadata, followed by `Inter` body text.

### Interactive Sliders (Time-Scrubbers)
- Designed for temporal reconstruction (e.g., moving between the 1755 earthquake state and the present day).
- **Track**: 2px hairline bar tinted in `rgba(255, 255, 255, 0.16)` with elapsed progress in Amber (`#F59E0B`).
- **Thumb**: 20px jewel-like disc with an inner core of `#F59E0B` and an outer halo of frosted glass, emitting a subtle ambient drop shadow (`0 2px 8px rgba(0, 0, 0, 0.6)`).

### Input Fields & Search HUD
- Translucent input bars with inset depth (`background: rgba(11, 13, 20, 0.6)`).
- Placeholder text in muted cool slate (`rgba(255, 255, 255, 0.4)`), shifting to a focused border state illuminated in Cobalt Azulejo Glaze with an outer glow.

### Reticles & Spatial Hotspots
- Ring-in-ring vector structures anchored directly to 3D spatial points. Inner ring pulses at 0.5Hz; outer ring maintains orientational crosshairs that respond to device tilt and distance metrics.