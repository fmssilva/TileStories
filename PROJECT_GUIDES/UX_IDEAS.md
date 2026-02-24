# UX Innovation Catalog - TileStories
**Comprehensive Guide to Advanced User Experience Features**

This document catalogs all innovative UX design patterns and interaction techniques used throughout the TileStories project. Features are ordered by "WOW factor" (impressiveness and impact on user experience), from highest to lowest.

---

## 🌟 TIER 1: HIGH-IMPACT IMMERSIVE FEATURES

### 1. Parallax Scrolling Background

**Effect:**  
Background images move at a slower speed than foreground content as the user scrolls, creating a 3D depth illusion. Used in hero sections to add cinematic feel and visual sophistication.

**Implementation Option 1: Custom useParallax Hook (RECOMMENDED)**

```tsx
// Custom hook leveraging RequestAnimationFrame for 60fps performance
import { useState, useEffect, CSSProperties } from 'react';

export interface UseParallaxOptions {
    speed?: number;      // 0-1, where 0.5 = half scroll speed
    enabled?: boolean;
}

export function useParallax(options: UseParallaxOptions = {}): CSSProperties {
    const { speed = 0.5, enabled = true } = options;
    const [offsetY, setOffsetY] = useState(0);

    useEffect(() => {
        if (!enabled) return;

        // Respect reduced motion preference
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (prefersReducedMotion) return;

        let rafId: number;
        let lastScrollY = window.scrollY;

        const handleScroll = () => {
            const currentScrollY = window.scrollY;
            if (currentScrollY !== lastScrollY) {
                lastScrollY = currentScrollY;
                rafId = requestAnimationFrame(() => {
                    setOffsetY(currentScrollY * speed);
                });
            }
        };

        handleScroll(); // Initial call
        window.addEventListener('scroll', handleScroll, { passive: true });

        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) cancelAnimationFrame(rafId);
        };
    }, [speed, enabled]);

    return {
        transform: `translateY(${offsetY}px)`,
        willChange: 'transform',  // Browser optimization hint
    };
}

// Usage in component
function HeroSection() {
    const parallaxStyle = useParallax({ speed: 0.5 });
    
    return (
        <div className="relative h-screen overflow-hidden">
            <img 
                src="background.jpg" 
                style={parallaxStyle}
                className="absolute inset-0 w-full h-full object-cover" 
            />
            <div className="relative z-10">Content</div>
        </div>
    );
}
```

**Implementation Option 2: CSS-Only Parallax**

```tsx
// Simpler CSS-based approach (less smooth, but no JS needed)
function HeroSection() {
    return (
        <div className="relative h-screen overflow-hidden">
            <div 
                className="absolute inset-0 bg-cover bg-center bg-fixed"
                style={{ backgroundImage: 'url(background.jpg)' }}
            />
            <div className="relative z-10">Content</div>
        </div>
    );
}

// Add to CSS/Tailwind
// background-attachment: fixed creates parallax effect
// bg-fixed in Tailwind
```

**When to Use:**  
- Hero sections, landing pages
- Large background images where depth enhances storytelling
- Sections with minimal text overlay (avoids motion sickness)
- Desktop-first experiences (disable on mobile for performance)

---

### 2. Scroll-Triggered Animations with Intersection Observer

**Effect:**  
Elements fade in, slide up, or scale into view as they enter the viewport. Creates sense of progressive revelation and keeps users engaged as they scroll.

**Implementation Option 1: useScrollAnimation Hook with getScrollAnimationClasses Utility (RECOMMENDED)**

```tsx
// Hook implementation
import { useEffect, useRef, useState } from 'react';

export interface UseScrollAnimationOptions {
    threshold?: number;      // 0-1, percentage visible before trigger
    delay?: number;          // ms delay before animation
    once?: boolean;          // Trigger only once (true) or on every entry (false)
    rootMargin?: string;     // Trigger earlier/later ("0px 0px -100px 0px")
}

export function useScrollAnimation<T extends HTMLElement = HTMLDivElement>(
    options: UseScrollAnimationOptions = {}
) {
    const { threshold = 0.1, delay = 0, once = true, rootMargin = '0px' } = options;
    const elementRef = useRef<T>(null);
    const [isVisible, setIsVisible] = useState(false);
    const [hasAnimated, setHasAnimated] = useState(false);

    useEffect(() => {
        const element = elementRef.current;
        if (!element) return;

        // Accessibility: Skip animation for users who prefer reduced motion
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (prefersReducedMotion) {
            setIsVisible(true);
            setHasAnimated(true);
            return;
        }

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        if (delay > 0) {
                            setTimeout(() => {
                                setIsVisible(true);
                                if (once) setHasAnimated(true);
                            }, delay);
                        } else {
                            setIsVisible(true);
                            if (once) setHasAnimated(true);
                        }
                    } else if (!once && hasAnimated) {
                        setIsVisible(false);  // Re-hide for re-animation
                    }
                });
            },
            { threshold, rootMargin }
        );

        observer.observe(element);
        return () => observer.disconnect();
    }, [threshold, delay, once, hasAnimated, rootMargin]);

    return { ref: elementRef, isVisible };
}

// Utility function for consistent animation classes
export function getScrollAnimationClasses(
    isVisible: boolean,
    type: 'fade' | 'slide-up' | 'slide-left' | 'slide-right' | 'scale' = 'slide-up'
): string {
    const baseClasses = 'transition-all duration-700 ease-out';

    const animations = {
        'fade': isVisible ? 'opacity-100' : 'opacity-0',
        'slide-up': isVisible 
            ? 'opacity-100 translate-y-0' 
            : 'opacity-0 translate-y-8',
        'slide-left': isVisible 
            ? 'opacity-100 translate-x-0' 
            : 'opacity-0 translate-x-8',
        'slide-right': isVisible 
            ? 'opacity-100 translate-x-0' 
            : 'opacity-0 -translate-x-8',
        'scale': isVisible 
            ? 'opacity-100 scale-100' 
            : 'opacity-0 scale-95',
    };

    return `${baseClasses} ${animations[type]}`;
}

// Usage in component
function Section() {
    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.2,
        once: true
    });

    return (
        <div ref={sectionRef}>
            <h2 className={getScrollAnimationClasses(isVisible, 'slide-up')}>
                Heading appears on scroll
            </h2>
            <p className={getScrollAnimationClasses(isVisible, 'fade')}
               style={{ transitionDelay: '100ms' }}>
                Text appears 100ms after heading
            </p>
        </div>
    );
}
```

**Implementation Option 2: framer-motion Scroll Animations**

```tsx
// Using framer-motion library for declarative animations
import { motion } from 'framer-motion';

function Section() {
    return (
        <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true, amount: 0.2 }}
            transition={{ duration: 0.7, ease: 'easeOut' }}
        >
            <h2>Heading appears on scroll</h2>
        </motion.div>
    );
}
```

