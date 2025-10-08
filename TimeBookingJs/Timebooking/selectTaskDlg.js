<<<<<<< HEAD
const task1Template = { taskName: "Task1", effort: 0, date: "2020-01-01" };
const myModal = document.getElementById("TaskSelectDlg");

function loadMyTasks() {
  const recs = reloadTbRecords(dateRange.from, dateRange.to);
  let tasks = groupTasks(recs);
  return tasks;
}

function groupTasks(bookings) {
  // from rec: id,project,task,comment,date,effort
  // to rec: taskName:, effort: effort, date: dat1
  // task as taskName, sum(effort) as effort, max(date) as lastBooked
  // group by task
  let tasksMap = new Map();
  for (let bookRec of bookings) {
    if (tasksMap.has(bookRec.task)) {
      const taskAgr = tasksMap.get(bookRec.task);
      taskAgr.effort += bookRec.effort;
      taskAgr.date =
        taskAgr.date.localeCompare(bookRec.date) == 1
          ? taskAgr.date
          : bookRec.date;
      continue;
    }
    tasksMap.set(bookRec.task, bookRec);
  }
  const tasks = [];
  tasksMap
    .values()
    .forEach((x) =>
      tasks.push({ taskName: x.task, effort: x.effort, date: x.date })
    );
  return tasks;
}

