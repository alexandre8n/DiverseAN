// imports rowColBySeqNo from ../../../JsUtils/utils1.js
//
var gameStarted = false;
var gamesCount = 0;
var totalScore = 0;
var totalTimeInSec = 0;
var gameScore = 0;
var gameTimeInSec = 0;
var guessTimeInSec = 0;
var bestScore = 0;
var bestTimeInSec = 0;
var successCount = 0; // count of successful games on level
var failCount = 0; // count of failed guesses in a game
const Levels = [
  { level: 0, x: 3, y: 2, min: 2, max: 3, imgCount: -1 },
  { level: 1, x: 3, y: 3, min: 2, max: 5, imgCount: -1 },
  { level: 2, x: 4, y: 3, min: 3, max: 6, imgCount: -1 },
  { level: 3, x: 4, y: 4, min: 4, max: 7, imgCount: -1 },
  { level: 4, x: 5, y: 4, min: 4, max: 8, imgCount: -1 },
  { level: 5, x: 5, y: 5, min: 4, max: 8, imgCount: -1 },
  { level: 6, x: 6, y: 5, min: 4, max: 8, imgCount: -1 },
  { level: 7, x: 6, y: 6, min: 4, max: 9, imgCount: -1 },
  { level: 8, x: 6, y: 7, min: 4, max: 10, imgCount: -1 },
];
const imgToGuess = "imgToGuess";

const mainDiv = document.getElementById("mainDiv");
const minImgNo = 1;
const maxImgNo = 64;

// here there will be the number of images to be demonstrated
// with their coordinates: {number: 1, row: 0, col: 0}
var numbers = [];

// here the array of objs: number of image with coordinates:
// {number: 1, row: 0, col: 0, timeInSec: 0.0, score: 0}
var imagesToGuessCoordinates = [];

const delay = (ms) => new Promise((res) => setTimeout(res, ms));

var btnUpDn = document.getElementById("btnUpDn");
btnUpDn.addEventListener("click", onLevelUpDown);
window.addEventListener("keydown", onKeyDown);

var levelXY = setGameLevel(0, 0);
defaultField();

function defaultField() {
  numbers = Array.from({ length: levelXY.x * levelXY.y }, (v, k) => -1);
  createPlayField();
  setShowScreenPos(); // the box where the image to guess position will be shown
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
    img.src = getImgSrc(numbers[idx]);
    img.id = getImgIdByXY(i, nRow);
    img.addEventListener("click", onImageClicked);
    newDiv.appendChild(img);
  }
  return newDiv;
}

function getImgSrc(imgSrcId) {
  return "../Images/" + imgSrcId + ".png";
}
function getImgIdByXY(x, y) {
  return `${imgToGuess}${x}-${y}`;
}
function getXYfromId(idStr) {
  var colRowStr = idStr.substring(imgToGuess.length);
  var ar = colRowStr.split("-");
  return { x: parseInt(ar[0]), y: parseInt(ar[1]) };
}

function setShowScreenPos() {
  const showScreen = document.getElementById("showScreen");
  const firstImg = document.getElementById(`${imgToGuess}${1}-${1}`);
  var rectOf1_1 = firstImg.getBoundingClientRect();

  showScreen.style.position = "absolute";
  var posY = Math.floor(rectOf1_1.y + 2 + rectOf1_1.height / 2);
  var posX = Math.floor(rectOf1_1.x - rectOf1_1.width - 27);
  showScreen.style.top = "" + posY + "px";
  showScreen.style.left = "" + posX + "px";
  showScreen.style.display = "block";
  showScreen.src = getImgSrc("-1");
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
  imagesToGuessCoordinates = prepareImagesToGuess();

  proceedWithDemo();
}

