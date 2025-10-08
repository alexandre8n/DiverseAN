document.querySelector(".burger").addEventListener("click", function () {
  this.classList.toggle("active");
  document.querySelector(".nav").classList.toggle("open");
});

function closeMenu() {
  document.querySelector(".burger").classList.remove("active");
  document.querySelector(".nav").classList.remove("open");
}

const viewItems = document.querySelectorAll(".submenu li a");
viewItems.forEach((item) => {
  item.addEventListener("click", () => {
    selectViewType(item.parentElement);
  });
});
function setViewMenuItemSelected(itemId) {
  const item = document.getElementById(itemId);
  if (!item.classList.contains("selected")) selectViewType(item);
}
// Function to handle view type selection
function selectViewType(item) {
  // Remove 'selected' class from all items
  const allItems = document.querySelectorAll(".submenu li");
  allItems.forEach((item) => item.classList.remove("selected"));

  // Add 'selected' class to the clicked item
  item.classList.add("selected");

  // Process the selection of the view:
  updateCurView(item.id);
  // close menu
  closeMenu();
}
