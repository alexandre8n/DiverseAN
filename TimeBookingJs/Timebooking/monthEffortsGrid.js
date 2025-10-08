// const dn = weekDayEn("2025-01-20");
// var ar2 = [{ name: "a" }, { name: "z" }, { name: "b" }, { name: "x" },
// { name: "c" }].sortBy("name");

//let res = maxHaving([-1, 2, 3, 1, -3]);

const parentDivId = "parentDiv";
const txtAddTbRec = "Add time-booking record";
const txtEditTbRec = "Update time-booking record";
const currentState = {
  date: null, // date of currently edited
  daySummaryRow: null, // row in calender-like grid, where details btn is clicked
  bookingRowTableBody: null, // table body, having rows of selected day
  bookingRow: null, // effort-row of currently edited tb-record, it has id, date...
  isAdd: null,
  btnEditDetailsDone: null,
  btnAddNewRec4EditDetails: null,
  plannedTransactions: null, // delele/add/update{ type: trType, record: tbRec }
  _recordsChanged: false, // records of the day bookings changed. (add, upd, del)
};
// container of the calender-like table
const containerDiv = document.getElementById(parentDivId);

// the form used to add/edit timebooking record
const editTbrForm = document.querySelector(".form-component");

// buttons to show details: day time-booking records
let btnsDetails = null;

// initiate component to edit date range
const btnDateRangeChange = dtRangeInit(
  "DateRangeId",
  dtFrom(),
  dtTo(),
  onDtRangeChanged
);

let dateRangesMonthly = BuildDateRangesMonthly(dtFrom(), dtTo());

// here we get all records that we have to process.
// group them by day
// date, effort
monthlyNavigate(1);

//--- states check functions:
function isEditedTimeBookingRec() {
  if (editTbrForm && editTbrForm.style.display == "grid") return true;
  return false;
}
function areDayRecordsChanged() {
  return currentState._recordsChanged;
}
function setDayRecordChange(hasChanged) {
  // enable/disable some buttons
  btnDateRangeChange.disabled = hasChanged;
  btnsDetails.forEach((x) => (x.disabled = hasChanged));
  currentState._recordsChanged = hasChanged;
}

function isRecordChanged() {
  if (!isEditedTimeBookingRec()) return false;
  const dataRow = prepareDataRowFromControls();
  if (isDataRowChanged(dataRow)) return true;
  return false;
}
//--- end of state functions

function prepareDataRowFromControls() {
  const effortInp = document.getElementById("effort");
  const effort = effortInp.value;
  if (isNaN(effort.trim())) {
    showTooltip(effortInp, "tooltip", "Invalid number entered!");
    return null;
  }
  let selectElem = document.getElementsByName("cmbProj")[0];
  const proj =
    selectElem.selectedIndex < 0
      ? ""
      : selectElem.options[selectElem.selectedIndex].text;
  const task = document.getElementById("task").value;
  const cmnt = document.getElementById("comment").value;
  const id = currentState.isAdd ? generateUUIDv4() : currentState.bookingRow.id;
  const dataRow = {
    id: id,
    user: user,
    date: currentState.date,
    project: proj,
    task: task,
    effort: Number(effort),
    comment: cmnt,
  };
  return dataRow;
}

function showCalendarPage(pageNo) {
  const dateRangeIdx = pageNo - 1;
  const dtFr = dateRangesMonthly[dateRangeIdx].from;
  const dtTo = dateRangesMonthly[dateRangeIdx].to;
  // filter by date range
  let doRecs = reloadTbRecords(dtFr, dtTo);
  doRecs = doRecs.sortBy("date");
  let datesOverviewRecs = groupBookingsByDay(doRecs);
  showDaysRecords(dtFr, dtTo, datesOverviewRecs);
  btnsDetails = containerDiv.querySelectorAll(".btnDayEffDetailsCls");
  btnsDetails.forEach((x) => x.addEventListener("click", showDetails));
}

