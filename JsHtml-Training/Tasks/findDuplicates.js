
let strArray = ["q", "w", "w", "w", "e", "i", "i", "u", "r"];
let findDuplicates = (arr) =>
  arr.filter((item, index) => arr.indexOf(item) !== index);

console.log(findDuplicates(strArray)); // All duplicates
let arrRes = [...new Set(findDuplicates(strArray))]; 

var alreadySeen = {};
arrRes = [];
strArray.forEach(function (str) {
  if (alreadySeen[str]) arrRes.push();
  else alreadySeen[str] = true;
});

function findDuplicates1a(arr) {
    let ar1 = new Set();
    for(let i=arr.length-1; i>0; i--){
        if(arr.indexOf(arr[i])>= i) continue;
        ar1.add(arr[i]);
    }
  return [...new Set(ar1)];
}
function findDuplicates2a(arr) {
  let ar1 = arr.filter((item, index) => arr.indexOf(item) !== index);
  return [...new Set(ar1)];
}
