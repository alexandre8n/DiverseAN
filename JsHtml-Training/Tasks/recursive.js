//let expr = "{aa{a}99}100";
let expr = "{aa{bb{cc}d{a}d}99}100";
let iStart = skipTill(expr, 0, "{");
let state = { iStart: -1, iPos: 0, level: 0 };
findClosingParanthesis(expr, state);
console.log(`input:  ${expr}`);
console.log(`start: ${state.iStart} end:${state.iPos}`);
console.log(`output: ${expr.slice(state.iStart, state.iPos + 1)}`);

function findClosingParanthesis(str, state) {
  if (state.iPos >= str.length) return;
  if (str[state.iPos] == "{") {
    if (state.level == 0) state.iStart = state.iPos;
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

function fndCloPrnth(str, iPos) {
  if (iPos >= str.length) return str.length;
  if (str[iPos] == "{") {
    lev++;
    iPos = fndCloPrnth(str, iPos + 1);
    if (iPos >= str.length) return str.length;
    if (lev == 0) return iPos;
  }
  if (str[iPos] == "}") {
    lev--;
    if (lev <= 0) return iPos;
  }
  iPos = fndCloPrnth(str, iPos + 1);
  return iPos;
}

function findClosingPrnth1(expr, iStart) {
  if (iStart >= expr.length) return iStart;
  let iPos = skipTill(expr, iStart + 1, "{}");
  if (iPos >= expr.length) return iPos;
  let curStr = expr.slice(iPos);
  if (expr[iPos] == "{") {
    let iPosInt = findClosingPrnth1(expr, iPos + 1);
    curStr = expr.slice(iPosInt);
    let iPosInt2 = findClosingPrnth1(expr, iPosInt + 1);
    curStr = expr.slice(iPosInt2);
    return iPosInt2;
  }
  return iPos;
}
function skipTill(str, iStart, delims) {
  // skip till one of delims is found
  // returns: position of one of delimiters
  for (let i = iStart; i < str.length; i++)
    if (delims.indexOf(str[i]) >= 0) return i;
  return str.length + 1;
}
