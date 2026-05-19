/**
 * Client Project table: POST completion photo to /Pages/User/UploadProjectCompletionPhoto.
 * Expects #papTableCard[data-project-completion-photo-upload-url] and __RequestVerificationToken on the page.
 */
(function (global) {
    'use strict';

    var MAX_IMAGE_BYTES = 20 * 1024 * 1024;

    function getToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el && el.value ? el.value : '';
    }

    function uploadUrl(card) {
        return (card && card.getAttribute('data-project-completion-photo-upload-url')) || '';
    }

    function setHint(wrap, text, isError) {
        var hint = wrap.querySelector('[data-pap-completion-photo-hint]');
        if (!hint) return;
        hint.textContent = text || '';
        hint.classList.toggle('d-none', !text);
        hint.classList.toggle('text-danger', !!isError);
        hint.classList.toggle('text-success', !!text && !isError);
    }

    function bindTable(card) {
        if (!card || card.dataset.dupdCompletionPhotoBound === '1') return;
        var base = uploadUrl(card);
        if (!base) return;
        card.dataset.dupdCompletionPhotoBound = '1';

        card.addEventListener('click', function (e) {
            var btn = e.target && e.target.closest('[data-pap-completion-photo-trigger]');
            if (!btn || !card.contains(btn)) return;
            e.preventDefault();
            var cid = btn.getAttribute('aria-controls');
            var input = cid ? document.getElementById(cid) : null;
            if (input) input.click();
        });

        card.addEventListener('change', function (e) {
            var input = e.target;
            if (!input || input.type !== 'file' || !input.hasAttribute('data-pap-completion-photo') || !card.contains(input))
                return;

            var wrap = input.closest('.user-project-completion-photo');
            if (!wrap) return;

            var url = uploadUrl(card);
            var projectId = input.getAttribute('data-project-id');
            var token = getToken();

            setHint(wrap, '', false);

            var file = input.files && input.files[0];
            input.value = '';
            if (!file) return;

            if (!file.type || file.type.indexOf('image/') !== 0) {
                setHint(wrap, 'Please choose an image file.', true);
                return;
            }
            if (file.size > MAX_IMAGE_BYTES) {
                setHint(wrap, 'Maximum file size is 20 MB.', true);
                return;
            }
            if (!projectId || !token) {
                setHint(wrap, 'Upload is not available. Refresh the page and try again.', true);
                return;
            }

            var fd = new FormData();
            fd.append('projectId', projectId);
            fd.append('photo', file);
            fd.append('__RequestVerificationToken', token);

            setHint(wrap, 'Uploading…', false);

            fetch(url, { method: 'POST', body: fd, credentials: 'same-origin' })
                .then(function (res) {
                    return res.json().then(function (data) {
                        return { ok: res.ok, data: data };
                    });
                })
                .then(function (result) {
                    if (result.ok && result.data && result.data.ok && result.data.url) {
                        setHint(wrap, 'Saved.', false);
                        var link = wrap.querySelector('[data-pap-completion-photo-link]');
                        var u = result.data.url;
                        if (link) {
                            link.setAttribute('href', u);
                            link.classList.remove('d-none');
                            link.style.display = '';
                        } else {
                            link = document.createElement('a');
                            link.setAttribute('href', u);
                            link.setAttribute('target', '_blank');
                            link.setAttribute('rel', 'noopener noreferrer');
                            link.className = 'small d-inline-block mb-1';
                            link.setAttribute('data-pap-completion-photo-link', '');
                            link.textContent = 'View photo';
                            var stack = wrap.querySelector('.d-flex.flex-column');
                            if (stack) wrap.insertBefore(link, stack);
                            else wrap.appendChild(link);
                        }
                        var trig = wrap.querySelector('[data-pap-completion-photo-trigger]');
                        if (trig) trig.textContent = 'Replace completion photo';
                        if (typeof global.showEditToast === 'function') {
                            global.showEditToast('Completion photo saved to the database.', 'success');
                        }
                    } else {
                        var msg = (result.data && result.data.error) ? result.data.error : 'Upload failed.';
                        setHint(wrap, msg, true);
                    }
                })
                .catch(function () {
                    setHint(wrap, 'Network error. Try again.', true);
                });
        });
    }

    function init() {
        var card = document.getElementById('papTableCard');
        bindTable(card);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(window);
