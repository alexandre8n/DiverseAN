const tutorKey = "PlusMulti.Tutor.User1";
const totorMode = { s: "Sequential", r: "Random", l: "Learning" };
const longAnswerInMs = 5000;
class Tutor extends SrvBase {
  settings = {
    opr: "+", // +-*/
    n1: 0,
    n2: 0,
    range1: [0, 0],
    range2: [0, 0],
    mode: "s", // s: sequential, r: random, l: learning
    lastUpdated: null, // if lastUpdate = 0 nothing was retreived
    history: null,
    version: 4,
  };
  // randomSet = []; // array of {n1:n1, n2:n2, opr: opr}
  lerningSet = []; // array of the longest of falsely answered questions
  indexInlerningSet = 0; // index of the next question
  historyIdOfNextSession = 0; // for every learning session we should now the lrn results

  constructor() {
    super();
    super.init(this.settings);
  }

  getSettingStr() {
    const settingJson = JSON.stringify(this.settings);
    return settingJson;
  }

  getSettingsModeStr(isShort) {
    if (isShort) return this.settings.mode;
    return totorMode[this.settings.mode];
  }
  getRangeStr(r1or2) {
    let rFrom = this.settings.range1[0];
    if (r1or2 == 2) {
      rFrom = this.settings.range2[0];
    }
    var rTo = rFrom - (rFrom % 10) + 9;
    return `${rFrom} .. ${rTo}`;
  }

  getSettingRangeStr() {
    return `${this.getRangeStr(1)} - ${this.getRangeStr(2)}`;
  }

  getRecommendedToRepeatStr() {
    let h = this.history();
    const ar = h.getRecsLongerThen(longAnswerInMs, this.historyIdOfNextSession);
    const isIn = this.isInProgress();
    if (ar == null && !isIn) {
      return "It seems you have learned this range! Select new one, or random set";
    } else if (ar == null) {
      return "It seems you have well answered proposed questions! Resume your progress to finish this learning set";
    }
    let arrRecomm = ar.map((arEl) => arEl[1]);
    let recommendedStr = arrRecomm.join(", ");
    return recommendedStr + "\n" + `(${arrRecomm.length} case(s))`;
  }

  lastUpdDt() {
    return super.str2Date(this.settings.lastUpdated);
  }

  // todo: obsolete - delete it later...
  initiateTutoring(settingStr) {
    let stateObj = JSON.parse(settingStr);
    this.settings = stateObj;
  }

  setState(st) {
    this.settings = st;
  }
  getState() {
    return this.settings;
  }

  saveState() {
    super.saveStateBase(this.settings, tutorKey);
  }

  retrieveState() {
    this.settings = super.retrieveStateBase(this.settings, tutorKey);
    return;
  }

  setLastNumbers(n1, n2) {
    this.settings.n1 = n1;
    this.settings.n2 = n2;
  }

  learningSetSize() {
    const mode = this.settings.mode;

    if (mode == "s") {
      const r1Len = this.settings.range1[1] - this.settings.range1[0] + 1;
      const r2Len = this.settings.range2[1] - this.settings.range2[0] + 1;
      return r1Len * r2Len;
    }
    if (this.lerningSet) return this.lerningSet.length;
    return 0;
  }

  indexInProgress() {
    const mode = this.settings.mode;
    if (mode == "s") {
      const r1go = this.n1() - this.settings.range1[0];
      const r2Len = this.settings.range2[1] - this.settings.range2[0] + 1;
      const r2go = this.n2() - this.settings.range2[0];
      return r1go * r2Len + r2go + 1;
    }
    return this.indexInlerningSet + 1;
  }

  isInProgress() {
    const idx = this.indexInProgress();
    const size = this.learningSetSize();
    return idx < size;
  }

  isActive() {
    return this.settings.lastUpdated != null;
  }

  setMode(mode) {
    if (!totorMode[mode]) {
      return false;
    }
    this.settings.mode = mode;

    if (mode == "s") {
      return true;
    }
    // learning mode or random mode: -----------
    // array of the longest of falsely answered questions
    this.lerningSet = this.prepareTheLearningSet();
    if (!this.lerningSet) return false;
    this.startProgress(this.lerningSet.length);
    this.indexInlerningSet = 0; // index of the next question
    if (this.lerningSet == null) return false;
    // fix history id, your learning history start point.
    this.historyIdOfNextSession = this.history().getNextHistoryRecId();
    return true;
  }

