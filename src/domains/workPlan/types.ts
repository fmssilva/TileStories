/**
 * WORK PLAN DOMAIN - TYPE DEFINITIONS
 * ====================================
 * 
 * Type definitions for the Work Plan page showing the 12-month development
 * roadmap with 4 phases, costs breakdown, and deliverables.
 * 
 * Data source: App_Plan.md
 */

/**
 * Technology stack item with name and cost
 */
export interface TechStackItem {
    name: string;
    cost: number; // in euros
}

/**
 * Project milestone with month and bilingual description
 */
export interface Milestone {
    month: number; // 1-12
    description_pt: string;
    description_en: string;
}

/**
 * Development phase (1-4) with all details
 */
export interface Phase {
    id: 1 | 2 | 3 | 4;
    title_pt: string;
    title_en: string;
    subtitle_pt: string;
    subtitle_en: string;
    months: string; // e.g., "1-3"
    cost: number; // in euros
    deliverables_pt: string[];
    deliverables_en: string[];
    value_pt: string; // Value proposition for visitor
    value_en: string;
    techStack: TechStackItem[];
    milestones: Milestone[];
}

/**
 * Cost breakdown category across all phases
 */
export interface CostCategory {
    category_pt: string;
    category_en: string;
    phase1: number;
    phase2: number;
    phase3: number;
    phase4: number;
    total: number;
}
