function isPrime(number) {
  if (number <= 1) return false;

  // The check for the number 2 and 3
  if (number <= 3) return true;

  if (number % 2 == 0 || number % 3 == 0) return false;

  for (var i = 5; i * i <= number; i = i + 6) {
    if (number % i == 0 || number % (i + 2) == 0) return false;
  }

  return true;
}

function isPrime2(num) {
  var sqrtnum = Math.floor(Math.sqrt(num));
  var prime = num != 1;
  for (var i = 2; i < sqrtnum + 1; i++) {
    // sqrtnum+1
    if (num % i == 0) {
      prime = false;
      break;
    }
  }
  return prime;
}

function factorize(int_number) {
  var newnum = int_number;
  var newtext = "";
  var checker = 2;
  while (checker * checker <= newnum) {
    if (newnum % checker == 0) {
      newtext = newtext + checker;
      newnum = newnum / checker;
      if (newnum != 1) {
        newtext = newtext + "*";
      }
    } else {
      checker++;
    }
  }
  if (newnum != 1) {
    newtext = newtext + newnum;
  }
  if (newtext == "" + int_number) {
    newtext = int_number + " is a Prime Number";
  }

  return newtext;
}

function getRandomNumber(size) {
  return Math.floor(Math.random() * size);
}

// this function returns the number from "from" to "to";
function getRandomIntFromTo(from, to) {
  var size = to - from + 1;
  return from + getRandomNumber(size);
}

function Sequence(from, to) {
  var arr = Array.from({ length: to - from + 1 }, (v, k) => from + k);
  return arr;
}

// returns an array of integers with specified size, integers are from ... to
function generateRandomNoDupplicate(size, from, to) {
  if (size > to - from + 1) return [];

  var arr = Array.from({ length: to - from + 1 }, (v, k) => from + k);
  var lastArrElmIdx = arr.length - 1;
  var randomArr = [];
  for (let i = 0; i < size; i++) {
    let idx = getRandomIntFromTo(0, lastArrElmIdx);
    randomArr.push(arr[idx]);
    arr[idx] = arr[lastArrElmIdx];
    lastArrElmIdx--;
  }
  return randomArr;
}

// returns {row: r, col: c} in array, if known the sequential number
// m*n: 0, ..., m*n-1    0 1 2 3   2*4
// 0,1,2,3..., m-1       4 5 6 7
// m,1,2,      2m-1
// 2m,         3m-1
// ...
// (n-1)*m,..., m*n-1
// N = r(m-1) + c   (r,c)   N=0 -> c = (N+1)%4-1    r = ceil((N+1)/4) -1
function rowColBySeqNo(nCol, seqN) {
  var r = Math.ceil((seqN + 1) / nCol) - 1;
  var c = seqN % nCol;
  return { row: r, col: c };
}

function digitCount(number) {
  let str = "" + number;
  return str.length;
}

function digits(number) {
  return [...(number + "")].map((n) => +n);
}

// usage of stopwatchInit and getElepsedTime
// var t0 = stopwatchInit();
// ...
// var timeInMiliSec = getElapsedTime(t0);

function stopwatchInit() {
  const d = new Date(); // used for debug
  return d.getTime();
}
function getElapsedTime(time0) {
  const d = new Date();
  return d.getTime() - time0;
}

// returns: dd/mm/yyyy HH:MM:SS
function datetimeToString(date) {
  var month = date.getMonth() + 1;
  var day = date.getDate();
  var dateOfString = (("" + day).length < 2 ? "0" : "") + day + "/";
  dateOfString += (("" + month).length < 2 ? "0" : "") + month + "/";
  dateOfString += date.getFullYear();
  dateOfString +=
    " " + date.getHours() + ":" + date.getMinutes() + ":" + getSeconds();
  return dateOfString;
}

function str2Date(str) {
  if (str == null) return null;
  var tp = typeof str;
  try {
    var dt = new Date(str);
    return dt;
  } catch (err) {
    return null;
  }
  return null;
}

function parseOperationStr(oprStr) {
  // receives: a (+) b = c, returns obj
  const regExp1 = /(\d+)\s*([*-+/])\s*(\d+)/g;
  let res = regExp1.exec(oprStr);
  if (!res) return null;
  let n1 = parseInt(res[1]);
  let n2 = parseInt(res[3]);
  let opr = res[2];
  return { n1: n1, n2: n2, opr: opr };
}

// remove all child nodes of specified node
function removeAllChilds(node) {
  if (!node) return;
  let last = null;
  while ((last = node.lastChild)) node.removeChild(last);
}

// find element by one of names and toggle (replace) the name and the text
function toggle12(nm1, nm2, tx1, tx2) {
  let hereElm = document.getElementsByName(nm1);
  if (hereElm.length > 0) {
    hereElm = hereElm[0];
    hereElm.name = nm2;
    hereElm.innerHTML = tx2;
    return hereElm;
  }
  hereElm = document.getElementsByName(nm2);
  if (hereElm.length > 0) {
    hereElm = hereElm[0];
    hereElm.name = nm1;
    hereElm.innerHTML = tx1;
    return hereElm;
  }
  return null;
}

function getUrlParam(parName) {
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);
  const paramVal = urlParams.get(parName);
  return paramVal;
}
