// https://ru.wikipedia.org/wiki/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BE%D1%87%D0%BD%D0%BE%D0%B9_%D1%81%D1%82%D0%B0%D0%BD%D1%86%D0%B8%D0%B8
// https://ru.wikibooks.org/wiki/%D0%A0%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D0%B8_%D0%B0%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%D0%BE%D0%B2/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BE%D1%87%D0%BD%D0%BE%D0%B9_%D1%81%D1%82%D0%B0%D0%BD%D1%86%D0%B8%D0%B8
// https://algolist.ru/syntax/revpn.php
let operators = [
  { id: ".", name: "dotFunc", prio: 0, left: ")" },
  { id: "*", name: "multiply", prio: 1 },
  { id: "/", name: "divide", prio: 1 },
  { id: "%", name: "remider", prio: 1 },
  //  { id: "+", name: "plusUnar", prio: 2 },
  //  { id: "-", name: "minusUnar", prio: 2 },
  { id: "+", name: "plus", prio: 3 },
  { id: "-", name: "minus", prio: 3 },
  { id: "==", name: "equal", prio: 4 },
  { id: "!=", name: "not-equal", prio: 4 },
  { id: "<>", name: "not-equal", prio: 4 },
  { id: "<", name: "less", prio: 4 },
  { id: ">", name: "more", prio: 4 },
  { id: "<=", name: "lessOrEqual", prio: 4 },
  { id: "<=", name: "lessOrEqual", prio: 4 },
  { id: ">=", name: "moreOrEqual", prio: 4 },
  { id: "||", name: "or", prio: 5 },
  { id: "&&", name: "and", prio: 6 },
  { id: "!", name: "not", prio: 7 },
];
let operandTypes = ["Text", "Number", "Bool", "Variable", "VariableWithDot"];
let itemObject = { type: "Unknown", val: "", start: -1, end: -1 };
// type is one of: Unknown, Text, Number, Bool, Variable, Operator

class ExpressionParser {
  inputStr;
  outNodes;
  stack;
  msg;
  idx;
  maxOperationLengh;
  constructor() {
    this.maxOperationLengh = operators.reduce(
      (prevMax, cur) => Math.max(prevMax, cur.id.length),
      0
    );
  }

