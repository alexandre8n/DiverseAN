class Parser {
  #iPos = 0; // current position
  #iCurWordPosStart = -1; // current word start position
  #iCurWordPosEnd = -1; // current word end position, idx of the next char
  #strToParce;
  #delimiters;
  constructor() {}
  init(str, delims = ",; ") {
    this.#strToParce = str;
    this.#delimiters = delims;
    this.#iPos = 0;
    this.#iCurWordPosStart = -1;
    this.#iCurWordPosEnd = -1;
  }
  setDelims(delims) {
    this.#delimiters = delims;
  }
  getAllWords(str, delims) {
    let delimToUse = this.delimitersToRegExp(delims);
    let re = new RegExp(delimToUse);
    let res = str.split(re).filter((s) => s.trim() !== "");
    return res;
  }

  delimitersToRegExp(delims) {
    let dlms = delims;
    let addDelim = "";
    if (dlms.indexOf("[") >= 0) {
      dlms = dlms.replace(/\[/g, "");
      addDelim += "\\[";
    }
    if (dlms.indexOf("]") >= 0) {
      dlms = dlms.replace(/\]/g, "");
      addDelim += "\\]";
    }
    if (dlms.indexOf("^") >= 0) {
      dlms = dlms.replace(/\^/g, "");
      addDelim += "\\^";
    }
    return `[${dlms + addDelim}]`;
  }

  nextWord() {
    if (!this.skipDelimiters()) return null;
    this.findEndOfWord();
    let obj = {
      word: this.#strToParce.substring(
        this.#iCurWordPosStart,
        this.#iCurWordPosEnd
      ),
      start: this.#iCurWordPosStart,
      end: this.#iCurWordPosEnd,
    };
    return obj;
  }

  skipDelimiters() {
    for (let i = this.#iPos; i < this.#strToParce.length; i++) {
      let ch = this.#strToParce.substring(i, i + 1);
      if (this.#delimiters.indexOf(ch) >= 0) continue;
      this.#iCurWordPosStart = i;
      this.#iCurWordPosEnd = -1; // not yet known
      this.#iPos = i;
      return true;
    }
    return false;
  }

  findEndOfWord() {
    let i;
    for (i = this.#iPos; i < this.#strToParce.length; i++) {
      let ch = this.#strToParce.substring(i, i + 1);
      if (this.#delimiters.indexOf(ch) < 0) continue;
      this.#iCurWordPosEnd = i;
      this.#iPos = i;
      return true;
    }
    this.#iCurWordPosEnd = i;
    this.#iPos = i;
    return true;
  }

  currentWordPosition() {
    return { start: this.#iCurWordPosStart, end: this.#iCurWordPosEnd };
  }
}

module.exports = Parser;
