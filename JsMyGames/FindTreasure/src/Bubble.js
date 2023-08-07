const bubblePointers = { 1: "TL", 2: "TR", 3: "BR", 4: "BL" };
const foundPointCorrections = { T: +1, B: -1, L: +1, R: -1 };
var pointerCorrection = {
  TR: { x: +1, y: -1 },
  BR: { x: +1, y: +1 },
  BL: { x: -1, y: +1 },
  TL: { x: -1, y: -1 },
};

class Bubble {
  ctx = null;
  config = {
    radius: 20,
    lineWidth: 2,
    timeoutInMs: 3000,
    bubbleHeight: 50,
    delta: 40,
    bkgColor: "red",
    txtColor: "yellow",
    borderColor: "black",
  };

  bubbleHeight = 50;
  delta = 40; // size from point to pointer
  bkgColor = "red";
  txtColor = "yellow";
  borderColor = "black";
  radius = 20;
  lineWidth = 2;
  timeoutInMs = 3000;

  lastBubbleRect = { x: -1, y: -1, width: -1, height: -1 };
  bubbleTimeout = null;
  bublePointer = ""; // possible: TL, TR, BR, BL
  basePoints = {
    TL: { x: 0, y: 0 },
    TR: { x: 0, y: 0 },
    BR: { x: 0, y: 0 },
    BL: { x: 0, y: 0 },
  };
  notifyClear = (x, y, width, height) => {
    return;
  };

  constructor(ctx, defaultConfig) {
    this.ctx = ctx;
    if (defaultConfig != null) {
      this.config = defaultConfig;
    }
    this.setDefaults();
  }

  setDefaults() {
    this.bubbleHeight = this.config.bubbleHeight;
    this.delta = this.config.delta; // size of x, y from point to pointer
    this.bkgColor = this.config.bkgColor;
    this.txtColor = this.config.txtColor;
    this.borderColor = this.config.borderColor;
    this.radius = this.config.radius;
    this.lineWidth = this.config.lineWidth;
    this.timeoutInMs = this.config.timeoutInMs;
  }

  clear() {
    const delta = this.radius + this.lineWidth;
    ctx.clearRect(
      this.lastBubbleRect.x,
      this.lastBubbleRect.y,
      this.lastBubbleRect.width + delta,
      this.lastBubbleRect.height + delta
    );

    clearTimeout(this.bubbleTimeout);

    if (this.notifyClear != null) {
      this.notifyClear(
        this.lastBubbleRect.x,
        this.lastBubbleRect.y,
        this.lastBubbleRect.width + delta,
        this.lastBubbleRect.height + delta
      );
    }
  }

  showFound(x, y, text) {
    this.draw(x, y, text, false);
    return;
  }

  getQuadrant(x, y) {
    //  quadrant numbers
    //   1         2
    //   4         3
    var sizeWidth = ctx.canvas.clientWidth;
    var sizeHeight = ctx.canvas.clientHeight;
    var quadrantNo = x < sizeWidth / 2 ? 14 : 23;
    quadrantNo =
      y < sizeHeight / 2 ? Math.floor(quadrantNo / 10) : quadrantNo % 10;
    return quadrantNo;
  }

