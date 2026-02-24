/**
 * BREADCRUMBS COMPONENT
 * =====================
 * 
 * Auto-generated breadcrumb navigation from hierarchy.
 * Shows user's current location in site structure.
 */

import { Link } from 'react-router-dom';
import { useBreadcrumbs } from '../hooks';
import { getNavItemLabel } from '../utils';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import type { BreadcrumbsProps } from '../types';

/**
 * Breadcrumbs Component
 * 
 * Automatically shows navigation trail for current page.
 * 
 * @example
 * <Breadcrumbs separator="/" maxItems={4} />
 */
export function Breadcrumbs({
    separator = '/',
    maxItems = 5,
    showHomeIcon = true,
}: BreadcrumbsProps = {}) {
    const breadcrumbs = useBreadcrumbs();
    const { language } = useLanguage();
    const homeLabel = useInlineTranslation('Início', 'Home');

    // Don't show breadcrumbs on home page or if only one item
    if (breadcrumbs.length <= 1) {
        return null;
    }

    // Truncate if too many items
    const displayCrumbs = breadcrumbs.length > maxItems
        ? [
            breadcrumbs[0],
            { id: 'ellipsis', label: '...', path: '#', styleLevel: 'inner' as const },
            ...breadcrumbs.slice(-(maxItems - 2))
        ]
        : breadcrumbs;

    return (
        <nav aria-label="Breadcrumb" className="py-3 px-4 bg-muted/50">
            <ol className="flex items-center space-x-2 text-sm">
                {displayCrumbs.map((crumb, index) => {
                    if (!crumb) return null;

                    const isLast = index === displayCrumbs.length - 1;
                    const isHome = crumb.path === '/';
                    const isEllipsis = crumb.id === 'ellipsis';

                    return (
                        <li key={crumb.id} className="flex items-center space-x-2">
                            {/* Separator */}
                            {index > 0 && (
                                <span className="text-muted-foreground" aria-hidden="true">
                                    {typeof separator === 'string' ? separator : separator}
                                </span>
                            )}

                            {/* Breadcrumb link or text */}
                            {isLast || isEllipsis ? (
                                <span className="text-foreground font-medium" aria-current="page">
                                    {isHome && showHomeIcon ? (
                                        <HomeIcon />
                                    ) : (
                                        getNavItemLabel(crumb, language, true)
                                    )}
                                </span>
                            ) : (
                                <Link
                                    to={crumb.path}
                                    className="text-primary hover:text-primary/80 hover:underline transition-colors"
                                >
                                    {isHome && showHomeIcon ? (
                                        <HomeIcon />
                                    ) : (
                                        isHome ? homeLabel : getNavItemLabel(crumb, language, true)
                                    )}
                                </Link>
                            )}
                        </li>
                    );
                })}
            </ol>
        </nav>
    );
}

/**
 * Simple home icon for breadcrumbs
 */
function HomeIcon() {
    return (
        <svg
            className="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-label="Home"
        >
            <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
            />
        </svg>
    );
}
