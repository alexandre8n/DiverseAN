Array.prototype.sortBy = function (p, ascDesc) {
  // usage: arrObjs.sortBy("name", "asc"); (name is a property supporting <> operations)
  // or arrObjs.sortBy("name", "desc")
  const arr = this.slice(0);
  const aOrD = ascDesc && ascDesc.toLowerCase() == "desc" ? -1 : 1;
  return this.slice(0).sort(function (a, b) {
    return a[p] > b[p] ? aOrD : a[p] < b[p] ? -aOrD : 0;
  });
};
// group by, and sum, example of usage:
// let arr = [{date: "2025-01-01", effort: 5},{date: "2025-01-01", effort: 10},
//   { date: "2025-01-02", effort: 7 },{date: "2025-01-02", effort: 1 }];
// let res = arr.groupBy((x) => x.date)
//   .aggregate("effort", (tot, cur) => tot + cur);
Array.prototype.groupBy = function (keyFunction) {
  var groups = {};
  this.forEach(function (el) {
    var key = keyFunction(el);
    if (key in groups == false) {
      groups[key] = [];
    }
    groups[key].push(el);
  });
  const groupResult = Object.keys(groups).map(function (key) {
    return {
      key: key,
      values: groups[key],
    };
  });
  groupResult.aggregate = aggregateGroup;
  return groupResult;
};
function aggregateGroup(fld, aggrFunc, iniVal) {
  // aggrFunc(total, curVal, idx, arr);
  // example of usage: res = recs.groupBy(x=>x.date)
  // res = res.aggregate("effort", (t,cur)=>t+cur, 0);
  // or: res = recs.groupBy(x=>x.date).aggregate("effort", (t,cur)=>t+cur, iniVal)
  // this: array of objects, results of groupby...
  const ini = iniVal ? iniVal : 0;
  this.forEach((x) => {
    // x = {key: key, values: arr}
    const aggrRes = x.values.reduce(
      (total, v, i) => aggrFunc(total, v[fld], i),
      ini
    );
    x[fld] = aggrRes;
  });
  return this;
}

