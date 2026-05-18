(function () {
    /**
     * Renders the Total PAPs KPI card into a container element.
     * Exposed globally for reuse on any page that includes this script (and superadmin-dashboard.css + Bootstrap Icons).
     * @param {Element|string} container - Element or selector (e.g. '#kpiTotalPaps')
     * @param {{ value?: number|string, label?: string }} [options]
     */
    function renderTotalPapsKpi(container, options) {
        options = options || {};
        var el = typeof container === 'string' ? document.querySelector(container) : container;
        if (!el) return;

        var label = options.label != null ? String(options.label) : 'Total PAPs';
        var raw = options.value != null ? options.value : 1750;
        var valueText;
        if (typeof raw === 'number' && !isNaN(raw)) {
            valueText = raw.toLocaleString('en-US');
        } else {
            valueText = String(raw);
        }

        el.textContent = '';
        var card = document.createElement('div');
        card.className = 'dashboard-kpi-card h-100';

        var iconWrap = document.createElement('div');
        iconWrap.className = 'dashboard-kpi-card__icon dashboard-kpi-card__icon--neutral';
        iconWrap.setAttribute('aria-hidden', 'true');
        iconWrap.innerHTML = '<i class="bi bi-clipboard2-data"></i>';

        var content = document.createElement('div');
        content.className = 'dashboard-kpi-card__content';

        var labelSpan = document.createElement('span');
        labelSpan.className = 'dashboard-kpi-card__label';
        labelSpan.textContent = label;

        var valueSpan = document.createElement('span');
        valueSpan.className = 'dashboard-kpi-card__value';
        valueSpan.textContent = valueText;

        content.appendChild(labelSpan);
        content.appendChild(valueSpan);
        card.appendChild(iconWrap);
        card.appendChild(content);
        el.appendChild(card);
    }

    window.renderTotalPapsKpi = renderTotalPapsKpi;

    /** Auto-fill any placeholder: [data-kpi-total-paps] with optional data-value and data-label. */
    document.querySelectorAll('[data-kpi-total-paps]').forEach(function (node) {
        var v = node.getAttribute('data-value');
        var label = node.getAttribute('data-label');
        var opts = {};
        if (label != null && label !== '') opts.label = label;
        if (v != null && v !== '') {
            var n = Number(String(v).replace(/,/g, ''));
            opts.value = !isNaN(n) ? n : v;
        }
        renderTotalPapsKpi(node, opts);
    });

    if (typeof Chart === 'undefined' || typeof ChartDataLabels === 'undefined') {
        return;
    }

    Chart.register(ChartDataLabels);

    const growthAxis = '#1e8449';
    const papBlue = '#4a90e2';
    const papOrange = '#f58231';
    const papGrey = '#a9a9a9';
    const gridColor = '#e0e0e0';
    const fontFamily = 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
    const labelsGrowth = ['G', 'R', 'O', 'W', 'T', 'H'];

    const pieColors = ['#5B9BD5', '#ED7D31', '#A5A5A5', '#FFC000', '#4472C4', '#70AD47'];

    /** Optional server payload from user dashboard (#pap-status-grouped-chart-data). */
    var groupedDataJson = null;
    var groupedDataEl = document.getElementById('pap-status-grouped-chart-data');
    if (groupedDataEl && groupedDataEl.textContent) {
        try {
            groupedDataJson = JSON.parse(groupedDataEl.textContent);
        } catch (e) {
            groupedDataJson = null;
        }
    }

    var defaultNotStarted = [120, 80, 200, 150, 90, 180];
    var defaultOngoing = [80, 150, 60, 100, 120, 50];
    var defaultCompleted = [50, 70, 30, 100, 40, 80];

    var rawNotStarted = groupedDataJson && Array.isArray(groupedDataJson.notYetStarted) && groupedDataJson.notYetStarted.length === 6
        ? groupedDataJson.notYetStarted
        : defaultNotStarted;
    var rawOngoing = groupedDataJson && Array.isArray(groupedDataJson.ongoing) && groupedDataJson.ongoing.length === 6
        ? groupedDataJson.ongoing
        : defaultOngoing;
    var rawCompleted = groupedDataJson && Array.isArray(groupedDataJson.completed) && groupedDataJson.completed.length === 6
        ? groupedDataJson.completed
        : defaultCompleted;

    var pillarPapCounts = groupedDataJson && Array.isArray(groupedDataJson.pillarPapCounts) && groupedDataJson.pillarPapCounts.length === 6
        ? groupedDataJson.pillarPapCounts
        : null;

    /** Office-wide total PAPs (same as KPI); used if legacy JSON has no per-pillar counts. */
    var totalPapsForChart = 1750;
    if (groupedDataJson && typeof groupedDataJson.totalPaps === 'number' && groupedDataJson.totalPaps > 0) {
        totalPapsForChart = groupedDataJson.totalPaps;
    }

    /** Demo: hypothetical PAP counts per G–H letter when not using live JSON. */
    var defaultPillarTotals = [220, 180, 250, 200, 160, 210];

    var pillarDenoms = [];
    if (pillarPapCounts) {
        pillarDenoms = pillarPapCounts;
    } else if (groupedDataJson) {
        var t = totalPapsForChart > 0 ? totalPapsForChart : 1;
        for (var j = 0; j < labelsGrowth.length; j++) {
            pillarDenoms.push(t);
        }
    } else {
        pillarDenoms = defaultPillarTotals;
    }

    /** Each bar: 100 × (PAPs with that status in pillar) / (PAPs in that pillar in DB). */
    function papsPercentPerPillar(countArr, denoms) {
        var out = [];
        for (var i = 0; i < labelsGrowth.length; i++) {
            var d = Number(denoms[i]) || 0;
            if (d <= 0) {
                out.push(0);
            } else {
                out.push((100 * (Number(countArr[i]) || 0)) / d);
            }
        }
        return out;
    }

    var dataNotStartedPct = papsPercentPerPillar(rawNotStarted, pillarDenoms);
    var dataOngoingPct = papsPercentPerPillar(rawOngoing, pillarDenoms);
    var dataCompletedPct = papsPercentPerPillar(rawCompleted, pillarDenoms);

    /** Pie: PAP counts per G–H from live data, else demo slices. */
    var defaultPieSlices = [18, 12, 25, 8, 15, 22];
    var pieDataValues;
    if (pillarPapCounts) {
        pieDataValues = pillarPapCounts.map(function (x) { return Number(x) || 0; });
    } else if (groupedDataJson) {
        pieDataValues = defaultPieSlices.slice();
    } else {
        pieDataValues = defaultPillarTotals.map(function (x) { return Number(x) || 0; });
    }

    var pieOfficeTotalForLabels = (groupedDataJson && typeof groupedDataJson.totalPaps === 'number' && groupedDataJson.totalPaps > 0)
        ? groupedDataJson.totalPaps
        : null;

    /** Pie caption: "G 1/11(9%)" — count in pillar / office total PAPs (live) or sum of slices (demo); % rounded. */
    function pieGrowthCaption(data, officeTotal) {
        var sumSlices = data.reduce(function (a, b) { return a + (Number(b) || 0); }, 0);
        var totalBase = (typeof officeTotal === 'number' && officeTotal > 0) ? officeTotal : sumSlices;
        return labelsGrowth.map(function (letter, i) {
            var v = Number(data[i]) || 0;
            var tb = totalBase > 0 ? totalBase : 0;
            var pct = totalBase > 0 ? Math.round((v / totalBase) * 100) : 0;
            return letter + ' ' + v + '/' + tb + '(' + pct + '%)';
        }).join('    ');
    }

    function pieSliceFractionLabel(count, dataArr, officeTotal) {
        var sumSlices = dataArr.reduce(function (a, b) { return a + (Number(b) || 0); }, 0);
        var totalBase = (typeof officeTotal === 'number' && officeTotal > 0) ? officeTotal : sumSlices;
        var v = Number(count) || 0;
        var tb = totalBase > 0 ? totalBase : 0;
        var pct = totalBase > 0 ? Math.round((v / totalBase) * 100) : 0;
        return v + '/' + tb + '(' + pct + '%)';
    }

    /** Legend row centered under the chart (matches Status of PAPs reference layout). */
    const legendBottom = {
        position: 'bottom',
        align: 'center',
        labels: {
            font: { family: fontFamily, size: 13, weight: '500' },
            color: '#212529',
            boxWidth: 14,
            boxHeight: 14,
            padding: 20,
            usePointStyle: true,
            pointStyle: 'rect'
        }
    };

    const datalabelsBase = {
        font: { family: fontFamily, size: 11, weight: '600' },
        anchor: 'end',
        align: 'top',
        /** Extra space above bar so "100%" labels are not clipped when y max is 100%. */
        offset: 6,
        clip: false
    };

    function yGridOnly(tickFontSize) {
        const fs = tickFontSize || 12;
        return {
            x: {
                grid: { display: false, drawBorder: false },
                ticks: { font: { family: fontFamily, size: fs }, maxRotation: 0 }
            },
            y: {
                beginAtZero: true,
                grid: {
                    display: true,
                    drawBorder: false,
                    color: gridColor,
                    lineWidth: 1
                },
                ticks: { font: { family: fontFamily, size: fs - 1 } }
            }
        };
    }

    new Chart(document.getElementById('chartStatusPapGrouped'), {
        type: 'bar',
        data: {
            labels: labelsGrowth,
            datasets: [
                {
                    label: 'Not yet started',
                    data: dataNotStartedPct,
                    rawCounts: rawNotStarted,
                    backgroundColor: papBlue,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 32
                },
                {
                    label: 'Ongoing',
                    data: dataOngoingPct,
                    rawCounts: rawOngoing,
                    backgroundColor: papOrange,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 32
                },
                {
                    label: 'Completed',
                    data: dataCompletedPct,
                    rawCounts: rawCompleted,
                    backgroundColor: papGrey,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 32
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                /** Top inset leaves room for % labels above bars at 100% (chart area max is still 100%). */
                padding: { top: 28, bottom: 8, left: 8, right: 8 }
            },
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    display: true,
                    ...legendBottom,
                    onClick: function () {
                        /* Legend labels are visual only; do not hide/show series on click. */
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var base = ctx.dataset.label ? ctx.dataset.label + ': ' : '';
                            var count = (ctx.dataset.rawCounts && ctx.dataset.rawCounts[ctx.dataIndex] != null)
                                ? Number(ctx.dataset.rawCounts[ctx.dataIndex])
                                : 0;
                            var pctVal = ctx.parsed.y;
                            var pctR = typeof pctVal === 'number' && !isNaN(pctVal) ? Math.round(pctVal) : 0;
                            var d = pillarDenoms[ctx.dataIndex] != null ? Number(pillarDenoms[ctx.dataIndex]) : 0;
                            return base + count + '/' + d + ' (' + pctR + '%)';
                        }
                    }
                },
                datalabels: {
                    ...datalabelsBase,
                    font: { family: fontFamily, size: 11, weight: '600' },
                    color: function (ctx) {
                        return ctx.dataset.backgroundColor;
                    },
                    formatter: function (value) {
                        if (typeof value !== 'number' || isNaN(value) || value < 0.05) return '';
                        return Math.round(value) + '%';
                    }
                }
            },
            scales: {
                ...yGridOnly(13),
                y: {
                    ...yGridOnly(13).y,
                    /** Slightly above 100% so bars at 100 don't touch the plot top (datalabels stay visible). */
                    max: 115,
                    ticks: {
                        ...yGridOnly(13).y.ticks,
                        stepSize: 20,
                        callback: function (v) {
                            var n = typeof v === 'number' ? v : Number(v);
                            if (n > 100) {
                                return '';
                            }
                            return n + '%';
                        }
                    }
                }
            }
        }
    });

    /** Accomplishment: % of PAPs in each pillar that have ≥1 quarter Completed (one row = one PAP; same counts as Status "Completed" series). */
    var accomplishmentPapDenoms = [];
    for (var aq = 0; aq < labelsGrowth.length; aq++) {
        if (pillarPapCounts) {
            accomplishmentPapDenoms.push(Number(pillarPapCounts[aq]) || 0);
        } else {
            accomplishmentPapDenoms.push(Number(defaultPillarTotals[aq]) || 0);
        }
    }
    var accomplishmentPcts = papsPercentPerPillar(rawCompleted, accomplishmentPapDenoms);

    new Chart(document.getElementById('chartAccomplishment'), {
        type: 'bar',
        data: {
            labels: labelsGrowth,
            datasets: [{
                label: 'PAPs with completed quarter',
                data: accomplishmentPcts,
                rawCompletedProjects: rawCompleted,
                rawPapsInPillar: accomplishmentPapDenoms,
                backgroundColor: papBlue,
                borderWidth: 0,
                borderRadius: 3,
                maxBarThickness: 38
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: { padding: { top: 24, bottom: 4 } },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var pctVal = ctx.parsed.y;
                            var pctR = typeof pctVal === 'number' && !isNaN(pctVal) ? Math.round(pctVal) : 0;
                            var rawC = (ctx.dataset.rawCompletedProjects && ctx.dataset.rawCompletedProjects[ctx.dataIndex] != null)
                                ? Number(ctx.dataset.rawCompletedProjects[ctx.dataIndex])
                                : 0;
                            var rawM = (ctx.dataset.rawPapsInPillar && ctx.dataset.rawPapsInPillar[ctx.dataIndex] != null)
                                ? Number(ctx.dataset.rawPapsInPillar[ctx.dataIndex])
                                : 0;
                            return (ctx.dataset.label || '') + ': ' + pctR + '% (' + rawC + '/' + rawM + ' PAPs)';
                        }
                    }
                },
                datalabels: {
                    ...datalabelsBase,
                    color: papBlue,
                    formatter: function (value) {
                        if (typeof value !== 'number' || isNaN(value) || value < 0.5) return '';
                        return Math.round(value) + '%';
                    }
                }
            },
            scales: {
                ...yGridOnly(12),
                y: {
                    ...yGridOnly(12).y,
                    max: 115,
                    ticks: {
                        ...yGridOnly(12).y.ticks,
                        stepSize: 20,
                        callback: function (v) {
                            var n = typeof v === 'number' ? v : Number(v);
                            if (n > 100) {
                                return '';
                            }
                            return n + '%';
                        }
                    }
                }
            }
        }
    });

    var growthEl = document.getElementById('chartGrowthIndicator');
    if (growthEl) {
        new Chart(growthEl, {
            type: 'bar',
            data: {
                labels: labelsGrowth,
                datasets: [{
                    label: 'GROWTH',
                    data: [99, 200, 150, 120, 80, 175],
                    backgroundColor: growthAxis,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 38
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: { padding: { top: 8, bottom: 4 } },
                plugins: {
                    legend: { display: false },
                    datalabels: {
                        ...datalabelsBase,
                        color: growthAxis,
                        formatter: function (value) {
                            return Math.round(value) + '%';
                        }
                    }
                },
                scales: {
                    ...yGridOnly(12),
                    y: {
                        ...yGridOnly(12).y,
                        max: 200,
                        ticks: {
                            ...yGridOnly(12).y.ticks,
                            stepSize: 50,
                            callback: function (v) {
                                return v + '%';
                            }
                        }
                    }
                }
            }
        });
    }

    const pieDataValuesForChart = pieDataValues;
    var pieTitleText = pieGrowthCaption(pieDataValuesForChart, pieOfficeTotalForLabels);
    new Chart(document.getElementById('chartStatusPie'), {
        type: 'pie',
        data: {
            labels: labelsGrowth,
            datasets: [{
                data: pieDataValuesForChart,
                backgroundColor: pieColors,
                borderWidth: 1,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: { top: 8, right: 12, bottom: 4, left: 12 }
            },
            plugins: {
                legend: { display: false },
                title: {
                    display: true,
                    text: pieTitleText,
                    position: 'bottom',
                    align: 'center',
                    padding: { top: 10, bottom: 2 },
                    font: { family: fontFamily, size: 13, weight: '500' },
                    color: '#212529'
                },
                datalabels: {
                    display: false
                },
                tooltip: {
                    displayColors: false,
                    titleFont: { size: 14, weight: '600' },
                    bodyFont: { size: 13 },
                    padding: 12,
                    callbacks: {
                        title: function (tooltipItems) {
                            if (!tooltipItems.length) return '';
                            return tooltipItems[0].label;
                        },
                        label: function (context) {
                            var v = context.parsed;
                            var data = context.dataset.data;
                            var frac = pieSliceFractionLabel(v, data, pieOfficeTotalForLabels);
                            return v + ' PAP' + (v === 1 ? '' : 's') + ' · ' + frac;
                        }
                    }
                }
            }
        }
    });
})();
