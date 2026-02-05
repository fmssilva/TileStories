# Plano de Execução
## PLANO EXECUTIVO COMPLETO - TESE 1 ANO (12 MESES)
App AR para Grande Panorama de Lisboa - Museu Nacional do Azulejo

### VALIDAÇÃO DAS NOTAS ANTIGAS
#### ✅ Ainda Válido:
1. Grande Panorama Lisboa: 
    - 23 metros comprimento ✅
    - Gabriel del Barco (~1700) ✅
    - Pré-terramoto 1755 ✅
    - 14km costa representados ✅
    - 100-150+ edifícios identificáveis ✅
2. Tracking multi-escala: ✅ Essencial e tecnicamente viável
3. Timeline temporal (4 épocas): ✅ Core feature confirmada
4. Simulação terramoto: ✅ Killer feature, válida
5. Guia com áudio: ✅ Confirmado como prioritário
6. Perfis académicos: ✅ Inovação forte para tese
7. Gamificação: ✅ Engagement comprovado
8. 360° views: ✅ Tecnicamente possível
9. Circuitos temáticos: ✅ Essencial para diferentes públicos

#### ⚠️ Precisa Atualização:
1. VR googles: Deixar para Fase 3 (após tese) - complexidade alta
2. Sensores físicos: Desnecessário, app mobile suficiente
3. Multiplayer AR: Muito complexo, low priority

#### ❌ Não Priorizar:
1. Chão interativo LED: Infraestrutura cara, fora do scope tese
2. Iluminação cenográfica sala: Requer parceria museu complexa
3. Body pose interaction: Gimmick, não adiciona valor científico

### MATRIZ DE PRIORIZAÇÃO DE FEATURES
| Feature                 | Interesse | Facilidade | Valor Tese | Custo   | Prioridade |
| ----------------------- | --------- | ---------- | ---------- | ------- | ---------- |
| Tracking multi-escala   | ⭐⭐⭐⭐⭐     | ⭐⭐⭐⭐       | ⭐⭐⭐⭐⭐      | €0      | 🥇 P0       |
| Edifícios interativos   | ⭐⭐⭐⭐⭐     | ⭐⭐⭐⭐⭐      | ⭐⭐⭐⭐⭐      | €500    | 🥇 P0       |
| Timeline 4 épocas       | ⭐⭐⭐⭐⭐     | ⭐⭐⭐        | ⭐⭐⭐⭐⭐      | €1k     | 🥇 P0       |
| Guia áudio scriptado    | ⭐⭐⭐⭐⭐     | ⭐⭐⭐⭐       | ⭐⭐⭐⭐       | €500    | 🥇 P0       |
| Perfis académicos       | ⭐⭐⭐⭐⭐     | ⭐⭐⭐        | ⭐⭐⭐⭐⭐      | €0      | 🥇 P0       |
| Google Maps overlay     | ⭐⭐⭐⭐      | ⭐⭐⭐⭐⭐      | ⭐⭐⭐⭐       | €0      | 🥈 P1       |
| Simulação terramoto     | ⭐⭐⭐⭐⭐     | ⭐⭐         | ⭐⭐⭐⭐⭐      | €2k     | 🥈 P1       |
| Circuitos temáticos     | ⭐⭐⭐⭐      | ⭐⭐⭐⭐       | ⭐⭐⭐⭐       | €500    | 🥈 P1       |
| Gamificação badges      | ⭐⭐⭐⭐      | ⭐⭐⭐⭐       | ⭐⭐⭐        | €0      | 🥈 P1       |
| GPT Q&A conversacional  | ⭐⭐⭐⭐      | ⭐⭐⭐        | ⭐⭐⭐⭐       | €50/mês | 🥉 P2       |
| 360° interior edifícios | ⭐⭐⭐⭐      | ⭐⭐         | ⭐⭐⭐        | €1.5k   | 🥉 P2       |
| Did you know? facts     | ⭐⭐⭐⭐      | ⭐⭐⭐⭐⭐      | ⭐⭐⭐        | €0      | 🥉 P2       |
| Analytics heatmap       | ⭐⭐⭐       | ⭐⭐⭐        | ⭐⭐⭐⭐⭐      | €0      | 🥉 P2       |
| 3D models Unity premium | ⭐⭐⭐⭐      | ⭐⭐         | ⭐⭐⭐⭐       | €1.5k   | 🏅 P3       |
| Som ambiente espacial   | ⭐⭐⭐       | ⭐⭐⭐        | ⭐⭐         | €300    | 🏅 P3       |
| Day/night cycle         | ⭐⭐⭐       | ⭐⭐⭐        | ⭐⭐         | €0      | 🏅 P3       |