**When to Use:**  
- Section headings, card grids, feature lists
- Any content that benefits from progressive disclosure
- Long-form pages where revealing content creates rhythm
- Avoid on above-the-fold content (should be immediately visible)

---

### 3. Animated Counters with Framer-Motion

**Effect:**  
Numbers smoothly count up from 0 to target value when they enter viewport. Creates engagement and draws attention to important metrics like budget, timeline, deliverables.

**Implementation Option 1: framer-motion useMotionValue (RECOMMENDED)**

```tsx
import { motion, useMotionValue, useTransform, animate } from 'framer-motion';
import { useEffect, useRef } from 'react';
import { useScrollAnimation } from '@/utils';

interface AnimatedCounterProps {
    target: number;
    duration?: number;
    prefix?: string;
    suffix?: string;
    separator?: string;  // Thousand separator
}

function AnimatedCounter({ 
    target, 
    duration = 2, 
    prefix = '', 
    suffix = '', 
    separator = ',' 
}: AnimatedCounterProps) {
    const count = useMotionValue(0);
    const rounded = useTransform(count, (latest) => {
        const value = Math.round(latest);
        // Format with thousand separator
        const formatted = value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, separator);
        return `${prefix}${formatted}${suffix}`;
    });
    const hasAnimated = useRef(false);

    // Trigger animation when element enters viewport
    const { ref: scrollRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.5,
        once: true,
    });

    useEffect(() => {
        if (isVisible && !hasAnimated.current) {
            hasAnimated.current = true;
            const controls = animate(count, target, {
                duration,
                ease: 'easeOut',
            });
            return () => controls.stop();
        }
    }, [isVisible, count, target, duration]);

    return (
        <div ref={scrollRef} style={{ display: 'inline' }}>
            <motion.span>{rounded}</motion.span>
        </div>
    );
}

// Usage
function MetricsSection() {
    return (
        <div className="grid grid-cols-3 gap-8">
            <div>
                <AnimatedCounter target={3500} prefix="€" />
                <p>Total Budget</p>
            </div>
            <div>
                <AnimatedCounter target={12} suffix=" Months" />
                <p>Timeline</p>
            </div>
            <div>
                <AnimatedCounter target={4} suffix=" Phases" />
                <p>Development Stages</p>
            </div>
        </div>
    );
}
```

**Implementation Option 2: React Spring useSpring**

```tsx
import { useSpring, animated } from '@react-spring/web';
import { useInView } from 'react-intersection-observer';

function AnimatedCounter({ target, prefix = '', suffix = '' }) {
    const { ref, inView } = useInView({ triggerOnce: true, threshold: 0.5 });
    
    const { number } = useSpring({
        from: { number: 0 },
        to: { number: inView ? target : 0 },
        config: { duration: 2000 },
    });

    return (
        <div ref={ref}>
            <animated.span>
                {number.to((n) => `${prefix}${Math.round(n).toLocaleString()}${suffix}`)}
            </animated.span>
        </div>
    );
}
```

**When to Use:**  
- Key metrics, statistics, pricing
- Overview sections, dashboards
- Budget breakdowns, timeline indicators
- Anywhere you want to emphasize numerical data

---

### 4. Staggered Animations (Sequential Reveal)

**Effect:**  
Multiple items (cards, list items) animate in sequence with slight delays, creating a cascading "wave" effect. More engaging than all items appearing simultaneously.

**Implementation Option 1: Index-Based Delay Pattern (RECOMMENDED)**

```tsx
// Pattern: baseDelay + (index * incrementDelay)
function CardGrid() {
    const { ref: gridRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    const items = [1, 2, 3, 4, 5, 6];
    const BASE_DELAY = 200;      // First item delay
    const INCREMENT_DELAY = 100; // Additional delay per item

    return (
        <div ref={gridRef} className="grid grid-cols-3 gap-6">
            {items.map((item, index) => (
                <div
                    key={item}
                    className={getScrollAnimationClasses(isVisible, 'scale')}
                    style={{ 
                        transitionDelay: `${BASE_DELAY + index * INCREMENT_DELAY}ms` 
                    }}
                >
                    Card {item}
                </div>
            ))}
        </div>
    );
}

// Result: Card 1 appears at 200ms, Card 2 at 300ms, Card 3 at 400ms, etc.
```

**Implementation Option 2: framer-motion staggerChildren**

```tsx
import { motion } from 'framer-motion';

function CardGrid() {
    const containerVariants = {
        hidden: { opacity: 0 },
        visible: {
            opacity: 1,
            transition: {
                staggerChildren: 0.1,  // 100ms delay between children
            }
        }
    };

    const itemVariants = {
        hidden: { opacity: 0, scale: 0.95 },
        visible: { 
            opacity: 1, 
            scale: 1,
            transition: { duration: 0.5 }
        }
    };

    return (
        <motion.div 
            className="grid grid-cols-3 gap-6"
            variants={containerVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, amount: 0.1 }}
        >
            {items.map((item) => (
                <motion.div key={item} variants={itemVariants}>
                    Card {item}
                </motion.div>
            ))}
        </motion.div>
    );
}
```

**When to Use:**  
- Card grids, feature lists, team members
- Any repeating elements (testimonials, logos, stats)
- Timeline items, phase breakdowns
- Navigation menus (with shorter delays)

---

### 5. Button Shine Effect (Swipe Animation)

**Effect:**  
On hover, a bright gradient "shines" across button from left to right, mimicking light reflecting off a surface. Creates premium feel and draws attention to CTAs.

**Implementation Option 1: Pseudo-Element with Transform (RECOMMENDED)**

```tsx
function ShineButton({ children }: { children: React.ReactNode }) {
    return (
        <button className="group relative overflow-hidden px-8 py-4 bg-azulejo-gold-500 text-white rounded-lg font-semibold">
            {/* Shine effect overlay */}
            <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                          transition-transform duration-700 ease-out
                          bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            
            {/* Button content with slight translate on hover */}
            <span className="relative z-10 transition-transform duration-300 group-hover:translate-x-1">
                {children}
            </span>
        </button>
    );
}
```

**Implementation Option 2: Keyframe Animation**

```tsx
// Add to global CSS or component
const buttonStyles = `
@keyframes shine {
    0% { left: -100%; }
    100% { left: 100%; }
}

.btn-shine::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(
        90deg,
        transparent,
        rgba(255, 255, 255, 0.4),
        transparent
    );
}

.btn-shine:hover::before {
    animation: shine 0.7s ease-out;
}
`;

function ShineButton({ children }) {
    return (
        <button className="btn-shine relative overflow-hidden px-8 py-4 bg-gold-500 text-white rounded-lg">
            {children}
        </button>
    );
}
```

