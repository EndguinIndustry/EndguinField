import Modal from "../modal.js";

document.addEventListener("DOMContentLoaded", () => {
	const eventContainer = document.getElementById("event-detail-view");
	if (!eventContainer) return;

	const config = {
		eventId: eventContainer.dataset.eventId,
		isCreateMode: eventContainer.dataset.isCreateMode === "true",
		backgroundImage: eventContainer.dataset.backgroundImage,
		backgroundImageFile: null,
		newImages: [],
		status: eventContainer.dataset.status || "Open",
	};

	initBackground();
	initToggles();
	initEditMode(config);
	initJoinButton(config);
	initBackgroundEditor(config);
	initStatusEditor(config);
	initImagesEditor(config);
	initCommentSection(config);
	initDeleteButton(config);

	if (config.isCreateMode) {
		toggleEditUI(true);
		document.querySelector(".create-event-submit-btn").addEventListener("click", () => {
			submitEvent(config, true);
		});
	} else {
		toggleEditUI(false);
	}
});

function initBackground() {
	const backgroundImage = document.getElementById("backgroundImage");
	if (!backgroundImage) return;
	const markAsLoaded = () => backgroundImage.classList.add("loaded");
	backgroundImage.complete ? markAsLoaded() : backgroundImage.addEventListener("load", markAsLoaded);
}

function initToggles() {
	const toggleButton = document.getElementById("toggleBtn");
	const detailsSection = document.getElementById("detailsSection");
	const commentsSection = document.getElementById("commentsSection");
	if (!toggleButton || !detailsSection || !commentsSection) return;

	toggleButton.addEventListener("click", () => {
		const isCommentsVisible = commentsSection.classList.toggle("hidden-section");
		detailsSection.style.display = !isCommentsVisible ? "none" : "block";

		const infoIcon = `<img src="/assets/icons/info.svg" alt="Details" class="icon-dark icon" />`;
		const commentIcon = `<img src="/assets/icons/comment.svg" alt="Comments" class="icon-dark icon" />`;

		toggleButton.innerHTML = !isCommentsVisible ? commentIcon : infoIcon;
		toggleButton.title = !isCommentsVisible ? "Show Details" : "Show Comments";
	});
}

function initEditMode(config) {
	const editButton = document.getElementById("editBtn");
	const cancelButton = document.getElementById("cancelEditBtn");
	if (!editButton) return;

	let isEditModeActive = false;
	const inputIds = [
		"editNameInput",
		"editDescriptionInput",
		"editLocationInput",
		"editStartTimeInput",
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
			await submitEvent(config, false);
		}
	});
}

function initDeleteButton(config) {
	const deleteButton = document.querySelector(".delete-event-btn");
	if (!deleteButton) return;

	deleteButton.addEventListener("click", async () => {
		const confirmed = await Modal.confirm(
			"Are you sure you want to delete this event?\nThis action cannot be undone.",
			"Delete Event",
		);

		if (confirmed) {
			try {
				const response = await fetch(`/API/Event/Delete/${config.eventId}`, {
					method: "DELETE",
				});
				const result = await response.json();

				if (response.ok && result.success) {
					await Modal.alert("Event deleted successfully.", "Deleted");
					window.location.href = "/";
				} else {
					await Modal.alert(result.message || "Failed to delete event.", "Error");
				}
			} catch {
				await Modal.alert("An error occurred while trying to delete the event.", "Error");
			}
		}
	});
}

function initJoinButton(config) {
	const joinButton = document.querySelector(".join-event-btn");
	if (!joinButton) return;

	joinButton.addEventListener("click", async () => {
		try {
			const response = await fetch(`/API/Event/Join/${config.eventId}`, {
				method: "POST",
				headers: { "Content-Type": "application/json" },
			});
			const result = await response.json();

			if (response.ok) {
				await Modal.alert(result.message, "Joined!");
				joinButton.textContent = "Requested";
				joinButton.disabled = true;
				joinButton.classList.replace("btn-success", "btn-secondary");
			} else {
				await Modal.alert(result.message || "An error occurred.", "Error");
			}
		} catch {
			await Modal.alert("An error occurred while trying to join the event.", "Error");
		}
	});
}

