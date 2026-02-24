/**
 * USE BREADCRUMBS HOOK
 * ====================
 * 
 * Access breadcrumb trail for current page.
 * Automatically generated from navigation hierarchy.
 */

import { useNavigationContext } from '../context';
import type { NavItem } from '../types';

/**
 * Use Breadcrumbs
 * 
 * Returns breadcrumb trail for current page.
 * Trail is automatically built from navigation hierarchy.
 * 
 * @example
 * const breadcrumbs = useBreadcrumbs();
 * 
 * breadcrumbs.forEach(item => {
 *   console.log(item.label, item.path);
 * });
 */
export function useBreadcrumbs(): NavItem[] {
    const { breadcrumbTrail } = useNavigationContext();
    return breadcrumbTrail;
}
