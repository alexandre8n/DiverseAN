let user = "";
// Get references to the elements
const loginButton = document.getElementById("loginButton");
const loginDialog = document.getElementById("loginDialog");
const closeDialog = document.getElementById("closeDialog");
const loginDone = document.getElementById("loginDone");
const loginForm = document.getElementById("loginForm");
document.getElementById("username").value = "AH";
document.getElementById("password").value = "123";

// Show the login dialog when the "Login" button is clicked
loginButton.addEventListener("click", function () {
  if (loginButton.innerText == "Exit") {
    var answer = window.confirm("Are you sure you want to logout?");
    if (!answer) return;
    document.getElementById("userName").innerHTML = "";
    clearUserData(user);
    fillEffortTable([]);
    setActiveView("");
    loginButton.innerText = "Login";
    return;
  }
  loginDialog.style.display = "flex";
});

// Close the dialog when the "Close" button is clicked
closeDialog.addEventListener("click", function () {
  loginDialog.style.display = "none";
});

// Handle the form submission
loginDone.addEventListener("click", function (event) {
  //event.preventDefault(); // Prevent the default form submission

  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;

  // Basic validation (you can add more complex logic)
  if (username && password) {
    document.getElementById("userName").innerHTML = username;
    loginDialog.style.display = "none"; // Close dialog after successful login
    loginButton.innerText = "Exit";
    document.getElementById("password").value = "";
  } else {
    alert("Please enter both username and password.");
    return;
  }
  user = username;
  setActiveView("tbMain");
});

// span for user name
// after closing dialog -> user to span
//
