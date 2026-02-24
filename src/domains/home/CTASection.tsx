/**
 * CALL TO ACTION SECTION
 * ======================
 * Final section driving visitor action: Download app, visit museum, or subscribe
 * 
 * Layout: 3-column grid (responsive to 1 column)
 * Design: Modern gradient with glass morphism, prominent CTAs with micro-interactions
 * Phase 2: Newsletter form with basic validation (useState)
 * Phase 4: Connect newsletter to backend API
 * Phase 5: Scroll-triggered animations
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { Button, Modal, ComingSoonBadge, FeedbackModal } from '@/components/ui';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';
import { useTheme } from '@/design/theme';
import { museumConfig, projectConfig } from '@/config';
import { validateEmail } from '@/domains/email/validation';

export default function CTASection() {
    const t = useInlineTranslation;
    const { theme } = useTheme();
    const { language } = useLanguage();

    // Scroll animation for section (Phase 5)
    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    // Newsletter form state
    const [email, setEmail] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitStatus, setSubmitStatus] = useState<'idle' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState('');

    // Coming Soon modal state
    const [showComingSoonModal, setShowComingSoonModal] = useState(false);

    const handleNewsletterSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage('');

        // Validate email using comprehensive validation utility
        const emailValidation = validateEmail(email, language);
        if (!emailValidation.isValid) {
            setSubmitStatus('error');
            setErrorMessage(emailValidation.error ||
                (language === 'pt' ? 'Email inválido.' : 'Invalid email.'));
            return;
        }

        setIsSubmitting(true);
        setSubmitStatus('idle');

        try {
            const response = await fetch('https://api.staticforms.xyz/submit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    accessKey: 'sf_6ki977i47881kfjjig1i32f4',
                    email: email.trim(),
                    subject: language === 'pt'
                        ? 'Newsletter TileStories - Nova Inscrição'
                        : 'TileStories Newsletter - New Subscription',
                    message: language === 'pt'
                        ? `Novo inscrito na newsletter: ${email.trim()}`
                        : `New newsletter subscription: ${email.trim()}`,
                    replyTo: email.trim(),
                })
            });

            const result = await response.json();

            // Check for successful submission
            if (response.ok && !result.error) {
                setSubmitStatus('success');
                setEmail('');
            } else {
                // API returned error
                throw new Error(result.error || result.message ||
                    (language === 'pt'
                        ? 'Erro ao processar inscrição.'
                        : 'Error processing subscription.'));
            }
        } catch (error) {
            setSubmitStatus('error');

            // Handle different types of errors
            if (error instanceof TypeError && error.message.includes('fetch')) {
                // Network error
                setErrorMessage(language === 'pt'
                    ? 'Erro de conexão. Verifique sua internet e tente novamente.'
                    : 'Connection error. Check your internet and try again.');
            } else if (error instanceof Error) {
                // Other errors with message
                setErrorMessage(error.message);
            } else {
                // Unknown error
                setErrorMessage(language === 'pt'
                    ? 'Ocorreu um erro inesperado. Por favor, tente novamente.'
                    : 'An unexpected error occurred. Please try again.');
            }
        } finally {
            setIsSubmitting(false);

            // Auto-reset success status after 5 seconds
            if (submitStatus === 'success') {
                setTimeout(() => {
                    setSubmitStatus('idle');
                }, 5000);
            }
        }
    };

    return (
        <section
            ref={sectionRef}
            className="py-20 px-4 md:px-8 relative overflow-hidden"
            aria-labelledby="cta-heading"
            style={{
                background: 'linear-gradient(135deg, #2C4A73 0%, #3C5E95 25%, #5081B6 50%, #7BA3D0 75%, #D4AF37 100%)',
            }}
        >
            {/* Success Feedback Modal */}
            <FeedbackModal
                isOpen={submitStatus === 'success'}
                type="success"
                title={language === 'pt' ? 'Inscrição Confirmada!' : 'Subscription Confirmed!'}
                message={language === 'pt'
                    ? 'Obrigado por se inscrever na nossa newsletter! Você receberá atualizações sobre novos recursos e exposições.'
                    : 'Thank you for subscribing to our newsletter! You will receive updates on new features and exhibitions.'}
                onClose={() => setSubmitStatus('idle')}
                autoDismiss={true}
                autoDismissDelay={5000}
            />

            {/* Error Feedback Modal */}
            <FeedbackModal
                isOpen={submitStatus === 'error'}
                type="error"
                title={language === 'pt' ? 'Erro ao Inscrever' : 'Subscription Error'}
                message={errorMessage || (language === 'pt'
                    ? 'Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente ou entre em contacto diretamente.'
                    : 'An error occurred while processing your subscription. Please try again or contact us directly.')}
                onClose={() => setSubmitStatus('idle')}
            />

            {/* Decorative azulejo pattern overlay */}
            <div
                className="absolute inset-0 opacity-10"
                style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M30 0l30 30-30 30L0 30 30 0z' fill='%23ffffff' fill-opacity='0.3'/%3E%3C/svg%3E")`,
                    backgroundSize: '60px 60px',
                }}
                aria-hidden="true"
            />

            {/* Container */}
            <div className="max-w-7xl mx-auto relative z-10">
                {/* Section Heading with enhanced typography */}
                <div className="text-center mb-12">
                    <h2
                        id="cta-heading"
                        className={`text-3xl md:text-4xl lg:text-5xl font-bold text-white mb-4 
                                   tracking-tight leading-tight
                                   ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    >
                        {t({
                            pt: 'Pronto Para Explorar?',
                            en: 'Ready to Explore?',
                        })}
                    </h2>
                    <p
                        className={`text-lg md:text-xl text-white/90 max-w-2xl mx-auto font-light
                                   ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '100ms' }}
                    >
                        {t({
                            pt: 'Comece sua jornada através da história de Lisboa hoje',
                            en: 'Start your journey through Lisbon\'s history today',
                        })}
                    </p>
                </div>

                {/* CTA Grid with glass morphism cards */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 lg:gap-8 mb-16">

                    {/* CTA 1: Download App - Enhanced with app store badges */}
                    <div
                        className={`group bg-white/95 dark:bg-gray-900/95 backdrop-blur-md 
                                   rounded-2xl shadow-2xl p-8 text-center 
                                   border border-white/20 dark:border-gray-700/50
                                   hover:shadow-[0_20px_60px_rgba(212,175,55,0.3)] 
                                   hover:-translate-y-2 transition-all duration-300
                                   ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '200ms' }}
                    >
                        <div
                            className="text-6xl mb-4 transform group-hover:scale-110 transition-transform duration-300"
                            aria-hidden="true"
                        >
                            📱
                        </div>
                        <h3 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white mb-3">
                            {t({
                                pt: 'Baixe o TileStories',
                                en: 'Download TileStories',
                            })}
                        </h3>
                        <p className="text-gray-600 dark:text-gray-300 mb-6 text-base leading-relaxed">
                            {t({
                                pt: 'Disponível para iOS e Android. Explore azulejos com realidade aumentada.',
                                en: 'Available for iOS and Android. Explore tiles with augmented reality.',
                            })}
                        </p>
                        <Button
                            variant="primary"
                            size="lg"
                            className="w-full text-lg py-4 font-semibold 
                                     bg-gradient-to-r from-azulejo-blue-600 to-azulejo-blue-700
                                     hover:from-azulejo-blue-700 hover:to-azulejo-blue-800
                                     transform hover:scale-105 transition-all duration-200
                                     shadow-lg hover:shadow-xl"
                            onClick={() => setShowComingSoonModal(true)}
                        >
                            {t({
                                pt: 'Baixar Agora',
                                en: 'Download Now',
                            })}
                        </Button>
                        <div className="mt-4 flex items-center justify-center gap-2 text-sm text-gray-500 dark:text-gray-400">
                            <span className="inline-block w-2 h-2 rounded-full bg-green-500 animate-pulse"></span>
                            {t({
                                pt: 'Lançamento Verão 2026',
                                en: 'Launching Summer 2026',
                            })}
                        </div>
                    </div>

                    {/* CTA 2: Visit Museum - Enhanced with hover effects */}
                    <div
                        className={`group bg-white/95 dark:bg-gray-900/95 backdrop-blur-md 
                                   rounded-2xl shadow-2xl p-8
                                   border border-white/20 dark:border-gray-700/50
                                   hover:shadow-[0_20px_60px_rgba(212,175,55,0.3)]
                                   hover:-translate-y-2 transition-all duration-300
                                   ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '300ms' }}
                    >
                        <div
                            className="text-6xl mb-4 text-center transform group-hover:scale-110 transition-transform duration-300"
                            aria-hidden="true"
                        >
                            🏛️
                        </div>
                        <h3 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white mb-3 text-center">
                            {t({
                                pt: 'Visite o Museu',
                                en: 'Visit the Museum',
                            })}
                        </h3>
                        <p className="text-gray-600 dark:text-gray-300 mb-6 text-base text-center leading-relaxed">
                            {t(museumConfig.name)}
                        </p>

                        {/* Museum Links with enhanced styling */}
                        <div className="space-y-3">
                            <a
                                href={museumConfig.website}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="flex items-center justify-between gap-3 px-5 py-3.5
                                         bg-gradient-to-r from-azulejo-blue-600 to-azulejo-blue-700
                                         hover:from-azulejo-blue-700 hover:to-azulejo-blue-800
                                         text-white rounded-xl transition-all duration-200
                                         text-base font-semibold group/link shadow-md hover:shadow-lg
                                         transform hover:scale-[1.02]"
                            >
                                <div className="flex items-center gap-2">
                                    <span className="text-xl">🌐</span>
                                    <span>{t({ pt: 'Site Oficial', en: 'Official Website' })}</span>
                                </div>
                                <span className="group-hover/link:translate-x-1 transition-transform text-lg">→</span>
                            </a>

                            <a
                                href={museumConfig.googleArtsUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="flex items-center justify-between gap-3 px-5 py-3.5
                                         bg-gradient-to-r from-azulejo-gold to-[#C19B2E]
                                         hover:from-[#C19B2E] hover:to-azulejo-gold
                                         text-gray-900 rounded-xl transition-all duration-200
                                         text-base font-semibold group/link shadow-md hover:shadow-lg
                                         transform hover:scale-[1.02]"
                            >
                                <div className="flex items-center gap-2">
                                    <span className="text-xl">🎨</span>
                                    <span>{t({ pt: 'Grande Panorama', en: 'Grande Panorama' })}</span>
                                </div>
                                <span className="group-hover/link:translate-x-1 transition-transform text-lg">→</span>
                            </a>

                            <a
                                href={museumConfig.googleMapsUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="flex items-center justify-between gap-3 px-5 py-3.5
                                         bg-white dark:bg-gray-800
                                         border-2 border-azulejo-blue-600 dark:border-azulejo-blue-500
                                         hover:bg-azulejo-blue-50 dark:hover:bg-azulejo-blue-900/30
                                         text-azulejo-blue-700 dark:text-azulejo-blue-400
                                         rounded-xl transition-all duration-200
                                         text-base font-semibold group/link shadow-md hover:shadow-lg
                                         transform hover:scale-[1.02]"
                            >
                                <div className="flex items-center gap-2">
                                    <span className="text-xl">📍</span>
                                    <span>{t({ pt: 'Como Chegar', en: 'Get Directions' })}</span>
                                </div>
                                <span className="group-hover/link:translate-x-1 transition-transform text-lg">→</span>
                            </a>
                        </div>
                    </div>

                    {/* CTA 3: Newsletter - Enhanced with better UX */}
                    <div
                        className={`group bg-white/95 dark:bg-gray-900/95 backdrop-blur-md 
                                   rounded-2xl shadow-2xl p-8 text-center
                                   border border-white/20 dark:border-gray-700/50
                                   hover:shadow-[0_20px_60px_rgba(212,175,55,0.3)]
                                   hover:-translate-y-2 transition-all duration-300
                                   ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '400ms' }}
                    >
                        <div
                            className="text-6xl mb-4 transform group-hover:scale-110 transition-transform duration-300"
                            aria-hidden="true"
                        >
                            ✉️
                        </div>
                        <h3 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white mb-3">
                            {t({
                                pt: 'Fique Atualizado',
                                en: 'Stay Updated',
                            })}
                        </h3>
                        <p className="text-gray-600 dark:text-gray-300 mb-6 text-base leading-relaxed">
                            {t({
                                pt: 'Receba notícias sobre novos recursos e exposições',
                                en: 'Get updates on new features and exhibitions',
                            })}
                        </p>

                        <form onSubmit={handleNewsletterSubmit} className="space-y-4">
                            <div className="relative">
                                <input
                                    type="email"
                                    value={email}
                                    onChange={(e) => {
                                        setEmail(e.target.value);
                                        if (submitStatus === 'error') {
                                            setSubmitStatus('idle');
                                            setErrorMessage('');
                                        }
                                    }}
                                    placeholder={t({
                                        pt: 'seu@email.com',
                                        en: 'your@email.com',
                                    })}
                                    className="w-full px-5 py-4 border-2 border-gray-300 dark:border-gray-600 
                                             rounded-xl text-base
                                             bg-white dark:bg-gray-800 text-gray-900 dark:text-white
                                             placeholder:text-gray-400 dark:placeholder:text-gray-500
                                             focus:ring-2 focus:ring-azulejo-gold focus:border-azulejo-gold 
                                             outline-none transition-all duration-200
                                             hover:border-azulejo-blue-400"
                                    aria-label={t({
                                        pt: 'Endereço de email',
                                        en: 'Email address',
                                    })}
                                    required
                                />
                            </div>

                            <Button
                                variant="outline"
                                size="lg"
                                className="w-full text-lg py-4 font-semibold
                                         bg-gradient-to-r from-azulejo-gold to-[#C19B2E]
                                         hover:from-[#C19B2E] hover:to-azulejo-gold
                                         text-white border-0
                                         transform hover:scale-105 transition-all duration-200
                                         shadow-lg hover:shadow-xl
                                         disabled:opacity-50 disabled:cursor-not-allowed 
                                         disabled:transform-none"
                                disabled={isSubmitting || !email}
                                asChild
                            >
                                <button type="submit">
                                    {isSubmitting ? (
                                        <span className="flex items-center justify-center gap-2">
                                            <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                            </svg>
                                            {t({ pt: 'Enviando...', en: 'Submitting...' })}
                                        </span>
                                    ) : (
                                        t({ pt: 'Inscrever-se', en: 'Subscribe' })
                                    )}
                                </button>
                            </Button>

                            {/* Contact Button - Secondary CTA */}
                            <Button
                                variant="outline"
                                size="lg"
                                className="w-full text-lg py-4 font-semibold
                                         bg-white dark:bg-gray-800
                                         border-2 border-azulejo-blue-600 dark:border-azulejo-blue-500
                                         hover:bg-azulejo-blue-50 dark:hover:bg-azulejo-blue-900/30
                                         text-azulejo-blue-700 dark:text-azulejo-blue-400
                                         transform hover:scale-105 transition-all duration-200
                                         shadow-md hover:shadow-lg"
                                asChild
                            >
                                <Link to="/contact">
                                    {t({ pt: 'Contactar', en: 'Contact' })}
                                </Link>
                            </Button>
                        </form>
                    </div>
                </div>

                {/* Partnership Section - Modernized with better spacing */}
                <div
                    className={`mt-4 text-center ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '500ms' }}
                >
                    <p className="text-white/80 text-base mb-8 font-light max-w-2xl mx-auto">
                        {t(projectConfig.collaborationText)}
                    </p>
                    <div className="flex flex-wrap items-center justify-center gap-8 md:gap-12">
                        {/* FCT NOVA Logo */}
                        <a
                            href="https://www.fct.unl.pt"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="bg-white/10 backdrop-blur-md rounded-2xl px-10 py-8 
                                     border border-white/20 hover:bg-white/20 
                                     transition-all duration-300 hover:scale-110 
                                     hover:shadow-[0_10px_40px_rgba(255,255,255,0.2)]
                                     group"
                        >
                            <img
                                src={theme === 'dark' ? '/FCT_logo_dark.png' : '/FCT_logo_light.png'}
                                alt="FCT NOVA - Faculdade de Ciências e Tecnologia da Universidade Nova de Lisboa"
                                className="h-20 w-auto object-contain group-hover:brightness-110 transition-all"
                            />
                        </a>

                        {/* Museu Nacional do Azulejo Logo */}
                        <a
                            href={museumConfig.website}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="bg-white/10 backdrop-blur-md rounded-2xl px-10 py-8 
                                     border border-white/20 hover:bg-white/20 
                                     transition-all duration-300 hover:scale-110 
                                     hover:shadow-[0_10px_40px_rgba(255,255,255,0.2)]
                                     group"
                        >
                            <img
                                src={museumConfig.logoPath}
                                alt={language === 'pt' ? museumConfig.name.pt : museumConfig.name.en}
                                className="h-20 w-auto object-contain group-hover:brightness-110 transition-all"
                            />
                        </a>
                    </div>
                </div>
            </div>

            {/* Coming Soon Modal - Enhanced design */}
            <Modal
                isOpen={showComingSoonModal}
                onClose={() => setShowComingSoonModal(false)}
                size="md"
            >
                <Modal.Header>
                    <h2 className="text-3xl font-bold text-azulejo-blue-900 dark:text-white">
                        {t({ pt: 'Aplicativo Em Breve!', en: 'App Coming Soon!' })}
                    </h2>
                </Modal.Header>
                <Modal.Content>
                    <div className="space-y-8 text-center py-6">
                        {/* Animated Icon */}
                        <div className="text-8xl animate-bounce">📱</div>

                        {/* Coming Soon Badge */}
                        <div className="flex justify-center">
                            <ComingSoonBadge
                                variant="inline"
                                launchText={t({ pt: 'Verão 2026', en: 'Summer 2026' })}
                            />
                        </div>


                        {/* Description */}
                        <p className="text-lg text-gray-700 dark:text-gray-300 max-w-md mx-auto leading-relaxed">
                            {t({
                                pt: 'Estamos a trabalhar arduamente para lançar a aplicação TileStories. Inscreva-se na nossa newsletter para ser notificado quando estivermos prontos!',
                                en: 'We\'re working hard to launch the TileStories app. Subscribe to our newsletter to be notified when we\'re ready!',
                            })}
                        </p>

                        {/* Feature list */}
                        <div className="bg-azulejo-blue-50 dark:bg-azulejo-blue-900/20 rounded-xl p-6 text-left">
                            <p className="font-semibold text-azulejo-blue-900 dark:text-azulejo-blue-300 mb-3">
                                {t({ pt: 'Recursos planejados:', en: 'Planned features:' })}
                            </p>
                            <ul className="space-y-2 text-gray-700 dark:text-gray-300">
                                <li className="flex items-center gap-2">
                                    <span className="text-azulejo-gold">✓</span>
                                    {t({ pt: 'Realidade Aumentada', en: 'Augmented Reality' })}
                                </li>
                                <li className="flex items-center gap-2">
                                    <span className="text-azulejo-gold">✓</span>
                                    {t({ pt: 'Guias de Áudio', en: 'Audio Guides' })}
                                </li>
                                <li className="flex items-center gap-2">
                                    <span className="text-azulejo-gold">✓</span>
                                    {t({ pt: 'Tours Interativos', en: 'Interactive Tours' })}
                                </li>
                            </ul>
                        </div>

                        {/* Close Button */}
                        <Button
                            onClick={() => setShowComingSoonModal(false)}
                            variant="primary"
                            className="w-full sm:w-auto px-8 py-3 text-lg"
                        >
                            {t({ pt: 'Entendi', en: 'Got It' })}
                        </Button>
                    </div>
                </Modal.Content>
            </Modal>
        </section>
    );
}
