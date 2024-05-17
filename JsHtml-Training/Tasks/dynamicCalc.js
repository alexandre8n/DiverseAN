// dynamic calculation

const arrWithExtractedVars = extractVariableNames(expression);
const replacedExpression = replaceVariables(expression, "this.");
console.log("Replaced expression:", replacedExpression); //this.a*(this.b+this.c1)
var calcHelper = Function("return " + replacedExpression);
let x1 = calcHelper.call(obj);
let x2 = calcHelper.apply(obj);
let funcCalcExp = calcHelper.bind(obj);
let x3 = funcCalcExp();

let expr = "qty*price*0.9";
var calcHelper3 = Function("return this.qty*this.price");
var calcHelper1 = Function("qty", "price", "return " + expr);
var calcHelper2 = Function("tr", "return tr.qty*tr.price");
let tr = transactions[0];
x1 = calcHelper1(tr.qty, tr.price);
x2 = calcHelper2(tr);
x3 = calcHelper3.call(tr);
//let x1 = calcHelper1.call({}, [tr.qty, tr.price]);
console.log(x1);
const obj = { a: 10, b: 5, c1: 3 };
const expression = "a*(b+c1)";