  initiateTutoring2(settingStr) {
    // format of msg: opr, from1, to1, from2, to2, mode, step, totalSteps
    var arrStr = settingStr.split(",");
    this.settings.opr = arrStr[0];
    this.settings.range1[0] = parseInt(arrStr[1]);
    this.settings.range1[1] = parseInt(arrStr[2]);
    this.settings.range2[0] = parseInt(arrStr[3]);
    this.settings.range2[1] = parseInt(arrStr[4]);
    this.settings.mode = arrStr[5];
    this.settings.currentStep = parseInt(arrStr[6]);
    this.settings.totalSteps = parseInt(arrStr[7]);
  }

  next2NumbersSeqMode() {
    let n1 = this.settings.range1[0];
    let n2 = this.settings.range2[0];
    if (this.settings.n1 == 0) {
      // first time
      return this.next2NumbersOut(n1, n2);
    }
    n1 = this.settings.n1;
    n2 = this.settings.n2 + 1; //now maybe n2?

    if (n2 <= this.settings.range2[1]) {
      return this.next2NumbersOut(n1, n2, null);
    }
    n1++;
    n2 = this.settings.range2[0];
    if (n1 <= this.settings.range1[1]) {
      return this.next2NumbersOut(n1, n2, null);
    }
    return null; // finished
  }

  next2NumbersLernMode() {
    if (this.lerningSet == null || this.lerningSet.length == 0)
      this.lerningSet = this.prepareTheLearningSet();
    if (
      this.lerningSet == null ||
      this.indexInlerningSet >= this.lerningSet.length
    ) {
      return null; //{n1: 0, n2: 0, opr: null};
    }
    const histRec = this.lerningSet[this.indexInlerningSet];
    this.indexInlerningSet++;
    var oprRes = parseOperationStr(histRec[1]);
    return this.next2NumbersOut(oprRes.n1, oprRes.n2, oprRes.opr);
  }

  next2Numbers() {
    // return {n1: n1, n2: n2, opr: opr};
    if (this.settings.mode == "s") {
      return this.next2NumbersSeqMode();
    }
    if (this.settings.mode == "l" || this.settings.mode == "r") {
      return this.next2NumbersLernMode();
    }
    return null;
  }

  prepareTheRandomSet() {
    // how many:
    const n1size = 1 + this.settings.range1[1] - this.settings.range1[0];
    const n2size = 1 + this.settings.range2[1] - this.settings.range2[0];
    const setLen = n1size * n2size;
    const idxArrrandomSet = generateRandomNoDupplicate(setLen, 0, setLen - 1);
    const randomSet = this.buildRandomSet(idxArrrandomSet);
    const resSet = randomSet.map((el, idx) => {
      const s = `${el.n1}${el.opr}${el.n2}`;
      return [idx, s];
    });
    return resSet;
  }

  buildRandomSet(idxArr) {
    const nCol = 1 + this.settings.range2[1] - this.settings.range2[0];
    //          range2, range2+1....
    // range1, .....
    // range1+1,
    const resArr = idxArr.map((seqNo, idx) => {
      const rowCol = rowColBySeqNo(nCol, seqNo);
      return {
        n1: this.settings.range1[0] + rowCol.row,
        n2: this.settings.range2[0] + rowCol.col,
        opr: this.getOpr(),
      };
    });
    return resArr;
  }
  prepareTheLearningSet() {
    if (this.settings.mode == "r") {
      // random!
      return this.prepareTheRandomSet();
    }
    return this.prepareTheLearningSetFromHistory();
  }

  // prepare the learning set considering last answers and time spent...
  prepareTheLearningSetFromHistory() {
    let h = this.history();
    return h.getRecsLongerThen(longAnswerInMs, this.historyIdOfNextSession);
  }

  getLevel() {
    return getLevel(this.getOpr(), this.n1(), this.n2());
  }

  // if opr is null take opr from Srv...
  next2NumbersOut(n1, n2, opr) {
    this.settings.n1 = n1;
    this.settings.n2 = n2;
    if (opr) this.setOpr(opr);
    return { n1: n1, n2: n2, opr: this.getOpr() };
  }

  teachHow(n1, n2, opr) {
    let msg = "";
    if (opr == "+") msg = this.teachPlus(n1, n2);
    else if (opr == "*") msg = this.teachMult(n1, n2);
    else if (opr == "-") msg = this.teachMinus(n1, n2);
    else if (opr == "/") msg = this.teachDevide(n1, n2);
    return msg;
  }

