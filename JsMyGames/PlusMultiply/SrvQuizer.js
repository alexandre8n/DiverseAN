const quizerKey = "PlusMulti.Quizer.User1";

class Quizer extends SrvBase {
  settings = {
    opr: "+", // +-*/
    n1: 0,
    n2: 0,
    level: 0,
    lastUpdated: null, // if lastUpdate = 0 nothing was retreived
    version: 2,
    history: null,
  };

  constructor() {
    super();
    super.init(this.settings);
  }
  next2Numbers() {
    return null;
    // { n1: n1, n2: n2, opr: opr };
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
  getLevel() {
    return this.settings.level;
  }
  getLevelStr() {
    var digits = levelInfo[this.getLevel()];
    return `${lvl}(${digits.a}, ${digits.b})`;
  }

  changeLevel(change) {
    this.settings.level += change;
    if (this.settings.level < 0) this.settings.level = 0;
    if (this.settings.level > 2) this.settings.level = 2;
  }

  lastUpdDt() {
    return super.str2Date(this.settings.lastUpdated);
  }

  saveHistory() {}
  saveState() {
    super.saveStateBase(this.settings, quizerKey);
  }

  retrieveState() {
    this.settings = super.retrieveStateBase(this.settings, quizerKey);
    return;
  }
}