### ROADMAP COMPLETO 12 MESES
#### 📅 FASE 1: MVP FUNCIONAL (Meses 1-3)
##### MÊS 1 - Setup & Fundações
###### Semana 1-2: Preparação Institucional
- ✅ Formalizar orientador FCT NOVA
- ✅ Contactar Museu Azulejo (email + reunião)
- ✅ Propor MoU (Memorando Entendimento)
- ✅ Obter acesso físico à sala do painel
- ✅ Fotografar painel alta resolução (múltiplos ângulos)
- ✅ Medir sala real (dimensões, iluminação)
- ✅ Testar cobertura rede WiFi/4G no museu ⚠️

**Deliverable:** MoU assinado + Fotos HD painel

###### Semana 3-4: Setup Técnico
- ✅ Instalar Flutter SDK + VS Code + Copilot
- ✅ Setup GitHub repo privado
- ✅ Criar projeto Flutter base
- ✅ Instalar AR plugins (ar_flutter_plugin)
- ✅ Setup Firebase (backend grátis)
- ✅ Candidatar Apple Developer (€99) + Google Play (€25)
- ✅ Tutorial Blender básico (10h - Blender Guru)

**Deliverable:** Ambiente dev completo + primeira build vazia  
**Custos Mês 1:** €124 (stores) + €10 (Copilot)

##### MÊS 2 - MVP Core (AR Básico)
**Objetivo:** App que deteta painel e mostra overlays simples

###### Semana 1-2:
- ✅ Implementar AR tracking (1 reference image do painel completo)
- ✅ Overlay simples: 10 pins 2D sobre edifícios principais
  - Castelo São Jorge
  - Sé de Lisboa
  - Paço da Ribeira (centro)
  - Torre de Belém
  - Jerónimos
  - São Vicente de Fora
  - Convento Carmo
  - Igreja São Roque
  - Terreiro do Paço
  - Madre de Deus (atual museu)
- ✅ Tap no pin → popup com:
  - Nome edifício
  - Texto 2-3 frases
  - Imagem histórica (se existir)
  - Botão "Localizar no Mapa Hoje"

###### Semana 3-4:
- ✅ Google Maps integration
  - Coordenadas GPS de cada edifício
  - Overlay mapa atual sobre painel
  - Botão "Ver no Google Maps"
- ✅ UI básica Flutter:
  - Bottom sheet info
  - Navegação entre pins
  - Botão "Todos os Edifícios" (lista)
- ✅ Teste no museu real (tracking stability)

**Deliverable:** MVP funcional com 10 POI + Maps  
**Teste com:** 5-10 visitantes museu (feedback informal)  
**Custos Mês 2:** €10 (Copilot) + €100 (assets icons)

##### MÊS 3 - Expandir Conteúdo
**Objetivo:** 30 edifícios + Timeline básica

###### Semana 1-2:
- ✅ Adicionar 20 edifícios secundários (total 30)
- ✅ Criar sistema de categorias:
  - 🏛️ Religiosos (igrejas, conventos)
  - 👑 Poder (palácios, castelo)
  - 🏘️ Civil (casas, mercados)
  - 🚢 Marítimo (docas, arsenais)
- ✅ Timeline temporal simples:
  - Slider: 1700 → 1755 → Hoje
  - Edifícios aparecem/desaparecem conforme época
  - Cores overlay indicam: destruído/sobreviveu/modificado

###### Semana 3-4:
- ✅ Sistema de zoom adaptativo:
  - Vista geral (longe): clusters de edifícios
  - Vista média (2-3m): edifícios individuais
  - Vista próxima (<1m): detalhes + botão "Explorar 3D"
