/**
 * FOOTER COMPONENT
 * ================
 * 
 * Main site footer with navigation and info.
 */

import { Link } from 'react-router-dom';
import { appConfig } from '@/config/app';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { navigationConfig } from '../config';
import { getFooterNavItems, getNavItemLabel } from '../utils';
import type { FooterProps } from '../types';

/**
 * Footer Component
 * 
 * Site footer with navigation links and copyright.
 * 
 * @example
 * <Footer showNavigation />
 */
export function Footer({
    children,
    showNavigation = true,
    copyright,
}: FooterProps = {}) {
    const { language } = useLanguage();
    const navItems = getFooterNavItems(navigationConfig);

    const descriptionText = useInlineTranslation(
        'Explore a história de Lisboa através de realidade aumentada.',
        'Explore Lisbon\'s history through augmented reality.'
    );
    const navigationTitle = useInlineTranslation('Navegação', 'Navigation');
    const projectText = useInlineTranslation('Projeto de tese de mestrado.', 'Master\'s thesis project.');

    const currentYear = new Date().getFullYear();
    const copyrightText = copyright || `© ${currentYear} ${appConfig.displayName}. ${projectText}`;

    return (
        <footer className="border-t mt-auto bg-muted/30">
            <div className="container mx-auto px-4 py-8">
                {/* Main Content */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
                    {/* Brand Column */}
                    <div className="space-y-4">
                        <h3 className="font-semibold text-lg text-foreground">
                            {appConfig.displayName}
                        </h3>
                        <p className="text-sm text-muted-foreground leading-relaxed">
                            {descriptionText}
                        </p>
                    </div>

                    {/* Navigation Column */}
                    {showNavigation && (
                        <div className="space-y-4">
                            <h4 className="font-medium text-sm uppercase tracking-wide text-muted-foreground">
                                {navigationTitle}
                            </h4>
                            <nav className="flex flex-col space-y-2">
                                {navItems.map(item => (
                                    <Link
                                        key={item.id}
                                        to={item.path}
                                        className="text-sm text-primary hover:text-primary/80 hover:underline transition-colors"
                                    >
                                        {getNavItemLabel(item, language)}
                                    </Link>
                                ))}
                            </nav>
                            {children}
                        </div>
                    )}
                </div>

                {/* Footer Bottom */}
                <div className="pt-8 border-t border-border">
                    <p className="text-sm text-center text-muted-foreground">
                        {copyrightText}
                    </p>
                </div>
            </div>
        </footer>
    );
}