function onDtRangeChanged(dtFr, dtTo) {
  dateRangesMonthly = BuildDateRangesMonthly(dtFr, dtTo);
  monthlyNavigate(1);
}

function getGridInfo(daysCount) {
  const colCount = daysCount < 11 ? 1 : daysCount < 21 ? 2 : 3;
  const rowCount = daysCount < 11 ? daysCount : daysCount < 31 ? 10 : 11;
  const gridInfo = {
    colCount: colCount,
    rowCount: rowCount,
    rowCountBase: 10,
  };
  return gridInfo;
}
function showDaysRecords(dateFrom, dateTo, dayRecs) {
  // dateFrom, dateTo (Input) are in range of month!!
  let monthRecs = prepareCalendarRecords(dateFrom, dateTo, dayRecs);
  publishMonthTable(monthRecs);
}

// todo: remove it/
function showDaysRecords2(dateFrom, dateTo, dayRecs) {
  // dateFrom, dateTo (Input) are in range of month!!
  let doRecs = prepareCalendarRecords(dateFrom, dateTo, dayRecs);
  let { colCount, rowCount } = getGridInfo(doRecs.length);

  let idxFr = 0;
  let idxTo = 0;
  for (; idxFr < colCount; idxFr = idxTo + 1) {
    // show month
    [idxFr, idxTo] = getTbRecByMonth(doRecs, idxFr);
    if (idxFr < 0) break;
    const monthRecs = doRecs.slice(idxFr, idxTo + 1);
    publishMonthTable(monthRecs);
    idxFr = idxTo + 1;
  }
}

function prepareCalendarRecords(dateFrom, dateTo, dayRecs) {
  const daysCount = dateDiffInDays(dateFrom, dateTo);
  let map1 = new Map();
  for (var i = 0; i <= daysCount; ++i) {
    const dtStr = datePlusDaysStr(dateFrom, i);
    map1.set(dtStr, { date: dtStr, effort: 0 });
  }
  for (let rec of dayRecs) {
    map1.set(rec.date, rec);
  }
  return Array.from(map1.values());
}

function getTbRecByMonth(recs, idxFr) {
  if (recs.length <= idxFr) return [-1, -1];
  const strDt = recs[idxFr].date;
  const yearMonth = strDt.slice(0, 7);
  for (let i = idxFr; i < recs.length; i++) {
    let curRecDate = recs[i].date.slice(0, 7);
    if (curRecDate != yearMonth) return [idxFr, i - 1];
  }
  return [idxFr, recs.length - 1];
}

function publishMonthTable(calendMonthRecs) {
  removeAllChildNodes(document.getElementById(parentDivId));
  // publish days rec in 3 cols
  const daysCount = calendMonthRecs.length;
  let { colCount, rowCount } = getGridInfo(daysCount);
  let containerDiv = createGridAndHeader(rowCount, colCount);
  for (let i = 0; i < colCount; i++) {
    addDataRows(i + 1, calendMonthRecs, containerDiv);
  }
  parentDiv.appendChild(containerDiv);
}

function addDataRows(col, recs, containerDiv) {
  // records to output, depending on col
  // 1st col from 0 to GR-1  (GR-gridRowsBase = 10) 0..9
  // 2nd col from GR till 2*GR-1, 10..19
  // 3rd col from 2*GR till the last rec. 20..lastRec (31 max)
  let { colCount, rowCount, rowCountBase } = getGridInfo(recs.length);
  let rowTempl = document.getElementById("tbOvRwTemplate");
  let rowCont = rowTempl.content.cloneNode(true);
  let rowDivToCopy = rowCont.querySelector(".tbOverviewRow");
  let gridRow = 2;
  const GR = rowCountBase;
  const iRecRow1 = (col - 1) * GR;
  const iRecRow2 = col < 3 ? col * GR - 1 : recs.length - 1;

  for (let i = iRecRow1; i <= iRecRow2 && i < recs.length; i++) {
    let rowDiv = rowDivToCopy.cloneNode(true);
    rowDiv.style.setProperty("grid-column", col);
    rowDiv.style.setProperty("grid-row", gridRow);
    gridRow++;
    const rec = recs[i];
    let childDivs = rowDiv.querySelectorAll("div");
    childDivs[0].textContent = rec.date;
    childDivs[1].textContent = weekDayEn(rec.date);
    childDivs[2].textContent = rec.effort.toString();
    containerDiv.appendChild(rowDiv);
  }
}

