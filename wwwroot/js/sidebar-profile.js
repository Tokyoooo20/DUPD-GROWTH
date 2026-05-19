/**
 * Sidebar account (Views/Client/Profile.cshtml): floating popover + optional photo upload + toasts.
 */
(function () {
    var wrap = document.querySelector('.app-sidebar-profile-wrap');
    if (!wrap || wrap.classList.contains('app-sidebar-profile-wrap--static')) return;

    var trigger = wrap.querySelector('.app-sidebar-profile-trigger');
    var panel = wrap.querySelector('.app-sidebar-profile-panel');
    var shell = document.getElementById('dashboardDetailSidebarShell');
    if (!trigger || !panel) return;

    if (panel.parentNode !== document.body) {
        document.body.appendChild(panel);
    }

    var open = false;
    var panelWidth = 288;

    function positionPanel() {
        if (!open) return;
        var r = trigger.getBoundingClientRect();
        var gap = 10;
        var left = r.right + gap;
        var vw = window.innerWidth;
        var vh = window.innerHeight;
        if (left + panelWidth > vw - 10) {
            left = Math.max(10, r.left - panelWidth - gap);
        }
        var top = Math.max(10, r.top);
        panel.style.left = left + 'px';
        panel.style.top = top + 'px';

        var rect = panel.getBoundingClientRect();
        if (rect.bottom > vh - 10) {
            top = Math.max(10, vh - rect.height - 10);
            panel.style.top = top + 'px';
        }
    }

    function setOpen(next) {
        open = next;
        trigger.setAttribute('aria-expanded', next ? 'true' : 'false');
        panel.setAttribute('aria-hidden', next ? 'false' : 'true');
        panel.classList.toggle('is-open', next);
        if (shell) {
            shell.classList.toggle('dashboard-sidebar-shell--profile-open', next);
        }
        if (next) {
            requestAnimationFrame(function () {
                positionPanel();
            });
        } else {
            panel.style.left = '';
            panel.style.top = '';
        }
    }

    trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        setOpen(!open);
    });

    document.addEventListener('mousedown', function (e) {
        if (!open) return;
        if (wrap.contains(e.target)) return;
        if (panel.contains(e.target)) return;
        setOpen(false);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && open) {
            setOpen(false);
        }
    });

    window.addEventListener('resize', function () {
        if (open) positionPanel();
    });

    window.addEventListener(
        'scroll',
        function () {
            if (open) positionPanel();
        },
        true
    );

    function bustUrl(url) {
        if (!url) return url;
        var sep = url.indexOf('?') >= 0 ? '&' : '?';
        return url + sep + 't=' + Date.now();
    }

    function setTriggerPhoto(url) {
        var thumb = document.getElementById('sidebarProfileTriggerThumb');
        if (!thumb || !url) return;
        var icon = document.getElementById('sidebarProfileTriggerIcon');
        var img = document.getElementById('sidebarProfileTriggerImg');
        var busted = bustUrl(url);
        if (img) {
            img.src = busted;
            return;
        }
        if (icon) {
            icon.remove();
        }
        var newImg = document.createElement('img');
        newImg.id = 'sidebarProfileTriggerImg';
        newImg.className = 'app-sidebar-profile-trigger-photo';
        newImg.setAttribute('width', '32');
        newImg.setAttribute('height', '32');
        newImg.setAttribute('alt', '');
        newImg.loading = 'eager';
        newImg.src = busted;
        thumb.insertBefore(newImg, thumb.firstChild);
    }

    function setPopoverAvatarImage(url) {
        var host = document.getElementById('profilePageAvatar');
        if (!host || !url) return;
        var existingImg = document.getElementById('profilePageAvatarImg');
        var initialsEl = document.getElementById('profilePageAvatarInitials');
        var busted = bustUrl(url);
        if (existingImg) {
            existingImg.src = busted;
            return;
        }
        if (initialsEl) {
            initialsEl.remove();
        }
        var avImg = document.createElement('img');
        avImg.id = 'profilePageAvatarImg';
        avImg.className = 'app-sidebar-profile-avatar-img';
        avImg.setAttribute('width', '256');
        avImg.setAttribute('height', '256');
        avImg.setAttribute('alt', '');
        avImg.loading = 'lazy';
        avImg.src = busted;
        host.insertBefore(avImg, host.firstChild);
    }

    window.sidebarProfileUpdateThumbs = function (url) {
        setTriggerPhoto(url);
        setPopoverAvatarImage(url);
    };
})();

