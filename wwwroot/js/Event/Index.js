import Modal from '../modal.js';

document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('event-detail-view');
    if (!container) return;

    const config = {
        eventId: container.dataset.eventId,
        isCreateMode: container.dataset.isCreateMode === 'true',
        backgroundImage: container.dataset.backgroundImage
    };

    initBackgroundLoad();
    initToggles();
    initEditMode(config);
    initJoinEvent(config);
    initBackgroundEdit(config);
    initImageEditor();

    if (config.isCreateMode) {
        document.getElementById('editBtn')?.click();
    }
});

function initBackgroundLoad() {
    const img = document.getElementById('backgroundImage');
    if (!img) return;
    const markLoaded = () => img.classList.add('loaded');
    img.complete ? markLoaded() : img.addEventListener('load', markLoaded);
}

function initImageEditor() {
    const addBtn = document.getElementById('addImageBtn');
    const fileInput = document.getElementById('imageUploadInput');
    const container = document.getElementById('imageListContainer');
    if (!addBtn || !fileInput || !container) return;

    addBtn.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', (e) => {
        Array.from(e.target.files).forEach(file => {
            const reader = new FileReader();
            reader.onload = (event) => addImageItem(container, event.target.result);
            reader.readAsDataURL(file);
        });
        fileInput.value = '';
    });

    container.addEventListener('click', (e) => {
        e.target.closest('.remove-image-btn')?.closest('.image-list-item')?.remove();
    });
}

function addImageItem(container, src) {
    const div = document.createElement('div');
    div.className = 'image-list-item position-relative mb-2';
    div.innerHTML = `
        <img src="${src}" class="w-100 rounded-3" alt="Event image" style="height: 200px; object-fit: cover;">
        <button type="button" class="remove-image-btn" title="Remove Image">&times;</button>
    `;
    container.appendChild(div);
}

function initBackgroundEdit(config) {
    const editBtn = document.getElementById('editBackgroundBtn');
    const fileInput = document.getElementById('backgroundImageInput');
    const imgEl = document.getElementById('backgroundImage');
    if (!editBtn || !fileInput || !imgEl) return;

    editBtn.addEventListener('click', () => fileInput.click());
    fileInput.addEventListener('change', (e) => {
        const file = e.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (ev) => {
            imgEl.src = ev.target.result;
            config.newBackgroundImageFile = file;
        };
        reader.readAsDataURL(file);
    });
}

function initJoinEvent(config) {
    const joinBtn = document.querySelector('.join-event-btn');
    if (!joinBtn) return;

    joinBtn.addEventListener('click', async () => {
        try {
            const res = await fetch(`/Event/Join/${config.eventId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
            const result = await res.json();
            if (res.ok) {
                await Modal.alert(result.message, 'Joined!');
                joinBtn.textContent = 'Requested';
                joinBtn.disabled = true;
                joinBtn.classList.replace('btn-success', 'btn-secondary');
            } else {
                await Modal.alert(result.message || 'An error occurred.', 'Error');
            }
        } catch {
            await Modal.alert('An error occurred while trying to join the event.', 'Error');
        }
    });
}

function initToggles() {
    const toggleBtn = document.getElementById('toggleBtn');
    const details = document.getElementById('detailsSection');
    const comments = document.getElementById('commentsSection');
    if (!toggleBtn || !details || !comments) return;

    toggleBtn.addEventListener('click', () => {
        comments.classList.toggle('hidden-section');
        const commentsVisible = !comments.classList.contains('hidden-section');
        details.style.display = commentsVisible ? 'none' : 'block';
        toggleBtn.innerHTML = commentsVisible
            ? `<img src="/assets/icons/info.svg" alt="Details" class="icon-dark icon" />`
            : `<img src="/assets/icons/comment.svg" alt="Comments" class="icon-dark icon" />`;
        toggleBtn.title = commentsVisible ? 'Show Details' : 'Show Comments';
    });
}

function initEditMode(config) {
    const editBtn = document.getElementById('editBtn');
    if (!editBtn) return;

    let isEditMode = false;
    const viewIds = ['viewName', 'viewDescription', 'viewLocation', 'viewDate', 'eventImageCarousel'];
    const views = viewIds.map(id => document.getElementById(id));
    const edits = document.querySelectorAll('.edit-mode-only');
    const inputIds = ['editNameInput', 'editDescriptionInput', 'editLocationInput', 'editStartTimeInput', 'editEndTimeInput'];
    let snapshot = {};

    document.getElementById('cancelEditBtn')?.addEventListener('click', () => {
        inputIds.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.value = snapshot[id] ?? element.value;
        });
        isEditMode = false;
        toggleEditUI(false, views, edits, editBtn);
    });

    editBtn.addEventListener('click', async () => {
        if (!isEditMode) {
            inputIds.forEach(id => {
                const element = document.getElementById(id);
                if (element) snapshot[id] = element.value;
            });
        }
        isEditMode = !isEditMode;
        toggleEditUI(isEditMode, views, edits, editBtn);
        if (!isEditMode) {
            config.isCreateMode ? await createEvent(config) : saveChanges();
        }
    });
}

async function createEvent(config) {
    const newEvent = {
        Name: document.getElementById('editNameInput')?.value,
        Description: document.getElementById('editDescriptionInput')?.value,
        Location: document.getElementById('editLocationInput')?.value,
        StartTime: document.getElementById('editStartTimeInput')?.value,
        EndTime: document.getElementById('editEndTimeInput')?.value,
        BackgroundImage: config.backgroundImage,
        Status: document.querySelector('input[name="eventStatus"]:checked')?.value ?? 'Open',
        MaxParticipants: 10,
        Category: 'General'
    };

    try {
        const res = await fetch('/Event/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newEvent)
        });
        const result = await res.json();
        if (res.ok && result.success) {
            window.location.href = result.redirectUrl;
        } else {
            await Modal.alert(result.message || 'An error occurred while creating the event.', 'Error');
        }
    } catch {
        await Modal.alert('An error occurred while trying to create the event.', 'Error');
    }
}

function toggleEditUI(isEdit, views, edits, btn) {
    views.forEach(element => element?.style.setProperty('display', isEdit ? 'none' : 'block', isEdit ? '' : 'important'));
    edits.forEach(element => element?.style.setProperty('display', isEdit ? 'block' : 'none', isEdit ? 'important' : ''));
    document.querySelectorAll('.view-mode-only').forEach(element =>
        element.style.setProperty('display', isEdit ? 'none' : 'block', isEdit ? '' : 'important')
    );
    btn.innerHTML = isEdit
        ? `<img src="/assets/icons/save.svg" alt="Save" class="icon-dark icon" />`
        : `<img src="/assets/icons/edit.svg" alt="Edit" class="icon-dark icon" />`;
    btn.title = isEdit ? 'Save Changes' : 'Edit Event';
}

function saveChanges() {
    [['viewName', 'editNameInput'], ['viewDescription', 'editDescriptionInput'], ['viewLocation', 'editLocationInput']]
        .forEach(([viewId, inputId]) => {
            const element = document.getElementById(viewId);
            const input = document.getElementById(inputId);
            if (element && input) element.textContent = input.value;
        });

    const viewDateP = document.getElementById('viewDate')?.querySelector('p');
    const start = document.getElementById('editStartTimeInput');
    const end = document.getElementById('editEndTimeInput');
    if (viewDateP && start && end) {
        const fmt = s => { const d = new Date(s), p = n => String(n).padStart(2, '0'); return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`; };
        viewDateP.textContent = `${fmt(start.value)} - ${fmt(end.value)}`;
    }
}
