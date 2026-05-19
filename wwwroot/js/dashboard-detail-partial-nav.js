/**
 * In-place updates for dashboard PAP list (KPI cards, GROWTH filter, pagination, year nav)
 * without full page reload. Server returns _DashboardDetailSwappable when header X-Dashboard-Partial: 1.
 */
(function () {
    var partialHeader = 'X-Dashboard-Partial';
    var inflight = null;

    function ajaxEnabled() {
        var main = document.querySelector('main.app-main[data-dashboard-detail-ajax="true"]');
        return !!main;
    }

    function refreshKpiTotalNodes(root) {
        if (typeof window.renderTotalPapsKpi !== 'function') return;
        (root || document).querySelectorAll('[data-kpi-total-paps]').forEach(function (node) {
            var v = node.getAttribute('data-value');
            var label = node.getAttribute('data-label');
            var opts = {};
            if (label != null && label !== '') opts.label = label;
            if (v != null && v !== '') {
                var n = Number(String(v).replace(/,/g, ''));
                opts.value = !isNaN(n) ? n : v;
            }
            window.renderTotalPapsKpi(node, opts);
        });
    }

    function syncChromeFromSwappable(el) {
        if (!el) return;
        var y = el.getAttribute('data-selected-year');
        if (y) {
            var btn = document.getElementById('navbarYearButton');
            if (btn) btn.textContent = 'Year: ' + y;
        }
        var back = document.querySelector('.dashboard-detail-back');
        var backUrl = el.getAttribute('data-detail-back-url');
        if (back && backUrl) back.setAttribute('href', backUrl);

        var yBtn = document.getElementById('navbarYearButton');
        if (yBtn && typeof bootstrap !== 'undefined') {
            var dd = bootstrap.Dropdown.getInstance(yBtn);
            if (dd) dd.hide();
        }
    }

    function navigatePartial(absUrl, options) {
        options = options || {};
        var push = options.push !== false;
        var shell = document.getElementById('dashboardDetailSwappable');
        if (!shell || !ajaxEnabled()) {
            window.location.href = absUrl;
            return;
        }
        if (inflight) inflight.abort();
        inflight = new AbortController();
        shell.classList.add('dashboard-detail-swappable--loading');

        var urlObj = new URL(absUrl, window.location.href);
        var headers = new Headers();
        headers.set(partialHeader, '1');

        fetch(urlObj.toString(), {
            method: 'GET',
            credentials: 'same-origin',
            signal: inflight.signal,
            headers: headers
        })
            .then(function (r) {
                if (!r.ok) throw new Error('partial fetch failed');
                return r.text();
            })
            .then(function (html) {
                var parsed = new DOMParser().parseFromString(html, 'text/html');
                var next = parsed.getElementById('dashboardDetailSwappable');
                if (!next) throw new Error('no swappable fragment');
                shell.replaceWith(next);
                inflight = null;
                var fresh = document.getElementById('dashboardDetailSwappable');
                if (fresh) fresh.classList.remove('dashboard-detail-swappable--loading');
                refreshKpiTotalNodes(fresh);
                syncChromeFromSwappable(fresh);
                if (push) history.pushState({ dashboardDetailPartial: true }, '', urlObj.pathname + urlObj.search);
            })
            .catch(function () {
                inflight = null;
                shell.classList.remove('dashboard-detail-swappable--loading');
                window.location.href = absUrl;
            });
    }

    document.addEventListener('click', function (e) {
        if (!ajaxEnabled()) return;
        var a = e.target.closest('a');
        if (!a || !a.getAttribute('href')) return;
        if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        if (!document.getElementById('dashboardDetailSwappable')) return;

        if (a.classList.contains('kpi-link--partial')) {
            e.preventDefault();
            navigatePartial(a.href);
            return;
        }
        if (a.classList.contains('pap-table-pagination-link')) {
            var li = a.closest('.page-item');
            if (li && li.classList.contains('disabled')) return;
            e.preventDefault();
            navigatePartial(a.href);
            return;
        }
        if (a.closest('#navbarYearDropdown') && a.classList.contains('dropdown-item')) {
            e.preventDefault();
            navigatePartial(a.href);
            return;
        }
    });

    document.addEventListener('submit', function (e) {
        if (!ajaxEnabled()) return;
        var form = e.target;
        if (!form.classList || !form.classList.contains('dashboard-growth-filter')) return;
        if (!document.getElementById('dashboardDetailSwappable')) return;
        e.preventDefault();
        var url = new URL(form.action, window.location.origin);
        var fd = new FormData(form);
        fd.forEach(function (value, key) {
            url.searchParams.set(key, value);
        });
        navigatePartial(url.pathname + url.search);
    });

    document.addEventListener('change', function (e) {
        if (!ajaxEnabled()) return;
        if (e.target.id !== 'papGrowthFilter') return;
        var form = e.target.closest('form.dashboard-growth-filter');
        if (!form || !document.getElementById('dashboardDetailSwappable')) return;
        var url = new URL(form.action, window.location.origin);
        var fd = new FormData(form);
        fd.forEach(function (value, key) {
            url.searchParams.set(key, value);
        });
        navigatePartial(url.pathname + url.search);
    });

    window.addEventListener('popstate', function () {
        if (!ajaxEnabled() || !document.getElementById('dashboardDetailSwappable')) return;
        navigatePartial(window.location.href, { push: false });
    });

    document.addEventListener('DOMContentLoaded', function () {
        if (!ajaxEnabled()) return;
        refreshKpiTotalNodes(document.getElementById('dashboardDetailSwappable'));
    });
})();
