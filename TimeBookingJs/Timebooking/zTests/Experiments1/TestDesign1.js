function maxHavingNegBro(list) {
  let negs = new Set(list.filter((x) => x < 0));
  let mx = -Infinity;
  list.forEach((x) => {
    if (x > 0 && negs.has(-x)) mx = Math.max(x, mx);
  });
  return mx == -Infinity ? 0 : mx;
}

let bookings = {}; // Store bookings for each day
let selectedDate = null;

// Function to generate the list of days with details: date, weekday, and total efforts
function generateCalendarDays(year, month) {
  const calendar = document.getElementById("calendar");
  calendar.innerHTML = ""; // Clear any existing days

  const daysInMonth = new Date(year, month, 0).getDate();
  const firstDayOfMonth = new Date(year, month - 1, 1).getDay();

  // Loop through days of the month and create the calendar
  for (let i = 0; i < firstDayOfMonth; i++) {
    const emptyCell = document.createElement("div");
    calendar.appendChild(emptyCell); // Empty cells for alignment
  }

  for (let day = 1; day <= daysInMonth; day++) {
    const dayCell = document.createElement("div");
    const date = new Date(year, month - 1, day);
    const weekday = date.toLocaleString("en-us", { weekday: "short" });
    const totalEfforts = getTotalEffortsForDay(year, month, day);

    dayCell.classList.add("calendar-day");
    dayCell.innerHTML = `
      <strong>${day}</strong><br>
      ${weekday}<br>
      ${totalEfforts} hrs
    `;
    dayCell.onclick = () => showBookingsForDay(year, month, day);
    calendar.appendChild(dayCell);
  }
}

// Get the total efforts (hours) for a specific day
function getTotalEffortsForDay(year, month, day) {
  const selectedDay = `${year}-${month}-${day}`;
  const dayBookings = bookings[selectedDay] || [];
  let totalEfforts = 0;

  dayBookings.forEach((booking) => {
    totalEfforts += parseFloat(booking.efforts);
  });

  return totalEfforts.toFixed(2);
}

// Show bookings for the selected day
function showBookingsForDay(year, month, day) {
  selectedDate = `${year}-${month}-${day}`;
  const selectedDayText = document.getElementById("selectedDay");
  const bookingList = document.getElementById("bookingList");
  const monthlyTotalElem = document.getElementById("monthlyTotal");
  let dailyTotal = 0;

  selectedDayText.textContent = `Bookings for ${selectedDate}`;

  // Clear existing bookings
  bookingList.innerHTML = "";

  const dayBookings = bookings[selectedDate] || [];
  dayBookings.forEach((booking, index) => {
    const bookingItem = document.createElement("li");
    bookingItem.innerHTML = `
      <strong>${booking.project}</strong> - ${booking.task}<br>
      Efforts: ${booking.efforts} hours<br>
      Comment: ${booking.comment}
      <button onclick="deleteBooking(${index})">Delete</button>
    `;
    bookingList.appendChild(bookingItem);
    dailyTotal += parseFloat(booking.efforts);
  });

  document.getElementById("bookingForm").reset();
  document.getElementById("efforts").focus();

  // Update the daily total
  document.getElementById(
    "dailyTotal"
  ).textContent = `Total Hours for ${selectedDate}: ${dailyTotal.toFixed(
    2
  )} hours`;

  // Update the monthly total
  let monthlyTotal = 0;
  Object.values(bookings).forEach((dayBookings) => {
    dayBookings.forEach((booking) => {
      monthlyTotal += parseFloat(booking.efforts);
    });
  });
  monthlyTotalElem.textContent = `Total Booking Hours for the Month: ${monthlyTotal.toFixed(
    2
  )} hours`;
}

// Handle booking form submission
document
  .getElementById("bookingForm")
  .addEventListener("submit", function (event) {
    event.preventDefault();

    const project = document.getElementById("project").value;
    const task = document.getElementById("task").value;
    const efforts = document.getElementById("efforts").value;
    const comment = document.getElementById("comment").value;

    if (!selectedDate || !project || !task || !efforts) {
      alert("Please fill in all fields.");
      return;
    }

    if (!bookings[selectedDate]) {
      bookings[selectedDate] = [];
    }

    bookings[selectedDate].push({ project, task, efforts, comment });

    // Refresh the bookings for the selected day
    const year = document.getElementById("monthSelect").value.split("-")[0];
    const month = document.getElementById("monthSelect").value.split("-")[1];
    showBookingsForDay(year, month, selectedDate);
  });

// Initialize the calendar and month selector
document
  .getElementById("monthSelect")
  .addEventListener("input", function (event) {
    const [year, month] = event.target.value.split("-");
    generateCalendarDays(year, parseInt(month, 10));
  });

// Delete a booking
function deleteBooking(index) {
  bookings[selectedDate].splice(index, 1);
  showBookingsForDay(...selectedDate.split("-"));
}

// Initialize with the current month
const currentDate = new Date();
const currentMonth = currentDate.getMonth() + 1;
const currentYear = currentDate.getFullYear();
document.getElementById("monthSelect").value = `${currentYear}-${currentMonth
  .toString()
  .padStart(2, "0")}`;
generateCalendarDays(currentYear, currentMonth);
