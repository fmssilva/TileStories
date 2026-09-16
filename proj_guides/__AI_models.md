# `Free Models`
Best Strategy for your C# / Unity Project in Cline:

**Plan - Refactoring Steps - Architect** 
  - LongCat 2.0 or Nemotron-3 Ultra
  - 
**Act - Engineer**
  - DeepSeek V4 Flash or Laguna S 2.1 

**Details**
1. DeepSeek V4 Flash (Top Overall Tier)
Role Fit: Primary Software Engineer & Rapid Debugger.

Why it ranks #1: Built with an efficient Mixture-of-Experts (284B total / 13B active) and a massive 1M-token context window, it hits an impressive ~79.0% on SWE-bench Verified. It is fast, highly accurate at syntax fixes, and handles multi-file C# agentic edit loops in Cline without hallucinating invalid API calls.

2. Laguna S 2.1 (by Poolside)
Role Fit: Primary Software Engineer & Code Refactoring Specialist.

Why it ranks #2: Trained strictly in real RL software engineering environments, it hits 70.2% on Terminal-Bench 2.1 and 59.4% on SWE-Bench Pro. It excels at multi-file C# execution, navigating command terminal outputs, and fixing compilation errors, though its raw multi-domain reasoning sits just behind DeepSeek.

3. LongCat 2.0
Role Fit: Lead Architect (System Design & Multi-Script Planning).

Why it ranks #3: A massive 1.6-trillion parameter MoE model with a 1M context window. It outperforms for high-level repository mapping and breaking down complex C# architectures into logical task lists. However, its token generation speed is slower (47.6 tok/s), making it better for planning than rapid, iterative coding loops.

4. NVIDIA Nemotron-3 Ultra 550B
Role Fit: Secondary Architect & Deep Code Reviewer.

Why it ranks #4: A hybrid Mamba-Transformer MoE model with 550B total / 55B active parameters. It has strong high-level reasoning and function-calling capabilities, but real-world agent benchmarks show noticeable latency spikes and higher tool-use error rates compared to dedicated coding models like Laguna or DeepSeek.

5. Muse Spark 1.3 Contributor
Role Fit: Multimodal Visual Architect (UI/Unity Scene Inspector Analysis).

Why it ranks #5: Meta’s native multimodal reasoning model is great if you pass screenshots of Unity Inspector components, UI layouts, or node graphs alongside your code tasks. For pure C# code generation and complex debugging, however, it lacks the raw coding-agent focus of the top four.

6. GLM-5.3-Flash
Role Fit: Quick Helper / Light Script Assistant.

Why it ranks #6: A lightweight multimodal flash model from the GLM family. It provides rapid response times for basic C# script creation and small single-file functions, but struggles with large context maintenance across a heavy, multi-directory C# repo.

7. Solar Pro 4
Role Fit: Office Productivity & Simple Scripting.

Why it ranks #7: While strong at general document-intensive tasks and simple code snippets, it is not optimized for autonomous, multi-file agentic tool calling inside extensions like Cline.



# More complete list

## `Architect & Reviewer Models`
*Prioritizes repository-wide comprehension, multi-file planning, refactoring logic, and review accuracy.*

### 1. anthropic/claude-sonnet-5
* **Pricing Tier:** Paid ($2.00 input / $10.00 output per 1M tokens)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Top-tier software engineering intelligence, strict constraint adherence, zero hallucination on API boundaries, and industry-leading code review accuracy.
  * **Bad:** Higher price point; expensive if used repeatedly for minor edits instead of high-level planning.

### 1.1 Cursor Grok 4.6 (Free in Cursor)
* **Explanation** Top-tier agent for executing architectural plans directly inside the IDE. Highly recommended for multi-file workspace edits, agentic refactoring, and complex C# / Unity state logic when free credits are available### 2. google/gemini-3.7-flash
* **Pricing Tier:** Paid ($0.375 input / $1.875 output per 1M tokens)
* **Context Size:** 1.05M tokens
* **Concise Explanation:**
  * **Good:** Exceptional price-to-performance ratio; massive context window allows scanning entire multi-module repositories in a single pass at low cost.
  * **Bad:** Slightly less rigorous than Claude Sonnet on edge-case architectural logic and complex multi-step debugging.

### 3. deepseek/deepseek-v4-pro-0813
* **Pricing Tier:** Paid ($0.43 input / $0.87 output per 1M tokens)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Deep reasoning capabilities for complex backend architectures, dependency resolution, and multi-file refactoring.
  * **Bad:** Provider routing latency on OpenRouter can fluctuate depending on time-of-day traffic.
  
### 3. poolside/laguna-s-2.1
* **Pricing Tier:** Paid ($0.09 input / $0.18 output per 1M tokens)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Ultra-cheap ($0.09/M input) with a full 1M context window; built specifically for agentic software workflows and repository mapping.
  * **Bad:** Specialized for code and agentic loops, making it less versatile for general non-coding tasks compared to Sonnet or Gemini.

