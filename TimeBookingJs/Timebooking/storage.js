let mainKey = "TimeBooking";
let mainProjKey = "TimeBookingProj";
let isDataLoading = false;
let globalRecords = null;
let globalProjects = null;
let testDateRangeDays = 200;

function _saveObj(obj) {
  const strJson = JSON.stringify(obj);
  localStorage.setItem(mainKey + "." + user + "." + obj.id, strJson);
}
function _getKeys() {
  let keyArray = [];
  for (let i = 0; i < localStorage.length; i++) {
    let key = localStorage.key(i);
    keyArray.push(key);
  }
  return keyArray;
}
function _getRecById(id) {
  let idRec = mainKey + "." + user + "." + id;
  let string = localStorage.getItem(idRec);
  let obj = JSON.parse(string);
  return obj;
}
function _addProj(proj) {
  let proestNames = getProjectsNames();
  if (proestNames.indexOf(proj.name) >= 0) return false;

  const strJson = JSON.stringify(proj);
  localStorage.setItem(mainProjKey + "." + proj.id, strJson);
  return true;
}

function _getProjNames() {
  let keys = getKeys();
  let goodKeys = keys.filter((key) => key.startsWith(mainProjKey + "."));
  let projNames = goodKeys.map(
    (el) => JSON.parse(localStorage.getItem(el)).name
  );
  return projNames;
}

function _getUserRecs(user) {
  let keys = getKeys();
  let goodKeys = keys.filter((key) =>
    key.startsWith(mainKey + "." + user + ".")
  ); //startsWith стандартна функція
  let objs = [];
  for (let x of goodKeys) {
    let string = localStorage.getItem(x);
    let obj = JSON.parse(string);
    objs.push(obj);
  }
  return objs;
}
function _clearAll() {
  localStorage.clear();
}
function _deleteRecFromDB(recId) {
  let key = mainKey + "." + user + "." + recId;
  localStorage.removeItem(key);
}
//////////////////////////////////////
function clearUserData(u) {
  user = "";
  globalRecords = null;
  globalProjects = null;
}
function reloadTbRecords(dtFr, dtTo) {
  if (!globalRecords) {
    globalRecords = isDataLoading ? _getUserRecs(user) : test_AddRandomData(80);
  }
  if (!dtFr) dtFr = new Date("1900-01-01");
  if (!dtTo) dtTo = new Date("2900-01-01");

  const recs = globalRecords.filter((x) => isDateInRange(x.date, dtFr, dtTo));
  return recs;
}

function loadDayBookings(dt) {
  if (!globalRecords) {
    globalRecords = isDataLoading ? _getUserRecs(user) : test_AddRandomData(80);
  }
  let dayRecs = globalRecords.filter((x) => x.date == dt);
  return dayRecs;
}

function getBookingRecordById(id) {
  if (isDataLoading) {
    return _getRecById(id);
  }
  if (!globalRecords) {
    globalRecords = isDataLoading ? _getUserRecs(user) : test_AddRandomData(80);
  }
  let tbRec = globalRecords.find((x) => x.id == id);
  return cloneObj(tbRec);
}

function addTbRecord(rec) {
  if (isDataLoading) {
    return _saveObj(rec);
  }
  if (!globalRecords) {
    globalRecords = isDataLoading ? _getUserRecs(user) : test_AddRandomData(80);
  }
  rec.id = generateUUIDv4();
  globalRecords.push(cloneObj(rec));
  console.log("addTbRecord", rec.id, rec.task, rec.date);
  return true;
}

function updateTbRecord(rec) {
  if (isDataLoading) {
    return _saveObj(rec);
  }
  let tbRec = globalRecords.find((x) => x.id == rec.id);
  tbRec.date = rec.date;
  tbRec.project = rec.project;
  tbRec.task = rec.task;
  tbRec.effort = rec.effort;
  tbRec.comment = rec.comment;
}

function deleteTbRecordById(id) {
  if (isDataLoading) {
    return _deleteRecFromDB(id);
  }
  console.log("trying to delete", id);

  let idx = globalRecords.findIndex((r) => r.id == id);
  if (idx >= 0) globalRecords.splice(idx, 1);
}

function getProjectsNames() {
  if (isDataLoading) {
    return _getProjNames();
  }
  return globalProjects;
}

function addProjectName(name) {
  if (isDataLoading) {
    return _addProj({ name: name });
  }
  if (!globalProjects) globalProjects = [];
  if (globalProjects.includes(name)) return false;
  globalProjects.push(name);
  return true;
}