**When to Use:**  
- Primary CTAs (contact, download, purchase)
- High-priority actions that need emphasis
- Premium/luxury brand buttons
- Sparingly - loses impact if overused

---

## 🔥 TIER 2: HIGH-VALUE INTERACTION PATTERNS

### 6. Multi-Layer Group Hover Effects

**Effect:**  
When hovering over a container, multiple child elements react differently (image zooms, text appears, overlay darkens). Creates rich, layered interaction that feels polished.

**Implementation Option 1: Tailwind Group Pattern (RECOMMENDED)**

```tsx
function EnhancedCard() {
    return (
        <div className="group relative overflow-hidden rounded-lg shadow-md hover:shadow-xl transition-shadow duration-300 cursor-pointer">
            {/* Background Image Layer - Zoom on hover */}
            <img 
                src="feature.jpg" 
                alt="Feature"
                className="w-full h-64 object-cover 
                         opacity-30 group-hover:opacity-40 
                         group-hover:scale-105 
                         transition-all duration-300"
            />

            {/* Overlay Layer - Darken on hover */}
            <div className="absolute inset-0 
                          bg-azulejo-blue-900/0 group-hover:bg-azulejo-blue-900/20 
                          transition-colors duration-300" />

            {/* Icon Layer - Fade in on hover */}
            <div className="absolute inset-0 flex items-center justify-center">
                <div className="opacity-0 group-hover:opacity-100 
                              transition-opacity duration-300 
                              bg-white rounded-full p-3 shadow-lg">
                    <svg className="w-6 h-6 text-azulejo-blue-600">
                        {/* Icon path */}
                    </svg>
                </div>
            </div>

            {/* Text Content - Slide up on hover */}
            <div className="absolute bottom-0 left-0 right-0 p-6 
                          translate-y-2 group-hover:translate-y-0 
                          transition-transform duration-300">
                <h3 className="text-white text-xl font-bold">Feature Title</h3>
                <p className="text-white/80 text-sm 
                            opacity-0 group-hover:opacity-100 
                            transition-opacity duration-300 delay-100">
                    Additional details appear on hover
                </p>
            </div>
        </div>
    );
}
```

**Implementation Option 2: Framer Motion Orchestration**

```tsx
import { motion } from 'framer-motion';

function EnhancedCard() {
    return (
        <motion.div 
            className="relative overflow-hidden rounded-lg cursor-pointer"
            whileHover="hover"
            initial="rest"
        >
            {/* Image Layer */}
            <motion.img
                src="feature.jpg"
                variants={{
                    rest: { scale: 1, opacity: 0.3 },
                    hover: { scale: 1.05, opacity: 0.4 }
                }}
                transition={{ duration: 0.3 }}
            />

            {/* Overlay Layer */}
            <motion.div 
                className="absolute inset-0 bg-blue-900"
                variants={{
                    rest: { opacity: 0 },
                    hover: { opacity: 0.2 }
                }}
            />

            {/* Icon Layer */}
            <motion.div 
                className="absolute inset-0 flex items-center justify-center"
                variants={{
                    rest: { opacity: 0 },
                    hover: { opacity: 1 }
                }}
            >
                <div className="bg-white rounded-full p-3 shadow-lg">
                    <svg className="w-6 h-6">{/* Icon */}</svg>
                </div>
            </motion.div>
        </motion.div>
    );
}
```

**When to Use:**  
- Feature cards, portfolio items, team members
- Image galleries, product showcases
- Interactive demos, case studies
- Any card-based layout where you want to reveal more info on hover

---

### 7. Backdrop Blur Glass Morphism

**Effect:**  
Semi-transparent elements with blurred backgrounds create modern "frosted glass" effect. Popular in modern UI design for overlays, navigation, badges.

**Implementation Option 1: Tailwind backdrop-blur Utilities (RECOMMENDED)**

```tsx
function GlassCard() {
    return (
        <div className="relative">
            {/* Background with gradient */}
            <div className="absolute inset-0 bg-gradient-to-br from-azulejo-gold-600 to-azulejo-terracotta-500" />

            {/* Glass overlay badge */}
            <div className="relative p-4 bg-white/20 backdrop-blur-sm rounded-lg shadow-lg">
                <h3 className="text-white font-bold">Glass Morphism Badge</h3>
                <p className="text-white/90 text-sm">Content appears to float above background</p>
            </div>
        </div>
    );
}

// Alternative: Full glass card
function GlassPanel() {
    return (
        <div className="bg-white/10 backdrop-blur-md rounded-xl border border-white/20 p-6 shadow-xl">
            <h2 className="text-white text-2xl font-bold">Frosted Glass Panel</h2>
            <p className="text-white/80">Background blurs through the panel</p>
        </div>
    );
}
```

**Implementation Option 2: CSS backdrop-filter**

```css
.glass-card {
    background: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
    -webkit-backdrop-filter: blur(10px); /* Safari support */
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}
```

**When to Use:**  
- Navigation bars, headers over hero images
- Modal overlays, tooltips
- Badges, tags, labels on colorful backgrounds
- Modern, premium brand aesthetics
- **Note**: Check browser support (not supported in IE)

---

### 8. Gradient Transitions on Hover

**Effect:**  
Gradients smoothly animate/intensify on hover, creating dynamic color shifts. More interesting than solid color changes.

**Implementation Option 1: Pseudo-Element Opacity Fade (RECOMMENDED)**

```tsx
function GradientCard() {
    return (
        <div className="group relative p-6 rounded-xl border-2 border-gray-200 
                      hover:border-azulejo-gold-500 transition-all duration-500
                      bg-gradient-to-br from-white via-white to-azulejo-ivory-50
                      overflow-hidden">
            
            {/* Hover gradient overlay - fades in */}
            <div className="absolute inset-0 
                          bg-gradient-to-br from-azulejo-gold-100/0 to-azulejo-gold-100/20 
                          opacity-0 group-hover:opacity-100 
                          transition-opacity duration-500 
                          pointer-events-none" />
            
            {/* Content */}
            <div className="relative z-10">
                <h3 className="text-xl font-bold">Card Title</h3>
                <p className="text-gray-600">Card description</p>
            </div>
        </div>
    );
}
```

**Implementation Option 2: Multiple Background Images**

