// done: ok/cancel button in day tb change, using updateRowInCalendar()
// done: disable buttons when edit/add tb record, using disableElement()
// 1) all main: header, calendar, day list, paginator, new/edit/cancel buttons disableMainElements(true/false)
//     call it after addTblRowClick, editTblRowClick, deleteTblRowClick
//     but after delete, enable buttons, add, done, cancel
// 2) in add/edit tb record: done/cancel buttons disableEffTblAndCancelDoneButtons(true/false)
// done: introduce selection of corrected calendar row.
// done: add in header of editing componenet option: now we adding new record
//      or editing existing record, show some mark, that day info is changed

let dateRangesMonthly = [];
const currentState = {
  date: null, // date of currently edited
  daySummaryRow: null, // row in calender-like grid, where details btn is clicked
  dayEffortHeader: null, // header of day-effort table (Day, effort, 3 buttons)
  bookingRowTableBody: null, // table body, having rows of selected day
  bookingRow: null, // effort-row of currently edited tb-record, it has id, date...
  isAdd: null,
  btnEditDetailsDone: null,
  btnAddNewRec4EditDetails: null,
  plannedTransactions: null, // delele/add/update{ type: trType, record: tbRec }
  _recordsChanged: false, // records of the day bookings changed. (add, upd, del)
};

function initTbDRO() {
  let span = document.querySelector(".header-left h1 span");
  span.style.display = "inline-block";
  span.innerHTML = "(Date Range View)";
  dateRangeInit(
    "#tbDRO-div .DtRange",
    dateRange.from,
    dateRange.to,
    onDateRangeChanged
  );
  dateRangesMonthly = buildDateRangesMonthly(dateRange.from, dateRange.to);
  showCalendarPage(1);
}

function buildDateRangesMonthly(dtFr, dtTo) {
  let lastDate = getLastDayOfMonth(dtFr);
  lastDate = lastDate >= dtTo ? dtTo : lastDate;
  const dateIntervals = [{ from: dtFr, to: lastDate }];
  while (lastDate < dtTo) {
    let dtFrom = datePlusDays(lastDate, 1);
    lastDate = getLastDayOfMonth(dtFrom);
    lastDate = lastDate >= dtTo ? dtTo : lastDate;
    dateIntervals.push({ from: dtFrom, to: lastDate });
    if (lastDate >= dtTo) return dateIntervals;
  }
  return dateIntervals;
}

function showCalendarPage(page) {
  const parentDiv = document.getElementById("calendarId");
  removeAllChildNodes(parentDiv);
  let dt = dateRangesMonthly[page - 1];
  let recs = reloadTbRecords(dt.from, dt.to);
  let dayRecords = groupByDate(recs);
  dayRecords = prepareCalendarRecords(dayRecords, dt.from, dt.to);
  let { rowCount, colCount } = getGridInfo(dayRecords.length);
  let calendarDiv = createCalendarTable(rowCount, colCount, dayRecords);
  // add calendar in the parent div
  parentDiv.appendChild(calendarDiv);
  if (dateRangesMonthly.count < 2) {
    hideNavigator();
    return;
  }
  showNavigator(page, dateRangesMonthly.length, showCalendarPage);
  if (isDateInRange(currentState.date, dt.from, dt.to)) selectRowInCalendar();
}
function createCalendarTable(rowCount, colCount, dayRecords) {
  let t1 = document.getElementById("gridContTemplate");
  let clonet1 = t1.content.cloneNode(true);
  const calendarDiv = clonet1.children[0];
  let colRepeat = `repeat(${colCount}, 1fr)`;
  let rowRepeat = `repeat(${rowCount + 1}, 25px)`;
  calendarDiv.style.setProperty("grid-template-columns", colRepeat);
  calendarDiv.style.setProperty("grid-template-rows", rowRepeat);
  addHeadersToCalendar(colCount, calendarDiv);
  addDataToCalendar(rowCount, colCount, calendarDiv, dayRecords);
  return calendarDiv;
}

function addHeadersToCalendar(colCount, calendarDiv) {
  // load header from template
  // set headers to calendar div (1st line in grid)
  for (let iCol = 1; iCol <= colCount; iCol++) {
    let headRow = crtTblColHeader(iCol);
    calendarDiv.appendChild(headRow);
  }
}

function crtTblColHeader(iCol) {
  let temp2 = document.getElementById("tbTblHeaderTemplate");
  let headerCont = temp2.content.cloneNode(true);
  let tblHeaderDiv = headerCont.querySelector(".tbOverviewHeader");
  let row1 = tblHeaderDiv.cloneNode(true);
  row1.style.setProperty("grid-row", "1");
  row1.style.setProperty("grid-column", iCol.toString());
  return row1;
}