function showTasksInGrid(tasks) {
  removeAllTasksInGrid();
  tasks.forEach((tsk) => {
    addTaskRec(tsk);
  });
}
function onSelTask(evnt) {
  hRef = evnt.currentTarget;
  const row1 = hRef.parentElement.parentElement;
  const selectedTaskName = row1.cells[0].innerText;
  closeTaskSelect(selectedTaskName);
}
function removeAllTasksInGrid() {
  let tb = document.getElementById("taskTblBodyId");
}
function addTaskRec(rec) {
  let tb = document.getElementById("taskTblBodyId");
  //let newrow= document.createElement('tr')створює елемент tr й присваює в змінну
  let newrow = document.createElement("tr"); //tb.appendRow();
  const taskCols = Object.keys(task1Template);
=======
function onSelectTask(onDone, onCancel, loadTaskFunc) {
  document.body.style.filter = "brightness(0.8)"; // Dim the main screen
  const dlg = document.getElementById("TaskSelectDlg");
  dlg.style.display = "block";
  dlg.onTaskSelected = onDone;
  dlg.onTaskSelectionCanceled = onCancel;
  let tasks = loadTaskFunc();
  showTasksInGrid(tasks);
  dlg.showModal();
  const closeModalBtn = document.getElementById("closeModalBtn");
  closeModalBtn.addEventListener("click", function (event) {
    closeTaskSelect();
  });
  dlg.addEventListener("click", function (event) {
    if (event.target === dlg) {
      closeTaskSelect();
    }
  });
}
function closeTaskSelect(selectedTaskName) {
  const dlg = document.getElementById("TaskSelectDlg");
  document.getElementById("FilterTasks").value = "";
  const onDone = dlg.onTaskSelected;
  const onCancel = dlg.onTaskSelectionCanceled;
  dlg.close();
  dlg.style.display = "none";
  document.body.style.filter = "";
  if (selectedTaskName) onDone(selectedTaskName);
  else if (onCancel) onCancel();
}

//const arr = testGenTasks(20);
function showTasksInGrid(tasks) {
  removeAllChildNodesById("taskTblBodyId");
  const grouppedtasks = groupTasksByName(tasks);
  grouppedtasks.forEach((task) => {
    addTaskRec(task);
  });
}
function addTaskRec(rec) {
  const task1Template = { task: "Task1", effort: 0, date: "2020-01-01" };
  let tb = document.getElementById("taskTblBodyId");
  let taskCols = Object.keys(task1Template);
  let newrow = document.createElement("tr");
>>>>>>> anChanges
  for (let fld of taskCols) {
    let td = document.createElement("td");
    td.textContent = rec[fld];
    newrow.appendChild(td);
  }
  var aElm = document.createElement("a");
  var createAText = document.createTextNode("Select and Exit");
  aElm.setAttribute("href", "#");
  aElm.appendChild(createAText);
  aElm.onclick = onSelTask;
  let tdLink = document.createElement("td");
  tdLink.appendChild(aElm);
  newrow.appendChild(tdLink);
<<<<<<< HEAD

  // newrow.id = rec.id;
  // let tdButton = createTdWithButton("Edit", editclick);
  // let tdButton1 = createTdWithButton("Delete", deleteclick);
  // newrow.appendChild(tdButton);
  // newrow.appendChild(tdButton1);
  tb.appendChild(newrow); //добавляє в кінець таблиців tbody
  return;
}
function onSelectTask(e) {
  const inputToUpdate = e.currentTarget.parentElement.querySelector("input");
  document.body.style.filter = "brightness(0.8)"; // Dim the main screen
  const dlg = document.getElementById("TaskSelectDlg");
  const closeModalBtn = document.getElementById("closeModalBtn");
  dlg.style.display = "block";

  // Close the modal when the close button (X) is clicked
  closeModalBtn.addEventListener("click", function () {
    closeTaskSelect();
  });
  // close by clicking outside dialog
  myModal.addEventListener("click", function (event) {
    if (event.target === myModal) {
      closeTaskSelect();
    }
  });

  let tasks = loadMyTasks();
  showTasksInGrid(tasks);
  dlg.inputToUpdate = inputToUpdate;
  dlg.showModal();
}
function closeTaskSelect(selectedTaskName) {
  const dlg = document.getElementById("TaskSelectDlg");
  const inputToUpdate = dlg.inputToUpdate;
  dlg.close();
  document.body.style.filter = ""; // Restore the brightness of the main screen
  dlg.style.display = "none"; // Hide the modal
  if (!selectedTaskName) return;
  selectedTaskName = selectedTaskName.trim();
  if (selectedTaskName && selectedTaskName.length > 0) {
    const inputSelectedTask = inputToUpdate
      ? inputToUpdate
      : document.getElementById("task");
    inputSelectedTask.value = selectedTaskName;
  }
}

function onKeyUpTaskFilter() {
  const input = document.getElementById("FilterTasks").value.toUpperCase();
  const table = document.getElementById("TaskTableId");
  const tr = table.getElementsByTagName("tr");

  for (let i = 1; i < tr.length; i++) {
    const td = tr[i].getElementsByTagName("td")[0];
    if (td) {
      const txtValue = td.textContent || td.innerText;
      tr[i].style.display =
=======
  tb.appendChild(newrow);
}
function onSelTask(event) {
  let a = event.currentTarget;
  const tr = a.parentElement.parentElement;
  const task = tr.cells[0].innerText;
  closeTaskSelect(task);
}
function onKeyUpTaskFilter() {
  let input = document.getElementById("FilterTasks").value.toUpperCase();
  const table = document.getElementById("TaskTableId");
  const trs = table.getElementsByTagName("tr");

  for (let i = 1; i < trs.length; i++) {
    const td = trs[i].getElementsByTagName("td")[0];
    if (td) {
      const txtValue = td.textContent || td.innerText;
      trs[i].style.display =
>>>>>>> anChanges
        txtValue.toUpperCase().indexOf(input) > -1 ? "" : "none";
    }
  }
}

<<<<<<< HEAD
//+ button to close. Styles, closeTaskSelect
//+ search line: #FilterTasks
//+ styles of task-tables .TblInDlg
//+ generate random tasks for grid
//+ show tasks in grid
//+ close by clicking outside dialog
//+ scroll for the table body
//+ select task and show in task control
//+ filter tasks by name input
//+ groupping tasks by name,sum(effort), max(date),
//       sort by last modify desc
//+ filter tasks by date range
//        all, from..to, week (this, prev, before prev), month(...), quarter (...)
//        could be in header from - to [...] week,month,year
//        2024-10-01 - 2024-12-10 [...]
// todo: loading tasks from storage
//    loadMyTasks() should be: reloadTbRecords() ? (used in timeBooking.js)
// todo: review: Required resources:
//    selectTaskDlg.js
// TaskSelectDlg with "taskTblBodyId",closeModalBtn, task
//#TaskSelectDlg, .TaskDlgHeader, #FilterTasks,
//.close-btn, .TblWithTasks {
//.TblWithTasks thead th {
//.TblWithTasks tbody td {
//.TblWithTasks tbody tr:hover {
//.TaskDlgTblContainer {
=======
function groupTasksByName(recs) {
  // {task:, effort:, date:}
  let tasksMap = new Map();
  recs.forEach((x) => {
    if (tasksMap.has(x.task)) {
      let agrTask = tasksMap.get(x.task);
      agrTask.effort += x.effort;
      agrTask.date = maxSting(agrTask.date, x.date);
    } else {
      tasksMap.set(x.task, { task: x.task, effort: x.effort, date: x.date });
    }
  });
  let myArray = Array.from(tasksMap, ([key, value]) => value);
  myArray = myArray.sortBy("date", "desc");
  return myArray;
}
>>>>>>> anChanges
