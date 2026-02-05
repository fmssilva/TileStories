# TileStories - COMPREHENSIVE BUILD PLAN

**Project**: AR Experiences for Museu Nacional do Azulejo  
**Target**: Grande Panorama de Lisboa (23m azulejo panel)  
**Tech Stack**: React + TypeScript + Tailwind + Vite  
**Architecture**: Domain-Driven Design  
**Date**: February 5, 2026  

---

## ⚠️ CRITICAL: FUTURE CONTEXT REFRESH INSTRUCTIONS

**If you're working on this project in a new session, READ THESE SECTIONS FIRST:**

1. **COLOR PALETTE** (lines 52-106): Azulejo-inspired colors are MANDATORY
   - Primary: `#3C5E95` (azulejo blue) - NOT teal, NOT generic blue
   - Use `azulejo-blue-500`, `azulejo-cobalt-500`, `azulejo-gold-500` in Tailwind
   - Logo uses PNG files (`public/Logo.png`, `public/Logo_with_name.png`) - NOT SVG except favicon

2. **COMPONENT SPECIFICATIONS** (lines 505-630): Detailed designs for each section
   - HeroSection: H1 "Explore Lisbon's Lost Skyline Through AR" + gradient overlay
   - ProblemStatement: 3-column cards (Traditional → AR Solution → Impact)
   - PanoramaShowcase: 60/40 split (image + historical details)
   - READ these specs BEFORE coding to maintain consistency

3. **MULTILINGUAL REQUIREMENTS** (GUIDE_LANGUAGES.md):
   - All text must support PT/EN via `useInlineTranslation()` hook
   - Example: `t({ pt: 'Explorar', en: 'Explore' })`
   - NEVER hardcode English-only strings

4. **SEO REQUIREMENTS** (lines 751-840):
   - H1 only once per page: "Explore Lisbon's Lost Skyline Through AR"
   - H2 for sections: "Why AR for Museums?", "The Grande Panorama de Lisboa"
   - Alt text for images must be descriptive + include keywords

5. **DESIGN PRINCIPLES** (lines 241-260):
   - Domain-centered architecture (assets with components)
   - 3-tier color system (Global → Domain → Local)
   - Mobile-first responsive (Tailwind breakpoints: sm, md, lg, xl)

6. **PROJECT CONTEXT** (lines 23-50):
   - This is a **12-month FCT NOVA thesis project** (not a commercial product)
   - Target: **Museum visitors**, students, tourists (NOT general tech users)
   - Content source: **Google Arts & Culture article** + **site_notes.md**
   - Historical accuracy is CRITICAL (verify facts against sources)

**WHY THESE INSTRUCTIONS EXIST:**
You may lose context between sessions. These reminders ensure:
- Correct azulejo color palette (not random blues)
- Proper multilingual support (PT/EN, not EN-only)
- SEO-optimized structure (H1/H2 hierarchy)
- Historical accuracy (Grande Panorama facts)
- Consistent design language (cards, gradients, spacing)

---