function addDataToCalendar(rowCount, colCount, calendarDiv, dayRecords) {
  // load data-cell from template
  // set real data and put them in corresponding grid cell
  let rowTempl = document.getElementById("tbOvRwTemplate");
  let rowCont = rowTempl.content.cloneNode(true);
  let rowDivToCopy = rowCont.querySelector(".tbOverviewRow");
  for (let i = 0; i < dayRecords.length; i++) {
    let rowDiv = rowDivToCopy.cloneNode(true);
    const rec = dayRecords[i];
    const iRow = i < 10 ? i + 2 : i < 20 ? i - 10 + 2 : i - 20 + 2;
    const iCol = i < 10 ? 1 : i < 20 ? 2 : 3;
    rowDiv.style.setProperty("grid-row", iRow.toString());
    rowDiv.style.setProperty("grid-column", iCol.toString());
    let childDivs = rowDiv.querySelectorAll("div");
    childDivs[0].textContent = rec.date;
    childDivs[1].textContent = weekDayEn(rec.date);
    childDivs[2].textContent = rec.effort.toString();
    calendarDiv.appendChild(rowDiv);
  }

  // buttons to show details: day time-booking records
  const btnsDetails = calendarDiv.querySelectorAll(".btnDayEffDetailsCls");
  btnsDetails.forEach((x) => x.addEventListener("click", showDetails));
}
function showDetails(event) {
  const btnDetailsRow = event.currentTarget.parentNode.parentNode;
  currentState.daySummaryRow = btnDetailsRow;
  showTableWithTbRecordsOfDay();
}

function showTableWithTbRecordsOfDay() {
  const btnDetailsRow = currentState.daySummaryRow;
  const date = btnDetailsRow.children[0].innerText;
  currentState.date = date;
  selectRowInCalendar();
  const totalDayEffort = btnDetailsRow.children[2].innerText;
  const dayRecs = loadDayBookings(date);
  // Get the container where the form components will be inserted
  const tblContainer = document.querySelector(".DayList");
  const tblInCont = tblContainer.querySelector(".effortTableContainer");
  if (tblInCont) tblInCont.remove();
  const tblTemplate = document.getElementById("tasksListTemplate");
  // Clone the template content (deep clone)
  const SelectedDayTblEffortsContainer = tblTemplate.content.cloneNode(true);
  addEffortRows(SelectedDayTblEffortsContainer, dayRecs);
  currentState.dayEffortHeader =
    SelectedDayTblEffortsContainer.querySelector(".dayEffortHeader");
  updateSpanValue(currentState.dayEffortHeader, "span[data-day]", date);
  updateSpanValue(
    currentState.dayEffortHeader,
    "span[data-effort]",
    totalDayEffort
  );

  // add on click processing for new and done
  const allBtns = SelectedDayTblEffortsContainer.querySelectorAll(
    ".dayEffortHeader button"
  );
  const btnNew = allBtns[0];
  const btnDone = allBtns[1];
  const btnCancel = allBtns[2];
  btnNew.onclick = addTblRowClick;
  btnDone.onclick = doneDayRecordsChange;
  btnCancel.onclick = cancelDayRecordsChange;
  currentState.btnEditDetailsDone = btnDone;
  currentState.btnAddNewRec4EditDetails = btnNew;
  // Append the cloned form to the container
  tblContainer.appendChild(SelectedDayTblEffortsContainer);
}

function addEffortRows(tbl, recs) {
  let tb = tbl.querySelector("tbody");
  currentState.bookingRowTableBody = tb;
  for (let rec of recs) {
    addRowToDayEffortTable(tb, rec);
  }
}

