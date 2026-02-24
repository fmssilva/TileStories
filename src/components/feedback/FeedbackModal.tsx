/**
 * FEEDBACK MODAL COMPONENT
 * ========================
 * 
 * Centered modal for displaying form submission feedback
 * 
 * Features:
 * - Success state (green theme with checkmark icon)
 * - Error state (red theme with X icon)
 * - Bilingual support (PT/EN)
 * - Auto-dismiss option for success
 * - Centered, prominent display
 * - Dark mode support
 * - Smooth animations
 * 
 * Usage:
 * <FeedbackModal
 *   isOpen={formStatus === 'success'}
 *   type="success"
 *   title="Success!"
 *   message="Your message was sent."
 *   onClose={() => setFormStatus('idle')}
 * />
 */

import { useEffect } from 'react';
import { Modal } from '@/components/ui/Modal/Modal';

interface FeedbackModalProps {
    isOpen: boolean;
    onClose: () => void;
    type: 'success' | 'error';
    title: string;
    message: string;
    autoDismiss?: boolean;
    autoDismissDelay?: number; // in milliseconds
}

export function FeedbackModal({
    isOpen,
    onClose,
    type,
    title,
    message,
    autoDismiss = false,
    autoDismissDelay = 5000,
}: FeedbackModalProps) {

    // Auto-dismiss functionality
    useEffect(() => {
        if (isOpen && autoDismiss && type === 'success') {
            const timer = setTimeout(() => {
                onClose();
            }, autoDismissDelay);

            return () => clearTimeout(timer);
        }

        return () => { }; // Return empty cleanup function for other cases
    }, [isOpen, autoDismiss, autoDismissDelay, type, onClose]);

    // Color schemes based on type
    const colorSchemes = {
        success: {
            bg: 'bg-green-50 dark:bg-green-900/20',
            border: 'border-green-200 dark:border-green-800',
            iconBg: 'bg-green-100 dark:bg-green-800/40',
            iconColor: 'text-green-600 dark:text-green-400',
            titleColor: 'text-green-900 dark:text-green-100',
            messageColor: 'text-green-800 dark:text-green-200',
            buttonBg: 'bg-green-600 hover:bg-green-700 dark:bg-green-700 dark:hover:bg-green-600',
        },
        error: {
            bg: 'bg-red-50 dark:bg-red-900/20',
            border: 'border-red-200 dark:border-red-800',
            iconBg: 'bg-red-100 dark:bg-red-800/40',
            iconColor: 'text-red-600 dark:text-red-400',
            titleColor: 'text-red-900 dark:text-red-100',
            messageColor: 'text-red-800 dark:text-red-200',
            buttonBg: 'bg-red-600 hover:bg-red-700 dark:bg-red-700 dark:hover:bg-red-600',
        },
    };

    const colors = colorSchemes[type];

    // Icons
    const SuccessIcon = () => (
        <svg className="w-12 h-12" fill="currentColor" viewBox="0 0 20 20">
            <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                clipRule="evenodd"
            />
        </svg>
    );

    const ErrorIcon = () => (
        <svg className="w-12 h-12" fill="currentColor" viewBox="0 0 20 20">
            <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                clipRule="evenodd"
            />
        </svg>
    );

    return (
        <Modal isOpen={isOpen} onClose={onClose} size="sm">
            <Modal.Content noPadding>
                <div className={`p-8 rounded-lg ${colors.bg} border-2 ${colors.border}`}>
                    {/* Icon */}
                    <div className="flex justify-center mb-6">
                        <div className={`${colors.iconBg} ${colors.iconColor} p-4 rounded-full 
                                       animate-in zoom-in-95 duration-300`}>
                            {type === 'success' ? <SuccessIcon /> : <ErrorIcon />}
                        </div>
                    </div>

                    {/* Title */}
                    <h3 className={`text-2xl font-bold text-center mb-4 ${colors.titleColor}`}>
                        {title}
                    </h3>

                    {/* Message */}
                    <p className={`text-center mb-8 leading-relaxed ${colors.messageColor}`}>
                        {message}
                    </p>

                    {/* Close Button */}
                    <button
                        onClick={onClose}
                        className={`w-full ${colors.buttonBg} text-white font-semibold 
                                  py-3 px-6 rounded-lg shadow-md hover:shadow-lg
                                  transform hover:scale-105 active:scale-95
                                  transition-all duration-200 focus:outline-none 
                                  focus:ring-2 focus:ring-offset-2 
                                  ${type === 'success' ? 'focus:ring-green-500' : 'focus:ring-red-500'}`}
                    >
                        {type === 'success'
                            ? (autoDismiss ? '✓ OK' : 'OK')
                            : 'Fechar / Close'}
                    </button>

                    {/* Auto-dismiss indicator */}
                    {autoDismiss && type === 'success' && (
                        <p className="text-xs text-center mt-4 text-green-600 dark:text-green-400 opacity-75">
                            Auto-close in {autoDismissDelay / 1000}s
                        </p>
                    )}
                </div>
            </Modal.Content>
        </Modal>
    );
}
