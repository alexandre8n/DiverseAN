const fs = require("fs");
const sqlite = require("./sqlightSyncHelper.js");

let allLinesTxt = fs.readFileSync("./input/sales.txt", "utf8");
let dbFilePath = "./Work/GitHub/DiverseAN/NodeJs/SqlightDbs/";
let dbFile = "./test2.db";
//dbFile = dbFilePath + dbFile;
try {
  fs.unlinkSync(dbFile);
} catch (e) {}
mainApp();

async function mainApp() {
  let propNames = ["id", "amount", "date"];
  let salesArr = csvToArray(allLinesTxt, propNames);
  let result = await sqlite.open(dbFile);
  // let sqlCr =
  //   "CREATE TABLE IF NOT EXISTS sales (id INTEGER PRIMARY KEY,amount REAL,dat1 TEXT (20)";
  // let r1 = await sqlite.run(sqlCr);

  let sql1 = "insert into sales (amount, dat1) values(5.5, ?)";
  let r = await sqlite.run(sql1, ["2020-10-10"]);
  sql1 = 'insert into sales (amount, dat1) values(5.9, "2000-11-05")';
  r = await sqlite.run(sql1);
  for (let obj of salesArr) {
    sql1 = "insert into sales (amount, dat1) values(?, ?)";
    r = await sqlite.run(sql1, [obj.amount, obj.date]);
  }
  //fillDb(salesArr, sqlite, dbFile);
  await sqlite.close();
}

function csvToArray(txt, propNames) {
  let strArray = txt.split("\r\n");
  strArray = strArray.filter((e) => e.trim() !== "");
  let objArray = strArray.map((el) => lineToObj(el, propNames));
  return objArray;
}
function lineToObj(str, propNames) {
  let obj = {};
  let arrValues = str.split(",").map((e) => e.trim());
  for (let i = 0; i < arrValues.length; i++) {
    const v = arrValues[i];
    obj[propNames[i]] = v;
  }

  return obj;
}
async function fillDb(salesArr, sqlite, dbFile) {
  // let i = await sqlite.run(
  //   "CREATE TABLE sales1(ID integer NOT NULL PRIMARY KEY, amount REAL, date text (20))"
  // );

  for (let j of salesArr) {
    var entry = `${j.amount},"${j.date}"`;
    //var entry = `${j.id},${j.amount},"${j.date}"`;
    var sql = "INSERT INTO sales1(amount, date) VALUES (" + entry + ")";
    const r = await sqlite.run(sql);
    await sqlite.close();
    await sqlite.open(dbFile);
    if (r) console.log("Inserted.");
  }
}

async function addAllrecords(salesArr, sqlite) {
  let i = await sqlite.run(
    "CREATE TABLE sales(ID integer NOT NULL PRIMARY KEY, amount REAL, date text (20))"
  );

  for (let j of salesArr) {
    var entry = `${j.amount},'${j.date}'`;

    var sql = "INSERT INTO sales(amount, date) VALUES (" + entry + ")";
    const r = await sqlite.run(sql);
    if (r) console.log("Inserted.");
  }
}
