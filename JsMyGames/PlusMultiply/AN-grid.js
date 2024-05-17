var defaultOptions = {
  gridHeaderClassName: "",
  gridRowClassName: "",
  sortingAllowed: true,
  ascSignElm: null,
  descSignEml: null,
};

function showGrid(tbl, header, rows, options) {
  if (tbl.onclick == null) tbl.onclick = onClickRow;
  if (options == null) {
    setGridOptions(tbl, defaultOptions);
  } else setGridOptions(tbl, options);
  const tblBody = document.createElement("tbody");
  // creating all cells
  for (let i = -1; i < rows.length; i++) {
    // creates a table row
    const trRow = document.createElement("tr");
    trRow.className = options.gridRowClassName;
    if (i == -1) {
      trRow.className = options.gridHeaderClassName;
    }

    for (let j = 0; j < header.length; j++) {
      // Create a <td> element and a text node, make the text
      // node the contents of the <td>, and put the <td> at
      // the end of the table row
      const tdOrth = i == -1 ? "th" : "td";
      const cell = document.createElement(tdOrth);
      var val = "";
      if (i == -1) {
        val = header[j];
        cell.style.cursor = "pointer";
        //cell.className = "bold";
      } else {
        val = rows[i][j];
        //cell.className = "basicTxt";
      }
      const cellText = document.createTextNode(`${val}`);
      cell.appendChild(cellText);
      trRow.appendChild(cell);
    }

    // add the row to the end of the table body
    tblBody.appendChild(trRow);
  }
  // put the <tbody> in the <table>
  tbl.appendChild(tblBody);
}

function setGridOptions(tbl, options) {
  options.ascSignElm = document.createElement("i");
  options.ascSignElm.anGridSorting = "asc";
  options.ascSignElm.style.cssText = `border: solid;
            border-width: 0 3px 3px 0;
            display: inline-block;
            padding: 2px;
            margin-left: 15px;
            transform: rotate(-135deg);
            -webkit-transform: rotate(-135deg);
            margin-bottom: 2px;
`;
  options.descSignElm = document.createElement("i");
  options.descSignElm.anGridSorting = "desc";
  options.descSignElm.style.cssText = `border: solid;
            border-width: 0 3px 3px 0;
            display: inline-block;
            padding: 2px;
            margin-left: 15px;
            transform: rotate(45deg);
            -webkit-transform: rotate(45deg);
            margin-bottom: 5px;
`;
  tbl.anGridOptions = options;
}

function setStyleFromElm(elmTo, elmFrom) {
  elmTo.style.cssText = elmFrom.style.cssText;
  elmTo.anGridSorting = elmFrom.anGridSorting;
}

function onClickRow(e) {
  var trg = e.target;
  var parent = trg.parentElement;
  if (parent.rowIndex > 0) return;
  var tbl = getTbl(parent);
  if (!tbl) return;
  var options = getGridOptions(parent);
  if (!options.sortingAllowed) return;
  var colIdx = trg.cellIndex;
  clearChildHeadersExcept(parent, colIdx);
  var lastChild = trg.lastChild;
  var nChlds = trg.childNodes.length;
  if (nChlds == 1) {
    lastChild = document.createElement("i");
    setStyleFromElm(lastChild, options.ascSignElm);
    //trg.appendChild(lastChild);
    trg.appendChild(lastChild);
    sortGrid(tbl, colIdx, "asc");
  } else if (nChlds == 2) {
    if (lastChild.anGridSorting == "asc") {
      sortGrid(tbl, colIdx, "desc");
      setStyleFromElm(lastChild, options.descSignElm);
    } else if (lastChild.anGridSorting == "desc") {
      sortGrid(tbl, colIdx, "asc");
      setStyleFromElm(lastChild, options.ascSignElm);
    }
  }
}

function getTbl(elm) {
  if (elm.parentElement) elm = elm.parentElement;
  if (elm.localName == "tbody") elm = elm.parentElement;
  if (elm.localName == "table") {
    return elm;
  }
  return null;
}
function getGridOptions(elm) {
  let tbl = getTbl(elm);
  return tbl.anGridOptions;
}

function clearChildHeadersExcept(header, exceptIdx) {
  let n = header.children.length;
  for (let i = 0; i < n; i++) {
    if (i == exceptIdx) continue;
    let th = header.children[i];
    var nChldsOfTh = th.childNodes.length;
    if (nChldsOfTh == 2) {
      th.removeChild(th.childNodes[1]);
    }
  }
}

function getTbRows(tb) {
  let tbChildren = tb.children;
  if (tbChildren.length == 0) return null;
  let childs = tbChildren[0];
  if (childs.localName == "tbody") childs = childs.children;
  return childs;
}

function getRowsFromGrid(tb) {
  let trs = getTbRows(tb);
  if (!trs) return;
  let rows = [];
  for (const tr of trs) {
    let trElms = tr.children;
    let trEl = trElms[0];
    if (trEl.localName == "th") {
      continue;
    }
    let nElms = trElms.length;
    let row = [];
    for (let i = 0; i < nElms; i++) {
      row.push(trElms[i].innerHTML);
    }
    rows.push(row);
  }
  return rows;
}

function sortGrid(tb, col, ascDesc) {
  let rows = getRowsFromGrid(tb);

  if (rows.length < 2) return;
  let ascDeskSign = ascDesc == "desc" ? -1 : 1;
  let comparator = (a, b) => {
    if (a.length - 1 < col) return -1 * ascDeskSign;
    if (b.length - 1 < col) return +1 * ascDeskSign;
    const aVal = NumberOrStr(a[col]);
    const bVal = NumberOrStr(b[col]);
    if (typeof aVal == "string") return ascDeskSign * (aVal > bVal ? 1 : -1);
    else if (typeof aVal == "number") return ascDeskSign * (aVal - bVal);
    else return 0;
  };
  rows.sort(comparator);
  refreshRowsInGrid(rows, tb);
}
function refreshRowsInGrid(rows, tb) {
  let trs = getTbRows(tb);
  let rIdx = -1;
  for (const tr of trs) {
    rIdx++;
    let trElms = tr.children;
    let trEl = trElms[0];
    if (trEl.localName == "th") {
      continue;
    }
    let nElms = trElms.length;
    for (let colIdx = 0; colIdx < nElms; colIdx++) {
      trElms[colIdx].innerHTML = rows[rIdx - 1][colIdx];
    }
  }
}

function NumberOrStr(val) {
  let v = Number(val);
  if (isNaN(v)) return val;
  return v;
}
