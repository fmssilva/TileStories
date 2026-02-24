/**
 * SPACER COMPONENT
 * ================
 * 
 * Simple component that enforces specific width/height for gaps between elements.
 * Used by Header to maintain exact spacing between sections.
 * 
 * This component ensures that gaps between header elements (logo, navigation, icons)
 * have hard-coded dimensions that cannot shrink or grow, preventing layout shifts.
 */

interface SpacerProps {
    /** Width in pixels (for horizontal spacers) */
    width?: number;
    /** Height in pixels (for vertical spacers) */
    height?: number;
    /** Additional CSS classes */
    className?: string;
}

/**
 * Spacer Component
 * 
 * Creates a fixed-size empty space with exact dimensions.
 * 
 * @example
 * // Horizontal gap of 16px
 * <Spacer width={16} />
 * 
 * @example
 * // Vertical gap of 24px
 * <Spacer height={24} />
 */
export function Spacer({ width, height, className = '' }: SpacerProps) {
    const style: React.CSSProperties = {
        flexShrink: 0, // Never shrink
        flexGrow: 0,   // Never grow
    };

    if (width !== undefined) {
        style.width = `${width}px`;
        style.minWidth = `${width}px`;
        style.maxWidth = `${width}px`;
    }

    if (height !== undefined) {
        style.height = `${height}px`;
        style.minHeight = `${height}px`;
        style.maxHeight = `${height}px`;
    }

    return <div className={className} style={style} aria-hidden="true" />;
}
