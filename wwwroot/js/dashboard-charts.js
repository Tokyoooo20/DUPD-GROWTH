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

    /** Bottom line: each GROWTH letter with its share (matches bar chart category row + value). */
    function pieGrowthCaption(data) {
        const total = data.reduce(function (a, b) { return a + b; }, 0);
        return labelsGrowth.map(function (letter, i) {
            const v = data[i];
            const pct = total ? ((v / total) * 100).toFixed(1) : '0.0';
            return letter + ' ' + pct + '%';
        }).join('    ');
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
        offset: 2,
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
                    data: [120, 80, 200, 150, 90, 180],
                    backgroundColor: papBlue,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 32
                },
                {
                    label: 'Ongoing',
                    data: [80, 150, 60, 100, 120, 50],
                    backgroundColor: papOrange,
                    borderWidth: 0,
                    borderRadius: 3,
                    maxBarThickness: 32
                },
                {
                    label: 'Completed',
                    data: [50, 70, 30, 100, 40, 80],
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
                padding: { top: 8, bottom: 8, left: 8, right: 8 }
            },
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    display: true,
                    ...legendBottom
                },
                datalabels: {
                    ...datalabelsBase,
                    font: { family: fontFamily, size: 11, weight: '600' },
                    color: function (ctx) {
                        return ctx.dataset.backgroundColor;
                    },
                    formatter: function (value) {
                        return value;
                    }
                }
            },
            scales: {
                ...yGridOnly(13),
                y: {
                    ...yGridOnly(13).y,
                    max: 250,
                    ticks: {
                        ...yGridOnly(13).y.ticks,
                        stepSize: 50,
                        callback: function (v) { return v; }
                    }
                }
            }
        }
    });

    new Chart(document.getElementById('chartAccomplishment'), {
        type: 'bar',
        data: {
            labels: labelsGrowth,
            datasets: [{
                label: 'Accomplishment',
                data: [15.07, 12.34, 18.99, 8.50, 5.25, 19.12],
                backgroundColor: papBlue,
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
                    color: papBlue,
                    formatter: function (value) {
                        return Number(value).toFixed(2) + '%';
                    }
                }
            },
            scales: {
                ...yGridOnly(12),
                y: {
                    ...yGridOnly(12).y,
                    max: 20,
                    ticks: {
                        ...yGridOnly(12).y.ticks,
                        stepSize: 5,
                        callback: function (v) {
                            return Number(v).toFixed(2) + '%';
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

    const pieDataValues = [18, 12, 25, 8, 15, 22];
    new Chart(document.getElementById('chartStatusPie'), {
        type: 'pie',
        data: {
            labels: labelsGrowth,
            datasets: [{
                data: pieDataValues,
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
                    text: pieGrowthCaption(pieDataValues),
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
                            const data = context.dataset.data;
                            const total = data.reduce(function (a, b) { return a + b; }, 0);
                            const v = context.parsed;
                            const pct = total ? ((v / total) * 100).toFixed(1) : '0.0';
                            return pct + '%';
                        }
                    }
                }
            }
        }
    });
})();
