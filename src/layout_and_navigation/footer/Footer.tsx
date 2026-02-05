// ============================================================================
// MODERN FOOTER COMPONENT - 2026 UX BEST PRACTICES
// ============================================================================

/**
 * Enhanced footer component with modern UX features:
 * 
 * ✅ Clean, minimal design
 * ✅ Domain-specific color system
 * ✅ Responsive layout
 * ✅ Theme-adaptive styling
 * ✅ Proper semantic structure
 * ✅ Accessible navigation
 */

import { ReactNode } from 'react';
import { appConfig } from '@/config/app';
import { useTheme } from '@/domains/theme';
import { useInlineTranslation } from '@/utils/language';
import { getFooterStyles } from '../colors';

interface FooterProps {
    /** Additional footer content */
    children?: ReactNode;
    /** Show social links */
    showSocialLinks?: boolean;
    /** Show navigation links */
    showNavigation?: boolean;
    /** Custom copyright text */
    copyright?: string;
}

export function Footer({
    children,
    showSocialLinks = false,
    showNavigation = true,
    copyright
}: FooterProps) {
    const { theme } = useTheme();
    const footerStyles = getFooterStyles(theme);

    // Inline translations for footer
    const descriptionText = useInlineTranslation('Seu assistente inteligente para cuidados de saúde. Conheça nossas soluções avançadas.', 'Your intelligent assistant for healthcare. Discover our advanced solutions.');
    const navigationTitle = useInlineTranslation('Navegação', 'Navigation');
    const homeText = useInlineTranslation('Início', 'Home');
    const aboutText = useInlineTranslation('Sobre', 'About');
    const contactText = useInlineTranslation('Contato', 'Contact');
    const socialTitle = useInlineTranslation('Redes Sociais', 'Social Media');
    const githubText = useInlineTranslation('GitHub', 'GitHub');
    const twitterText = useInlineTranslation('Twitter', 'Twitter');
    const linkedinText = useInlineTranslation('LinkedIn', 'LinkedIn');
    const technologyTitle = useInlineTranslation('Tecnologia', 'Technology');
    const technologyDescription = useInlineTranslation('Desenvolvido com as mais modernas tecnologias web.', 'Built with modern web technologies.');
    const privacyText = useInlineTranslation('Privacidade', 'Privacy');
    const termsText = useInlineTranslation('Termos', 'Terms');

    const currentYear = new Date().getFullYear();
    const copyrightText = copyright || `© ${currentYear} ${appConfig.displayName}. Built with modern web technologies.`;

    return (
        <footer
            className="border-t mt-auto"
            style={footerStyles}
        >
            <div className="container mx-auto px-4 py-8">
                {/* Main Footer Content */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-8">
                    {/* Brand Column */}
                    <div className="space-y-4">
                        <h3
                            className="font-semibold text-lg"
                            style={{
                                color: theme === 'light'
                                    ? 'rgb(17, 24, 39)'
                                    : 'rgb(243, 244, 246)'
                            }}
                        >
                            {appConfig.displayName}
                        </h3>
                        <p
                            className="text-sm leading-relaxed"
                            style={{
                                color: theme === 'light'
                                    ? 'rgb(107, 114, 128)'
                                    : 'rgb(156, 163, 175)'
                            }}
                        >
                            {descriptionText}
                        </p>
                    </div>

                    {/* Navigation Column */}
                    {showNavigation && (
                        <div className="space-y-4">
                            <h4
                                className="font-medium text-sm uppercase tracking-wide"
                                style={{
                                    color: theme === 'light'
                                        ? 'rgb(75, 85, 99)'
                                        : 'rgb(209, 213, 219)'
                                }}
                            >
                                {navigationTitle}
                            </h4>
                            <nav className="flex flex-col space-y-2">
                                <FooterLink href="/" theme={theme}>
                                    {homeText}
                                </FooterLink>
                                <FooterLink href="/about" theme={theme}>
                                    {aboutText}
                                </FooterLink>
                                <FooterLink href="/contact" theme={theme}>
                                    {contactText}
                                </FooterLink>
                            </nav>
                        </div>
                    )}

                    {/* Social Links or Custom Content Column */}
                    <div className="space-y-4">
                        {showSocialLinks ? (
                            <>
                                <h4
                                    className="font-medium text-sm uppercase tracking-wide"
                                    style={{
                                        color: theme === 'light'
                                            ? 'rgb(75, 85, 99)'
                                            : 'rgb(209, 213, 219)'
                                    }}
                                >
                                    {socialTitle}
                                </h4>
                                <div className="flex space-x-4">
                                    <SocialLink href="#" theme={theme} label={githubText}>
                                        <GitHubIcon />
                                    </SocialLink>
                                    <SocialLink href="#" theme={theme} label={twitterText}>
                                        <TwitterIcon />
                                    </SocialLink>
                                    <SocialLink href="#" theme={theme} label={linkedinText}>
                                        <LinkedInIcon />
                                    </SocialLink>
                                </div>
                            </>
                        ) : (
                            <div>
                                <h4
                                    className="font-medium text-sm uppercase tracking-wide mb-2"
                                    style={{
                                        color: theme === 'light'
                                            ? 'rgb(75, 85, 99)'
                                            : 'rgb(209, 213, 219)'
                                    }}
                                >
                                    {technologyTitle}
                                </h4>
                                <p
                                    className="text-sm"
                                    style={{
                                        color: theme === 'light'
                                            ? 'rgb(107, 114, 128)'
                                            : 'rgb(156, 163, 175)'
                                    }}
                                >
                                    {technologyDescription}
                                </p>
                            </div>
                        )}

                        {/* Custom content */}
                        {children}
                    </div>
                </div>

                {/* Footer Bottom */}
                <div className="pt-8 border-t" style={{ borderTopColor: footerStyles.borderColor }}>
                    <div className="flex flex-col md:flex-row justify-between items-center space-y-4 md:space-y-0">
                        {/* Copyright */}
                        <p
                            className="text-sm text-center md:text-left"
                            style={{ color: footerStyles.textColor }}
                        >
                            {copyrightText}
                        </p>

                        {/* Additional Footer Links */}
                        <div className="flex space-x-6">
                            <FooterLink href="/privacy" theme={theme} size="sm">
                                {privacyText}
                            </FooterLink>
                            <FooterLink href="/terms" theme={theme} size="sm">
                                {termsText}
                            </FooterLink>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
}

// ============================================================================
// FOOTER LINK COMPONENT
// ============================================================================

interface FooterLinkProps {
    href: string;
    children: ReactNode;
    theme: 'light' | 'dark';
    size?: 'sm' | 'md';
    onClick?: () => void;
}

function FooterLink({ href, children, theme, size = 'md', onClick }: FooterLinkProps) {
    const linkColor = theme === 'light' ? 'rgb(59, 130, 246)' : 'rgb(96, 165, 250)';
    const linkHoverColor = theme === 'light' ? 'rgb(37, 99, 235)' : 'rgb(147, 197, 253)';
    const textSize = size === 'sm' ? 'text-xs' : 'text-sm';

    return (
        <a
            href={href}
            onClick={onClick}
            className={`${textSize} transition-colors duration-200 hover:underline`}
            style={{ color: linkColor }}
            onMouseEnter={(e) => {
                e.currentTarget.style.color = linkHoverColor;
            }}
            onMouseLeave={(e) => {
                e.currentTarget.style.color = linkColor;
            }}
        >
            {children}
        </a>
    );
}

// ============================================================================
// SOCIAL LINK COMPONENT
// ============================================================================

interface SocialLinkProps {
    href: string;
    children: ReactNode;
    theme: 'light' | 'dark';
    label: string;
}

function SocialLink({ href, children, theme, label }: SocialLinkProps) {
    const iconColor = theme === 'light' ? 'rgb(107, 114, 128)' : 'rgb(156, 163, 175)';
    const iconHoverColor = theme === 'light' ? 'rgb(59, 130, 246)' : 'rgb(96, 165, 250)';

    return (
        <a
            href={href}
            aria-label={label}
            className="p-2 rounded-md transition-all duration-200 hover:scale-110"
            style={{
                color: iconColor,
                backgroundColor: 'transparent',
            }}
            onMouseEnter={(e) => {
                e.currentTarget.style.color = iconHoverColor;
                e.currentTarget.style.backgroundColor = theme === 'light'
                    ? 'rgba(59, 130, 246, 0.1)'
                    : 'rgba(96, 165, 250, 0.1)';
            }}
            onMouseLeave={(e) => {
                e.currentTarget.style.color = iconColor;
                e.currentTarget.style.backgroundColor = 'transparent';
            }}
        >
            {children}
        </a>
    );
}

// ============================================================================
// SIMPLE ICON COMPONENTS
// ============================================================================

function GitHubIcon() {
    return (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 0C4.477 0 0 4.484 0 10.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0110 4.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.203 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.942.359.31.678.921.678 1.856 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0020 10.017C20 4.484 15.522 0 10 0z" />
        </svg>
    );
}

function TwitterIcon() {
    return (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path d="M6.29 18.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0020 3.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.073 4.073 0 01.8 7.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 010 16.407a11.616 11.616 0 006.29 1.84" />
        </svg>
    );
}

function LinkedInIcon() {
    return (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M16.338 16.338H13.67V12.16c0-.995-.017-2.277-1.387-2.277-1.39 0-1.601 1.086-1.601 2.207v4.248H8.014v-8.59h2.559v1.174h.037c.356-.675 1.227-1.387 2.526-1.387 2.703 0 3.203 1.778 3.203 4.092v4.711zM5.005 6.575a1.548 1.548 0 11-.003-3.096 1.548 1.548 0 01.003 3.096zm-1.337 9.763H6.34v-8.59H3.667v8.59zM17.668 1H2.328C1.595 1 1 1.581 1 2.298v15.403C1 18.418 1.595 19 2.328 19h15.34c.734 0 1.332-.582 1.332-1.299V2.298C19 1.581 18.402 1 17.668 1z" />
        </svg>
    );
}