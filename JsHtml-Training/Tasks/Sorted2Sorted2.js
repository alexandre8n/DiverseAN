//There is a sorted array of integer numbers:
// Example: [-3, 2, 4]
//develop a function to build a new sorted array
// of squares of these numbers. Expected: [4,9,16]
let srcArr = [-10, -3, -1, 0, 2, 7, 12];

let ar2 = getSortedSqrs(srcArr);
console.log(ar2);

function getSortedSqrs(arr) {
  let arRes = [];
  let i = -1;
  for (i = 0; i < arr.length; i++) {
    if (arr[i] >= 0) break;
  }
  if (i == arr.length || i == 0) {
    arRes = arr.map((x) => x * x);
    return arRes;
  }
  let iPos = i;
  let iNeg = i - 1;
  while (true) {
    let checkRes = checkRanges(arr, iNeg, iPos, arRes);
    if (checkRes == 0) return arRes;
    let cur;
    if (checkRes == -1) {
      cur = arr[iNeg];
      iNeg--;
    } else if (checkRes == 1) {
      cur = arr[iPos];
      iPos++;
    } else return null;
    arRes.push(cur * cur);
  }
  return null;
}

function checkRanges(arr, iNeg, iPos, arRes) {
  if (iNeg < 0 && iPos >= arr.length) return 0;
  if (iNeg < 0) return 1;
  if (iPos >= arr.length) return -1;
  if (-arr[iNeg] < arr[iPos]) return -1;
  return 1;
}

function sortedArrToSortedOfAbs(arr) {
  const result = new Array(arr.length); // Create a new array to store the result
  let i = 0;
  let j = arr.length - 1;
  let k = arr.length - 1;
  // Handle negative and positive elements simultaneously using two pointers
  while (i <= j) {
    const absI = Math.abs(arr[i]);
    const absJ = Math.abs(arr[j]);
    if (absI > absJ) {
      result[k] = absI;
      i++;
    } else {
      result[k] = absJ;
      j--;
    }
    k--;
  }
  return result;
}
