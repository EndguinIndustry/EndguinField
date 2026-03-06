import Modal from "../../Shared/modal.js";
import { request } from "../../Shared/utils.js";
import { toggleEditUI, updateViewDisplayFromInputs } from "./ui.js";

export function setupTagsInteraction() {
    const tagButtons = document.querySelectorAll(".event-tag-btn");

    tagButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            if (btn.classList.contains("selected")) {
                btn.classList.remove("bg-primary", "text-white", "selected");
                btn.classList.add("bg-secondary", "text-light");
            } else {
                btn.classList.add("bg-primary", "text-white", "selected");
                btn.classList.remove("bg-secondary", "text-light");
            }
        });
    });
}

function setupDateValidation() {
    const startDateInput = document.getElementById("editStartDateInput");
    const startTimeInput = document.getElementById("editStartTimeInput");
    const endDateInput = document.getElementById("editEndDateInput");
    const endTimeInput = document.getElementById("editEndTimeInput");

    const validateDates = async () => {
        if (!startDateInput || !startTimeInput || !endDateInput || !endTimeInput) return;

        const startDate = startDateInput.value;
        const startTime = startTimeInput.value;
        const endDate = endDateInput.value;
        const endTime = endTimeInput.value;

        if (startDate && startTime && endDate && endTime) {
            const startDateTime = new Date(`${startDate}T${startTime}`);
            const endDateTime = new Date(`${endDate}T${endTime}`);

            if (endDateTime <= startDateTime) {
                await Modal.alert("End date and time must be after the start date and time.", "Invalid Date/Time");
            }
        }
    };

    [startDateInput, startTimeInput, endDateInput, endTimeInput].forEach(input => {
        if (input) {
            input.addEventListener("blur", validateDates);
        }
    });
}

export function initEditMode(config) {
    if (config.isCreateMode) {
        setupTagsInteraction();
        setupDateValidation();
        toggleEditUI(true);
        const submitBtn = document.querySelector(".create-event-submit-btn");
        if (submitBtn) {
            submitBtn.addEventListener("click", () => submitEvent(config, true));
        }
        return;
    }

    const disabledStatuses = ["ended", "cancelled"];
    if (disabledStatuses.includes(config.status?.toLowerCase())) {
        toggleEditUI(false);
        return;
    }

    setupTagsInteraction();
    setupDateValidation();
    toggleEditUI(false);

    const editButton = document.getElementById("editBtn");
    const cancelButton = document.getElementById("cancelEditBtn");
    if (!editButton) return;

    let isEditModeActive = false;
    const inputIds = [
        "editNameInput",
        "editDescriptionInput",
        "editLocationInput",
        "editStartDateInput",
        "editStartTimeInput",
        "editEndDateInput",
        "editEndTimeInput",
        "editMaxParticipantsInput",
    ];
    const inputSnapshot = {};

    cancelButton.addEventListener("click", () => {
        inputIds.forEach((id) => {
            const inputElement = document.getElementById(id);
            if (inputElement) {
                inputElement.value = inputSnapshot[id] ?? inputElement.value;
            }
        });
        isEditModeActive = false;
        isEditModeActive = false;
        toggleEditUI(false);
    });


    editButton.addEventListener("click", async () => {
        if (!isEditModeActive) {
            inputIds.forEach((id) => {
                const inputElement = document.getElementById(id);
                if (inputElement) inputSnapshot[id] = inputElement.value;
            });
        }

        isEditModeActive = !isEditModeActive;
        toggleEditUI(isEditModeActive);

        if (!isEditModeActive) {
            if (config.status?.toLowerCase() === "cancelled") {
                const confirmed = await Modal.confirm(
                    "Setting the status to Cancelled will prevent users from joining or managing participation. Continue?",
                    "Cancel this event?"
                );
                if (!confirmed) {
                    config.status = "Open";
                    const openInput = document.querySelector('input[name="eventStatus"][value="Open"]');
                    if (openInput) openInput.click();
                    isEditModeActive = true;
                    toggleEditUI(true);
                    return;
                }
            }
            await submitEvent(config, false);
        }
    });
}

