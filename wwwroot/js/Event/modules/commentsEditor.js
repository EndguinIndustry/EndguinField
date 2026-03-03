import Modal from "../../Shared/modal.js";
import { request } from "../../Shared/utils.js";

export function initCommentSection(config) {
    const sendCommentButton = document.getElementById("sendCommentBtn");
    const commentInput = document.getElementById("newCommentInput");
    const replyWrapper = document.getElementById("replyTargetWrapper");
    const cancelReplyButton = document.getElementById("cancelReplyBtn");
    const replyTargetName = document.getElementById("replyTargetName");
    const replyTargetContent = document.getElementById("replyTargetContent");
    const refreshCommentsBtn = document.getElementById("refreshCommentsBtn");
    const charCounter = document.getElementById("newCommentCharCount");
    const charMax = 1000;
    let activeReplyId = null;

    if (!sendCommentButton || !commentInput) return;

    if (charCounter) {
        const updateCounter = () => {
            const length = commentInput.value.length;
            charCounter.textContent = length;
            const container = charCounter.closest('.char-counter');
            if (container) {
                container.classList.toggle('warning', length > charMax * 0.8 && length <= charMax);
                container.classList.toggle('danger', length > charMax);
            }
        };
        const resizeTextarea = () => {
            commentInput.style.height = "auto";
            commentInput.style.height = Math.min(commentInput.scrollHeight, parseInt(window.getComputedStyle(commentInput).maxHeight)) + "px";
        };
        const handleInput = () => {
            updateCounter();
            resizeTextarea();
        }
        commentInput.addEventListener('input', handleInput);
        commentInput.addEventListener('change', handleInput);
        resizeTextarea();
    }

    const rebindCommentEvents = () => {
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
    };
    rebindCommentEvents();

    cancelReplyButton?.addEventListener("click", (event) => {
        event.preventDefault();
        activeReplyId = null;
        replyWrapper.classList.remove("active");
        commentInput.focus();
    });

    const fetchComments = async (scrollToBottom = false) => {
        try {
            if (refreshCommentsBtn?.querySelector('img')) refreshCommentsBtn.querySelector('img').classList.add('spin-anim');

            const html = await request(`/Event/CommentsSection/${config.eventId}`, {
                method: "GET"
            });

            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');

            const newScrollableContent = doc.getElementById('commentsScrollableContainer');
            const currentScrollableContent = document.getElementById('commentsScrollableContainer');

            if (newScrollableContent && currentScrollableContent) {
                currentScrollableContent.innerHTML = newScrollableContent.innerHTML;
                rebindCommentEvents();
                if (scrollToBottom) {
                    currentScrollableContent.scrollTop = currentScrollableContent.scrollHeight;
                }
            }
        } catch (error) {
            console.error("Error fetching comments:", error);
        } finally {
            if (refreshCommentsBtn?.querySelector('img')) refreshCommentsBtn.querySelector('img').classList.remove('spin-anim');
        }
    };

    if (refreshCommentsBtn) {
        refreshCommentsBtn.addEventListener("click", fetchComments);
    }

    const submitComment = async () => {
        const content = commentInput.value.trim();
        if (!content) return;

        try {
            const result = await request(`/API/Event/Comment/${config.eventId}`, {
                method: "POST",
                body: { Content: content, RepliedMessageId: activeReplyId },
                isFormData: false,
            });

            if (result.success !== false) {
                commentInput.value = "";
                commentInput.style.height = 'auto'; // Reset height
                commentInput.dispatchEvent(new Event('input')); // trigger counter reset
                cancelReplyButton?.click();
                await fetchComments(true);
            } else {
                await Modal.alert(result.message || "An error occurred.", "Error");
            }
        } catch (error) {
            await Modal.alert(error.message || "An error occurred while sending the comment.", "Error");
        }
    };

    sendCommentButton.addEventListener("click", submitComment);
    commentInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter" && event.ctrlKey) {
            event.preventDefault();
            submitComment();
        }
    });
}
