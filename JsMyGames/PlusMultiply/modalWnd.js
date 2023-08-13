const openModalButtons = document.querySelectorAll("[data-modal-target]");
const closeModalButtons = document.querySelectorAll("[data-close-button]");
const overlay = document.getElementById("overlay");

openModalButtons.forEach((button) => {
  button.addEventListener("click", () => {
    const modal = document.querySelector(button.dataset.modalTarget);
    openModal(modal);
  });
});

overlay.addEventListener("click", () => {
  const modals = document.querySelectorAll(".modal.active");
  modals.forEach((modal) => {
    closeModal(modal);
  });
});

closeModalButtons.forEach((button) => {
  button.addEventListener("click", () => {
    const modal = button.closest(".modal");
    closeModal(modal);
  });
});

function openModal(modal, msg) {
  if (modal == null) return;
  modal.classList.add("active");
  overlay.classList.add("active");
  document.getElementById("modalHelpId").innerHTML = msg;
}

// this method if !=null is call when closing modal dlg...
callBackAfterFinish = null;

function setAfterFinishCallback(func1) {
  callBackAfterFinish = func1;
}

function closeModal(modal) {
  if (modal == null) return;
  if (elemOfHistory != null) {
    elemOfHistory.remove();
  }
  modal.classList.remove("active");
  overlay.classList.remove("active");
  filterId = fltAll;
  if (callBackAfterFinish != null) {
    callBackAfterFinish();
    callBackAfterFinish = null;
  }
}

let elemOfHistory = null;
let histTableStru = { table: null, cols: null };
const fltQuick = 1;
const fltAll = 0;
const fltWrong = 2;
const fltWrongAndSlow = 3;
const timeSlow = 5;
var filterId = fltAll;

function openModalHistory(modal, columns, historyArr, callBackAfterClose) {
  if (modal == null) return;

  if (callBackAfterClose) {
    setAfterFinishCallback(callBackAfterClose);
  }

  setHeader("History");
  modal.classList.add("active");
  overlay.classList.add("active");

  //modalHelpId;
  // Clone the template content to reuse it multiple times
  elemOfHistory = document.createElement("div");
  elemOfHistory.append(tblTmpl.content.cloneNode(true));
  let helpDiv = document.getElementById("modalHelpId");
  helpDiv.innerText = "";
  helpDiv.append(elemOfHistory);
  const cols4Table = columns.map((x) => {
    let v = { title: x };
    return v;
  });

  $("#example").one("preInit.dt", function () {
    var elSrc = document.getElementById("filterSelectInvisible");
    var elClone = elSrc.cloneNode(true);
    elSrc.style.display = "none";
    elClone.id = "filterSelect";
    //$("#example_filter label").append(elClone);
    let filterDiv = document.getElementById("example_filter");
    let firstChild = filterDiv.firstChild;
    filterDiv.insertBefore(elClone, firstChild);
  });

  let table = new DataTable("#example", {
    data: historyArr,
    columns: cols4Table,
    order: [[0, "desc"]],
  });

  histTableStru = { table: table, cols: cols4Table };

  DataTable.ext.search.push(function (settings, data, dataIndex) {
    let score = parseInt(data[4]);
    let time = score == 0 ? 100000000 : parseInt(data[3]) / 1000;

    if (filterId == fltAll) return true;
    if (filterId == fltQuick) return time <= timeSlow;
    if (filterId == fltWrong) return score == 0;
    if (filterId == fltWrongAndSlow) return time > timeSlow;
    return false;
  });
} // end of openModalHistory

function onFilterSelect() {
  const ctlCombo = document.getElementById("filterSelect");
  const idx = ctlCombo.selectedIndex;
  const val = parseInt(ctlCombo.value);
  switch (val) {
    case 1:
      // quick
      filterId = fltQuick;
      break;
    case 2:
      // wrong
      filterId = fltWrong;
      break;
    case 3:
      // slow or wrong
      filterId = fltWrongAndSlow;
      break;
    default:
      filterId = fltAll;
  }
  histTableStru.table.draw();
}

// how to process click on the row
//let table = new DataTable('#example');
// table.on('click', 'tbody tr', function () {
//   let data = table.row(this).data();
//alert('You clicked on ' + data[0] + "'s row");
//});
// how to filter()
// https://datatables.net/reference/api/filter()
// example: var table = $('#example').DataTable();
// var filteredData = table
//   .columns([0, 1])
//   .data()
//   .flatten()
//   .filter(function (value, index) {
//     return value > 20 ? true : false;
//   });

function onClearHistory() {
  // let t = $("#example");

  //histTableStru.table.destroy();
  // could be an option for using destroy: true;

  histTableStru.table = new DataTable("#example", {
    destroy: true, // this just needed to be able to reinitialize!!
    data: [], // empty array
    columns: histTableStru.cols,
    order: [[0, "desc"]],
  });
  console.log("on clear history");
  // todo: here the history is to be deleted!!
}

function setHeader(strHeader) {
  mdlHeader = document.getElementById("modalHeaderId");
  mdlHeader.innerHTML = strHeader;
  clrHistoryId.style.visibility = "visible";
}
