const sqlite3 = require("sqlite3").verbose();

async function mainApp() {
  const db = new sqlite3.Database(
    "./test2.db",
    sqlite3.OPEN_READWRITE,
    (err) => {
      if (err) return console.error(err.message);
    }
  );

  let sql;
  sql = `CREATE TABLE users (id INTEGER PRIMARY KEY, first_name, last_name, username, password, email)`;
  //await db.run(sql);
  //db.run("drop table users")
  sql = "select * from users";
  let resArr = await db_all(db, sql);
  db.close();
  console.log(resArr);
}

async function db_all(db, query) {
  return new Promise(function (resolve, reject) {
    db.all(query, function (err, rows) {
      if (err) {
        return reject(err);
      }
      resolve(rows);
    });
  });
}

mainApp();
