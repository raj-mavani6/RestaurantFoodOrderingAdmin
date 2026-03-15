// ===================================
// Chart.js Configuration & Functions
// ===================================

// Global Chart Configuration (only if Chart.js is loaded)
if (typeof Chart !== 'undefined') {
    Chart.defaults.font.family = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
    Chart.defaults.color = '#6C757D';
    Chart.defaults.plugins.legend.display = true;
    Chart.defaults.plugins.legend.position = 'bottom';
}

// Color Palette
const chartColors = {
    primary: '#C1121F',
    secondary: '#780000',
    success: '#28A745',
    danger: '#DC3545',
    warning: '#FFC107',
    info: '#17A2B8',
    blue: '#4E73DF',
    green: '#1CC88A',
    orange: '#FF5722',
    purple: '#9C27B0',
    cyan: '#00BCD4',
    yellow: '#F6C23E'
};

// Sales Chart
let salesChart = null;

function createSalesChart() {
    const ctx = document.getElementById('salesChart');

    if (!ctx) {
        console.error('Sales chart canvas not found');
        return;
    }

    // Check if chart data exists
    if (typeof salesChartDates === 'undefined' || typeof salesChartAmounts === 'undefined') {
        console.error('Sales chart data not provided');
        return;
    }

    // Destroy existing chart if it exists
    if (salesChart) {
        salesChart.destroy();
    }

    // Create gradient
    const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(193, 18, 31, 0.2)');
    gradient.addColorStop(1, 'rgba(193, 18, 31, 0)');

    salesChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: salesChartDates,
            datasets: [{
                label: 'Sales (₹)',
                data: salesChartAmounts,
                backgroundColor: gradient,
                borderColor: chartColors.primary,
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: chartColors.primary,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7,
                pointHoverBackgroundColor: chartColors.primary,
                pointHoverBorderColor: '#fff',
                pointHoverBorderWidth: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: chartColors.primary,
                    borderWidth: 1,
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'Sales: ₹' + context.parsed.y.toLocaleString('en-IN');
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return '₹' + value.toLocaleString('en-IN');
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'index'
            }
        }
    });
}

// Order Status Pie Chart
let orderStatusChart = null;

function createOrderStatusChart() {
    const ctx = document.getElementById('orderStatusChart');

    if (!ctx) {
        console.error('Order status chart canvas not found');
        return;
    }

    // Check if chart data exists
    if (typeof orderStatusData === 'undefined') {
        console.error('Order status data not provided');
        return;
    }

    // Destroy existing chart if it exists
    if (orderStatusChart) {
        orderStatusChart.destroy();
    }

    const data = [
        orderStatusData.pending,
        orderStatusData.confirmed,
        orderStatusData.preparing,
        orderStatusData.delivery,
        orderStatusData.delivered,
        orderStatusData.cancelled
    ];

    const labels = ['Pending', 'Confirmed', 'Preparing', 'Out for Delivery', 'Delivered', 'Cancelled'];

    const colors = [
        '#FFC107', // Pending - Yellow
        '#17A2B8', // Confirmed - Cyan
        '#FF5722', // Preparing - Orange
        '#9C27B0', // Out for Delivery - Purple
        '#28A745', // Delivered - Green
        '#DC3545'  // Cancelled - Red
    ];

    orderStatusChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderColor: '#fff',
                borderWidth: 3,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        usePointStyle: true,
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    callbacks: {
                        label: function (context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%'
        }
    });
}

// NOTE: Filter button click handlers are now managed via onclick attributes in the HTML
// The loadSalesChart function handles all chart updates

// Refresh Charts
function refreshCharts() {
    createSalesChart();
    createOrderStatusChart();
}

// Export Chart as Image
function exportChartAsImage(chartId, filename) {
    const canvas = document.getElementById(chartId);
    if (canvas) {
        const url = canvas.toDataURL('image/png');
        const link = document.createElement('a');
        link.download = filename || 'chart.png';
        link.href = url;
        link.click();
    }
}