function createGridAndHeader(rowCount, colCount) {
  const colRepeat = `repeat(${colCount}, 1fr)`;
  const rowRepeat = `repeat(${rowCount}, 22px)`;
  let temp1 = document.getElementById("gridContTemplate");
  let aClone = temp1.content.cloneNode(true);
  const containerDiv = aClone.children[0];
  containerDiv.style.setProperty("grid-template-columns", colRepeat);
  containerDiv.style.setProperty("grid-template-rows", rowRepeat + 1);
  for (let iCol = 1; iCol <= colCount; iCol++) {
    let headRow = crtTblColHeader(iCol);
    containerDiv.appendChild(headRow);
  }
  return containerDiv;
}

function crtTblColHeader(iCol) {
  let temp2 = document.getElementById("tbTblHeaderTemplate");
  let headerCont = temp2.content.cloneNode(true);
  let tblHeaderDiv = headerCont.querySelector(".tbOverviewHeader");
  let row1 = tblHeaderDiv.cloneNode(true);
  row1.style.setProperty("grid-row", "1");
  row1.style.setProperty("grid-col", iCol.toString());
  return row1;
}

function groupBookingsByDay(bookings) {
  // from rec: id,project,task,comment,date,effort
  // to rec: date, weekDay, effort
  // date, sum(effort) as effort
  // group by date
  let tasksMap = new Map();
  for (let bookRec of bookings) {
    if (tasksMap.has(bookRec.date)) {
      const dateAgr = tasksMap.get(bookRec.date);
      dateAgr.effort += bookRec.effort;
      continue;
    }
    const aggregBookRec = { date: bookRec.date, effort: bookRec.effort };
    tasksMap.set(bookRec.date, aggregBookRec);
  }
  const tasks = [];
  tasksMap
    .values()
    .forEach((x) => tasks.push({ date: x.date, effort: x.effort }));
  return tasks;
}

function showDetails(event) {
  const btnDetailsRow = event.currentTarget.parentNode.parentNode;
  currentState.daySummaryRow = btnDetailsRow;
  const date = btnDetailsRow.children[0].innerText;
  currentState.date = date;
  const totalDayEffort = btnDetailsRow.children[2].innerText;
  const dayRecs = loadDayBookings(date);
  //if (dayRecs.length == 0) return;
  const tblTemplate = document.getElementById("tasksListTemplate");

  // Get the container where the form components will be inserted
  const tblContainer = document.getElementById("listOfDayTasks");
  const tblInCont = tblContainer.querySelector(".effortTableContainer");
  if (tblInCont) tblInCont.remove();
  // Clone the template content (deep clone)
  const tblEfforts = tblTemplate.content.cloneNode(true);
  addEffortRows(tblEfforts, dayRecs);

  // update the effort-total:

  const totEffSpan = tblEfforts.querySelector("span[data-effort]");
  totEffSpan.innerHTML = totalDayEffort;
  const daySpan = tblEfforts.querySelector("span[data-day]");
  daySpan.innerHTML = date;

  // add on click processing for new and done
  const allBtns = tblEfforts.querySelectorAll(".dayEffortHeader button");
  const btnNew = allBtns[0];
  const btnDone = allBtns[1];
  const btnCancel = allBtns[2];
  btnNew.onclick = addTblRowClick;
  btnDone.onclick = doneDayRecordsChange;
  btnCancel.onclick = cancelDayRecordsChange;
  currentState.btnEditDetailsDone = btnDone;
  currentState.btnAddNewRec4EditDetails = btnNew;

  // Append the cloned form to the container
  tblContainer.appendChild(tblEfforts);
}

