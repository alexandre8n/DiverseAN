const lampCharCode = 9728; // sun character
const strLamp = String.fromCharCode(lampCharCode);

// history related stuf is in history.js
let stateOferror = false;
var oprTexts = {
  "+": "Addition(+)",
  "*": "Multiplication(*)",
  "-": "Subtraction(-)",
  "/": "Division(/)",
  "+short": "plus",
  "*short": "multiply by",
  "-short": "minus",
  "/short": "devide by",
};

const delay = (ms) => new Promise((res) => setTimeout(res, ms));

const questionEl = document.getElementById("question");
const formEl = document.getElementById("form");
const scoreEl = document.getElementById("score");
var btnUpDn = document.getElementById("btnUpDn");

btnUpDn.addEventListener("click", onLevelUpDown);
const inputEl = document.getElementById("input");
inputEl.addEventListener("keypress", function (event) {
  // If the user presses the "Enter" key on the keyboard
  if (event.key === "Enter") {
    // Cancel the default action, if needed
    event.preventDefault();
    onClick();
    // Trigger the button element with a click
    //document.getElementById("submitId").click();
  }
});

let dropdown = document.querySelector(".dropdown");
dropdown.onclick = function () {
  dropdown.classList.toggle("active");
};

const selfTester = new SelfTester();
const tutor = new Tutor();
const quizer = new Quizer();

// depending on the latest mode - assign server...
var numServer = loadAllStatesSelectServer();
numServer.startProgress(null);
prepareQuestion();

//----------------------------------------
// todo: if display = ''?
function hideShowElm(x, isToShow) {
  if (x.style.display === "none" && isToShow) {
    x.style.display = "block";
  } else if ((x.style.display = "block" && !isToShow)) {
    x.style.display = "none";
  }
}

function setQuizMode(isOn) {
  if (isOn) {
    showActiveMode(btnQuiz);
    setSelfTestMode(false);
    setTutorMode(false);
  }
}

function setSelfTestMode(isOn) {
  hideShowElm(dropDnId, isOn);
  if (!isOn) {
    return;
  }
  if (isOn) {
    setTutorMode(false);
    setQuizMode(false);
    showActiveMode(btnSelfTest);
    changeGameLevel(0);
    doChangeOpr(oprTexts[selfTester.getOpr()]);
  }
  inputEl.focus();
  inputEl.select();
}

function setTutorMode(isTutorOn) {
  if (isTutorOn) {
    showActiveMode(btnTutor);
    setSelfTestMode(false);
    setQuizMode(false);
    btnTutorModeSeqRnd.value = tutor.getSettingsModeStr(true);
    const strTooltip = tutor.getSettingsModeStr(false);
    btnTutorModeSeqRnd.setAttribute("title", strTooltip);
    btnTutorOpr.innerHTML = tutor.getOpr();
    btnTutorOpr.setAttribute("title", oprTexts[tutor.getOpr()]);
    divRangeId.innerHTML = tutor.getSettingRangeStr();
  } else {
    hideNodes("[data-tutor]");
  }
}

function hideNodes(strDataName) {
  const divsToHide = document.querySelectorAll(strDataName);
  divsToHide.forEach((nd) => (nd.style.display = "none"));
}

function showActiveMode(btn) {
  btnArr = [btnTutor, btnSelfTest, btnQuiz];
  btnArr.forEach((element) => {
    element.innerHTML = element.innerHTML.replace(strLamp, "");
  });
  btn.innerHTML = `&#${lampCharCode} ${btn.innerHTML}`;
}

function onClick() {
  const userAns = +inputEl.value;
  stateOferror = numServer.getErrorState();
  const isOk = numServer.setUserAnswer(userAns);

  numServer.saveState();
  if (isOk) {
    setErrorState(false);
  } else {
    setErrorState(true);
    processError();
    return;
  }
  prepareQuestion();
}

function setErrorState(newErrorState) {
  var btnHlp = document.getElementById("btnHelp");
  if (newErrorState) {
    btnHlp.style.backgroundColor = "red";
  } else {
    btnHlp.style.backgroundColor = "green";
  }
  stateOferror = newErrorState;
}

async function processError() {
  var element = document.getElementById("questionDiv");
  element.classList.toggle("shakeByError");
  showErrorMsg(true);
  await delay(1000);
  element.classList.toggle("shakeByError");
  await delay(3000);
  showErrorMsg(false);
}

function showErrorMsg(show) {
  var errorEl = document.getElementById("ErrorMsg");
  errorEl.style.display = show ? "block" : "none";
  var errDetailEl = document.getElementById("errorDetails");
  const curHrec = numServer.history().getLast();
  errDetailEl.innerHTML = `${curHrec.n1} ${numServer.getOpr()} 
    ${curHrec.n2} is not ${curHrec.answer}`;
}

function onLevelUpDown(event) {
  var h = btnUpDn.clientHeight;
  if (event.offsetY < h / 2) changeGameLevel(+1);
  else changeGameLevel(-1);
  prepareQuestion();
}

function changeGameLevel(change) {
  numServer.changeLevel(change);
  showGameLevel();
}

function showGameLevel() {
  var gl = document.getElementById("gameLevel");
  gl.innerHTML = numServer.getLevelStr();
}