// Resize charts on window resize
window.addEventListener('resize', function () {
    if (salesChart) {
        salesChart.resize();
    }
    if (orderStatusChart) {
        orderStatusChart.resize();
    }
});

// ===================================
// Sales Reports Chart Functions
// ===================================

// Load Sales Chart with period filter
function loadSalesChart(period) {
    console.log('loadSalesChart called with period:', period);

    // Update active button
    const allBtns = document.querySelectorAll('.chart-filters .filter-btn');
    allBtns.forEach(function (btn) {
        btn.classList.remove('active');
    });
    const activeBtn = document.querySelector('.filter-btn[data-filter="' + period + '"]');
    if (activeBtn) {
        activeBtn.classList.add('active');
    }

    // Check canvas exists
    const chartContainer = document.querySelector('.chart-container');
    if (!chartContainer) {
        console.error('Chart container not found');
        return;
    }

    console.log('Fetching chart data for period:', period);

    // Fetch chart data
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Report/GetSalesChartData?period=' + period, true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    console.log('Chart data received:', data);
                    if (data.labels && data.values) {
                        renderSalesChart(data.labels, data.values);
                    } else {
                        console.error('Invalid data format');
                    }
                } catch (e) {
                    console.error('JSON parse error:', e);
                }
            } else {
                console.error('Request failed with status:', xhr.status);
            }
        }
    };
    xhr.send();
}

// Render Sales Chart
function renderSalesChart(labels, values) {
    console.log('renderSalesChart called');

    // Get chart container
    var chartContainer = document.querySelector('.chart-container');
    if (!chartContainer) {
        console.error('Chart container not found');
        return;
    }

    // Remove existing canvas and create new one
    chartContainer.innerHTML = '<canvas id="salesChart"></canvas>';

    var chartCanvas = document.getElementById('salesChart');
    if (!chartCanvas) {
        console.error('Could not create canvas');
        return;
    }

    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded');
        return;
    }

    var ctx = chartCanvas.getContext('2d');

    // Create gradient
    var gradient = ctx.createLinearGradient(0, 0, 0, 350);
    gradient.addColorStop(0, 'rgba(193, 18, 31, 0.3)');
    gradient.addColorStop(1, 'rgba(193, 18, 31, 0)');

    // Create chart
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Sales (₹)',
                data: values,
                backgroundColor: gradient,
                borderColor: '#C1121F',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#C1121F',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'Sales: ₹' + context.parsed.y.toLocaleString('en-IN');
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return '₹' + value.toLocaleString('en-IN');
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
    console.log('Chart created successfully');
}

// Initialize Sales Chart on page load (if canvas exists)
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('salesChart')) {
        loadSalesChart('monthly');
    }
});

// ===================================
// Order Reports Chart Functions
// ===================================

// Load Order Chart with period filter
function loadOrderChart(period) {
    console.log('loadOrderChart called with period:', period);

    // Update active button
    var allBtns = document.querySelectorAll('.chart-filters .filter-btn');
    allBtns.forEach(function (btn) {
        btn.classList.remove('active');
    });
    var activeBtn = document.querySelector('.filter-btn[data-filter="' + period + '"]');
    if (activeBtn) {
        activeBtn.classList.add('active');
    }

    // Check container exists
    var chartContainer = document.querySelector('.order-chart-container');
    if (!chartContainer) {
        console.error('Order chart container not found');
        return;
    }

    console.log('Fetching order chart data for period:', period);

    // Fetch chart data
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Report/GetOrderChartData?period=' + period, true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    console.log('Order chart data received:', data);
                    if (data.labels && data.values) {
                        renderOrderChart(data.labels, data.values);
                    } else {
                        console.error('Invalid data format');
                    }
                } catch (e) {
                    console.error('JSON parse error:', e);
                }
            } else {
                console.error('Request failed with status:', xhr.status);
            }
        }
    };
    xhr.send();
}

