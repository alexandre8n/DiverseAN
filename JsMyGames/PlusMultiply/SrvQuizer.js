const quizerKey = "PlusMulti.Quizer.User1";
const quizerLevels = [
  { range1: [2, 9], range2: [11, 39], questsCount: 15 },
  { range1: [2, 9], range2: [41, 99], questsCount: 18 },
  { range1: [11, 19], range2: [11, 59], questsCount: 20 },
  { range1: [11, 19], range2: [61, 99], questsCount: 20 },
  { range1: [21, 39], range2: [21, 59], questsCount: 20 },
  { range1: [21, 39], range2: [61, 99], questsCount: 20 },
  { range1: [41, 59], range2: [41, 99], questsCount: 20 },
  { range1: [61, 79], range2: [61, 99], questsCount: 20 },
  { range1: [81, 99], range2: [81, 99], questsCount: 20 },
];

class Quizer extends SrvBase {
  settings = {
    opr: "+", // +-*/
    n1: 0,
    n2: 0,
    level: 0,
    step: 0,
    lastUpdated: null, // if lastUpdate = 0 nothing was retreived
    version: 2,
    history: null,
  };

  quizerSet = [];

  constructor() {
    super();
    this.init();
  }

  stgs() {
    return this.settings;
  }

  getKey() {
    return quizerKey;
  }

  allLevels() {
    return quizerLevels;
  }
  restartSet() {
    super.restartSet();
    this.prepareTheQuizerSet();
    this.startProgress(this.quizerSet.length);
  }

  learningSetSize() {
    return this.quizerSet.length;
  }
  next2Numbers() {
    const idx = this.progressStep - 1;
    if (idx >= this.quizerSet.length) return null;
    const elms = this.quizerSet[idx];
    this.settings.n1 = elms[0];
    this.settings.n2 = elms[1];
    return { n1: this.n1(), n2: this.n2(), opr: this.getOpr() };
  }

  prepareTheQuizerSet() {
    // sets quizerSet with [ [n1, n2], [n1, n2]...] for the current level
    const lvl = this.settings.level;
    const lvlInfo = quizerLevels[lvl];
    const arr1 = Sequence(lvlInfo.range1[0], lvlInfo.range1[1]).filter(
      (v) => v % 10 != 0
    );
    const arr2 = Sequence(lvlInfo.range2[0], lvlInfo.range2[1]).filter(
      (v) => v % 10 != 0
    );
    const countOfNums = arr1.length * arr2.length;

    // from this matrix of possible pairs we select questsCount random pairs.
    const arOfIdxs = generateRandomNoDupplicate(
      lvlInfo.questsCount,
      0,
      countOfNums
    );
    this.quizerSet = arOfIdxs.map((idx) => {
      let ij = { row: 0, col: 0 };
      ij = rowColBySeqNo(arr1.length, idx);
      let elmX = arr1[ij.col];
      let elmY = arr2[ij.row];
      if (elmX == undefined || elmY == undefined) {
        console.log(`very strange: ${ij.col}, ${ij.row} -> ${elmX}, ${elmY}`);
      }
      return [elmX, elmY];
    });
  }
  getOpr() {
    return this.settings.opr;
  }
  n1() {
    return this.settings.n1;
  }
  n2() {
    return this.settings.n2;
  }
  changeLevel(change) {
    const lvl = this.settings.level + change;
    if (lvl < 0) this.settings.level = 0;
    if (lvl > quizerLevels.length) this.settings.level = quizerLevels.length;
  }

  lastUpdDt() {
    return super.str2Date(this.settings.lastUpdated);
  }

  saveState() {
    this.settings.step = this.progressStep; // 1, 2, ...
    super.saveStateBase(this.settings, quizerKey);
  }

  retrieveState() {
    this.settings = super.retrieveStateBase(this.settings, quizerKey);
    this.progressStep = this.settings.step - 1;
    return;
  }

  calcScore(lvl, timeInMs) {
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
    const expectedTime = this.getExpectedTime();
    if (tInSec <= expectedTime) {
      return 100;
    }
    let score = Math.round((100 * expectedTime) / tInSec);
    return score;
  }

  getExpectedTime() {
    const timeToRead = 1;
    const timeToWrite = 1.5;
    const timeDelay2Dig = 0.5;
    const timePlus1 = 0.7;
    const timePlus2 = 1.2;
    const timeMult1 = 1.3;

    let tRW = timeToRead + timeToWrite;
    let t = 0;
    const n1 = Math.max(this.n1(), this.n2());
    const n2 = Math.min(this.n1(), this.n2());
    var digitsArr1 = digits(n1);
    var digitsArr2 = digits(n2);
    const d1a = digitsArr1[0];
    const d1b = digitsArr2[0];
    const d2a = digitsArr1[1];
    const d2b = digitsArr2[1];
    if (this.getOpr() == "+") {
      if (digitsArr1.length == 1) t = d1a + d1b < 10 ? timePlus1 : timePlus2;
      else if (digitsArr1.length == 2) {
        if (digitsArr2.length == 1) {
          t = timeDelay2Dig + (d2a + d1b < 10 ? timePlus1 : timePlus2);
        }
        if (digitsArr2.length == 2) {
          t = 2 * timeDelay2Dig + timePlus1 + timePlus2;
        }
      }
    } else if (this.getOpr() == "*") {
      if (digitsArr1.length == 1) t = 1;
      else if (digitsArr1.length == 2) {
        if (digitsArr2.length == 1) {
          t = timeDelay2Dig + 2 * timeMult1 + timePlus2;
          if (d1a * d2a * 10 + d1b * d2a > Math.ceil((d1a * d2a) / 10) * 100)
            t += timePlus1;
        }
        if (digitsArr2.length == 2) {
          t = 2 * timeDelay2Dig + 4 * timeMult1 + 3 * timePlus2;
        }
      }
    } else if (this.getOpr() == "-") {
      t = 5;
    } else if (this.getOpr() == "/") {
      t = 10;
    }
    return tRW + t;
  }

  answeredStatistics() {
    let h = this.history();
    let historyRecs = h == null ? [] : h.records();
    let recs = [];
    let idxInHistory1 = historyRecs.length - this.progressStep;

    for (let i = idxInHistory1; i < historyRecs.length; i++) {
      recs.push(historyRecs[i]);
    }
    return recs;
  }
}
