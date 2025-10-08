let spanDates = document.querySelectorAll("#DateRangeView .smallMargin span");
let dateRangeFrom = spanDates[0];
let dateRangeTo = spanDates[1];

if (dateRange.from === null && dateRange.to === null) {
  let dtRangeTo = new Date();
  let dtRangeFrom = new Date();
  dtRangeFrom.setDate(-100);
  saveGlobalDateRange(dtRangeFrom, dtRangeTo);
}

const selectProjElement = document.getElementById("Project");
if (selectProjElement.selectedIndex == 0) selectProjElement.selectedIndex = -1;

let rec = {
  id: "abc",
  user: "AH",
  date: "2024-01-01",
  project: "PRO1",
  task: "task1",
  effort: 5,
  comment: "Cool",
};

function initTbMain() {
  let span = document.querySelector(".header-left h1 span");
  span.style.display = "inline-block";
  span.innerHTML = "(Main)";

  let records = loadTasks();
  fillEffortTable(records);
  showMainTbInput();
  //addNewProjectsInCmb();
  setDateRanges();
}
function showMainTbInput() {
  const tbEditDiv2 = loadTbRecEditDivFromTemplate2();
  const tbEditDivHead = tbEditDiv2[0];
  const tbEditDiv = tbEditDiv2[1];
  const tbEditParent = document.querySelector(".div1-left");
  removeAllChildNodes(tbEditParent);
  tbEditParent.appendChild(tbEditDivHead);
  tbEditParent.appendChild(tbEditDiv);
  const settings = {
    container: tbEditDiv,
    record: null,
    projListGetter: getProjectsNames,
    newProjectSaver: addProjectName,
    taskListGetter: loadTasks,
    onTbRecSubmitClick: onTbRecSubmiClick,
    date: new Date(),
  };
  tbEditDiv.tbEditor = new TBRecEditor(settings);
}

function setDateRanges() {
  //dateRangeFrom.innerHTML = dateToStdStr(dateRange.from);
  //dateRangeTo.innerHTML = dateToStdStr(dateRange.to);
  dateRangeInit(
    ".div1-right",
    dateRange.from,
    dateRange.to,
    onDateRangeMainChanged
  );
}

function onDateRangeMainChanged(dtFr, dtTo) {
  saveGlobalDateRange(dtFr, dtTo);
  let records = reloadTbRecords(dateRange.from, dateRange.to);
  fillEffortTable(records);
}

function onTbRecSubmiClick(rec) {
  addTbRecord(rec);
  addRecord(rec);
}

function validateTbRec(rec) {
  if (isNaN(rec.effort) || Number(rec.effort) <= 0) return false;
  if (!rec.date || rec.date.trim() == "") return false;
  if (!rec.project) return false;
  rec.effort = Number(rec.effort);
  return true;
}

function addNewProjectsInCmb() {
  let cmbProj = document.getElementById("Project");
  let arrProj = getProjectsNames();
  addOptionsToSelect(cmbProj, arrProj, "");
}