// returns imagesToGuessCoordinates
function prepareImagesToGuess() {
  var whatToGues = [];
  numbers = generateRandomNoDupplicate(levelXY.imgCount, minImgNo, maxImgNo);
  //createPlayField();
  // imges ids are in Numbers, imgToGuessPositions is of the same size
  var imgToGuessPositions = generateRandomNoDupplicate(
    levelXY.imgCount, // number of images to be demonstrated, to be remembered
    0,
    levelXY.x * levelXY.y - 1 // size of field 0 to m*n-1
  );
  for (let i = 0; i < numbers.length; i++) {
    var { row, col } = rowColBySeqNo(levelXY.x, imgToGuessPositions[i]);
    var imgToGuessInfo = {
      number: numbers[i],
      row: row,
      col: col,
      timeInSec: 0,
      score: 0,
    };
    whatToGues.push(imgToGuessInfo);
  }
  return whatToGues;
}

// this idx is an index of image that should be guessed by player
var currentImageIdx = -1;
function getPlayerGuesses(guessNo) {
  currentImageIdx = guessNo;
  var imgInfo = imagesToGuessCoordinates[currentImageIdx];
  showScreen.src = getImgSrc(imgInfo.number);
  startStopWatch(); // to move for evaluation of guesses
}

function onImageClicked(event) {
  if (!gameStarted) return;
  guessTimeInSec = elapsedTime() / 1000;
  var imgIdStr = event.srcElement.id;
  var xy = getXYfromId(imgIdStr);
  var imgInfo = imagesToGuessCoordinates[currentImageIdx];
  var success = xy.x - 1 == imgInfo.col && xy.y - 1 == imgInfo.row;
  imgInfo.timeInSec = guessTimeInSec;
  imgInfo.score = calcScore(success, imgInfo);
  gameScore += imgInfo.score;
  gameTimeInSec += guessTimeInSec;

  var proceedAfterGuess = async () => {
    var img = document.getElementById(imgIdStr);
    if (success) {
      img.src = getImgSrc(imgInfo.number);
    } else {
      img.src = getImgSrc("sorry");
      failCount++;
    }
    await delay(800);
    img.src = getImgSrc("-1");
    showScreen.src = getImgSrc("-1");
    if (currentImageIdx < imagesToGuessCoordinates.length - 1) {
      getPlayerGuesses(currentImageIdx + 1);
      return;
    }
    gameStarted = false;
    updateStatistics();
    gameScore = 0;
    if (failCount == 0) {
      changeGameLevel(+1);
    } else {
      changeGameLevel(-1);
    }
    failCount = 0;
    document.getElementById("btnStartStop").innerHTML = "Start Game";
    defaultField();
  };
  proceedAfterGuess();
}

// array of such objects: {idx: 0, timeInSec: 0, score: 0} score 0 not guessed
//var guessResults = [];

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

function updateStatistics() {
  gamesCount++;
  guessTimeInSec = elapsedTime() / 1000;
  totalScore += gameScore;
  totalTimeInSec += guessTimeInSec;
  var avgScore = Math.round(totalScore / gamesCount);
  var avgTimeInSec = Math.round(totalTimeInSec / gamesCount);
  if (bestScore < gameScore) {
    bestScore = gameScore;
    bestTimeInSec = guessTimeInSec;
  }
  document.getElementById("gamesCount").innerHTML = "" + gamesCount;
  document.getElementById("totScore").innerHTML = "" + totalScore.toFixed(1);
  document.getElementById("totTime").innerHTML = "" + totalTimeInSec.toFixed(1);
  document.getElementById("gameScore").innerHTML = "" + gameScore.toFixed(1);
  document.getElementById("gameTime").innerHTML =
    "" + guessTimeInSec.toFixed(1);
  document.getElementById("bestScore").innerHTML = "" + bestScore.toFixed(1);
  document.getElementById("bestGameTime").innerHTML =
    "" + bestTimeInSec.toFixed(1);
  document.getElementById("avgScore").innerHTML = "" + avgScore;
  document.getElementById("avgTime").innerHTML = "" + avgTimeInSec.toFixed(1);
}

