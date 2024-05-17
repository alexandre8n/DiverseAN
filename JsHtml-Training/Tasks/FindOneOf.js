let operationsArray = [
  "+",
  "=",
  "==",
  "<>",
  "!=",
  "!",
  ">=",
  "<=",
  "<",
  ">",
  "&&",
  "*",
];
let str = "a*b==12 && !x";
//let rezArray = sortArray(operationsArray);
//let foundOper = parsString(str, operationsArray);

let foundOper = findOneOf(str, operationsArray);

let foundSub = findOneOf("Hello, Alex, 12", ["12", "b=", "Alex"]);

console.log(foundOper);
console.log(foundSub);

/////////////////////////////////
function sortArray(arr) {
  let sortedArr = [...arr];
  sortedArr.sort((a, b) => b.length - a.length);
  return sortedArr;
}
function parsString(inpStr, ops) {
  let foundOperators = [];
  for (let i = 0; i < ops.length; i++) {
    let operCurrent = ops[i];
    let operLength = operCurrent.length;
    let idxOfOper = inpStr.indexOf(operCurrent);
    if (idxOfOper == -1) continue;
    foundOperators.push({ pos: idxOfOper, operatorName: operCurrent });
    //foundOperators.push([idxOfOper, operCurrent]);
  }
  foundOperators.sort((a, b) => {
    //return a.pos - b.pos;
    if (a.pos > b.pos) return 1;
    if (a.pos < b.pos) return -1;
    return b.operatorName.length - a.operatorName.length;
  });
  return foundOperators[0];
}
// знайти без допомоги массиву
function parsString2(inpStr, ops) {
  let bestFoundOperator = { pos: inpStr.length, name: "" };
  for (let i = 0; i < ops.length; i++) {
    let operCurrent = ops[i];
    let idxOfOper = inpStr.indexOf(operCurrent);
    if (idxOfOper == -1) continue;
    if (idxOfOper > bestFoundOperator.pos) continue;
    bestFoundOperator = { pos: idxOfOper, name: operCurrent };
  }
  return bestFoundOperator.name;
}

function findOperator(rez, index) {
  let i = rez.slice(0, 2);
}

function findOneOf(inpStr, opers) {
  let ops = [...opers].sort((a, b) => b.length - a.length);
  for (let i = 0; i < inpStr.length; i++) {
    let resOpr = checkIfOperInPos(ops, inpStr, i);
    if (resOpr) return resOpr;
  }
  return "";
}

function checkIfOperInPos(ops, inpStr, idx) {
  for (let i = 0; i < ops.length; i++) {
    let operLen = ops[i].length;
    let opCandidate = inpStr.slice(idx, idx + operLen);
    if (opCandidate == ops[i]) return opCandidate;
  }
  return null;
}
