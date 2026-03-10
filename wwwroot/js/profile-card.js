let originalData = {};
let uploadedImageFile = null;
let uploadedImageData = null;

function toggleEditMode(isEdit) {
  const viewMode = document.querySelector(".profile-view-mode");
  const editMode = document.querySelector(".profile-edit-mode");

  if (isEdit) {
    originalData = {
      name: document.getElementById("nameDisplay").textContent,
      contact: document
        .getElementById("contactDisplay")
        .textContent.trim()
        .replace(/\s+/g, " "),
      gender:
        document.getElementById("genderDisplay")?.textContent?.trim() || "",
      bio: document.getElementById("bioDisplay")?.textContent || "",
      imageUrl: document.getElementById("avatarDisplay")?.src || "",
    };

    viewMode.style.display = "none";
    editMode.style.display = "block";

    uploadedImageFile = null;
    uploadedImageData = null;
  } else {
    viewMode.style.display = "block";
    editMode.style.display = "none";

    document.getElementById("profileEditForm").reset();
    uploadedImageFile = null;
    uploadedImageData = null;
  }
}

document.addEventListener("DOMContentLoaded", function () {
  const imageUpload = document.getElementById("imageUpload");
  const avatarPreview = document.getElementById("avatarPreview");
  const bioInput = document.getElementById("bioInput");
  const charCount = document.getElementById("charCount");

  if (imageUpload) {
    imageUpload.addEventListener("change", function (e) {
      const file = e.target.files[0];

      if (file) {
        if (!file.type.startsWith("image/")) {
          showNotification("Please select a valid image file", "error");
          return;
        }

        if (file.size > 5 * 1024 * 1024) {
          showNotification("Image size must be less than 5MB", "error");
          return;
        }

        uploadedImageFile = file;

        const reader = new FileReader();
        reader.onload = function (event) {
          uploadedImageData = event.target.result;

          if (avatarPreview.tagName === "IMG") {
            avatarPreview.src = uploadedImageData;
          } else {
            const container = avatarPreview.parentNode;
            const newImg = document.createElement("img");
            newImg.src = uploadedImageData;
            newImg.className = "avatar-image";
            newImg.id = "avatarPreview";
            container.replaceChild(newImg, avatarPreview);
          }

          showNotification("Image selected successfully", "success");
        };
        reader.readAsDataURL(file);
      }
    });
  }

  if (bioInput && charCount) {
    bioInput.addEventListener("input", function () {
      const count = this.value.length;
      charCount.textContent = count;

      const counterElement = charCount.parentElement;
      counterElement.classList.remove("warning", "danger");

      if (count > 450) {
        counterElement.classList.add("danger");
      } else if (count > 400) {
        counterElement.classList.add("warning");
      }
    });
  }

  const form = document.getElementById("profileEditForm");
  if (form) {
    form.addEventListener("submit", async function (e) {
      e.preventDefault();

      const profileCard = document.querySelector(".profile-card");
      const userId = profileCard.dataset.userId;

      const formData = new FormData();
      formData.append("id", userId);
      formData.append("name", document.getElementById("nameInput").value);
      formData.append("contact", document.getElementById("contactInput").value);
      formData.append("gender", document.getElementById("genderInput").value);
      formData.append("bio", document.getElementById("bioInput").value);

      if (uploadedImageFile) {
        formData.append("imageFile", uploadedImageFile);
      }

      profileCard.classList.add("loading");

      try {
        const response = await fetch(`/Profile/Update/${userId}`, {
          method: "POST",
          body: formData,
        });

        if (response.ok) {
          const result = await response.json();

          document.getElementById("nameDisplay").textContent =
            document.getElementById("nameInput").value;

          const newContact = document.getElementById("contactInput").value;
          document.getElementById("contactDisplay").innerHTML = `
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path>
                        </svg>
                        ${newContact}
                    `;

          const newGender = document.getElementById("genderInput").value;
          document.getElementById("genderDisplay").innerHTML = `
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <circle cx="12" cy="7" r="4"></circle>
                            <path d="M5.5 21a6.5 6.5 0 0 1 13 0"></path>
                        </svg>
                        ${newGender || "Not specified"}
                    `;

          const bioDisplay = document.getElementById("bioDisplay");
          const newBio = document.getElementById("bioInput").value;
          if (bioDisplay) {
            bioDisplay.textContent = newBio;
          }

          if (uploadedImageData) {
            const avatarDisplay = document.getElementById("avatarDisplay");
            if (result.imageUrl) {
              if (avatarDisplay.tagName === "IMG") {
                avatarDisplay.src = result.imageUrl;
              } else {
                const container = avatarDisplay.parentNode;
                const newImg = document.createElement("img");
                newImg.src = result.imageUrl;
                newImg.className = "avatar-image";
                newImg.id = "avatarDisplay";
                container.replaceChild(newImg, avatarDisplay);
              }
            } else {
              if (avatarDisplay.tagName === "IMG") {
                avatarDisplay.src = uploadedImageData;
              } else {
                const container = avatarDisplay.parentNode;
                const newImg = document.createElement("img");
                newImg.src = uploadedImageData;
                newImg.className = "avatar-image";
                newImg.id = "avatarDisplay";
                container.replaceChild(newImg, avatarDisplay);
              }
            }
          }

          showNotification("Profile updated successfully!", "success");

          toggleEditMode(false);
        } else {
          const error = await response.json();
          showNotification(
            error.message || "Failed to update profile",
            "error",
          );
        }
      } catch (error) {
        console.error("Error:", error);
        showNotification("An error occurred while updating profile", "error");
      } finally {
        profileCard.classList.remove("loading");
      }
    });
  }
});

