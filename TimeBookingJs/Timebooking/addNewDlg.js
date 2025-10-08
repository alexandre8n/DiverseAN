// this component allows to specify the value
// send the entered value to proceed to some function
// check if the entered value is allowed
// cancel entering value, callback, if canceled.
// styling of this dialog is addNewDlg.css (default), or some other, if desired.
// the following classes are standard:
//
// settings={
//  dialogClass: "addNewDlg",
//  onCancel: null,
//  onDone: null,
//  placeholderMsg: "Please, specify value...",
//  buttonText: "Done...",
//  excludeList: listOfValuesToExclude
//  excludeListMsg: 'You may not specify this value: @val'
//}
// example of onDone:
// function onDone(input) {
//   const inpVal = input.value.trim();
//   if (inpVal == "a") {
//     showTooltip(input, "This project: a, already exists");
//     return false;
//   } else if (inpVal == "") {
//      showTooltip(input, "Project name is empty");
//      return false;
//   }
//   testId.innerHTML = input.value;
//   return true;
// }
// onCancel is called if user clicks outside the dialog
// by defaule it is empty. It should return true (cancel OK) or false (not Ok)

function addNewDlgOpen(settings) {
  const cover = createBodyCover("dtRangeCover");
  const dlgDiv = addNewDlgCreateDlgDiv(settings);
  const btnDone = dlgDiv.querySelector("button");
  const input = dlgDiv.querySelector("input");
  if (settings.onDone) {
    cover.addEventListener("click", function () {
      addNewDlgCancel(dlgDiv, input, cover, settings);
    });
    btnDone.addEventListener("click", function (e) {
      if (settings.onDone) {
        if (settings.onDone(input)) {
          dlgDiv.remove();
          cover.style.display = "none";
        }
      }
    });
  }
  cover.style.display = "block";
  dlgDiv.style.display = "block";
  dlgDiv.style.zIndex = "3";
  document.body.appendChild(cover);
  document.body.appendChild(dlgDiv);
}

function addNewDlgCancel(dlg, input, cover, settings) {
  if (settings.onCancel) {
    if (!settings.onCancel(input)) return;
  }
  dlg.remove();
  cover.style.display = "none";
}

function addNewDlgCreateDlgDiv(settings) {
  const div = document.createElement("div");
  let divTxt = `
    <input type="text" placeholder="@templateMsg@">
    <button>@btnTxt@</button>
    `;
  let tmplMsg = settings.placeholderMsg;
  if (!tmplMsg) tmplMsg = "Enter value...";
  divTxt = divTxt.replace("@templateMsg@", tmplMsg);
  let btnTxt = settings.buttonText;
  if (!btnTxt) btnTxt = "Done...";
  divTxt = divTxt.replace("@btnTxt@", btnTxt);
  div.innerHTML = divTxt;
  if (!settings.dialogClass) settings.dialogClass = "addNewDlg";
  div.classList.add(settings.dialogClass);
  return div;
}

// #overlay {
//     position: fixed;
//     top: 0;
//     left: 0;
//     width: 100%;
//     height: 100%;
//     background-color: rgba(0, 0, 0, 0.5);
//     /* Semi-transparent black */
//     display: none;
//     /* Initially hidden */
//     z-index: 1;
// }
