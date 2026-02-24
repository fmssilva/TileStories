/**
 * WORK PLAN DOMAIN - BARREL EXPORTS
 * ==================================
 * 
 * Central export file for all work plan page components and utilities.
 * Following the domain-driven architecture pattern.
 */

// Main page component
export { default as WorkPlanPage } from './WorkPlanPage';

// Section components
export { default as WorkPlanHero } from './WorkPlanHero';
export { default as WorkPlanOverview } from './WorkPlanOverview';
export { default as WorkPlanTimeline } from './WorkPlanTimeline';
export { default as PhaseTabsView } from './PhaseTabsView';
export { default as PhaseDetailLayout } from './PhaseDetailLayout';
export { default as Phase1Detail } from './Phase1Detail';
export { default as Phase2Detail } from './Phase2Detail';
export { default as Phase3Detail } from './Phase3Detail';
export { default as Phase4Detail } from './Phase4Detail';
export { default as FinancialBreakdown } from './FinancialBreakdown';
export { default as RiskMitigation } from './RiskMitigation';
export { default as InvestorCTA } from './InvestorCTA';
export { default as CostDetailModal } from './CostDetailModal';

// Types
export type { Phase, TechStackItem, Milestone, CostCategory } from './types';
export type { CostDetail } from './costDetails';

// Utilities
export { phases, costBreakdown, getTotalCost, getPhaseById, projectMetadata } from './utils';
export { getCostDetail, getCostDetailsByCategory } from './costDetails';

// Colors
export { getPhaseColor, getPhaseGradient, workPlanColors } from './colors';

