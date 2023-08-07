var gamesCount = 0;
var totalScore = 0;
var totalClicks = 0;
var gameScore = 0;
var gameClicks = 0;
var timeOfLastGameInSec = 0;
var bestScore = 0;
var bestClicks = 0;

var gameIsStarted = false;
var cliHeight = 0;
var radiusOfBreadCrumb = 0.01;

var imgMap = document.getElementById("img1");
var imgTreasure = document.getElementById("treasureImg");
var canvas = document.getElementById("gameCanvas");
var ctx = canvas.getContext("2d");

// image info is specified in terms of % of image size
var mapImageInfo = { x: 0, y: 0, width: 0, height: 0 };
var target = { x: 0, y: 0 };
var steps = 0;

window.addEventListener("resize", afterResize);
canvas.addEventListener("mousedown", mouseDown);
window.addEventListener("keydown", onKeyDown);
afterResize(null);

var x = canvas.clientWidth / 2;
var y = canvas.clientHeight / 2;

var bubble = new Bubble(ctx, null);
bubble.bubbleHeight = 100;
bubble.bkgColor = "blue";
bubble.txtColor = "yellow";
const greeting = "Hi, Start the game and try to find treasure!";
bubble.draw(x, y, greeting, false);
bubble.notifyClear = afterClear;

function afterClear(x, y, width, height) {
  redrawBreadcrumbs();
}

function getDistanceInPercents(offsetX, offsetY) {
  var diffX = offsetX - target.x;
  var diffY = offsetY - target.y;
  return Math.sqrt(diffX * diffX + diffY * diffY);
}

function mouseDown(event) {
  if (!gameIsStarted) {
    return;
  }
  steps++;

  const mapRect = getRectImg(imgMap);
  //const tresureRect = getRectImg(imgTreasure);
  var xOfImg = event.clientX - mapRect.left;
  var yOfImg = event.clientY - mapRect.top;
  var xClick = (100 * xOfImg) / mapRect.width;
  var yClick = (100 * yOfImg) / mapRect.height;

  const distance = getDistanceInPercents(xClick, yClick);
  // hint: {title: "Hot", text: "Hot!(Горячо!)" }
  const hint = getDistanceHint(distance);
  if (hint === Hint.Found) {
    showFound(xOfImg, yOfImg);
    stopGame();
    return;
  }
  showHint(hint, xOfImg, yOfImg);
  keepBreadcrumb(xClick, yClick, hint);
}

function showHint(hint, x, y) {
  xOfBubble = x;
  yOfBubble = y;
  bubble.bkgColor = hint.bkgColor;
  bubble.txtColor = hint.txtColor;
  bubble.draw(x, y, hint.text, true);
}

var time0 = 0;
function startStopWatch() {
  const d = new Date(); // used for debug
  time0 = d.getTime();
}
function elapsedTime() {
  const d = new Date();
  return d.getTime() - time0;
}

function showFound(x, y) {
  setTreasurePosition(target, true);
  bubble.bubbleHeight = 100;
  bubble.bkgColor = "blue";
  bubble.txtColor = "yellow";
  bubble.showFound(
    x,
    y,
    "Congratulations, you have found it in " + steps + " steps!"
  );
  updateStatistics();
}

function afterResize(event) {
  cliWidth = document.documentElement.clientWidth;
  cliHeight = document.documentElement.clientHeight;
  var rect = imgMap.getBoundingClientRect();

  canvas = document.getElementById("gameCanvas");
  const rectImg = getRectImg(imgMap);
  canvas.style.position = "absolute";
  canvas.style.top = rectImg.top + "px";
  canvas.style.left = rectImg.left + "px";
  canvas.style.width = rectImg.right - rectImg.left + "px";
  canvas.style.height = rectImg.bottom - rectImg.top + "px";
  canvas.width = rectImg.right - rectImg.left;
  canvas.height = rectImg.bottom - rectImg.top;

  ctx.font = "15px Helvetica";
  if (isTreasureVisible()) {
    setTreasurePosition(target, true);
  }
}

function getRectImg(img) {
  var rect = img.getBoundingClientRect();
  return rect;
}

function onChangeMap() {
  var src = imgMap.src;
  var charPos = src.length - 5;
  var charWithNo = src[charPos];
  charWithNo = (charWithNo % 5) + 1;
  src = setCharAt(src, charPos, charWithNo);
  imgMap.src = src;
}

