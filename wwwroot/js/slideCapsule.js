document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.slide-capsule').forEach(capsule => {
        const indicator = capsule.querySelector('.slide-capsule-indicator');
        const options = capsule.querySelectorAll('.slide-capsule-option');

        options.forEach((label, index) => {
            label.addEventListener('click', () => {
                const variant = label.dataset.variant;
                indicator.style.transform = `translateX(calc(${index} * 100%))`;
                indicator.dataset.variant = variant;
            });
        });
    });
});
