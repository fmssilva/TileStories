/**
 * NAVIGATION CONFIGURATION
 * ========================
 * 
 * Single source of truth for all navigation structure.
 * This config drives:
 * - Routes generation
 * - Navigation menus
 * - Breadcrumbs
 * - Mobile menu
 * 
 * Add new pages here and they automatically appear everywhere.
 */

import { lazy } from 'react';
import type { NavItem } from '../types';

// ============================================================================
// LAZY-LOADED COMPONENTS
// ============================================================================

const HomePage = lazy(() => import('@/domains/home/HomePage').then(m => ({ default: m.HomePage })));
const WorkPlanPage = lazy(() => import('@/domains/workPlan').then(m => ({ default: m.WorkPlanPage })));
const ContactPage = lazy(() => import('@/domains/contact').then(m => ({ default: m.ContactPage })));
const NotFoundPage = lazy(() => import('@/components/feedback').then(m => ({ default: m.NotFound })));

// ============================================================================
// NAVIGATION STRUCTURE
// ============================================================================

/**
 * Main navigation configuration
 * 
 * Structure determines:
 * - URL paths
 * - Navigation menu items
 * - Breadcrumb hierarchy
 * - Page components
 * 
 * To add a new page:
 * 1. Import component (lazy loaded)
 * 2. Add NavItem to this array
 * 3. That's it! Routes, nav, breadcrumbs auto-update
 */
export const navigationConfig: NavItem[] = [
    {
        id: 'home',
        label: { pt: 'Início', en: 'Home' },
        labelShort: { pt: 'Início', en: 'Home' },
        path: '/',
        component: HomePage,
        styleLevel: 'top',
        metadata: {
            showInNav: true,
            showInBreadcrumb: true,
            order: 1,
            preserveScroll: false,
        }
    },
    {
        id: 'work-plan',
        label: { pt: 'Plano de Trabalho', en: 'Work Plan' },
        labelShort: { pt: 'Plano Trabalho', en: 'Work Plan' },
        path: '/work-plan',
        component: WorkPlanPage,
        styleLevel: 'top',
        metadata: {
            showInNav: true,
            showInBreadcrumb: true,
            order: 2,
            preserveScroll: false,
        }
    },
    {
        id: 'contact',
        label: { pt: 'Contacto', en: 'Contact' },
        labelShort: { pt: 'Contacto', en: 'Contact' },
        path: '/contact',
        component: ContactPage,
        styleLevel: 'top',
        metadata: {
            showInNav: true,
            showInBreadcrumb: true,
            order: 3,
            preserveScroll: false,
        }
    },
    // 404 - Special case, not in main nav
    {
        id: '404',
        label: { pt: 'Página Não Encontrada', en: 'Page Not Found' },
        labelShort: { pt: '404', en: '404' },
        path: '*',
        component: NotFoundPage,
        styleLevel: 'inner',
        metadata: {
            showInNav: false,
            showInBreadcrumb: false,
        }
    }
];
