// recursive calls examples
//let expr = "if(a){if(a>1){if(a)r();}; 78 }79 aa";
let expr = "{{{{if(a){if(a>1){}a{f}}{if(a)r();}; 78 }79 aa";

let state = { iStart: -1, iPos: 0, level: 0 };
findClosingParanthesis(expr, state);
console.log(`input:  ${expr}`);
console.log(`start: ${state.iStart} end: ${state.iPos}`);
console.log(`output: ${expr.slice(state.iStart, state.iPos + 1)}`);
let iStart = 0;
let iStartOpnParnth = -1;
let level = 0;
let rez = findClosingPrnth(expr, iStart, 0);
console.log(rez, expr.slice(iStartOpnParnth, rez));

let f1 = factorial(7);
console.log("7!=" + f1);

//let expr = "if(a){if(a>1){if(a==7){r());};i++}}    ";
//let iClosing = findClosingPrnth(expr, 0);
//let iStart = skipTill(expr, 0, "{");
//let iClosing = findClosingPrnth1(expr, iStart);

//console.log(iClosing, expr.slice(iClosing));

function findClosingPrnth1(expr, iStart) {
  //let curStr = expr.slice(iStart);
  if (expr[iStart] == "{") {
    let iPos = findClosingPrnth1(expr, iStart + 1);
    return iPos;
  }
  if (expr[iStart] == "}") return iStart;
  return skipTill(expr, iStart, "{}");
}
function skipTill(str, iStart, delims) {
  for (let i = iStart; i < str.length; i++)
    if (delims.indexOf(str[i]) >= 0) return i;
}

function findClosingPrnth(expr, iStart, level) {
  if (iStart >= expr.length) return expr.length;
  let curStr = expr.slice(iStart);

  if (expr[iStart] == "{") {
    if (iStartOpnParnth == -1) iStartOpnParnth = iStart;
    level++;
    let iPos = findClosingPrnth(expr, iStart + 1, level);
    return iPos;
  }
  if (expr[iStart] == "}") {
    level--;
    if (level == 0) return iStart + 1;
  }
  return findClosingPrnth(expr, iStart + 1, level);
}

function factorial(n) {
  // return (n<=1)? 1: n * factorial(n - 1);
  //  console.log("entered with n=" + n);
  if (n <= 1) return 1;
  let res = n * factorial(n - 1);
  //console.log("returning result:" + res);
  return res;
}

function findClosingParanthesis(str, state) {
  if (state.iPos >= str.length) return;
  if (str[state.iPos] == "{") {
    if (state.level == 0) {
      state.iStart = state.iPos;
    }
    state.iPos++;
    state.level++;
    findClosingParanthesis(str, state);
    if (state.iPos >= str.length) return;
    if (state.level == 0) return;
  }
  if (str[state.iPos] == "}") {
    state.level--;
    if (state.level <= 0) return;
  }
  state.iPos++;
  findClosingParanthesis(str, state);
}