function editTblRowClick(event) {
  let parent = event.currentTarget.parentElement.parentElement;
  let rec = parent.tbRec;
  if (!rec) throw new Error("No record found in the clicked row");
  // open editor with record
  const tbEditDiv2 = loadTbRecEditDivFromTemplate2(
    "Editing time-booking record..."
  );
  const tbEditDivHead = tbEditDiv2[0];
  const tbEditDiv = tbEditDiv2[1];

  const tbEditParent = document.querySelector(".TimeBookingEdit");
  removeAllChildNodes(tbEditParent);
  tbEditParent.appendChild(tbEditDivHead);
  tbEditParent.appendChild(tbEditDiv);

  const onTbRecUpdateSubmitClick = function (rec) {
    addTbRecordToTransactionList(rec, "update");
    updateRowInDayEffortTable(currentState.bookingRowTableBody, rec);
    const total = sumTotalEffortOfDay(currentState.bookingRowTableBody);
    updateSpanValue(currentState.dayEffortHeader, "span[data-effort]", total);
    removeAllChildNodes(tbEditParent);
    enableEffTblAndCancelDoneButtons();
  };
  const settings = {
    container: tbEditDiv,
    record: rec,
    projListGetter: getProjectsNames,
    newProjectSaver: addProjectName,
    taskListGetter: loadTasks,
    onTbRecSubmitClick: onTbRecUpdateSubmitClick,
    onTbRecCancelClick: function () {
      removeAllChildNodes(tbEditParent);
      enableEffTblAndCancelDoneButtons();
    },
    date: currentState.date,
  };
  currentState.isAdd = false;
  tbEditDiv.tbEditor = new TBRecEditor(settings);
  tbEditDiv.scrollIntoView({ behavior: "smooth", block: "start" });
  disableMainElements(true);
}
function deleteTblRowClick(event) {
  let parent = event.currentTarget.parentElement.parentElement;
  let rec = parent.tbRec;
  if (!rec) throw new Error("No record found in the clicked row");
  addTbRecordToTransactionList(rec, "delete");
  parent.remove();
  incrementSpanValue(
    currentState.dayEffortHeader,
    "span[data-effort]",
    -rec.effort
  );
  disableMainElements(true);
  enableEffTblAndCancelDoneButtons();
}
function doneDayRecordsChange(event) {
  if (!currentState.plannedTransactions) {
    cancelDayRecordsChange();
    return;
  }
  for (let tr of currentState.plannedTransactions) {
    if (tr.type == "add") {
      addTbRecord(tr.record);
    } else if (tr.type == "delete") {
      deleteTbRecordById(tr.record.id);
    } else if (tr.type == "update") {
      updateTbRecord(tr.record);
    }
  }
  updateRowInCalendar(currentState.daySummaryRow);
  cancelDayRecordsChange(event);
}
function updateRowInCalendar(row) {
  const dayEfforts = calculateDateEfforts(currentState.date);
  row.children[2].textContent = dayEfforts.toString();
}

function calculateDateEfforts(date) {
  const recs = reloadTbRecords(date, date);
  let total = 0;
  for (let rec of recs) {
    total += rec.effort;
  }
  return total;
}

function cancelDayRecordsChange(event) {
  // remove table with day records
  const tblContainer = document.querySelector(".DayList");
  const tblInCont = tblContainer.querySelector(".effortTableContainer");
  if (tblInCont) tblInCont.remove();
  // remove edit section
  const tbEditParent = document.querySelector(".TimeBookingEdit");
  removeAllChildNodes(tbEditParent);
  resetCurrentState();
  disableMainElements(false);
}

function resetCurrentState() {
  //currentState.date = null;       // this date is used to show the currently selected calendar row
  currentState.daySummaryRow = null;
  currentState.dayEffortHeader = null;
  currentState.bookingRowTableBody = null;
  currentState.bookingRow = null;
  currentState.isAdd = null;
  currentState.btnEditDetailsDone = null;
  currentState.btnAddNewRec4EditDetails = null;
  currentState.plannedTransactions = null;
  currentState._recordsChanged = false;
}

function addTblRowClick(event) {
  disableMainElements(true);
  const tbEditDiv2 = loadTbRecEditDivFromTemplate2();
  const tbEditDivHead = tbEditDiv2[0];
  const tbEditDiv = tbEditDiv2[1];

  const tbEditParent = document.querySelector(".TimeBookingEdit");
  removeAllChildNodes(tbEditParent);
  tbEditParent.appendChild(tbEditDivHead);
  tbEditParent.appendChild(tbEditDiv);
  const onTbRecSubmiClick = function (rec) {
    addTbRecordToTransactionList(rec);
    addRowToDayEffortTable(currentState.bookingRowTableBody, rec);
    incrementSpanValue(
      currentState.dayEffortHeader,
      "span[data-effort]",
      rec.effort
    );
    removeAllChildNodes(tbEditParent);
    enableEffTblAndCancelDoneButtons();
  };
  const settings = {
    container: tbEditDiv,
    record: null,
    projListGetter: getProjectsNames,
    newProjectSaver: addProjectName,
    taskListGetter: loadTasks,
    onTbRecSubmitClick: onTbRecSubmiClick,
    onTbRecCancelClick: function () {
      removeAllChildNodes(tbEditParent);
      enableEffTblAndCancelDoneButtons();
    },
    date: currentState.date,
  };
  currentState.isAdd = true;
  tbEditDiv.tbEditor = new TBRecEditor(settings);
  tbEditDiv.scrollIntoView({ behavior: "smooth", block: "start" });
  return;
}