### 5. moonshotai/kimi-k2.7-code
* **Pricing Tier:** Paid ($0.67 input / $3.40 output per 1M tokens)
* **Context Size:** 262K tokens
* **Concise Explanation:**
  * **Good:** Highly specialized in isolated module analysis, deep code logic, and precise refactoring plans.
  * **Bad:** Context window (262K) is smaller than 1M+ competitors, making full-repo ingestion impossible.

### 6. poolside/laguna-s-2.1:free
* **Pricing Tier:** FREE ($0.00 / $0.00)
* **Context Size:** 262K tokens
* **Concise Explanation:**
  * **Good:** Best zero-cost option for localized sub-task planning and generating task specs without consuming paid credits.
  * **Bad:** Capped at 262K context (vs. 1M on paid) and susceptible to rate limits (429 errors) during high-demand windows.

### 7. deepseek/deepseek-v4-flash:free
* **Pricing Tier:** FREE ($0.00 / $0.00)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Maintains a massive 1M context window at zero cost; handles high-level repository mapping and planning without paying for API calls.
  * **Bad:** Subject to free-tier rate limits (429 errors) and queue delay during peak traffic; weaker constraint-following for fine-grained multi-step refactoring plans compared to paid claude-sonnet-5 or gemini-3.7-flash.

### 8. nvidia/nemotron-3-super:free
* **Pricing Tier:** FREE ($0.00 / $0.00)
* **Context Size:** 262K tokens
* **Concise Explanation:**
  * **Good:** 120B parameter MoE architecture provides strong baseline reasoning for structural analysis at zero cost.
  * **Bad:** High latency on free routes and prone to timeouts during peak server load.

### 9. anthropic/claude-opus-5
* **Pricing Tier:** Paid ($5.00 input / $25.00 output per 1M tokens)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Unmatched reasoning for critical security audits, payments integration, and complex concurrency bugs.
  * **Bad:** Prohibitively expensive ($25/M output) for daily use; strictly reserved as an escalation model.

---

## `Executioner Models`
*Prioritizes tool-calling stability (file edits, terminal execution), syntax precision, speed, and cost per test/fix loop.*

### 1. poolside/laguna-s-2.1:free
* **Pricing Tier:** FREE ($0.00 / $0.00)
* **Context Size:** 262K tokens
* **Concise Explanation:**
  * **Good:** #1 free execution model; 70.2% on Terminal-Bench 2.1 makes it unmatched for zero-cost file edits and test execution.
  * **Bad:** Capped at 262K context; no structured `response_format` support; free endpoints carry rate-limiting risks.

### 2. kwaipilot/kat-coder-air-v2.5
* **Pricing Tier:** Paid ($0.15 input / $0.60 output per 1M tokens)
* **Context Size:** 256K tokens
* **Concise Explanation:**
  * **Good:** #1 overall paid executor; unmatched editing stability on tool calls without dropping code blocks, breaking syntax, or failing string replacements.
  * **Bad:** 256K context requires periodic context pruning when running long agent threads with heavy terminal logs.

### 3. poolside/laguna-s-2.1
* **Pricing Tier:** Paid ($0.09 input / $0.18 output per 1M tokens)
* **Context Size:** 1.0M tokens
* **Concise Explanation:**
  * **Good:** Combines precise file editing with a 1M context window at ultra-cheap rates ($0.09/M input); easily ingests huge test and build logs.
  * **Bad:** Requires paid API credits (though significantly cheaper than general frontier models).

### 4. deepseek/deepseek-v4-flash-latest
* **Pricing Tier:** Paid ($0.065 input / $0.14 output per 1M tokens)
* **Context Size:** 1.31M tokens
* **Concise Explanation:**
  * **Good:** Cheapest paid model ($0.065/M input) with a massive 1.31M context window; ideal for fast, high-volume test/fix loops without free-tier rate limits.
  * **Bad:** Requires slightly higher tool-use retry rates compared to KAT-Coder-Air or Laguna on multi-file modifications.

### 5. google/gemini-3.7-flash
* **Pricing Tier:** Paid ($0.375 input / $1.875 output per 1M tokens)
* **Context Size:** 1.05M tokens
* **Concise Explanation:**
  * **Good:** Reliable execution fallback when tasks require holding large sections of a repository along with long build outputs.
  * **Bad:** Output token pricing ($1.875/M) is higher than dedicated lightweight execution models.

### 6. bytedance/seed-2.0-code
* **Pricing Tier:** Paid ($0.50 input / $3.00 output per 1M tokens)
* **Context Size:** 262K tokens
* **Concise Explanation:**
  * **Good:** High agentic capability when an execution task involves updating 3–5 interconnected files simultaneously.
  * **Bad:** Output pricing ($3.00/M) is higher than other specialized execution options.

### 7. cohere/north-mini-code:free
* **Pricing Tier:** FREE ($0.00 / $0.00)
* **Context Size:** 256K tokens
* **Concise Explanation:**
  * **Good:** Fast response times and low initial latency for basic, single-file code edits.
  * **Bad:** Lower overall accuracy on complex terminal tasks; can get stuck in tool-call error loops.



