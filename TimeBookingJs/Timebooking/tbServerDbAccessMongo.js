// auth: https://www.youtube.com/watch?v=d_aJdcDq6AY
// Mongo:
// https://www.youtube.com/watch?v=LNvmI8a9jwY&t=975s
// web-simplified: https://www.youtube.com/watch?v=ofme2o29ngU
// use (AG,p:OrynaIlia)=>AG:OrynaIlia
// createCollection, showCollections;
// dropDatabase, insertOne, insertMany ([]), find, updateOne
// init project in vsCode: npm init -y, pagage.json should appear
// install mongo: npm i mongodb -g (if globally -g )
// if compas: mongodb+srv://<db_username>:<db_password>@cluster0.bfk6j.mongodb.net/
// findOne, find(), find({name: "Iv"}), find({}, {name:1, age:1, _id:0}) (only 2 fields)
// find({}, {_id:0}) // all fields wo id.
// find({name: {$neq: "Sally"}}, {_id:0}) // all fields wo id.
// other filters:  {name: {$in: ["Iv","Sally"]}},
// other filters:  {name: {$nin: ["Iv","Sally"]}}, // not in
// other filters:  {name: {$nin: ["Iv","Sally"]}}, // not in
// other filters:  {age: {$exists: true}}, // filed age should be
// other filters:  {name: "Bill", age: {$eq: 25}}, // filed age should be
// other filters:  {age: {$gt: 25, $lt: 40} }, //  25<age<40
// $and: [](it works by default if, ), $or: []
// $not: {someth...},
// $expr: {$gt: "$debit", "$balance"}
// countDocuments({age: {$lte: 40}})
// updateOne({_id:ObjectId("26...99"},{$set: {age:27, }})
// $rename, $unset, $push, $pull
// replaceOne({age: 30}, {name: "John"})
// deleteOne()

const utl1 = require("./utl1");
const { MongoClient, ServerApiVersion, ObjectId } = require("mongodb");
const uri_0 = "mongodb+srv://AG:OrynaIlia@cluster0.bfk6j.mongodb.net/";
//mongodb+srv://AG:OrynaIlia@cluster0.bfk6j.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0
const uri =
  "mongodb+srv://AG:OrynaIlia@cluster0.bfk6j.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
//mongodb+srv://AG:OrynaIlia@cluster0.bfk6j.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
let tbRec1 = {
  user: "AN",
  project: "Time booking",
  task: "New feature1",
  effort: 5,
  date: "2025-03-07",
  comment: "no comment",
};

const start = async () => {
  const client = new MongoClient(uri);
  try {
    await client.connect();
    console.log("connected to mongo...");
    await client.close();
    console.log("disconnected ...");
    tbRec1 = await insertTbRec(tbRec1);
    tbRec1.date = utl1.dateToStdStr(new Date());
    tbRec1.task = "update at " + tbRec1.date;
    let updRes = await updateTbRec(tbRec1);
    console.log("update Ok ...");
    let delRes = await deleteTbRec(tbRec1.id);
    console.log("delete Ok ...");
    const dtTo = utl1.datePlusDaysStr(new Date(), 1);
    const recs = await getUserTbRecords("AN", "2024-01-01", dtTo);
  } catch (e) {
    console.log(e);
  } finally {
    await client.close();
  }
};

//test1();
//start();
async function test1(params) {
  const res = await addUser({ name: "AG1", email: "ag1@ag.ag", pwd: "1" });
  const client = new MongoClient(uri);
  try {
    // Connect the client to the server	(optional starting in v4.7)
    await client.connect();
    // Send a ping to confirm a successful connection
    await client.db("timebook").command({ ping: 1 });
    console.log(
      "Pinged your deployment. You successfully connected to MongoDB!"
    );
  } finally {
    // Ensures that the client will close when you finish/error
    await client.close();
    console.log("connection to mongo is closed in run...");
  }
}
async function run() {
  const client = new MongoClient(uri);
  try {
    // Connect the client to the server	(optional starting in v4.7)
    await client.connect();
    // Send a ping to confirm a successful connection
    await client.db("admin").command({ ping: 1 });
    console.log(
      "Pinged your deployment. You successfully connected to MongoDB!"
    );
  } finally {
    // Ensures that the client will close when you finish/error
    await client.close();
    console.log("connection to mongo is closed in run...");
  }
}

