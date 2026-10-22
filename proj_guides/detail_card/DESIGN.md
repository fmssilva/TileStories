---
name: Azulejo Sunlit Horizon
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#434655'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#747686'
  outline-variant: '#c4c5d7'
  surface-tint: '#2151da'
  primary: '#0037b0'
  on-primary: '#ffffff'
  primary-container: '#1d4ed8'
  on-primary-container: '#cad3ff'
  inverse-primary: '#b7c4ff'
  secondary: '#006398'
  on-secondary: '#ffffff'
  secondary-container: '#5bb8fe'
  on-secondary-container: '#00476e'
  tertiary: '#6b3700'
  on-tertiary: '#ffffff'
  tertiary-container: '#8d4b00'
  on-tertiary-container: '#ffcba3'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dce1ff'
  primary-fixed-dim: '#b7c4ff'
  on-primary-fixed: '#001551'
  on-primary-fixed-variant: '#0039b5'
  secondary-fixed: '#cce5ff'
  secondary-fixed-dim: '#93ccff'
  on-secondary-fixed: '#001d31'
  on-secondary-fixed-variant: '#004b73'
  tertiary-fixed: '#ffdcc3'
  tertiary-fixed-dim: '#ffb77d'
  on-tertiary-fixed: '#2f1500'
  on-tertiary-fixed-variant: '#6e3900'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  display-lg:
    fontFamily: Playfair Display
    fontSize: 56px
    fontWeight: '600'
    lineHeight: 64px
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Playfair Display
    fontSize: 38px
    fontWeight: '600'
    lineHeight: 44px
    letterSpacing: -0.01em
  headline-lg:
    fontFamily: Playfair Display
    fontSize: 36px
    fontWeight: '600'
    lineHeight: 44px
    letterSpacing: -0.015em
  headline-lg-mobile:
    fontFamily: Playfair Display
    fontSize: 28px
    fontWeight: '600'
    lineHeight: 34px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Playfair Display
    fontSize: 28px
    fontWeight: '500'
    lineHeight: 36px
  headline-sm:
    fontFamily: Playfair Display
    fontSize: 22px
    fontWeight: '500'
    lineHeight: 28px
  title-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  title-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 24px
  body-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 15px
    fontWeight: '400'
    lineHeight: 24px
  body-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  label-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.04em
  label-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 11px
    fontWeight: '700'
    lineHeight: 14px
    letterSpacing: 0.06em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-sm: 1rem
  gutter-lg: 2rem
  margin: 1.5rem
  margin-sm: 1rem
  margin-lg: 3rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
  space-2xl: 4rem
---

## Brand & Style

The brand personality channels coastal Mediterranean heritage, architectural balance, and the crisp clarity of oceanic sunlight. It feels tactile, sun-bleached, and culturally curated—bridging historical artisanal craft (Portuguese *calçada* mosaic pavers, hand-painted cobalt azulejos, weathered terracotta rooflines) with forward-looking spatial computing and editorial lifestyle storytelling.

The visual style blends sunlit architectural minimalism with delicate glassmorphism. Interfaces rest on warm limestone surfaces with frosted, light-transmitting ceramic panels. Interfaces feel airborne and luminous rather than flat or purely synthetic, pairing deep azulejo cobalt accents with subtle terracotta warmth over limestone ivory foundations.

## Colors

The palette establishes high chromatic tension between traditional ceramic blues and warm earthen foundations:

- **Primary (`#1D4ED8`):** Deep Azulejo Cobalt. Used for primary calls-to-action, active wayfinding indicators, key interactive focal points, and dominant branding elements.
- **Secondary (`#0284C7`):** Mediterranean Cerulean. Functions as a luminous companion for interactive hover states, selection fills, tags, and spatial focus highlights.
- **Tertiary (`#D97706`):** Sunbaked Terracotta Ochre. Applied sparingly for badges, status highlights, artisanal notifications, and warm architectural accents. Deep companion tone `#B45309` is reserved for accessible text-level links or high-contrast callouts.
- **Neutral Canvas & Foundations:** Built upon Portuguese *calçada* ivory and limestone tones (`#FDFBF7`, `#F8FAFC`, `#F1F5F9`).
- **Typography & Structural Contrast:** Deep navy-charcoal (`#0F172A` and `#1E293B`) replaces pure black to retain editorial richness under high-luminance light backgrounds.

Never use harsh desaturated zinc grays; neutral mid-tones must always carry a microscopic warm limestone or faint cobalt undertone.

## Typography

The typographical pairing expresses architectural craft alongside contemporary digital legibility:

- **Editorial Headings (`Playfair Display`):** Reserved for titles, section headers, milestone moments, and narrative quotes. The classical serifs evoke carved stone inscriptions and literary journals. It must never be set in all-caps or used for dense interface controls.
- **Interface & Content (`Plus Jakarta Sans`):** Drives all functional UI, body copy, interactive controls, spatial tooltips, and data labels. Its rounded, open letterforms balance the sharpness of the serif titles while maintaining crisp readability in complex AR overlays and ambient glass cards.
- **Hierarchy Rules:** Keep high-contrast color values (`#0F172A`) on `display` and `headline` scales. For `body-md` and `body-sm`, use `#334155` to soften text readability over sunlit backgrounds without sacrificing WCAG AA contrast.

## Layout & Spacing

The layout is structured around an airy 12-column responsive fluid grid designed to evoke the open courtyards and loggias of coastal architecture:

