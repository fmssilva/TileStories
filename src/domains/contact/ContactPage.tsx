/**
 * CONTACT PAGE - IMPROVED DESIGN
 * ================================
 * 
 * Modern contact page with enhanced UX, proper contrast ratios (WCAG AA),
 * refined visual hierarchy, and polished interactions.
 * 
 * Improvements:
 * - Better contrast ratios for accessibility (WCAG AA compliant)
 * - Modern card designs with subtle shadows and borders
 * - Enhanced form validation feedback
 * - Refined color palette for both themes
 * - Smoother animations and micro-interactions
 * - Improved mobile responsiveness
 * - Better visual hierarchy and spacing
 */
import { useState } from 'react';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { useScrollAnimation, LAYOUT } from '@/design';
import { validateEmail } from '@/domains/email/validation';
import { museumConfig, contactConfig } from '@/config';
import { FeedbackModal } from '@/components/ui';

export function ContactPage() {
    const { language } = useLanguage();
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        subject: '',
        message: ''
    });
    const [formStatus, setFormStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState('');
    const [touchedFields, setTouchedFields] = useState({
        name: false,
        email: false,
        subject: false,
        message: false
    });

    const pageTitle = useInlineTranslation('Contacto', 'Contact');
    const pageSubtitle = useInlineTranslation(
        'Vamos criar algo extraordinário juntos',
        'Let\'s create something extraordinary together'
    );
    const nameLabel = useInlineTranslation('Nome', 'Name');
    const emailLabel = useInlineTranslation('Email', 'Email');
    const subjectLabel = useInlineTranslation('Assunto', 'Subject');
    const messageLabel = useInlineTranslation('Mensagem', 'Message');
    const sendButton = useInlineTranslation('Enviar Mensagem', 'Send Message');
    const directContactTitle = useInlineTranslation('Contacto Direto', 'Direct Contact');
    const projectLeadTitle = useInlineTranslation('Líder do Projeto', 'Project Lead');

    const { ref: formRef, isVisible: formVisible } = useScrollAnimation({
        threshold: 0.2,
        once: true
    });

    const { ref: infoRef, isVisible: infoVisible } = useScrollAnimation({
        threshold: 0.2,
        once: true
    });

    const handleFieldBlur = (field: keyof typeof touchedFields) => {
        setTouchedFields(prev => ({ ...prev, [field]: true }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage('');

        // Mark all fields as touched
        setTouchedFields({
            name: true,
            email: true,
            subject: true,
            message: true
        });

        // Validate email before submitting
        const emailValidation = validateEmail(formData.email, language);
        if (!emailValidation.isValid) {
            setFormStatus('error');
            setErrorMessage(emailValidation.error ||
                (language === 'pt' ? 'Email inválido.' : 'Invalid email.'));
            return;
        }

        // Validate required fields
        if (!formData.name.trim() || !formData.subject.trim() || !formData.message.trim()) {
            setFormStatus('error');
            setErrorMessage(language === 'pt'
                ? 'Por favor, preencha todos os campos obrigatórios.'
                : 'Please fill in all required fields.');
            return;
        }

        setFormStatus('loading');

        try {
            const response = await fetch('https://api.staticforms.xyz/submit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    accessKey: 'sf_6ki977i47881kfjjig1i32f4',
                    name: formData.name.trim(),
                    email: formData.email.trim(),
                    subject: formData.subject.trim(),
                    message: formData.message.trim(),
                    replyTo: formData.email.trim(),
                })
            });

            const result = await response.json();

            if (response.ok && !result.error) {
                setFormStatus('success');
                // Reset form after successful submission
                setFormData({
                    name: '',
                    email: '',
                    subject: '',
                    message: ''
                });
                setTouchedFields({
                    name: false,
                    email: false,
                    subject: false,
                    message: false
                });
            } else {
                throw new Error(result.error || result.message ||
                    (language === 'pt'
                        ? 'Erro ao processar a mensagem.'
                        : 'Error processing message.'));
            }
        } catch (error) {
            setFormStatus('error');
            if (error instanceof TypeError && error.message.includes('fetch')) {
                setErrorMessage(language === 'pt'
                    ? 'Erro de conexão. Verifique sua internet e tente novamente.'
                    : 'Connection error. Check your internet and try again.');
            } else if (error instanceof Error) {
                setErrorMessage(error.message);
            } else {
                setErrorMessage(language === 'pt'
                    ? 'Ocorreu um erro inesperado. Por favor, tente novamente.'
                    : 'An unexpected error occurred. Please try again.');
            }
        }
    };

    return (
        <div className="contact-page min-h-screen bg-gray-50 dark:bg-gray-950">
            {/* Success Feedback Modal */}
            <FeedbackModal
                isOpen={formStatus === 'success'}
                type="success"
                title={language === 'pt' ? 'Mensagem Enviada!' : 'Message Sent!'}
                message={language === 'pt'
                    ? 'Obrigado pelo seu contacto! Responderemos em breve ao seu email.'
                    : 'Thank you for contacting us! We will respond to your email soon.'}
                onClose={() => setFormStatus('idle')}
                autoDismiss={true}
                autoDismissDelay={5000}
            />

            {/* Error Feedback Modal */}
            <FeedbackModal
                isOpen={formStatus === 'error'}
                type="error"
                title={language === 'pt' ? 'Erro ao Enviar' : 'Error Sending Message'}
                message={errorMessage || (language === 'pt'
                    ? 'Ocorreu um erro ao enviar sua mensagem. Por favor, tente novamente ou entre em contacto diretamente pelo email.'
                    : 'An error occurred while sending your message. Please try again or contact us directly via email.')}
                onClose={() => setFormStatus('idle')}
            />

            {/* Hero Section with Video Background */}
            <section
                className="relative overflow-hidden"
                style={{
                    minHeight: `calc(100vh - ${LAYOUT.HEADER_HEIGHT}px)`,
                    marginTop: `${LAYOUT.HEADER_HEIGHT}px`
                }}
            >
                {/* Video Background */}
                <video
                    autoPlay
                    loop
                    muted
                    playsInline
                    className="absolute inset-0 w-full h-full object-cover"
                >
                    <source src="/videos/Lisbon_AR_vide_4_views.mp4" type="video/mp4" />
                </video>

                {/* Enhanced Gradient Overlay for Better Contrast */}
                <div className="absolute inset-0 bg-gradient-to-b from-gray-900/90 via-gray-900/80 to-gray-900/70"></div>

                {/* Subtle Pattern Overlay */}
                <div className="absolute inset-0 opacity-10"
                    style={{ backgroundImage: 'radial-gradient(circle at 2px 2px, white 1px, transparent 0)', backgroundSize: '32px 32px' }}></div>

                {/* Content - Centered both horizontally and vertically */}
                <div className="absolute inset-0 flex items-center justify-center text-center px-4">
                    <div className="max-w-4xl mx-auto">
                        <h1 className="text-5xl sm:text-6xl lg:text-7xl font-bold text-white mb-6 
                                     drop-shadow-2xl animate-fade-in-up tracking-tight"
                            style={{ textShadow: '0 4px 30px rgba(0, 0, 0, 0.8)' }}>
                            {pageTitle}
                        </h1>
                        <p className="text-xl sm:text-2xl lg:text-3xl text-gray-100 drop-shadow-lg
                                     animate-fade-in-up leading-relaxed"
                            style={{
                                animationDelay: '200ms',
                                textShadow: '0 2px 20px rgba(0, 0, 0, 0.7)'
                            }}>
                            {pageSubtitle}
                        </p>
                    </div>
                </div>

                {/* Scroll Indicator - Clickable */}
                <button
                    onClick={() => {
                        formRef.current?.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }}
                    className="absolute bottom-8 left-1/2 transform -translate-x-1/2 
                             animate-bounce cursor-pointer
                             hover:scale-110 transition-transform duration-200
                             focus:outline-none focus:ring-2 focus:ring-white/50 focus:ring-offset-2 focus:ring-offset-transparent
                             rounded-full p-2"
                    aria-label={language === 'pt' ? 'Rolar para o formulário' : 'Scroll to form'}
                >
                    <svg
                        className="w-8 h-8 text-white drop-shadow-lg"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 14l-7 7m0 0l-7-7m7 7V3"
                        />
                    </svg>
                </button>
            </section>

            {/* Contact Form Section */}
            <section className="py-20 bg-gray-50 dark:bg-gray-950">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12 items-start">
                        {/* Form Column */}
                        <div ref={formRef}
                            className={`transition-all duration-700 
                                       ${formVisible ? 'opacity-100 translate-x-0' : 'opacity-0 -translate-x-8'}`}>
                            <div className="bg-white dark:bg-gray-900 p-8 lg:p-10 rounded-2xl shadow-lg 
                                          border border-gray-200 dark:border-gray-800 
                                          hover:shadow-xl transition-shadow duration-300">
                                <h2 className="text-3xl font-bold mb-2 text-gray-900 dark:text-white">
                                    {useInlineTranslation('Envie-nos uma Mensagem', 'Send Us a Message')}
                                </h2>
                                <p className="text-gray-600 dark:text-gray-400 mb-8">
                                    {useInlineTranslation(
                                        'Responderemos o mais breve possível',
                                        'We\'ll get back to you as soon as possible'
                                    )}
                                </p>

                                <form onSubmit={handleSubmit} className="space-y-6">
                                    {/* Name Field */}
                                    <div>
                                        <label htmlFor="name" className="block text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">
                                            {nameLabel}
                                        </label>
                                        <input
                                            type="text"
                                            id="name"
                                            required
                                            value={formData.name}
                                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                            onBlur={() => handleFieldBlur('name')}
                                            className="w-full px-4 py-3 rounded-xl 
                                                     border-2 border-gray-300 dark:border-gray-700 
                                                     bg-white dark:bg-gray-800 
                                                     text-gray-900 dark:text-gray-100
                                                     placeholder-gray-500 dark:placeholder-gray-500
                                                     focus:ring-2 focus:ring-azulejo-blue-500 focus:border-azulejo-blue-500
                                                     transition-all duration-200
                                                     hover:border-gray-400 dark:hover:border-gray-600"
                                            placeholder={language === 'pt' ? 'Seu nome completo' : 'Your full name'}
                                        />
                                    </div>

                                    {/* Email Field */}
                                    <div>
                                        <label htmlFor="email" className="block text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">
                                            {emailLabel}
                                        </label>
                                        <input
                                            type="email"
                                            id="email"
                                            required
                                            pattern="[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"
                                            title={language === 'pt'
                                                ? 'Por favor, insira um endereço de email válido (ex: nome@exemplo.com)'
                                                : 'Please enter a valid email address (e.g., name@example.com)'}
                                            value={formData.email}
                                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                            onBlur={() => handleFieldBlur('email')}
                                            className="w-full px-4 py-3 rounded-xl 
                                                     border-2 border-gray-300 dark:border-gray-700 
                                                     bg-white dark:bg-gray-800 
                                                     text-gray-900 dark:text-gray-100
                                                     placeholder-gray-500 dark:placeholder-gray-500
                                                     focus:ring-2 focus:ring-azulejo-blue-500 focus:border-azulejo-blue-500
                                                     transition-all duration-200
                                                     hover:border-gray-400 dark:hover:border-gray-600
                                                     invalid:border-red-500 dark:invalid:border-red-500"
                                            placeholder={language === 'pt' ? 'seu@email.com' : 'your@email.com'}
                                        />
                                    </div>

                                    {/* Subject Field */}
                                    <div>
                                        <label htmlFor="subject" className="block text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">
                                            {subjectLabel}
                                        </label>
                                        <input
                                            type="text"
                                            id="subject"
                                            required
                                            value={formData.subject}
                                            onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                                            onBlur={() => handleFieldBlur('subject')}
                                            className="w-full px-4 py-3 rounded-xl 
                                                     border-2 border-gray-300 dark:border-gray-700 
                                                     bg-white dark:bg-gray-800 
                                                     text-gray-900 dark:text-gray-100
                                                     placeholder-gray-500 dark:placeholder-gray-500
                                                     focus:ring-2 focus:ring-azulejo-blue-500 focus:border-azulejo-blue-500
                                                     transition-all duration-200
                                                     hover:border-gray-400 dark:hover:border-gray-600"
                                            placeholder={language === 'pt' ? 'Assunto da mensagem' : 'Message subject'}
                                        />
                                    </div>

                                    {/* Message Field */}
                                    <div>
                                        <label htmlFor="message" className="block text-sm font-semibold text-gray-900 dark:text-gray-100 mb-2">
                                            {messageLabel}
                                        </label>
                                        <textarea
                                            id="message"
                                            required
                                            rows={6}
                                            value={formData.message}
                                            onChange={(e) => setFormData({ ...formData, message: e.target.value })}
                                            onBlur={() => handleFieldBlur('message')}
                                            className="w-full px-4 py-3 rounded-xl 
                                                     border-2 border-gray-300 dark:border-gray-700 
                                                     bg-white dark:bg-gray-800 
                                                     text-gray-900 dark:text-gray-100
                                                     placeholder-gray-500 dark:placeholder-gray-500
                                                     focus:ring-2 focus:ring-azulejo-blue-500 focus:border-azulejo-blue-500
                                                     transition-all duration-200 resize-none
                                                     hover:border-gray-400 dark:hover:border-gray-600"
                                            placeholder={language === 'pt' ? 'Escreva sua mensagem aqui...' : 'Write your message here...'}
                                        />
                                    </div>

                                    {/* Submit Button */}
                                    <button
                                        type="submit"
                                        disabled={formStatus === 'loading'}
                                        className="w-full bg-azulejo-blue-600 hover:bg-azulejo-blue-700 
                                                 dark:bg-azulejo-blue-600 dark:hover:bg-azulejo-blue-700
                                                 text-white font-semibold py-4 px-6 rounded-xl
                                                 shadow-lg hover:shadow-xl
                                                 transform hover:scale-[1.02] active:scale-[0.98]
                                                 transition-all duration-200
                                                 flex items-center justify-center gap-2
                                                 disabled:opacity-60 disabled:cursor-not-allowed 
                                                 disabled:hover:scale-100 disabled:hover:shadow-lg
                                                 focus:outline-none focus:ring-2 focus:ring-azulejo-blue-500 focus:ring-offset-2
                                                 dark:focus:ring-offset-gray-900"
                                    >
                                        {formStatus === 'loading' ? (
                                            <>
                                                <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                                </svg>
                                                {language === 'pt' ? 'Enviando...' : 'Sending...'}
                                            </>
                                        ) : (
                                            <>
                                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                                                </svg>
                                                {sendButton}
                                            </>
                                        )}
                                    </button>
                                </form>
                            </div>
                        </div>

                        {/* Contact Info Column */}
                        <div ref={infoRef}
                            className={`space-y-6 transition-all duration-700 delay-200
                                       ${infoVisible ? 'opacity-100 translate-x-0' : 'opacity-0 translate-x-8'}`}>

                            {/* Direct Contact Card */}
                            <div className="bg-gradient-to-br from-azulejo-blue-600 to-azulejo-blue-700 
                                          p-8 rounded-2xl shadow-xl text-white
                                          border border-azulejo-blue-500/20">
                                <h2 className="text-2xl font-bold mb-6">{directContactTitle}</h2>
                                <div className="space-y-5">
                                    {/* Project Lead */}
                                    <div className="flex items-start gap-4 p-4 rounded-xl bg-white/10 backdrop-blur-sm
                                                  hover:bg-white/15 transition-colors duration-200">
                                        <div className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center 
                                                      flex-shrink-0 ring-2 ring-white/30">
                                            <span className="text-2xl">👤</span>
                                        </div>
                                        <div>
                                            <p className="font-semibold text-base text-white/90 mb-1">{projectLeadTitle}</p>
                                            <p className="text-lg font-bold">Francisco Silva</p>
                                        </div>
                                    </div>

                                    {/* Email */}
                                    <div className="flex items-start gap-4 p-4 rounded-xl bg-white/10 backdrop-blur-sm
                                                  hover:bg-white/15 transition-colors duration-200">
                                        <div className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center 
                                                      flex-shrink-0 ring-2 ring-white/30">
                                            <span className="text-2xl">✉️</span>
                                        </div>
                                        <div className="min-w-0">
                                            <p className="font-semibold text-base text-white/90 mb-1">Email</p>
                                            <a href="mailto:fmso.silva@campus.fct.unl.pt"
                                                className="text-white font-medium hover:underline break-all
                                                         transition-all duration-200">
                                                fmso.silva@campus.fct.unl.pt
                                            </a>
                                        </div>
                                    </div>

                                    {/* Institution */}
                                    <div className="flex items-start gap-4 p-4 rounded-xl bg-white/10 backdrop-blur-sm
                                                  hover:bg-white/15 transition-colors duration-200">
                                        <div className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center 
                                                      flex-shrink-0 ring-2 ring-white/30">
                                            <span className="text-2xl">🎓</span>
                                        </div>
                                        <div>
                                            <p className="font-semibold text-base text-white/90 mb-1">
                                                {useInlineTranslation('Instituição', 'Institution')}
                                            </p>
                                            <p className="font-medium">{contactConfig.institution[language]}</p>
                                            <p className="text-white/80 text-sm mt-1">
                                                {useInlineTranslation(
                                                    'Projeto de Tese de Mestrado',
                                                    'Master\'s Thesis Project'
                                                )}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Museum Partner Card */}
                            <div className="bg-white dark:bg-gray-900 p-8 rounded-2xl shadow-lg 
                                          border border-gray-200 dark:border-gray-800
                                          hover:shadow-xl transition-shadow duration-300">
                                {/* Museum Header */}
                                <div className="text-center mb-6 pb-6 border-b border-gray-200 dark:border-gray-800">
                                    <div className="text-5xl mb-3">🏛️</div>
                                    <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                                        {museumConfig.name[language]}
                                    </h3>
                                    <p className="text-gray-700 dark:text-gray-300 font-medium">
                                        {museumConfig.role[language]}
                                    </p>
                                    <p className="text-gray-600 dark:text-gray-400 text-sm mt-1">
                                        {museumConfig.location.city}, {museumConfig.location.country}
                                    </p>
                                </div>

                                {/* Links Grid */}
                                <div className="space-y-3">
                                    {/* Website Link */}
                                    <a
                                        href={museumConfig.website}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center justify-between p-4 rounded-xl
                                                 bg-azulejo-blue-600 hover:bg-azulejo-blue-700
                                                 dark:bg-azulejo-blue-600 dark:hover:bg-azulejo-blue-700
                                                 text-white transition-all duration-200 group
                                                 shadow-md hover:shadow-lg transform hover:scale-[1.02]"
                                    >
                                        <div className="flex items-center gap-3">
                                            <span className="text-2xl">🌐</span>
                                            <div className="text-left">
                                                <p className="font-semibold text-base">
                                                    {useInlineTranslation('Site Oficial', 'Official Website')}
                                                </p>
                                                <p className="text-sm text-white/90">
                                                    {useInlineTranslation('Visite o museu online', 'Visit museum online')}
                                                </p>
                                            </div>
                                        </div>
                                        <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform"
                                            fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                        </svg>
                                    </a>

                                    {/* Google Maps Link */}
                                    <a
                                        href={museumConfig.googleMapsUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center justify-between p-4 rounded-xl
                                                 bg-red-600 hover:bg-red-700
                                                 dark:bg-red-600 dark:hover:bg-red-700
                                                 text-white transition-all duration-200 group
                                                 shadow-md hover:shadow-lg transform hover:scale-[1.02]"
                                    >
                                        <div className="flex items-center gap-3">
                                            <span className="text-2xl">📍</span>
                                            <div className="text-left">
                                                <p className="font-semibold text-base">
                                                    {useInlineTranslation('Como Chegar', 'Get Directions')}
                                                </p>
                                                <p className="text-sm text-white/90">
                                                    {useInlineTranslation('Abrir no Google Maps', 'Open in Google Maps')}
                                                </p>
                                            </div>
                                        </div>
                                        <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform"
                                            fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                        </svg>
                                    </a>

                                    {/* Google Arts & Culture Link */}
                                    <a
                                        href={museumConfig.googleArtsUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center justify-between p-4 rounded-xl
                                                 bg-amber-600 hover:bg-amber-700
                                                 dark:bg-amber-600 dark:hover:bg-amber-700
                                                 text-white transition-all duration-200 group
                                                 shadow-md hover:shadow-lg transform hover:scale-[1.02]"
                                    >
                                        <div className="flex items-center gap-3">
                                            <span className="text-2xl">🎨</span>
                                            <div className="text-left">
                                                <p className="font-semibold text-base">{museumConfig.googleArtsTitle[language]}</p>
                                                <p className="text-sm text-white/90">
                                                    {useInlineTranslation('Explore a coleção', 'Explore the collection')}
                                                </p>
                                            </div>
                                        </div>
                                        <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform"
                                            fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                        </svg>
                                    </a>
                                </div>
                            </div>

                            {/* Response Time Card */}
                            <div className="bg-emerald-50 dark:bg-emerald-950/40 p-5 rounded-xl 
                                          border-2 border-emerald-200 dark:border-emerald-900/50
                                          shadow-sm">
                                <div className="flex items-start gap-3">
                                    <div className="w-10 h-10 bg-emerald-100 dark:bg-emerald-900/50 
                                                  rounded-full flex items-center justify-center flex-shrink-0">
                                        <span className="text-xl">⏱️</span>
                                    </div>
                                    <div>
                                        <p className="font-semibold text-gray-900 dark:text-gray-100 mb-1">
                                            {useInlineTranslation('Tempo de Resposta', 'Response Time')}
                                        </p>
                                        <p className="text-sm text-gray-700 dark:text-gray-300">
                                            {useInlineTranslation(
                                                `Normalmente respondemos em ${contactConfig.responseTime.pt}`,
                                                `We typically respond within ${contactConfig.responseTime.en}`
                                            )}
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    );
}

export default ContactPage;