## TABLE OF CONTENTS
1. [Project Overview](#project-overview)
2. [Color System Update](#color-system-update)
3. [Folder Structure](#folder-structure)
4. [Implementation Phases](#implementation-phases)
5. [Detailed Task Breakdown](#detailed-task-breakdown)
6. [SEO & Content Strategy](#seo--content-strategy)
7. [Component Architecture](#component-architecture)
8. [Development Checklist](#development-checklist)

---

## PROJECT OVERVIEW

### Vision
Transform the 23-meter Grande Panorama de Lisboa azulejo panel (c.1700) into an interactive AR experience that brings 300 years of Lisbon history to life for museum visitors and online users.

### Key Historical Context (from Google Arts & Culture)
- **Grande Panorama**: 23-meter ceramic painting showing Lisbon before 1755 earthquake
- **Historical Significance**: Only complete visual record of pre-earthquake Lisbon
- **Coverage**: 14km of coastline, 150+ identified buildings
- **Time Periods**: 4 historical epochs (1700, 1755 earthquake, reconstruction, present)
- **Artist**: Gabriel del Barco (Spanish baroque master)
- **Current Location**: Museu Nacional do Azulejo

### Target Audiences
1. **Museum Visitors**: Enhanced on-site experience with AR overlays
2. **Students/Researchers**: Deep historical analysis and architectural details
3. **Tourists**: Quick 30-min guided tours with multilingual support
4. **Children**: Gamified exploration with quizzes and badges
5. **Investors/Stakeholders**: Academic rigor + impressive tech demonstration

### Success Metrics
- 1000+ museum visitors validated
- WCAG 2.1 accessibility compliance
- <3s page load time
- 95+ Lighthouse score
- Published to app stores (iOS/Android)

---

## COLOR SYSTEM UPDATE

### Logo Analysis
Based on the provided logo image showing azulejo tile iconography with "TileStories" branding:

**Primary Colors** (extracted from logo):
- **Azulejo Blue**: `#3C5E95` (deep historical blue from Portuguese tiles)
- **Azulejo Cobalt**: `#5081B6` (lighter accent blue)
- **Gold/Amber**: `#D4AF37` (gold accents from tile decorations)
- **Ivory/Cream**: `#FFF8E7` (traditional tile background)
- **Terracotta**: `#C1440E` (earthquake/historical drama sections)

### Current vs. Target Colors

**CURRENT** (from clinic-compare):
```typescript
brand: { 500: '#14b8a6' }  // Teal
gray: { 500: '#64748b' }
```

**TARGET** (TileStories brand):
```typescript
// Azulejo-inspired palette
primary: {
  50: '#EBF1F8',
  100: '#D7E3F1',
  200: '#AFC7E3',
  300: '#87ABD5',
  400: '#5F8FC7',
  500: '#3C5E95',  // Main azulejo blue
  600: '#2F4B77',
  700: '#233859',
  800: '#17253B',
  900: '#0B121E'
},

secondary: {
  500: '#5081B6',  // Lighter cobalt
},

accent: {
  gold: '#D4AF37',
  terracotta: '#C1440E',
  ivory: '#FFF8E7',
},

semantic: {
  success: '#4CAF50',  // Keep as is
  error: '#EF4444',
  warning: '#F59E0B',
  info: '#3C5E95',  // Use primary blue
}
```

### Design Token Updates Required

**Files to modify**:
1. `src/design/colors.ts` - Update `globalColors` and `themeColors`
2. `src/design/themeClasses.ts` - Update Tailwind class mappings
3. `tailwind.config.js` - Add custom color palette
4. `src/branding/` - Update logo component variants

---

## FOLDER STRUCTURE

### Domain-Centered Architecture

```
src/
├── domains/                    # Business domains
│   ├── home/                   # Landing page domain
│   │   ├── HeroSection.tsx
│   │   ├── ProblemStatement.tsx
│   │   ├── SolutionOverview.tsx
│   │   ├── hero_bg.jpg         # Domain-specific asset
│   │   ├── colors.ts           # Domain-specific colors (if needed)
│   │   └── index.ts
│   │
│   ├── phases/                 # App development phases showcase
│   │   ├── PhasesTimeline.tsx
│   │   ├── PhaseCard.tsx
│   │   ├── phase_mvp_mockup.png
│   │   ├── phase_core_mockup.png
│   │   ├── phase_wow_mockup.png
│   │   ├── phase_excellence_mockup.png
│   │   └── index.ts
│   │
│   ├── ar_demo/                # Interactive AR demonstration
│   │   ├── ARDemoSection.tsx
│   │   ├── QRCodeGenerator.tsx
│   │   ├── BeforeAfterSlider.tsx
│   │   ├── EarthquakeAnimation.tsx
│   │   ├── earthquake_still.jpg
│   │   └── index.ts
│   │
│   ├── panorama/               # Grande Panorama information
│   │   ├── PanoramaInfo.tsx
│   │   ├── BuildingCard.tsx
│   │   ├── NeighborhoodCard.tsx
│   │   ├── belem_illustration.jpg
│   │   ├── baixa_illustration.jpg
│   │   └── index.ts
│   │
│   ├── personas/               # User personas showcase
│   │   ├── PersonasSection.tsx
│   │   ├── PersonaCard.tsx
│   │   ├── maria_avatar.svg
│   │   ├── joao_avatar.svg
│   │   ├── emma_avatar.svg
│   │   └── index.ts
│   │
│   ├── timeline/               # Project timeline/roadmap
│   │   ├── TimelineSection.tsx
│   │   ├── MilestoneCard.tsx
│   │   └── index.ts
│   │
│   ├── credibility/            # Academic rigor & metrics
│   │   ├── CredibilitySection.tsx
│   │   ├── MetricCounter.tsx
│   │   ├── PartnerLogos.tsx
│   │   └── index.ts
│   │
│   ├── contact/                # Contact information
│   │   ├── ContactPage.tsx
│   │   ├── ContactForm.tsx
│   │   └── index.ts
│   │
│   └── theme/                  # (existing)
│       ├── ThemeToggle.tsx
│       └── useTheme.ts
│
├── layout_and_navigation/      # Site-wide layout
│   ├── MainLayout.tsx
│   ├── header/
│   │   ├── Header.tsx          # UPDATE: Remove clinic refs
│   │   └── colors.ts
│   ├── footer/
│   │   ├── Footer.tsx          # UPDATE: TileStories footer
│   │   └── FooterLinks.tsx
│   └── breadcrumbs/
│
├── components/                 # Shared UI components
│   ├── ui/
│   │   ├── Button.tsx
│   │   ├── Card.tsx            # NEW: Generic card
│   │   ├── Badge.tsx           # NEW: For phases, stats
│   │   ├── Counter.tsx         # NEW: Animated counter
│   │   └── Modal.tsx           # NEW: For financial details
│   └── LanguageSelector.tsx
│
├── design/                     # Design system
│   ├── colors.ts               # UPDATE: New palette
│   ├── themeClasses.ts         # UPDATE: New classes
│   ├── sizes.ts
│   ├── sizeHelpers.ts
│   └── index.ts
│
├── branding/                   # Brand assets
│   ├── Logo.tsx                # UPDATE: TileStories logo
│   ├── assets/
│   │   ├── logo.svg            # User will update
│   │   ├── logo-dark.svg
│   │   └── logo-icon.svg
│   └── index.ts
│
├── utils/                      # Utilities
│   ├── language/
│   │   ├── context.tsx
│   │   ├── hooks.ts
│   │   └── useInlineTranslation.ts
│   ├── animations.ts           # NEW: Scroll animations
│   ├── seo.ts                  # NEW: SEO helpers
│   └── index.ts
│
└── config/
    ├── app.ts                  # UPDATE: App metadata
    └── index.ts

public/
├── assets/
│   ├── grande_panorama.jpg     # Hero background
│   ├── fct_nova_logo.png       # Partnership logos
│   ├── museu_azulejo_logo.png
│   └── demo_video.mp4          # 30-45s demo
├── favicon.svg                 # UPDATE: Tile icon
└── manifest.json               # UPDATE: PWA manifest
```

### Key Principles

1. **Domain Isolation**: Each feature domain is self-contained with its own:
   - Components
   - Assets (images, mockups)
   - Domain-specific styles (if needed)
   - Index exports

2. **Co-location**: Assets live with the components that use them
   - ❌ DON'T: `public/images/hero_bg.jpg` used by hero
   - ✅ DO: `src/domains/home/hero_bg.jpg` 

3. **Clear Boundaries**: 
   - `domains/` = Business features
   - `components/` = Reusable UI primitives
   - `layout_and_navigation/` = Site structure
   - `design/` = Design tokens and system

---

## IMPLEMENTATION PHASES

### Phase 1: Foundation (Priority: HIGH)
**Goal**: Clean codebase, update branding, establish design system

**Tasks**:
1. ✅ Initialize fresh git repo
2. ✅ Remove clinic references
3. ✅ Read all documentation
4. ⏳ Update color system
5. ⏳ Update branding (Logo component)
6. ⏳ Create BUILD_PLAN.md
7. Clean up unused domains (pokemons)
8. Update manifest.json & meta tags

**Deliverables**:
- Clean codebase with TileStories branding
- Updated design system with azulejo colors
- Comprehensive build plan documentation

---

### Phase 2: Core Sections (Priority: HIGH)
**Goal**: Implement main landing page sections

**Sections to build** (in order):
1. **Hero Section** - Fullscreen with parallax
2. **Problem Statement** - Frustration visualization  
3. **Solution Overview** - 3-column feature cards
4. **App Phases** - Vertical timeline with mockups
5. **Credibility** - Metrics dashboard

**Components needed**:
- Card.tsx (reusable card)
- Badge.tsx (phase labels, feature tags)
- Counter.tsx (animated metrics)

---

### Phase 3: Interactive Features (Priority: MEDIUM)
**Goal**: Add engagement and interactivity

**Features**:
1. **AR Demo Section**
   - QR code generator
   - Before/After slider (1700 vs 1755 vs 2026)
   - Earthquake animation trigger

2. **Personas Showcase**
   - Horizontal scrollable cards
   - Avatar illustrations
   - Screen mockups per persona

3. **Timeline/Roadmap**
   - Gantt-style horizontal timeline
   - Progress indicators
   - Milestone tooltips

---

### Phase 4: Content & SEO (Priority: MEDIUM)
**Goal**: Optimize for discovery and engagement

**Tasks**:
1. Write SEO-optimized content (PT/EN)
2. Add structured data (Schema.org)
3. Optimize images (WebP, lazy loading)
4. Add meta tags and OG tags
5. Create sitemap.xml

**Content sections**:
- About page (project background)
- Technical documentation page
- Privacy policy

---

### Phase 5: Polish & Performance (Priority: LOW)
**Goal**: Final optimizations and testing

**Tasks**:
1. Scroll-triggered animations
2. Parallax effects
3. Performance optimization
4. Accessibility audit (WCAG 2.1)
5. Mobile testing
6. Dark mode refinement
7. Analytics setup

---

## DETAILED TASK BREAKDOWN

### TASK 1: Update Color System

**File**: `src/design/colors.ts`

```typescript
// NEW azulejo-inspired palette
export const globalColors = {
    // Primary azulejo blue (from traditional Portuguese tiles)
    primary: {
        50: '#EBF1F8',
        100: '#D7E3F1',
        200: '#AFC7E3',
        300: '#87ABD5',
        400: '#5F8FC7',
        500: '#3C5E95',  // Main brand color
        600: '#2F4B77',
        700: '#233859',
        800: '#17253B',
        900: '#0B121E'
    },
    
    // Secondary cobalt
    secondary: {
        50: '#EEF4F9',
        100: '#DDE9F3',
        200: '#BBD3E7',
        300: '#99BDDB',
        400: '#77A7CF',
        500: '#5081B6',  // Cobalt accent
        600: '#406792',
        700: '#304D6E',
        800: '#203449',
        900: '#101A25'
    },
    
    // Accent colors
    accent: {
        gold: {
            500: '#D4AF37',
            600: '#B8962E',
            700: '#9C7D25'
        },
        terracotta: {
            500: '#C1440E',
            600: '#9D360B',
            700: '#7A2908'
        },
        ivory: {
            500: '#FFF8E7',
            600: '#F5ECDB',
            700: '#EBE0CF'
        }
    },
    
    // Neutral grays (keep existing)
    gray: {
        50: '#F9FAFB',
        100: '#F3F4F6',
        // ... rest unchanged
    },
    
    // Semantic (update info to use primary)
    semantic: {
        success: '#4CAF50',
        error: '#EF4444',
        warning: '#F59E0B',
        info: '#3C5E95',  // Use primary blue
    }
};

// Update theme colors
export const themeColors = {
    light: {
        primary: globalColors.primary[500],      // #3C5E95
        secondary: globalColors.secondary[500],  // #5081B6
        accent: globalColors.accent.gold[500],   // #D4AF37
        background: '#FFFFFF',
        surface: globalColors.accent.ivory[500], // #FFF8E7
        text: globalColors.primary[900],         // #0B121E
        textMuted: globalColors.gray[600],
        border: globalColors.primary[200],
        borderSoft: globalColors.primary[100],
    },
    dark: {
        primary: globalColors.primary[400],      // Lighter for dark mode
        secondary: globalColors.secondary[400],
        accent: globalColors.accent.gold[500],
        background: globalColors.primary[900],   // #0B121E
        surface: globalColors.primary[800],      // #17253B
        text: globalColors.accent.ivory[500],    // #FFF8E7
        textMuted: globalColors.gray[400],
        border: globalColors.primary[700],
        borderSoft: globalColors.primary[800],
    }
};
```

**File**: `tailwind.config.js`

```javascript
module.exports = {
    theme: {
        extend: {
            colors: {
                // Azulejo palette
                'azulejo-blue': {
                    50: '#EBF1F8',
                    500: '#3C5E95',
                    900: '#0B121E',
                },
                'azulejo-cobalt': {
                    500: '#5081B6',
                },
                'azulejo-gold': '#D4AF37',
                'azulejo-terracotta': '#C1440E',
                'azulejo-ivory': '#FFF8E7',
                
                // Semantic aliasing
                primary: '#3C5E95',
                secondary: '#5081B6',
            },
            
            // Custom gradients for hero/sections
            backgroundImage: {
                'hero-gradient': 'linear-gradient(135deg, #3C5E95 0%, #5081B6 100%)',
                'gold-gradient': 'linear-gradient(135deg, #D4AF37 0%, #B8962E 100%)',
                'panorama-overlay': 'linear-gradient(to right, rgba(60,94,149,0.85), rgba(80,129,182,0.75), rgba(80,129,182,0.6))',
            }
        }
    }
};
```

---

### TASK 2: Update Header & Footer

**File**: `src/layout_and_navigation/header/Header.tsx`

**Changes**:
- Update header text to "TileStories"
- Change navigation links: Home, About
- Update header gradient to use azulejo colors
- Update logo styling to complement blue tones

---

## COMPONENT SPECIFICATIONS

This section details the components needed for the TileStories landing page, following the vision from site_notes.md and the phased approach from App_Plan.md.

### Hero Section (`src/domains/home/HeroSection.tsx`)

**Purpose**: Capture visitor attention immediately with compelling visuals and clear value proposition

**Content Elements**:
- **Headline (H1)**: "Explore Lisbon's Lost Skyline Through AR"
- **Subheadline**: "Experience the Grande Panorama de Lisboa like never before. Point your phone at this 18th-century masterpiece and watch history come alive."
- **Background**: Hero image of the Grande Panorama (use hero_img-Dnolkfcy.png or update)
- **CTA Button**: "Start Your AR Journey" (primary azulejo blue)
- **Supporting Text**: Brief mention of 150+ buildings, 4 historical epochs, earthquake simulation

**Design**:
```tsx
// Gradient overlay with azulejo colors
background: linear-gradient(
    180deg, 
    rgba(60, 94, 149, 0.85) 0%,    // Azulejo blue
    rgba(80, 129, 182, 0.75) 50%,  // Cobalt
    rgba(212, 175, 55, 0.3) 100%   // Gold accent at bottom
);

// Typography
H1: font-size: 3rem (mobile), 4rem (desktop)
    font-weight: 800
    color: white
    text-shadow for readability

Subheadline: font-size: 1.25rem
             color: azulejo-ivory-300
             max-width: 720px
```

**Interactive Elements**:
- Scroll indicator (animated arrow)
- "Watch Demo" video modal (Phase 3)
- Subtle parallax effect on background (Phase 5)

---

### Problem Statement Section

**Purpose**: Establish why AR for museums matters (target: tourists, educators, museum professionals)

**Content Structure**:

#### Subheading: "Why AR for Museums?"

**Three-column layout**:

1. **Traditional Museum Experience**
   - Icon: 🖼️
   - Text: "Static displays limit engagement. Visitors struggle to imagine historical context."
   - Color: gray scale

2. **The AR Solution** 
   - Icon: ✨ (or custom AR icon)
   - Text: "Interactive AR layers bring artifacts to life. See buildings as they were, compare epochs, witness historical events."
   - Color: azulejo-blue gradient background

3. **Impact**
   - Icon: 📈
   - Text: "Increased engagement time, deeper learning, memorable experiences that visitors share."
   - Color: azulejo-gold accents

**Design Notes**:
- Use card components from `src/components/ui/Button.tsx` patterns
- Cards elevate on hover (subtle 3D lift)
- Icons use azulejo colors (blue-500, cobalt-500, gold-500)

---

### Grande Panorama Showcase Section

**Purpose**: Introduce the specific artifact and its historical significance

**Content** (from Google Arts & Culture article + site_notes.md):

#### H2: "The Grande Panorama de Lisboa"

**Split layout** (60/40):

**Left Column - Image**:
- High-resolution image of the panorama section
- Caption: "23-meter azulejo panel by Gabriel del Barco (~1700), showing pre-earthquake Lisbon"
- Image overlay on hover: "Explore in AR →"

**Right Column - Details**:
- **Historical Context**:
  - Created ~1700, pre-earthquake Lisbon (1755)
  - 23 meters long, showing Tagus River, 150+ buildings
  - Located at Museu Nacional do Azulejo

- **What Makes It Special**:
  - "Only panoramic view of Lisbon before the 1755 earthquake"
  - "Shows buildings that no longer exist"
  - "Masterpiece of Portuguese azulejo art"

- **AR Enhancement**:
  - "Point your device at the panorama"
  - "Tap buildings to reveal their stories"
  - "Compare 4 historical epochs"
  - "Experience the 1755 earthquake simulation"

**CTA**: "Explore the Interactive Guide" (secondary button)

---

### AR Demo / Screenshots Section

**Purpose**: Show visitors what the AR experience looks like

**Layout**: Image carousel or grid (3-4 screenshots)

**Screenshots needed** (placeholders for now, update in Phase 4):
1. AR view of a building with info overlay
2. Epoch comparison slider (before/after)
3. Earthquake simulation screenshot
4. Interactive map view

**Captions**:
- "Tap any building to reveal its history"
- "Slide between 4 historical epochs"
- "Experience the devastating 1755 earthquake"
- "Explore with an interactive map"

**Technical**: Use `<img>` with lazy loading, `loading="lazy"`

---

### Historical Epochs Overview Section

**Purpose**: Highlight the 4 time periods featured in the app

**H2**: "Journey Through Time: 4 Historical Epochs"

**Grid Layout** (2x2 on desktop, 1 column on mobile):

1. **Pre-Earthquake Glory (~1700)**
   - Color: azulejo-blue-500 card
   - Description: "See Lisbon at its peak before the catastrophic earthquake"
   - Icon: Historical building icon

2. **The Great Earthquake (1755)**
   - Color: azulejo-terracotta-500 card (dramatic)
   - Description: "Experience the event that changed Lisbon forever"
   - Icon: Shaking/earthquake icon

3. **Pombaline Reconstruction (1760s-1800s)**
   - Color: azulejo-cobalt-500 card
   - Description: "Watch the city rise from the ashes with modern urban planning"
   - Icon: Construction/rebuild icon

4. **Modern Day (Present)**
   - Color: azulejo-gold-500 card
   - Description: "Compare the historical view to today's Lisbon skyline"
   - Icon: Modern city icon

**Design**:
- Each card has subtle gradient background
- Hover effect: card lifts with shadow
- Click action (Phase 3): Opens modal with more details

---

### App Features Summary Section

**Purpose**: Quick feature highlights for scanning visitors

**H2**: "What You'll Discover"

**Features Grid** (3 columns):

✓ **150+ Identified Buildings**  
Tap any structure to learn its name, purpose, and fate

✓ **4 Historical Epochs**  
Slide between time periods to see Lisbon's evolution

✓ **Earthquake Simulation**  
Experience the 1755 earthquake that reshaped the city

✓ **Interactive Map**  
Navigate the panorama with an easy-to-use map interface

✓ **Educational Content**  
Learn from expert historians and museum curators

✓ **Multilingual Support**  
Available in Portuguese and English (Phase 2+)

**Design**:
- Checkmark icons in azulejo-green or azulejo-gold
- Simple bullet-style layout
- Light azulejo-ivory background with subtle tile pattern

---

### Thesis Context Section (Optional - for academic transparency)

**Purpose**: Acknowledge this is a thesis project (builds trust, explains scope)

**H3**: "An Academic Collaboration"

**Content**:
- "TileStories is a master's thesis project at FCT NOVA in collaboration with the Museu Nacional do Azulejo"
- "Duration: 12 months (MVP by Month 6, Excellence by Month 12)"
- "Supervised by Dr. [Name], focusing on AR applications for cultural heritage"

**Design**: Subtle gray box, smaller font, at bottom of page

---

### Call to Action Section

**Purpose**: Drive visitor action (download app, visit museum, subscribe)

**H2**: "Ready to Explore?"

**Two-column CTA**:

**Primary CTA**:
- Button: "Download the App" (azulejo-blue-500, large)
- Subtext: "Available for iOS and Android" (placeholder - update when available)

**Secondary CTA**:
- Button: "Visit the Museum" (azulejo-cobalt outline button)
- Subtext: "Museu Nacional do Azulejo, Lisbon"
- Link to museum website

**Tertiary CTA**:
- Newsletter signup: "Get updates on new features and exhibitions"
- Input field + "Subscribe" button (azulejo-gold)

---

## SEO CONTENT TEMPLATES

Based on SEO_EXAMPLES.md target search queries:

### Meta Tags (for `public/index.html` or Next.js `<Head>`)

```html
<title>TileStories | AR Experience for Lisbon's Grande Panorama - Museu do Azulejo</title>

<meta name="description" content="Explore Lisbon's pre-earthquake skyline through augmented reality. Interactive AR app for the Grande Panorama de Lisboa at Museu Nacional do Azulejo. 150+ buildings, 4 historical epochs, 1755 earthquake simulation.">

<meta name="keywords" content="augmented reality museum Lisbon, Grande Panorama de Lisboa AR, Museu do Azulejo app, 1755 earthquake simulation, Lisbon historical tour AR, Portuguese azulejo interactive, AR museum guide Portugal">

<!-- Open Graph for social sharing -->
<meta property="og:title" content="TileStories - AR for Lisbon's Lost Skyline">
<meta property="og:description" content="Point your phone at the 23-meter Grande Panorama and watch 18th-century Lisbon come alive.">
<meta property="og:image" content="[URL to hero image]">
<meta property="og:url" content="https://tilestories.app">

<!-- Twitter Card -->
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="TileStories - AR Museum Experience">
<meta name="twitter:description" content="Explore Lisbon's history through AR at Museu do Azulejo">
```

---

### H1/H2 Examples for SEO

**Primary H1** (only one per page):
```
"Explore Lisbon's Lost Skyline Through AR"
```

**Supporting H2s**:
```
"Why AR for Museums?"
"The Grande Panorama de Lisboa: A 23-Meter Masterpiece"
"Journey Through Time: 4 Historical Epochs"
"What You'll Discover in the TileStories App"
"Visit Museu Nacional do Azulejo in Lisbon"
```

**SEO Strategy**:
- H1 includes keywords: "Lisbon", "AR"
- H2s include long-tail keywords: "Museu do Azulejo", "Grande Panorama", "historical epochs", "AR app"
- Natural language matching tourist queries: "augmented reality museum Lisbon", "1755 earthquake simulation"

---

### Target Search Queries (from SEO_EXAMPLES.md)

**Tourist Searches**:
- "things to do in Lisbon"
- "Lisbon museum tours"
- "interactive museum experiences Lisbon"
- "augmented reality museum Lisbon"

**AR Museum Searches**:
- "AR apps for museums"
- "augmented reality cultural heritage"
- "museum technology Portugal"

**Educational Searches**:
- "1755 Lisbon earthquake"
- "Portuguese azulejo art"
- "pre-earthquake Lisbon"
- "Grande Panorama de Lisboa"

**Local SEO**:
- "Museu do Azulejo"
- "Museu Nacional do Azulejo Lisbon"
- "azulejo museum AR"

---

### Schema.org Markup (JSON-LD)

Add to `<script type="application/ld+json">`:

```json
{
  "@context": "https://schema.org",
  "@type": "WebApplication",
  "name": "TileStories",
  "description": "AR app for exploring the Grande Panorama de Lisboa at Museu Nacional do Azulejo",
  "applicationCategory": "EducationalApplication",
  "operatingSystem": "iOS, Android",
  "offers": {
    "@type": "Offer",
    "price": "0",
    "priceCurrency": "EUR"
  },
  "about": {
    "@type": "Museum",
    "name": "Museu Nacional do Azulejo",
    "address": {
      "@type": "PostalAddress",
      "streetAddress": "Rua da Madre de Deus, 4",
      "addressLocality": "Lisbon",
      "postalCode": "1900-312",
      "addressCountry": "PT"
    }
  }
}
```

---

## DEVELOPMENT CHECKLIST

### Phase 1: Foundation ✅ COMPLETED
- [x] Initialize fresh git repo
- [x] Remove clinic-compare references
- [x] Update color system with azulejo palette
  - [x] `src/design/colors.ts` (globalColors, themeColors, getPrimaryColor, getPrimaryGradient)
  - [x] `tailwind.config.js` (azulejo-blue, azulejo-cobalt, azulejo-gold, azulejo-terracotta, azulejo-ivory)
  - [x] `src/design/themeClasses.ts` (primary, primarySoft, interactive options)
- [x] Fix build errors
  - [x] Remove pokemon/comparing imports from HomePage.tsx
  - [x] Update Button.tsx (brand → primary)
  - [x] Update layout_and_navigation/colors.ts (getBrandGradient → getPrimaryGradient)
  - [x] Fix Header.tsx TypeScript types (MouseEvent)
- [x] Verify build succeeds (`npm run build` ✓)

### Phase 2: Core Sections 🔲 TODO
- [ ] Update HeroSection.tsx
  - [ ] New headline: "Explore Lisbon's Lost Skyline Through AR"
  - [ ] Update background gradient to azulejo colors
  - [ ] Add CTA button "Start Your AR Journey"
  - [ ] Update hero image (hero_img or new panorama image)
  
- [ ] Create ProblemStatement.tsx (`src/domains/home/ProblemStatement.tsx`)
  - [ ] Three-column card layout
  - [ ] Icons and content as specified above
  - [ ] Hover effects with azulejo colors
  
- [ ] Create PanoramaShowcase.tsx (`src/domains/home/PanoramaShowcase.tsx`)
  - [ ] 60/40 split layout
  - [ ] Historical context from Google Arts article
  - [ ] Image with AR overlay on hover
  
- [ ] Create ARDemoSection.tsx (`src/domains/home/ARDemoSection.tsx`)
  - [ ] Image carousel or grid
  - [ ] Placeholder screenshots (update in Phase 4)
  - [ ] Captions for each screenshot
  
- [ ] Create EpochsSection.tsx (`src/domains/home/EpochsSection.tsx`)
  - [ ] 2x2 grid (responsive)
  - [ ] Four epoch cards with colors:
    - Pre-Earthquake: azulejo-blue-500
    - Earthquake: azulejo-terracotta-500
    - Reconstruction: azulejo-cobalt-500
    - Modern Day: azulejo-gold-500
  
- [ ] Create FeaturesSection.tsx (`src/domains/home/FeaturesSection.tsx`)
  - [ ] 3-column grid
  - [ ] Checkmark icons
  - [ ] 6 features as listed above
  
- [ ] Create CTASection.tsx (`src/domains/home/CTASection.tsx`)
  - [ ] Primary CTA: "Download the App"
  - [ ] Secondary CTA: "Visit the Museum"
  - [ ] Newsletter signup form

- [ ] Update HomePage.tsx to import and render all sections

### Phase 3: Interactive Features 🔲 TODO
- [ ] Add video modal for "Watch Demo" button
- [ ] Implement epoch card click actions (modals with detailed info)
- [ ] Add image carousel navigation (arrows, dots)
- [ ] Implement smooth scroll-to-section from header navigation
- [ ] Add "Back to Top" button (already exists in layout, verify styling)

### Phase 4: Content & SEO 🔲 TODO
- [ ] Replace placeholder hero image with actual Grande Panorama image
- [ ] Create/obtain 4 AR screenshot images (AR view, epoch comparison, earthquake, map)
- [ ] Add meta tags to `index.html` or create SEO component
- [ ] Implement Schema.org JSON-LD markup
- [ ] Add favicon with azulejo tile design
- [ ] Create social sharing images (Open Graph 1200x630)
- [ ] Update manifest.json with correct app icon paths

### Phase 5: Polish & Performance 🔲 TODO
- [ ] Add subtle parallax effect to hero section background
- [ ] Optimize images (WebP format, lazy loading)
- [ ] Add loading skeletons for sections
- [ ] Implement page transitions (fade-in on scroll)
- [ ] Test mobile responsiveness on actual devices
- [ ] Accessibility audit (screen reader, keyboard navigation)
- [ ] Performance audit (Lighthouse score target: 90+)
- [ ] Add subtle azulejo tile pattern to backgrounds (CSS pseudo-element)
- [ ] Implement theme toggle (if desired - light/dark modes)

### Multilingual Support (Phase 6 - Optional)
- [ ] Portuguese translations
  - [ ] Update `src/utils/language/` with Portuguese content
  - [ ] Translate all section headings
  - [ ] Translate CTA buttons
  - [ ] Translate meta descriptions
- [ ] Language selector in header (already exists, verify Portuguese added)

---

## TECHNICAL NOTES

### File Structure After Completion

```
src/domains/home/
├── HomePage.tsx (main container, imports all sections)
├── HeroSection.tsx ✅ (exists, needs update)
├── ProblemStatement.tsx (NEW)
├── PanoramaShowcase.tsx (NEW)
├── ARDemoSection.tsx (NEW)
├── EpochsSection.tsx (NEW)
├── FeaturesSection.tsx (NEW)
├── CTASection.tsx (NEW)
└── index.ts (exports)
```

### Key Dependencies
- React Router (already installed, v7.9.3)
- Tailwind CSS (configured with azulejo colors)
- Existing UI components: Button, layout components

### Color Reference (Quick Lookup)
```
Primary (Azulejo Blue): #3C5E95
Secondary (Cobalt): #5081B6
Gold Accent: #D4AF37
Terracotta (Drama): #C1440E
Ivory (Background): #FFF8E7
```

### Gradient Reference
```css
/* Hero Gradient */
background: linear-gradient(135deg, #3C5E95 0%, #5081B6 100%);

/* Gold Gradient (for buttons/accents) */
background: linear-gradient(135deg, #D4AF37 0%, #B8962E 100%);

/* Panorama Overlay */
background: linear-gradient(
    to right, 
    rgba(60,94,149,0.85), 
    rgba(80,129,182,0.75), 
    rgba(80,129,182,0.6)
);
```

---

## DEPLOYMENT NOTES

### Build Command
```bash
npm run build
```
**Output**: `dist/` folder with optimized production files

### Environment Variables
Create `.env.production`:
```
VITE_APP_NAME=TileStories
VITE_APP_DESCRIPTION=AR Experience for the Grande Panorama de Lisboa
VITE_APP_URL=https://tilestories.app
```

### Hosting Recommendations
- **Netlify** (easy deployment from GitHub)
- **Vercel** (optimized for React/Vite apps)
- **GitHub Pages** (free, but requires custom domain for HTTPS)

### Continuous Deployment
1. Connect GitHub repo to Netlify/Vercel
2. Set build command: `npm run build`
3. Set publish directory: `dist`
4. Auto-deploy on push to `main` branch

---

## MAINTENANCE & UPDATES

### Post-Launch Tasks
- [ ] Monitor Google Analytics (Phase 4 - add tracking code)
- [ ] Collect user feedback via form or email
- [ ] Update app screenshots when iOS/Android apps are ready
- [ ] Add blog section for museum news (future expansion)

### Performance Monitoring
- Use Lighthouse CI in GitHub Actions
- Target scores: Performance 90+, Accessibility 100, SEO 95+

---

## SUCCESS CRITERIA

This BUILD_PLAN is considered complete when:

✅ All Phase 1 tasks completed (color system, build fixes)  
✅ All Phase 2 tasks completed (core sections implemented)  
✅ All sections render correctly on mobile and desktop  
✅ Build succeeds without errors  
✅ Lighthouse Performance score > 90  
✅ Lighthouse SEO score > 95  
✅ All content from site_notes.md integrated  
✅ SEO meta tags from SEO_EXAMPLES.md implemented  
✅ Phased delivery aligns with App_Plan.md timeline (MVP Month 6, Excellence Month 12)

---

**END OF BUILD_PLAN.md**

*Last Updated*: February 5, 2026  
*Status*: Phase 1 Complete ✅ | Phase 2 Ready to Start 🚀  
*Next Action*: Begin implementing core sections (HeroSection update, ProblemStatement, PanoramaShowcase)
