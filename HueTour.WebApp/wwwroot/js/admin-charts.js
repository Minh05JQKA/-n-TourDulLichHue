// admin-charts.js - Chart.js handler and dashboard actions for Hue Tour Portal
window.hueDashboardCharts = {
    instances: {},

    destroyChart: function (canvasId) {
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
            delete this.instances[canvasId];
        }
    },

    renderRevenueChart: function (canvasId, labels, revenueData, orderData, viewMode) {
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js not loaded yet');
            return false;
        }

        var canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        this.destroyChart(canvasId);

        var isDark = document.documentElement.dataset.theme === 'dark';
        var textColor = isDark ? '#d6ded8' : '#263331';
        var gridColor = isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.06)';

        var datasets = [];

        var showRevenue = viewMode === 'all' || viewMode === 'revenue';
        if (showRevenue) {
            datasets.push({
                type: 'bar',
                label: 'Doanh thu (VNĐ)',
                data: revenueData,
                backgroundColor: 'rgba(24, 95, 87, 0.78)',
                borderColor: '#185f57',
                borderWidth: 2,
                borderRadius: 8,
                yAxisID: 'yRevenue',
                order: 2
            });
        }

        if (viewMode === 'all' || viewMode === 'orders') {
            datasets.push({
                type: 'line',
                label: 'Số lượng đơn đặt',
                data: orderData,
                borderColor: '#c18b2f',
                backgroundColor: 'rgba(193, 139, 47, 0.25)',
                borderWidth: 3,
                pointBackgroundColor: '#c18b2f',
                pointBorderColor: '#fff',
                pointHoverRadius: 7,
                pointRadius: 5,
                fill: false,
                tension: 0.35,
                yAxisID: viewMode === 'all' ? 'yOrders' : 'yOrdersOnly',
                order: 1
            });
        }

        var scalesConfig = {
            x: {
                grid: { color: gridColor },
                ticks: { color: textColor, font: { family: 'Plus Jakarta Sans', weight: '600' } }
            },
            yRevenue: {
                type: 'linear',
                display: showRevenue,
                position: 'left',
                grid: { color: gridColor },
                ticks: {
                    color: textColor,
                    callback: function (value) {
                        if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M đ';
                        if (value >= 1000) return (value / 1000).toFixed(0) + 'k đ';
                        return value;
                    }
                }
            }
        };

        if (viewMode === 'all') {
            scalesConfig.yOrders = {
                type: 'linear',
                display: true,
                position: 'right',
                grid: { drawOnChartArea: false },
                ticks: {
                    color: '#c18b2f',
                    stepSize: 1,
                    callback: function (val) { return val + ' đơn'; }
                }
            };
        }
        else if (viewMode === 'orders') {
            scalesConfig.yOrdersOnly = {
                type: 'linear',
                display: true,
                position: 'left',
                beginAtZero: true,
                ticks: {
                    color: textColor,
                    precision: 0,
                    callback: function (value) { return value + ' đơn'; }
                },
                grid: { color: gridColor }
            };
        }

        var ctx = canvas.getContext('2d');
        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            color: textColor,
                            font: { family: 'Plus Jakarta Sans', weight: '600' },
                            usePointStyle: true,
                            padding: 15
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(16, 62, 58, 0.95)',
                        titleColor: '#efd17f',
                        bodyColor: '#fff',
                        padding: 12,
                        cornerRadius: 10,
                        callbacks: {
                            label: function (context) {
                                if (context.dataset.yAxisID === 'yRevenue' && viewMode !== 'orders') {
                                    return context.dataset.label + ': ' + Number(context.parsed.y).toLocaleString('vi-VN') + ' đ';
                                }
                                return context.dataset.label + ': ' + context.parsed.y + ' đơn';
                            }
                        }
                    }
                },
                scales: scalesConfig
            }
        });

        return true;
    },

    renderStatusChart: function (canvasId, labels, data, colors) {
        if (typeof Chart === 'undefined') return false;

        var canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        this.destroyChart(canvasId);

        var isDark = document.documentElement.dataset.theme === 'dark';
        var textColor = isDark ? '#d6ded8' : '#263331';

        var ctx = canvas.getContext('2d');
        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors && colors.length ? colors : [
                        '#28a745', // Confirmed (Green)
                        '#f59e0b', // Pending (Amber)
                        '#185f57', // Completed (Jade)
                        '#e11d48'  // Cancelled (Rose)
                    ],
                    borderColor: isDark ? '#1a2723' : '#ffffff',
                    borderWidth: 3,
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: textColor,
                            font: { family: 'Plus Jakarta Sans', weight: '600', size: 12 },
                            usePointStyle: true,
                            padding: 12
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(16, 62, 58, 0.95)',
                        titleColor: '#efd17f',
                        bodyColor: '#fff',
                        padding: 10,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                var total = context.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var val = context.parsed;
                                var pct = total > 0 ? ((val / total) * 100).toFixed(1) + '%' : '0%';
                                return ' ' + context.label + ': ' + val + ' đơn (' + pct + ')';
                            }
                        }
                    }
                }
            }
        });

        return true;
    },

    renderOccupancyChart: function (canvasId, labels, bookedData, availableData) {
        if (typeof Chart === 'undefined') return false;

        var canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        this.destroyChart(canvasId);

        var isDark = document.documentElement.dataset.theme === 'dark';
        var textColor = isDark ? '#d6ded8' : '#263331';
        var gridColor = isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.06)';

        var ctx = canvas.getContext('2d');
        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Số ghế đã đặt',
                        data: bookedData,
                        backgroundColor: 'rgba(193, 139, 47, 0.85)',
                        borderRadius: 6
                    },
                    {
                        label: 'Còn chỗ',
                        data: availableData,
                        backgroundColor: isDark ? 'rgba(255, 255, 255, 0.18)' : 'rgba(24, 95, 87, 0.24)',
                        borderRadius: 6
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        stacked: true,
                        beginAtZero: true,
                        grid: { color: gridColor },
                        ticks: { color: textColor, font: { family: 'Plus Jakarta Sans' } }
                    },
                    y: {
                        stacked: true,
                        grid: { display: false },
                        ticks: {
                            color: textColor,
                            font: { family: 'Plus Jakarta Sans', weight: '600' }
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            color: textColor,
                            font: { family: 'Plus Jakarta Sans', weight: '600' },
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(16, 62, 58, 0.95)',
                        titleColor: '#efd17f',
                        callbacks: {
                            afterLabel: function (context) {
                                if (context.datasetIndex === 0) {
                                    var cap = bookedData[context.dataIndex] + availableData[context.dataIndex];
                                    var pct = cap > 0 ? ((bookedData[context.dataIndex] / cap) * 100).toFixed(1) : '0.0';
                                    return 'Tỷ lệ lấp đầy: ' + pct + '%';
                                }
                                return '';
                            }
                        }
                    }
                }
            }
        });

        return true;
    },

    renderCategoryRevenueChart: function (canvasId, labels, revenueData) {
        if (typeof Chart === 'undefined') return false;

        var canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        this.destroyChart(canvasId);

        var isDark = document.documentElement.dataset.theme === 'dark';
        var textColor = isDark ? '#d6ded8' : '#263331';
        var gridColor = isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.06)';
        var colors = ['#582c83', '#c18b2f', '#185f57', '#0284c7', '#db6b37', '#7c3aed'];

        this.instances[canvasId] = new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Doanh thu đã ghi nhận',
                    data: revenueData,
                    backgroundColor: labels.map(function (_, index) { return colors[index % colors.length]; }),
                    borderRadius: 7,
                    maxBarThickness: 28
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: { color: gridColor },
                        ticks: {
                            color: textColor,
                            callback: function (value) {
                                if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                                if (value >= 1000) return Math.round(value / 1000) + 'k';
                                return value;
                            }
                        }
                    },
                    y: {
                        grid: { display: false },
                        ticks: { color: textColor, font: { family: 'Plus Jakarta Sans', weight: '600' } }
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(16, 62, 58, 0.95)',
                        titleColor: '#efd17f',
                        bodyColor: '#fff',
                        padding: 10,
                        callbacks: {
                            label: function (context) {
                                return ' ' + Number(context.parsed.x).toLocaleString('vi-VN') + ' đ';
                            }
                        }
                    }
                }
            }
        });

        return true;
    },

    printReport: function () {
        window.print();
    }
};
