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
}
