const selfTesterKey = "PlusMulti.SelfTester.User1";

class SelfTester extends SrvBase {
  settings = {
    opr: "+", // +-*/
    n1: 0,
    n2: 0,
    level: 0,
    lastUpdated: null, // if lastUpdate = 0 nothing was retreived
    version: 3,
    history: null,
  };

  constructor() {
    super();
    super.init(this.settings);
  }

  next2Numbers() {
    // { n1: n1, n2: n2, opr: opr };
    var digitsInfo = levelInfo[this.getLevel()];
    var n1info = digitsInfo.a == 1 ? [2, 9] : [11, 99];
    var n2info = digitsInfo.b == 1 ? [2, 9] : [11, 99];
    this.settings.n1 = getRandomIntFromTo(n1info[0], n1info[1]);
    this.settings.n2 = getRandomIntFromTo(n2info[0], n2info[1]);
    return {
      n1: this.settings.n1,
      n2: this.settings.n2,
      opr: this.settings.opr,
    };
  }
  getLevel() {
    return this.settings.level;
  }
  getLevelStr() {
    const lvl = this.getLevel();
    var digits = levelInfo[lvl];
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
    super.saveStateBase(this.settings, selfTesterKey);
  }

  retrieveState() {
    this.settings = super.retrieveStateBase(this.settings, selfTesterKey);
    return;
  }
}