export function showUploadProgressModal(titleText = "Saving...") {
    const progressContent = document.createElement("div");
    progressContent.innerHTML = `
        <div class="upload-progress-item">
            <div class="upload-progress-info">
                <span>Total Progress</span>
                <span id="total-percent">0%</span>
            </div>
            <div class="progress-bar-container">
                <div class="progress-bar-fill" id="total-progress-bar"></div>
            </div>
        </div>
    `;
    Modal.show({ titleText, customContent: progressContent });

    return {
        update: (percentage) => {
            const progressBar = document.getElementById("total-progress-bar");
            const percentText = document.getElementById("total-percent");
            if (progressBar) progressBar.style.width = `${percentage}%`;
            if (percentText) percentText.textContent = `${percentage}%`;
        },
        markComplete: () => {
            document.getElementById("total-progress-bar")?.classList.add("success");
        },
        markError: () => {
            document.getElementById("total-progress-bar")?.classList.add("error");
        },
    };
}

export async function submitEvent(config, isCreationMode) {
    const formData = new FormData();
    const getValue = (id) => document.getElementById(id)?.value;

    formData.append("Name", getValue("editNameInput"));
    formData.append("Description", getValue("editDescriptionInput"));
    formData.append("Location", getValue("editLocationInput"));

    const startDate = getValue("editStartDateInput");
    const startTime = getValue("editStartTimeInput");
    const endDate = getValue("editEndDateInput");
    const endTime = getValue("editEndTimeInput");

    if (startDate && startTime && endDate && endTime) {
        const startDateTime = new Date(`${startDate}T${startTime}`);
        const endDateTime = new Date(`${endDate}T${endTime}`);

        if (endDateTime <= startDateTime) {
            await Modal.alert("Please fix the end date and time before saving.", "Invalid Date/Time");
            return;
        }
    }

    if (startDate && startTime) {
        formData.append("StartTime", `${startDate}T${startTime}`);
    }

    if (endDate && endTime) {
        formData.append("EndTime", `${endDate}T${endTime}`);
    }

    formData.append("MaxParticipants", parseInt(getValue("editMaxParticipantsInput")) || 10);

    formData.append("Status", config.status);

    const selectedTagButtons = document.querySelectorAll(".event-tag-btn.selected");
    selectedTagButtons.forEach(btn => {
        formData.append("Tags", btn.dataset.tagValue);
    });

    if (config.backgroundImageFile) {
        formData.append("BackgroundImageFile", config.backgroundImageFile);
    } else {
        formData.append("BackgroundImage", config.backgroundImage);
    }

    if (!isCreationMode) {
        const imagsContainer = document.getElementById("imageListContainer");
        imagsContainer?.querySelectorAll(".image-list-item").forEach((item) => {
            if (item.dataset.isNew !== "true") {
                const imageSource = item.querySelector("img")?.src;
                if (imageSource) formData.append("ExistingImages", imageSource);
            }
        });
    }

    config.newImages.forEach((file) => formData.append("NewImages", file));

    const progressIndicator = showUploadProgressModal(isCreationMode ? "Creating Event..." : "Saving Changes...");
    const requestUrl = isCreationMode ? "/API/Event/Create" : `/API/Event/Update/${config.eventId}`;
    const requestMethod = isCreationMode ? "POST" : "PUT";

    try {
        const responseResult = await request(requestUrl, {
            method: requestMethod,
            body: formData,
            isFormData: true,
            onProgress: (event) => {
                if (event.lengthComputable) {
                    progressIndicator.update(Math.round((event.loaded / event.total) * 100));
                }
            }
        });

        if (responseResult.success) {
            progressIndicator.markComplete();
            if (isCreationMode) {
                setTimeout(() => {
                    window.location.href = responseResult.redirectUrl;
                }, 500);
            } else {
                updateViewDisplayFromInputs();
                setTimeout(() => Modal.close(), 1000);
                await Modal.alert(responseResult.message || "Changes saved successfully!", "Success");
                window.location.reload();
            }
        } else {
            progressIndicator.markError();
            await Modal.alert(responseResult.message || "Failed to process request.", "Error");
        }
    } catch (error) {
        progressIndicator.markError();
        await Modal.alert(error.message || "An error occurred.", "Error");
    }
}
