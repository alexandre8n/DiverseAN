export function editObject(obj, title = "Edit") {
  return new Promise((resolve) => {
    // Create modal elements
    const overlay = document.createElement("div");
    overlay.id = "modal-overlay";

    const dialog = document.createElement("div");
    dialog.id = "editObjDlg";

    const heading = document.createElement("h3");
    heading.textContent = title;
    dialog.appendChild(heading);

    const inputs = {};
    for (const key in obj) {
      const label = document.createElement("label");
      label.textContent = key.charAt(0).toUpperCase() + key.slice(1);
      const input = document.createElement("input");
      input.value = obj[key];
      inputs[key] = input;
      dialog.appendChild(label);
      dialog.appendChild(input);
    }

    const btnDone = document.createElement("button");
    btnDone.textContent = "Done";
    btnDone.onclick = () => {
      const result = {};
      for (const key in inputs) {
        result[key] = inputs[key].value;
      }
      document.body.removeChild(overlay);
      resolve(result);
    };

    const btnCancel = document.createElement("button");
    btnCancel.textContent = "Cancel";
    btnCancel.onclick = () => {
      document.body.removeChild(overlay);
      resolve(null);
    };

    dialog.appendChild(btnDone);
    dialog.appendChild(btnCancel);
    overlay.appendChild(dialog);
    document.body.appendChild(overlay);
  });
}
