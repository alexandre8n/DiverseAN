const Parser = require("./parser.js");
// csv: item1,item2,item3, ...
let str = "id, name,    [phone], salary";

let str1 = "s = a * (b + c*a)";
let prs = new Parser();
let words = prs.getAllWords(str, " [],;^~");
console.log("words:\n", words);

prs.init(str, " [],;^~");
for (let wrdInfo = prs.nextWord(); wrdInfo != null; wrdInfo = prs.nextWord()) {
  console.log(wrdInfo);
}

prs.init(str1, " [],=*/+-()");
for (let wrdInfo = prs.nextWord(); wrdInfo != null; wrdInfo = prs.nextWord()) {
  console.log(wrdInfo);
}
