Ar switch optionsCódigo · HTML BaixarNow for the POI markers, if possible i want to have some simple design that can be good looking and can represent 1 or if possible more variables like the type of poi (bridge, castle, house...) and the earthquake result (stayed the same, changed when reconstruction or destroyed and don't exist anymore)
Give me many options of design for that01:59Let me think through this properly before designing — POI markers need to encode multiple variables simultaneously while staying readable at map scale.Read frontend design skill for design guidanceRead frontend design skill for design guidanceGood — I can see the brand properly now. Let me think through the design problem carefully before writing any code.
The key challenge: markers need to encode 2 dimensions simultaneously at glanceable map scale:

Type (bridge, castle, church, house, palace...)
Earthquake fate (survived intact / rebuilt/changed / destroyed)

And they need to work at small size, dark + light map backgrounds, and feel premium.Criou um arquivo, leu um arquivoCriou um arquivo, leu um arquivo5 marker systems, all hoverable. Here's the logic behind the design decisions:

The core encoding challenge is that you have two variables — type and fate — and you need both readable at tiny map scale. The solution is to assign each variable to a different visual channel so they don't fight each other:
SystemType encoded viaFate encoded viaBest forA · TeardropIcon inside headFill colorTap/selected stateB · Azulejo TileIconBottom color bandNeighbourhood zoom — most on-brandC · Circle + dotEmoji iconBorder + corner dotDense mapsD · Halo dotInner fill colorRing color + styleZoomed out city viewE · Split badgeRight iconLeft color codeFew markers, explicit reading
Destroyed is consistently handled across all systems: desaturated, dimmed to ~55% opacity, dashed border where applicable. It reads as "absent" without being invisible.
My strong recommendation: use B + D together as a multi-zoom system. The azulejo tile shape is completely unique to your brand — no other map app uses it — and it communicates TileStories' identity the moment someone sees the map. At city zoom when tiles would be too small, collapse them to System D (halo dots). When a marker is tapped/selected, expand it to System A (the pin) as the active callout state. Three systems, one coherent visual language.Poi markersCódigo · HTML BaixarMaybe give me example with circle with the type icon inside + plus the type colour as circle background. And the outer line represents the earthquake fate and maybe red for destroyed and dots border line, blue for rebuilt and dashed line, and green for survived with continuous line... ?? And these borders colors maybe can have a bit of smooth or shine to make them better looking more modern? And for small scales it is all the same except the circle is smaller and we don't show icon. Maybe we can have 4 scales??And for the selected marker we make the shape square with round corners and with more overflow effects?? Can a schema like this be good and modern and premium looking?Mostrar mais02:17Great direction — this is actually a very clean information design system. Let me build it properly.Visualizou um arquivo, criou um arquivo, leu um arquivoVisualizou um arquivo, criou um arquivo, leu um arquivoYes — this system is absolutely good and modern and premium. Here's why it works so well:
The encoding is crystal clear at a glance. Your instinct to use border style as the fate signal is information-design gold — solid/dashed/dotted is a universally understood visual grammar (used in cartography for centuries), and the colored glow makes each state immediately distinct even on a cluttered map background.
The 4-scale progression is logical:

Scales 1–2: just the colored circle + glowing ring — pure chromatic data, no visual noise at city/district zoom
Scales 3–4: icon appears as soon as there's room to read it
Selected state: the squircle morph (border-radius: 30%) gives a satisfying transformation without being jarring, and the pulsing aura anchors it as "this is what you tapped"

Destroyed markers feel appropriately absent — the opacity drop + grayscale filter + dotted red ring communicates "there used to be something here" without hiding the marker entirely.
What to hover — all markers respond, and the tooltip appears only on the matrix view so you can test the hover behavior there.Poi markers v2Código · HTML Baixar2 / 2https://artsandculture.google.com/story/gAWhceMYFOAfIA?hl=pt-PTI think there are some 150 poi on the panorama. What would be the good types to have? And what icons symbol for each?And in terms of the "destruction level" maybe we can have those 3 styles red blue green and dot dashed continuous line in a range and then we can interpolate and have a spectrum of levels? Example destruction 100% and doesn't exist anymore or destroyed 50% and there are still ruins... ??08:21Fetched: Grande Panorama de Lisboa - Google Arts & CultureFetched: Grande Panorama de Lisboa - Google Arts & CultureExcellent — this is rich. Now I have a clear picture of what's in the panorama. Let me also think through the spectrum idea carefully before designing.Criou um arquivo, leu um arquivoCriou um arquivo, leu um arquivoThere are 4 sections — use the live builder (③) to drag the slider and switch types to feel how it works.

On the POI types — reading through the panorama, it's actually quite clean. About 16 distinct types, which group naturally into 8 categories. The dominant ones you'll encounter most are:

Churches (~35) and Convents (~20) — over half the POIs are religious. Lisbon really was the "city of a thousand churches"
Noble palaces/quintas (~21) — especially dense in Alcântara and Belém
Residential blocks (~20) — representing entire neighbourhoods, not individual houses
The maritime types (quays, shipyards, docks) are relatively few but visually prominent in the panorama

On the destruction spectrum — yes, this works really well and is more honest than 3 binary states. The 6 semantic levels I'd suggest are:
%LabelReal example0%IntactTorre de Belém, Sé Cathedral20%Minor damageMost of Alfama40%Partially damagedMost rebuilt churches60%Heavily changedRebuilt in Pombaline style80%RuinsConvento do Carmo (kept as ruin intentionally)100%VanishedPaço da Ribeira (royal palace), Casa da Índia
The border goes solid → long-dash → short-dash → dots as destruction increases, which encodes the idea of something becoming less "whole" — a nice metaphor.


Type colour reasoning — intuitive mapping:
(not like in the html examples, maybe we should implement these final colors mappings:)

Religious → Deep purple/violet — globally associated with spirituality, liturgy, incense, vestments
Royal/Noble → Gold/amber — universal royalty signal, also echoes the app's own gold
Military → Red/crimson — danger, blood, fortification; universal
Civic → Sky blue — civic trust, public space, municipal; used in city branding worldwide
Maritime → Teal/cyan — water, sea, sailors; obvious
Infrastructure → Warm grey/stone — neutral, utilitarian, roads and walls; no obvious "wrong" colour
Landscape → Green — nature, hills, vegetation; universal
Commerce → Warm orange — markets, trade, energy; used in many commerce UIs



The 3-component type row (dot · icon · name) is now explicit in both variants. The colour dot alone can identify the category on a small map circle with no icon — the user trains their eye to "purple = religious, red = military, teal = maritime" etc.
Type colours — the reasoning:
CategoryColourWhy it's obviousReligiousVioletLiturgy, vestments, incense — universal spiritual colourRoyal/NobleAmberGold = royalty everywhere, and echoes the app's own goldMilitaryCrimsonDanger, fortification — every map ever uses red for militaryCivicCobalt bluePublic trust, municipal — city halls, civic buildings worldwideMaritimeTealThe Tejo river, sea, sailors — distinct from civic blueInfrastructureStone greyNeutral, utilitarian — literally the colour of the bridges and wallsLandscapeForest greenThe most universal colour–meaning mapping in existenceCommerceOrangeTrade, markets, energy — Amazon, Shopify, every marketplace



Icon decisions (SVG, not emoji):

Religious → cross/dome silhouette (universal church symbol on every map ever)
Royal/Noble → crown (unambiguous, globally understood)
Military → shield with diagonal (fortification, defence)
Civic → classical column/pillar (public building, forum)
Maritime → anchor (universal nautical symbol)
Infrastructure → arch/bridge (the thing itself)
Landscape → tree/hill (nature)
Commerce → scales of trade (market, exchange)




Both are fully interactive — tap the ⓘ button or the Legenda pill to open the panel, then Details on each section to expand. Tap the map to close.

The ⓘ icon is styled in the app's gold palette — thin circle with a dot above and a vertical stroke below, slightly more refined than the standard bold info icon in the reference image. It reads instantly as "information" while feeling like it belongs to TileStories.
SVG icons chosen for each type — all drawn as clean 20×20 geometric strokes, no emoji:
TypeIconWhyReligiousCross + archEvery church map symbol worldwide, instantly legibleRoyalCrownZero ambiguity — the most universal royalty symbolMilitaryShield + crossFortification, defence, heraldryCivicTemple columns + pedimentPublic buildings, forums, civic architectureMaritimeAnchorThe nautical symbol since antiquityInfrastructureArch bridgeLiterally the thing itself — the Alcântara bridge is in the panoramaLandscapeHill + layered linesCartographic convention for hills/terrainCommerceBalance scalesTrade, exchange, markets — used since Roman times
The Details section for each type follows exactly your structure: category header with colour dot + icon, then bullet sub-types each with a description line below in a lighter colour.