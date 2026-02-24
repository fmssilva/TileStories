/**
 * VALIDATION UTILITIES
 * ====================
 * 
 * Form validation functions for user inputs
 * 
 * Features:
 * - Email validation with comprehensive regex
 * - Bilingual error messages
 * - Type-safe validation results
 */

export interface ValidationResult {
    isValid: boolean;
    error?: string;
}

/**
 * Validate email address format
 * 
 * Uses comprehensive regex that checks for:
 * - Local part (before @) with valid characters
 * - Domain with valid structure
 * - TLD with minimum 2 characters
 * 
 * @param email - Email address to validate
 * @param language - Language for error message ('pt' or 'en')
 * @returns Validation result with error message if invalid
 */
export function validateEmail(email: string, language: 'pt' | 'en' = 'en'): ValidationResult {
    // Check if email is empty
    if (!email || email.trim() === '') {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'O email é obrigatório.'
                : 'Email is required.'
        };
    }

    // Comprehensive email regex
    // Pattern explanation:
    // ^[a-zA-Z0-9._%+-]+ - Local part: letters, numbers, and special chars
    // @ - Required @ symbol
    // [a-zA-Z0-9.-]+ - Domain: letters, numbers, dots, hyphens
    // \. - Required dot before TLD
    // [a-zA-Z]{2,}$ - TLD: at least 2 letters
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (!emailRegex.test(email.trim())) {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'Por favor, insira um endereço de email válido.'
                : 'Please enter a valid email address.'
        };
    }

    // Additional checks for common mistakes
    const trimmedEmail = email.trim();

    // Check for double @ symbols
    if ((trimmedEmail.match(/@/g) || []).length !== 1) {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'Email inválido: deve conter apenas um símbolo @.'
                : 'Invalid email: must contain only one @ symbol.'
        };
    }

    // Check for spaces
    if (trimmedEmail.includes(' ')) {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'Email inválido: não pode conter espaços.'
                : 'Invalid email: cannot contain spaces.'
        };
    }

    // Check minimum length (e.g., a@b.co = 6 chars)
    if (trimmedEmail.length < 6) {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'Email muito curto.'
                : 'Email is too short.'
        };
    }

    // Check maximum length (RFC 5321 standard)
    if (trimmedEmail.length > 254) {
        return {
            isValid: false,
            error: language === 'pt'
                ? 'Email muito longo.'
                : 'Email is too long.'
        };
    }

    return {
        isValid: true
    };
}

/**
 * Validate required text field
 * 
 * @param value - Text value to validate
 * @param fieldName - Name of field for error message
 * @param language - Language for error message
 * @param minLength - Minimum length (optional)
 * @returns Validation result
 */
export function validateRequired(
    value: string,
    fieldName: string,
    language: 'pt' | 'en' = 'en',
    minLength?: number
): ValidationResult {
    if (!value || value.trim() === '') {
        return {
            isValid: false,
            error: language === 'pt'
                ? `${fieldName} é obrigatório.`
                : `${fieldName} is required.`
        };
    }

    if (minLength && value.trim().length < minLength) {
        return {
            isValid: false,
            error: language === 'pt'
                ? `${fieldName} deve ter pelo menos ${minLength} caracteres.`
                : `${fieldName} must be at least ${minLength} characters.`
        };
    }

    return {
        isValid: true
    };
}