- ✅ Pesquisa e documentação histórica:
  - 30 fichas informativas completas
  - Fontes: NOVA FCSH + Museu Azulejo
- ✅ Primeira versão TestFlight (iOS) + Play Internal (Android)

**Deliverable:** App com 30 POI + Timeline + Zoom  
**Teste com:** 20 visitantes (formulário Google Forms feedback)  
**Custos Mês 3:** €10 (Copilot) + €200 (imagens históricas stock)

#### 📅 FASE 2: FEATURES CORE (Meses 4-6)
##### MÊS 4 - Perfis Académicos & Guia Áudio
**Objetivo:** Personalização por domínio + narração

###### Semana 1-2: Sistema de Perfis
- ✅ Onboarding com 2 perguntas:
  1. Área interesse: [Arquitetura / História / Economia / Vida Quotidiana / Militar / Geral]
  2. Escolaridade: [6º-9º / 10º-12º / Superior]
- ✅ Adaptar conteúdo por perfil:
  - ARQUITETURA: foco estilos, proporções, materiais
  - HISTÓRIA: contexto político, eventos, personagens
  - ECONOMIA: comércio, rotas marítimas, riqueza
  - VIDA QUOTIDIANA: profissões, dia-a-dia, costumes
  - MILITAR: defesas, estratégia, batalhas
- ✅ Linguagem adaptada por idade:
  - Básico: frases curtas, emojis, gamificado
  - Secundário: texto médio, factos
  - Superior: académico, fontes citadas

###### Semana 3-4: Guia Áudio
- ✅ 20 clips áudio scriptados (contexto geral):
  - Introdução geral (2min)
  - 10 edifícios principais (1min cada)
  - Transições entre zonas (30s cada)
  - Terramoto introdução (1min)
  - Conclusão (1min)
- ✅ Opção A (budget): TTS Google grátis
  - OU
  - Opção B (quality): Voice actor Fiverr (€250)
- ✅ Áudio espacial: volume adapta conforme proximidade POI
- ✅ Legendas sincronizadas (acessibilidade)

**Deliverable:** App personalizada + áudio guiado  
**Custos Mês 4:** €10 (Copilot) + €250 (voice actor)

##### MÊS 5 - Circuitos Temáticos & Gamificação
**Objetivo:** Percursos guiados + engagement

###### Semana 1-2: Circuitos
- ✅ Criar 5 circuitos (8-12 POI cada, 10-15min):
  1. "Terramoto 1755: Antes & Depois"
      - Edifícios destruídos vs sobreviventes
  2. "Poder Real"
      - Palácios, Terreiro Paço, fortificações
  3. "Lisboa Religiosa"
      - Igrejas, conventos, mosteiros
  4. "Vida na Cidade"
      - Mercados, docas, casas, ofícios
  5. "Aventura Infantil" (6-12 anos)
      - Castelo, piratas, navegadores, tesouros
- ✅ Setas animadas AR overlay guiam percurso
- ✅ Narração específica cada circuito
- ✅ Estimativa tempo + dificuldade

###### Semana 3-4: Gamificação
- ✅ Sistema de progressão:
  - Badge por edifício visitado
  - Conquistas especiais: "Completou circuito X"
  - Contador: "Visitou 15/150 edifícios"
- ✅ Quiz contextual:
  - 3 perguntas por POI importante
  - "Este edifício sobreviveu ao terramoto?"
  - Feedback imediato + explicação
- ✅ Photo moments:
  - Screenshot automático descoberta POI
  - Moldura temática Lisboa
  - Botão share social (Instagram/Facebook)
- ✅ Leaderboard anónimo (opcional)

**Deliverable:** Circuitos funcionais + gamificação  
**Custos Mês 5:** €10 (Copilot)

##### MÊS 6 - Unity Integration (Edifícios Premium)
**Objetivo:** 5 modelos 3D interativos Unity

###### Semana 1: Setup Unity
- ✅ Instalar Unity 2022 LTS
- ✅ Setup projeto Unity (AR Foundation)
- ✅ Integração Flutter-Unity (flutter_unity_widget)
- ✅ Pipeline export/import FBX
- ✅ Teste comunicação Flutter ↔ Unity