function calcScore(success, timeInSec) {
  if (!success) return 0;
  // desired score level 0, 1 sec -> max, 2 sec -> -5%, 4sec and longer - 10%:
  // 1 sec. = 10
  var timeKoef = timeInSec < 1.5 ? 1 : timeInSec < 4 ? 0.95 : 0.9;
  var scoreBase = 10 * timeKoef;
  var koef = calcLevelKoef();
  var score = koef * scoreBase;
  return score;
}

const LevelKoefsOnNofImg = {
  2: 1,
  3: 1.3,
  4: 1.8,
  5: 2.5,
  6: 4,
  7: 7,
  8: 12,
  9: 20,
};
function calcLevelKoef() {
  var nLevel0 = Levels[0].x * Levels[0].y;
  var nCurLevel = Levels[levelXY.level].x * Levels[levelXY.level].y;
  var levelOfFieldSize = nCurLevel / nLevel0;
  var imgCount = levelXY.imgCount;
  var kOfN = LevelKoefsOnNofImg[levelXY.imgCount];
  return kOfN * levelOfFieldSize;
}

function proceedWithDemo() {
  var size = numbers.length;
  // 0, ...., size-1
  var sequence = generateRandomNoDupplicate(size, 0, size - 1);

  const makeAdemo = async () => {
    for (const i of sequence) {
      var curImgInfo = imagesToGuessCoordinates[i];
      showImgInPos(curImgInfo.col, curImgInfo.row, curImgInfo.number);
      await delay(1000);
      showImgInPos(curImgInfo.col, curImgInfo.row, -1);
      await delay(500);
    }

    getPlayerGuesses(0);
  };
  makeAdemo();
}

function showImgInPos(x, y, imgNo) {
  let imgId = getImgIdByXY(x + 1, y + 1);
  const img = document.getElementById(imgId);
  if (img == null) {
    console.log("error, see x, y, imgNo", x, y, imgNo);
  }
  img.src = getImgSrc(imgNo);
}

function onLevelUpDown(event) {
  if (gameStarted) return;
  var h = btnUpDn.clientHeight;
  if (event.offsetY < h / 2) changeGameLevel(+1);
  else changeGameLevel(-1);
}

function changeGameLevel(change) {
  var imgCount = 0;
  if (change < 0) {
    imgCount = levelXY.imgCount - 1;
  } else if (change > 0) {
    imgCount = levelXY.imgCount + 1;
  } else return;

  let iLevel = levelXY.level;
  if (levelXY.imgCount > levelXY.max) {
    iLevel++;
  } else if (levelXY.imgCount < levelXY.min) {
    iLevel--;
  }
  levelXY = setGameLevel(iLevel, imgCount);
  defaultField();
}

function setGameLevel(iLevel, imgCount) {
  if (iLevel < 0) iLevel = 0;
  if (iLevel > 7) iLevel = 8;

  let lvl = Object.assign({}, Levels[iLevel]);
  if (imgCount <= 0) lvl.imgCount = lvl.min;
  else if (iLevel == 0 && imgCount <= lvl.min) {
    lvl.imgCount = lvl.min;
  } else if (iLevel == 8 && imgCount >= lvl.max) {
    lvl.imgCount = lvl.max;
  } else if (imgCount < lvl.min) {
    lvl = Object.assign({}, Levels[iLevel - 1]);
    lvl.imgCount = lvl.min;
  } else if (imgCount > lvl.max) {
    lvl = Object.assign({}, Levels[iLevel + 1]);
    lvl.imgCount = lvl.min;
  } else {
    lvl.imgCount = imgCount;
  }

  successCount = 0;
  failCount = 0;
  showGameLevel(lvl);
  return lvl;
}

function showGameLevel(lvl) {
  document.getElementById(
    "gameLevel"
  ).innerHTML = `${lvl.level}(${lvl.imgCount})`;
}

var sTimeStamp = 0;
var nOfS = 0;

function onKeyDown(event) {
  if (event.key == "Enter") {
    onStartGame();
    return;
  }
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
}
