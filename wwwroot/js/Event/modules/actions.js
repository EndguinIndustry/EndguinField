import Modal from "../../Shared/modal.js";
import { request, addEventIfElementExists } from "../../Shared/utils.js";

export function initDeleteButton(config) {
    addEventIfElementExists(".delete-event-btn", "click", async () => {
        const confirmed = await Modal.confirm(
            "Are you sure you want to delete this event?\nThis action cannot be undone.",
            "Delete Event",
        );

        if (confirmed) {
            try {
                const result = await request(`/API/Event/Delete/${config.eventId}`, { method: "DELETE" });
                if (result.success) {
                    await Modal.alert("Event deleted successfully.", "Deleted");
                    window.location.href = "/";
                } else {
                    await Modal.alert(result.message || "Failed to delete event.", "Error");
                }
            } catch (error) {
                await Modal.alert(error.message || "An error occurred while trying to delete the event.", "Error");
            }
        }
    });
}

export function initQuitButton(config) {
    const container = document.querySelector(".join-actions-container");
    if (!container) return;

    container.addEventListener("click", async (event) => {
        const quitBtn = event.target.closest(".quit-event-btn");
        if (!quitBtn) return;

        const confirmResult = await Modal.confirm("Are you sure you want to quit this event?", "Quit Event");
        if (!confirmResult) return;

        try {
            const result = await request(`/API/Event/Quit/${config.eventId}`, { method: "DELETE" });
            if (result.success) {
                await Modal.alert(result.message || "Successfully quit the event.", "Success");
                container.innerHTML = "";
                container.appendChild(createJoinButton());

                // If the user was in the list, we might want to remove them. 
                // However, since we don't know the current user's ID here conveniently, 
                // and they might not even be in the visible part of the list, 
                // a reload might actually be safer for "Quit" if we want to be 100% accurate.
                // But the user asked for "don't reload", so I'll try to find them if possible.
                // For now, at least swipe the button.
            } else {
                await Modal.alert(result.message || "Failed to quit the event.", "Error");
            }
        } catch (error) {
            await Modal.alert(error.message || "An error occurred.", "Error");
        }
    });
}

function updateParticipantCount(delta) {
    const countEl = document.getElementById("participantCount");
    if (countEl) {
        const currentCount = parseInt(countEl.textContent) || 0;
        countEl.textContent = currentCount + delta;
    }
}

function createCancelButton() {
    const btn = document.createElement("button");
    btn.className = "btn btn-secondary cancel-join-btn";
    btn.type = "button";
    btn.textContent = "Cancel Request";
    return btn;
}

function createJoinButton() {
    const btn = document.createElement("button");
    btn.className = "btn btn-primary join-event-btn";
    btn.type = "button";
    btn.textContent = "Join Event";
    return btn;
}

export function initJoinButton(config) {
    const container = document.querySelector(".join-actions-container");
    if (!container) return;

    container.addEventListener("click", async (event) => {
        const joinButton = event.target.closest(".join-event-btn");
        if (!joinButton) return;

        try {
            const result = await request(`/API/Event/Join/${config.eventId}`, { method: "POST" });
            if (result.success !== false) {
                await Modal.alert(result.message || "Successfully joined.", "Joined!");
                container.innerHTML = "";
                container.appendChild(createCancelButton());
            } else {
                await Modal.alert(result.message || "An error occurred.", "Error");
            }
        } catch (error) {
            await Modal.alert(error.message || "An error occurred while trying to join the event.", "Error");
        }
    });
}

export function initCancelJoinButton(config) {
    const container = document.querySelector(".join-actions-container");
    if (!container) return;

    container.addEventListener("click", async (event) => {
        const cancelBtn = event.target.closest(".cancel-join-btn");
        if (!cancelBtn) return;

        const confirmed = await Modal.confirm("Are you sure you want to cancel your join request?", "Cancel Request");
        if (!confirmed) return;

        try {
            const result = await request(`/API/Event/CancelJoin/${config.eventId}`, { method: "POST" });
            if (result.success) {
                await Modal.alert(result.message || "Request cancelled.", "Success");
                container.innerHTML = "";
                container.appendChild(createJoinButton());
            } else {
                await Modal.alert(result.message || "Failed to cancel request.", "Error");
            }
        } catch (error) {
            await Modal.alert(error.message || "An error occurred.", "Error");
        }
    });
}

export function initParticipantControls(config) {
    const list = document.querySelector(".participants-list");
    if (!list) return;

    list.addEventListener("click", async (event) => {
        const btn = event.target.closest("button");
        if (!btn) return;

        const item = btn.closest(".participant-item");
        if (!item) return;

        const userId = item.dataset.userId;
        if (!userId) return;

        if (btn.classList.contains("accept-join-btn")) {
            try {
                const result = await request(`/API/Event/AcceptJoin/${config.eventId}?userId=${userId}`, { method: "POST" });
                if (result.success) {
                    item.classList.remove("status-pending");
                    item.classList.add("status-accepted");
                    const controls = item.querySelector(".participant-controls");
                    if (controls) {
                        controls.innerHTML = `
                            <button class="btn btn-icon-only remove-join-btn" title="Remove">
                                <img src="/assets/icons/delete.svg" alt="Remove" class="icon participant-icon" />
                            </button>
                        `;
                    }
                    updateParticipantCount(1);
                } else {
                    await Modal.alert(result.message || "Failed to accept.", "Error");
                }
            } catch (error) {
                await Modal.alert(error.message || "An error occurred.", "Error");
            }
        } else if (btn.classList.contains("decline-join-btn")) {
            const confirmed = await Modal.confirm("Are you sure you want to decline this request?", "Decline Request");
            if (!confirmed) return;

            try {
                const result = await request(`/API/Event/DeclineJoin/${config.eventId}?userId=${userId}`, { method: "POST" });
                if (result.success) {
                    item.remove();
                    if (list.children.length === 0) {
                        list.innerHTML = '<div class="no-participants">No participants yet</div>';
                    }
                } else {
                    await Modal.alert(result.message || "Failed to decline.", "Error");
                }
            } catch (error) {
                await Modal.alert(error.message || "An error occurred.", "Error");
            }
        } else if (btn.classList.contains("remove-join-btn")) {
            const confirmed = await Modal.confirm("Are you sure you want to remove this participant?", "Remove Participant");
            if (!confirmed) return;

            try {
                const result = await request(`/API/Event/RemoveParticipant/${config.eventId}?userId=${userId}`, { method: "DELETE" });
                if (result.success) {
                    item.remove();
                    updateParticipantCount(-1);
                    if (list.children.length === 0) {
                        list.innerHTML = '<div class="no-participants">No participants yet</div>';
                    }
                } else {
                    await Modal.alert(result.message || "Failed to remove.", "Error");
                }
            } catch (error) {
                await Modal.alert(error.message || "An error occurred.", "Error");
            }
        }
    });
}


