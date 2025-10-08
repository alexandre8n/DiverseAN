// see jwt explanations in jwtExample.txt

const dbAccessor = require("./tbServerDbAccessMongo");
const jwt = require("jsonwebtoken");
const express = require("express");
const cors = require("cors");

const app = express();
const PORT = process.env.port || 3000;
const cryptoKey = "abc99";
//dbAccessor.connect("AG", "");
app.use(cors());

app.use(express.json());

// app.get, .post, .put, delete,

app.listen(PORT, () => {
  console.log(`tbServer started, port:${PORT}`);
});

app.get("/", (req, res) => {
  res.status(200).send(`tbServer is ready, port:${PORT}`);
});

app.get("/tb", (req, res) => {
  res.status(200).send({
    records: [{ id: 1 }, { id: 2 }],
  });
});

const timeBookings = [
  { id: 1, name: "tb1" },
  { id: 2, name: "tb2" },
  { id: 7, name: "tb7" },
];
app.get("/tb/:id", (req, res) => {
  const tbRec = timeBookings.find((x) => x.id === parseInt(req.params.id));
  if (!tbRec) res.status(404).send("The record is not found");
  res.send(tbRec);
});

// localhost:3000/api/posts/:2025/1?sortBy=name
app.get("api/posts/:year/:month", (req, res) => {
  res.send(req.query);
});

app.post("/tb/verifyUser", (req, res) => {
  // Extract token from the Authorization header
  const token = req.headers["authorization"]?.split(" ")[1];
  const decoded = jwt.verify(token, cryptoKey);
  const userId = decoded.userId;
  if (!userId) {
    return res.status(403).send("Token is expired, or some other problem");
  }
  // here you should check if token is not expired.
  res.send({ verification: true });
});

app.post("/tb/login", (req, res) => {
  if (!req.body.user) {
    res.status(400).send("user is required, should not be empty");
    return;
  }

  // expected format of user: {name:x,pwd: pwd}
  let usr = req.body.user;
  const usrInfo = dbAccessor.verifyLogin(usr).then((x) => {
    if (!x?.id) {
      res.status(400).send("Failed to login this user");
      return null;
    }
    const token = jwt.sign({ userId: x.id }, cryptoKey, {
      expiresIn: "1h",
    });
    res.send({ token: token });
    console.log(`user ${x.name} login Ok` + JSON.stringify(x));
  });
});

app.post("/tb/addUser", (req, res) => {
  if (!req.body.user) {
    res.status(400).send("user is required, should not be empty");
    return;
  }
  // expected format of user: {name:x,email:y,pwd:hash}
  let rec = req.body.user;
  const recRes = dbAccessor.addUser(rec).then((x) => {
    res.send(x);
    console.log("user record added:" + JSON.stringify(x));
  });
});

app.post("/tb/add", (req, res) => {
  if (!req.body.tbRecord) {
    res.status(400).send("name is required, should not be empty");
    return;
  }
  // generally recommended to use Joi (see npm joi, const Joi = required('joi'))
  // const schema = {name: Joi.string().min(1).required(),...};
  // const result=Joi.validate(req.body,schema);
  //if(result.error) res.status(400).send(result.error);
  let rec = req.body.tbRecord;
  timeBookings.push(rec);
  console.log("record added:" + JSON.stringify(rec));
  res.send(rec);
});

app.put("/tb/upd/:id", (req, res) => {
  const tbRec = timeBookings.find((x) => x.id === parseInt(req.params.id));
  if (!tbRec) return res.status(404).send("The record is not found");
  // validate here...
  // let {err} = validateTb(tbRec); if(err) res.status(400).send(err);

  // update tbRec
  tbRec.name = req.body.name;
  res.send(tbRec);
});

app.delete("/tb/upd/:id", (req, res) => {
  const tbRec = timeBookings.find((x) => x.id === parseInt(req.params.id));
  if (!tbRec) return res.status(404).send("The record is not found");
  const idx = timeBookings.indexOf(tbRec);
  timeBookings.splice(idx, 1);
  res.send(tbRec);
});

// authorization on server:
// const jwt = require("jsonwebtoken");

// // генерация токена после успешной авторизации
// const token = jwt.sign({ userId: user.id }, "секретный_ключ", {
//   expiresIn: "1h",
// });

// // проверка токена на защищённых маршрутах
// app.get("/protected", (req, res, next) => {
//   const token = req.headers["authorization"]?.split(" ")[1];

//   if (!token) return res.status(401).send("Токен не предоставлен.");

//   jwt.verify(token, "секретный_ключ", (err, decoded) => {
//     if (err) return res.status(403).send("Токен недействителен.");
//     req.userId = decoded.userId;
//     next();
//   });
// });

// todo: verification to every request to server
// todo: later on.... way to save token and send it in every request on frontend
// todo: for admin: add user/change password/delete user
// todo: add project/ delete project
// todo:
