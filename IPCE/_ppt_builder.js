const pptxgen = require("pptxgenjs");
const fs = require("fs");

// ---------- Palette ----------
const NAVY = "0F1F3D";
const NAVY2 = "16294D";
const BLUE = "2B59F7";
const ICE = "E8EEFB";
const ICE2 = "F3F6FC";
const TEXT_DARK = "1B2A4A";
const MUTED = "6B7690";
const WHITE = "FFFFFF";
const RED = "FF0000";

const FONT = "Calibri";

const SITE = "ipce-184ea7.gitlab.io";
const EMAIL = "fmso.silva@campus.fct.unl.pt";

// ---------- Type scale ----------
const SZ_TITLE = 32;
const SZ_SUB = 24;
const SZ_SUB_LG = 28;
const SZ_BODY = 20;
const SZ_SMALL = 18;
const SZ_KICKER = 16;
const SZ_FOOTER = 13;

function pres() {
  const p = new pptxgen();
  p.defineLayout({ name: "WIDE", width: 13.333, height: 7.5 });
  p.layout = "WIDE";
  return p;
}

// ---------- Helpers ----------
function footer(slide, pageLabel) {
  slide.addText(
    [
      { text: "IPCE 2026/2027   ", options: { color: MUTED } },
      { text: "\u2022  ", options: { color: MUTED } },
      { text: SITE + "   ", options: { color: BLUE } },
      { text: "\u2022  ", options: { color: MUTED } },
      { text: EMAIL, options: { color: MUTED } },
    ],
    {
      x: 0.5, y: 7.16, w: 9.5, h: 0.28,
      fontFace: FONT, fontSize: SZ_FOOTER, align: "left", valign: "middle",
      margin: 0,
    }
  );
  if (pageLabel) {
    slide.addText(pageLabel, {
      x: 12.2, y: 7.16, w: 0.7, h: 0.28,
      fontFace: FONT, fontSize: SZ_FOOTER, color: MUTED, align: "right", valign: "middle",
      margin: 0,
    });
  }
}

function lightBg(slide) {
  slide.background = { color: WHITE };
  slide.addImage({ path: "bg_full_169.jpg", x: 0, y: 0, w: 13.333, h: 7.5 });
}

function logoTopLeft(slide, dark) {
  slide.addImage({
    path: dark ? "logo_white_sm.png" : "logo_clean_sm.png",
    x: 0.5, y: 0.38, w: 1.45, h: 0.84,
  });
}

function kicker(slide, text, opts = {}) {
  slide.addText(text.toUpperCase(), {
    x: opts.x ?? 0.5, y: opts.y ?? 1.28, w: opts.w ?? 9, h: 0.32,
    fontFace: FONT, fontSize: SZ_KICKER, bold: true, color: BLUE,
    charSpacing: 2, align: "left", margin: 0,
  });
}

function title(slide, text, opts = {}) {
  slide.addText(text, {
    x: opts.x ?? 0.5, y: opts.y ?? 1.62, w: opts.w ?? 9.5, h: opts.h ?? 0.95,
    fontFace: FONT, fontSize: opts.size ?? SZ_TITLE, bold: true,
    color: opts.color ?? TEXT_DARK, align: "left", margin: 0, lineSpacingMultiple: 1.02,
  });
}

// icon-in-circle + heading (SUB) + description (BODY) row
function iconRow(slide, x, y, w, iconPath, heading, desc, opts = {}) {
  const circleD = 0.6;
  slide.addShape("ellipse", {
    x, y, w: circleD, h: circleD,
    fill: { color: opts.circleFill ?? ICE },
    line: { type: "none" },
  });
  slide.addImage({ path: iconPath, x: x + 0.13, y: y + 0.13, w: 0.34, h: 0.34 });
  slide.addText(heading, {
    x: x + circleD + 0.25, y: y - 0.04, w: w - circleD - 0.25, h: 0.4,
    fontFace: FONT, fontSize: opts.headSize ?? SZ_SUB, bold: true, color: opts.headColor ?? TEXT_DARK,
    align: "left", margin: 0, valign: "top",
  });
  if (desc) {
    slide.addText(desc, {
      x: x + circleD + 0.25, y: y + 0.4, w: w - circleD - 0.25, h: opts.descH ?? 0.4,
      fontFace: FONT, fontSize: opts.descSize ?? SZ_BODY, color: opts.descColor ?? MUTED,
      align: "left", margin: 0, lineSpacingMultiple: 1.08, valign: "top",
    });
  }
}

