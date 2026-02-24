/**
 * MUSEUM & PARTNERSHIP CONFIGURATION
 * ===================================
 * 
 * Single source of truth for museum and partnership information
 * Used across ContactPage, CTASection, Footer, and other components
 * 
 * This ensures consistent branding and easy updates
 */

export const museumConfig = {
    /**
     * Museum Name
     */
    name: {
        pt: 'Museu Nacional do Azulejo',
        en: 'National Tile Museum'
    },

    /**
     * Museum Location
     */
    location: {
        city: 'Lisboa',
        country: 'Portugal',
        fullAddress: {
            pt: 'Rua Madre de Deus, 4, 1900-312 Lisboa, Portugal',
            en: 'Rua Madre de Deus, 4, 1900-312 Lisboa, Portugal'
        }
    },

    /**
     * Official Museum Website
     */
    website: 'https://www.museunacionaldoazulejo.pt/',

    /**
     * Google Maps Location Link
     */
    googleMapsUrl: 'https://maps.app.goo.gl/8nL5u3J2KWNZF17Y9',

    /**
     * Google Arts & Culture - Grande Panorama
     */
    googleArtsUrl: 'https://artsandculture.google.com/story/gAWhceMYFOAfIA?hl=pt-PT',

    /**
     * Google Arts & Culture Title
     */
    googleArtsTitle: {
        pt: 'Grande Panorama de Lisboa',
        en: 'Grande Panorama de Lisboa'
    },

    /**
     * Museum Logo Image Path
     */
    logoPath: '/logo_museu_nac_azulejo.png',

    /**
     * Museum Role in Project
     */
    role: {
        pt: 'Parceiro Principal do Projeto',
        en: 'Main Project Partner'
    }
} as const;

/**
 * University Partnership Configuration
 */
export const universityConfig = {
    /**
     * University Name
     */
    name: {
        pt: 'FCT NOVA - Faculdade de Ciências e Tecnologia',
        en: 'FCT NOVA - Faculty of Sciences and Technology'
    },

    /**
     * Full Name
     */
    fullName: {
        pt: 'Faculdade de Ciências e Tecnologia da Universidade Nova de Lisboa',
        en: 'Faculty of Sciences and Technology, NOVA University Lisbon'
    },

    /**
     * University Website
     */
    website: 'https://www.fct.unl.pt/',

    /**
     * Logo Paths (light and dark mode)
     */
    logo: {
        light: '/FCT_logo_light.png',
        dark: '/FCT_logo_dark.png'
    }
} as const;

/**
 * Contact Information
 */
export const contactConfig = {
    /**
     * Project Lead Information
     */
    projectLead: {
        name: 'Francisco Silva',
        role: {
            pt: 'Líder do Projeto',
            en: 'Project Lead'
        }
    },

    /**
     * Contact Email
     */
    email: 'fmso.silva@campus.fct.unl.pt',

    /**
     * Institution
     */
    institution: {
        pt: 'FCT NOVA',
        en: 'FCT NOVA'
    },

    /**
     * Response Time
     */
    responseTime: {
        pt: '24-48 horas',
        en: '24-48 hours'
    }
} as const;

/**
 * Project Metadata
 */
export const projectConfig = {
    /**
     * Project Type
     */
    type: {
        pt: 'Projeto de tese de mestrado',
        en: 'Master\'s thesis project'
    },

    /**
     * Collaboration Statement
     */
    collaborationText: {
        pt: 'Projeto em colaboração com:',
        en: 'Project in collaboration with:'
    }
} as const;

/**
 * Helper function to get localized text
 */
export function getLocalizedText<T extends { pt: string; en: string }>(
    text: T,
    language: 'pt' | 'en'
): string {
    return text[language];
}
