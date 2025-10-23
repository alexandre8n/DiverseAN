import bcrypt from "bcrypt";

import {
  test_AddRandomData,
  testGenBookings,
  testGetProjectNames,
} from "./testDataGeneration.js";

import utl1 from "../tbShared/utl1.js";

export default class TbDataManager {
  mode = "local"; // 'local' | 'remote'
  timeBookings = []; // {id, user, project, task, comment, date, effort}
  users = []; // list of users
  projects = []; // list of projects
  constructor(mode) {
    this.mode = mode ? mode : this.mode;
    if (this.mode === "local") {
      this.initLocalData();
    }
  }

  initLocalData() {
    // Initialize local data for testing
    this.timeBookings = testGenBookings(10);

    // ⚠️ Демонстрационная «БД»
    this.users = [
      // пароль "xxx" захэширован (пример: bcrypt.hashSync("xxx", 10))
      {
        id: 1,
        username: "user1",
        passwordHash: bcrypt.hashSync("xxx", 10),
        role: "user",
      },
      {
        id: 2,
        username: "admin",
        passwordHash: bcrypt.hashSync("admin", 10),
        role: "admin",
      },
    ];

    this.projects = testGetProjectNames();
  }

  getUsers() {
    return this.users;
  }
  getUserByName(userName) {
    const user = this.users.find((u) => u.username === userName);
    return user;
  }
  getUserById(id) {
    const user = this.users.find((u) => u.id === id);
    return user;
  }
  getTbRecords(fr, to, user) {
    const recs = this.timeBookings.filter(
      (x) => utl1.isDateInRange(x.date, fr, to) && x.user == user.username
    );
    return recs;
  }
  getTbRecordById(id, username) {
    const rec = this.timeBookings.find((x) => x.id == id);
    if (!username || (rec && rec.user === username)) {
      return rec;
    }
    return null;
  }
  getUserProjects(user) {
    return this.projects;
  }
  addProject(projName, user) {
    // user is for future use
    if (this.projects.includes(projName)) {
      return null; // already exists
    }
    this.projects.push(projName);
    return projName;
  }
  addTbRec(tbRec, user) {
    // user is for future use
    tbRec.id = utl1.generateUUIDv4();
    const clonedObj = utl1.cloneObj(tbRec);
    this.timeBookings.push(clonedObj);
    return tbRec;
  }
  updateTbRec(tbRec, user) {
    const index = this.timeBookings.findIndex((x) => x.id === tbRec.id);
    const rec = index !== -1 ? this.timeBookings[index] : null;
    const isAllowed = !userName || (rec && rec.user === user.username);
    if (rec && isAllowed) {
      const clonedObj = utl1.cloneObj(tbRec);
      this.timeBookings[index] = clonedObj;
      return tbRec;
    }
    return null;
  }
  deleteTbRec(tbRec, user) {
    const index = this.timeBookings.findIndex((x) => x.id === tbRec.id);
    const rec = index !== -1 ? this.timeBookings[index] : null;
    const isAllowed = !user || (rec && rec.user === user.username);
    if (rec && isAllowed) {
      this.timeBookings.splice(index, 1);
      return tbRec;
    }
    return null;
  }
}