function card(slide, x, y, w, h, opts = {}) {
  slide.addShape("roundRect", {
    x, y, w, h,
    rectRadius: 0.1,
    fill: { color: opts.fill ?? ICE2 },
    line: opts.line ?? { type: "none" },
    shadow: opts.shadow,
  });
}

// red annotation arrow pointing from a bullet to a spot on the screenshot
function arrow(slide, x, y, w, h, flipV) {
  slide.addShape("line", {
    x, y, w, h, flipV: !!flipV,
    line: { color: RED, width: 6, endArrowType: "triangle" },
  });
}

// site QR card ("ACEDE AGORA")
function qrCard(slide, x, y, w, h, qrPath, qrSize, caption) {
  card(slide, x, y, w, h, { fill: NAVY });
  slide.addText("ACEDE AGORA", {
    x, y: y + 0.13, w, h: 0.32,
    fontFace: FONT, fontSize: SZ_KICKER, bold: true, color: BLUE, align: "center", charSpacing: 2, margin: 0,
  });
  const qx = x + (w - qrSize) / 2;
  slide.addImage({ path: qrPath, x: qx, y: y + 0.53, w: qrSize, h: qrSize });
  if (caption) {
    slide.addText(caption, {
      x, y: y + 0.53 + qrSize + 0.08, w, h: 0.35,
      fontFace: FONT, fontSize: SZ_SMALL, color: WHITE, align: "center", margin: 0,
    });
  }
}

// simplified "P1 -> T1" flow bubble (stands in for the SmartArt diagram)
function p1t1Diagram(slide, x, y) {
  const d = 1.05;
  const gap = 0.32;
  // P1 circle
  slide.addShape("ellipse", {
    x, y, w: d, h: d,
    fill: { color: ICE }, line: { color: BLUE, width: 1.5 },
  });
  slide.addText("P1", {
    x, y, w: d, h: d, fontFace: FONT, fontSize: SZ_SUB, bold: true, color: TEXT_DARK,
    align: "center", valign: "middle", margin: 0,
  });
  // connector triangle
  slide.addShape("triangle", {
    x: x + d + 0.03, y: y + d / 2 - 0.14, w: 0.26, h: 0.28,
    rotate: 90, fill: { color: BLUE }, line: { type: "none" },
  });
  // T1 circle
  const x2 = x + d + gap;
  slide.addShape("ellipse", {
    x: x2, y, w: d, h: d,
    fill: { color: ICE }, line: { color: BLUE, width: 1.5 },
  });
  slide.addText("T1", {
    x: x2, y, w: d, h: d, fontFace: FONT, fontSize: SZ_SUB, bold: true, color: TEXT_DARK,
    align: "center", valign: "middle", margin: 0,
  });
}

// ============================================================
const P = pres();

