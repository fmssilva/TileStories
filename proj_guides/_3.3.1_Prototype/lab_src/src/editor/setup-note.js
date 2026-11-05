/* The Detail Card tab's note on the order of setting up a card. */
/* ----- the setup flow, where the developer works: at the top of the Detail Card tab ----- */
on("edCard.html",function(h){var i=h.indexOf('<details');var flow='<div class="note flow"><b>Setting up a card, in order.</b> 3 · Blocks &amp; Components: elements and their named styles, patterns made of them, then each block\'s own options, preview and page. 1 · Card and 2 · Sections: how the card opens and looks, and its chapters. 4 · Motion &amp; Effects and 5 · Card Controls: movement and the parts around the content. Then Specific Marker: 1 · The card (this point\'s blocks), 2 · Pages (their content). Opening any part shows it on the phone.</div>';
  return i>=0?h.slice(0,i)+flow+h.slice(i):flow+h});