function initBackgroundEditor(config) {
	const editButton = document.getElementById("editBackgroundBtn");
	const fileInput = document.getElementById("backgroundImageInput");
	const backgroundPreview = document.getElementById("backgroundImage");
	if (!editButton || !fileInput || !backgroundPreview) return;

	editButton.addEventListener("click", () => fileInput.click());

	fileInput.addEventListener("change", (event) => {
		const selectedFile = event.target.files[0];
		if (!selectedFile) return;

		const fileReader = new FileReader();
		fileReader.onload = (readEvent) => {
			backgroundPreview.src = readEvent.target.result;
			config.backgroundImageFile = selectedFile;
		};
		fileReader.readAsDataURL(selectedFile);
	});
}

function initStatusEditor(config) {
	const statusInputs = document.querySelectorAll('input[name="eventStatus"]');
	statusInputs.forEach((input) => {
		input.addEventListener("change", (event) => {
			if (event.target.checked) {
				config.status = event.target.value;
			}
		});
	});
}

function initImagesEditor(config) {
	const addImageButton = document.getElementById("addImageBtn");
	const fileInput = document.getElementById("imageUploadInput");
	const imagesContainer = document.getElementById("imageListContainer");
	if (!addImageButton || !fileInput || !imagesContainer) return;

	addImageButton.addEventListener("click", () => fileInput.click());

	fileInput.addEventListener("change", (event) => {
		const selectedFiles = Array.from(event.target.files);
		selectedFiles.forEach((file) => {
			const fileReader = new FileReader();
			fileReader.onload = (readEvent) => {
				const imageItem = addImagesItem(imagesContainer, readEvent.target.result);
				imageItem.dataset.isNew = "true";
				imageItem.file = file;
				config.newImages.push(file);
			};
			fileReader.readAsDataURL(file);
		});
		fileInput.value = "";
	});

	imagesContainer.addEventListener("click", (event) => {
		const removeButton = event.target.closest(".remove-image-btn");
		if (!removeButton) return;

		const imageItem = removeButton.closest(".image-list-item");
		if (imageItem.dataset.isNew === "true") {
			config.newImages = config.newImages.filter((file) => file !== imageItem.file);
		}
		imageItem.remove();
	});
}

function initCommentSection(config) {
	const sendCommentButton = document.getElementById("sendCommentBtn");
	const commentInput = document.getElementById("newCommentInput");
	const replyWrapper = document.getElementById("replyTargetWrapper");
	const cancelReplyButton = document.getElementById("cancelReplyBtn");
	const replyTargetName = document.getElementById("replyTargetName");
	const replyTargetContent = document.getElementById("replyTargetContent");
	let activeReplyId = null;

	if (!sendCommentButton || !commentInput) return;

	// Handle reply button clicks
	document.querySelectorAll(".reply-btn").forEach((button) => {
		button.addEventListener("click", (event) => {
			const commentItem = event.target.closest(".comment-item");
			if (!commentItem) return;

			activeReplyId = commentItem.id;
			const authorName =
				commentItem.querySelector(".fw-bold")?.textContent ||
				commentItem.querySelector("h5")?.textContent ||
				"Someone";
			const commentPreview = commentItem.querySelector("p")?.textContent || "";

			replyTargetName.textContent = authorName;
			replyTargetContent.textContent = commentPreview;
			replyWrapper.classList.add("active");
			commentInput.focus();
		});
	});

	// Handle clicking a reply reference to scroll to the original comment
	document.querySelectorAll(".replied-reference").forEach((reference) => {
		reference.addEventListener("click", (event) => {
			const targetId = event.currentTarget.dataset.targetId;
			const targetElement = document.getElementById(targetId);
			if (targetElement) {
				targetElement.scrollIntoView({ behavior: "smooth", block: "center" });
				targetElement.style.backgroundColor = "#fdffcd";
				targetElement.style.border = "1px solid #fff307";
				setTimeout(() => {
					targetElement.style.backgroundColor = "";
					targetElement.style.border = "";
				}, 2000);
			}
		});
	});

	// Populate reply reference text content
	document.querySelectorAll(".replied-reference-text").forEach((textElement) => {
		const parentReference = textElement.closest(".replied-reference");
		const targetComment = document.getElementById(parentReference?.dataset.targetId);
		if (targetComment) {
			const author = targetComment.querySelector(".comment-author")?.textContent || "Unknown";
			const text = targetComment.querySelector(".comment-text")?.textContent || "";
			textElement.textContent = `Replied | ${author} - ${text}`;
		}
	});

	cancelReplyButton?.addEventListener("click", (event) => {
		event.preventDefault();
		activeReplyId = null;
		replyWrapper.classList.remove("active");
		commentInput.focus();
	});

	const submitComment = async () => {
		const content = commentInput.value.trim();
		if (!content) return;

		try {
			const response = await fetch(`/API/Event/Comment/${config.eventId}`, {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ Content: content, RepliedMessageId: activeReplyId }),
			});
			const result = await response.json();
			if (response.ok && result.success) {
				window.location.reload();
			} else {
				await Modal.alert(result.message || "An error occurred.", "Error");
			}
		} catch {
			await Modal.alert("An error occurred while sending the comment.", "Error");
		}
	};

	sendCommentButton.addEventListener("click", submitComment);
	commentInput.addEventListener("keypress", (event) => {
		if (event.key === "Enter") {
			event.preventDefault();
			submitComment();
		}
	});
}

