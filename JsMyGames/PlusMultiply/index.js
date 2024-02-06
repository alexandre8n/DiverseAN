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
    onSubmit();
    // Trigger the button element with a click
    //document.getElementById("submitId").click();
  }
});

// // for special dropdown
// let dropdown = document.querySelector(".dropdown");
// dropdown.onclick = function () {
//   dropdown.classList.toggle("active");
// };

const selfTester = new SelfTester();
const tutor = new Tutor();
const quizer = new Quizer();

// depending on the latest mode - assign server...
var numServer = loadAllStatesSelectServer();
numServer.startProgress(null);
showGameLevel();
prepareQuestion();

//----------------------------------------
function onOprSelect() {
  const curOpr = oprSelect.value;
  selfTester.setOpr(curOpr);
  prepareQuestion();
}

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
    quizer.restartSet();
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
    numServer = selfTester;
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
    btnTutorModeSeqRnd.innerHTML = tutor.getSettingsModeStr(true);
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

function onSubmit() {
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
  if (
    numServer.constructor.name == "Tutor" ||
    numServer.constructor.name == "Quizer"
  ) {
    return;
  }
  var h = btnUpDn.clientHeight;
  const lvlStrOld = numServer.getLevelStr();
  if (event.offsetY < h / 2) changeGameLevel(+1);
  else changeGameLevel(-1);
  if (numServer.getLevelStr() == lvlStrOld) return;
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

function onOprSelectSelfTest() {
  const idx = oprSelect.selectedIndex;
  const val = parseInt(oprSelect.value);
  const keys = Object.keys(oprTexts);
  const curOpr = keys[val];
  selfTester.setOpr(curOpr);
  prepareQuestion();
}

function onChangeOpr(val) {
  doChangeOpr(val);
  prepareQuestion();
}

// ?? to check if needed
function doChangeOpr(val) {
  curOpr = getKeyByValue(oprTexts, val);
  if (!curOpr) curOpr = "+";
  selfTester.setOpr(curOpr);
  oprSelect.value = curOpr;
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
  openModalHistory(modal, columns, historyArr, callBackAfterClose, numServer);
}

function onTutor() {
  location.href = "tutor.html";
}
function onSelfTest() {
  numServer = selfTester;
  numServer.startProgress(null);
  setSelfTestMode(true);
  prepareQuestion();
}
function onQuiz() {
  // save state?
  location.href = "Quizer.html";
}
function onTutorOpr(opr) {
  if (opr == "menu") {
    tutorMenuOpr.style.visibility = "visible";
    return;
  }
  if (oprTexts[opr]) {
    tutor.setOpr(opr);
    tutor.restartSet();
    setTutorMode(true);
    tutorMenuOpr.style.visibility = "hidden";
    prepareQuestion();
  }
}

function onTutorSeqRnd(mode) {
  if (mode == "menu") {
    tutorMenuSeqRnd.style.visibility = "visible";
  }
  if (mode == "s" || mode == "r") {
    tutor.setMode(mode);
    tutor.restartSet();
    setTutorMode(true);
    //tutorMenuSeqRnd.style.display = "none";
    tutorMenuSeqRnd.style.visibility = "hidden";
    prepareQuestion();
  }
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
  // todo check history here?

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

function opnDlgQuizFinished() {
  numServer.saveState();
  location.href = "Quizer.html?Finish=1";
  //http://yourdomain.com/login?cid=username&pwd=password
  // window.open(
  //   "http://localhost:8080/login?cid=" + myu + "&pwd=" + myp,
  //   "MyTargetWindowName"
  // );
}

function opnDlgFinished() {
  if (numServer.constructor.name == "Quizer") {
    opnDlgQuizFinished();
  }
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
    btnFollowResumeId.innerHTML = "Follow recommendation";
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
  // if the learning set has been finished, then switch to self test
  onSelfTest();
}
function onDlgMore() {
  let ar = tutor.prepareTheLearningSetFromHistory();
  // now open history dlg based on this set...
  closeTeachingDlg();
  showHistory4Array(ar, opnDlgFinished);
}
function onDlgFollowResume() {
  dlgFinishedTeachingId.close();
  if (tutor.isInProgress()) {
    // we have to resume the pause
    return;
  }
  const isModeOk = tutor.setMode("l"); // learning your the most difficult cases...
  if (!isModeOk) {
    onDlgRanges();
    return;
  }
  prepareQuestion();
}

function onDlgYou() {
  closeTeachingDlg();
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