###### Semana 2-3: Modelos 3D
- ✅ Comprar 5 assets base (€200):
  - Medieval Castle (Castelo S. Jorge)
  - Gothic Cathedral (Sé)
  - Renaissance Palace (Paço Ribeira)
  - Monastery (Jerónimos)
  - Baroque Church (Carmo)
- ✅ Customizar em Blender:
  - Adicionar/remover torres
  - Retexturizar com cores Lisboa
  - Otimizar polígonos (<50k cada)
  - Exportar FBX
- ✅ Importar Unity:
  - Setup materials PBR
  - Iluminação básica
  - Colliders para interação

###### Semana 4: Implementação
- ✅ Cena Unity por edifício
- ✅ Controlos:
  - Pinch zoom
  - Rotate
  - Tap hotspots → info popup
- ✅ Transição Flutter → Unity:
  - Animação fade in
  - Loading screen
  - Botão voltar
- ✅ Teste performance (target 60 FPS)

**Deliverable:** 5 edifícios Unity interativos  
**Custos Mês 6:** €10 (Copilot) + €200 (assets) + €50 (texturas)

#### 📅 FASE 3: FEATURES AVANÇADAS (Meses 7-9)
##### MÊS 7 - Simulação Terramoto (Killer Feature)
**Objetivo:** Experiência WOW imersiva

###### Semana 1-2: Preparação Assets
- ✅ Modelos Lisboa 1755 (low-poly):
  - 20 edifícios principais
  - Opção A: Assets stock adaptados (€300)
  - Opção B: Modelar simplificado Blender (40h)
- ✅ Particle systems:
  - Fumo/poeira (Unity Asset Store grátis)
  - Fogo incêndios (free)
  - Água tsunami (free ou €35)
- ✅ Sound effects:
  - Terremoto rumble (freesound.org)
  - Colapsos edifícios (free)
  - Sinos tocando (free)
  - Tsunami wave (free ou €50 pack)

###### Semana 3-4: Implementação Unity
- ✅ Cena temporal 4 fases:
  - **FASE 1 - Lisboa 1755 (30s)**  
     → Cidade calma, dia normal  
     → Sons ambiente: mercado, carruagens, gaivotas  
     → Pode explorar livremente
  - **FASE 2 - Terramoto (45s)**  
     → 1 Nov 1755, 9:40  
     → Tela treme (device vibra)  
     → Edifícios balançam  
     → Collapsos progressivos (Rigidbody physics)  
     → Som estrondoso
  - **FASE 3 - Incêndios (30s)**  
     → Chamas emergem (particle systems)  
     → Fumo denso  
     → Céu escurece  
     → Panic sounds
  - **FASE 4 - Tsunami (45s)**  
     → Vista move para Tejo  
     → Onda gigante animada  
     → Inunda Baixa/Terreiro Paço  
     → Water simulation básica
  - **FASE 5 - Aftermath (30s)**  
     → Ruínas fumegantes  
     → Silêncio  
     → Estatísticas: "85% destruída, 60k mortos"  
     → Botão "Ver Reconstrução Pombalina"
- ✅ Otimização:
  - LOD automático
  - Quality settings por device
  - Target: funcionar iPhone 12+ / Android equiv.

**Deliverable:** Simulação terramoto completa  
**Custos Mês 7:** €10 (Copilot) + €385 (assets/sounds)

##### MÊS 8 - GPT Q&A & Conteúdo Expandido
**Objetivo:** Guia conversacional + 100 edifícios

###### Semana 1-2: GPT Integration
- ✅ Setup OpenAI API (GPT-4o-mini: €0.40/1000 perguntas)
- ✅ Sistema híbrido:
  - Áudio scriptado para narrativa base
  - GPT para perguntas específicas user
- ✅ Context awareness:
  - GPT sabe localização atual user
  - Edifícios visíveis no ecrã
  - Perfil académico user
  - Histórico visita
- ✅ Implementação:
  - Botão "Perguntar ao Guia"
  - Voice-to-text (Google grátis)
  - GPT responde (texto)
  - Text-to-speech (Google €15/mês ou Eleven Labs €30)
