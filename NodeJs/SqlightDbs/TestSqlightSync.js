const fs = require("fs");
const sqlite = require("./sqlightSyncHelper");

let dbFile = "./test.db";
console.log(__dirname);
async function mainApp() {
  console.log(await sqlite.open(dbFile));

  // Adds a table

  let doesExist = tableExists("users", sqlite);
  var r = await sqlite.run(
    "CREATE TABLE IF NOT EXISTS users(ID integer NOT NULL PRIMARY KEY, name text, city text)"
  );
  if (r) console.log("Table created");

  // Fills the table

  let users = {
    Naomi: "chicago",
    Julia: "Frisco",
    Amy: "New York",
    Scarlett: "Austin",
    Amy: "Seattle",
  };

  var id = 1;
  for (var x in users) {
    var entry = `'${id}','${x}','${users[x]}'`;
    var sql = "INSERT INTO users(ID, name, city) VALUES (" + entry + ")";
    r = await sqlite.run(sql);
    if (r) console.log("Inserted.");
    id++;
  }

  // Starting a new cycle to access the data

  await sqlite.close();
  await sqlite.open(dbFile);

  console.log("Select one user:");

  var sql = "SELECT ID, name, city FROM users WHERE name='Naomi'";
  sql = `SELECT name FROM sqlite_master WHERE type='table' and name='users'`;
  r = await sqlite.get(sql);
  console.log("Read:", r.ID, r.name, r.city);

  console.log("Get all users:");

  sql = "SELECT * FROM users";
  r = await sqlite.all(sql, []);
  r.forEach(function (row) {
    console.log("Read:", row.ID, row.name, row.city);
  });

  console.log("Get some users:");

  sql = "SELECT * FROM users WHERE name=?";
  r = await sqlite.all(sql, ["Amy"]);
  r.forEach(function (row) {
    console.log("Read:", row.ID, row.name, row.city);
  });

  console.log("One by one:");

  sql = "SELECT * FROM users";
  r = await sqlite.each(sql, [], function (row) {
    console.log("Read:", row.ID, row.name, row.city);
  });

  if (r) console.log("Done.");

  sqlite.close();
}

async function tableExists(tblName, sqli) {
  let sql = `SELECT * FROM sqlite_master WHERE type='table'`; // AND name=?`;
  sql = `SELECT name FROM sqlite_master WHERE type='table' and name='users'`;
  var r = await sqli.get(sql); //, tblName);

  return r != null;
}

try {
  fs.unlinkSync(dbFile);
} catch (e) {}

mainApp();
