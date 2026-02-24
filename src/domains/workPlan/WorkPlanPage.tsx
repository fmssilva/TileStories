/**
 * WORK PLAN PAGE - Main Container
 * ================================
 * 
 * Investor-facing page showing the 12-month development roadmap
 * with 4 phases, cost breakdown (€3,500), and deliverables.
 * 
 * Layout: Hero → Overview → Tabbed Phases → Financial → Risk → CTA
 */

import WorkPlanHero from './WorkPlanHero';
import WorkPlanOverview from './WorkPlanOverview';
import PhaseTabsView from './PhaseTabsView';
import FinancialBreakdown from './FinancialBreakdown';
import RiskMitigation from './RiskMitigation';
import InvestorCTA from './InvestorCTA';

export function WorkPlanPage() {
    return (
        <div className="work-plan-page min-h-screen">
            <WorkPlanHero />
            <WorkPlanOverview />
            <PhaseTabsView />
            <FinancialBreakdown />
            <RiskMitigation />
            <InvestorCTA />
        </div>
    );
}

export default WorkPlanPage;