function addEffortRows(tbl, recs) {
  let tb = tbl.querySelector("tbody");
  currentState.bookingRowTableBody = tb;
  for (let rec of recs) {
    addRowToDayEffortTable(tb, rec);
  }
}
function addRowToDayEffortTable(tblBodyElm, rec) {
  let newrow = tblBodyElm.insertRow(0);
  const cols = ["date", "project", "task", "effort"];
  for (let fld of cols) {
    let td = document.createElement("td");
    td.textContent = rec[fld];
    newrow.appendChild(td);
  }
  newrow.id = rec.id;
  let tdButtonEdi = createTdWithButton("Edit", editTblRowClick);
  let tdButtonDel = createTdWithButton("Delete", deleteTblRowClick);
  newrow.appendChild(tdButtonEdi);
  newrow.appendChild(tdButtonDel);
  //tb.appendChild(newrow)добавляє в кінець таблиців tbody
  return;
}
function updateRowInDayEffortTable(tblBodyElm, rec) {
  const tdElmsToModify = currentState.bookingRow.querySelectorAll("td");
  tdElmsToModify[1].innerHTML = rec.project;
  tdElmsToModify[2].innerHTML = rec.task;
  const effortChange = rec.effort - Number(tdElmsToModify[3].innerHTML);
  tdElmsToModify[3].innerHTML = rec.effort;
  return effortChange;
}
function createTdWithButton(btnTxt, btnOnclick) {
  let td = document.createElement("td");
  let tbtn = document.createElement("button");
  tbtn.innerHTML = btnTxt;
  tbtn.onclick = btnOnclick;
  td.appendChild(tbtn);
  return td;
}

//
function doneDayRecordsChange(event) {
  const tblContainer = document.querySelector(".effortTableContainer");
  //todo eff-span
  const totEfforts = Number(
    tblContainer.querySelector("span[data-effort]").innerText
  );
  proceedWithTransactions();
  tblContainer.remove();
  updateDayEffortInCalenderView(currentState.date, totEfforts);
  clearCurrentState();
}

function proceedWithTransactions() {
  if (!currentState.plannedTransactions) return;
  if (currentState.plannedTransactions.count == 0) return;
  for (const tr of currentState.plannedTransactions) {
    const rec = tr.record;
    if (tr.type == "add") prcAddTbRecTransaction(rec);
    if (tr.type == "update") prcUpdTbRecTransaction(rec);
    if (tr.type == "delete") prcDelTbRecTransaction(rec);
  }
}
function prcAddTbRecTransaction(rec) {
  const effortChange = rec.effort;
  addTbRecord(rec);
}

function prcUpdTbRecTransaction(rec) {
  const effortChange = 0;
  updateTbRecord(rec);
}

function prcDelTbRecTransaction(rec) {
  const effortChange = -rec.effort;
  deleteTbRecord(rec);
}

function cancelDayRecordsChange(event) {
  if (isRecordChanged() || areDayRecordsChanged()) {
    var answer = window.confirm(
      "Record(s) has been changed!\nAre you sure you want to discard changes?"
    );
    if (!answer) return false;
  }
  const tblContainer = document.querySelector(".effortTableContainer");
  tblContainer.remove();
  clearTbrForm();
  editTbrForm.style.display = "none";
  clearCurrentState();
}

function clearTbrForm() {
  AssignTbRecValuesToEdit(null);
}

function clearCurrentState() {
  currentState.date = null;
  currentState.daySummaryRow = null;
  currentState.bookingRowTableBody = null;
  currentState.bookingRow = null;
  currentState.isAdd = null;
  currentState.plannedTransactions = null;
  currentState.btnEditDetailsDone = null;
  currentState.btnAddNewRec4EditDetails = null;
  setDayRecordChange(false);
}

