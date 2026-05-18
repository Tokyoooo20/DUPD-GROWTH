/**
 * Toast notifications for #appNotificationHost (Views/Shared/Notification.cshtml).
 * @param {string} message
 * @param {'success'|'error'} type
 */
(function (global) {
    function showAppNotification(message, type) {
        var host = document.getElementById('appNotificationHost');
        if (!host) return;

        var existing = host.querySelector('.app-notification-toast');
        if (existing) {
            existing.remove();
        }

        var el = document.createElement('div');
        el.className =
            'app-notification-toast app-notification-toast--' +
            (type === 'success' ? 'success' : 'error');
        el.setAttribute('role', 'alert');
        el.textContent = message || '';

        host.appendChild(el);
        requestAnimationFrame(function () {
            el.classList.add('is-visible');
        });

        var hideMs = 2500;
        var fadeMs = 300;
        setTimeout(function () {
            el.classList.remove('is-visible');
            setTimeout(function () {
                if (el.parentNode) el.remove();
            }, fadeMs);
        }, hideMs);
    }

    global.showAppNotification = showAppNotification;
})(typeof window !== 'undefined' ? window : this);