function addTbRecordToTransactionList(rec, trType = "add") {
  if (!rec.id) {
    rec.id = generateUUIDv4();
  }
  if (!currentState.plannedTransactions)
    currentState.plannedTransactions = new Array();
  if (trType == "add") {
    currentState.plannedTransactions.push({ record: rec, type: trType });
    return;
  }
  if (trType == "delete") {
    // if record is added and then deleted, remove it from the list
    const existingIndex = currentState.plannedTransactions.findIndex(
      (x) => x.record.id == rec.id
    );
    if (existingIndex !== -1) {
      currentState.plannedTransactions.splice(existingIndex, 1);
      return;
    }
    currentState.plannedTransactions.push({ record: rec, type: trType });
    return;
  }
  if (trType == "update") {
    const existingIndex = currentState.plannedTransactions.findIndex(
      (x) => x.record.id == rec.id
    );
    if (existingIndex === -1) {
      currentState.plannedTransactions.push({ record: rec, type: trType });
      return;
    }
    if (currentState.plannedTransactions[existingIndex].type == "add") {
      // if record is added and then updated, keep it as "add"
      currentState.plannedTransactions[existingIndex] = {
        record: rec,
        type: "add",
      };
      return;
    }
    currentState.plannedTransactions[existingIndex] = {
      record: rec,
      type: trType,
    };
    return;
  }
}

function selectTaskFromList(event) {
  onSelectTask(onTaskSelectedInDRO, null, loadTasks);
}
function onTaskSelectedInDRO(taskName) {
  const taskInput = document.querySelector('.tbRecEdit-Div input[name="task"]');
  taskInput.value = taskName;
}

function getRecByRowElm(recId) {
  if (!recId) {
    return { project: "", task: "", effort: 0, comment: "" };
  }
  let trRec = tryToGetTbRecFromTransactions(recId);
  if (!trRec) {
    return getBookingRecordById(recId);
  }
  return trRec.record;
}

function tryToGetTbRecFromTransactions(id) {
  const lst = currentState.plannedTransactions;
  if (!lst) return null;
  const trRec = lst.find((x) => x.record.id == id);
  return trRec;
}

function updateRowInDayEffortTable(tblBodyElm, rec) {
  // find row with rec.id, update its cells
  let rows = Array.from(tblBodyElm.rows);
  let rowToUpdate = rows.find((r) => r.tbRec && r.tbRec.id === rec.id);
  if (!rowToUpdate) return;
  update1RowInDayEffortTable(rowToUpdate, rec);
}
function update1RowInDayEffortTable(row, rec) {
  row.tbRec = rec;
  const cols = ["date", "project", "task", "effort"];
  for (let i = 0; i < cols.length; i++) {
    let cell = row.cells[i];
    let fieldName = cols[i];
    if (fieldName && rec[fieldName] !== undefined) {
      cell.textContent = rec[fieldName];
    }
  }
}
function sumTotalEffortOfDay(tblBodyElm) {
  let total = 0;
  let rows = Array.from(tblBodyElm.rows);
  for (let row of rows) {
    let effortCell = row.cells[3]; // effort is in the 4th column
    total += parseFloat(effortCell.textContent) || 0;
  }
  return total;
}

function addRowToDayEffortTable(tblBodyElm, rec) {
  let newrow = tblBodyElm.insertRow(0);
  const cols = ["date", "project", "task", "effort"];
  for (let fld of cols) {
    let td = document.createElement("td");
    td.textContent = rec[fld];
    newrow.appendChild(td);
  }

  newrow.tbRec = rec;
  let tdButtonEdi = createTdWithButton("Edit", editTblRowClick);
  let tdButtonDel = createTdWithButton("Delete", deleteTblRowClick);
  newrow.appendChild(tdButtonEdi);
  newrow.appendChild(tdButtonDel);
  //tb.appendChild(newrow)добавляє в кінець таблиців tbody
  return;
}
function createTdWithButton(btnTxt, btnOnclick) {
  let td = document.createElement("td");
  let tbtn = document.createElement("button");
  tbtn.innerHTML = btnTxt;
  tbtn.onclick = btnOnclick;
  td.appendChild(tbtn);
  return td;
}

function getGridInfo(arrSize) {
  if (arrSize <= 10) return { rowCount: arrSize, colCount: 1 };
  if (arrSize <= 20) return { rowCount: 10, colCount: 2 };
  return { rowCount: arrSize == 31 ? 11 : 10, colCount: 3 };
}