// ------------------------------------------------------------
// SLIDE 1 — Capa
// ------------------------------------------------------------
{
  const s = P.addSlide();
  s.background = { color: NAVY };

  const braces = [
    { x: 9.6, y: -1.0, size: 260, color: NAVY2 },
    { x: 11.6, y: 3.2, size: 200, color: NAVY2 },
    { x: 8.6, y: 4.6, size: 170, color: NAVY2 },
  ];
  braces.forEach(b => {
    s.addText("{ }", {
      x: b.x, y: b.y, w: 4, h: 3, fontFace: "Cambria",
      fontSize: b.size, color: b.color, align: "left", margin: 0,
    });
  });

  logoTopLeft(s, true);

  s.addText("INTRODUÇÃO À PROGRAMAÇÃO PARA A\nCIÊNCIA E ENGENHARIA", {
    x: 0.7, y: 2.45, w: 10.8, h: 1.55,
    fontFace: FONT, fontSize: 34, bold: true, color: WHITE,
    align: "left", margin: 0, lineSpacingMultiple: 1.08,
  });

  s.addShape("rect", { x: 0.75, y: 4.1, w: 0.55, h: 0.05, fill: { color: BLUE }, line: { type: "none" } });

  s.addText("Aula Prática 01  —  Boas-vindas & Set Up", {
    x: 0.7, y: 4.28, w: 9, h: 0.55,
    fontFace: FONT, fontSize: SZ_SUB_LG, color: ICE, align: "left", margin: 0,
  });

  s.addText("Turno P1  ·  2ª-feira de manhã  ·  Ano letivo 2026/2027", {
    x: 0.7, y: 4.92, w: 9, h: 0.4,
    fontFace: FONT, fontSize: SZ_SMALL, color: MUTED, align: "left", margin: 0,
  });

  s.addText([
    { text: "Francisco Silva\n", options: { bold: true, fontSize: SZ_BODY, color: WHITE } },
    { text: EMAIL, options: { fontSize: SZ_SMALL, color: "9BB0E0" } },
  ], {
    x: 0.7, y: 6.3, w: 6, h: 0.75, fontFace: FONT, align: "left", margin: 0, lineSpacingMultiple: 1.2,
  });

  // site QR, tucked into the brace motif bottom-right
  s.addImage({ path: "qr_site.png", x: 8.915, y: 4.972, w: 1.948, h: 1.948 });
  s.addText(SITE, {
    x: 8.374, y: 6.867, w: 3.1, h: 0.4,
    fontFace: FONT, fontSize: SZ_KICKER, color: BLUE, align: "center", margin: 0,
  });

  s.addNotes(
    "ANTES DE ABRIR ESTE SLIDE (~5 min, luzes acesas, sem projetor se der):\n" +
    "- Chegar mais cedo, testar projetor/som e o link do QR.\n" +
    "- Mãos no ar: quem já programou? em que linguagem? quem já viu Python? " +
    "todos são da mesma licenciatura? todos percebem português? têm PC pessoal " +
    "(aconselhar a trazer o deles - qualquer PC serve, e assim levam o código para casa).\n" +
    "- Apresentação pessoal rápida (30 seg): nome, papel na cadeira, tom leve.\n\n" +
    "NESTE SLIDE:\n" +
    "- Nome da cadeira, turno P1, ano letivo. Sem pressa, é só o cartão de visita."
  );
}

// ------------------------------------------------------------
// SLIDE 2 — Tudo passa por aqui (site + QR + peek do site real)
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Apresentação da disciplina");
  title(s, "Tudo passa por aqui", { size: SZ_TITLE });

  card(s, 0.5, 2.403, 6.6, 0.85, { fill: ICE });
  s.addImage({ path: "icons_link_blue.png", x: 0.75, y: 2.664, w: 0.38, h: 0.38 });
  s.addText(SITE, {
    x: 1.3, y: 2.403, w: 5.65, h: 0.85,
    fontFace: FONT, fontSize: SZ_SUB, bold: true, color: BLUE, valign: "middle", margin: 0,
  });

  qrCard(s, 1.201, 3.451, 3.9, 3.491, "qr_site.png", 2.5, null);

  // real peek of the course site, bleeding to the right/bottom edge
  s.addImage({ path: "site_screenshot.png", x: 6.455, y: 0.38, w: 6.878, h: 7.12 });

  footer(s, "2");

  s.addNotes(
    "- Pedir para tirarem já o telemóvel e lerem o QR (30 seg) - 'a partir de agora é aqui que vive tudo'.\n" +
    "- Mostrar os avisos reais no ecrã: instalar Miniconda ANTES da aula (a rede Wifi não aguenta a turma toda a instalar ao mesmo tempo).\n" +
    "- Apontar rapidamente: Teóricas/Práticas, Fórum, Mooshak - só para saberem que existe, sem entrar em detalhe agora."
  );
}

// ------------------------------------------------------------
// SLIDE 3 — Turno fora de ordem
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Apresentação da disciplina");
  title(s, "Turno fora de ordem", { size: SZ_TITLE });

  p1t1Diagram(s, 0.9, 2.3);

  const rows = [
    ["icons_users_blue.png", "Aprender fazendo", "Mais mão na massa, menos teoria aqui."],
    ["icons_calendar_blue.png", "Guião de pré-aula", "Resumo curto + aquecimento, antes da aula."],
  ];
  let y = 3.683;
  rows.forEach(([icon, h, d]) => {
    iconRow(s, 0.5, y, 6.9, icon, h, d, { descH: 0.4 });
    y += 0.947;
  });

  footer(s, "3");

  s.addNotes(
    "- Explicar o P1 -> T1: a prática desta turma é de manhã, a teórica da MESMA matéria só à tarde.\n" +
    "- Por isso mandamos sempre um guião de pré-aula durante a semana - não é obrigatório, mas ajuda muito.\n" +
    "- Mensagem chave para hoje: 'aqui vamos aprender fazendo - vão programar muito mais do que ouvir'."
  );
}

