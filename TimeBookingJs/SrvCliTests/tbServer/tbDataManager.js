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

    //  Демонстрационная «БД»
    this.users = [
      // пароль "xxx" захэширован (пример: bcrypt.hashSync("xxx", 10))
      {
        id: 1,
        username: "user1",
        passwordHash: bcrypt.hashSync("xxx", 10),
        role: "user",
        isActive: true,
      },
      {
        id: 2,
        username: "admin",
        passwordHash: bcrypt.hashSync("admin", 10),
        role: "admin",
        isActive: true,
      },
    ];

    this.projects = testGetProjectNames();
  }

  getUsers() {
    return this.users;
  }
  addUser(userToAdd) {
    const existingUser = this.users.find(
      (u) => u.username === userToAdd.username
    );
    if (existingUser) return null; // User already exists
    const user = utl1.cloneObj(userToAdd);
    // delete attribute user.password; // remove plain password
    delete user.password;

    user.id = this.users.length + 1;
    user.passwordHash = bcrypt.hashSync(userToAdd.password, 10);
    this.users.push(user);
    return user;
  }

  updateUser(userToUpdate) {
    const index = this.users.findIndex(
      (u) => u.username === userToUpdate.username
    );
    if (index === -1) return null; // User not found
    const user = utl1.cloneObj(userToUpdate);
    // delete attribute user.password; // remove plain password
    delete user.password;
    if (userToUpdate.password == "ignoredUserObj.password") {
      user.passwordHash = this.users[index].passwordHash;
    } else {
      user.passwordHash = bcrypt.hashSync(userToUpdate.password, 10);
    }
    this.users[index] = user;
    return user;
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
  getTbRecordById(id, user) {
    const rec = this.timeBookings.find((x) => x.id == id);
    const isAllowed = !user || (rec && rec.user === user.username);
    return isAllowed ? rec : null;
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
    const isAllowed = !user || (rec && rec.user === user.username);
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