// todo: decision about history record
// by preparing question - create and add, after the answer, fill the answer
function prepareQuestion() {
  inputEl.value = "";
  inputEl.focus();
  inputEl.select();
  const numbers2 = numServer.next2Numbers();

  if (numbers2 == null) {
    // finish of the serie of questions
    opnDlgFinished();
    return;
  }
  scoreEl.innerText = `score: ${numServer.score}`;
  var question = prepare4Operation(
    numServer.getOpr(),
    numServer.n1(),
    numServer.n2()
  );
  numServer.addHistoryRec(null, null, null);
  // currentHistoryRecord.result = correctAns;
  questionEl.innerText = question;
  numServer.time0 = stopwatchInit();
  numServer.saveState();
  progressId.innerHTML = numServer.progressStr();
  numServer.progressInc();
}

function updateLocalStorage() {
  localStorage.setItem("score", JSON.stringify(numServer.score));
}

function onChangeOpr(val) {
  doChangeOpr(val);
  prepareQuestion();
}

function doChangeOpr(val) {
  curOpr = getKeyByValue(oprTexts, val);
  if (!curOpr) curOpr = "+";
  selfTester.setOpr(curOpr);
  document.querySelector(".text-box").value = val;
}

function show(value) {
  curOpr = getKeyByValue(oprTexts, value);
  if (!curOpr) curOpr = "+";
  selfTester.setOpr(curOpr);
  document.querySelector(".text-box").value = value;
}
function prepare4Operation(opr, n1, n2) {
  const oprShort = oprTexts[opr + "short"];
  const questionStr = `${n1} ${oprShort} ${n2}`;
  return questionStr;
}

function onClickHelp() {
  const curHrec = numServer.history().getLast();
  var n1 = curHrec.n1;
  var n2 = curHrec.n2;
  var curOpr = numServer.getOpr();
  var msg = `${n1} ${curOpr} ${n2} = ${calcCorrectRes(curOpr, n1, n2)}<br/>`;

  msg += "why is it so?<br/>";
  msg += tutor.teachHow(n1, n2, curOpr);
  stateOferror = true; // it means for the answer after this no score
  var modal = document.getElementById("modal");
  openModal(modal, msg);
}

function onClickHistory() {
  let historyArr = numServer.history().getHistoryArray(0);
  showHistory4Array(historyArr);
}

// second param callBackAfterClose - means this func must be call
// when modal dlg is closed, null if nothing to call
function showHistory4Array(historyArr, callBackAfterClose) {
  var modal = document.getElementById("modal");
  let columns = numServer.history().getHistoryArrayColumns();
  openModalHistory(modal, columns, historyArr, callBackAfterClose);
}

function onTutor() {
  location.href = "tutor.html";
}
function onSelfTest() {
  numServer = selfTester;
  setSelfTestMode(true);
  prepareQuestion();
}
function onQuiz() {
  location.href = "Quiz.html";
}

// this function returns the number from "from" to "to";
function getRandomIntFromTo(from, to) {
  var size = to - from + 1;
  return from + getRandomNumber(size);
}

function getRandomNumber(size) {
  return Math.floor(Math.random() * size);
}

function getKeyByValue(object, value) {
  return Object.keys(object).find((key) => object[key] === value);
}

function loadAllStatesSelectServer() {
  tutor.retrieveState();
  selfTester.retrieveState();
  quizer.retrieveState();

  let slfT = selfTester.lastUpdDt().valueOf();
  let ttr = tutor.lastUpdDt().valueOf();
  let qzr = quizer.lastUpdDt().valueOf();

  const maxUpd = Math.max(slfT, ttr, qzr);
  if (ttr == maxUpd) {
    setTutorMode(true);
    return tutor;
  }
  if (qzr == maxUpd) {
    setQuizMode(true);
    return quizer;
  }
  setSelfTestMode(true);
  return selfTester;
}

function opnDlgFinished() {
  rng1Id.innerText = tutor.getRangeStr(1);
  rng2Id.innerText = tutor.getRangeStr(2);
  if (tutor.isInProgress()) {
    const progressVal = tutor.indexInProgress() / tutor.learningSetSize();
    titleId.innerHTML = `Your progress is ${(100 * progressVal).toFixed(
      1
    )}%, currently paused`;
    btnFollowResumeId.innerHTML = "Resume progress";
  } else {
    titleId.innerHTML = `Learing Step Is Finished!`;
  }

  dlgTextareaId.textContent = tutor.getRecommendedToRepeatStr();
  dlgFinishedTeachingId.numServer = "abc-tutor";
  dlgFinishedTeachingId.showModal();
}

function closeTeachingDlg() {
  dlgFinishedTeachingId.close();
}

function onDlgCancel() {
  closeTeachingDlg();
  //now : what next?
  // resume progress, if it was paused
  // otherwise 2 options: to tutor or self-learning on some reasonable level
}
function onDlgMore() {
  let ar = tutor.prepareTheLearningSetFromHistory();
  // now open history dlg based on this set...
  // todo:
  //now
  closeTeachingDlg();
  showHistory4Array(ar, opnDlgFinished);
}
function onDlgFollowResume() {
  dlgFinishedTeachingId.close();
  if (tutor.isInProgress()) {
    return;
  }
  const isModeOk = tutor.setMode("l"); // learning your the most difficult cases...
  if (!isModeOk) {
    onDlgRanges();
  }
  prepareQuestion();
  //now : possible cases:
  // user finished and follows rec.
  // user paused and got recommendation: he prob. should proceed with recommended
  //          and after this resume his main learning s/r?
  // user paused after following rec. and got new rec. w/o finishing prev.???
}
function onDlgYou() {
  onSelfTest();
}
function onDlgRanges() {
  onTutor();
}

function onDlgRandom() {
  const isModeOk = tutor.setMode("r"); // learning your the most difficult cases...
  dlgFinishedTeachingId.close();
  if (!isModeOk) {
    onDlgRanges();
  }
  prepareQuestion();
}
