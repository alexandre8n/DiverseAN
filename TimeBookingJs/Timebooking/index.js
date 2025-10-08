const submenuItemIds = ["tbMain", "tbDRO", "tbPO"];
const checkboxGlb = document.getElementById("hamburger-menu-id");
let currenViewDivId = "tbMain-div";
// Add an event listener for the 'change' event
checkboxGlb.addEventListener("change", function () {
  // Check if the checkbox is checked, use: checkbox.checked
  if (checkboxGlb.checked) {
    window.addEventListener("click", onWndClick);
  } else {
    window.removeEventListener("click", onWndClick);
  }
});

function setActiveView(viewName) {
  const listItems = document.querySelectorAll(".submenu li");
  listItems.forEach((x) => {
    x.classList.remove("selected");
  });
  const menuElm = document.getElementById(viewName);
  const curViewDiv = document.getElementById(currenViewDivId);
  if (!menuElm || !curViewDiv || user == "" || viewName == "") {
    document.getElementById(currenViewDivId).style.display = "none";
    return;
  }
  menuElm.classList.add("selected");
  const divNewCandidateId = viewName + "-div";
  curViewDiv.style.display = "none";
  currenViewDivId = divNewCandidateId;
  const curDiv = document.getElementById(divNewCandidateId);
  if (!curDiv) return;
  curDiv.style.display = "block";
  if (viewName == "tbMain") initTbMain();
  if (viewName == "tbDRO") initTbDRO();
}

function onWndClick(event) {
  const trg = event.target;
  const trgParent = trg.parentNode;
  const sideBar = document.querySelector(".sidebar");
  if (trg.id == "hamburgerId") {
    event.stopImmediatePropagation(); //not needed...
    return;
  } else if (trg !== sideBar && !sideBar.contains(trg)) {
    checkboxGlb.checked = false;
    window.removeEventListener("click", onWndClick);
  }
  if (!trgParent && !trgParent.id) return;
  if (submenuItemIds.includes(trgParent.id)) {
    setActiveView(trgParent.id);
  }
}
