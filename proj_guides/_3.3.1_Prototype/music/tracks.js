/* The music bed's tracks (the Architect adds approved ones; each with its licence and source). Loaded like voicesets.js: from the
   page's folder or one up. Each track: id, name (the visitor's "feeling"), file, licence, author, source, seconds. The first is the default. */
(function(){var u=(document.currentScript&&document.currentScript.src||"").replace(/tracks\.js.*$/,""),tracks=[
  {"id":"exploration","name":{"en":"Exploration","pt":"Exploração"},"file":"exploration.mp3","licence":"CC0","author":"RandomMind","source":"https://opengameart.org/content/medieval-exploration","seconds":236},
  {"id":"bards-tale","name":{"en":"Storyteller","pt":"Contador de histórias"},"file":"bards-tale.mp3","licence":"CC0","author":"RandomMind","source":"https://opengameart.org/content/medieval-the-bards-tale","seconds":159},
  {"id":"harvest-season","name":{"en":"Calm","pt":"Calma"},"file":"harvest-season.mp3","licence":"CC0","author":"RandomMind","source":"https://opengameart.org/content/medieval-harvest-season","seconds":214},
  {"id":"rejoicing","name":{"en":"Cinematic","pt":"Cinematográfica"},"file":"rejoicing.mp3","licence":"CC0","author":"RandomMind","source":"https://opengameart.org/content/medieval-rejoicing","seconds":103},
  {"id":"cabezon-duuiensela","name":{"en":"Period (16th century)","pt":"Época (século XVI)"},"file":"cabezon-duuiensela.ogg","licence":"CC BY-SA 2.0","author":"Joan Benson","source":"https://commons.wikimedia.org/wiki/File:Antonio_de_Cabezon_-_Duuiensela.ogg","seconds":250,
   "credit":{"en":"Antonio de Cabezón, Duuiensela, played on the clavichord by Joan Benson (1973). CC BY-SA 2.0, via Wikimedia Commons.","pt":"Antonio de Cabezón, Duuiensela, tocada em clavicórdio por Joan Benson (1973). CC BY-SA 2.0, via Wikimedia Commons."}}
];tracks.forEach(function(t){t.url=u+t.file});window.MUSICTRACKS=(window.MUSICTRACKS||[]).concat(tracks)})();
