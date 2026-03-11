document.addEventListener("DOMContentLoaded", function () {
    const imageUpload = document.getElementById("imageUpload");
    const navbarAvatar = document.querySelector(".navbar-avatar");

    if (!imageUpload || !navbarAvatar) {
        return;
    }

    imageUpload.addEventListener("change", function (event) {
        const file = event.target.files?.[0];

        if (!file) {
            return;
        }

        if (!file.type.startsWith("image/")) {
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            return;
        }

        const reader = new FileReader();
        reader.onload = function (loadEvent) {
            const imageDataUrl = loadEvent.target?.result;
            if (typeof imageDataUrl !== "string") {
                return;
            }

            const currentNavbarAvatar = document.querySelector(".navbar-avatar");
            if (!currentNavbarAvatar) {
                return;
            }

            if (currentNavbarAvatar.tagName === "IMG") {
                currentNavbarAvatar.src = imageDataUrl;
                return;
            }

            const replacementImage = document.createElement("img");
            replacementImage.src = imageDataUrl;
            replacementImage.alt = currentNavbarAvatar.getAttribute("aria-label") || "Profile";
            replacementImage.className = currentNavbarAvatar.className
                .replace("user-avatar-placeholder", "user-avatar-image")
                .trim();

            currentNavbarAvatar.parentNode?.replaceChild(replacementImage, currentNavbarAvatar);
        };

        reader.readAsDataURL(file);
    });
});