- ✅ Fallbacks:
  - Se sem internet → respostas pré-gravadas
  - Rate limiting (3 perguntas/minuto)

###### Semana 3-4: Expandir Edifícios
- ✅ Adicionar 70 edifícios (total 100):
  - Tier 3: representações simples
  - Pins 2D + info textual
  - Imagens históricas stock
- ✅ "Did You Know?" facts:
  - 50 factoids interessantes
  - Aparecem randomicamente durante visita
  - Ex: "Profissão acendedor lampiões!"
  - Ex: "Paço Ribeira tinha 400 divisões!"
- ✅ Sistema de favoritos:
  - User guarda edifícios preferidos
  - Exporta lista email

**Deliverable:** GPT conversacional + 100 POI  
**Custos Mês 8:** €10 (Copilot) + €30 (TTS) + €5 (OpenAI)

##### MÊS 9 - Analytics & 360° Views
**Objetivo:** Dados tese + imersão aumentada

###### Semana 1-2: Sistema Analytics
- ✅ Firebase Analytics completo:
  - Heatmap: onde users param mais tempo
  - Dwell time por POI
  - Percurso típico (flow)
  - Perfil mais popular
  - Circuito mais completado
  - Taxa abandono por feature
- ✅ Dashboard web (Firebase Console)
- ✅ A/B testing framework:
  - Versão A vs B de features
  - Randomização users
  - Métricas automáticas
- ✅ Feedback in-app:
  - Thumbs up/down por POI
  - Comentário opcional
  - NPS score final visita

###### Semana 3-4: 360° Interior Views
- ✅ 5 edifícios principais:
  - Castelo: vista do alto torre
  - Sé: interior nave central
  - Paço: sala do trono
  - Jerónimos: claustro
  - Carmo: ruínas pós-terramoto
- ✅ Implementação:
  - Opção A: Fotos 360° reais (free - próprias)
  - Opção B: Renders 3D 360° Blender (20h)
  - Opção C: Assets 360° stock (€200)
- ✅ Viewer 360°:
  - Gyroscope control
  - Hotspots interativos
  - Narração específica

**Deliverable:** Analytics completo + 360° views  
**Custos Mês 9:** €10 (Copilot) + €200 (360° assets opcional)

#### 📅 FASE 4: POLISH & VALIDAÇÃO (Meses 10-12)
##### MÊS 10 - Optimização & Testes Extensivos
**Objetivo:** App rock-solid

###### Semana 1-2:
- ✅ Optimization sweep:
  - Reduce APK/IPA size (target <100MB)
  - Battery optimization
  - Memory leaks fix
  - Loading times <3s
  - AR tracking stability
- ✅ Testes dispositivos:
  - iPhone 12, 13, 14, 15 (pedir emprestados)
  - Android flagship (Samsung S22, S23)
  - Android mid-range (Xiaomi Redmi Note)
  - Tablets iPad, Galaxy Tab

###### Semana 3-4: Beta Testing
- ✅ Recrutar 50 beta testers:
  - 20 estudantes FCT NOVA
  - 20 visitantes museu
  - 10 professores/investigadores
- ✅ Testes no museu (2 sessões, 4h cada):
  - Observação uso real
  - Entrevistas qualitativas
  - Formulário quantitativo
- ✅ Bug fixing iterativo
- ✅ Ajustes UX baseados em feedback

**Deliverable:** App otimizada + feedback 50 users  
**Custos Mês 10:** €10 (Copilot) + €100 (incentivos testers)

##### MÊS 11 - Conteúdo Final & Acessibilidade
**Objetivo:** 150 edifícios + inclusão

###### Semana 1-2:
- ✅ Completar 150 edifícios (adicionar 50):
  - Fichas informativas todas
  - Categorização completa
  - Fontes citadas (tese)
- ✅ Acessibilidade:
  - VoiceOver (iOS) support
  - TalkBack (Android) support
  - High contrast mode
  - Font size adjustable
  - Legendas em todos áudios
- ✅ Multilíngue:
  - Português (completo)
  - Inglês (tradução essencial)
  - Espanhol (opcional, se budget)

