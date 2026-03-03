export function initStatusEditor(config) {
    const statusInputs = document.querySelectorAll('input[name="eventStatus"]');
    statusInputs.forEach((input) => {
        input.addEventListener("change", (event) => {
            if (event.target.checked) {
                config.status = event.target.value;
            }
        });
    });
}
