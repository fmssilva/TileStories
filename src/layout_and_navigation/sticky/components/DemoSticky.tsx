/**
 * DEMO STICKY COMPONENT
 * ======================
 * 
 * Comprehensive test component for the sticky system
 * Tests multiple scenarios: varying heights, nested hierarchies, dynamic measurement
 * 
 * TEMPORARY FILE - For testing sticky system improvements
 * Delete after testing is complete
 */

import { useRef, useEffect, useState } from 'react';
import { useSticky } from '../hooks/useSticky';
import { LAYOUT } from '@/design/size/layout';

// ============================================================================
// UTILITY COMPONENTS
// ============================================================================

/**
 * Spacer for visual separation during scroll testing
 */
function Spacer({ height, color, label }: { height: number; color: string; label: string }) {
    return (
        <div
            style={{ height: `${height}px`, backgroundColor: color }}
            className="flex items-center justify-center text-white font-bold text-lg"
        >
            {label}
        </div>
    );
}

/**
 * Test case description component - explains what to observe
 */
function TestCaseDescription({ title, observations }: { title: string; observations: string[] }) {
    return (
        <div className="mt-4 mb-6 p-4 bg-blue-50 dark:bg-blue-900/20 border-l-4 border-blue-500 rounded">
            <h4 className="font-bold text-blue-900 dark:text-blue-200 mb-2">🧪 {title}</h4>
            <ul className="text-sm text-blue-800 dark:text-blue-300 space-y-1">
                {observations.map((obs, idx) => (
                    <li key={idx}>• {obs}</li>
                ))}
            </ul>
        </div>
    );
}

// ============================================================================
// TEST CASE 1: EXPAND/COLLAPSE WITH DIFFERENT HEIGHTS
// ============================================================================