function addRecord(rec) {
  let tb = document.getElementById("tblBodyId");
  //let newrow= document.createElement('tr')створює елемент tr й присваює в змінну
  let newrow = tb.insertRow(0);
  const cols = ["date", "project", "task", "effort"];
  for (let fld of cols) {
    let td = document.createElement("td");
    td.textContent = rec[fld];
    newrow.appendChild(td);
  }
  newrow.id = rec.id;
  let tdButton = createTdWithButton("Edit", editclick);
  let tdButton1 = createTdWithButton("Delete", deleteclick);
  newrow.appendChild(tdButton);
  newrow.appendChild(tdButton1);
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

function editclick(btn) {
  let parent = btn.srcElement.parentElement.parentElement;
  let rec = getBookingRecordById(parent.id);

  const tbEditDiv = loadTbRecEditDivFromTemplate();
  const modal2 = document.getElementById("tbRecEditDlg");
  const tbEditParent = modal2.querySelector("#tbEditDivID");
  removeAllChildNodes(tbEditParent);
  tbEditParent.appendChild(tbEditDiv);

  const onDlgTbEditDone = function (rec) {
    updateTbRecord(rec);
    updateTr(parent, rec);
    modal2.close();
  };
  const onDlgCancel = function () {
    modal2.close();
  };
  const closeBtn = modal2.querySelector(".close-btn");
  closeBtn.onclick = onDlgCancel;

  const settings = {
    container: tbEditDiv,
    record: rec,
    projListGetter: getProjectsNames,
    newProjectSaver: addProjectName,
    taskListGetter: loadTasks,
    onTbRecSubmitClick: onDlgTbEditDone,
    date: rec.date,
  };

  tbEditDiv.tbEditor = new TBRecEditor(settings);
  modal2.showModal();
}

function deleteclick(btn) {
  let parent = btn.srcElement.parentElement.parentElement;
  let id = parent.id;
  deleteTbRecordById(id);
  removeTableRow(parent);

  console.log("delete click");
}
function removeTableRow(row) {
  row.remove();
}

function closeProject() {
  let prj = document.getElementById("newProjectDlg");
  prj.close();
  const selectElement = document.getElementById("Project");
  if (selectElement.selectedIndex == 0) selectElement.selectedIndex = -1;
}

function updateTr(recRow, rec) {
  let tds = recRow.childNodes;
  let td0 = tds[0]; // дата
  let td1 = tds[1]; // проект
  let td2 = tds[2]; // задача
  let td3 = tds[3]; // потрачений час
  td0.innerHTML = rec.date;
  td1.innerHTML = rec.project;
  td2.innerHTML = rec.task;
  td3.innerHTML = String(rec.effort);
}

function onAddNew(evt) {
  if (evt.target.value == "AddNew") {
    let newPrjDlg = document.getElementById("newProjectDlg");
    newPrjDlg.showModal();
  }
}
function onCancelNewProj2() {
  let newPrjDlg = document.getElementById("newProjectDlg");
  let textareaEditProj = document.getElementById("editProject");
  textareaEditProj.value = "";
  showProjectNameError(null);
  newPrjDlg.close();
  const selectElement = document.getElementById("Project");
  if (selectElement.selectedIndex == 0) selectElement.selectedIndex = -1;
}

function saveNewProject2() {
  let projName = editProject.value.trim();
  if (projName == "" || !addProjectName(projName)) {
    showProjectNameError(projName);
    return;
  }
  updateProj2(projName);
  onCancelNewProj2();
}
function showProjectNameError(projName) {
  if (projName || projName == "") {
    document.getElementById("prgNmId").innerHTML = projName;
    document.getElementById("newProjErr").style.visibility = "visible";
    return;
  }
  document.getElementById("prgNmId").innerHTML = "";
  document.getElementById("newProjErr").style.visibility = "hidden";
}

function updateProj2(projName) {
  let selectElement = document.getElementById("Project");
  addOptionsToSelect(selectElement, [projName], projName);
}

function onSelectClick(event) {
  // const target = event.target;
  // const targetTagName = target.tagName;
  // return;
  // let idx = event.currentTarget.selectedIndex;
  // if (idx != 0) return
  // let newPrjDlg = document.getElementById("newProjectDlg");
  // newPrjDlg.showModal();
  // return;
  // const target = event.target; AH one comment
  //  let selOpts = event.currentTarget.options;
  //  if (selOpts.length > 1) return;
  // if (event.target.value == "AddNew") {
  // }
}
function onDateRangeChange() {
  openCover();
  document.getElementById("DateRangeView").style.display = "none";
  document.getElementById("DateRangeEdit").style.display = "flex";
  let inputDates = document.querySelectorAll(".smallMargin input");
  inputDates[0].value = dateToStdStr(dateRange.from);
  inputDates[1].value = dateToStdStr(dateRange.to);
}
function openCover() {
  let cover = document.querySelector(".cover");
  window.onclick = function (event) {
    if (event.target === document.querySelector(".cover")) {
      clouseCover();
      window.onclick = null;
    }
  };
  cover.style.display = "block";
}
function clouseCover() {
  let cover = document.querySelector(".cover");
  cover.style.display = "none";
  document.getElementById("DateRangeView").style.display = "flex";
  document.getElementById("DateRangeEdit").style.display = "none";
}
function onDateRangeDone() {
  let inputDates = document.querySelectorAll(".smallMargin input");
  let dateFrom = inputDates[0].value;
  let dateTo = inputDates[1].value;
  if (dateFrom == "" || dateTo == "") {
    clouseCover();
    return;
  }
  let errorParagraph = document.querySelector(".smallMargin p");
  if (dateFrom > dateTo) {
    errorParagraph.style.display = "flex";
    return;
  }
  errorParagraph.style.display = "none";

  clouseCover();
  saveGlobalDateRange(dateFrom, dateTo);
  setDateRanges();
  let records = loadTasks();
  fillEffortTable(records);
}
function fillEffortTable(recs) {
  const tblBody = document.getElementById("tblBodyId");
  removeAllChildNodes(tblBody);
  let recsSorted = recs.sortBy("date", "asc");
  let dates = recsSorted.map((x) => x.date);
  for (let x of recsSorted) {
    addRecord(x);
  }
}
function onTbMainSelectTask() {
  onSelectTask(onSelectTaskDone, null, loadTasks);
}
function onSelectTaskDone(taskName) {
  const inputSelectedTask = document.getElementById("task");
  inputSelectedTask.value = taskName;
}
