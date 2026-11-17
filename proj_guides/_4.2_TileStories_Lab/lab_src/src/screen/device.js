/* The device the app is seen on (the World Lab's four, W's js/12_device.js): a phone or a tablet, portrait or landscape. ui.device
   is the device now (Unity: Screen.width / height and Screen.orientation at run time; a Device Simulator profile in the Editor).
   Every size is the device's logical size in px; the Lab's frame is scaled to fit the window (deviceFrame), never resized, so a
   layout is judged at the device's own size. Landscape draws the card as a full-height side sheet over the camera (.ph.land,
   card/card-base.css), a layout not designed yet; the tablet's portrait is the phone's layout, wider (not designed yet either).
   The device buttons (W's): a click picks the phone or the tablet, a second click on the same one turns it. */
var DEVICES={
  phone_p:{label:"Phone",w:390,h:844,kind:"phone"},
  phone_l:{label:"Phone, landscape",w:844,h:390,kind:"phone"},
  tablet_p:{label:"Tablet",w:820,h:1180,kind:"tablet"},
  tablet_l:{label:"Tablet, landscape",w:1180,h:820,kind:"tablet"}};
ui.device="phone_p";
function dev(){return DEVICES[ui.device]}
function devW(){return dev().w}
function devH(){return dev().h}
function devLand(){var d=dev();return d.w>d.h}
function deviceTo(id){if(!DEVICES[id])throw new Error("No device "+id);if(id===ui.device)return;ui.device=id;LAB.ptoolPos=null;renderPhone(false)}
/* the frame: the device's size, scaled down to the window's height (less the page's 48 px) and, off the phone's portrait, to most of
   the page's width (the stage's column widens with it) */
function deviceFrame(){var d=dev(),grid=document.querySelector(".grid"),wide=ui.device!=="phone_p";
  /* a new device takes its size at once (no transition: what is drawn next measures the new screen) */
  if(ph.style.width!==d.w+"px"||ph.style.height!==d.h+"px"){ph.style.transition="none";ph.style.width=d.w+"px";ph.style.height=d.h+"px";void ph.offsetWidth;ph.style.transition=""}
  ph.classList.toggle("land",devLand());ph.classList.toggle("tablet",d.kind==="tablet");
  var gw=grid?grid.getBoundingClientRect().width:1380,k=Math.min(1,(window.innerHeight-48)/d.h,wide?Math.max(390,gw*.62)/d.w:1);
  fit.style.transform="scale("+k+")";fit.style.width=(d.w*k)+"px";fit.style.height=(d.h*k)+"px";fit.style.transformOrigin="top left";
  if(grid)grid.style.gridTemplateColumns=wide?"minmax(0,"+Math.ceil(d.w*k+30)+"px) minmax(0,1fr)":"";return k}
var deviceT=0;window.addEventListener("resize",function(){clearTimeout(deviceT);deviceT=setTimeout(deviceFrame,120)});
/* W's device buttons, on the preview bar (the Lab's tools on the phone): their icons, pressed for the device's kind, turned in landscape */
var DEV_ICON={phone:'<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="7" y="2.5" width="10" height="19" rx="2.2"/><path d="M11 5h2"/></svg>',
  tablet:'<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="4.5" y="2.5" width="15" height="19" rx="2"/><path d="M11 19h2"/></svg>'};
on("ptool.html",function(h){var d=dev();return h+'<span class="devbtns" role="group" aria-label="Device">'+["phone","tablet"].map(function(kind){var on=d.kind===kind;
  return'<button type="button" data-dev="'+kind+'" aria-pressed="'+on+'" class="'+(on&&devLand()?"land":"")+'" title="'+TXH("pbar_dev_"+kind)+'" aria-label="'+esc(DEVICES[kind+"_p"].label)+'">'+DEV_ICON[kind]+'</button>'}).join("")+'</span>'});
document.addEventListener("click",function(e){var b=e.target.closest&&e.target.closest("#ptool [data-dev]");if(!b)return;e.stopPropagation();e.preventDefault();
  var kind=b.dataset.dev;deviceTo(dev().kind===kind?(ui.device.slice(-2)==="_p"?kind+"_l":kind+"_p"):kind+"_p")},true);
