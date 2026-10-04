export function openDetails(element) {
    if (element) {
        element.open = true;
    }
}

export function focusField(id) {
    if (!id) {
        return;
    }

    const element = document.getElementById(id);
    if (element) {
        element.focus();
    }
}