async function getUsers(filter) {
  const client = new MongoClient(uri);
  try {
    await client.connect(); // Connect to the MongoDB server
    let database = client.db("timebook");
    const tbRecs = database.collection("users");
    let results = null;

    if (!filter) results = await tbRecs.find().toArray();
    else results = await tbRecs.find(filter).toArray();

    const stdResults = results.map((x) => recWithId(x));
    return stdResults;
  } catch (err) {
    console.error(`Something went wrong trying to find documents: ${err}\n`);
    return false;
  } finally {
    await client.close();
  }
  return null;
}

async function verifyLogin(usr) {
  const filter = { name: usr.name };
  const usrRes = await getUsers(filter);
  if (usrRes.length < 1) return null;
  const usrFound = usrRes[0];
  return { id: usrFound.id }; // todo: here should pwd be checked.!!
}
async function addUser(usrObj) {
  const res = await addObjToDb(usrObj, "timebook", "users");
  return res;
}

async function addObjToDb(rec, dbName, collectionName) {
  const client = new MongoClient(uri);
  await client.connect();
  let database = client.db(dbName);
  const dbCollection = database.collection(collectionName);
  let res = await dbCollection.insertOne(rec);
  let savedId = res.insertedId.toHexString();
  await client.close();
  rec.id = savedId;
  return rec;
}

async function insertTbRec(rec) {
  const res = await addObjToDb(usrObj, "timebook", "timeBookings");
  return res;
}

async function updateTbRec(rec) {
  const recordId = rec.id;
  let result = null;
  const client = new MongoClient(uri);
  try {
    await client.connect(); // Connect to the MongoDB server
    // Access the database and collection
    let database = client.db("timebook");
    const tbRecs = database.collection("timeBookings");
    // Prepare the update
    let id = new ObjectId(rec.id);
    const filter = { _id: id }; // Convert the string _id to ObjectId
    const updateDoc = { $set: tbRecWoId(rec) };

    // Perform the update
    result = await tbRecs.updateOne(filter, updateDoc);

    // Log the result
    if (result.matchedCount > 0) {
      console.log(`Successfully updated ${result.modifiedCount} document.`);
    } else {
      console.log("No document found with the specified _id.");
    }
  } catch (err) {
    console.error("Error updating the document:", err);
  } finally {
    await client.close(); // Close the connection
  }
  return result;
}

async function deleteTbRec(recordId) {
  //const deleteQuery = { name: { $in: ["elotes", "fried rice"] } };
  let result = null;
  const client = new MongoClient(uri);
  let id = new ObjectId(recordId);
  const filter = { _id: id }; // Convert the string _id to ObjectId
  try {
    await client.connect(); // Connect to the MongoDB server
    // Access the database and collection
    let database = client.db("timebook");
    const tbRecs = database.collection("timeBookings");
    const deleteResult = await tbRecs.deleteOne(filter);
    console.log(`Deleted ${deleteResult.deletedCount} documents\n`);
  } catch (err) {
    console.error(`Something went wrong trying to delete documents: ${err}\n`);
    return false;
  }
  return true;
}
async function getUserTbRecords(user, dateFr, dateTo) {
  const filter = {
    user: user,
    date: {
      $gte: dateFr, // date >= dateFr
      $lt: dateTo, // date < dateTo
    },
  };
  const client = new MongoClient(uri);
  try {
    await client.connect(); // Connect to the MongoDB server
    // Access the database and collection
    let database = client.db("timebook");
    const tbRecs = database.collection("timeBookings");
    const results = await tbRecs.find(filter).toArray();
    const stdResults = results.map((x) => recWithId(x));
    return stdResults;
  } catch (err) {
    console.error(`Something went wrong trying to find documents: ${err}\n`);
    return false;
  }
  return null;
}

// helpers
function recWoId(rec) {
  // skip only id
  const recClone = utl1.cloneObj(rec);
  delete recClone._id;
  return recClone;
}
function recWithId(rec) {
  let savedId = rec._id.toHexString();
  const resRec = recWoId(rec);
  resRec.id = savedId;
  return resRec;
}

module.exports = {
  getUsers,
  addUser,
  insertTbRec,
  updateTbRec,
  deleteTbRec,
  getUserTbRecords,
  verifyLogin,
  //disconnect,
};