  parse(str) {
    this.inputStr = str;
    this.outNodes = [];
    this.stack = [];
    this.msg = "";
    this.idx = -1;

    for (let i = 0; i < this.inputStr.length; i++) {
      if (i < this.idx) i = this.idx;
      else this.idx = i;
      let c = this.inputStr[i];
      if (c == " ") continue;
      if (c == ",") {
        this.prcComa();
        continue;
      }
      if (c == "(") {
        this.pushToStack(c);
        continue;
      }
      if (c == ")") {
        if (!this.fromStackToOutTillOpnParanthis()) {
          return false;
        }
        continue;
      }

      // getNextItem returns item obj (see above)
      let curItem = this.getNextItem(i); // opr|func|(|)|identif|const
      i = curItem.end - 1;
      if (this.isOperand(curItem)) {
        this.outNodes.push(curItem);
        continue;
      }
      if (curItem.type == "Function") {
        this.pushToStack(curItem);
        continue;
      }
      if (curItem.type == "Operator") {
        let prio1 = this.getOprPrio(curItem);
        while (true) {
          let op2 = this.readHeadOfStack();
          if (op2 == null) break;
          // todo: to check if correct
          if (op2.val == "(") break;
          let prio2 = this.getOprPrio(op2);
          if (
            prio2 > prio1 ||
            (prio2 == prio1 && this.isOperLeftAssoc(curItem))
          ) {
            this.popFromStack();
            this.outNodes.push(op2);
          } else break;
        }
        this.pushToStack(curItem);
        continue;
      }
    }
    // no more tokens
    while (!this.stackIsEmpty()) {
      let opInStack = this.popFromStack();
      if (opInStack.val == "(") {
        throw "( - unbalanced ";
      }
      this.outNodes.push(opInStack);
    }
  }
  prcComa() {
    //As long as the token at the top of the stack is not ( do:
    // - Push the operator from the stack to the output queue.
    // - If the stack ran out before the open brace token was encountered,
    // - then the function argument separator (,) is missing in the expression,
    //   or the opening parenthesis is missing.
    while (!this.stackIsEmpty()) {
      let c1 = this.readHeadOfStack();
      if (c1.val == "(") return;
      c1 = this.popFromStack();
      this.outNodes.push(c1);
    }
    throw "Error: missing delimiter in arg list, or ( in function";
  }
  fromStackToOutTillOpnParanthis() {
    let op2 = this.readHeadOfStack();
    while (op2.val != "(") {
      if (op2 == null) {
        throw "( - is missing in expression";
      }
      this.popFromStack();
      this.outNodes.push(op2);
      op2 = this.readHeadOfStack();
    }
    // == (
    this.popFromStack();
    let opFunc = this.readHeadOfStack();
    if (opFunc && opFunc.type == "Function") {
      opFunc = this.popFromStack();
      this.outNodes.push(opFunc);
      return true;
    }
    // todo: to check if this is OK
    return false;
  }
  isOperand(c) {
    if (operandTypes.indexOf(c.type) > 0) return true;
    return false;
  }
  pushToStack(c) {
    if (!c.type) {
      // coma, or (
      let iEnd = this.idx + c.length;
      c = { type: "Unknown", val: c, start: this.idx, end: iEnd };
    }
    this.stack.unshift(c);
  }
  readHeadOfStack() {
    return this.stack[0];
  }
  stackIsEmpty() {
    return this.stack.length == 0;
  }
  popFromStack() {
    return this.stack.shift();
  }
  getOprPrio(c) {
    let opFound = operators.find((op) => op.id == c.val);
    return opFound.prio;
  }
  isOperLeftAssoc(c) {
    return true;
  }
  getNextItem(i) {
    let item = { type: "Unknown", val: null, start: i, end: i };
    if (this.isTextConst(i, item, "+,")) {
      return item;
    }
    if (this.isNumber(i, item)) {
      return item;
    }
    if (this.isBool(i, item)) {
      return item;
    }
    if (this.isVarName(i, item)) {
      return item;
    }
    if (this.isOperator(i, item)) {
      return item;
    }
    if (this.isFunctionName(i, item)) {
      return item;
    }
    return this.inputStr[i];
  }
  isTextConst(i, item, allowedDelimsAfter) {
    let c = this.inputStr[i];
    if (c != '"' && c != "'") return false;
    let j = findClosingQm(this.inputStr, i, ",+");
    if (j == -1) {
      this.msg += `Error: expected text literal, closing quatation mark not found in Line:\n${
        this.inputStr
      }\nPosition: ${i + 1}`;
      throw this.msg;
    }
    item.type = "Text";
    item.val = this.inputStr.slice(i + 1, j);
    item.end = j + 1;

    if (j >= this.inputStr.length) return true;
    let charAfter = this.inputStr.slice(j + 1).trim();

    if (allowedDelimsAfter.indexOf(charAfter[0]) < 0) {
      throw (
        `Error: unexpected char: ${charAfter[0]} after text literal\n` +
        `${item.val}`
      );
    }
    return true;
  }
  isNumber(i, item) {
    let str = this.inputStr.slice(i);
    if ("0123456789.".indexOf(str[0]) < 0) return false;
    const objFound = /^[.]?[0-9]+[.]?(\d+)?[eE]?[-+]?(\d+)?/.exec(str);
    if (objFound == null) return false;
    const len = objFound[0].length;
    const val = Number(objFound[0]);
    if (isNaN(val)) {
      return false;
      //throw `Error: incorrect number specified: ${objFound[0]}`;
    }
    item.type = "Number";
    item.val = val;
    item.end = i + len;
    return true;
  }
  isBool(i, item) {
    let str = this.inputStr.slice(i);
    const objFound = /^(true|false)/.exec(str);
    if (objFound == null) return false;
    const len = objFound[0].length;
    if (str.length > len) {
      // check for allowed delims
      if (" ,)".indexOf(str[len]) < 0) {
        throw `Error: unexpected delimiter after boolean: ${str.slice(
          0,
          7
        )}...`;
      }
    }
    const val = objFound[0] == "true";
    item.type = "Bool";
    item.val = val;
    item.end = i + len;
    return true;
  }
  isVarName(i, item) {
    let str = this.inputStr.slice(i);
    const objFound = /^[a-zA-Z_#]+([0-9a-zA-Z_#]+)?/.exec(str);
    if (objFound == null) return false;
    const len = objFound[0].length;
    if (str.length > len) {
      if (nextNotBlankChar(str, len) == "(") return false;
      // check for allowed delims
      if (" ,)=+-*/%!<>".indexOf(str[len]) < 0) {
        throw `Error: unexpected delimiter after identifier: ${str}`;
      }
    }
    const val = objFound[0];
    item.type = "Variable";
    item.val = val;
    item.end = i + len;
    return true;
  }
  isOperator(i, item) {
    let str = this.inputStr.slice(i, this.maxOperationLengh + i);
    let oprMatch = operators.reduce((prev, cur) => {
      let curOp = str.slice(0, cur.id.length);
      if (curOp != cur.id || prev.length > curOp.length) return prev;
      return curOp;
    }, "");
    if (oprMatch == "") return false;
    item.type = "Operator";
    item.val = oprMatch;
    item.end = i + oprMatch.length;
    return true;
  }
  isFunctionName(i, item) {
    let str = this.inputStr.slice(i);
    const objFound = /^[a-zA-Z_#]+([0-9a-zA-Z_#]+)?/.exec(str);
    if (objFound == null) return false;
    const val = objFound[0];
    const len = val.length;
    if (str.length == len) return false; // because we need ( for func
    // check for allowed delims
    if (nextNotBlankChar(str, len) != "(") return false;
    item.type = "Function";
    item.val = val;
    item.end = i + len;
    return true;
  }
}

function findClosingQm(str, iStart) {
  // return -1 if not quatation mark, or if unbalanced
  // quatmarks inside: "a\""; or 'O\'Connar'
  const qm = str[iStart];
  if (qm != "'" && qm != '"') return -1;
  for (let i = iStart + 1; i < str.length; i++) {
    if (isClosingQm(str, i, qm)) return i;
  }
  return -1;
}
function isClosingQm(str, iCur, qm) {
  if (str[iCur] == qm) {
    if (iCur - 1 > 0 && str[iCur] == "\\") return false;
    return true;
  }
  return false;
}

function nextNotBlankChar(str, iStart) {
  let idx = nextNotBlank(str, iStart);
  if (idx < 0) return "";
  return str[idx];
}

function nextNotBlank(str, iStart) {
  // returns index of the next non blank
  for (let i = iStart; i < str.length; i++) {
    if (str[i] != " ") return i;
  }
  return -1;
}

function replaceVariables(expression, prefixToVar) {
  const variableNames = extractVariableNames(expression);
  // word boundary \b to ensure whole word matches
  const regex = new RegExp(`\\b(${variableNames.join("|")})\\b`, "g");
  return expression.replace(regex, prefixToVar + "$1");
}

function extractVariableNames(expression) {
  const variableNames = [];
  const regex = /[a-zA-Z][a-zA-Z0-9]*/g; // Regular expression to match variable names
  let match;
  while ((match = regex.exec(expression)) !== null) {
    variableNames.push(match[0]); // Add matched variable name to the array
  }

  return variableNames;
}

test("abc(x,y+1)");
test(" a+b+c");
test("a*(b+c");
test("(a+b)*c");
test("(a+b)*(c+d)");
test("(a+b)*(c+d");

function test(exp) {
  let ep = new ExpressionParser();
  try {
    ep.parse(exp);
  } catch (error) {
    console.log("Error: " + error);
    return;
  }
  console.log("input: " + ep.inputStr);
  console.log("output: " + JSON.stringify(ep.outNodes));
}