  teachPlus(n_1, n_2) {
    var n2 = Math.min(n_1, n_2);
    var n1 = Math.max(n_1, n_2);
    var msg1opr2 = `${n1} + ${n2} = `;
    var [d11, d12] = digits(n1);
    var [d21, d22] = digits(n2);
    // both 1 digit?
    if (typeof d12 === "undefined" && typeof d22 === "undefined") {
      return this.teach1dPlus1d(d11, d21);
    }
    // 2-dig plus one-dig
    if (typeof d22 === "undefined") {
      return this.teach2dPlus1d(d11, d12, d21);
    }
    return this.teach2dPlus2d(d11, d12, d21, d22);
  }

  teach1dPlus1d(n1, n2) {
    if (n1 + n2 <= 10) {
      const msg = `You should just remember that<br/>${n1} + ${n2} = ${
        n1 + n2
      }`;
      return msg;
    }
    let msg1opr2 = `${n1} + ${n2} = `;
    let fromLastDigN1to10 = 10 - n1;
    msg1opr2 += `${n1} + ${fromLastDigN1to10} + ${n2 - fromLastDigN1to10} = ${
      n1 + n2
    }`;
    return msg1opr2;
  }

  teach2dPlus1d(d11, d12, d21) {
    // like: 34+ 5, or 34+8
    const n1 = d11 * 10 + d12;
    let msg1opr2 = `${n1} + ${d21} = `;
    if (d12 + d21 < 10) {
      msg1opr2 += `${d11 * 10} + ${d12} + ${d21} = ${n1 + d21}`;
    } else {
      msg1opr2 += `${d11 * 10} + (${d12} + ${d21})(=${d12 + d21}) = `;
      msg1opr2 += `${d11 * 10 + 10} + ${d12 + d21 - 10} = ${n1 + d21}`;
    }
    return msg1opr2;
  }
  teach2dPlus2d(d11, d12, d21, d22) {
    const n1 = d11 * 10 + d12;
    const n2 = d21 * 10 + d22;
    let msg1opr2 = `${n1} + ${n2} = `;
    // a) like 72+27 -> (70+20=90) + (2+7=9)=99
    if (d12 + d22 < 10) {
      var d10 = d11 * 10 + d21 * 10;
      var d0 = d12 + d22;
      msg1opr2 += `(${d11 * 10}+${d21 * 10} = ${d10}) `;
      msg1opr2 += ` + (${d12}+${d22} = ${d0})<br/>`;
      msg1opr2 += `Result: ${n1 + n2}`;
      return msg1opr2;
    }
    // b) like 77+27 -> (77+3=80) + (27-3=24)=80+24=104
    const additionTo10 = 10 - d12;
    msg1opr2 += `(${n1}+${additionTo10} =${n1 + additionTo10}) `;
    msg1opr2 += `+ (${n2}-${additionTo10} =${n2 - additionTo10}) `;
    msg1opr2 += `= ${n1 + additionTo10} + ${n2 - additionTo10}<br/>`;
    msg1opr2 += `Result: ${n1 + n2}`;
    return msg1opr2;
  }