  ///////////////////////////////////////////
  // new version functions
  draw(x, y, text, needClear = true) {
    if (this.lastBubbleRect.width > 0) {
      this.clear();
    }
    const widthOfText = ctx.measureText(text).width + 40;
    const quadrant = this.getQuadrant(x, y);
    const pointers = { 1: "TL", 2: "TR", 3: "BR", 4: "BL" };
    const pointer = pointers[quadrant];
    var delta = this.delta;
    var x1 = x + delta;
    var y1 = y + delta;

    if (quadrant == 4) {
      y1 = y - delta - this.bubbleHeight;
    } else if (quadrant == 3) {
      x1 = x - delta - widthOfText;
      y1 = y - delta - this.bubbleHeight;
    } else if (quadrant == 2) {
      x1 = x - delta - widthOfText;
    }

    ctx.strokeStyle = "black";
    ctx.fillStyle = this.bkgColor;
    ctx.lineWidth = 3;
    this.drawBubble2(x1, y1, widthOfText, this.bubbleHeight, pointer);

    ctx.fillStyle = this.txtColor;
    ctx.fillText(text, x1 + 20, y1 + 30);
    this.saveLastBubbleRect(pointer);
    var myClear = () => {
      this.clear();
    };

    if (needClear) {
      this.bubbleTimeout = setTimeout(myClear, this.timeoutInMs);
    } else {
      this.bubbleTimeout = null;
      //this.bubbleTimeout = setTimeout(myClear, 5000);
    }
  }

  drawBubble2(x, y, w, h, pointer) {
    // 4 basic points
    this.basePoints["TL"] = { x: x, y: y };
    this.basePoints["TR"] = { x: x + w, y: y };
    this.basePoints["BR"] = { x: x + w, y: y + h };
    this.basePoints["BL"] = { x: x, y: y + h };
    const radius = this.radius;

    var curPos = { x: x + radius, y: y };
    ctx.beginPath();
    ctx.moveTo(curPos.x, curPos.y);
    curPos = this.drawHorLine(curPos, w - 2 * radius);
    curPos = this.drawCorner(curPos, "TR", pointer);
    curPos = this.drawVertLine(curPos, h - 2 * radius);
    curPos = this.drawCorner(curPos, "BR", pointer);
    curPos = this.drawHorLine(curPos, -w + 2 * radius);
    curPos = this.drawCorner(curPos, "BL", pointer);
    curPos = this.drawVertLine(curPos, -h + 2 * radius);
    curPos = this.drawCorner(curPos, "TL", pointer);
    ctx.stroke();
    ctx.fill();
  }

  drawHorLine(curPos, len) {
    ctx.lineTo(curPos.x + len, curPos.y);
    return { x: curPos.x + len, y: curPos.y };
  }
  drawVertLine(curPos, len) {
    ctx.lineTo(curPos.x, curPos.y + len);
    return { x: curPos.x, y: curPos.y + len };
  }

  drawCorner(curPos, cornerToDraw, pointerCorner) {
    const radius = this.radius;
    var basicPoint = this.basePoints[cornerToDraw];
    var delta = radius / 2;
    var directionKoefX = cornerToDraw[0] == "T" ? 1 : -1;
    var directionKoefY = cornerToDraw[1] == "R" ? 1 : -1;
    var pointerKoef = pointerCorrection[pointerCorner];
    const resPoint = {
      x: curPos.x + directionKoefX * radius,
      y: curPos.y + directionKoefY * radius,
    };
    if (cornerToDraw !== pointerCorner) {
      ctx.quadraticCurveTo(basicPoint.x, basicPoint.y, resPoint.x, resPoint.y);
      return resPoint;
    }

    // draw the bubble pointer

    ctx.quadraticCurveTo(
      basicPoint.x,
      basicPoint.y,
      basicPoint.x + pointerKoef.x * delta,
      basicPoint.y + pointerKoef.y * delta
    );
    curPos.x += directionKoefX * radius;
    curPos.y += directionKoefY * radius;
    ctx.lineTo(curPos.x, curPos.y);
    return curPos;
  }

  saveLastBubbleRect(pointer) {
    var r = this.radius / 2;
    var topL = this.basePoints["TL"];
    var botR = this.basePoints["BR"];
    var x = topL.x - this.lineWidth - r;
    var y = topL.y - this.lineWidth - r;
    var w = botR.x - topL.x + 2 * this.lineWidth + 2 * r;
    var h = botR.y - topL.y + 2 * this.lineWidth + 2 * r;
    this.lastBubbleRect = { x: x, y: y, width: w, height: h };
  }
}