function addImagesItem(container, source) {
	const itemDiv = document.createElement("div");
	itemDiv.className = "image-list-item position-relative mb-2";
	itemDiv.innerHTML = `
        <img src="${source}" class="w-100 rounded-3" alt="Event image" style="height: 200px; object-fit: cover;">
        <button type="button" class="remove-image-btn" title="Remove Image">&times;</button>
    `;
	container.appendChild(itemDiv);
	return itemDiv;
}

function showUploadProgressModal(titleText = "Saving...") {
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

async function submitEvent(config, isCreationMode) {
	const formData = new FormData();
	const getValue = (id) => document.getElementById(id)?.value;

	formData.append("Name", getValue("editNameInput"));
	formData.append("Description", getValue("editDescriptionInput"));
	formData.append("Location", getValue("editLocationInput"));
	formData.append("StartTime", getValue("editStartTimeInput"));
	formData.append("EndTime", getValue("editEndTimeInput"));
	formData.append("MaxParticipants", parseInt(getValue("editMaxParticipantsInput")) || 10);

	formData.append("Status", config.status);

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
		const responseResult = await new Promise((resolve, reject) => {
			const xhrRequest = new XMLHttpRequest();
			xhrRequest.open(requestMethod, requestUrl, true);

			xhrRequest.upload.onprogress = (event) => {
				if (event.lengthComputable) {
					progressIndicator.update(Math.round((event.loaded / event.total) * 100));
				}
			};

			xhrRequest.onload = () => {
				if (xhrRequest.status >= 200 && xhrRequest.status < 300) {
					resolve(JSON.parse(xhrRequest.responseText));
				} else {
					reject(new Error(xhrRequest.responseText));
				}
			};

			xhrRequest.onerror = () => reject(new Error("Network Error"));
			xhrRequest.send(formData);
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

function updateViewDisplayFromInputs() {
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

	const startTime = document.getElementById("editStartTimeInput")?.value;
	const endTime = document.getElementById("editEndTimeInput")?.value;
	const dateView = document.getElementById("viewDate")?.querySelector("p");

	if (dateView && startTime && endTime) {
		const format = (dateString) => {
			const date = new Date(dateString);
			const pad = (n) => String(n).padStart(2, "0");
			return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
		};
		dateView.textContent = `${format(startTime)} - ${format(endTime)}`;
	}
}

function toggleEditUI(isEditMode) {
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

	if (mainEditButton) {
		mainEditButton.innerHTML = isEditMode
			? `<img src="/assets/icons/save.svg" alt="Save" class="icon-dark icon" />`
			: `<img src="/assets/icons/edit.svg" alt="Edit" class="icon-dark icon" />`;
		mainEditButton.title = isEditMode ? "Save Changes" : "Edit Event";
	}
}
