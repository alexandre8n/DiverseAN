// https://ru.wikipedia.org/wiki/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BE%D1%87%D0%BD%D0%BE%D0%B9_%D1%81%D1%82%D0%B0%D0%BD%D1%86%D0%B8%D0%B8
// https://ru.wikibooks.org/wiki/%D0%A0%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8_%D0%B0%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%D0%BE%D0%B2/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BE%D1%87%D0%BD%D0%BE%D0%B9_%D1%81%D1%82%D0%B0%D0%BD%D1%86%D0%B8%D0%B8
// https://algolist.ru/syntax/revpn.php
class ExpressionParser0 {
  inputStr;
  outputStr;
  stack;
  msg;
  idx;

  constructor() {}

  parse(str) {
    this.inputStr = str;
    this.outputStr = "";
    this.stack = [];
    this.msg = "";
    this.idx = -1;
    for (let i = 0; i < this.inputStr.length; i++) {
      this.idx = i;
      let c = this.inputStr[i];
      if (c == " ") continue;
      if (this.isOperand(c)) {
        this.outputStr += c;
        continue;
      }
      if (this.isFunc(c)) {
        this.pushToStack(c);
        continue;
      }
      if (c == ",") {
        while (true) {
          let c1 = this.checkOprInHeadOfStack();
          if (c1 == "(") break;
          c1 = this.popFromStack();
          if (c1 == null) {
            throw "missing delimiter in arg list, or ( in function";
          }
          this.stack += c1;
        }
        continue;
      }
      if (this.isOper(c)) {
        let prio1 = this.getOprPrio(c);
        while (true) {
          let op2 = this.checkOprInHeadOfStack();
          if (op2 == null) break;
          let prio2 = this.getOprPrio(op2);
          if (prio2 > prio1 || (prio2 == prio1 && isOperLeftAssoc(c))) {
            this.popFromStack();
            this.outputStr += op2;
          } else break;
        }
        this.pushToStack(c);
        continue;
      }
      if (c == "(") {
        this.pushToStack(c);
        continue;
      }
      if (c == ")") {
        this.fromStackToOutTillOpnParanthis();
        continue;
      }
    }
    // no more tokens
    let opInStack = this.checkOprInHeadOfStack();
    while (opInStack != null) {
      opInStack = this.popFromStack();
      if (opInStack == "(") throw "( - unbalanced ";
      this.outputStr += opInStack;
      opInStack = this.checkOprInHeadOfStack();
    }
  }
  fromStackToOutTillOpnParanthis() {
    let op2 = this.checkOprInHeadOfStack();
    while (op2 != "(") {
      if (op2 == null) {
        throw "( - is missing in expression";
      }
      this.popFromStack();
      this.outputStr += op2;
      op2 = this.checkOprInHeadOfStack();
    }
    // == (
    this.popFromStack();
    // todo: for the func case modify!!
    // if (this.isFunc(op2)) {
    //   this.popFromStack();
    //   this.outputStr += op2;
    // }
  }
  isOperand(c) {
    if (c.match(/\w/)) {
      return true;
    }
    return false;
  }
  pushToStack(c) {
    this.stack.unshift(c);
  }
  checkOprInHeadOfStack() {
    return this.stack[0];
  }
  popFromStack() {
    return this.stack.shift();
  }
  getOprPrio(c) {
    switch (c) {
      case "*":
      case "/":
        return 3;

      case "-":
      case "+":
        return 2;

      case "(":
        return 1;
    }
  }
  isOperLeftAssoc(c) {
    return true;
  }
  isOper(c) {
    return "+-*/".indexOf(c) >= 0;
  }
  isFunc(c) {
    return false;
  }
}

test("a*(b+c");
test("a*b+c");
test("(a+b)*c");
test("(a+b)*(c+d)");
test("(a+b)*(c+d");

function test(exp) {
  let ep = new ExpressionParser0();
  try {
    ep.parse(exp);
  } catch (error) {
    console.log("Error: " + error);
    return;
  }
  console.log("input: " + ep.inputStr);
  console.log("output: " + ep.outputStr);
}
