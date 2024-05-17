function mesureTime(nTimes, func, par1) {
  let t0 = stopwatchInit();
  for (let i = 0; i < nTimes; i++) {
    func(par1);
  }
  let t = getElapsedTime(t0);
  return t;
}

function compareArrays(ar1, ar2) {
  if (ar1.length != ar2.length) return false;
  for (let i = 0; i < ar1.length; i++) {
    if (ar1[i] != ar2[i]) return false;
  }
  return true;
}

function getRandomNumber(size) {
  return Math.floor(Math.random() * size);
}
function stopwatchInit() {
  const d = new Date(); // used for debug
  return d.getTime();
}
function getElapsedTime(time0) {
  const d = new Date();
  return d.getTime() - time0;
}

// this function returns the number from "from" to "to";
function getRandomIntFromTo(from, to) {
  var size = to - from + 1;
  return from + getRandomNumber(size);
}
function swap2(arr, i, j) {
  const t = arr[i];
  arr[i] = arr[j];
  arr[j] = t;
}
