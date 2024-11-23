let inStr = "1,2,3,4,5,6,3";
let arIds = inStr.split(",").map((x) => parseInt(x));
// {key: x, next=obj}
let arObjs = [];
for (let i = 0; i < arIds.length; i++) {
  let obj = { key: arIds[i], next: null };
  let idx = arIds.indexOf(arIds[i]);
  if (idx < i) {
    // loop case
    obj = arObjs[idx];
    arObjs[i - 1].next = obj;
    break;
  }
  arObjs.push(obj);
  if (i > 0) arObjs[i - 1].next = obj;
}
let first = arObjs[0];
let sOut = unidirectedList2Str(first);
if (sOut) console.log(sOut);
let last = getLastInUniDirListBy2Ponters(first);
console.log(`found 1 last=${last.key}`);
let last2 = findLast(first);
console.log(`found 2 last=${last2.key}`);

// {key: x, next=obj}
function getLastInUniDirListBy2Ponters(n1) {
  if (n1 == null) return n1;
  let p1 = n1;
  let p2 = n1;
  let i = 1,
    j = 1;
  for (; ; i++, j++) {
    let p1next = p1.next;
    let p2next = p2.next;
    if (!p1next) return p1;
    if (!p2next) return p2;
    if (p2next == p1) {
      // loop is identified
      p2 = p2next;
      break;
    }
    p2 = p2next.next;
    if (!p2) return p2next;
    j++;
    if (p2.key == p1.key) break;
    p1 = p1next;
    console.log(`step-i,j=${i},${j} p1=${p1.key}, p2=${p2.key}`);
  }
  console.log(`Exit:i=${i},j=${j} p1=${p1.key}, p2=${p2.key}`);
  let cycle = j - i + 1;
  p1 = n1;
  for (let stp = 0; stp < i; stp++) {
    let [pRes, pPrev] = doSteps(p1, cycle);
    if (p1.key == pRes.key) return pPrev;
    p1 = p1.next;
  }
  return p1;
}
function doSteps(p1, nSteps) {
  let pNext = p1;
  let pPrev = null;
  for (let i = 0; i < nSteps; i++) {
    pPrev = pNext;
    pNext = pNext.next;
    if (p1.key == pNext.key) return [pNext, pPrev];
  }
  return [pNext, pPrev];
}
function unidirectedList2Str(n1) {
  let str = "";
  if (n1 == null) return str;
  let arr = new Set();
  str += n1.key;
  arr.add(n1.key);
  while (n1.next != null) {
    n1 = n1.next;
    if (arr.has(n1.key)) return str + "->" + n1.key;
    else str = str.concat("->" + n1.key);
    arr.add(n1.key);
  }
  return str;
}

function findLast(head) {
  if (!head) return null;

  let slow = head;
  let fast = head;

  // Step 1: Detect cycle using Floyd's Tortoise and Hare algorithm
  while (fast !== null && fast.next !== null) {
    slow = slow.next;
    fast = fast.next.next;

    if (slow === fast) {
      break;
    }
  }

  // Step 2: If no cycle, find the last node
  if (fast === null || fast.next === null) {
    while (slow.next !== null) {
      slow = slow.next;
    }
    return slow;
  }

  // Step 3: If a cycle is detected, find the start of the cycle
  // it will happen in N steps. Why? Because if both met in N+x,
  // if the slow starts from head in N steps he will be in the beginning of the loop
  // but the second will be there too, because in N+x steps he will be in N+x!
  slow = head;
  while (slow !== fast) {
    slow = slow.next;
    fast = fast.next;
  }
  // now the slow and fast are at the point, starting the loop
  // to find the last is possible in the steps to return, but just
  // keeping saved the previous.
  let prev = slow;

  // Step 4: Determine the length of the cycle
  let cycleLength = 1;
  fast = slow.next;
  while (fast !== slow) {
    prev = fast;
    fast = fast.next;
    cycleLength++;
  }
  // NOW: prev is the last point before the start of loop
  console.log("!!!the last is:", prev.key);
  console.log("the loop size:", cycleLength);

  // Step 5: Move pointer to find the last node in the cycle
  let ptr1 = head;
  let ptr2 = head;

  for (let i = 0; i < cycleLength; i++) {
    ptr2 = ptr2.next;
  }

  while (ptr1 !== ptr2) {
    ptr1 = ptr1.next;
    ptr2 = ptr2.next;
  }

  // Step 6: Find the node just before the start of the cycle
  while (ptr2.next !== ptr1) {
    ptr2 = ptr2.next;
  }

  return ptr2;
}

// Helper function to create a linked list node
function ListNode(key, next = null) {
  this.key = key;
  this.next = next;
}