###### Semana 3-4:
- ✅ Conteúdo educativo:
  - 10 quizzes temáticos completos
  - Bibliografia por tema
  - Links recursos externos
  - PDFs descarregáveis (opcional)
- ✅ Easter eggs:
  - 5 segredos escondidos (engagement)
  - Ex: "Gabriel del Barco signature"
  - Badge especial se descobrir todos

**Deliverable:** App completa 150 POI + acessível  
**Custos Mês 11:** €10 (Copilot) + €200 (traduções)

##### MÊS 12 - Launch & Documentação Tese
**Objetivo:** App live + escrita tese

###### Semana 1-2: Launch
- ✅ Preparação stores:
  - Screenshots profissionais (10+ cada store)
  - Vídeo demo (1-2min)
  - Descrição optimizada ASO
  - Press kit (para media)
- ✅ Soft launch:
  - Portugal apenas (iOS + Android)
  - Monitoring 24/7 primeira semana
  - Hotfixes rápidos se bugs críticos
- ✅ Evento lançamento museu:
  - Demonstração ao vivo
  - Imprensa cultural
  - Stakeholders (CML, DGPC)

###### Semana 3-4: Documentação
- ✅ Escrita tese (estrutura):
  1. Introdução (10 págs)
      - Contexto Grande Panorama
      - Problema investigação
      - Objetivos
  2. Estado da Arte (20 págs)
      - AR em património cultural
      - Apps museus mundo
      - Tecnologias educativas
  3. Metodologia (15 págs)
      - Design thinking
      - User research
      - Tecnologias escolhidas
      - Arquitetura sistema
  4. Implementação (25 págs)
      - Features desenvolvidas
      - Desafios técnicos
      - Decisões design
  5. Avaliação (20 págs)
      - Testes 100+ users
      - Métricas quantitativas
      - Feedback qualitativo
      - Analytics
  6. Discussão & Conclusões (10 págs)
      - Contribuições
      - Limitações
      - Trabalho futuro
  7. Bibliografia + Anexos (10 págs)
- ✅ Dados para tese:
  - Analytics 1000+ visitantes (Mês 12)
  - A/B testing results
  - Heatmaps
  - User satisfaction (NPS)
  - Task completion rates
  - Engagement metrics

**Deliverable:** App publicada + tese escrita  
**Custos Mês 12:** €10 (Copilot) + €50 (video demo)

### CONTINGÊNCIAS & PLANOS B
- **Risco 1: Unity integration muito difícil**  
  Plano B:  
  - Usar só Flutter com modelos 3D simples (model_viewer_plus)  
  - Qualidade menor mas funciona  
  - Economiza €500 + 40h trabalho

- **Risco 2: Tracking AR instável no museu**  
  Plano B:  
  - QR codes físicos backup (museu coloca)  
  - Localização manual (user seleciona zona)  
  - Hybrid: AR + manual selection

- **Risco 3: GPT API muito cara**  
  Plano B:  
  - Limite 5 perguntas/visita  
  - Apenas para users Premium (futura monetização)  
  - Respostas pré-gravadas (FAQ approach)

- **Risco 4: Não conseguir financiamento**  
  Plano B:  
  - MVP ultra-lean (30 POI, sem Unity, TTS grátis)  
  - Custo total: €500-700  
  - Ainda publicável como tese válida  
  - Expandir pós-tese se conseguir funding

### DELIVERABLES FINAIS TESE
1. **App Publicada:**  
    - iOS App Store ✅  
    - Google Play Store ✅  
    - 150 edifícios interativos  
    - 5 circuitos temáticos  
    - Simulação terramoto  
    - Guia áudio + GPT Q&A  

2. **Tese Escrita (100 págs):**  
    - Metodologia rigorosa  
    - Dados empíricos 1000+ users  
    - Contribuições científicas  
    - Publicável em conferência (ACM CHI, Museums & Heritage)  

3. **Portfolio Profissional:**  
    - App complexa AR/Unity  
    - UX design provado  
    - Analytics data-driven  
    - Referências (museu + orientador)  

4. **Impacto Real:**  
    - Museu tem ferramenta nova (grátis)  
    - Visitantes experiência melhorada  
    - Possível replicação outros museus  
    - Startup potential (se quiser prosseguir)
