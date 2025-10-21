import utl1 from "../tbShared/utl1.js";

const taskNames = [
  "Learn Js",
  "Learn Html",
  "Learn Css",
  "Gui design",
  "Coding tasks",
  "Communication",
];
const projectNames = ["Time Booking", "Git Learn", "English learning"];
const users = ["user1", "user2", "user3"];

export function testGetProjectNames() {
  return projectNames;
}

export function testGenBookings(taskCount) {
  let bookings = [];
  const effortRanges = { from: 1, to: 10 };
  let dateRange = Math.ceil(taskCount * 1.5); // range from today [today-dateRange, today+0]
  let today = new Date();
  for (let i = 0; i < taskCount; i++) {
    let id = utl1.generateUUIDv4();
    const idxOfTask = utl1.getRandomIntFromTo(0, taskNames.length - 1);
    const idxOfProj = utl1.getRandomIntFromTo(0, projectNames.length - 1);
    const idxOfUser = utl1.getRandomIntFromTo(0, users.length - 1);
    let effort = utl1.getRandomIntFromTo(effortRanges.from, effortRanges.to);
    let dat1 = utl1.getRandomDate(today, -dateRange, 0);
    bookings.push({
      id: id,
      user: users[idxOfUser],
      project: projectNames[idxOfProj],
      task: taskNames[idxOfTask],
      effort: effort,
      date: dat1,
      comment: "default" + i,
    });
  }
  return bookings;
}
export function test_AddRandomData(taskCount) {
  let timeBookings = [];
  const projectNames = [
    "General-Activities",
    "Timebooking Prj",
    "Database-connect",
    "GitHub Mastering",
  ];

  globalProjects = projectNames;

  const taskNames = [
    "Learn Js",
    "Learn Html",
    "Learn Css",
    "Gui design",
    "Coding Tasks",
    "Communication",
  ];

  const effortRanges = { from: 1, to: 12 };
  // range from today [today-100, today+0]
  let dateRange = testDateRangeDays;
  let today = new Date();
  // rec: id,project,task,comment,date,effort

  for (let i = 0; i < taskCount; i++) {
    let id = utl1.generateUUIDv4();
    const idxOfPrj = utl1.getRandomIntFromTo(0, projectNames.length - 1);
    const idxOfTask = utl1.getRandomIntFromTo(0, taskNames.length - 1);
    let effort = utl1.getRandomIntFromTo(effortRanges.from, effortRanges.to);
    let dat1 = utl1.getRandomDate(today, -dateRange, 0);
    timeBookings.push({
      id: id,
      user: user,
      project: projectNames[idxOfPrj],
      task: taskNames[idxOfTask],
      effort: effort,
      date: dat1,
      comment: "comment to booking " + id,
    });
  }
  return timeBookings;
}
