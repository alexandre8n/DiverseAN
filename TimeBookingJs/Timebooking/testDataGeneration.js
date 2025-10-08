const taskNames = [
  "Learn Js",
  "Learn Html",
  "Learn Css",
  "Gui design",
  "Coding tasks",
  "Communication",
];
const projectNames = ["Time Booking", "Git Learn", "English learning"];
// function testGenTasks(taskCount) {
//   let tasks = [];
//   const effortRanges = { from: 1, to: 100 };
//   let dateRange = 100; // range from today [today-100, today+0]
//   let today = new Date();
//   for (let i = 0; i < taskCount; i++) {
//     const idxOfTask = getRandomIntFromTo(0, taskNames.length - 1);
//     let effort = getRandomIntFromTo(effortRanges.from, effortRanges.to);
//     let dat1 = getRandomDate(today, -dateRange, 0);
//     tasks.push({ taskName: taskNames[idxOfTask], effort: effort, date: dat1 });
//   }
//   return tasks;
// }
function testGenBookings(taskCount) {
  let bookings = [];
  const effortRanges = { from: 1, to: 10 };
  let dateRange = 100; // range from today [today-100, today+0]
  let today = new Date();
  for (let i = 0; i < taskCount; i++) {
    let id = generateUUIDv4();
    const idxOfTask = getRandomIntFromTo(0, taskNames.length - 1);
    const idxOfProj = getRandomIntFromTo(0, projectNames.length - 1);
    let effort = getRandomIntFromTo(effortRanges.from, effortRanges.to);
    let dat1 = getRandomDate(today, -dateRange, 0);
    bookings.push({
      id: id,
      user: user,
      project: projectNames[idxOfProj],
      task: taskNames[idxOfTask],
      effort: effort,
      date: dat1,
      comment: "default" + i,
    });
  }
  return bookings;
}
function test_AddRandomData(taskCount) {
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
    let id = generateUUIDv4();
    const idxOfPrj = getRandomIntFromTo(0, projectNames.length - 1);
    const idxOfTask = getRandomIntFromTo(0, taskNames.length - 1);
    let effort = getRandomIntFromTo(effortRanges.from, effortRanges.to);
    let dat1 = getRandomDate(today, -dateRange, 0);
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