- **Desktop (1024px+):** 12 columns, `gutter-lg` (2rem / 32px), with `margin-lg` (3rem / 48px). Cards and interactive spatial modules nest into 3, 4, 6, or 8-column wide formations.
- **Tablet (640px - 1023px):** 8 columns, `gutter` (1.5rem / 24px), with `margin` (1.5rem / 24px).
- **Mobile (< 640px):** 4 columns, `gutter-sm` (1rem / 16px), with `margin-sm` (1rem / 16px).

### Spacing Principles
- Spacing relies on generous internal component padding (`space-lg` to `space-xl`) to establish an open gallery feel.
- Vertical section cadences must use `space-2xl` to mimic Mediterranean courtyards, separating content clusters with breathing space rather than heavy dividing lines.
- Compact micro-elements (chips, badges, metadata indicators) use `space-xs` and `space-sm` for tight, cohesive groupings.

## Elevation & Depth

Visual depth combines frosted glassmorphism with sun-drenched architectural shadows:

- **Surface Tiers:**
  - *Base Surface:* Calcada ivory background (`#FDFBF7`) providing radiant warmth.
  - *Elevation 1 (Standard Card):* Semi-translucent white (`rgba(255, 255, 255, 0.82)`) with `backdrop-filter: blur(12px)`. Outlined with a soft limestone highlight border (`rgba(255, 255, 255, 0.95)` at the top, fading to `rgba(226, 232, 240, 0.6)` at the bottom).
  - *Elevation 2 (Floating Overlays / Modals):* Glazed frosted ceramic panel (`rgba(255, 255, 255, 0.92)`) with `backdrop-filter: blur(20px)`.
- **Shadow Profiles:**
  - Shadows avoid dead gray or synthetic black. Instead, they incorporate warm terracotta-limestone undertones (`rgba(180, 83, 9, 0.05)`) mixed with atmospheric cobalt sky tinting (`rgba(29, 78, 216, 0.04)`).
  - *Resting Card:* `0 4px 20px -2px rgba(15, 23, 42, 0.05), 0 2px 6px -1px rgba(217, 119, 6, 0.04)`.
  - *Active / Hover:* `0 12px 32px -4px rgba(29, 78, 216, 0.10), 0 4px 12px -2px rgba(15, 23, 42, 0.04)`.

## Shapes

The roundedness level is strictly set to `2` (Rounded). This provides a softened, glazed ceramic edge inspired by hand-molded Portuguese tiles and worn sea-glass:

- **Standard Elements (Buttons, Inputs, Badges):** `0.5rem` (8px). Delivers a crisp, tactile, crafted profile without leaning into sterile geometric squares or toy-like bubbles.
- **Containers & Glass Panels (`rounded-lg`):** `1rem` (16px). Softens larger surface edges and grounds floating panels on the map or spatial viewport.
- **Hero Containers & Media Modules (`rounded-xl`):** `1.5rem` (24px). Creates an architectural framing effect reminiscent of coastal arches and ceramic tile vignettes.

## Components

### Buttons
- **Primary:** Azulejo cobalt background (`#1D4ED8`) with crisp white text (`#FFFFFF`). Radius `0.5rem`, padding `0.75rem 1.5rem`. Subtle sunlit top inner highlight (`inset 0 1px 0 rgba(255, 255, 255, 0.25)`). Hover shifts to `#1E40AF` with a soft cobalt glow.
- **Secondary (Glazed Ceramic):** Frosted limestone background (`rgba(255, 255, 255, 0.85)`), borders in subtle azure outline (`rgba(29, 78, 216, 0.2)`), text in `#1D4ED8`. Hover shifts background to `#F0F7FF`.
- **Tertiary / Ghost:** Transparent background, deep navy text (`#0F172A`), active states highlighted with warm terracotta tint (`#D97706`).

### Cards & Narrative Panels
- **Glazed Glass Surface:** Background `rgba(255, 255, 255, 0.85)`, `backdrop-filter: blur(14px)`, border `1px solid rgba(255, 255, 255, 0.8)`. Radius `1rem`. Internal padding `space-lg`.
- **Artisanal Story Vignettes:** Cards displaying AR tile reconstructions feature a delicate top border in Cobalt (`#1D4ED8`) with an Ochre terracotta sub-accent tag (`#D97706`).

### Chips & Filters
- Compact pill-rounded segments with `0.5rem` corner curves.
- Inactive: Frosted limestone ivory surface (`#F8FAFC`) with subtle stone border (`#E2E8F0`), text in `#334155`.
- Active: Filled with pale azure tint (`#EFF6FF`), text `#1D4ED8`, border `1px solid #BFDBFE`.

### Input Fields
- Background `rgba(255, 255, 255, 0.9)`, border `1.5px solid #CBD5E1`, text `#0F172A`. Radius `0.5rem`.
- Focus state: Border turns to Azulejo cobalt (`#1D4ED8`) with an atmospheric glow ring: `0 0 0 3px rgba(29, 78, 216, 0.15)`.

### Checkboxes & Radio Buttons
- Checkbox borders use `#94A3B8` resting on white limestone; checked state fills with `#1D4ED8` displaying an ultra-sharp white vector checkmark.
- Radio buttons use an inner ivory ring enclosed by the cobalt field when selected.

### AR Spatial Tooltips & Inspect Nodes
- Specialized overlay anchors that pin to scanned surfaces. Built as micro-frosted beacons with a glowing Azulejo cobalt core (`#1D4ED8`), expanding terracotta pulse ripple (`rgba(217, 119, 6, 0.4)`), and a high-legibility floating card rendered in `Playfair Display` for artifacts and `Plus Jakarta Sans` for historical measurements.