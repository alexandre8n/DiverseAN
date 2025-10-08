let dateRange = { from: null, to: null };
function saveGlobalDateRange(fr, to) {
  dateRange.from = new Date(fr);
  dateRange.to = new Date(to);
}
function loadTasks() {
  return reloadTbRecords(dateRange.from, dateRange.to);
}
function loadTbRecEditDivFromTemplate() {
  const tbEditFormTemplate = document.getElementById("editTBRecordTemplate");
  const editFormDivCont = tbEditFormTemplate.content.cloneNode(true);
  const editFormDiv = editFormDivCont.querySelector(".div4TimeBookingEdit");
  return editFormDiv;
}
function loadTbRecEditDivFromTemplate2(headText) {
  const tbEditFormTemplate = document.getElementById("editTBRecordTemplate");
  const editFormDivCont = tbEditFormTemplate.content.cloneNode(true);
  const editFormDivHead = editFormDivCont.querySelector(".BookingEditHeader");
  if (headText) editFormDivHead.innerHTML = headText;
  const editFormDiv = editFormDivCont.querySelector(".div4TimeBookingEdit");
  return [editFormDivHead, editFormDiv];
}