// ------------------------------------------------------------
// SLIDE 4 — Dúvidas fora da aula
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Contactos & logística");
  title(s, "Dúvidas fora da aula", { size: SZ_TITLE });

  const items = [
    ["icons_clock_blue.png", "Atendimento", "[a definir]"],
    ["icons_question_blue.png", "Fórum", "https://groups.google.com/g/ipce-2627"],
    ["icons_mail_blue.png", "Email", EMAIL],
  ];
  let y = 3.011;
  items.forEach(([icon, h, d]) => {
    iconRow(s, 0.5, y, 6.05, icon, h, d, { descH: 0.4 });
    y += 1.155;
  });

  // real peek of the course site, bleeding to the right/bottom edge
  s.addImage({ path: "site_screenshot.png", x: 6.455, y: 0.38, w: 6.878, h: 7.12 });

  // annotation arrows pointing at the matching links on the screenshot
  arrow(s, 3.454, 3.345, 3.211, 0.301, false);
  arrow(s, 5.899, 4.231, 0.767, 0.34, true);

  footer(s, "4");

  s.addNotes(
    "- Mostrar rapidamente ONDE no site encontram cada coisa (seguir as setas no ecrã).\n" +
    "- Fórum: dizer que é preferível ao email para dúvidas de matéria - a resposta fica visível para toda a turma.\n" +
    "- Horário de atendimento: dizer que fica marcado no site assim que definido."
  );
}

// ------------------------------------------------------------
// SLIDE 5 — Onde estamos (anterior / hoje / seguinte)
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Onde estamos");
  title(s, "O nosso percurso", { size: SZ_TITLE });

  const track = [
    { label: "AULA ANTERIOR", text: "...", sub: null, current: false, muted: true },
    { label: "HOJE  ·  AULA 1", text: "Boas-vindas & Set Up", sub: "Spyder, Python, Mooshak", current: true },
    { label: "PRÓXIMA AULA", text: "Aula 2 — if, ciclos, funções, recursividade, operadores", sub: null, current: false },
  ];

  let y = 2.55;
  track.forEach((step) => {
    const h = step.current ? 1.55 : 1.1;
    card(s, 0.5, y, 11.3, h, {
      fill: step.current ? BLUE : ICE2,
      line: step.current ? { type: "none" } : { color: ICE, width: 1 },
    });
    s.addText(step.label, {
      x: 0.85, y: y + 0.16, w: 10, h: 0.34,
      fontFace: FONT, fontSize: SZ_KICKER, bold: true, charSpacing: 1.5,
      color: step.current ? "CFE0FF" : BLUE, align: "left", margin: 0,
    });
    s.addText(step.text, {
      x: 0.85, y: y + 0.48, w: 10.6, h: 0.5,
      fontFace: FONT, fontSize: step.current ? SZ_SUB_LG : SZ_SUB,
      bold: true, color: step.muted ? MUTED : (step.current ? WHITE : TEXT_DARK),
      italic: step.muted, align: "left", margin: 0,
    });
    if (step.sub) {
      s.addText(step.sub, {
        x: 0.85, y: y + 1.02, w: 10.6, h: 0.4,
        fontFace: FONT, fontSize: SZ_SMALL, color: "D9E4FF",
        align: "left", margin: 0,
      });
    }
    y += h + 0.22;
  });

  footer(s, "5");

  s.addNotes(
    "- Explicar que TODAS as semanas vamos abrir a aula assim: o que vimos, o que vamos ver hoje, o que vem a seguir.\n" +
    "- Hoje: boas-vindas + setup (Spyder, Python, Mooshak). Para a semana: se/ciclos/funções/recursividade - a lógica a sério.\n" +
    "- Não entrar em detalhe da aula 2 agora, é só para situarem o mapa."
  );
}

