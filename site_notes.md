# BUILD: TileStories Landing Site - Museu Nacional do Azulejo AR App

## TECH STACK: React + TypeScript + Tailwind + Vite

## STRUCTURE:
/src
    /domain
        /brand (colors, typography, logo paths)
        /home
            /Hero, ProblemStatement, AppPhases, Timeline, Demo, CTA, 
        /Contact
        /domain or components?? 
            ARDemo, BeforeAfterSlider, EarthquakeAnimation, PhaseCards
    

## COLOR PALETTE (domain/brand/colors.ts):
- azulejo-blue: #3C5E95 (primary, CTA, headers)
- azulejo-cobalt: #5081b6 (accents, hover states)
- ivory: #FFF8E7 (light bg, card backgrounds)
- gold: #D4AF37 (highlights, achievements, premium features)
- terracota: #C1440E (earthquake section, historical moments)
- dark-base: #0A1929 (dark mode bg)

## SECTIONS ARCHITECTURE:

### 1. HEADER (sticky, glass morphism)
     - Logo (azulejo tile icon + "TileStories" wordmark)
     - Nav: [Projeto, App, Timeline, Demo, Contacto]
     - Language toggle (PT/EN)
     - Dark mode toggle
     - CTA: "Experimentar Demo"

### 2. HERO (fullscreen, parallax layered)
     - BG: Grande Panorama faded with blue overlay gradient
     - H1: "150 Edifícios. 4 Épocas. 300 Anos de História na Palma da Mão"
     - Subhead: "Experiência AR para o Grande Panorama de Lisboa | Tese Mestrado FCT NOVA × Museu Nacional do Azulejo"
     - Dual CTA: "Ver Demo AR" (primary) + "Download Apresentação PDF" (ghost)
     - Scroll indicator animation
     - Institutional logos strip: FCT NOVA + Museu Nacional Azulejo

### 3. PROBLEM STATEMENT (centered, minimalist)
     - Visual: 23m panorama photo with frustrated visitor silhouette
     - Text: "Como tornar 23 metros de azulejos do séc. XVIII relevantes para visitantes de hoje?"
     - Stats cards (3 cols): "23m comprimento" | "300+ anos" | "150+ edifícios invisíveis"

### 4. SOLUTION OVERVIEW (3-col grid)
     - Card 1: AR icon → "Aponta e Descobre" → smartphone mockup with AR overlay
     - Card 2: Timeline icon → "Viaja no Tempo" → slider 1700→1755→hoje
     - Card 3: Brain icon → "Experiência Personalizada" → profile badges (estudante, turista, criança)

### 5. APP PHASES (vertical timeline with side-by-side content)
     Each phase: Badge + Title + Feature list + Visual mockup + Cost tag
     
     **FASE 1 MVP (€541):**
     - 30 edifícios AR identificáveis
     - Timeline 3 épocas
     - Info cards interativos
     - Mockup: phone pointing at panel with AR pins
     
     **FASE 2 CORE (€617):**
     - Perfis académicos personalizados
     - 5 circuitos temáticos + áudio guia
     - 5 edifícios Unity 3D premium
     - Gamificação (badges, leaderboard)
     - Mockup: 3D Castelo S. Jorge rotativo
     
     **FASE 3 WOW (€822):**
     - Simulação Terramoto 1755 (highlight with terracota accent)
     - GPT-4 Q&A conversacional
     - 100 edifícios completos
     - 360° interior views
     - Analytics para tese
     - Mockup: earthquake animation still + chat bubble
     
     **FASE 4 EXCELÊNCIA (€967):**
     - 150 edifícios finalizados
     - Multilíngue (PT/EN/ES)
     - Acessibilidade WCAG 2.1
     - App stores publicada
     - 1000+ visitantes validados
     - Mockup: app store listings screenshots

### 6. INTERACTIVE DEMO SECTION
     - WebAR micro-demo: QR code → scan → see 1 building in AR (Castelo or Sé)
     - Before/After slider: Lisboa 1700 vs 1755 vs 2026 (embedded Google Maps comparison)
     - Mini earthquake animation trigger button (3s vibration + collapse animation)