  teachMult(n_1, n_2) {
    let msg = "";
    if (n_1 == 11 || n_2 == 11) {
      msg = this.teachMult11(n_1, n_2);
      return msg;
    }
    var n1 = n_1,
      n2 = n_2;
    let msg1opr2 = `${n1} * ${n2} = `;
    let res = `${n1 * n2}`;
    if (n1 < n2) {
      n1 = n_2;
      n2 = n_1;
      msg1opr2 += `${n1} * ${n2} = `;
    }
    if (n1 % 11 == 0 || n2 % 11 == 0) {
      msg = this.teachMultAbNN(n1, n2);
      return msg;
    }
    var digitsArr1 = digits(n1);
    var digitsArr2 = digits(n2);
    const d1a = digitsArr1[0];
    const d1b = digitsArr2[0];
    const d2a = digitsArr1[1];
    const d2b = digitsArr2[1];
    if (digitsArr1.length == 1) {
      msg =
        "You have first to learn the multiplication table<br/>" +
        `<a href="https://www.mathsisfun.com/tables.html" target="_blank" rel="noopener noreferrer">
    Multiplication table!</a><br/>`;
      msg += msg1opr2 + res + "<br/>";
      return msg;
    }

    // case like: ab*a{10-b}, for example: 43*47=4*{4+1}{7*3}={4*5=20}{3*7=21}=2021
    // or 65*65={6*7}25=4225
    if (
      digitsArr1.length == 2 &&
      digitsArr2.length == 2 &&
      digitsArr1[0] == digitsArr2[0] &&
      digitsArr1[1] + digitsArr2[1] == 10
    ) {
      msg =
        "case: ab*a{10-b}={a*(a+1)}{b(10-b)},<br/>" +
        "for example: 12*18=216, or 37*33=1221, or 65*65=4225<br/>" +
        "43*47=4*{4+1}{7*3}={4*5=20}{3*7=21}=2021<br>";
      return msg;
    }

    // case like: 65 * 5 or 75*5
    if (
      digitsArr1.length == 2 &&
      digitsArr1[1] == 5 &&
      digitsArr2.length == 1 &&
      digitsArr2[0] == 5
    ) {
      let msg =
        "if you multiply the 2 digit number {a}5 * 5,<br/>just know that:<br/>";
      msg +=
        "if first digit is even you can write the result as (digit1/2)25<br/>";
      msg += "example: 45*5 = {4/2}25 = 225<br/>";
      msg +=
        "if first digit is odd you can write the result as {(digit1-1)/2}75<br/>";
      msg += "example: 75*5 = {(6/2)}75 = 375<br/>";
      return msg;
    }
    if (digitsArr2.length == 1) {
      let resD1n2 = digitsArr1[0] * 10 * n2;
      msg =
        msg1opr2 +
        `${digitsArr1[0] * 10} * ${n2} + ${digitsArr1[1]} * ${n2} = <br/>`;
      msg += `${resD1n2} + ${digitsArr1[1] * n2} = ${res}`;
      return msg;
    } else if (digitsArr2.length == 2) {
      // 27*49 = 20*40 + 7*40 * 20*9 + 7*9 = 800+280+180+63 = 1323
      // 27*46 = 20*40(=800) + 7*40(=280) * 20*6(=120) + 7*6(42) = 1242
      msg = `${d1a}0*${d1b}0(=${d1a * d1b * 100}) + `;
      msg += `${d1a}0*${d2b}(=${d1a * d2b * 10}) + `;
      msg += `${d1b}0*${d2a}(=${d1b * d2a * 10}) + `;
      msg += `${d2a}*${d2b}(${d2a * d2b})<br/>`;
      msg += `Result: ${n1 * n2}`;

      return msg;
    }
  }

  teachMult11(n1, n2) {
    if (n1 == 11) {
      n1 = n2;
      n2 = 11;
    }
    let msg = "";

    if (n1 < 10) {
      // one digit
      msg = `Multiplication on 11 for one digit number -> just duplicate the digit:<br/>`;
      msg += "example: 2*11 = 22,..., 9*11 = 99";
      return msg;
    }
    var digitsArr1 = digits(n1);
    msg = "If you multiply the 2 digit number (ab) by 11 <br/>";
    msg += "just know that the result is: a{a+b}b,<br/>";
    msg +=
      "examples: 11*11 = 1{1+1}1 = 121, 12*11= 1{1+2}2 = 132, ..., 17*11 = 187, 19*11=1{10}9=209<br/>";
    msg += `it means ${n1} * 11 = ${n1 * 11}`;
    return msg;
  }

  teachMultAbNN(n1, n2) {
    // n1 or n2 looks like NN
    // 34*77 = 34*7*10 + 34*7 = 2380 + 238 = 2618
    let msg = "";
    if (n1 % 11 == 0) msg = this.teachMultAbNNHelper(n1, n2);
    else msg = this.teachMultAbNNHelper(n2, n1);
    return msg;
  }

  teachMultAbNNHelper(n1, n2) {
    // n1%11==0
    const d = n1 / 11;
    const msg = `${n2}*${d}*10 + ${n2}*${d} = ${n2 * d * 10} + ${n2 * d} = ${
      n1 * n2
    }`;
    return msg;
  }

  /// area of tests
  testRnd() {
    this.settings.range1 = [2, 3];
    this.settings.range2 = [4, 7];
    let rndSet = this.prepareTheRandomSet();
  }
  testOutNumbers() {
    let res = this.next2Numbers();
    console.log(
      `ranges: ${this.settings.range1[0]} .. ${this.settings.range2[1]}\n`
    );
    while (res && res.n1) {
      console.log(`${res.n1}, ${res.n2} `);
      res = this.next2Numbers();
    }
  }
}
