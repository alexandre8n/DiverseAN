var gameStarted = false;
var gamesCount = 0;
var totalScore = 0;
var totalTimeInSec = 0;
var gameScore = 0;
var gameTimeInSec = 0;
var bestScore = 0;
var bestTimeInSec = 0;
const Levels = [
  { level: 0, x: 3, y: 2 },
  { level: 1, x: 3, y: 3 },
  { level: 2, x: 4, y: 3 },
  { level: 3, x: 4, y: 4 },
  { level: 4, x: 5, y: 4 },
  { level: 5, x: 5, y: 5 },
  { level: 6, x: 6, y: 5 },
  { level: 7, x: 6, y: 6 },
  { level: 8, x: 7, y: 6 },
];
var levelXY = { level: 0, x: 3, y: 2 };

const mainDiv = document.getElementById("mainDiv");
const minImgNo = 1;
const maxImgNo = 64;
var numbers = [];

var btnUpDn = document.getElementById("btnUpDn");
btnUpDn.addEventListener("click", onLevelUpDown);

levelXY = setGameLevel(1);
defaultField();
window.addEventListener("keydown", onKeyDown);

function defaultField() {
  numbers = Array.from({ length: levelXY.x * levelXY.y }, (v, k) => -1);
  createPlayField();
}

function onStartGame() {
  if (gameStarted) {
    gameStarted = false;
    defaultField();
    document.getElementById("btnStartStop").innerHTML = "Start Game";
    return;
  }
  gameStarted = true;
  document.getElementById("btnStartStop").innerHTML = "Stop Game";
  var size = levelXY.x * levelXY.y;
  numbers = generateRandomNoDupplicate(size, minImgNo, maxImgNo);
  createATwin();
  gameStarted = true;
  createPlayField();
  startStopWatch();
}

function clearPlayField() {
  while (mainDiv.firstChild) {
    mainDiv.removeChild(mainDiv.lastChild);
  }
}

function createPlayField() {
  clearPlayField();
  var mn = levelXY.x * levelXY.y;
  for (let j = 1; j <= levelXY.y; j++) {
    var rowElm = addRow(j, levelXY.x);
    mainDiv.appendChild(rowElm);
  }
}
function addRow(nRow, nInRow) {
  const newDiv = document.createElement("div");
  newDiv.className = "row";
  for (let i = 1; i <= nInRow; i++) {
    const img = document.createElement("img");
    var idx = (nRow - 1) * nInRow + i - 1;
    img.alt = "" + numbers[idx];
    img.src = "../Images/" + numbers[idx] + ".png";
    img.id = "twin" + img.alt;
    if (gameStarted) img.addEventListener("click", onImageClicked);
    newDiv.appendChild(img);
  }
  return newDiv;
}

var twinImgNo = -1;

function onImageClicked(event) {
  var imgId = event.srcElement.alt;
  var success = imgId == twinImgNo;
  gameStarted = false;
  document.getElementById("btnStartStop").innerHTML = "Start Game";

  updateStatistics(success);
  if (success) {
    //showSuccessMessage();
    if (gameScore >= totalScore / gamesCount) {
      levelXY = setGameLevel(levelXY.level + 1);
    }
  } else {
    showFailMessage();
  }
  defaultField();
}

function showSuccessMessage() {
  var modal = document.getElementById("modal");
  openModal(modal);
}
function showFailMessage() {
  var modal = document.getElementById("modal");
  openModal(modal);
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

function updateStatistics(success) {
  gamesCount++;
  gameTimeInSec = elapsedTime() / 1000;
  gameScore = calcScore(success);
  totalScore += gameScore;
  totalTimeInSec += gameTimeInSec;
  var avgScore = Math.round(totalScore / gamesCount);
  var avgTimeInSec = Math.round(totalTimeInSec / gamesCount);
  if (bestScore < gameScore) {
    bestScore = gameScore;
    bestTimeInSec = gameTimeInSec;
  }
  document.getElementById("gamesCount").innerHTML = "" + gamesCount;
  document.getElementById("totScore").innerHTML = "" + totalScore;
  document.getElementById("totTime").innerHTML = "" + totalTimeInSec.toFixed(1);
  document.getElementById("gameScore").innerHTML = "" + gameScore;
  document.getElementById("gameTime").innerHTML = "" + gameTimeInSec.toFixed(1);
  document.getElementById("bestScore").innerHTML = "" + bestScore;
  document.getElementById("bestGameTime").innerHTML =
    "" + bestTimeInSec.toFixed(1);
  document.getElementById("avgScore").innerHTML = "" + avgScore;
  document.getElementById("avgTime").innerHTML = "" + avgTimeInSec.toFixed(1);
}

function calcScore(success) {
  if (!success) return 0;
  // desired score:
  //1 sec. = 50, 2 sec. 30, 3 sec. 20, 4 -> 15, 5->12, 10 -> 7, 20 -> 4, more 3
  var x = Math.max(0.1, gameTimeInSec - 1);
  var score = (400 * x + 10) / (7 * x * x + x + 0.1);
  var koef = calcLevelKoef();
  return Math.round(score * koef);
}

function calcLevelKoef() {
  var nLevel0 = Levels[0].x * Levels[0].y;
  var nCurLevel = Levels[levelXY.level].x * Levels[levelXY.level].y;
  var cofL0by2 = nLevel0 * (nLevel0 - 1);
  var cofLcurBy2 = nCurLevel * (nCurLevel - 1);
  return cofLcurBy2 / cofL0by2;
}

function createATwin() {
  var size = numbers.length;
  var twinsArray = generateRandomNoDupplicate(2, 0, size - 1);
  numbers[twinsArray[0]] = numbers[twinsArray[1]];
  twinImgNo = numbers[twinsArray[0]];
}

function onLevelUpDown(event) {
  if (gameStarted) return;
  var h = btnUpDn.clientHeight;
  if (event.offsetY < h / 2) levelXY = setGameLevel(levelXY.level + 1);
  else levelXY = setGameLevel(levelXY.level - 1);
}
function setGameLevel(iLevel) {
  if (iLevel < 0) iLevel = 0;
  if (iLevel > 7) iLevel = 8;
  var lvl = Levels[iLevel];
  document.getElementById("gameLevel").innerHTML = "" + iLevel;
  return lvl;
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
    let imgElm = document.getElementById("twin" + twinImgNo);
    if (!imgElm) return;
    const isVisibleNow = isTwinVisible(imgElm);
    makeTwinVisible(imgElm, !isVisibleNow);
    sTimeStamp = 0;
    nOfS = 0;
  }

  function makeTwinVisible(imgElm, makeVisible) {
    imgElm.setAttribute("class", "");
    if (makeVisible) {
      imgElm.setAttribute("class", "filtered-pic");
    }
  }

  function isTwinVisible(imgElm) {
    if (imgElm.className == "filtered-pic") return true;
    return false;
  }
}

// done: discover how to know what image is clicked
// done: start game button
// done: create a stop, process a stop
// done: generate an array of random numbers between (1-64),
//        no duplicates
// done: stopwatch
// done: score table update
// done: change level
// done: score(level, time)
// done: swow result
