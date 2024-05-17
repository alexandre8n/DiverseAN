const fs = require("fs");
const { createConnection } = require("net");
const { gunzip } = require("zlib");
const fileName = "../Files/peopleConnections.txt";
let res = fs.readFileSync(fileName, "utf8");
let lines = textTolines(res);
let relations = getRelations(lines); // name: Name, Name2
let members = new Set();
relations.forEach((el, index) => {
  members.add(el.name);
  members.add(el.knows);
});
let peopleConnections = []; // array of personal connections {name: Vasy, knows: Pety, Masha}
for (let name of members) {
  let obj = createConnections(name, relations);
  peopleConnections.push(obj);
}
let peopleNoConnections = []; // array of personal no connections {name: Vasy, knows: Pety, Masha}
for (let name of members) {
  let obj = listUnknown(name, relations, members);
  peopleNoConnections.push(obj);
}

let result = "";
peopleConnections.forEach((e) => {
  console.log(`${e.name}: ${e.knows}`);
  result += `${e.name}: ${e.knows}\n`;
});
peopleNoConnections.forEach((e) => {
  console.log(`${e.name} doesn't know: ${e.unknown}`);
  result += `${e.name} doesn't know: ${e.unknown}\n`;
});

let fileNameRes = fileName.slice(0, -4) + "Result.txt";
fs.writeFile(fileNameRes, result, "utf8", function (err) {
  if (err) return console.log(err);
});
let a = "Vasya"; // sorse
let b = "Pety"; // target
let rezchain = getChain(relations, a, b);
if (rezchain == "no") {
  console.log(`sourse: ${a}, target: ${b} Has no connections in the group`);
} else if (rezchain == null) console.log(`${b} is not a member of the group`);
else {
  let str1 = rezchain.split(",").slice(1, -1).join("->");
  let resStr = `source: ${a}, target: ${b}, path: ${str1}`;
  console.log(resStr);
}
// just for info---------------
// how to append the file, write at the end of existing file
//const fs = require('fs');
// fs.appendFile('message.txt', 'data to append', function (err) {
//   if (err) throw err;
//   console.log('Saved!');
// });
function getChain(relations, source, target) {
  let members = new Set();
  relations.forEach((element) => {
    members.add(element.name);
    members.add(element.knows);
  });
  if (!members.has(source) || !members.has(target)) return null;
  //let members1 = new Set()
  let levels = [];
  let peopleIncluded = new Set();
  levels.push([source]);
  peopleIncluded.add(source);
  let prsToTrg = "";
  while (prsToTrg == "") {
    prsToTrg = fillLevel(levels, relations, target, peopleIncluded);
  }
  if (prsToTrg == null) return "no";
  let pathToTargetArr = pathToTarget(relations, prsToTrg, levels);
  pathToTargetArr.reverse();
  pathToTargetArr.push(target);
  let strPath = pathToTargetArr.toString();
  return strPath;
}

function pathToTarget(relations, prsToTrg, levels) {
  let arrPath = [prsToTrg];
  let target = prsToTrg;
  for (let i = levels.length - 2; i >= 0; i--) {
    let arr = nextLevel(relations, target, levels[i], null);
    arrPath.push(arr[1]);
    target = arr[1];
  }
  return arrPath;
}
function fillLevel(levels, relations, target, peopleIncluded) {
  let arr = nextLevel(
    relations,
    target,
    levels[levels.length - 1],
    peopleIncluded
  );
  if (arr == null) return null;
  if (arr[0] == target) return arr[1];
  levels.push(arr);
  return "";
}

function nextLevel(relations, target, lastLevel, peopleIncluded) {
  let arrObjName = new Set();
  for (let guy of lastLevel) {
    let arr = relations.filter((el) => guy == el.name).map((el) => el.knows);
    if (peopleIncluded != null)
      arr = arr.filter((el) => !peopleIncluded.has(el));
    if (arr.includes(target)) return [target, guy];
    arr.forEach((el) => arrObjName.add(el));
  }
  if (arrObjName.size == 0) return null;
  arrObjName.forEach((el) => peopleIncluded.add(el));

  return Array.from(arrObjName);
}

function createConnections(name, relations) {
  let myrelatioms = relations
    .filter((el) => name == el.name)
    .map((e) => e.knows);
  let knows = myrelatioms.join(", ");
  let objPersonalConections = { name: name, knows: knows };
  return objPersonalConections;
}
function listUnknown(name, relations, members) {
  let myrelatioms = relations
    .filter((el) => name == el.name)
    .map((e) => e.knows);
  let isInArray = function (nameTst) {
    if (nameTst == name) return false;
    let known = myrelatioms.includes(nameTst);
    return !known;
  };
  let all = Array.from(members); //[...members]
  let strangers = all.filter(isInArray);
  let unKnown = strangers.join(", ");
  return { name: name, unknown: unKnown };
}
function textTolines(str) {
  let arrLines = str.split("\r\n");
  let isToend = false;
  arrLines = arrLines.filter((elem) => {
    if (isToend) return false;
    let str3 = elem.slice(0, 3);
    if (str3 == "---") {
      isToend = true;
      return false;
    }
    return true;
  });
  return arrLines;
}
function getRelations(lines) {
  let arrRelation = [];
  for (let str of lines) {
    let relObj = lineToRel(str);
    arrRelation.push(relObj);
  }
  return arrRelation;
}
function lineToRel(str) {
  let arrWords = str.split(" ");
  let relObj = { name: arrWords[0], knows: arrWords[2] };
  return relObj;
}