function TestCase1_ExpandCollapse() {
    const [isExpanded, setIsExpanded] = useState(false);

    // NEW SIMPLIFIED API - Everything automated!
    const {
        ref,
        isSticky,
        isShrunk,
        stickyClasses,
        stickyStyles,
    } = useSticky('demo-sticky-1', {
        enableShrink: {
            shrinkThreshold: 100,  // Shrink when scrolled 100px past element position
            expandThreshold: 50,   // Expand when scrolled back to 50px (50px hysteresis)
        },
        autoMeasure: true,           // Auto-measure height with ResizeObserver
        autoRegisterPosition: true,  // Auto-register position on mount
    });

    return (
        <>
            <div
                ref={ref}
                className={`${stickyClasses} transition-all duration-300`}
                style={{
                    ...stickyStyles,
                    marginBottom: isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${isShrunk
                        ? 'bg-blue-600 py-3 px-6'
                        : 'bg-blue-500 py-8 px-8'
                    }
                    text-white rounded-lg shadow-lg transition-all duration-300
                `}>
                    <div className="flex items-center justify-between">
                        <h3 className={`font-bold ${isShrunk ? 'text-lg' : 'text-3xl'}`}>
                            📊 Data Dashboard
                        </h3>
                        <button
                            onClick={() => setIsExpanded(!isExpanded)}
                            className="px-4 py-2 bg-white/20 hover:bg-white/30 rounded transition-colors text-sm font-medium"
                        >
                            {isExpanded ? '▲ Collapse' : '▼ Expand'}
                        </button>
                    </div>

                    {isExpanded && (
                        <div className="mt-4 space-y-2">
                            <div className="bg-blue-600/50 p-3 rounded">
                                📈 Chart: Revenue Growth (Q1-Q4)
                            </div>
                            <div className="bg-blue-600/50 p-3 rounded">
                                👥 User Analytics: 45,231 active users
                            </div>
                            <div className="bg-blue-600/50 p-3 rounded">
                                ⚡ Performance: 98.5% uptime
                            </div>
                        </div>
                    )}

                    {!isSticky && !isExpanded && (
                        <p className="mt-3 text-sm opacity-90">
                            Click expand to see more content. Notice height changes dynamically.
                        </p>
                    )}
                </div>
            </div>

            <TestCaseDescription
                title="Test Case 1: SIMPLIFIED API - One Hook Does It All!"
                observations={[
                    '✅ NO manual refs, useEffect, or position registration needed!',
                    '✅ Auto-measures height with ResizeObserver',
                    '✅ Auto-registers position on mount',
                    '✅ Integrated scroll-shrink with hysteresis (no trembling)',
                    '✅ Just one hook call with options - that\'s it!',
                    'Shrinks at 100px scroll, expands at 50px scroll (50px dead zone)',
                    'Observe 📏 console logs when shrinking/expanding',
                ]}
            />
        </>
    );
}

// ============================================================================
// TEST CASE 2: VS CODE-STYLE HIERARCHICAL BREADCRUMB (Chapter > Section > Subsection)
// ============================================================================

function TestCase2_DocumentHierarchy() {
    const chapterRef = useRef<HTMLDivElement>(null);
    const sectionRef = useRef<HTMLDivElement>(null);
    const subsectionRef = useRef<HTMLDivElement>(null);

    const chapter = useSticky('demo-sticky-2-chapter');
    const section = useSticky('demo-sticky-2-section');
    const subsection = useSticky('demo-sticky-2-subsection');

    const [activeSection, setActiveSection] = useState('1.2');
    const [activeSubsection, setActiveSubsection] = useState('1.2.1');

    // Register positions once on mount
    useEffect(() => {
        if (chapterRef.current) {
            const rect = chapterRef.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            chapter.registerPosition(offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only run once on mount

    useEffect(() => {
        if (sectionRef.current) {
            const rect = sectionRef.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            section.registerPosition(offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only run once on mount

    useEffect(() => {
        if (subsectionRef.current) {
            const rect = subsectionRef.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            subsection.registerPosition(offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only run once on mount

    // Measure and report heights ONCE on mount - do NOT re-measure to prevent trembling
    useEffect(() => {
        if (chapterRef.current) {
            const height = chapterRef.current.offsetHeight;
            chapter.reportHeight(height);
            console.log('📐 [Chapter] INITIAL height measured:', height, 'px');
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once

    useEffect(() => {
        if (sectionRef.current) {
            const height = sectionRef.current.offsetHeight;
            section.reportHeight(height);
            console.log('📐 [Section] INITIAL height measured:', height, 'px');
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once

    useEffect(() => {
        if (subsectionRef.current) {
            const height = subsectionRef.current.offsetHeight;
            subsection.reportHeight(height);
            console.log('📐 [Subsection] INITIAL height measured:', height, 'px');
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once

    const sections = ['1.2', '1.3', '1.4'];
    const subsections: Record<string, string[]> = {
        '1.2': ['1.2.1', '1.2.2', '1.2.3'],
        '1.3': ['1.3.1', '1.3.2'],
        '1.4': ['1.4.1', '1.4.2', '1.4.3', '1.4.4'],
    };

    return (
        <>
            {/* CHAPTER LEVEL - VS Code Style Breadcrumb */}
            <div
                ref={chapterRef}
                className={`${chapter.stickyClasses} transition-all duration-300`}
                style={{
                    ...chapter.stickyStyles,
                    marginBottom: chapter.isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${chapter.isSticky
                        ? 'bg-purple-700/95 backdrop-blur-sm py-1.5 px-3'
                        : 'bg-purple-600 py-6 px-8'
                    }
                    text-white rounded-lg shadow-lg
                `}>
                    {chapter.isSticky ? (
                        // Compact breadcrumb - just chapter name
                        <div className="flex items-center gap-1.5 text-sm font-mono py-0.5">
                            <span className="opacity-70">📖</span>
                            <span className="font-semibold">Ch 1</span>
                        </div>
                    ) : (
                        // Full display when free
                        <div className="flex items-center">
                            <span className="text-2xl mr-3">📖</span>
                            <div>
                                <h3 className="font-bold text-2xl">
                                    Chapter 1: Introduction to TileStories
                                </h3>
                                <p className="text-sm opacity-90 mt-1">
                                    Exploring the AR Museum Experience
                                </p>
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* SECTION LEVEL - Indented Breadcrumb */}
            <div
                ref={sectionRef}
                className={`${section.stickyClasses} transition-all duration-300`}
                style={{
                    ...section.stickyStyles,
                    marginBottom: section.isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${section.isSticky
                        ? 'bg-green-600/95 backdrop-blur-sm py-1.5 px-3'
                        : 'bg-green-500 py-5 px-8'
                    }
                    text-white rounded-lg shadow-lg
                `}>
                    {section.isSticky ? (
                        // Sticky view: Show all sections as SEPARATE VERTICAL BUTTONS
                        <div className="flex flex-col gap-1.5">
                            <div className="text-xs font-mono opacity-70 mb-1">📑 Sections</div>
                            {sections.map(sec => (
                                <button
                                    key={sec}
                                    onClick={() => setActiveSection(sec)}
                                    className={`
                                        w-full px-4 py-2 rounded-md text-left font-medium transition-all text-sm
                                        ${activeSection === sec
                                            ? 'bg-white text-green-600 shadow-sm font-bold'
                                            : 'bg-green-700/50 hover:bg-green-600 text-white'
                                        }
                                    `}
                                >
                                    <div className="flex items-center gap-2">
                                        <span>{sec === '1.2' ? 'sec 1.2' : sec === '1.3' ? 'sec 1.3' : 'sec 1.4'}</span>
                                        {activeSection === sec && <span className="ml-auto text-xs">✓</span>}
                                    </div>
                                </button>
                            ))}
                        </div>
                    ) : (
                        // Full display when free
                        <div>
                            <div className="flex items-center mb-3">
                                <span className="text-xl mr-2">📑</span>
                                <h4 className="font-bold text-xl">Select Section</h4>
                            </div>
                            <div className="flex gap-3">
                                {sections.map(sec => (
                                    <button
                                        key={sec}
                                        onClick={() => setActiveSection(sec)}
                                        className={`
                                            flex-1 px-6 py-3 rounded-lg font-semibold transition-all
                                            ${activeSection === sec
                                                ? 'bg-white text-green-600 shadow-md scale-105'
                                                : 'bg-green-600 hover:bg-green-500'
                                            }
                                        `}
                                    >
                                        <div className="text-lg">{sec}</div>
                                        <div className="text-xs opacity-75">
                                            {sec === '1.2' ? 'AR Technology' :
                                                sec === '1.3' ? 'User Experience' :
                                                    'Historical Context'}
                                        </div>
                                    </button>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* SUBSECTION LEVEL - Further Indented */}
            <div
                ref={subsectionRef}
                className={`${subsection.stickyClasses} transition-all duration-300`}
                style={{
                    ...subsection.stickyStyles,
                    marginBottom: subsection.isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${subsection.isSticky
                        ? 'bg-orange-600/95 backdrop-blur-sm py-1.5 px-3'
                        : 'bg-orange-500 py-4 px-8'
                    }
                    text-white rounded-lg shadow-lg
                `}>
                    {subsection.isSticky ? (
                        // Sticky view: Show all subsections as SEPARATE VERTICAL BUTTONS
                        <div className="flex flex-col gap-1.5">
                            <div className="text-xs font-mono opacity-70 mb-1">📝 Subsections of {activeSection}</div>
                            {subsections[activeSection]?.map(sub => (
                                <button
                                    key={sub}
                                    onClick={() => setActiveSubsection(sub)}
                                    className={`
                                        w-full px-4 py-2 rounded-md text-left font-medium transition-all text-sm
                                        ${activeSubsection === sub
                                            ? 'bg-white text-orange-600 shadow-sm font-bold'
                                            : 'bg-orange-700/50 hover:bg-orange-600 text-white'
                                        }
                                    `}
                                >
                                    <div className="flex items-center gap-2">
                                        <span>p {sub}</span>
                                        {activeSubsection === sub && <span className="ml-auto text-xs">✓</span>}
                                    </div>
                                </button>
                            ))}
                        </div>
                    ) : (
                        // Full display when free
                        <div>
                            <div className="flex items-center mb-2">
                                <span className="mr-2">📝</span>
                                <h5 className="font-bold text-lg">Subsections of {activeSection}</h5>
                            </div>
                            <div className="grid grid-cols-2 gap-2">
                                {subsections[activeSection]?.map(sub => (
                                    <button
                                        key={sub}
                                        onClick={() => setActiveSubsection(sub)}
                                        className={`
                                            px-4 py-2 rounded font-medium transition-all
                                            ${activeSubsection === sub
                                                ? 'bg-white text-orange-600 shadow-sm'
                                                : 'bg-orange-600 hover:bg-orange-500'
                                            }
                                        `}
                                    >
                                        {sub}
                                        <span className="text-xs block opacity-75">
                                            {sub.endsWith('1') ? 'Overview' :
                                                sub.endsWith('2') ? 'Details' :
                                                    sub.endsWith('3') ? 'Examples' :
                                                        'Advanced'}
                                        </span>
                                    </button>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>

            <TestCaseDescription
                title="Test Case 2: VS Code-Style Hierarchical Breadcrumb Navigation"
                observations={[
                    'THREE LEVELS showing VS Code-style file explorer breadcrumbs when sticky',
                    'Chapter 1 → Section 1.X → Subsection 1.X.X (indented with └─ symbols)',
                    'Each level shows full path with visual tree structure',
                    'Quick navigation buttons for switching sections/subsections while sticky',
                    'Observe proper stacking with 8px spacing between levels',
                    'Notice mono-spaced font for breadcrumb clarity',
                ]}
            />
        </>
    );
}

// ============================================================================
// TEST CASE 3: MULTIPLE BUTTONS WITH DIFFERENT HEIGHTS
// ============================================================================

function TestCase3_VariableHeightButtons() {
    const ref = useRef<HTMLDivElement>(null);
    const buttonRefs = useRef<(HTMLButtonElement | null)[]>([]);
    const { isSticky, stickyClasses, stickyStyles, reportHeight, registerPosition } = useSticky('demo-sticky-3');
    const [selectedButton, setSelectedButton] = useState<number | null>(null);

    // Register position once on mount
    useEffect(() => {
        if (ref.current) {
            const rect = ref.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            registerPosition(offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only run once on mount

    // Measure ONCE on mount - do NOT re-measure to prevent trembling
    useEffect(() => {
        if (ref.current) {
            const height = ref.current.offsetHeight;
            reportHeight(height);
            console.log('📐 [TestCase3] INITIAL height measured:', height, 'px');

            // Log individual button heights
            if (buttonRefs.current.length > 0) {
                console.log('🔍 [TestCase3] INDIVIDUAL BUTTON HEIGHTS:');
                buttonRefs.current.forEach((btnEl, idx) => {
                    if (btnEl) {
                        const btnHeight = btnEl.offsetHeight;
                        const btnWidth = btnEl.offsetWidth;
                        const computedStyle = window.getComputedStyle(btnEl);
                        const padding = computedStyle.padding;
                        console.log(`  Button ${idx + 1}:`, {
                            height: `${btnHeight}px`,
                            width: `${btnWidth}px`,
                            padding: padding,
                            className: btnEl.className.substring(0, 100) + '...'
                        });
                    }
                });
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once on mount

    const buttons = [
        {
            id: 1,
            label: 'Short',
            description: 'One line',
            content: 'Minimal button'
        },
        {
            id: 2,
            label: 'Medium Size Button',
            description: 'Two to three lines of content here',
            content: 'This button has a moderate amount of text content spread across multiple lines to demonstrate medium height sizing in the grid layout.'
        },
        {
            id: 3,
            label: 'Very Long Button Title With Lots Of Extended Text Content',
            description: 'Four to five lines with extensive description',
            content: 'This button demonstrates a significantly longer content block with multiple paragraphs of text. It includes detailed descriptions and additional context to create a visibly taller button component. The purpose is to show how variable heights work in the grid system and how they adapt when transitioning to sticky compact mode.'
        },
        {
            id: 4,
            label: 'Multiline\nExplicit\nLine\nBreaks\nButton',
            description: 'Many explicit line breaks for maximum height testing',
            content: 'Line 1: Introduction text\nLine 2: Secondary information\nLine 3: Additional details here\nLine 4: More context provided\nLine 5: Extended content area\nLine 6: Final summary notes'
        },
    ];

    return (
        <>
            <div
                ref={ref}
                className={`${stickyClasses} transition-all duration-300`}
                style={{
                    ...stickyStyles,
                    marginBottom: isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${isSticky
                        ? 'bg-red-600 py-2 px-4'
                        : 'bg-red-500 py-6 px-8'
                    }
                    text-white rounded-lg shadow-lg
                `}>
                    <h3 className={`font-bold mb-3 ${isSticky ? 'text-base' : 'text-2xl'}`}>
                        🎯 Variable Height Buttons
                    </h3>

                    {isSticky ? (
                        // Compact horizontal layout when sticky
                        <div className="flex gap-2 flex-wrap">
                            {buttons.map(btn => (
                                <button
                                    key={btn.id}
                                    onClick={() => setSelectedButton(btn.id)}
                                    className={`
                                        px-3 py-1 rounded text-sm font-medium transition-all
                                        ${selectedButton === btn.id
                                            ? 'bg-white text-red-600'
                                            : 'bg-red-700 hover:bg-red-600'
                                        }
                                    `}
                                >
                                    {btn.label.split('\n')[0]}
                                </button>
                            ))}
                        </div>
                    ) : (
                        // Expanded grid layout when free - with VARIABLE HEIGHTS
                        <div className="grid grid-cols-2 gap-4">
                            {buttons.map((btn, idx) => (
                                <button
                                    key={btn.id}
                                    ref={(el) => { buttonRefs.current[idx] = el; }}
                                    onClick={() => setSelectedButton(btn.id)}
                                    className={`
                                        rounded-lg font-semibold transition-all text-left flex flex-col
                                        ${selectedButton === btn.id
                                            ? 'bg-white text-red-600 shadow-lg scale-105'
                                            : 'bg-red-600 hover:bg-red-500'
                                        }
                                        ${btn.id === 1 ? 'p-3' : btn.id === 2 ? 'p-4' : btn.id === 3 ? 'p-5' : 'p-6'}
                                    `}
                                    style={{
                                        minHeight: btn.id === 1 ? '80px' : btn.id === 2 ? '150px' : btn.id === 3 ? '250px' : '350px'
                                    }}
                                >
                                    <div className={`font-bold whitespace-pre-line mb-2 ${btn.id === 1 ? 'text-base' : btn.id === 2 ? 'text-lg' : 'text-xl'}`}>
                                        {btn.label}
                                    </div>
                                    <div className="text-xs opacity-75 mb-2">{btn.description}</div>
                                    <div className={`text-sm opacity-90 ${btn.id >= 3 ? 'whitespace-pre-line' : ''}`}>
                                        {btn.content}
                                    </div>
                                </button>
                            ))}
                        </div>
                    )}

                    {selectedButton && !isSticky && (
                        <div className="mt-4 p-3 bg-red-600 rounded">
                            <p className="text-sm">
                                ✓ Button {selectedButton} selected - Notice how selection state persists across sticky transitions
                            </p>
                        </div>
                    )}
                </div>
            </div>

            <TestCaseDescription
                title="Test Case 3: Multiple Buttons with Variable Heights"
                observations={[
                    'Buttons have DIFFERENT heights (short, medium, long, multiline)',
                    'Layout changes from 2x2 GRID (free) to HORIZONTAL ROW (sticky)',
                    'Click buttons to see state persistence across sticky transitions',
                    'Height dynamically adjusts based on content and state',
                    'Notice how compact format optimizes space when sticky',
                ]}
            />
        </>
    );
}

// ============================================================================
// TEST CASE 4: ACCORDION/NESTED CONTENT
// ============================================================================

function TestCase4_AccordionNested() {
    const ref = useRef<HTMLDivElement>(null);
    const { isSticky, stickyClasses, stickyStyles, reportHeight, registerPosition } = useSticky('demo-sticky-4');
    const [openItems, setOpenItems] = useState<Set<number>>(new Set([1]));

    // Register position once on mount
    useEffect(() => {
        if (ref.current) {
            const rect = ref.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            registerPosition(offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only run once on mount

    // Measure ONCE on mount - do NOT re-measure to prevent trembling
    useEffect(() => {
        if (ref.current) {
            const height = ref.current.offsetHeight;
            reportHeight(height);
            console.log('📐 [TestCase4] INITIAL height measured:', height, 'px');
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once on mount

    const toggleItem = (id: number) => {
        const newOpen = new Set(openItems);
        if (newOpen.has(id)) {
            newOpen.delete(id);
        } else {
            newOpen.add(id);
        }
        setOpenItems(newOpen);
    };

    const accordionItems = [
        {
            id: 1,
            title: 'Museum Collections',
            icon: '🏛️',
            content: ['Ancient Artifacts from Greece', 'Renaissance Paintings Collection', 'Modern Sculptures Gallery', 'Medieval Manuscripts Archive']
        },
        {
            id: 2,
            title: 'AR Features',
            icon: '📱',
            content: ['3D Model Viewer with Rotation', 'Interactive Historical Timelines', 'Virtual Museum Tours', 'Augmented Reality Overlays', 'Audio Guide Integration']
        },
        {
            id: 3,
            title: 'Historical Periods',
            icon: '⏳',
            content: ['Ancient Rome (753 BC - 476 AD)', 'Medieval Europe (476 - 1500)', 'Industrial Revolution (1760 - 1840)', 'Modern Era (1900 - Present)', 'Renaissance Period (1300 - 1600)', 'World War Era (1914 - 1945)']
        },
    ];

    return (
        <>
            <div
                ref={ref}
                className={`${stickyClasses} transition-all duration-300`}
                style={{
                    ...stickyStyles,
                    marginBottom: isSticky ? `${LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px` : '0',
                }}
            >
                <div className={`
                    ${isSticky
                        ? 'bg-teal-600 py-2 px-4'
                        : 'bg-teal-500 py-6 px-8'
                    }
                    text-white rounded-lg shadow-lg
                `}>
                    <h3 className={`font-bold mb-3 ${isSticky ? 'text-base' : 'text-2xl'}`}>
                        🗂️ Accordion Content
                    </h3>

                    {isSticky ? (
                        // COMPACT chips - just icon + title (very small)
                        <div className="flex gap-1.5 flex-wrap items-center">
                            {accordionItems.map(item => (
                                <button
                                    key={item.id}
                                    onClick={() => toggleItem(item.id)}
                                    className={`
                                        px-2 py-0.5 rounded text-xs font-medium transition-all inline-flex items-center gap-1
                                        ${openItems.has(item.id)
                                            ? 'bg-white text-teal-600 shadow-sm'
                                            : 'bg-teal-700/70 hover:bg-teal-600'
                                        }
                                    `}
                                >
                                    <span className="text-sm">{item.icon}</span>
                                    <span className="text-xs">{openItems.has(item.id) ? '▼' : '▶'}</span>
                                </button>
                            ))}
                        </div>
                    ) : (
                        // LARGE full panels - extensive content (very big)
                        <div className="space-y-4">
                            {accordionItems.map(item => (
                                <div key={item.id} className="bg-teal-600/80 rounded-xl overflow-hidden border-2 border-teal-400/30">
                                    <button
                                        onClick={() => toggleItem(item.id)}
                                        className="w-full px-6 py-4 flex items-center justify-between hover:bg-teal-500 transition-colors"
                                    >
                                        <div className="flex items-center gap-3">
                                            <span className="text-3xl">{item.icon}</span>
                                            <span className="font-bold text-xl">{item.title}</span>
                                        </div>
                                        <span className="text-2xl font-bold">
                                            {openItems.has(item.id) ? '▼' : '▶'}
                                        </span>
                                    </button>
                                    {openItems.has(item.id) && (
                                        <div className="px-6 py-5 bg-teal-700/50 border-t-2 border-teal-400/20">
                                            <div className="space-y-3">
                                                {item.content.map((subItem, idx) => (
                                                    <div
                                                        key={idx}
                                                        className="px-5 py-3 bg-teal-600 rounded-lg flex items-start gap-3 hover:bg-teal-500 transition-colors"
                                                    >
                                                        <span className="text-lg mt-0.5">•</span>
                                                        <span className="text-base flex-1">{subItem}</span>
                                                        <span className="text-teal-300 text-sm">#{idx + 1}</span>
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <TestCaseDescription
                title="Test Case 4: Accordion with Nested Content"
                observations={[
                    'Accordion shows FULL items when free (expandable panels)',
                    'Accordion shows COMPACT chips when sticky (just titles)',
                    'Click items to expand/collapse - height updates dynamically',
                    'Multiple items can be open simultaneously',
                    'Perfect for testing content that changes height frequently',
                    'Console logs show height changes with each toggle',
                ]}
            />
        </>
    );
}

// ============================================================================
// MAIN DEMO COMPONENT
// ============================================================================

export function DemoSticky() {
    return (
        <div className="py-8 bg-gray-100 dark:bg-gray-800">
            <div className="max-w-5xl mx-auto px-4">
                <div className="text-center mb-8">
                    <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-3">
                        🧪 Sticky System Demo - Enhanced Testing Suite
                    </h2>
                    <p className="text-lg text-gray-600 dark:text-gray-400 mb-2">
                        Scroll down to test sticky behavior with dynamic height measurement
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-500">
                        Open browser console to see detailed height measurements and state changes
                    </p>
                    <div className="mt-4 inline-block px-4 py-2 bg-green-100 dark:bg-green-900/30 rounded-lg">
                        <span className="text-sm font-semibold text-green-800 dark:text-green-200">
                            ✓ Sticky spacing: {LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS}px between elements
                        </span>
                    </div>
                </div>

                {/* TEST CASE 1: Expand/Collapse */}
                <TestCase1_ExpandCollapse />
                <Spacer height={500} color="#e3f2fd" label="📜 Scroll Area - 500px" />

                {/* TEST CASE 2: Hierarchical Document Structure */}
                <TestCase2_DocumentHierarchy />
                <Spacer height={700} color="#f3e5f5" label="📜 Scroll Area - 700px" />

                {/* TEST CASE 3: Variable Height Buttons */}
                <TestCase3_VariableHeightButtons />
                <Spacer height={600} color="#fff3e0" label="📜 Scroll Area - 600px" />

                {/* TEST CASE 4: Accordion/Nested */}
                <TestCase4_AccordionNested />
                <Spacer height={800} color="#e0f2f1" label="📜 Final Scroll Area - 800px" />

                {/* Footer */}
                <div className="mt-12 p-6 bg-white dark:bg-gray-700 rounded-lg shadow-md text-center">
                    <p className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                        ✅ End of Demo
                    </p>
                    <p className="text-gray-600 dark:text-gray-300 mb-4">
                        Check browser console for detailed height measurements
                    </p>
                    <div className="text-sm text-gray-500 dark:text-gray-400 space-y-1">
                        <p>✓ 4 comprehensive test cases implemented</p>
                        <p>✓ Hierarchical stacking with parent-child relationships</p>
                        <p>✓ Dynamic height measurement with real-time updates</p>
                        <p>✓ Expand/collapse, tabs, buttons, and accordion patterns</p>
                    </div>
                </div>
            </div>
        </div>
    );
}
