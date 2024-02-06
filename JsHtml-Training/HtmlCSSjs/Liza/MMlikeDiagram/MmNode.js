class MmNode {
  ctx;
  x;
  y;
  width;
  height;
  text;
  isSelected = false;

  // todo: next step -> think about related mmNodes
  // the child nodes of this node,it is an array of nodes
  childs = [];

  constructor(ctx, x, y, w, h) {
    this.ctx = ctx;
    this.x = x;
    this.y = y;
    this.width = w;
    this.height = h;
    this.text = "";
  }

  draw() {
    ctx.strokeStyle = "darkblue";
    ctx.beginPath();
    ctx.roundRect(cx, cy, wRect, hRect, hRect / 5);
    ctx.stroke();
  }

  isInside(x, y) {
    return (
      this.x <= x &&
      x <= this.x + this.width &&
      this.y <= y &&
      y <= this.y + this.height
    );
  }

  drawSelection() {
    this.isSelected = true;
    this.ctx.strokeStyle = "black";
    this.ctx.beginPath();
    this.ctx.lineWidth = "3";
    this.ctx.moveTo(this.x - 3, this.y - 3);
    this.ctx.lineTo(this.x + this.width + 3, this.y - 3);
    this.ctx.lineTo(this.x + this.width + 3, this.y + this.height + 3);
    this.ctx.lineTo(this.x - 3, this.y + this.height + 3);
    this.ctx.closePath();
    this.ctx.stroke(); // Draw it
    //    ctx.rect(this.x - 3, this.y - 3, this.width + 6, this.height + 6);
    this.ctx.strokeStyle = "blue";
  }

  clearSelection() {
    if (!this.isSelected) return;
    this.ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    this.draw();
  }

  drawText(text) {
    let numberOfLines = 3;
    let fontSize = Math.floor(this.height / numberOfLines);
    this.ctx.font = `${fontSize}px serif`;
    let horSizeOfText = this.ctx.measureText(text).width;
    let noOfLines = horSizeOfText / this.width;
    let arrOfLines = [];
    arrOfLines = splitToLines();

    if (noOfLines > numberOfLines) {
      console.log(noOfLines, numberOfLines);
    } else {
      arrOfLines = splitToLines();
    }
    if (horSizeOfText > this.width) {
      arrOfLines = splitText(text);
      showSplittedText(arrOfLines);
    }
    let textOffsetX = (this.width - horSizeOfText) / 2;
    this.ctx.fillText(text, this.x + textOffsetX, this.y + this.height / 2);
  }

  splitToLines() {
    let desiredLineSize = Math.floor(0.9 * this.width);
    var arrOfWords = strToArrayWordsPlusDelimiters(this.text, ',. !?" -+*/=');
    var arrOfLines = [];
    var curLineSize = 0;
    var line = "";
    for (let i = 0; i < arrOfWords; i++) {
      let horSizeOfWord = this.ctx.measureText(arrOfWords[i]).width;

      if (curLineSize + horSizeOfWord < this.width) {
        line += arrOfWords[i];
        curLineSize += horSizeOfWord;
        continue;
      }
      if (line == "") {
        line = arrOfWords[i];
        arrOfLines.push(line);
        curLineSize = 0;
        line = "";
        continue;
      } else {
      }
      arrOfLines.push(line);
    }
  }

  /* Create new nodes */
  createNewNode() {
    ctx.strokeStyle = "darkblue";
    ctx.beginPath();
    ctx.roundRect(cx + 10, cy + 10, wRect + 15, hRect + 15, hRect / 5);
    ctx.stroke();
  }
  splitText(text) {
    // try current font size;
    // step1/ calc length
    // a) (ex: box 100, text < 100) -> one line nothing to do
    // b) text : 500, then split by words. find words for the 1st line; 2nd, ...nth.
    //     n< 5 -> ok
    //     n>5 20% ext.box > 20%, but< 50%  make font smaller /1.5 and go to step1
    //    if fails increase the box size to 10 lines and size of length and go to step1
    //    if fails in this case (show first part)+ <more...>
    var words = text.split(/[\s,.?!]/);
    // splitToWordsWithDelimiter()
    // word1, word2! word3 word4.
    // ["word1, ", "word2! ", "word3 ", "word4."]
  }
}
