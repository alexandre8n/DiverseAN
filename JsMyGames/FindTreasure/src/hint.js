const Hint = {
  Found: { name: "Found", text: "", bkgColor: "blue", txtColor: "yellow" },
  BoilingHot: {
    name: "BoilingHot",
    text: "Boiling hot! (Очень горячо!)",
    bkgColor: "yellow",
    txtColor: "MidnightBlue",
  },
  ReallyHot: {
    name: "ReallyHot",
    text: "Really hot! ( Совсем горячо!)",
    bkgColor: "red",
    txtColor: "yellow",
  },
  Hot: {
    name: "Hot",
    text: "Hot! (Горячо!)",
    bkgColor: "OrangeRed",
    txtColor: "yellow",
  },
  Warm: {
    name: "Warm",
    text: "Warm! (Тепло!)",
    bkgColor: "ForestGreen",
    txtColor: "yellow",
  },
  Cold: {
    name: "Cold",
    text: "Cold! (Холодно!)",
    bkgColor: "BlueViolet",
    txtColor: "yellow",
  },
  ReallyCold: {
    name: "ReallyCold",
    text: "Really Cold! (Совсем холодно!)",
    bkgColor: "DarkBlue",
    txtColor: "yellow",
  },
  Freezing: {
    name: "Freezing",
    text: "Freezing! (Леденящий холод!)",
    bkgColor: "Black",
    txtColor: "yellow",
  },
};
var breadcrumbs = [];

function clearBreadcrumbs() {
  breadcrumbs = [];
}
function addBreadcrumb(x, y, hint) {
  breadcrumbs.push({ x: x, y: y, hint: hint });
}

function redrawBreadcrumbs() {
  breadcrumbs.forEach((bc) => {
    drawBreadcrumb(bc.x, bc.y, bc.hint);
  });
}

function getDistanceHint(distance) {
  if (distance < 1.5) {
    return Hint.Found;
  }
  if (distance < 2.5) {
    return Hint.BoilingHot;
  } else if (distance < 4.7) {
    return Hint.ReallyHot;
  } else if (distance < 9.3) {
    return Hint.Hot;
  } else if (distance < 18.6) {
    return Hint.Warm;
  } else if (distance < 37) {
    return Hint.Cold;
  } else if (distance < 70) {
    return Hint.ReallyCold;
  }
  return Hint.Freezing;
}

function drawPoint(x, y, r, hint) {
  ctx.beginPath();
  ctx.strokeStyle = "black";
  ctx.fillStyle = hint.bkgColor;
  ctx.lineWidth = 1;
  ctx.arc(x, y, r, 0, 2 * Math.PI);
  ctx.stroke();
  ctx.fill();
}

function drawBreadcrumb(x, y, hint) {
  const mapRect = getRectImg(imgMap);
  var xOfImg = (x * mapRect.width) / 100;
  var yOfImg = (y * mapRect.height) / 100;
  var r = canvas.clientWidth * radiusOfBreadCrumb;
  drawPoint(xOfImg, yOfImg, r, hint);
}

function keepBreadcrumb(xClick, yClick, hint) {
  drawBreadcrumb(xClick, yClick, hint);
  addBreadcrumb(xClick, yClick, hint);
}
