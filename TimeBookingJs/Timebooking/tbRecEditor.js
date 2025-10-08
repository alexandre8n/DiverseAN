class TBRecEditor {
  constructor(settings) {
    this.container = settings.container;
    //this.projListGetter = settings.projListGetter;
    this.setRecord(settings.record);
    this.setOnSubmitClick(settings.onTbRecSubmitClick);
    this.setOnCancelClick(settings.onTbRecCancelClick);
    this.onProjChange = this.onProjChange.bind(this);
    this.onCancelNewProj = this.onCancelNewProj.bind(this);
    this.onAddNewPrjDone = this.onAddNewPrjDone.bind(this);
    if (settings.date) this.setDate(settings.date);
    this.setProjectListGetterSaver(
      settings.projListGetter,
      settings.newProjectSaver
    );
    this.setTaskListGetter(settings.taskListGetter);
    const prj = this.getProjElm();
    prj.onchange = this.onProjChange;

    // add task select is clicked event to button
    const taskInp = this.getTaskElm();
    const taskSelButton = taskInp.nextElementSibling;
    taskSelButton.onclick = this.onTaskSelClick.bind(this);
  }

  onSubmit(evt) {
    const rec = this.getTbRec();
    if (this.onTbRecSubmitClick) {
      this.onTbRecSubmitClick(rec);
    }
  }
  onCancel(evt) {
    const rec = this.getTbRec();
    if (this.onTbRecCancelClick) {
      this.onTbRecCancelClick(rec);
    }
  }
  onProjChange(evt) {
    if (evt.target.value == "AddNew") {
      const settings = {
        dialogClass: "addNewDlg",
        onCancel: this.onCancelNewProj,
        onDone: this.onAddNewPrjDone,
        placeholderMsg: "Specify project name ...",
        buttonText: "Done...",
      };
      addNewDlgOpen(settings);
    }
  }

  onCancelNewProj() {
    const selectElement = this.getProjElm();
    if (selectElement.selectedIndex == 0) selectElement.selectedIndex = -1;
    return true;
  }

  onAddNewPrjDone(input) {
    const inpVal = input.value.trim();
    if (!this.projListGetter) return;
    const projectNames = this.projListGetter();
    if (projectNames.includes(inpVal)) {
      showTooltip(input, "tooltip", "This project already exists");
      return false;
    } else if (inpVal == "") {
      showTooltip(input, "tooltip", "Project name is empty");
      return false;
    }
    this.updateProj(inpVal);
    if (this.newProjectSaver) this.newProjectSaver(inpVal);
    return true;
  }

  updateProj(projName) {
    let selectElement = this.getProjElm();
    addOptionsToSelect(selectElement, [projName], projName);
  }

  onTaskSelClick(evt) {
    const onTaskDone = this.onTaskSelDone.bind(this);
    onSelectTask(onTaskDone, null, this.taskListGetter);
  }

  onTaskSelDone(taskName) {
    const taskInput = this.getTaskElm();
    taskInput.value = taskName;
  }

  getTbRec() {
    if (!this.record) this.record = {};
    this.record.date = this.getDateElm().value;
    const selElm = this.getProjElm();
    const txt = selElm.options[selElm.selectedIndex].text;
    this.record.project = txt;
    this.record.task = this.getTaskElm().value;
    this.record.effort = parseFloat(this.getEffortElm().value);
    this.record.comment = this.getCommentElm().value;
    return cloneObj(this.record);
  }

  getDateElm() {
    const div = this.container;
    const dateElm = div.querySelector('input[name="inputDate"]');
    return dateElm;
  }
  getProjElm() {
    const div = this.container;
    return div.querySelector('select[name="cmbProj"]');
  }
  getTaskElm() {
    const div = this.container;
    return div.querySelector('input[name="inputTask"]');
  }
  getEffortElm() {
    const div = this.container;
    return div.querySelector('input[name="inputEffort"]');
  }
  getCommentElm() {
    const div = this.container;
    return div.querySelector('textarea[name="inputComment"]');
  }
  getSubmitButtonElm() {
    const div = this.container;
    return div.querySelector('button[name="btnSubmit"]');
  }
  getCancelButtonElm() {
    const div = this.container;
    return div.querySelector('button[name="btnCancel"]');
  }

  setDate(date) {
    if (!this.record) {
      this.record = {};
    }
    this.record.date = date;
    const dateElm = this.getDateElm();
    dateElm.value = dateToStdStr(date);
  }

  setProject(proj) {
    if (!this.record) {
      this.record = {};
    }
    this.record.project = proj;
    this.updateProj(proj);
  }
  setTask(task) {
    if (!this.record) {
      this.record = {};
    }
    this.record.task = task;
    const elm = this.getTaskElm();
    elm.value = task;
  }
  setEffort(effort) {
    if (!this.record) {
      this.record = {};
    }
    this.record.effort = effort;
    const elm = this.getEffortElm();
    elm.value = effort;
  }
  setComment(comment) {
    if (!this.record) {
      this.record = {};
    }
    this.record.comment = comment;
    const elm = this.getCommentElm();
    elm.value = comment;
  }

  setProjectListGetterSaver(projListGetter, newProjSaver) {
    this.projListGetter = projListGetter;
    this.newProjectSaver = newProjSaver;
    if (!this.projListGetter) return;
    const projElm = this.getProjElm();
    const selTxt = getSelectedText(projElm);
    const projs = this.projListGetter();
    addOptionsToSelect(projElm, projs, selTxt);
  }

  setTaskListGetter(taskListGetter) {
    // this method returns the list of tasks
    this.taskListGetter = taskListGetter;
  }

  setRecord(rec) {
    this.record = rec;
    if (!this.record) return;
    this.setDate(this.record.date);
    this.setProject(this.record.project);
    this.setTask(this.record.task);
    this.setEffort(this.record.effort);
    this.setComment(this.record.comment);
  }

  setOnSubmitClick(onTbRecSubmitClick) {
    this.onTbRecSubmitClick = onTbRecSubmitClick;
    const btn = this.getSubmitButtonElm();
    this.onSubmit = this.onSubmit.bind(this);
    btn.onclick = this.onSubmit;
  }
  setOnCancelClick(onCancel) {
    if (!onCancel) {
      this.onTbRecCancelClick = null;
      const btn = this.getCancelButtonElm();
      btn.style.display = "none";
      return;
    }
    this.onTbRecCancelClick = onCancel;
    const btn = this.getCancelButtonElm();
    this.onCancel = this.onCancel.bind(this);
    btn.onclick = this.onCancel;
  }

  validate(data) {
    return data.date && data.project && data.task && !isNaN(data.effort);
  }
  destroy() {}
}
