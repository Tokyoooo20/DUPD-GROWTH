/**
 * Edit Project form (#editPapForm): quarter sequence, Completed locks later quarters, upload UI (frontend only).
 */
(function (global) {
    'use strict';

    var MAX_IMAGE_BYTES = 20 * 1024 * 1024;

    function getQuarterSelects() {
        return ['userAddPapQ1', 'userAddPapQ2', 'userAddPapQ3', 'userAddPapQ4'].map(function (id) {
            return document.getElementById(id);
        });
    }

    function hasSelection(sel) {
        return !!(sel && String(sel.value || '').trim() !== '');
    }

    function isCompleted(sel) {
        if (!sel) return false;
        return String(sel.value || '').trim().toLowerCase() === 'completed';
    }

    function firstCompletedIndex(q) {
        for (var i = 0; i < q.length; i++) {
            if (isCompleted(q[i])) return i;
        }
        return -1;
    }

    function ensureUploadUi(select, quarterLabel) {
        var sib = select.nextElementSibling;
        if (sib && sib.classList && sib.classList.contains('user-pap-quarter-upload')) {
            return sib;
        }
        var wrap = document.createElement('div');
        wrap.className = 'user-pap-quarter-upload mt-2';
        wrap.hidden = true;

        var fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.className = 'visually-hidden';
        fileInput.setAttribute('tabindex', '-1');
        fileInput.setAttribute('aria-label', 'Upload photo for ' + quarterLabel);
        fileInput.accept = 'image/*';

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-sm btn-outline-secondary';
        btn.textContent = 'Upload Photo';

        var hint = document.createElement('small');
        hint.className = 'text-body-secondary d-block mt-1';

        btn.addEventListener('click', function () {
            fileInput.click();
        });

        fileInput.addEventListener('change', function () {
            hint.textContent = '';
            hint.classList.remove('text-danger');
            var f = fileInput.files && fileInput.files[0];
            if (!f) return;
            if (!f.type || f.type.indexOf('image/') !== 0) {
                hint.textContent = 'Please choose an image file.';
                hint.classList.add('text-danger');
                fileInput.value = '';
                return;
            }
            if (f.size > MAX_IMAGE_BYTES) {
                hint.textContent = 'Maximum file size is 20MB.';
                hint.classList.add('text-danger');
                fileInput.value = '';
                return;
            }
            hint.textContent = 'Selected: ' + f.name + ' (not saved)';
            hint.classList.remove('text-danger');
        });

        wrap.appendChild(fileInput);
        wrap.appendChild(btn);
        wrap.appendChild(hint);
        select.parentNode.insertBefore(wrap, select.nextSibling);
        return wrap;
    }

    function syncEditFormQuarters() {
        var form = document.getElementById('editPapForm');
        if (!form) return;

        var q = getQuarterSelects();
        if (q.some(function (el) { return !el; })) return;

        var labels = ['1st quarter', '2nd quarter', '3rd quarter', '4th quarter'];
        for (var u = 0; u < 4; u++) {
            ensureUploadUi(q[u], labels[u]);
        }

        var fc = firstCompletedIndex(q);

        for (var j = 0; j < 4; j++) {
            var needPrior = j > 0 && !hasSelection(q[j - 1]);
            var afterCompleted = fc >= 0 && j > fc;
            if (needPrior || afterCompleted) {
                // Clear value before disabling so all browsers apply the empty option; disabled controls can
                // ignore subsequent value writes in some cases.
                q[j].value = '';
                q[j].disabled = true;
                if (needPrior) {
                    q[j].title = 'Select the previous quarter first.';
                } else {
                    q[j].title = 'Disabled: an earlier quarter is marked Completed.';
                }
            } else {
                q[j].disabled = false;
                q[j].title = '';
            }

            var uploadEl = q[j].nextElementSibling;
            if (uploadEl && uploadEl.classList.contains('user-pap-quarter-upload')) {
                var showUpload = isCompleted(q[j]);
                uploadEl.hidden = !showUpload;
                if (!showUpload) {
                    var fi = uploadEl.querySelector('input[type=file]');
                    if (fi) fi.value = '';
                    var hint = uploadEl.querySelector('small');
                    if (hint) {
                        hint.textContent = '';
                        hint.classList.remove('text-danger');
                    }
                }
            }
        }
    }

    function bindForm() {
        var form = document.getElementById('editPapForm');
        if (!form || form.dataset.dupdQuarterBound === '1') return;
        form.dataset.dupdQuarterBound = '1';
        form.addEventListener('change', function (e) {
            var t = e.target;
            if (!t || !t.id) return;
            if (t.id !== 'userAddPapQ1' && t.id !== 'userAddPapQ2' &&
                t.id !== 'userAddPapQ3' && t.id !== 'userAddPapQ4') {
                return;
            }
            syncEditFormQuarters();
        });
    }

    global.dupdInitEditFormQuarters = function () {
        bindForm();
        syncEditFormQuarters();
    };
})(window);
