const fs = require("fs");
let data = fs.readFileSync("./Files/test1.txt", "utf8"); //

data = "hello***world***!***";

let linesArray = data.split("***");
let i = 1;
for (line of linesArray) {
  let line1 = `${i++}`.padStart(2, "0") + `: ${line}`;
  console.log(line1);
}
console.log(linesArray);

let listOfPeople = csvToObjArray(data);

function csvToObjArray(data) {
  // get list of lines
  // get list of props
  // loop for
  //      get list of values
  // createObj(listOfProps, listOfValues)
}