// Render Order Chart
function renderOrderChart(labels, values) {
    console.log('renderOrderChart called');

    // Get chart container
    var chartContainer = document.querySelector('.order-chart-container');
    if (!chartContainer) {
        console.error('Order chart container not found');
        return;
    }

    // Remove existing canvas and create new one
    chartContainer.innerHTML = '<canvas id="orderChart"></canvas>';

    var chartCanvas = document.getElementById('orderChart');
    if (!chartCanvas) {
        console.error('Could not create canvas');
        return;
    }

    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded');
        return;
    }

    var ctx = chartCanvas.getContext('2d');

    // Create gradient
    var gradient = ctx.createLinearGradient(0, 0, 0, 350);
    gradient.addColorStop(0, 'rgba(78, 115, 223, 0.8)');
    gradient.addColorStop(1, 'rgba(78, 115, 223, 0.2)');

    // Create chart (Bar chart for orders)
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Orders',
                data: values,
                backgroundColor: gradient,
                borderColor: '#4E73DF',
                borderWidth: 2,
                borderRadius: 5,
                hoverBackgroundColor: '#4E73DF'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'Orders: ' + context.parsed.y;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        callback: function (value) {
                            if (Number.isInteger(value)) {
                                return value;
                            }
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
    console.log('Order chart created successfully');
}

// Initialize Order Chart on page load (if canvas exists)
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('orderChart')) {
        loadOrderChart('monthly');
    }
});

// ===================================
// Customer Reports Chart Functions
// ===================================

// Load Customer Chart with period filter
function loadCustomerChart(period) {
    console.log('loadCustomerChart called with period:', period);

    // Update active button
    var allBtns = document.querySelectorAll('.chart-filters .filter-btn');
    allBtns.forEach(function (btn) {
        btn.classList.remove('active');
    });
    var activeBtn = document.querySelector('.filter-btn[data-filter="' + period + '"]');
    if (activeBtn) {
        activeBtn.classList.add('active');
    }

    // Check container exists
    var chartContainer = document.querySelector('.customer-chart-container');
    if (!chartContainer) {
        console.error('Customer chart container not found');
        return;
    }

    console.log('Fetching customer chart data for period:', period);

    // Fetch chart data
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Report/GetCustomerChartData?period=' + period, true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    console.log('Customer chart data received:', data);
                    if (data.labels && data.values) {
                        renderCustomerChart(data.labels, data.values);
                    } else {
                        console.error('Invalid data format');
                    }
                } catch (e) {
                    console.error('JSON parse error:', e);
                }
            } else {
                console.error('Request failed with status:', xhr.status);
            }
        }
    };
    xhr.send();
}

// Render Customer Chart
function renderCustomerChart(labels, values) {
    console.log('renderCustomerChart called');

    // Get chart container
    var chartContainer = document.querySelector('.customer-chart-container');
    if (!chartContainer) {
        console.error('Customer chart container not found');
        return;
    }

    // Remove existing canvas and create new one
    chartContainer.innerHTML = '<canvas id="customerChart"></canvas>';

    var chartCanvas = document.getElementById('customerChart');
    if (!chartCanvas) {
        console.error('Could not create canvas');
        return;
    }

    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded');
        return;
    }

    var ctx = chartCanvas.getContext('2d');

    // Create gradient (Green theme for customers)
    var gradient = ctx.createLinearGradient(0, 0, 0, 350);
    gradient.addColorStop(0, 'rgba(40, 167, 69, 0.4)');
    gradient.addColorStop(1, 'rgba(40, 167, 69, 0)');

    // Create chart (Area chart for customers)
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'New Customers',
                data: values,
                backgroundColor: gradient,
                borderColor: '#28a745',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#28a745',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'New Customers: ' + context.parsed.y;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        callback: function (value) {
                            if (Number.isInteger(value)) {
                                return value;
                            }
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
    console.log('Customer chart created successfully');
}

// Initialize Customer Chart on page load (if canvas exists)
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('customerChart')) {
        loadCustomerChart('monthly');
    }
});
