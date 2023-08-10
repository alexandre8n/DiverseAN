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

function closeModal(modal) {
  if (modal == null) return;
  if (elemOfHistory != null) {
    elemOfHistory.remove();
  }
  modal.classList.remove("active");
  overlay.classList.remove("active");
}

let elemOfHistory = null;

function openModalHistory(modal, columns, historyArr) {
  if (modal == null) return;

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
  let table = new DataTable("#example", {
    data: historyArr,
    columns: cols4Table,
    order: [[0, "desc"]],
  });

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
}

function onClearHistory() {
  let table = new DataTable("#example", {
    data: [], // empty array
    columns: cols4Table,
    order: [[0, "desc"]],
  });
  console.log("on clear history");
}

function setHeader(strHeader) {
  mdlHeader = document.getElementById("modalHeaderId");
  mdlHeader.innerHTML = strHeader;
  clrHistoryId.style.visibility = "visible";
}
