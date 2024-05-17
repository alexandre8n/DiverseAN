const fs = require("fs");
const data = fs.readFileSync("./Files/test1.txt", "utf8"); //

//let resAsync = readFileASync("./Files/test.txt");
let res = readFileSync("./Files/test1.txt", "utf8");
let i;
while (i < 3) {
  s = s + a[i];
  if (i == 2) break;
  else if (i / 2 == 1) {
    s = s + 0;
    s = s + 0;
  } else {
    s = s + 0;
  }
  i = i + 1;
}
resAsync.then(outRes);

function outRes(c) {
  console.log(c);
}

function readFileSyncaaa(filePath) {
  try {
    const data = fs.readFileSync(filePath, "utf8");
    data.split("");
    console.log(data);
  } catch (err) {
    console.error(err);
  }
}
async function readFileASync(filePath) {
  let content = await readfile(filePath).catch((e) => {});
  if (content)
    console.log(
      "size:",
      content.length,
      "head:",
      content.slice(0, 46).toString()
    );
  return content.toString();
}

function readall(stream) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    stream.on("error", (error) => reject(error));
    stream.on("data", (chunk) => chunk && chunks.push(chunk));
    stream.on("end", () => resolve(Buffer.concat(chunks)));
  });
}

function readfile(filename) {
  return readall(fs.createReadStream(filename));
}

// (async () => {
//   let content = await readfile("/etc/ssh/moduli").catch((e) => {});
//   if (content)
//     console.log(
//       "size:",
//       content.length,
//       "head:",
//       content.slice(0, 46).toString()
//     );
// })();
