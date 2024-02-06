var c = document.getElementById("mmCanvas");
var ctx = c.getContext("2d");

ctx.lineWidth = 2;
var boundingRect = c.getBoundingClientRect();
var cliRect = c.getClientRects();
ctx.canvas.width = boundingRect.width; //window.innerWidth;
ctx.canvas.height = boundingRect.height; //window.innerHeight;
var w = ctx.canvas.width;
var h = ctx.canvas.height;
var kX = w / (boundingRect.right - boundingRect.left);
var kY = h / (boundingRect.bottom - boundingRect.top);
var hRect = h / 5;
var wRect = w / 5;
var cx = w / 2 - wRect / 2;
var cy = h / 2 - hRect / 2;
var mmCentralNode = new MmNode(ctx, cx, cy, wRect, hRect);
mmCentralNode.draw();
var nodes = [];
nodes.push(mmCentralNode);

const inputEl = document.getElementById("mmCanvas");
const inputTxtElm = document.getElementById("text");

// todo: select drawn rectangle after mouse click
// todo: provide input after mouse click of selected

c.onmousedown = function (event) {
  var posInCanvas = clickXYtoCavasXY(event.clientX, event.clientY);
  if (!mmCentralNode.isInside(posInCanvas.x, posInCanvas.y)) {
    mmCentralNode.clearSelection();
    return;
  }
  if (mmCentralNode.isSelected) {
    inputTxtElm.style.position = "absolute";
    inputTxtElm.style.left = event.clientX + "px";
    inputTxtElm.style.top = event.clientY + "px";
    inputTxtElm.style.visibility = "visible";
    addEventListnerForInputEnter();
    //mmCentralNode.drawText();
  }
  mmCentralNode.drawSelection();
};

function addEventListnerForInputEnter() {
  inputTxtElm.addEventListener("keypress", onKeyPressed);
}

function onKeyPressed(event) {
  var inputText = "";
   if (event.key == "Enter") {
    inputText = inputTxtElm.value;
     console.log(inputText.length);
    // var arrStrInts = strInput.split(" ");
    // var inputVal = parseInt(inputText);
   
    inputTxtElm.style.visibility = "hidden";
    mmCentralNode.drawText(inputText);
    inputTxtElm.removeEventListener("keypress", onKeyPressed);
  }
}

function clickXYtoCavasXY(x, y) {
  return { x: (x - boundingRect.left) * kX, y: (y - boundingRect.y) * kY };
}

c.addEventListener("dblclick", function (event) {
  var posInCanvas = clickXYtoCavasXY(event.clientX, event.clientY);
  if (mmCentralNode.isSelected) {
    mmCentralNode.drawText("");
  }
});

/* Add new nodes */
c.addEventListener("Insert", function (event) {
  var posInCanvas = clickXYtoCavasXY(event.clientX, event.clientY);
  if (!mmCentralNode.isSelected){
    c.createNewNode();
  }
});

 /* Draw a line */
function drawLine(){
  if(mmCentralNode.isSelected){
    var pi = Math.PI;
    ctx.beginPath();
    ctx.lineWidth = 5;
    ctx.strokeStyle = "black";
    ctx.arc(x, y, 75, 0, pi/2, false );
    ctx.stroke();
 
  }
}