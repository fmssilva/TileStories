# SKILL: End-of-chat process review

1. **Name:** "revê o processo deste chat" -- extract reusable process lessons from one long chat.
2. **Trigger:** the end of a long or bumpy chat. Input: the chat itself and the guides available in it. If the guides are not
   available, mark comparisons as "nao verificavel" instead of guessing.
3. **Procedure:** sections 1-6 below.
4. **Result:** the five-part answer of section 7, ending with the fixed closing sentence; proposals only, nothing applied.
   Few strong changes over many weak ones; "no change recommended" is a valid result.

# END-OF-CHAT — PROCESS LEARNING REVIEW
Analisa este chat exclusivamente como uma revisão do processo de trabalho entre utilizador e agente.
O objetivo é extrair melhorias reutilizáveis para futuras tarefas, não avaliar nem resumir o conteúdo da tarefa realizada.
## 1. OBJETIVO
Identifica apenas lessons learned generalizáveis que possam tornar futuras interações mais robustas, previsíveis, eficientes e fáceis de validar.
Não assumas que qualquer recomendação será adotada: apresenta propostas para decisão posterior.
O objetivo não é aumentar o número de regras, mas melhorar o sistema com o mínimo de complexidade adicional necessário.
## 2. ÂMBITO E EXCLUSÕES
Não faças histórico cronológico do chat.
Não faças resumo das tarefas, funcionalidades, entregáveis ou decisões específicas.
Não listes simplesmente o que foi feito.
Não transformes detalhes específicos deste projeto numa regra global sem justificação.
Não cries uma nova regra apenas porque ocorreu um erro isolado, exceto quando esse erro revela uma lacuna importante ou uma classe de erros que vale a pena prevenir.
Não recomendes alterações que apenas reformulem uma guideline existente sem acrescentar clareza, cobertura ou operacionalidade.
Não inventes aprendizagens, evidência, recorrência ou problemas que não estejam suportados pelo chat.
Usa evidência concreta do chat para justificar as conclusões, mas descreve-a de forma breve e abstraída, sem transformar a resposta num resumo da conversa.
## 3. O QUE PROCURAR
Procura sobretudo erros, omissões ou ambiguidades evitáveis por uma guideline.
Procura comportamentos do agente que exigiram correção ou redirecionamento.
Procura pressupostos indevidos ou situações em que deveria ter sido pedido ou confirmado contexto antes de agir.
Procura validações, pesquisas, testes, inspeções ou revisões que deveriam ter ocorrido obrigatoriamente ou numa ordem diferente.
Procura decisões de processo que tenham melhorado claramente o resultado.
Procura procedimentos repetíveis que possam justificar uma skill, checklist ou workflow.
Procura guidelines existentes que estejam vagas, redundantes, em conflito, demasiado específicas ou difíceis de executar.
Distingue sempre entre erro pontual, melhoria útil, padrão recorrente e princípio geral.
## 4. CRITÉRIO DE INCLUSÃO
Uma aprendizagem deve ser recomendada para alteração do sistema global quando existir evidência suficiente de pelo menos uma destas condições: ocorreu mais do que uma vez ou exigiu correção repetida; revelou uma falha com impacto relevante; revelou uma lacuna clara numa regra existente; evita uma classe de erros semelhantes; define um procedimento repetível com benefício claro.
Uma única ocorrência pode ser suficiente quando revelar uma falha estrutural, de segurança, validação ou processo com impacto material; nesse caso explica explicitamente porque merece generalização.
Uma única ocorrência também pode ser suficiente quando é "corrigida de forma clara e direta" pelo user, por isso confirma bem todos os comandos que eu dei ao longo do chat e tenta perceber, o que foi uma simples tarefa vs o que poderia ser uma guideline geral a aplicar nas tarefas futuras?  
Se a evidência for insuficiente, coloca a aprendizagem em “O que não deve mudar” em vez de a promover a regra global.
## 5. FONTE DE VERDADE E COMPARAÇÃO
Compara as aprendizagens com as guidelines, skills, workflows e templates que estejam efetivamente disponíveis no contexto deste chat.
Não assumes que um ficheiro, regra ou guideline existe se não estiver disponível ou não tiver sido fornecido.
Se os materiais necessários para a comparação não estiverem acessíveis, declara explicitamente “não verificável” e não inventes a comparação.
Para cada potencial alteração, avalia se já existe uma regra equivalente, se acrescenta algo concreto ou é apenas reformulação, se pode conflituar com regras existentes, se é suficientemente geral para reutilização futura e se reduz erros ou custo de coordenação de forma material sem acrescentar complexidade desnecessária.
Quando uma regra existente deve ser modificada, indica claramente qual regra ou secção deve ser substituída, fundida ou reforçada.
## 6. TAXONOMIA
Classifica cada aprendizagem numa destas categorias: GUIDELINE — regra transversal para futuras tarefas.
SKILL — procedimento reutilizável para um tipo de tarefa.
WORKFLOW — melhoria na sequência de trabalho, validação ou tomada de decisão.
PROMPT/TEMPLATE — melhoria reutilizável de instruções, mas sem justificar uma regra ou skill permanente.
LOCAL / NÃO GENERALIZÁVEL — válida neste chat, mas não deve entrar no sistema global.
Usa “LOCAL / NÃO GENERALIZÁVEL” apenas na secção própria e nunca como recomendação de alteração global.
## 7. FORMATO DA RESPOSTA
### 1. Lessons learned relevantes
Inclui apenas aprendizagens suficientemente generalizáveis.
Para cada item indica: Problema observado; Aprendizagem; Classificação; Evidência no chat; Porque é reutilizável; Avaliação face às guidelines atuais: já existe / parcial / não existe / não verificável.
### 2. Alterações recomendadas
Inclui apenas alterações com valor claro.
Para cada uma indica: Tipo — ADD / MODIFY / REMOVE-MERGE; Alvo — guideline, secção ou área afetada; Proposta — redação curta, clara e operacional; Justificação — problema resolvido e valor acrescentado; Risco ou conflito — nenhum identificado ou descrição do conflito; Prioridade — alta / média / baixa.
Se for MODIFY, indica qual regra existente deve ser alterada e como.
Se for REMOVE-MERGE, indica que regras ou ideias devem ser consolidadas.
### 3. Skills ou workflows a criar
Só propõe uma skill ou workflow se existir um procedimento repetível, distinto de uma simples guideline e suficientemente valioso para justificar uma abstração própria.
Para cada proposta indica: nome; quando usar; gatilho de ativação; passos essenciais; resultado esperado; razão para não ser apenas uma guideline.
### 4. O que não deve mudar
Inclui observações interessantes que não justificam alterar o sistema global.
Para cada item indica: observação; motivo para não generalizar; classificação — erro pontual / detalhe local / evidência insuficiente / já coberto por regra existente.
### 5. Alterações consolidadas propostas
Termina com uma lista curta, deduplicada e acionável contendo apenas guidelines a adicionar, modificar, remover ou consolidar; skills/workflows a criar; templates a atualizar.
Não repitas aqui explicações longas já apresentadas nas secções anteriores.
Não apresentes estas propostas como aprovadas.
Termina exatamente com: “Estas são propostas de alteração; aguardam validação antes de serem incorporadas nas guidelines.”
## 8. PRINCÍPIO DE QUALIDADE
Prefere poucas alterações fortes a muitas alterações fracas.
Uma boa aprendizagem deve ser específica o suficiente para orientar comportamento futuro, mas geral o suficiente para não ficar presa a uma única tarefa.
Não transformes preferências contextuais, decisões arquiteturais específicas ou detalhes de implementação numa regra global sem evidência de reutilização.
Se não houver nenhuma aprendizagem suficientemente forte, diz explicitamente que não há alteração recomendada.
