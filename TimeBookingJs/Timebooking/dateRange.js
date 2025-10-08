function dateRangeInit(contPath, dtFr, dtTo, onDtRangeChanged) {
  // to show date range:
  const dtRangeShow = createDtRangeShow();
  // to edit: initially hidden
  const dtRangeEdit = createDtRangeEdit();
  // cover to avoid clicks outside date-range control
  const cover = createBodyCover("dtRangeCover");

  const dtShowSpans = dtRangeShow.querySelectorAll("span");
  dtShowSpans[0].innerHTML = dateToStdStr(dtFr);
  dtShowSpans[1].innerHTML = dateToStdStr(dtTo);
  const btnChange = dtRangeShow.querySelector("button");
  const container = document.querySelector(contPath);
  removeAllChildNodes(container);

  container.appendChild(dtRangeShow);
  container.appendChild(dtRangeEdit);
  btnChange.addEventListener("click", function () {
    dtRangeShow.style.display = "none";
    // show cover:
    cover.style.display = "block";
    cover.addEventListener("click", function () {
      dtClose(cover, dtRangeShow, dtRangeEdit);
    });
    // show dtRangeEdit
    dtRangeEdit.style.display = "flex";
    const dtInputs = dtRangeEdit.querySelectorAll("input");
    dtInputs[0].value = dtShowSpans[0].innerHTML;
    dtInputs[1].value = dtShowSpans[1].innerHTML;
    const btnDone = dtRangeEdit.querySelector("button");
    btnDone.addEventListener("click", function () {
      let inpDtFr = dtInputs[0].value;
      let inpDtTo = dtInputs[1].value;
      if (inpDtFr == "" && inpDtTo == "") return;
      else if (inpDtFr == "") inpDtFr = inpDtTo;
      else if (inpDtTo == "") inpDtTo = inpDtFr;
      else if (inpDtFr > inpDtTo) {
        [inpDtFr, inpDtTo] = [inpDtTo, inpDtFr];
      }
      dtShowSpans[0].innerHTML = inpDtFr;
      dtShowSpans[1].innerHTML = inpDtTo;
      if (onDtRangeChanged) onDtRangeChanged(inpDtFr, inpDtTo);
      dtClose(cover, dtRangeShow, dtRangeEdit);
    });
  });

  return btnChange;
}
function createDtRangeShow() {
  // create:
  // <div data-containerId="" style="display:flex;gap:5px">
  //   Date Range: <span class="dtRange">2020-01-01</span> to
  //   <span class="dtRange">2025-01-01</span>
  // </div>;
  const div = document.createElement("div");
  div.innerHTML = `<div data-containerId="" style="display:flex;align-items: center;gap:5px">
   Date Range: <span class="dtRange">2020-01-01</span> to
   <span class="dtRange">2025-01-01</span>
   <button>Change</button>
  </div>`;
  return div;
}
function createDtRangeEdit() {
  // <div data-group="dtRangeEdit" style="display: none;
  // flex-direction: row; gap: 5px; padding: 3px;
  // z-index: 3;background-color: white;">
  //   <label for="dateFrom">From: </label>
  //   <input type="date" id="dateFrom" name="dateFrom" />
  //   <label for="dateTo">To: </label>
  //   <input type="date" id="dateTo" name="dateTo" />
  //   <button>Done</button>
  // </div>;
  const div = document.createElement("div");
  div.style.display = "none";
  div.style.alignItems = "center";
  div.style.gap = "5px";
  div.style.flexDirection = "row";
  div.style.zIndex = "3";
  div.style.backgroundColor = "white";
  div.style.padding = "7px";
  div.innerHTML =
    '<label for="dateFrom">From:</label><input type="date" name="dateFrom" />' +
    '<label for="dateTo">To:</label><input type="date" name="dateTo" />' +
    "<button>Done</button>";
  return div;
}
function dtClose(cover, dtRangeShow, dtRangeEdit) {
  cover.style.display = "none";
  dtRangeEdit.style.display = "none";
  dtRangeShow.style.display = "flex";
}
