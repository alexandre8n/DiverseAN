// statistics: history of answers and score calculation
// error: {opr: "+", n1: 2, n2: 19, given: 0, timeInMs: 0}
// history record {opr: "+", n1:0, n2:0, result:0, answer:0}
// todo remove: var historyRecords = [];
const maxScoreOfLevel0 = 10;
const historyRecordsMaxSize = 100;
// var currentHistoryRecord = {  // todo: remove after review...
//   id: 0,
//   opr: "+",
//   n1: 0,
//   n2: 0,
//   result: 0,
//   answer: 0,
//   timeInMs: 0,
//   score: 0,
// };

class History {
  historyRecords = [];
  constructor() {}
  getHistoryArrayColumns() {
    return ["id", "Operation", "your answer", "time in ms", "score"];
  }

  records() {
    return this.historyRecords;
  }

  clearAll() {
    this.historyRecords = [];
  }

  getHistoryArray(idStartingFrom) {
    var resArr = [];
    this.historyRecords.forEach((rec) => {
      if (rec.id < idStartingFrom) return;
      let val = calcCorrectRes(rec.opr, rec.n1, rec.n2);
      let valStr = `${rec.n1} ${rec.opr} ${rec.n2} = ${val}`;
      let curRec = [rec.id, valStr, rec.answer, rec.timeInMs, rec.score];
      resArr.push(curRec);
    });
    return resArr;
  }

  addRecord(opr, n1, n2, answer, timeInMs, score) {
    var lastRec = this.getLast();
    if (lastRec != null && lastRec.answer == null) {
      // empty record by preparing question
      lastRec.opr = opr;
      lastRec.n1 = n1;
      lastRec.n2 = n2;
      lastRec.answer = answer;
      lastRec.timeInMs = timeInMs;
      lastRec.score = score;
      return;
    }

    const rec = {
      id: this.getNextHistoryRecId(),
      opr: opr,
      n1: n1,
      n2: n2,
      answer: answer,
      timeInMs: timeInMs,
      score: score,
    };
    if (this.historyRecords.length >= historyRecordsMaxSize) {
      this.historyRecords.shift();
    }
    this.historyRecords.push(rec);
  }

  setHistoryRecords(histArr) {
    this.historyRecords = histArr.map((elm) => elm);
    //histArr.forEach((elm) => this.this.historyRecords.push(elm));
  }

  getNextHistoryRecId() {
    const l = this.historyRecords.length;
    if (l < 1) return 1;
    return this.historyRecords[l - 1].id + 1;
  }
  getLast() {
    const l = this.historyRecords.length;
    if (l < 1) return null;
    return this.historyRecords[l - 1];
  }

  getRecsLongerThen(time, idStartingFrom) {
    let arr = this.getHistoryArray(idStartingFrom);
    if (!arr || arr.length < 1) return null;
    let obj = {}; // to get only unique operations
    const arrFiltered = arr.filter((el) => {
      const operationStr = el[1];
      const t = el[3];
      const scorOr0 = el[4];
      // make them unique
      if (obj[operationStr] == 1) return false;
      obj[operationStr] = 1;
      if (scorOr0 == 0 || t >= time) return true;
      return false;
    });
    if (!arrFiltered || arrFiltered.length < 1) return null;

    const arrSorted = arrFiltered.sort((a, b) => {
      if (a[4] == 0) return 1;
      if (b[4] == 0) return -1;
      if (a[3] == b[3]) return 0;
      if (a[3] > b[3]) return 1;
      return -1;
    });
    return arrSorted;
  }
}
