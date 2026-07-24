🏆 Ranked List

poolside/laguna-m.1:free
nvidia/nemotron-3-ultra-550b-a55b:free
nvidia/nemotron-3-super-120b-a12b:free


# **Tier 1 - Free (Best for Architecture, Agents & MCP)**

1. poolside/laguna-m.1:free 
   🥇(Best Overall Coding Agent)
    Context Window: 262,144 tokens
    Role: Lead Autonomous Software Engineer
    Why it’s #1: Laguna M.1 was trained specifically by Poolside for agentic coding harnesses (like SWE-Agent and OpenCode). It has native awareness of tool-calling schemas, rarely breaks Cline’s XML formatting, and executes Unity MCP commands (e.g., inspecting scenes, creating GameObjects, reading logs) with near zero syntax failures.

2. nvidia/nemotron-3-ultra-550b-a55b:free 
   🥈 (Best System Architect)
    Context Window: 1,000,000 tokens
    Role: System Architect & Deep Project Debugger
    Why it’s #2: Built on a 550B Mixture-of-Experts (MoE) architecture, this is the largest model on your list. Its massive 1M token context allows you to feed your entire Unity project directory, C# class hierarchy, and detailed .clinerules into memory. It excels at high-level reasoning, dependency planning, and solving complex multi-script bugs.

3. google/gemma-4-31b-it:free 
   🥉 (Best Multimodal / Vision Model)
    Context Window: 262,144 tokens
    Role: UI/Visual Debugger & Feature Implementer
    Why it’s #3: Scoring 43.4% on the Artificial Analysis Coding Index, this 31B model features native vision capabilities. If you need Cline to analyze a screenshot of a Unity Inspector error, a UI layout, or a visual bug, Gemma 4 31B is the best free model in your list that can "see" images and output clean code.





# **Tier 2 - Free (Fast Execution & Planning)**
1. nvidia/nemotron-3-super-120b-a12b:free   
    Context Window: 1,000,000 tokens
    Role: Fast Architectural Planning
    Why: A lighter, lower-latency 120B MoE sibling to Nemotron Ultra. It keeps the huge 1M token context window while responding significantly faster for planning and multi-step reasoning.

1. poolside/laguna-xs-2.1:free
    Context Window: 262,144 tokens
    Role: Rapid Single-File Edits & C# Scripting
    Why: A compact version of Laguna M.1 designed for fast execution. Perfect when you just need quick C# function refactoring or rapid bug fixes without long planning phases.

1. cohere/north-mini-code:free
    Context Window: 256,000 tokens
    Role: Terminal & Script Execution
    Why: Built by Cohere specifically for agent harnesses. It boasts an extremely low tool-call error rate (1.92%), making it solid for Unity MCP interactions, though its smaller 3B active parameter count limits deep architectural planning.




# **"Best Value" Paid Models**

1. deepseek/deepseek-chat
   Role: The Everyday Senior Architect (Best Overall Value)  
   Cost: ~$0.20 Input / ~$0.80 Output per 1M tokens  
   Context: 164K tokens
   Why use it: This is the base DeepSeek-V3 engine. It costs virtually nothing (a full project review costs pennies) and rivals top-tier models for code sanity checks, DRY principles, and spotting bad patterns.

2. deepseek/deepseek-r1
   Role: The Strict Principal Architect (Best for Complex Logic)
   Cost: ~$0.70 Input / ~$2.50 Output per 1M tokens  
   Context: 164K tokens
   Why use it: R1 uses chain-of-thought reasoning before outputting. It is the single best cheap model to catch subtle state bugs, broken dependencies, or deep design flaws across multiple files.
   
3. deepseek/deepseek-v4-pro
    Role: The Upgraded Code & Agent Specialist
    Cost: ~$0.43 Input / ~$0.87 Output per 1M tokens
    Context: 1M tokens
    Why use it: An upgraded DeepSeek architecture designed with a massive 1-million-token context window and high benchmark scores for software engineering agents. Ideal when you want DeepSeek's quality across an entire repository without context truncations.

3. moonshotai/kimi-k2.6
   Role: The Large-Codebase Specialist  
   Cost: ~$0.66 Input / ~$3.41 Output per 1M tokens  
   Why use it: With its 262K token context window and agent-oriented design, Kimi K2.6 handles massive amounts of file context without losing attention. Ideal if your codebase is large.  

4. qwen/qwen-2.5-coder-32b-instruct
   Role: The Budget Pragmatist  
   Cost: ~$0.66 Input / ~$1.00 Output per 1M tokens  
   Why use it: One of the most benchmarked open coding models. Highly reliable for verifying refactoring plans and standard structural conventions.  