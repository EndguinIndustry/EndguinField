document.addEventListener("DOMContentLoaded", () => {
	document.querySelectorAll(".slide-capsule").forEach((capsule) => {
		const indicator = capsule.querySelector(".slide-capsule-indicator");
		const options = capsule.querySelectorAll(".slide-capsule-option");

		function updateIndicator() {
			const checkedRadio = capsule.querySelector(".slide-capsule-input:checked");
			if (!checkedRadio) return;

			const checkedLabel = capsule.querySelector(`label[for="${checkedRadio.id}"]`);
			if (checkedLabel) {
				const variant = checkedLabel.dataset.variant;
				indicator.style.width = `${checkedLabel.offsetWidth}px`;
				indicator.style.transform = `translateX(${checkedLabel.offsetLeft}px)`;
				indicator.dataset.variant = variant;
			}
		}

		updateIndicator();

		options.forEach((label) => {
			label.addEventListener("click", () => {
				setTimeout(updateIndicator, 0);
			});
		});

		window.addEventListener("resize", updateIndicator);
	});
});