(function () {
    var root = document.getElementById('profilePhotoUploadRoot');
    if (!root || typeof bootstrap === 'undefined') return;

    var uploadEndpoint = root.getAttribute('data-upload-endpoint');
    var canUpload = root.getAttribute('data-can-upload') === 'true';
    var fileInput = document.getElementById('profilePagePhotoInput');
    var photoBtn = document.getElementById('profilePagePhotoBtn');
    var intentModalEl = document.getElementById('profilePhotoChangeIntentModal');
    var pendingFile = null;
    var skipIntentModalHiddenOnce = false;

    if (!canUpload || !uploadEndpoint || !fileInput || !intentModalEl) return;

    function bustUrl(url) {
        if (!url) return url;
        var sep = url.indexOf('?') >= 0 ? '&' : '?';
        return url + sep + 't=' + Date.now();
    }

    function setPageAvatar(url) {
        var host = document.getElementById('profilePageAvatar');
        if (!host || !url) return;
        var existingImg = document.getElementById('profilePageAvatarImg');
        var initialsEl = document.getElementById('profilePageAvatarInitials');
        var busted = bustUrl(url);
        if (existingImg) {
            existingImg.src = busted;
            return;
        }
        if (initialsEl) {
            initialsEl.remove();
        }
        var img = document.createElement('img');
        img.id = 'profilePageAvatarImg';
        img.className = 'app-sidebar-profile-avatar-img';
        img.setAttribute('width', '256');
        img.setAttribute('height', '256');
        img.setAttribute('alt', '');
        img.loading = 'lazy';
        img.src = busted;
        host.insertBefore(img, host.firstChild);
    }

    function notifyUpload(success, plainMessage) {
        if (typeof window.showAppNotification !== 'function') return;
        window.showAppNotification(plainMessage, success ? 'success' : 'error');
    }

    function performUpload() {
        if (!pendingFile || !uploadEndpoint) return;
        var tokenEl = root.querySelector('input[name="__RequestVerificationToken"]');
        var token = tokenEl ? tokenEl.value : '';
        var fd = new FormData();
        fd.append('photo', pendingFile);
        fd.append('__RequestVerificationToken', token);

        fetch(uploadEndpoint, {
            method: 'POST',
            body: fd,
            credentials: 'same-origin'
        })
            .then(function (r) {
                return r.text().then(function (t) {
                    var body = {};
                    try {
                        body = t ? JSON.parse(t) : {};
                    } catch (e) {
                        body = { ok: false, error: t || 'Unexpected response from server.' };
                    }
                    return { status: r.status, okHttp: r.ok, body: body };
                });
            })
            .then(function (res) {
                pendingFile = null;

                if (res.okHttp && res.body && res.body.ok && res.body.url) {
                    var url = res.body.url;
                    setPageAvatar(url);
                    if (typeof window.sidebarProfileUpdateThumbs === 'function') {
                        window.sidebarProfileUpdateThumbs(url);
                    }
                    if (fileInput) fileInput.value = '';
                    notifyUpload(
                        true,
                        'Profile photo successfully updated.'
                    );
                } else {
                    if (fileInput) fileInput.value = '';
                    var err =
                        res.body && res.body.error
                            ? String(res.body.error)
                            : 'Something went wrong. Please try a smaller JPG, PNG, WebP, or GIF (max 20 MB).';
                    notifyUpload(false, err);
                }
            })
            .catch(function () {
                pendingFile = null;
                if (fileInput) fileInput.value = '';
                notifyUpload(
                    false,
                    'We could not reach the server. Check your connection and try again.'
                );
            });
    }

    if (photoBtn) {
        photoBtn.addEventListener('click', function () {
            bootstrap.Modal.getOrCreateInstance(intentModalEl).show();
        });
    }

    var intentContinueBtn = document.getElementById('profilePhotoChangeIntentContinueBtn');
    if (intentContinueBtn) {
        intentContinueBtn.addEventListener('click', function () {
            skipIntentModalHiddenOnce = true;
            var inst = bootstrap.Modal.getInstance(intentModalEl);
            if (inst) inst.hide();
            requestAnimationFrame(function () {
                fileInput.click();
            });
        });
    }

    intentModalEl.addEventListener('hidden.bs.modal', function () {
        if (skipIntentModalHiddenOnce) {
            skipIntentModalHiddenOnce = false;
            return;
        }
        if (fileInput) fileInput.value = '';
    });

    fileInput.addEventListener('change', function () {
        var file = fileInput.files && fileInput.files[0];
        if (!file) return;

        var maxBytes = 20 * 1024 * 1024;
        if (file.size > maxBytes) {
            fileInput.value = '';
            notifyUpload(false, 'Please choose an image under 20 MB.');
            return;
        }

        var okType = /^image\/(jpeg|png|webp|gif)$/i.test(file.type);
        if (!okType) {
            fileInput.value = '';
            notifyUpload(false, 'Please use JPG, PNG, WebP, or GIF.');
            return;
        }

        pendingFile = file;
        performUpload();
    });
})();
