import {
  test_AddRandomData,
  testGenBookings,
  testGetProjectNames,
} from "./testDataGeneration.js";
import utl1 from "./utl1.js";

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

    this.users = [
      {
        id: 1,
        username: "user1",
        passwordHash: "hashed_password_1",
        role: "user",
      },
      {
        id: 2,
        username: "user2",
        passwordHash: "hashed_password_2",
        role: "user",
      },
    ];

    this.projects = testGetProjectNames();
  }
  getTbRecords(fr, to, user) {
    const recs = this.timeBookings.filter(
      (x) => utl1.isDateInRange(x.date, fr, to) && x.user == user.username
    );

    return recs;
  }
}