### 7. PERSONAS SHOWCASE (horizontal scrollable cards)
     - Maria, 12: "Aventura gamificada com quiz e badges"
     - João, 22, Arquitetura: "Análise detalhada estilos e proporções"
     - Emma, Tourist: "English audio guide + quick 30min circuit"
     Each: avatar illustration + quote + relevant app screens

### 8. CREDIBILITY SECTION
     - Metrics dashboard (animated counters on scroll): 
         * 150 edifícios | 4 épocas | 5 circuitos | €3,500 investimento | 12 meses | 1000+ validações
     - Academic rigor: "Tese Mestrado com Metodologia Validada"
     - Partnership logos: FCT NOVA + Museu Nacional Azulejo (with MoU badge)

### 9. TIMELINE/ROADMAP (horizontal gantt-style, progress indicator)
     - Months 1-3: MVP ✓
     - Months 4-6: Core (current phase indicator if applicable)
     - Months 7-9: WOW
     - Months 10-12: Launch
     - Milestone markers with deliverable tooltips

### 10. FINANCIAL TRANSPARENCY (optional accordion or modal)
        - Pie chart: Copilot €468 | Assets 3D €1,295 | Stores €124 | AI APIs €260 | Other €564
        - Justification blurb: "Investimento otimizado vs. alternativas (comparar com custo agência €20k+)"

### 11. FOOTER
        - Left: TileStories logo + tagline "História aumentada, conhecimento real"
        - Center: Links [Sobre, Documentação Técnica, GitHub, Privacidade]
        - Right: Contact (email, LinkedIn) + institutional logos
        - Bottom: © 2026 + "Projeto Tese Mestrado FCT NOVA"

## INTERACTIONS:
- Parallax scroll: Hero background moves slower than foreground
- Scroll-triggered animations: fade-in, slide-up, counter animations (use Intersection Observer)
- Phase cards: hover → lift + shadow increase + accent border glow
- AR demo section: sticky when scrolling through phases
- Dark mode: smooth transition, persist localStorage
- Mobile: hamburger menu, collapsible sections, thumb-zone CTAs

## PERFORMANCE:
- Lazy load images below fold
- WebP format with fallbacks
- Preload critical assets (hero image, fonts)
- Code splitting by route/section
- Target: <2s FCP, <3s LCP, 95+ Lighthouse score

## ACCESSIBILITY:
- Semantic HTML (header, nav, main, section, article, footer)
- ARIA labels for interactive elements
- Focus indicators (visible keyboard nav)
- Alt text for all images
- Contrast ratio minimum 4.5:1
- Reduced motion media query support


## SEO:
- Meta title: "TileStories - App AR Museu Nacional Azulejo Lisboa | Experiência Terramoto 1755"
- Meta description: "Explore 150 edifícios do Grande Panorama Lisboa (1700) em realidade aumentada. Visite 4 épocas, viva o terramoto 1755. Projeto tese FCT NOVA."
- OG tags for social sharing (preview image: hero mockup with AR overlay)
- Structured data: Organization, WebApplication schema
- and also access and read this site with more information that you can use when building the site: https://artsandculture.google.com/story/gAWhceMYFOAfIA?hl=pt-PT


## ANALYTICS TRACKING:
- Page views
- Section scroll depth
- CTA clicks (Demo, Download PDF, Contact)
- Demo interactions
- Average time on page


## NOTES:
- Avoid walls of text: max 2-3 sentences per paragraph, bullet lists for features
- Mobile-first: 90% visitors mobile, test on iPhone 12+ and equivalent Android
- Video demo: 30-45s screen recording embedded in Hero or Demo section (MP4, muted autoplay)
- Download PDF: 1-pager executive summary (separate design, link in CTA)
- Emphasize scientific rigor + practical impact balance (not just cool tech, but validated research)
  » but also make everything interesting for a normal user. I want to impress inverstors and visitors of museums that visit this page and after this page will want to visit Museu Nacional do Azulejo. 
