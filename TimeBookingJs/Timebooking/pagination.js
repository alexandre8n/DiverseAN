// Pagination logic
let currentPage = 1;
let totalPages = 1; // Set total pages for your pagination (change as needed)
let pageChangedCallback = null;

// Get elements from the DOM
const navigator = document.querySelector(".pagination");

const pageInput = document.getElementById("pageInput");
const totalPagesElement = document.getElementById("totalPages");
const prevPageButton = document.getElementById("prevPage");
const nextPageButton = document.getElementById("nextPage");
const firstPageButton = document.getElementById("firstPage");
const lastPageButton = document.getElementById("lastPage");

// Update the total number of pages in the UI
totalPagesElement.textContent = totalPages;

// Update page input field when the current page changes
function updatePageInput() {
  pageInput.value = currentPage;
  totalPagesElement.textContent = totalPages;
}

// Disable or enable buttons based on current page
function updateButtonState() {
  prevPageButton.disabled = currentPage === 1;
  nextPageButton.disabled = currentPage === totalPages;
  firstPageButton.disabled = currentPage === 1;
  lastPageButton.disabled = currentPage === totalPages;
}

// Navigate to a specific page number
function goToPage(page) {
  if (page < 1) page = 1;
  if (page > totalPages) page = totalPages;
  const changed = page != currentPage;
  currentPage = page;
  updatePageInput();
  updateButtonState();
  if (currentPage && pageChangedCallback) pageChangedCallback(currentPage);
}

// First page button
firstPageButton.addEventListener("click", () => goToPage(1));

// Previous page button
prevPageButton.addEventListener("click", () => goToPage(currentPage - 1));

// Next page button
nextPageButton.addEventListener("click", () => goToPage(currentPage + 1));

// Last page button
lastPageButton.addEventListener("click", () => goToPage(totalPages));

// Input field change for navigating to specific page
pageInput.addEventListener("input", (e) => {
  let page = parseInt(e.target.value);
  if (isNaN(page)) page = 1; // If the input is not a number, default to page 1
  goToPage(page);
});

// Initialize the pagination state
updateButtonState();

function hideNavigator() {
  navigator.style.display = "none";
  pageChangedCallback = null;
}
function showNavigator(page, total, onChanged) {
  navigator.style.display = "flex";
  currentPage = page;
  totalPages = total;
  pageChangedCallback = onChanged;
  updatePageInput();
  updateButtonState();
}
