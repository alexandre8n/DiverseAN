const Key = "";
const minDate = new Date(-8640000000000000);
const levelInfo = [
  { lvl: 0, a: 1, b: 1, scoreKoef: 1 },
  { lvl: 1, a: 1, b: 2, scoreKoef: 2 },
  { lvl: 2, a: 2, b: 2, scoreKoef: 3 },
];

class SrvBase {
  score = 0;
  time0 = 0;
  stateOferror = false;
  settings = null;
  _history = null;
  progressStep = 0;
  progressAllSteps = 0;

  constructor() {
    this._history = new History();
  }
  init(settings) {
    settings.history = this._history;
    this.settings = settings;
  }
  next2Numbers() {
    return null; //{ n1: 0, n2: 0, opr: null };
  }

  getOpr() {
    return this.settings.opr;
  }
  setOpr(opr) {
    this.settings.opr = opr;
  }

  n1() {
    return this.settings.n1;
  }
  n2() {
    return this.settings.n2;
  }

  lastUpdDt() {
    return str2Date(new Date());
  }

  changeLevel(change) {
    return; // used only in childs...
  }

  str2Date(strDate) {
    const res = str2Date(strDate);
    if (res != null) return res;
    return minDate;
  }

  history() {
    return this._history;
  }

  // todo: stateOfError, calcOfRes, calcScore
  setErrorState(errState) {
    this.stateOferror = errState;
  }
  getErrorState() {
    return this.stateOferror;
  }

  setUserAnswer(userAns) {
    const res = calcCorrectRes(this.getOpr(), this.n1(), this.n2());
    const isOk = res == userAns;
    if (!isOk) this.setErrorState(true);
    const timeInMs = getElapsedTime(numServer.time0);
    const lvl = this.getLevel();
    const score = !this.getErrorState() ? this.calcScore(lvl, timeInMs) : 0;
    this.score += score;
    this.history().addRecord(
      this.getOpr(),
      this.n1(),
      this.n2(),
      userAns,
      timeInMs,
      score
    );
    if (isOk) this.setErrorState(false);
    return isOk;
  }

  addHistoryRec(ans, timeInMs, score) {
    this.history().addRecord(
      this.getOpr(),
      this.n1(),
      this.n2(),
      ans,
      timeInMs,
      score
    );
  }

  saveStateBase(settings, key1) {
    settings.lastUpdated = new Date();
    const myJSON = JSON.stringify(settings);
    localStorage.setItem(key1, myJSON);
  }

  retrieveStateBase(settings, key1) {
    const text = localStorage.getItem(key1);
    if (text != null) {
      const s1 = JSON.parse(text);
      if (settings.version != s1.version) {
        this.saveStateBase(settings, key1);
      }
      s1.lastUpdated = str2Date(s1.lastUpdated);
      this.settings = s1;
      return s1;
    }
    this.settings = settings;
    return settings;
  }

  getLocalStorageKeys() {
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      const value = localStorage.getItem(key);
      console.log({ key, value });
    }
    return Object.keys(localStorage);
  }

  clearLocalStorage(key1) {
    if (key1 == null) localStorage.clear();
    localStorage.removeItem(key1);
  }

  calcScore(level, timeInMs) {
    // 6s -10%, 10s -20%, 15s -25%, 30s -50%, 60s... -70%
    const koefArr = [
      { from: 0, to: 3, k: 1 },
      { from: 3, to: 6, k: 0.9 },
      { from: 6, to: 10, k: 0.8 },
      { from: 10, to: 15, k: 0.75 },
      { from: 15, to: 30, k: 0.5 },
      { from: 30, to: 60, k: 0.3 },
    ];
    const tInSec = timeInMs / 1000;
    var intervalObj = koefArr.find(
      (obj) => tInSec >= obj.from && tInSec < obj.to
    );
    const kRes = intervalObj ? intervalObj.k : koefArr[koefArr.length - 1].k;
    const score = maxScoreOfLevel0 * getLevelKoef(level) * kRes;
    return score;
  }

  startProgress(progressMax) {
    this.progressStep = 1;
    if (progressMax) this.progressAllSteps = progressMax;
  }
  progressInc() {
    this.progressStep++;
  }
  progressStr() {
    if (this.progressAllSteps <= 0) return `step: ${this.progressStep}`;
    return `progress: ${this.progressStep} of ${this.progressAllSteps}`;
  }
}

//////////////////////////////
// static functions
function getLevelKoef(level) {
  return levelInfo[level].scoreKoef;
}

function calcCorrectRes(opr, n1, n2) {
  switch (opr) {
    case "+":
      return n1 + n2;
    case "*":
      return n1 * n2;
    case "-":
      return n1 - n2;
    case "/":
      return Math.floor(n1 / n2);
  }
  return null;
}

function getLevel(opr, i1, i2) {
  var n1 = Math.min(i1, i2);
  var n2 = Math.max(i1, i2);
  if (n1 < 10 && n2 < 10) return 0;
  if (n1 < 10) return 1;
  return 2;
}
