let transactions = [
  { date: "2020/01/12", good: "Table", qty: 1, price: 50 },
  { date: "2020/01/13", good: "Chair", qty: 2, price: 40 },
  { date: "2020/01/12", good: "Chair", qty: 5, price: 43 },
  { date: "2020/01/14", good: "Table", qty: 2, price: 51 },
  { date: "2020/01/13", good: "Book", qty: 1, price: 10 },
  { date: "2021/02/17", good: "Book", qty: 100, price: 12 },
];

class GroupByMaster {
  data = null;
  constructor() {}
  setData(arr) {
    this.data = arr;
  }
  sum(func) {
    //
    let sum2 = 0;
    for (let tr1 of this.data) {
      sum2 += func(tr1);
    }
    return sum2;
  }
  max(func) {
    let res = 0;
    let resmax = res;
    for (let tr1 of this.data) {
      res = func(tr1);
      if (res > resmax) {
        resmax = res;
      }
    }
    return resmax;
  }
  groupBy(propName) {
    this.groupKey = propName;
    this.groupObj = {};
    for (let i = 0; i < this.data.length; i++) {
      let obj = this.data[i];
      let propVal = obj[propName];
      if (this.groupObj[propVal]) {
        this.groupObj[propVal].push(obj);
      } else this.groupObj[propVal] = [obj];
    }
  }
  aggregate() {
    // argI aggrFunc(Fld)
    // aggrFuncs: Count(), Min(fld), Max(fld), Sum(fld)
    // return {} count() transCount", "sum(qty) Quantity
    // let arrAggrInfo = [
    //   { objProp: "transCount", aggrFunc: "Count", expr: "" },
    //   { objProp: "Quantity", aggrFunc: "Sum", expr: "qty" }
    // ];
    let arrAggrInfo = this.getAggrInfo(arguments);
    let arr = [];
    for (let grpKey in this.groupObj) {
      let obj = {};
      obj[this.groupKey] = grpKey;
      this.calcAggrInfo(obj, arrAggrInfo, this.groupObj[grpKey]);
      arr.push(obj);
    }
    return arr;
  }
  calcAggrInfo(grpObj, arrAggrInfo, arr) {
    let aggrResArr = arrAggrInfo.forEach((el) => {
      if (el.aggrFunc == "count") grpObj[el.objProp] = arr.length;
      if (el.aggrFunc == "sum") {
        const propToSum = el.expr;
        const valsToSum = arr.map((itm) => itm[propToSum]);
        grpObj[el.objProp] = valsToSum.reduce((prev, next) => prev + next);
      }
      if (el.aggrFunc == "min") {
        const vals = arr.map((itm) => itm[el.expr]);
        grpObj[el.objProp] = Math.min(...vals);
      }
      if (el.aggrFunc == "max") {
        const vals = arr.map((itm) => itm[el.expr]);
        grpObj[el.objProp] = Math.min(...vals);
      }
    });
  }
  getAggrInfo(argArr) {
    let arr = [];
    let regPtrn = /(count|min|max|sum)[(](\w*)[)]\s+(\w+)/;
    for (let i = 0; i < argArr.length; i++) {
      let arg = argArr[i];
      //   { objProp: "Quantity", aggrFunc: "Sum", expr: "qty" }
      const matchObj = regPtrn.exec(arg);
      const aggrFunc = matchObj[1];
      const expr = matchObj[2];
      const objProp = matchObj[3];
      const obj = { objProp: objProp, aggrFunc: aggrFunc, expr: expr };
      arr.push(obj);
    }
    return arr;
  }
}

let gpm = new GroupByMaster();
gpm.setData(transactions);
gpm.groupBy("good");
gpm.aggregate("count() transCount", "sum(qty) Quantity");
// {good: "Table", transCount: 2, Quantity: 12}
let sum1 = gpm.sum((tr) => tr.qty * tr.price);
let maxSale = gpm.max((tr) => tr.qty * tr.price);
let arrTrCountReport = gpm.groupBy("good", "good Product", "Count SalesCount");
let arrSales = gpm.groupBy("good", "good as Product", [
  "Sum Amount",
  (tr) => tr.qty * tr.price,
]);