function setCharAt(str, index, chr) {
  if (index > str.length - 1) return str;
  return str.substring(0, index) + chr + str.substring(index + 1);
}

function onStartGame() {
  if (gameIsStarted) {
    stopGame();
    bubble.clear();
    return;
  }
  restartGame();
}
function stopGame() {
  gameIsStarted = false;
  const btn = document.getElementById("btnStartStop");
  btn.innerHTML = "Start";
  bubble.setDefaults();
}
var getRandomNumber = function (size) {
  return Math.floor(Math.random() * size);
};

function restartGame() {
  if (gamesCount > 0) {
    onChangeMap();
  }
  bubble.clear();
  clearBreadcrumbs();
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  initStatistics();
  gameIsStarted = true;
  const btn = document.getElementById("btnStartStop");
  btn.innerHTML = "Stop";
  target.x = getRandomNumber(100);
  target.y = getRandomNumber(100);
  setTreasurePosition(target, false);
}
function setTreasurePosition(target, isVisible) {
  var rect = getRectImg(imgMap);
  var xDelta = imgTreasure.width / 2;
  var yDelta = imgTreasure.height / 2;

  var xAbs = rect.left + 0.01 * target.x * rect.width - xDelta;
  var yAbs = rect.top + 0.01 * target.y * rect.height - yDelta;
  imgTreasure.style.position = "absolute";
  imgTreasure.style.top = yAbs + "px";
  imgTreasure.style.left = xAbs + "px";
  imgTreasure.style.visibility = isVisible ? "visible" : "hidden";
}
function isTreasureVisible() {
  return imgTreasure.style.visibility === "visible";
}
function initStatistics() {
  steps = 0;
  gameClicks = 0;
  startStopWatch();
}
function updateStatistics() {
  gamesCount++;
  gameClicks = steps;
  totalClicks += steps;
  timeOfLastGameInSec = Math.floor(elapsedTime() / 1000);
  const avgTime = timeOfLastGameInSec / steps;
  gameScore = calcScore(avgTime);
  totalScore += gameScore;
  if (gameScore > bestScore) {
    bestScore = gameScore;
    bestClicks = gameClicks;
  }
  var avgScore = Math.round(totalScore / gamesCount);
  var avgClicks = Math.round(totalClicks / gamesCount);

  document.getElementById("gamesCount").innerHTML = "" + gamesCount;
  document.getElementById("totScore").innerHTML = "" + totalScore;
  document.getElementById("totClicks").innerHTML = "" + totalClicks;
  document.getElementById("gameScore").innerHTML = "" + gameScore;
  document.getElementById("gameClicks").innerHTML = "" + gameClicks;
  document.getElementById("bestScore").innerHTML = "" + bestScore;
  document.getElementById("bestClicks").innerHTML = "" + bestClicks;
  document.getElementById("avgScore").innerHTML = "" + avgScore;
  document.getElementById("avgClicks").innerHTML = "" + avgClicks;
}
function calcScore(avgTime) {
  let scoreForTimeInPerCents = 0;
  if (avgTime < 1) {
    scoreForTimeInPerCents = 0.25;
  } else if (avgTime < 2) {
    scoreForTimeInPerCents = 0.15;
  } else if (avgTime < 3) {
    scoreForTimeInPerCents = 0.1;
  } else if (avgTime < 5) {
    scoreForTimeInPerCents = 0.05;
  }

  var score4Clicks = 5000 / (steps * steps + 50);
  gameScore = Math.floor(score4Clicks * (1 + scoreForTimeInPerCents));
  return gameScore;
}

var sTimeStamp = 0;
var nOfS = 0;
function onKeyDown(event) {
  const isTimeOut = sTimeStamp != 0 && event.timeStamp - sTimeStamp > 1000;
  if (event.key != "s" || isTimeOut) {
    nOfS = 0;
    sTimeStamp = 0;
    return;
  }

  nOfS++;
  if (nOfS == 1) {
    sTimeStamp = event.timeStamp;
  } else if (nOfS == 3) {
    const isVisibleNow = isTreasureVisible();
    setTreasurePosition(target, !isVisibleNow);
    sTimeStamp = 0;
    nOfS = 0;
  }
}
