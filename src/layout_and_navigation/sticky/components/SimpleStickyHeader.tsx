/**
 * SIMPLE STICKY HEADER EXAMPLE
 * =============================
 * 
 * Demonstrates the simplified sticky API with one-line integration.
 * This component shows how easy it is to create a sticky header that:
 * - Shrinks when you scroll down (no trembling!)
 * - Auto-measures its height
 * - Auto-registers its position
 * - All with just ONE hook call
 */

import { useSticky } from '@/layout_and_navigation/sticky';

export function SimpleStickyHeader() {
    const {
        ref,           // Auto-managed ref - just attach it
        isShrunk,      // Scroll-shrink state (true when scrolled past threshold)
        stickyClasses, // 'sticky' or 'relative'
        stickyStyles,  // { top, zIndex }
    } = useSticky('simple-header', {
        enableShrink: {
            shrinkThreshold: 100,  // Shrink after 100px scroll
            expandThreshold: 50,   // Expand when back to 50px (50px hysteresis = no trembling!)
        },
        autoMeasure: true,          // ✅ Auto-measure height with ResizeObserver
        autoRegisterPosition: true, // ✅ Auto-register position on mount
    });

    return (
        <header
            ref={ref}
            className={`${stickyClasses} transition-all duration-300`}
            style={stickyStyles}
        >
            <div className={`
                ${isShrunk
                    ? 'bg-blue-600 py-2 px-4'    // Compact when shrunk
                    : 'bg-blue-500 py-6 px-8'    // Expanded normally
                }
                text-white shadow-lg
            `}>
                <div className="flex items-center justify-between">
                    <h1 className={`font-bold ${isShrunk ? 'text-lg' : 'text-3xl'}`}>
                        🎯 TileStories
                    </h1>
                    <nav className="flex gap-4">
                        <a href="#" className="hover:underline">Home</a>
                        <a href="#" className="hover:underline">About</a>
                        <a href="#" className="hover:underline">Contact</a>
                    </nav>
                </div>
            </div>
        </header>
    );
}

/**
 * THAT'S IT! 
 * 
 * Compare this to the OLD way:
 * - No manual useRef
 * - No manual useEffect for position
 * - No manual useEffect for height
 * - No separate useScrollShrink hook
 * - No dependency array issues
 * - No trembling/flickering
 * 
 * Just ONE hook call and you're done! 🚀
 */