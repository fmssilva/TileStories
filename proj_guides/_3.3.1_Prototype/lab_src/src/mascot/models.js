/* The 3D models to choose from (AZ3D_MODELS): Azu detailed, Azu simple, the mouse, the explorer, the poet, the soldier. */
var AZ3D_MODELS=[
  {id:"azu",name:"Azu, detailed",file:"azu.glb",glb:"__AZUGLB_AZU__",thumb:"__AZTHUMB_AZU__",
   about:"Meshy model of the tile with the Belém tower (CC BY 4.0: credit Meshy), reduced to 24,000 triangles; the arms lift freely"},
  {id:"tile",name:"Azu, simple",file:"azu_tile.glb",glb:AZUGLB,thumb:"__AZTHUMB_TILE__",
   about:"modelled by script in Blender: a light model (0.7 MB) with an outline"},
  {id:"mouse",name:"The mouse",file:"mouse.glb",glb:"__AZUGLB_MOUSE__",thumb:"__AZTHUMB_MOUSE__",
   about:"Meshy model (CC BY 4.0) with a swinging tail; its arms are part of the coat, so it points and waves with the arm out, not up"},
  {id:"explorer",name:"The explorer",file:"explorer.glb",glb:"__AZUGLB_EXPLORER__",thumb:"__AZTHUMB_EXPLORER__",
   about:"Meshy model (CC BY 4.0); points and waves with the spyglass, the arm out (its arms are part of the coat)"},
  {id:"poet",name:"The poet",file:"poet.glb",glb:"__AZUGLB_POET__",thumb:"__AZTHUMB_POET__",
   about:"Meshy model (CC BY 4.0) with a book and a quill; its arms are part of the robe, so its gestures are small"},
  {id:"soldier",name:"The soldier",file:"soldier.glb",glb:"__AZUGLB_SOLDIER__",thumb:"__AZTHUMB_SOLDIER__",
   about:"Meshy model (CC BY 4.0) on Meshy's own skeleton (elbows, knees, a spine) with its walk; the halberd stays upright, the shield arm waves and points"}];
S.guideCfg.azuModel="azu";
function az3dModelOf(id){for(var i=0;i<AZ3D_MODELS.length;i++)if(AZ3D_MODELS[i].id===id)return AZ3D_MODELS[i];return AZ3D_MODELS[0]}
/* the model to draw: the one the Editor picks */
function az3dModel(){var m=az3dModelOf(S.guideCfg.azuModel);return{id:m.id,name:m.name+" ("+m.file+")",glb:m.glb}}
/* the model changes nothing the phone's picture can show until the 3D model is drawn: a behaviour for the self-test */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guideCfg\\.azuModel|"));

/* Editor: 6 · Guide > Azu > Mascot source = 3D model: the Model picker, one card per model with its picture */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  if(az3dOn()){var r=rowEl(d,"s7mascot","Mascot source");
    if(r)r.insertAdjacentHTML("afterend",row("Model",'<div class="thumbs az3dpick">'+AZ3D_MODELS.map(function(m){return'<button class="sc" type="button" data-set="@guideCfg.azuModel" data-v="'+m.id+'" aria-pressed="'+(S.guideCfg.azuModel===m.id)+'" title="'+esc(m.about)+'"><img src="'+m.thumb+'" alt="" width="96" height="96"><span class="nm"><span>'+esc(m.name)+'</span></span></button>'}).join("")+'</div>',
      "Which 3D model is Azu. Each has the same skeleton contract: the clips idle, talk, listen, think, point, walk and wave; the morphs Blink, Open, Wide and Round; the front arm as its own mesh. "+AZ3D_MODELS.map(function(m){return m.name+": "+m.about+"."}).join(" ")+" Import… below tries a GLB of your own.",true))}
  return d.innerHTML});
