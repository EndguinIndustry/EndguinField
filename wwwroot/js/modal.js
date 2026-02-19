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
        const el = overlay();
        if (!el) return;
        el.classList.remove('active');
        if (_resolve) { _resolve(value); _resolve = null; }
    };

    document.addEventListener('DOMContentLoaded', () => {
        closeBtn()?.addEventListener('click', () => close(false));
        overlay()?.addEventListener('click', (e) => { if (e.target === overlay()) close(false); });
    });

    const open = ({ titleText = '', messageText = '', showCancel = false }) => {
        const el = overlay();
        if (!el) return Promise.resolve(false);

        title().textContent = titleText;
        message().textContent = messageText;
        cancelBtn().style.display = showCancel ? '' : 'none';

        confirmBtn().onclick = () => close(true);
        cancelBtn().onclick = () => close(false);

        el.classList.add('active');

        return new Promise(res => { _resolve = res; });
    };

    return {
        alert: (messageText, titleText = 'Notice') =>
            open({ titleText, messageText, showCancel: false }),

        confirm: (messageText, titleText = 'Confirm') =>
            open({ titleText, messageText, showCancel: true }),
    };
})();

export default Modal;