function addTblRowClick(event) {
  editTbrForm.style.display = "grid";
  editTbrForm.scrollIntoView({ behavior: "smooth", block: "start" });
  currentState.isAdd = true;
  document.getElementById("tbRecordUpdateDone").innerHTML = txtAddTbRec;
  clearTbrForm();
  disableButtons4DetailsChanged(true);
}
function disableButtons4DetailsChanged(isToDisable) {
  currentState.btnEditDetailsDone.disabled = isToDisable;
  currentState.btnAddNewRec4EditDetails.disabled = isToDisable;
}
function cancelAddOrEditTbRec(isWarningNeeded) {
  if (isWarningNeeded && isRecordChanged()) {
    var answer = window.confirm(
      "Record has been changed!\nAre you sure you want to discard changes?"
    );
    if (!answer) return false;
  }
  const editRecForm = document.querySelector("#taskToEdit div");
  editTbrForm.style.display = "none";
  disableButtons4DetailsChanged(false);
  return true;
}

function addTblRowDone() {
  // called if add or edit is clicked
  const dataRow = prepareDataRowFromControls();
  if (!dataRow) return;
  if (currentState.isAdd) {
    //todo: save record to db - plan saving if postponed
    const tblBodyElm = currentState.bookingRowTableBody;
    addRowToDayEffortTable(tblBodyElm, dataRow);
    addTransactionToDoWhenDone("add", dataRow);
    updateEffortTotalsInDayBlock(currentState.date, dataRow.effort);
    setDayRecordChange(true);
  } else {
    const tblBodyElm = currentState.bookingRowTableBody;
    const effChange = updateRowInDayEffortTable(tblBodyElm, dataRow);
    //update totals
    addTransactionToDoWhenDone("update", dataRow);
    updateEffortTotalsInDayBlock(currentState.date, effChange);
    setDayRecordChange(true);
  }
  cancelAddOrEditTbRec(false); // remove edit row form, when done
  disableButtons4DetailsChanged(false);
}
function addTransactionToDoWhenDone(trType, rec) {
  //todo: check in trans. list
  let trRec = tryToGetTbRecFromTransactions(rec.id);
  if (trRec != null) {
    trRec.record = JSON.parse(JSON.stringify(rec));
    return;
  }
  if (!currentState.plannedTransactions) currentState.plannedTransactions = [];
  currentState.plannedTransactions.push({ type: trType, record: rec });
}
function editTblRowClick(event) {
  editTbrForm.style.display = "grid";
  currentState.bookingRow = event.currentTarget.parentElement.parentElement;
  editTbrForm.scrollIntoView({ behavior: "smooth", block: "start" });
  // todo: eff-span
  const tblContainer = document.querySelector(".effortTableContainer");
  const totEfforts = tblContainer.querySelector("span[data-effort]").innerText;
  const totEffortsVal = Number(totEfforts);
  currentState.isAdd = false;
  const btnDone = editTbrForm.querySelector("#tbRecordUpdateDone");
  btnDone.innerHTML = txtEditTbRec;
  // assign all values:
  AssignTbRecValuesToEdit(currentState.bookingRow);
  disableButtons4DetailsChanged(true);
}
function AssignTbRecValuesToEdit(rowElm) {
  // data are taked from last transaction, source record, or empty
  const rec = getRecByRowElm(rowElm.id);
  const listOfProjs = getProjectsNames();
  const projSelect = editTbrForm.querySelector("#project");
  addOptionsToSelect(projSelect, listOfProjs, rec.project);
  const taskInput = editTbrForm.querySelector("#task");
  taskInput.value = rec.task;
  const effortInput = editTbrForm.querySelector("#effort");
  effortInput.value = rec.effort;
  const commentInput = editTbrForm.querySelector("#comment");
  commentInput.innerText = rec.comment;
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

function deleteTblRowClick(event) {
  // Ask the user a Yes/No question
  const userResponse = window.confirm("Do you want to delete this record?");
  if (!userResponse) return;
  // User clicked "Yes" (OK)
  const btn = event.currentTarget;
  let row = btn.parentElement.parentElement;
  const id = parent.id;
  const date = row.children[0].innerHTML;
  const effort = row.children[3].innerHTML;
  addTransactionToDoWhenDone("delete", {
    id: id,
    date: date,
    effort: parseInt(effort),
  });
  row.remove();
  setDayRecordChange(true);
  updateEffortTotalsInDayBlock(date, -effort);
}

function updateEffortTotalsInDayBlock(date, effortChange) {
  const spanWithEff = document.querySelector(
    ".dayEffortHeader span[data-effort]"
  );
  const effOld = parseInt(spanWithEff.innerHTML);
  spanWithEff.innerHTML = effOld + effortChange;
}

function updateDayEffortInCalenderView(date, totalEffort) {
  const dateRec = findRecByDate(date);
  if (dateRec == null) return;
  const effBlk = dateRec.children[2];
  effBlk.innerHTML = totalEffort;
}

function findRecByDate(date) {
  const elms = containerDiv.childNodes[0];
  for (const elm of elms.childNodes) {
    const elm0 = elm.children[0];
    if (elm0.innerHTML == "Date") continue;
    const datVal = elm0.innerHTML;
    if (datVal == date) return elm;
  }
  return null;
}

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
    showTooltip(input, "This project already exists");
    return false;
  } else if (inpVal == "") {
    showTooltip(input, "Project name is empty");
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

function monthlyNavigate(pageNo) {
  showCalendarPage(pageNo);
  if (dateRangesMonthly.count < 2) {
    hideNavigator();
    return;
  }
  showNavigator(pageNo, dateRangesMonthly.length, monthlyNavigate);
}
//--------------- mockup functions to remove when done
// todo: this function is to rewrite for get list of all projects
//+++++ done
//+ onAddNewProject for project
//+todo: show the date in day records table header
//+ todo: by adding tb-rec - just plan it
//+todo: by deleting tb-rec - just plan it
//+todo: by updateing tb-rec - just plan it
// ---corrections: process all actions below the effort-table
//+    btn: cancel in tbl with tb-records - just clear state
//+          cancelDayRecordsChange()
//+   btn: done in tbl with tb-records - do all planned transactions
//+         see: doneDayRecordsChange
//+   btn: add tb rec, see:   addTblRowClick()
//+    btn: edit in tb-row, see: editTblRowClick()
//+    btn: delete in tb-row, see: deleteTblRowClick()
//+    btn: done for add tb rec  see: addTblRowDone()
//+    btn: done for update tb rec, see: addTblRowDone()
//+    btn: cancel for add/update tb rec, see: cancelAddOrEditTbRec
//+    remove all transactions when done or canceled
//+        call clearCurrentState
//+todo:    proceedWithTransactions(), update efforts after transactions:
//+for delete  const effortChange = -effort;updateEffortTotals(date, effortChange);
//+ todo: load edit-booking-rec component
//+ todo: clear after loaded edit-booking-rec component
//+ todo2: AssignTbRecValuesToEdit, null to assign empty values
//+ todo: updateRowInDayEffortTable in the day grid of time-bookings -> update info
//+ todo: verify effort value
//+todo: states to check for day tasks Done or Cancel!
//        DayTableShown, No Changes: table list of day tasks
//        DayTableChanged: table list of day tasks
//        EditRecord: user opened edit-timebooking-record form
//+todo: disable buttons, that should not work
//        newRec, done(for edit details), show-details
//        disableButtons4DetailsChanged(true/false)
//        disable date-range button
//+ todo: verify that it works: getRecByRowElm
//+ todo: modify list of transactions if the record was already added/edited
//+ todo: isDataRowChanged should be modified too

// low-prio:
// todo: move methods to save read, test-gen-data, to sep. file.
// todo: eff-span, now it should have the date too...
//todo: if the details btn in calender clicked make it visually selected
//      but previousel selected make normal

// teach SashaG: callbacks, usage of vars for functions
// onDateRangeChange: build month ranges, and show page (n)
// BuildDateRangesMonthly
// monthlyNavigate(calenderPage)
// showCalendarPage
