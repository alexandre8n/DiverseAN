const sqlite3 = require("sqlite3").verbose();

let dbPath = "./test2.db";
let db = new sqlite3.Database(dbPath, sqlite3.OPEN_READWRITE, (err) => {
  if (err) return console.error(err.message);
});

async function mainApp() {
  let sql;
  sql = "SELECT * FROM sales";
  let resArr = await db_all(sql);
  db.close();
  //   let sqlCr =
  //     "CREATE TABLE IF NOT EXISTS sales (id INTEGER PRIMARY KEY,amount REAL,dat1 TEXT (20)";
  //   await db.run(sqlCr);
  //   await db.close();
  await db_open(dbPath);
  sql = 'insert into sales (amount, dat1) values(2.8, "2020-11-13")';
  await db.run(sql);
  await db.close();

  await db_open(dbPath);
  sql = 'insert into sales (amount, dat1) values(3.9, "2021-11-14")';
  await db.run(sql);

  sql = "SELECT * FROM sales";
  resArr = await db_all(sql);
  await db.close();

  await db_open(dbPath);
  // check if the table name exists in db
  sql = "SELECT name FROM sqlite_master where name=?";
  let tables = await db_get(sql, ["sales"]);
  await db.close();
}

async function db_open(path) {
  return new Promise(function (resolve) {
    db = new sqlite3.Database(path, sqlite3.OPEN_READWRITE, function (err) {
      if (err) reject("Open error: " + err.message);
      else resolve(path + " opened");
    });
  });
}

async function db_all(query) {
  return new Promise(function (resolve, reject) {
    db.all(query, function (err, rows) {
      if (err) {
        return reject(err);
      }
      resolve(rows);
    });
  });
}
async function db_get(query, params) {
  return new Promise(function (resolve, reject) {
    db.get(query, params, function (err, row) {
      if (err) reject("Read error: " + err.message);
      else {
        resolve(row);
      }
    });
  });
}

mainApp();