// ------------------------------------------------------------
// SLIDE 6 — O que fazemos hoje (Práticas 01a + 01b)
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Aula prática 1");
  title(s, "O que fazemos hoje – Práticas [01a + 01b]", { size: SZ_TITLE, w: 11.5 });

  const items = [
    ["icons_terminal_blue.png", "Miniconda + Spyder", "Instalar IDE (Integrated Development Environment)"],
    ["icons_code_blue.png", "Primeiro contacto com Python", "Explorar o interpretador"],
    ["icons_target_blue.png", "Mooshak", "Eduroam / VPN + Mooshak"],
  ];
  let y = 2.5;
  items.forEach(([icon, h, d]) => {
    iconRow(s, 0.5, y, 6.9, icon, h, d, { descH: 0.4 });
    y += 1.0;
  });

  footer(s, "6");

  s.addNotes(
    "- Este é o mapa da aula de hoje - a partir daqui é tudo prático, fecha-se o portátil de slides.\n" +
    "- 1a - Miniconda + Spyder: instalar em conjunto, ir passando pela sala. Avisar já: quem não tiver Wifi a funcionar, ajudar com pen/hotspot.\n" +
    "- 1b - Primeiro contacto: abrir o ficheiro aula_1_complement.py, mostrar como correr por células (Ctrl+Enter). Projetar o teu ecrã, eles seguem no deles.\n" +
    "- Mooshak: mostrar o registo e o VPN/Eduroam - só o essencial, não é preciso resolver exercícios já.\n" +
    "- Formar grupos de 2 agora: durante a aula ajudam-se um ao outro; para o projeto sentam-se sempre juntos.\n" +
    "- Se muita gente encravar no mesmo ponto, parar todos e resolver no quadro/projetor.\n" +
    "- Gerir o tempo: ~15 min instalação, ~25 min primeiro contacto Python, ~10 min Mooshak/VPN."
  );
}

// ------------------------------------------------------------
// SLIDE 7 — Consolidar e preparar próxima aula (com QR novo)
// ------------------------------------------------------------
{
  const s = P.addSlide();
  lightBg(s);
  logoTopLeft(s, false);
  kicker(s, "Antes da próxima aula");
  title(s, "Consolidar e Preparar próxima aula", { size: SZ_TITLE, w: 10.5 });

  iconRow(s, 0.5, 3.066, 6.9, "icons_code_blue.png", "Ler e Experimentar", "O exercício de consolidação e aquecimento", { descH: 0.4 });

  qrCard(s, 7.568, 2.879, 3.364, 3.452, "qr_drive.png", 2.3, null);

  footer(s, "7");

  s.addNotes(
    "- Mostrar este slide nos últimos 5 min da aula.\n" +
    "- Pedir para lerem o ficheiro de consolidação e fazerem o exercício de aquecimento durante a semana - 20-30 min chegam.\n" +
    "- O QR aponta para a pasta da próxima aula (Drive) - dar tempo para todos digitalizarem."
  );
}

// ------------------------------------------------------------
// SLIDE 8 — Fecho / Dúvidas
// ------------------------------------------------------------
{
  const s = P.addSlide();
  s.background = { color: NAVY };
  const braces = [
    { x: 9.8, y: -0.8, size: 240, color: NAVY2 },
    { x: 8.6, y: 4.6, size: 170, color: NAVY2 },
  ];
  braces.forEach(b => {
    s.addText("{ }", { x: b.x, y: b.y, w: 4, h: 3, fontFace: "Cambria", fontSize: b.size, color: b.color, margin: 0 });
  });

  logoTopLeft(s, true);

  s.addText("Dúvidas?", {
    x: 0.7, y: 2.6, w: 8, h: 1.05, fontFace: FONT, fontSize: 40, bold: true, color: WHITE, margin: 0,
  });
  s.addText("Bom semestre a todos — vemo-nos na próxima 2ª-feira.", {
    x: 0.7, y: 3.62, w: 9, h: 0.55, fontFace: FONT, fontSize: SZ_SUB, color: ICE, margin: 0,
  });

  const items = [
    ["icons_mail_white.png", EMAIL],
    ["icons_link_white.png", SITE],
  ];
  let y = 4.75;
  items.forEach(([icon, txt]) => {
    s.addImage({ path: icon, x: 0.72, y, w: 0.32, h: 0.32 });
    s.addText(txt, { x: 1.2, y: y - 0.05, w: 7, h: 0.45, fontFace: FONT, fontSize: SZ_BODY, color: WHITE, valign: "middle", margin: 0 });
    y += 0.58;
  });

  s.addNotes(
    "- Fecho da aula: perguntar se ficou algo por esclarecer, dar espaço real para perguntas (não só retórico).\n" +
    "- Lembrar: passar a folha de presenças, se ainda não foi.\n" +
    "- Confirmar que todos sabem o que têm de fazer até à próxima aula (slide anterior).\n" +
    "- Nota pessoal: avisar outros turnos/colegas se surgiu algum problema técnico hoje (Wifi, instalação, Mooshak)."
  );
}

P.writeFile({ fileName: "IPCE_Aula1_P1.pptx" }).then(() => console.log("written"));
