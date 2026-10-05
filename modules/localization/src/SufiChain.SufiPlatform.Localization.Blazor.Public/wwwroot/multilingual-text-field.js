export function focus(id) {
  const element = document.getElementById(id);
  if (element && typeof element.focus === "function") {
    element.focus();
  }
}
