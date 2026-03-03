const Modal = (() => {
    let _resolve = null;

    const overlay = () => document.getElementById('app-modal');
    const title = () => document.getElementById('modal-title');
    const message = () => document.getElementById('modal-message');
    const footer = () => document.getElementById('modal-footer');
    const confirmBtn = () => document.getElementById('modal-confirm-btn');
    const cancelBtn = () => document.getElementById('modal-cancel-btn');
    const closeBtn = () => document.getElementById('modal-close-btn');

    const close = (value = false) => {
        const overlay_element = overlay();
        if (!overlay_element || !overlay_element.classList.contains('active')) return;

        overlay_element.classList.add('closing');

        setTimeout(() => {
            overlay_element.classList.remove('active', 'closing');
            if (_resolve) { _resolve(value); _resolve = null; }
        }, 300);
    };

    document.addEventListener('DOMContentLoaded', () => {
        closeBtn()?.addEventListener('click', () => close(false));
        overlay()?.addEventListener('click', (e) => { if (e.target === overlay()) close(false); });
    });

    const open = ({ titleText = '', messageText = '', showCancel = false, customContent = null }) => {
        const overlay_element = overlay();
        if (!overlay_element) return Promise.resolve(false);

        title().innerText = titleText;

        if (customContent) {
            message().innerHTML = '';
            if (typeof customContent === 'string') {
                message().innerHTML = customContent;
            } else {
                message().appendChild(customContent);
            }
        } else {
            message().innerText = messageText;
        }

        cancelBtn().style.display = showCancel ? '' : 'none';
        confirmBtn().style.display = (titleText === 'Notice' || titleText === 'Confirm' || !customContent) ? '' : 'none';

        confirmBtn().onclick = () => close(true);
        cancelBtn().onclick = () => close(false);

        overlay_element.classList.add('active');

        return new Promise(res => { _resolve = res; });
    };

    return {
        alert: (messageText, titleText = 'Notice') =>
            open({ titleText, messageText, showCancel: false }),

        confirm: (messageText, titleText = 'Confirm') =>
            open({ titleText, messageText, showCancel: true }),

        show: (options) => open(options),

        close: (value) => close(value)
    };
})();

export default Modal;