```tsx
function GradientButton() {
    return (
        <button 
            className="relative px-6 py-3 rounded-lg text-white font-semibold
                     overflow-hidden transition-all duration-300"
            style={{
                background: `
                    linear-gradient(135deg, #3C5E95 0%, #5081B6 100%),
                    linear-gradient(135deg, #D4AF37 0%, #C1440E 100%)
                `,
                backgroundSize: '100% 100%, 100% 100%',
                backgroundPosition: '0 0, 0 0',
            }}
            onMouseEnter={(e) => {
                e.currentTarget.style.backgroundSize = '0 100%, 100% 100%';
            }}
            onMouseLeave={(e) => {
                e.currentTarget.style.backgroundSize = '100% 100%, 100% 100%';
            }}
        >
            Hover Me
        </button>
    );
}
```

**When to Use:**  
- Cards with hover states
- Buttons, CTAs that need visual interest
- Section backgrounds on scroll/hover
- Brand-aligned color transitions

---

### 9. Smart Back-to-Top with Scroll Progress

**Effect:**  
Back-to-top button with circular progress ring showing how far user has scrolled. Smooth scroll animation with easing. Auto-hides when at top.

**Implementation Option 1: SVG Circle with Stroke-Dashoffset (RECOMMENDED)**

```tsx
import { useState, useEffect, useCallback } from 'react';

function BackToTop() {
    const [scrollProgress, setScrollProgress] = useState(0);
    const [isScrolling, setIsScrolling] = useState(false);

    // Calculate scroll progress
    const updateScrollProgress = useCallback(() => {
        const scrollY = window.scrollY;
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        const progress = docHeight > 0 ? (scrollY / docHeight) * 100 : 0;
        setScrollProgress(progress);
    }, []);

    // Smooth scroll to top
    const scrollToTop = useCallback(() => {
        const startY = window.scrollY;
        const startTime = performance.now();
        const duration = 800;

        setIsScrolling(true);

        const animateScroll = (currentTime: number) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-in-out-cubic)
            const easeInOutCubic = (t: number): number =>
                t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

            const easedProgress = easeInOutCubic(progress);
            const currentY = startY * (1 - easedProgress);

            window.scrollTo(0, currentY);

            if (progress < 1) {
                requestAnimationFrame(animateScroll);
            } else {
                setIsScrolling(false);
            }
        };

        requestAnimationFrame(animateScroll);
    }, []);

    useEffect(() => {
        const handleScroll = () => {
            requestAnimationFrame(updateScrollProgress);
        };
        window.addEventListener('scroll', handleScroll, { passive: true });
        updateScrollProgress();
        return () => window.removeEventListener('scroll', handleScroll);
    }, [updateScrollProgress]);

    const circumference = 2 * Math.PI * 18; // radius = 18
    const strokeDashoffset = circumference - (scrollProgress / 100) * circumference;

    return (
        <button
            onClick={scrollToTop}
            disabled={isScrolling}
            className="fixed bottom-6 right-6 z-50 
                     flex items-center justify-center w-12 h-12 
                     rounded-full backdrop-blur-md 
                     bg-white/80 dark:bg-gray-800/80 
                     border border-gray-200 dark:border-gray-700
                     shadow-lg hover:scale-110 active:scale-95 
                     transition-all duration-300"
            aria-label="Scroll to top"
        >
            {/* Progress Ring */}
            <svg className="absolute inset-0 w-full h-full -rotate-90">
                <circle
                    cx="24"
                    cy="24"
                    r="18"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2"
                    className="text-azulejo-blue-500"
                    style={{
                        strokeDasharray: circumference,
                        strokeDashoffset,
                        transition: 'stroke-dashoffset 0.1s ease-out'
                    }}
                />
            </svg>

            {/* Arrow Icon */}
            <svg 
                className="w-5 h-5 transition-transform group-hover:-translate-y-0.5" 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
            >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
            </svg>
        </button>
    );
}
```

**Implementation Option 2: Progress Bar Variant**

```tsx
function BackToTopBar() {
    const [scrollProgress, setScrollProgress] = useState(0);

    useEffect(() => {
        const handleScroll = () => {
            const progress = (window.scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100;
            setScrollProgress(progress);
        };
        window.addEventListener('scroll', handleScroll, { passive: true });
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    return (
        <>
            {/* Fixed progress bar at top */}
            <div className="fixed top-0 left-0 right-0 h-1 bg-gray-200 z-50">
                <div 
                    className="h-full bg-azulejo-blue-500 transition-all duration-100"
                    style={{ width: `${scrollProgress}%` }}
                />
            </div>

            {/* Back to top button */}
            {scrollProgress > 10 && (
                <button
                    onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                    className="fixed bottom-6 right-6 p-3 rounded-full bg-azulejo-blue-500 text-white shadow-lg hover:scale-110"
                >
                    ↑
                </button>
            )}
        </>
    );
}
```

**When to Use:**  
- Long-form pages (blog posts, documentation, landing pages)
- Sites with lots of scrolling content
- When you want to show progress through page
- **Positioning**: Fixed bottom-right (z-index behind header to auto-reveal)

---

### 10. Native Zoom Auto-Snap Enhancement

**Effect:**  
Detects when user zooms close to 100% (ideal viewing level) and automatically snaps them to exactly 100%. Helps users find optimal zoom level without manual adjustment.

**Implementation Option 1: Visual Viewport API (RECOMMENDED)**

```typescript
// Auto-snap zoom enhancer
let isAutoSnapping = false;
const IDEAL_ZOOM = 1.0;
const SNAP_THRESHOLD = 0.15;    // 15% tolerance (85%-115% triggers snap)
const SNAP_DEBOUNCE = 300;      // Wait 300ms after zoom stops
const TRANSITION_DURATION = 200;

export function initializeNativeZoom() {
    let debounceTimer: number | null = null;

    const handleZoomChange = () => {
        if (isAutoSnapping) return;

        if (debounceTimer) clearTimeout(debounceTimer);

        debounceTimer = window.setTimeout(() => {
            checkAndSnapToIdealZoom();
        }, SNAP_DEBOUNCE);
    };

    // Visual Viewport API - most reliable zoom detection
    if ('visualViewport' in window && window.visualViewport) {
        window.visualViewport.addEventListener('resize', handleZoomChange);
        window.visualViewport.addEventListener('scroll', handleZoomChange);
    }

    // Fallback for older browsers
    window.addEventListener('resize', handleZoomChange);

    return () => {
        if (debounceTimer) clearTimeout(debounceTimer);
        if (window.visualViewport) {
            window.visualViewport.removeEventListener('resize', handleZoomChange);
            window.visualViewport.removeEventListener('scroll', handleZoomChange);
        }
        window.removeEventListener('resize', handleZoomChange);
    };
}

function getCurrentZoom(): number {
    if ('visualViewport' in window && window.visualViewport) {
        return window.visualViewport.scale || 1.0;
    }
    return window.screen.width / window.innerWidth;
}

function checkAndSnapToIdealZoom() {
    const currentZoom = getCurrentZoom();
    const distanceFromIdeal = Math.abs(currentZoom - IDEAL_ZOOM);

    // Snap if close enough (but not already at ideal)
    if (distanceFromIdeal <= SNAP_THRESHOLD && distanceFromIdeal > 0.02) {
        snapToIdealZoom();
    }
}

function snapToIdealZoom() {
    if (isAutoSnapping) return;
    isAutoSnapping = true;

    // Smooth transition
    document.documentElement.style.transition = `transform ${TRANSITION_DURATION}ms ease-out`;

    // Reset zoom via viewport meta tag
    const viewport = document.querySelector('meta[name="viewport"]');
    if (viewport) {
        const content = viewport.getAttribute('content') || '';
        const newContent = content.replace(/initial-scale=[\d.]+/, 'initial-scale=1.0');
        viewport.setAttribute('content', newContent);

        setTimeout(() => {
            viewport.setAttribute('content', content);
            document.documentElement.style.transition = '';
            isAutoSnapping = false;
        }, TRANSITION_DURATION + 50);
    }
}

// Initialize in main.tsx or App.tsx
// const cleanup = initializeNativeZoom();
// return cleanup; // in useEffect
```

**Implementation Option 2: Custom Zoom Controls**

```tsx
// Provide explicit zoom controls instead of auto-snap
function ZoomControls() {
    const [zoom, setZoom] = useState(1);

    const handleZoom = (newZoom: number) => {
        setZoom(newZoom);
        document.body.style.transform = `scale(${newZoom})`;
        document.body.style.transformOrigin = 'top left';
    };

    return (
        <div className="fixed bottom-20 right-6 flex flex-col gap-2 bg-white rounded-lg shadow-lg p-2">
            <button onClick={() => handleZoom(1.25)} className="px-3 py-1 hover:bg-gray-100">125%</button>
            <button onClick={() => handleZoom(1.0)} className="px-3 py-1 hover:bg-gray-100 font-bold">100%</button>
            <button onClick={() => handleZoom(0.9)} className="px-3 py-1 hover:bg-gray-100">90%</button>
        </div>
    );
}
```

**When to Use:**  
- Accessibility enhancement (helps users with vision impairments)
- Apps with precise visual content (dashboards, design tools)
- **Use sparingly**: Can be surprising if user intentionally zoomed
- **Alternative**: Provide explicit zoom controls instead

---

## ⚡ TIER 3: POLISH & MICRO-INTERACTIONS

### 11. Icon Scale on Hover

**Effect:**  
Icons slightly enlarge when parent element is hovered, creating subtle feedback that element is interactive.

**Implementation Option 1: Group Hover Pattern (RECOMMENDED)**

```tsx
function IconCard() {
    return (
        <div className="group p-6 border rounded-lg hover:shadow-lg transition-shadow">
            {/* Icon scales on parent hover */}
            <div className="text-4xl mb-4 
                          transition-transform duration-300 
                          group-hover:scale-110"
                 aria-hidden="true">
                🎯
            </div>
            <h3 className="text-xl font-bold">Feature Title</h3>
            <p className="text-gray-600">Description text</p>
        </div>
    );
}
```

**Implementation Option 2: Individual Hover**

```tsx
function IconButton() {
    return (
        <button className="p-3 rounded-full hover:bg-gray-100 transition-colors">
            <svg 
                className="w-6 h-6 transition-transform duration-200 hover:scale-125"
                fill="none" 
                stroke="currentColor"
            >
                {/* Icon path */}
            </svg>
        </button>
    );
}
```

**When to Use:**  
- Card headers with emoji/icon
- Icon buttons, navigation items
- Feature lists, benefit cards
- **Timing**: duration-300 for cards, duration-200 for buttons

---

### 12. Shadow Elevation Transitions

**Effect:**  
Elements "lift" off the page on hover by increasing shadow depth. Creates 3D effect and signals interactivity.

**Implementation Option 1: Tailwind Shadow Utilities (RECOMMENDED)**

```tsx
function ElevatingCard() {
    return (
        <div className="p-6 rounded-lg border 
                      shadow-md hover:shadow-2xl 
                      transition-shadow duration-500
                      cursor-pointer">
            <h3 className="text-xl font-bold">Hover to Elevate</h3>
            <p className="text-gray-600">Card lifts with deeper shadow</p>
        </div>
    );
}

// Progressive shadow levels
function MultiLevelCards() {
    return (
        <>
            <div className="shadow-sm hover:shadow-md">Subtle lift</div>
            <div className="shadow-md hover:shadow-lg">Medium lift</div>
            <div className="shadow-lg hover:shadow-xl">High lift</div>
            <div className="shadow-xl hover:shadow-2xl">Maximum lift</div>
        </>
    );
}
```

**Implementation Option 2: Custom Shadow Values**

```tsx
// Add to tailwind.config.js
module.exports = {
    theme: {
        extend: {
            boxShadow: {
                'soft': '0 2px 15px rgba(0, 0, 0, 0.08)',
                'soft-lg': '0 10px 40px rgba(0, 0, 0, 0.12)',
                'colored': '0 10px 25px rgba(60, 94, 149, 0.15)', // Azulejo blue shadow
            }
        }
    }
}

function ColoredShadowCard() {
    return (
        <div className="p-6 rounded-lg 
                      shadow-soft hover:shadow-colored 
                      transition-shadow duration-500">
            Colored shadow on hover
        </div>
    );
}
```

**When to Use:**  
- Cards, panels, product tiles
- Navigation items, buttons
- **Pair with**: Slight scale transform for enhanced 3D effect
- **Duration**: 300-500ms for smooth, noticeable transition

---

### 13. Translate Transforms (Lift/Slide)

**Effect:**  
Elements move slightly up, down, or sideways on hover. Common for cards that "lift" (-translate-y) or CTAs that "advance" (translate-x).

**Implementation Option 1: Combine with Shadow (RECOMMENDED)**

```tsx
function LiftingCard() {
    return (
        <div className="p-6 rounded-lg border 
                      shadow-md hover:shadow-2xl 
                      hover:-translate-y-2
                      transition-all duration-500">
            <h3>Card lifts up on hover</h3>
        </div>
    );
}

function AdvancingCTA() {
    return (
        <button className="group px-6 py-3 bg-azulejo-blue-500 text-white rounded-lg">
            <span className="inline-flex items-center gap-2 
                           transition-transform duration-300 
                           group-hover:translate-x-1">
                Learn More
                <svg className="w-4 h-4">→</svg>
            </span>
        </button>
    );
}
```

**Implementation Option 2: Directional Slide**

```tsx
function SlideRevealCard() {
    return (
        <div className="group relative overflow-hidden p-6 bg-gray-100 rounded-lg">
            {/* Content slides up */}
            <div className="translate-y-0 group-hover:-translate-y-4 transition-transform duration-300">
                <h3 className="text-xl font-bold">Slide Up</h3>
            </div>

            {/* Hidden content slides in from bottom */}
            <div className="absolute bottom-0 left-0 right-0 p-6 
                          translate-y-full group-hover:translate-y-0 
                          transition-transform duration-300 
                          bg-azulejo-blue-500 text-white">
                <p>Additional info revealed on hover</p>
            </div>
        </div>
    );
}
```

**When to Use:**  
- Cards: -translate-y-1 or -translate-y-2 (lift)
- CTAs/Buttons: translate-x-1 (advance forward)
- Navigation items: -translate-y-0.5 (subtle lift)
- **Combine with**: Shadow changes for enhanced 3D effect

---

### 14. Table Row Gradient Hover

**Effect:**  
Table rows highlight with subtle gradient sweep on hover instead of solid color. More elegant than flat background change.

**Implementation Option 1: Gradient Overlay (RECOMMENDED)**

```tsx
function EnhancedTable() {
    const rows = [
        { phase: 'Phase 1', cost: '€800', duration: '3 months' },
        { phase: 'Phase 2', cost: '€1,000', duration: '3 months' },
        { phase: 'Phase 3', cost: '€900', duration: '3 months' },
        { phase: 'Phase 4', cost: '€800', duration: '3 months' },
    ];

    return (
        <table className="w-full">
            <thead className="bg-gradient-to-r from-azulejo-ivory-100 via-azulejo-ivory-50 to-azulejo-ivory-100">
                <tr>
                    <th className="p-4 text-left">Phase</th>
                    <th className="p-4 text-left">Cost</th>
                    <th className="p-4 text-left">Duration</th>
                </tr>
            </thead>
            <tbody>
                {rows.map((row, index) => (
                    <tr 
                        key={index}
                        className="border-b border-gray-200
                                 hover:bg-gradient-to-r 
                                 hover:from-azulejo-ivory-50 
                                 hover:via-transparent 
                                 hover:to-azulejo-ivory-50
                                 hover:scale-[1.01]
                                 transition-all duration-300"
                    >
                        <td className="p-4">{row.phase}</td>
                        <td className="p-4 font-semibold">{row.cost}</td>
                        <td className="p-4">{row.duration}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}
```

**Implementation Option 2: Phase-Colored Hover**

```tsx
// Row highlights with phase-specific color
function PhaseColoredTable() {
    const getPhaseColor = (index: number) => {
        const colors = ['#3C5E95', '#5081B6', '#C1440E', '#D4AF37'];
        return colors[index % 4];
    };

    return (
        <tbody>
            {rows.map((row, index) => (
                <tr 
                    key={index}
                    className="group border-b transition-all duration-300"
                    style={{
                        '--hover-color': getPhaseColor(index)
                    } as React.CSSProperties}
                    onMouseEnter={(e) => {
                        e.currentTarget.style.background = `linear-gradient(90deg, ${getPhaseColor(index)}10, transparent, ${getPhaseColor(index)}10)`;
                    }}
                    onMouseLeave={(e) => {
                        e.currentTarget.style.background = '';
                    }}
                >
                    <td className="p-4">{row.phase}</td>
                    <td className="p-4">{row.cost}</td>
                </tr>
            ))}
        </tbody>
    );
}
```

**When to Use:**  
- Financial tables, pricing breakdowns
- Phase/timeline tables
- Data tables where rows need differentiation
- **Subtlety**: Use low opacity (10-20%) to avoid overwhelming

---

### 15. Opacity Transitions (Fade In/Out)

**Effect:**  
Elements smoothly appear or disappear by changing opacity. One of the most fundamental and versatile transitions.

**Implementation Option 1: Tailwind Opacity Utilities (RECOMMENDED)**

```tsx
function FadeElements() {
    return (
        <div className="group">
            {/* Badge fades in on parent hover */}
            <span className="opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                New
            </span>

            {/* Image dims on hover */}
            <img 
                src="photo.jpg" 
                className="opacity-100 group-hover:opacity-70 transition-opacity duration-300"
            />

            {/* Text appears with delay */}
            <p className="opacity-0 group-hover:opacity-100 transition-opacity duration-300 delay-200">
                Details appear after 200ms
            </p>
        </div>
    );
}
```

**Implementation Option 2: Conditional Rendering with Transition**

```tsx
function ConditionalFade() {
    const [isVisible, setIsVisible] = useState(false);

    return (
        <>
            <button onClick={() => setIsVisible(!isVisible)}>
                Toggle
            </button>

            {/* Fade in/out based on state */}
            <div 
                className={`transition-opacity duration-500 ${
                    isVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'
                }`}
            >
                Content fades in/out
            </div>
        </>
    );
}
```

**When to Use:**  
- Overlays, tooltips, badges
- Hover reveals, hidden content
- Loading states, skeleton screens
- **Pair with**: translate transforms for slide-fade effects
- **Duration**: 200-300ms for snappy feel, 500ms+ for dramatic reveals

---

## 🎨 TIER 4: FOUNDATIONAL DESIGN PATTERNS

### 16. Color Transitions

**Effect:**  
Background or text colors smoothly shift on hover/focus. More pleasant than instant color changes.

**Implementation Option 1: Tailwind transition-colors (RECOMMENDED)**

```tsx
function ColorTransitions() {
    return (
        <>
            {/* Button color transition */}
            <button className="px-6 py-3 
                             bg-azulejo-blue-500 hover:bg-azulejo-blue-600 
                             text-white 
                             transition-colors duration-300 
                             rounded-lg">
                Hover Me
            </button>

            {/* Text color transition */}
            <a href="#" className="text-gray-600 hover:text-azulejo-blue-500 
                                 transition-colors duration-200">
                Link hovers to brand color
            </a>

            {/* Border color transition */}
            <div className="border-2 border-gray-200 
                          hover:border-azulejo-gold-500 
                          transition-colors duration-500 
                          p-6 rounded-lg">
                Border changes on hover
            </div>
        </>
    );
}
```

**Implementation Option 2: Custom Easing**

```tsx
// Add to tailwind.config.js for custom timing
module.exports = {
    theme: {
        extend: {
            transitionTimingFunction: {
                'smooth': 'cubic-bezier(0.4, 0, 0.2, 1)',
                'bounce-in': 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
            }
        }
    }
}

function CustomEasingColors() {
    return (
        <button className="px-6 py-3 bg-blue-500 hover:bg-blue-600 
                         transition-colors duration-300 ease-bounce-in">
            Bouncy color transition
        </button>
    );
}
```

**When to Use:**  
- All interactive elements (buttons, links, inputs)
- Navigation items, menu highlights
- Status indicators (success/error states)
- **Duration**: 200ms for fast interactions (links), 300ms for buttons, 500ms for large areas

---

### 17. Border Color & Width Transitions

**Effect:**  
Borders change color or width on interaction. Useful for focus states, active tabs, selected cards.

**Implementation Option 1: Phase-Colored Left Border (RECOMMENDED)**

```tsx
function PhaseCard({ phase }: { phase: number }) {
    const getPhaseColor = (p: number) => {
        const colors = ['#3C5E95', '#5081B6', '#C1440E', '#D4AF37'];
        return colors[p - 1];
    };

    return (
        <div 
            className="p-6 rounded-xl border-2 border-gray-200 
                     transition-all duration-300"
            style={{
                borderLeftWidth: '1.5px',
                borderLeftColor: getPhaseColor(phase)
            }}
        >
            <h3>Phase {phase}</h3>
        </div>
    );
}
```

**Implementation Option 2: Animated Border Expand**

```tsx
function ExpandingBorderCard() {
    return (
        <div className="group relative p-6 border-2 border-gray-200 
                      hover:border-azulejo-gold-500 
                      transition-colors duration-300 
                      rounded-lg">
            
            {/* Expanding bottom border on hover */}
            <div className="absolute bottom-0 left-0 h-1 
                          w-0 group-hover:w-full 
                          bg-azulejo-gold-500 
                          transition-all duration-500" />
            
            <h3>Content</h3>
        </div>
    );
}
```

**When to Use:**  
- Active tab indicators
- Card selections, form focus states
- Phase/category visual coding
- **Subtlety**: 1-2px width changes are noticeable but not jarring

---

### 18. Scale Transforms (Zoom In/Out)

**Effect:**  
Elements enlarge or shrink on hover. Common for images, buttons, icons to signal interactivity.

**Implementation Option 1: Image Scale on Hover (RECOMMENDED)**

```tsx
function ScalableImage() {
    return (
        <div className="overflow-hidden rounded-lg">
            <img 
                src="photo.jpg" 
                alt="Feature"
                className="w-full h-auto 
                         transition-transform duration-500 
                         hover:scale-105"
            />
        </div>
    );
}

// Card with image zoom inside
function CardWithImageZoom() {
    return (
        <div className="group rounded-lg overflow-hidden shadow-md cursor-pointer">
            <div className="overflow-hidden">
                <img 
                    src="feature.jpg" 
                    className="w-full h-64 object-cover 
                             transition-transform duration-500 
                             group-hover:scale-105"
                />
            </div>
            <div className="p-6">
                <h3>Card Title</h3>
            </div>
        </div>
    );
}
```

**Implementation Option 2: Button Press Effect**

```tsx
function PressableButton() {
    return (
        <button className="px-6 py-3 bg-azulejo-blue-500 text-white rounded-lg
                         transition-transform duration-150
                         hover:scale-105 
                         active:scale-95">
            Press Me (scales down on click)
        </button>
    );
}
```

**When to Use:**  
- Images in cards/galleries: scale-105 (5% larger)
- Buttons on hover: scale-105, on active: scale-95
- Icons: scale-110 (10% larger)
- **Important**: Wrap in overflow-hidden to prevent layout shift

---

### 19. Drop Shadow on Text

**Effect:**  
Text has subtle shadow for depth and readability, especially over images/gradients. Makes text "pop" from background.

**Implementation Option 1: Tailwind drop-shadow Utility (RECOMMENDED)**

```tsx
function HeroWithShadowText() {
    return (
        <div className="relative h-screen">
            {/* Background image */}
            <img src="hero-bg.jpg" className="absolute inset-0 w-full h-full object-cover" />

            {/* Text with drop shadow for readability */}
            <div className="relative z-10 flex items-center justify-center h-full">
                <h1 className="text-6xl font-bold text-white drop-shadow-lg">
                    Readable over any background
                </h1>
                <p className="text-2xl text-white drop-shadow-md">
                    Subtitle with medium shadow
                </p>
            </div>
        </div>
    );
}
```

**Implementation Option 2: Custom Text Shadows**

```tsx
// Add to tailwind.config.js
module.exports = {
    theme: {
        extend: {
            textShadow: {
                'sm': '0 1px 2px rgba(0, 0, 0, 0.5)',
                'md': '0 2px 4px rgba(0, 0, 0, 0.5)',
                'lg': '0 4px 8px rgba(0, 0, 0, 0.6)',
                'colored': '2px 2px 4px rgba(60, 94, 149, 0.3)',
            }
        }
    }
}

function CustomShadowText() {
    return (
        <h1 className="text-shadow-colored text-4xl font-bold">
            Text with branded color shadow
        </h1>
    );
}
```

**When to Use:**  
- Text over images, gradients, videos
- Hero headings, banner text
- **Color**: Black shadows for light text, white shadows for dark text
- **Intensity**: drop-shadow-md for headings, drop-shadow-sm for body

---

### 20. Responsive Grid Layouts with Hover States

**Effect:**  
Grid items respond to hover with combined effects (shadow, scale, border). Creates cohesive, polished grid experience.

**Implementation Option 1: Feature Card Grid (RECOMMENDED)**

```tsx
function FeatureGrid() {
    const features = [
        { icon: '🏛️', title: '150+ Buildings', description: 'Detailed historical data' },
        { icon: '⏳', title: '4 Epochs', description: 'Time travel through history' },
        { icon: '⚡', title: 'Earthquake Sim', description: 'Experience 1755 disaster' },
        { icon: '🗺️', title: 'Interactive Map', description: 'Easy navigation' },
        { icon: '📚', title: 'Expert Content', description: 'Museum-curated info' },
        { icon: '🌍', title: 'Multilingual', description: 'PT/EN support' },
    ];

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => (
                <div 
                    key={index}
                    className="group p-6 rounded-xl border-2 border-gray-200 
                             hover:border-azulejo-blue-500 
                             hover:shadow-xl 
                             hover:-translate-y-1
                             transition-all duration-300 
                             cursor-pointer"
                >
                    <div className="text-4xl mb-4 
                                  transition-transform duration-300 
                                  group-hover:scale-110">
                        {feature.icon}
                    </div>
                    <h3 className="text-xl font-bold mb-2 
                                 text-gray-900 
                                 group-hover:text-azulejo-blue-600 
                                 transition-colors">
                        {feature.title}
                    </h3>
                    <p className="text-gray-600">{feature.description}</p>
                </div>
            ))}
        </div>
    );
}
```

**Implementation Option 2: Staggered Grid with Scroll Animations**

```tsx
function AnimatedGrid() {
    const { ref: gridRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    return (
        <div ref={gridRef} className="grid grid-cols-3 gap-6">
            {features.map((feature, index) => (
                <div
                    key={index}
                    className={`p-6 rounded-xl border-2 
                              hover:shadow-xl hover:-translate-y-1 
                              transition-all duration-300
                              ${getScrollAnimationClasses(isVisible, 'scale')}`}
                    style={{ transitionDelay: `${200 + index * 100}ms` }}
                >
                    {feature.content}
                </div>
            ))}
        </div>
    );
}
```

**When to Use:**  
- Feature showcases, service grids, product catalogs
- Team members, testimonials, case studies
- **Grid**: 1 col mobile, 2 cols tablet, 3-4 cols desktop
- **Gap**: gap-6 (24px) or gap-8 (32px) for breathing room

---

## 🔧 IMPLEMENTATION BEST PRACTICES

### Accessibility Considerations

```tsx
// ALWAYS respect prefers-reduced-motion
const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

if (prefersReducedMotion) {
    // Skip animations, show final state immediately
    setIsVisible(true);
    return;
}

// Add to global CSS
@media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
    }
}
```

### Performance Optimization

```tsx
// Use CSS transforms (GPU-accelerated) instead of position/margin
// ✅ GOOD
transform: translateY(-8px);

// ❌ AVOID
margin-top: -8px;

// Add willChange for complex animations
<div style={{ willChange: 'transform' }}>
    {/* Animated content */}
</div>

// Use requestAnimationFrame for scroll listeners
let rafId: number;
const handleScroll = () => {
    rafId = requestAnimationFrame(() => {
        // Update state
    });
};
window.addEventListener('scroll', handleScroll, { passive: true });

// Cleanup
return () => {
    if (rafId) cancelAnimationFrame(rafId);
    window.removeEventListener('scroll', handleScroll);
};
```

### Animation Timing Guide

```tsx
// Ultra-fast (instant feedback)
duration-100    // 100ms - Icon hover, active states

// Fast (snappy)
duration-200    // 200ms - Link color, text opacity
duration-300    // 300ms - Button hover, card shadow

// Medium (smooth)
duration-500    // 500ms - Image scale, gradient transitions
duration-700    // 700ms - Scroll animations, slide-up effects

// Slow (dramatic)
duration-1000   // 1000ms - Page transitions, major state changes
```

### Combining Effects (The "Full Stack")

```tsx
// Example: Premium card with ALL effects combined
function UltimateCard() {
    const { ref, isVisible } = useScrollAnimation({ threshold: 0.2, once: true });

    return (
        <div 
            ref={ref}
            className={`group relative p-8 rounded-xl 
                      border-2 border-gray-200 
                      shadow-md hover:shadow-2xl 
                      hover:border-azulejo-gold-500 
                      hover:-translate-y-2
                      transition-all duration-500
                      bg-gradient-to-br from-white to-azulejo-ivory-50
                      overflow-hidden cursor-pointer
                      ${getScrollAnimationClasses(isVisible, 'scale')}`}
            style={{ transitionDelay: '200ms' }}
        >
            {/* Hover gradient overlay */}
            <div className="absolute inset-0 
                          bg-gradient-to-br from-azulejo-gold-100/0 to-azulejo-gold-100/20 
                          opacity-0 group-hover:opacity-100 
                          transition-opacity duration-500 
                          pointer-events-none" />

            {/* Icon with scale */}
            <div className="text-5xl mb-4 
                          transition-transform duration-300 
                          group-hover:scale-110">
                🎯
            </div>

            {/* Content */}
            <div className="relative z-10">
                <h3 className="text-2xl font-bold mb-2 
                             text-gray-900 group-hover:text-azulejo-blue-600 
                             transition-colors duration-300">
                    Premium Feature
                </h3>
                <p className="text-gray-600">
                    Scroll animation + hover shadow + gradient + icon scale + border color
                </p>
            </div>

            {/* Hidden detail revealed on hover */}
            <div className="opacity-0 group-hover:opacity-100 
                          transition-opacity duration-300 delay-100 
                          mt-4 pt-4 border-t border-azulejo-gold-300">
                <p className="text-sm text-gray-700">Extra details appear on hover</p>
            </div>
        </div>
    );
}
```

---

## 📚 RECOMMENDED LIBRARIES

### Core Animation Libraries

**framer-motion** (Recommended)
```bash
npm install framer-motion
```
- Best for: Declarative animations, orchestration, gestures
- Used for: Animated counters, complex sequences
- Docs: https://www.framer.com/motion/

**react-spring**
```bash
npm install @react-spring/web
```
- Best for: Physics-based animations, natural motion
- Used for: Spring animations, drag interactions
- Docs: https://www.react-spring.dev/

### Utility Libraries

**Intersection Observer** (Built-in API)
- No installation needed
- Used for: Scroll-triggered animations
- Polyfill: https://github.com/w3c/IntersectionObserver

**clsx / tailwind-merge**
```bash
npm install clsx tailwind-merge
```
- Best for: Conditional className logic
- Used for: Dynamic class combinations

---

## 🎯 QUICK REFERENCE TABLE

| Feature | Tier | Complexity | Performance Impact | Use Case |
|---------|------|------------|-------------------|----------|
| Parallax Scrolling | 1 | Medium | Medium | Hero sections |
| Scroll Animations | 1 | Medium | Low | Section reveals |
| Animated Counters | 1 | Medium | Low | Metrics, stats |
| Staggered Animations | 1 | Low | Low | Card grids |
| Button Shine | 1 | Low | Low | Primary CTAs |
| Group Hover | 2 | Low | Low | Cards, images |
| Backdrop Blur | 2 | Medium | Medium | Overlays, nav |
| Gradient Transitions | 2 | Low | Low | Cards, backgrounds |
| Back-to-Top Progress | 2 | Medium | Low | Long pages |
| Native Zoom Snap | 2 | High | Low | Accessibility |
| Icon Scale | 3 | Very Low | Very Low | All cards |
| Shadow Elevation | 3 | Very Low | Very Low | Interactive elements |
| Translate Lift | 3 | Very Low | Very Low | Cards, buttons |
| Table Gradient Hover | 3 | Low | Low | Data tables |
| Opacity Fade | 3 | Very Low | Very Low | Universal |
| Color Transitions | 4 | Very Low | Very Low | All interactive |
| Border Transitions | 4 | Very Low | Very Low | Focus states |
| Scale Transforms | 4 | Very Low | Very Low | Images, buttons |
| Text Drop Shadow | 4 | Very Low | Very Low | Text over images |
| Grid Hover States | 4 | Low | Low | Feature grids |

---

## 📝 NOTES

**Created:** Based on comprehensive TileStories codebase analysis  
**Last Updated:** Current implementation as of latest build  
**Maintained By:** Development team  
**Related Docs:** BUILD_PLAN.md, WORK_PLAN_PAGE_GUIDE.md, GUIDE_COLORS.md, GUIDE_SIZE.md

**Future Enhancements to Consider:**
- Page transition animations (route changes)
- Skeleton loading states
- Scroll-snap sections
- Drag-to-reorder interactions
- 3D card flip effects
- Lottie animations for complex illustrations
- Video background with parallax
- Cursor trail effects (desktop only)
- Audio feedback on interactions
- Haptic feedback (mobile PWA)

---

**END OF DOCUMENT**
