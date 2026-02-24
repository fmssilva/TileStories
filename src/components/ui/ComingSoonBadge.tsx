/**
 * ComingSoonBadge Component
 * 
 * A decorative badge to indicate the app is under development.
 * Features azulejo-inspired design with subtle animations.
 * Can be used as a ribbon or inline badge.
 */

import { Z_INDEX } from '@/design';

interface ComingSoonBadgeProps {
    /** Variant of the badge */
    variant?: 'ribbon' | 'inline' | 'corner';
    /** Launch timeframe */
    launchText?: string;
    /** Custom className for positioning */
    className?: string;
    /**
     * Z-index for layering
     * @default Z_INDEX.FLOATING (3000) - Badges should float above content but below modals
     */
    zIndex?: number;
}

export function ComingSoonBadge({
    variant = 'corner',
    launchText = 'Coming Summer 2026',
    className = '',
    zIndex = Z_INDEX.FLOATING,
}: ComingSoonBadgeProps) {

    if (variant === 'ribbon') {
        // Diagonal ribbon across top-right corner
        return (
            <div
                className={`fixed top-12 right-0 ${className}`}
                style={{ zIndex }}
            >
                <div className="relative">
                    <div
                        className="bg-gradient-to-br from-azulejo-gold-500 to-azulejo-gold-600 
                            text-white font-bold py-2 px-12 shadow-lg
                            transform rotate-45 translate-x-8 translate-y-6
                            text-sm tracking-wide"
                        style={{
                            transformOrigin: 'center',
                        }}
                    >
                        {launchText}
                    </div>
                </div>
            </div>
        );
    }

    if (variant === 'inline') {
        // Inline badge for use within sections - high contrast version
        return (
            <div className={`inline-flex items-center gap-3 bg-white/95 backdrop-blur-sm 
                px-4 py-2 rounded-full shadow-lg border-2 border-azulejo-gold-400 ${className}`}>
                <span className="relative flex h-3 w-3">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-azulejo-gold-500 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-3 w-3 bg-azulejo-gold-600"></span>
                </span>
                <span className="text-azulejo-blue-900 font-bold text-base uppercase tracking-wide">
                    {launchText}
                </span>
            </div>
        );
    }

    // Default: Corner badge (tilted like an azulejo tile at 45°)
    return (
        <div
            className={`absolute top-4 right-4 sm:top-8 sm:right-8 ${className}`}
            style={{ zIndex }}
        >
            <div
                className="relative group cursor-default"
                style={{
                    transformStyle: 'preserve-3d',
                }}
            >
                {/* Azulejo tile-inspired badge */}
                <div
                    className="bg-gradient-to-br from-azulejo-gold-500 to-azulejo-gold-600 
                        text-white font-bold py-3 px-6 sm:py-4 sm:px-8
                        shadow-2xl transform rotate-[45deg]
                        border-4 border-white
                        transition-all duration-300 ease-out
                        group-hover:scale-110 group-hover:rotate-[48deg]
                        relative overflow-hidden"
                    style={{
                        transformOrigin: 'center',
                    }}
                >
                    {/* Subtle azulejo pattern overlay */}
                    <div
                        className="absolute inset-0 opacity-10"
                        style={{
                            backgroundImage: `repeating-linear-gradient(
                                45deg,
                                transparent,
                                transparent 10px,
                                rgba(255,255,255,0.3) 10px,
                                rgba(255,255,255,0.3) 20px
                            )`,
                        }}
                    />

                    {/* Badge content - Text remains horizontal (no rotation) */}
                    <div className="relative z-10 text-center whitespace-nowrap text-nowrap">
                        <div className="text-xs sm:text-sm font-semibold uppercase tracking-wider mb-1 text-white/90">
                            Coming Soon
                        </div>
                        <div className="text-lg sm:text-xl font-bold">
                            {launchText}
                        </div>
                    </div>

                    {/* Shine effect on hover */}
                    <div
                        className="absolute inset-0 opacity-0 group-hover:opacity-100 
                            transition-opacity duration-500"
                        style={{
                            background: 'linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent)',
                            animation: 'shimmer 2s infinite',
                        }}
                    />
                </div>

                {/* Shadow for depth */}
                <div
                    className="absolute inset-0 bg-black/20 blur-xl transform rotate-[45deg] -z-10"
                    style={{
                        transformOrigin: 'center',
                    }}
                />
            </div>
        </div>
    );
}

// Add shimmer animation to global CSS if needed
// Or use inline animation in Tailwind config