function prepareCalendarRecords(dayRecords, dtfr, dtto) {
  let strDt = dateToStdStr(dtfr);
  const strDtTo = dateToStdStr(dtto);
  let arrCalendar = [];
  let i = 0;

  while (strDt <= strDtTo) {
    if (i < dayRecords.length && strDt == dayRecords[i].date) {
      arrCalendar.push(dayRecords[i]);
      i++;
    } else {
      arrCalendar.push({ date: strDt, effort: 0 });
    }
    strDt = datePlusDaysStr(strDt, 1);
  }
  return arrCalendar;
}
function groupByDate(recs) {
  // {date:, effort:}
  let dateMap = new Map();
  recs.forEach((x) => {
    if (dateMap.has(x.date)) {
      let agrEffort = dateMap.get(x.date);
      agrEffort.effort += x.effort;
    } else {
      dateMap.set(x.date, { date: x.date, effort: x.effort });
    }
  });
  let arr = Array.from(dateMap, ([key, value]) => value);
  arr = arr.sortBy("date", "asc");
  return arr;
}

function onDateRangeChanged(dtFr, dtTo) {
  saveGlobalDateRange(dtFr, dtTo);
  dateRangesMonthly = buildDateRangesMonthly(dateRange.from, dateRange.to);
  showCalendarPage(1);
}

// Add new project section
function onAddNewProject(evt) {
  if (evt.target.value != "AddNew") return;
  const settings = {
    dialogClass: "addNewDlg",
    onCancel: onCancelNewProj,
    onDone: onAddNewPrjDone,
    placeholderMsg: "Specify project name ...",
    buttonText: "Done...",
  };
  addNewDlgOpen(settings);
}

function onAddNewPrjDone(input) {
  const inpVal = input.value.trim();
  const projectNames = getProjectsNames();
  if (projectNames.includes(inpVal)) {
    showTooltip(input, "tooltip", "This project already exists");
    return false;
  } else if (inpVal == "") {
    showTooltip(input, "tooltip", "Project name is empty");
    return false;
  }
  saveNewProject(inpVal);
  return true;
}
function onCancelNewProj() {
  const selectElement = document.getElementById("project");
  if (selectElement.selectedIndex == 0) selectElement.selectedIndex = -1;
  return true;
}

function saveNewProject(projName) {
  if (!addProjectName(projName)) {
    return false;
  }
  updateProj(projName);
}
function updateProj(projName) {
  let selectElement = document.getElementById("project");
  let opt = document.createElement("option");
  opt.value = selectElement.options.count + 1; // some uniq value
  opt.innerHTML = projName; // name of the project
  opt.selected = true; // option should be selected = shown in combobox as active
  selectElement.appendChild(opt); // add option to combobox
}

function disableMainElements(disable) {
  const header = document.querySelector("header");
  const dateRangeDiv = document.querySelector(".DtRange");
  const calendar = document.querySelector("#calendarId");
  const dayEffortHeader = document.querySelector(".dayEffortHeader");
  const paginator = document.querySelector(".pagination");
  //const newEditCancelButtons = document.querySelectorAll(".new-edit-cancel");

  // Disable/enable all main elements
  disableElement(header, disable);
  disableElement(dateRangeDiv, disable);
  disableElement(calendar, disable);
  if (dayEffortHeader) disableElement(dayEffortHeader, disable);
  disableElement(paginator, disable);
  if (currentState.bookingRowTableBody)
    disableElement(currentState.bookingRowTableBody, disable);
  // newEditCancelButtons.forEach((btn) => {
  //   btn.disabled = disable;
  // });
}
function enableEffTblAndCancelDoneButtons() {
  const dayEffortHeader = document.querySelector(".dayEffortHeader");
  disableElement(dayEffortHeader, false);
  disableElement(currentState.bookingRowTableBody, false);
}
function selectRowInCalendar() {
  if (!currentState.date) return;
  currentState.daySummaryRow = findRowInCalendarByDate(currentState.date);
  if (!currentState.daySummaryRow) return;
  const parentDiv = document.getElementById("calendarId");
  const selElm = currentState.daySummaryRow;
  selElm.style.backgroundColor = "blue";
  selElm.style.color = "white";
  // unselect other rows
  const allRows = parentDiv.querySelectorAll(".tbOverviewRow");
  allRows.forEach((r) => {
    if (r !== selElm) {
      r.style.backgroundColor = "";
      r.style.color = "";
    }
    return;
  });
}
function findRowInCalendarByDate(date) {
  const parentDiv = document.getElementById("calendarId");
  const allRows = parentDiv.querySelectorAll(".tbOverviewRow");
  for (let r of allRows) {
    if (r.children[0].innerText == date) return r;
  }
  return null;
}
