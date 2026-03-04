import { initBackground, initToggles, initDateReskin, initCharacterCounters } from "./modules/ui.js";
import { initEditMode } from "./modules/editHandler.js";
import { initDeleteButton, initJoinButton, initQuitButton, initCancelJoinButton, initParticipantControls } from "./modules/actions.js";
import { initBackgroundEditor, initImagesEditor } from "./modules/mediaEditor.js";
import { initCommentSection } from "./modules/commentsEditor.js";
import { initStatusEditor } from "./modules/statusEditor.js";

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
	initDateReskin();
	initCharacterCounters();
	initEditMode(config);
	initJoinButton(config);
	initCancelJoinButton(config);
	initQuitButton(config);
	initParticipantControls(config);
	initBackgroundEditor(config);
	initStatusEditor(config);
	initImagesEditor(config);
	initCommentSection(config);
	initDeleteButton(config);
});