function showNotification(message, type) {
  const existing = document.querySelectorAll(".notification");
  existing.forEach((n) => n.remove());

  const notification = document.createElement("div");
  notification.className = `notification notification-${type}`;

  const icon =
    type === "success"
      ? '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"></polyline></svg>'
      : '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';

  notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 10px;">
            ${icon}
            <span>${message}</span>
        </div>
    `;

  notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        background: ${type === "success" ? "#48bb78" : "#f56565"};
        color: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 10000;
        animation: slideIn 0.3s ease;
        font-size: 14px;
        font-weight: 500;
    `;

  document.body.appendChild(notification);

  setTimeout(() => {
    notification.style.animation = "slideOut 0.3s ease";
    setTimeout(() => notification.remove(), 300);
  }, 3000);
}

const style = document.createElement("style");
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(400px); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(400px); opacity: 0; }
    }
`;
document.head.appendChild(style);

// Placeholder function for add friend functionality
async function addFriend(userId) {
  try {
    const response = await fetch("/API/AddFriend", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        receiverId: userId,
      }),
    });

    if (!response.ok) {
      throw new Error("Failed to send friend request");
    }

    const data = await response.json();

    if (data.success) {
      showNotification("Friend request sent!", "success");
    } else {
      showNotification("Something went wrong", "error");
    }
  } catch (err) {
    console.error(err);
    showNotification("Error sending friend request", "error");
  }
}

function toggleFriendRequest(show) {
  const panel = document.getElementById("friendRequestPanel");

  if (show) {
    panel.style.display = "flex";
  } else {
    panel.style.display = "none";
  }
}

function toggleFriendList(show) {
  const panel = document.getElementById("friendListPanel");

  if (show) panel.classList.add("active");
  else panel.classList.remove("active");
}

async function handleFriendRequest(userId, accept) {
  const res = await fetch("/API/HandleFriendRequest", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body: `senderId=${encodeURIComponent(userId)}&accept=${accept}`,
  });

  const data = await res.json();

  if (data.success) {
    showNotification(
      accept ? "Friend added!" : "Friend request rejected",
      "success",
    );

    location.reload();
  }
}

async function unfriend(userId) {
  const res = await fetch("/API/Unfriend", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body: `friendId=${encodeURIComponent(userId)}`,
  });

  const data = await res.json();

  if (data.success) {
    showNotification("Friend removed", "success");
    location.reload();
  }
}
