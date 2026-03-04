import { addEventIfElementExists } from "../../Shared/utils.js";

export function initBackgroundEditor(config) {
    const fileInput = document.getElementById("backgroundImageInput");
    const backgroundPreview = document.getElementById("backgroundImage");
    if (!fileInput || !backgroundPreview) return;

    addEventIfElementExists("#editBackgroundBtn", "click", () => fileInput.click());

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

export function initImagesEditor(config) {
    const fileInput = document.getElementById("imageUploadInput");
    const imagesContainer = document.getElementById("imageListContainer");
    if (!fileInput || !imagesContainer) return;

    addEventIfElementExists("#addImageBtn", "click", () => fileInput.click());

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

function addImagesItem(container, source) {
    const itemDiv = document.createElement("div");
    itemDiv.className = "image-list-item position-relative mb-2";
    itemDiv.innerHTML = `
        <img src="${source}" class="w-100 rounded-3" alt="Event image" style="aspect-ratio: 16/9; object-fit: cover;">
        <button type="button" class="remove-image-btn" title="Remove Image">&times;</button>
    `;
    container.appendChild(itemDiv);
    return itemDiv;
}