function generateUUIDv4() {
  //функція генератор уникальних id
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    let r = (Math.random() * 16) | 0;
    let v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function getRandomNumber(size) {
  return Math.floor(Math.random() * size);
}

// this function returns the number from "from" to "to";
function getRandomIntFromTo(from, to) {
  var size = to - from + 1;
  return from + getRandomNumber(size);
}
function datePlusDaysStr(dateStr, plusDays) {
  const dt1 = new Date(dateStr);
  dt1.setDate(dt1.getDate() + plusDays);
  return dateToStdStr(dt1);
}
function dateToStdStr(dateObjOrStr) {
  const dateobj = new Date(dateObjOrStr);
  var month = (dateobj.getMonth() + 1).toString().padStart(2, "0");
  var day = dateobj.getDate().toString().padStart(2, "0");
  var year = dateobj.getFullYear();
  let str = `${year}-${month}-${day}`;
  return str;
}

// returns random date as string in "YYYY-MM-DD" format from
// [dateBase+plusDays1, dateBase+plusDays2]
function getRandomDate(dateBaseInp, plusDays1, plusDays2) {
  const dateBase = new Date(dateBaseInp);
  let dateobj = new Date();
  let dateOffset = getRandomIntFromTo(plusDays1, plusDays2);
  dateobj.setDate(dateBase.getDate() + dateOffset);
  var month = (dateobj.getMonth() + 1).toString().padStart(2, "0");
  var day = dateobj.getDate().toString().padStart(2, "0");
  var year = dateobj.getFullYear();
  let str = `${year}-${month}-${day}`;
  return str;
}

function removeAllChildNodesById(parentId) {
  var node = document.getElementById(parentId);
  removeAllChildNodes(node);
}

function removeAllChildNodes(node) {
  while (node.hasChildNodes()) {
    node.removeChild(node.lastChild);
  }
}

function isDateInRange(dateToTest, left, right) {
  if (!dateToTest || dateToTest === "") return false;
  let dt = typeof dateToTest == "string" ? new Date(dateToTest) : dateToTest;
  let from = typeof left == "string" ? new Date(left) : left;
  let to = typeof right == "string" ? new Date(right) : right;
  let dtFr = new Date(from);
  dtFr.setHours(0, 0, 0, 0);
  let dtTo = new Date(to);
  dtTo.setDate(to.getDate() + 1);
  dtTo.setHours(0, 0, 0, 0);
  let res = dt >= dtFr && dt < dtTo;
  return res;
}

function addOptionsToSelect(selElm, optTexts, selectedTxt) {
  // add options to combo-box, avoid duplicates...
  let i = 1;
  const options = Array.from(selElm.options);
  const arrOptionsTxt = options.map((x) => x.text);
  const optTextsInSelect = new Set(arrOptionsTxt);
  for (let x of optTexts) {
    if (optTextsInSelect.has(x)) continue;
    let opt = document.createElement("option");
    opt.value = ++i;
    opt.innerHTML = x;
    selElm.appendChild(opt);
  }
  const optionsAfter = Array.from(selElm.options).map((x) => x.text);
  selElm.selectedIndex = optionsAfter.indexOf(selectedTxt);
}

function getSelectedText(selectElement) {
  // Check if there's a valid selection
  if (selectElement.selectedIndex === -1) return null;
  else {
    // Get the selected option text
    const selectedOptionText =
      selectElement.options[selectElement.selectedIndex].text;
    return selectedOptionText;
  }
}

function removeSelectOptions(selectElement) {
  var i,
    L = selectElement.options.length - 1;
  for (i = L; i >= 0; i--) {
    selectElement.remove(i);
  }
}
function maxSting(a, b) {
  return a > b ? a : b;
}
function cloneObj(obj) {
  const strJson = JSON.stringify(obj);
  let objCopy = JSON.parse(strJson);
  return objCopy;
}

function createBodyCover(viewNameCover) {
  const daName = `data-${viewNameCover}`;
  const dataAttrSelector = `[${daName}]`;
  var cover = document.querySelector(dataAttrSelector);
  if (cover) return cover;
  cover = document.createElement("div");
  cover.setAttribute(daName, "");
  cover.style.display = "none";
  cover.style.position = "fixed";
  cover.style.top = "0";
  cover.style.left = "0";
  cover.style.width = "100%";
  cover.style.height = "100%";
  cover.style.backgroundColor = "rgba(0, 0, 0, 0.2)";
  cover.style.zIndex = "2";
  document.body.appendChild(cover);
  return cover;
}

function showTooltip(ctl, tooltipCls, msg) {
  const tooltip = document.createElement("div");
  tooltip.innerHTML = msg;
  tooltip.classList.add(tooltipCls);
  const inputRect = ctl.getBoundingClientRect();
  tooltip.style.left = `${inputRect.left + window.scrollX}px`;
  tooltip.style.top = `${
    inputRect.top + window.scrollY - tooltip.offsetHeight + 18
  }px`;
  tooltip.style.zIndex = "10";
  tooltip.style.display = "block";
  document.body.appendChild(tooltip);
  // Hide the tooltip after 2 seconds
  setTimeout(function () {
    //tooltip.style.display = "none";
    tooltip.remove();
  }, 2000);
}
function getLastDayOfMonth(date) {
  // Get the current month and year from the provided date
  const year = date.getFullYear();
  const month = date.getMonth();

  // Set the date to the first day of the next month
  const firstDayNextMonth = new Date(year, month + 1, 1);

  // Subtract one day to get the last day of the current month
  const lastDay = new Date(firstDayNextMonth - 1);

  return lastDay;
}
function datePlusDays(dateStr, plusDays) {
  const dt1 = new Date(dateStr);
  dt1.setDate(dt1.getDate() + plusDays);
  return dt1;
}

function dateDiffInDays(dtFrom, dtTo) {
  var day_start = new Date(dtFrom);
  var day_end = new Date(dtTo);
  var total_days = (day_end - day_start) / (1000 * 60 * 60 * 24);
  return total_days;
}
function weekDayEn(dt) {
  const dt1 = new Date(dt);
  const weekdays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
  const dayName = weekdays[dt1.getDay()];
  return dayName;
}

function updateSpanValue(container, selector, newValue) {
  const span = container.querySelector(selector);
  if (span) span.innerHTML = newValue;
}
function incrementSpanValue(container, selector, incValue) {
  const span = container.querySelector(selector);
  if (span) {
    const currentValue = parseFloat(span.innerHTML) || 0;
    span.innerHTML = currentValue + incValue;
  }
}

function disableElement(ctl, disable) {
  ctl.style.pointerEvents = disable ? "none" : "auto";
  ctl.style.opacity = disable ? "0.5" : "1";
}

export default {
  generateUUIDv4,
  getRandomIntFromTo,
  datePlusDaysStr,
  dateToStdStr,
  getRandomDate,
  removeAllChildNodesById,
  isDateInRange,
  addOptionsToSelect,
  getSelectedText,
  removeSelectOptions,
  maxSting,
  cloneObj,
  createBodyCover,
  showTooltip,
  getLastDayOfMonth,
  datePlusDays,
  dateDiffInDays,
  weekDayEn,
  updateSpanValue,
  incrementSpanValue,
  disableElement,
};
