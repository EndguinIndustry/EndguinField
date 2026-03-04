import { addEventIfElementExists } from "../../Shared/utils.js";

export function initBackground() {
    const backgroundImage = document.getElementById("backgroundImage");
    if (!backgroundImage) return;
    const markAsLoaded = () => backgroundImage.classList.add("loaded");
    backgroundImage.complete ? markAsLoaded() : backgroundImage.addEventListener("load", markAsLoaded);
}

export function initToggles() {
    const detailsSection = document.getElementById("detailsSection");
    const commentsSection = document.getElementById("commentsSection");

    if (!detailsSection || !commentsSection) return;

    addEventIfElementExists("#toggleBtn", "click", (event) => {
        const toggleButton = event.currentTarget;
        const isCommentsVisible = commentsSection.classList.toggle("hidden-section");
        detailsSection.style.display = !isCommentsVisible ? "none" : "block";

        const infoIcon = `<img src="/assets/icons/info.svg" alt="Details" class="icon-dark icon" />`;
        const commentIcon = `<img src="/assets/icons/comment.svg" alt="Comments" class="icon-dark icon" />`;

        toggleButton.innerHTML = !isCommentsVisible ? commentIcon : infoIcon;
        toggleButton.title = !isCommentsVisible ? "Show Details" : "Show Comments";
    });
}

export function toggleEditUI(isEditMode) {
    const viewElementIds = ["viewName", "viewDescription", "viewLocation", "viewDate", "eventImageCarousel"];
    const viewElements = [
        ...viewElementIds.map((id) => document.getElementById(id)),
        ...document.querySelectorAll(".view-mode-only"),
    ].flat();

    const editElements = document.querySelectorAll(".edit-mode-only");
    const mainEditButton = document.getElementById("editBtn");

    if (isEditMode) {
        const comments = document.getElementById("commentsSection");
        const details = document.getElementById("detailsSection");
        const toggleButton = document.getElementById("toggleBtn");

        if (comments && !comments.classList.contains("hidden-section")) {
            comments.classList.add("hidden-section");
            if (details) details.style.display = "block";
            if (toggleButton) {
                toggleButton.innerHTML = `<img src="/assets/icons/comment.svg" alt="Comments" class="icon-dark icon" />`;
                toggleButton.title = "Show Comments";
            }
        }
    }

    viewElements.forEach((element) => {
        if (element) element.style.display = isEditMode ? "none" : "";
    });
    editElements.forEach((element) => {
        if (element) element.style.display = isEditMode ? "" : "none";
    });

    if (isEditMode) {
        window.dispatchEvent(new Event("resize"));
    }

    if (mainEditButton) {
        mainEditButton.innerHTML = isEditMode
            ? `<img src="/assets/icons/save.svg" alt="Save" class="icon-dark icon" />`
            : `<img src="/assets/icons/edit.svg" alt="Edit" class="icon-dark icon" />`;
        mainEditButton.title = isEditMode ? "Save Changes" : "Edit Event";
    }
}

export function updateViewDisplayFromInputs() {
    const fieldMappings = [
        ["viewName", "editNameInput"],
        ["viewDescription", "editDescriptionInput"],
        ["viewLocation", "editLocationInput"],
    ];

    fieldMappings.forEach(([viewId, inputId]) => {
        const viewElement = document.getElementById(viewId);
        const inputElement = document.getElementById(inputId);
        if (viewElement && inputElement) {
            viewElement.textContent = inputElement.value;
        }
    });

    const startDate = document.getElementById("editStartDateInput")?.value;
    const startTime = document.getElementById("editStartTimeInput")?.value;
    const endDate = document.getElementById("editEndDateInput")?.value;
    const endTime = document.getElementById("editEndTimeInput")?.value;
    const dateView = document.getElementById("viewDate")?.querySelector("p");

    if (dateView && startDate && startTime && endDate && endTime) {
        const format = (dateStr, timeStr) => {
            const date = new Date(`${dateStr}T${timeStr}`);
            const pad = (n) => String(n).padStart(2, "0");
            return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
        };
        dateView.textContent = `${format(startDate, startTime)} - ${format(endDate, endTime)}`;
    }
}

export function initDateReskin() {
    const reskinContainers = document.querySelectorAll(".date-picker-overlay");
    reskinContainers.forEach(container => {
        const input = container.querySelector('input[type="date"]');
        const display = container.querySelector('.date-display');
        if (!input || !display) return;

        const updateDisplay = () => {
            if (!input.value) {
                display.textContent = "dd/mm/yyyy";
                return;
            }
            const [year, month, day] = input.value.split('-');
            display.textContent = `${day}/${month}/${year}`;
        };

        input.addEventListener('change', updateDisplay);
        input.addEventListener('input', updateDisplay);
        updateDisplay(); // initial call
    });
}

export function initCharacterCounters() {
    const limits = {
        "editNameInput": { countId: "nameCharCount", max: 100 },
        "editDescriptionInput": { countId: "descriptionCharCount", max: 2000 },
        "editLocationInput": { countId: "locationCharCount", max: 500 }
    };

    Object.entries(limits).forEach(([inputId, { countId, max }]) => {
        const input = document.getElementById(inputId);
        const counter = document.getElementById(countId);
        if (!input || !counter) return;

        const updateCounter = () => {
            const length = input.value.length;
            counter.textContent = length;

            const container = counter.closest('.char-counter');
            if (container) {
                container.classList.toggle('warning', length > max * 0.8 && length <= max);
                container.classList.toggle('danger', length > max);
            }
        };

        input.addEventListener('input', updateCounter);
        input.addEventListener('change', updateCounter);
        updateCounter();
    });
}
