(function () {
    var card = document.getElementById('papTableCard');
    var input = document.getElementById('userPapTableSearch');
    if (!card || !input) return;

    var listKey = (card.getAttribute('data-pap-search-list') || '').trim().toLowerCase();
    if (!listKey) return;

    var table = document.getElementById('papDetailTable');
    var shell = document.getElementById('papTablePaginationShell');
    if (!table || !shell) return;

    var debounceMs = 280;
    var timer = null;
    var seq = 0;

    function segmentForProject() {
        var path = (window.location.pathname || '').replace(/\/+$/, '');
        var base = '/Pages/User/Project';
        if (path.length <= base.length) return 'total';
        var rest = path.slice(base.length + 1);
        if (!rest) return 'total';
        return rest.split('/')[0] || 'total';
    }

    function buildFragmentUrl(page, q) {
        var u = new URL('/Pages/User/PapTableData', window.location.origin);
        u.searchParams.set('list', listKey);
        u.searchParams.set('page', String(Math.max(1, page | 0) || 1));
        if (q) u.searchParams.set('q', q);
        if (listKey === 'project') u.searchParams.set('segment', segmentForProject());
        var y = table.getAttribute('data-report-year');
        if (y) u.searchParams.set('year', y);
        return u.toString();
    }

    function syncEllipsisTitles(root) {
        if (!root) return;
        root.querySelectorAll('input.dashboard-pap-text-input--ellipsis').forEach(function (el) {
            el.setAttribute('title', el.value);
        });
        root.querySelectorAll('select.dashboard-pap-select').forEach(function (sel) {
            var opt = sel.options[sel.selectedIndex];
            sel.setAttribute('title', opt ? (opt.text || opt.value).trim() : '');
        });
    }

    function applyFragmentHtml(html) {
        var doc = new DOMParser().parseFromString(html, 'text/html');
        var bundle = doc.querySelector('.pap-ajax-bundle');
        if (!bundle) return;
        /* Rows fragment must parse inside <tbody>; a <div> around <tr> breaks table layout after innerHTML injection. */
        var trs = bundle.querySelector('tbody.pap-ajax-trs') || bundle.querySelector('.pap-ajax-trs');
        var pag = bundle.querySelector('.pap-ajax-pagination');
        var tbody = table.querySelector('tbody');
        if (tbody && trs) tbody.innerHTML = trs.innerHTML;
        if (pag) shell.innerHTML = pag.innerHTML;
        syncEllipsisTitles(table);

        try {
            var u = new URL(window.location.href);
            var qv = (input.value || '').trim();
            if (qv) u.searchParams.set('q', qv);
            else u.searchParams.delete('q');
            history.replaceState(null, '', u.pathname + (u.search ? u.search : ''));
        } catch (e) { /* noop */ }
    }

    function load(page, q) {
        var mine = ++seq;
        fetch(buildFragmentUrl(page, q), {
            credentials: 'same-origin',
            headers: { Accept: 'text/html', 'X-Requested-With': 'XMLHttpRequest', 'Cache-Control': 'no-store' },
        })
            .then(function (res) {
                if (!res.ok) throw new Error('table update failed');
                return res.text();
            })
            .then(function (text) {
                if (mine === seq) applyFragmentHtml(text);
            })
            .catch(function () {
                /* keep current table on failure */
            });
    }

    function scheduleReload() {
        clearTimeout(timer);
        timer = setTimeout(function () {
            timer = null;
            load(1, (input.value || '').trim());
        }, debounceMs);
    }

    input.addEventListener('input', scheduleReload);

    shell.addEventListener('click', function (e) {
        var a = e.target.closest('a.pap-table-pagination-link');
        if (!a) return;
        var li = a.closest('li.page-item');
        if (li && li.classList.contains('disabled')) return;
        if (a.getAttribute('aria-disabled') === 'true') return;
        var href = a.getAttribute('href');
        if (!href || href === '#') return;
        e.preventDefault();
        var u;
        try {
            u = new URL(href, window.location.origin);
        } catch (err) {
            return;
        }
        var page = parseInt(u.searchParams.get('page') || '1', 10);
        if (isNaN(page) || page < 1) page = 1;
        var q = (input.value || '').trim();
        if (q) u.searchParams.set('q', q);
        else u.searchParams.delete('q');
        load(page, q);
    });
})();
