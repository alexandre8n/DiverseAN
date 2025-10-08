const { MongoClient } = require("mongodb");

const uri =
  "mongodb+srv://AG:OrynaIlia@cluster0.bfk6j.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
// important note: configure db to access from anywhere or specify correct list
// of ip addresses!!

async function main() {
  const cli = new MongoClient(uri);
  try {
    await cli.connect();
    console.log("connected to mongo...");
    await addObjects();
  } catch (e) {
    console.log(e);
  } finally {
    await cli.close();
  }
}

main().catch(console.error);

async function addObjects(params) {
  generate;
}
