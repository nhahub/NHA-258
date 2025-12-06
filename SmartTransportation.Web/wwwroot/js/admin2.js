// ================== UTILITY FUNCTIONS ==================
function getAntiForgeryToken() {
    const tokenEl = document.getElementById('antiforgery-token');
    return tokenEl ? tokenEl.value : '';
}

// ================== TAB SWITCHING ==================
function switchTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));

    document.getElementById(tabName + '-tab').classList.add('active');
    event.target.classList.add('active');
    window.activeTab = tabName;

    document.querySelector('.tab-content.active').scrollIntoView({ behavior: 'smooth' });
}

// ================== DRIVER FUNCTIONS ==================
async function verifyDriver(driverId, isVerified) {
    try {
        const res = await fetch(`?handler=VerifyDriver&driverId=${driverId}&isVerified=${isVerified}`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        });
        const data = await res.json();

        if (data.success !== false) {
            const verifiedCell = document.getElementById(`driver-verified-${driverId}`);
            verifiedCell.innerHTML = isVerified
                ? '<span class="status-badge status-active">✔ Verified</span>'
                : '<span class="status-badge status-inactive">✖ Not Verified</span>';

            const vehicleButtons = document.querySelectorAll(`#vehicle-body-${driverId} button`);
            vehicleButtons.forEach(btn => {
                btn.disabled = !isVerified;
                btn.style.opacity = isVerified ? '1' : '0.5';
                btn.style.cursor = isVerified ? 'pointer' : 'not-allowed';
            });

            if (!isVerified) {
                const vehicleCells = document.querySelectorAll(`#vehicle-body-${driverId} td[id^="vehicle-verified-"]`);
                vehicleCells.forEach(td => td.innerHTML = '<span class="status-badge status-inactive">✖</span>');
            }
        }
        alert(data.message || 'Operation completed');
    } catch (e) {
        console.error(e);
        alert("An error occurred. Please try again.");
    }
}

async function verifyVehicle(driverId, vehicleId, isVerified) {
    try {
        const res = await fetch(`?handler=VerifyVehicle&driverId=${driverId}&vehicleId=${vehicleId}&isVerified=${isVerified}`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        });
        const data = await res.json();

        if (data.success !== false) {
            const cell = document.getElementById(`vehicle-verified-${vehicleId}`);
            cell.innerHTML = isVerified
                ? '<span class="status-badge status-active">✔</span>'
                : '<span class="status-badge status-inactive">✖</span>';
        }
        alert(data.message || 'Operation completed');
    } catch (e) {
        console.error(e);
        alert("An error occurred. Please try again.");
    }
}

function toggleVehicles(driverId) {
    const row = document.getElementById(`vehicles-${driverId}`);
    row.style.display = row.style.display === 'none' || row.style.display === '' ? 'table-row' : 'none';
}

// ================== ROUTE FORM FUNCTIONS ==================
function toggleRouteForm(mode = 'create') {
    const createForm = document.getElementById('create-route-form');
    if (mode === 'create') {
        const isVisible = createForm.style.display !== 'none';
        createForm.style.display = isVisible ? 'none' : 'block';

        if (!isVisible) {
            resetCreateForm();
            createForm.scrollIntoView({ behavior: 'smooth' });
        }
    }
}

function resetCreateForm() {
    const container = document.getElementById('create-segments-container');
    container.innerHTML = `<div class="segment-row" data-index="0">
        <input name="NewRoute.Segments[0].StartPoint" placeholder="Start Point *" required />
        <input name="NewRoute.Segments[0].EndPoint" placeholder="End Point *" required />
        <input name="NewRoute.Segments[0].SegmentDistanceKm" placeholder="Distance (km)" type="number" step="0.1" min="0" />
        <input name="NewRoute.Segments[0].SegmentEstimatedMinutes" placeholder="Est. Minutes" type="number" min="0" />
        <input name="NewRoute.Segments[0].SegmentOrder" type="hidden" value="1" />
        <button type="button" onclick="removeSegment(this)" class="btn btn-sm btn-delete">✕</button>
    </div>`;

    const form = document.getElementById('create-form');
    Array.from(form.querySelectorAll('input[type="text"], input[type="checkbox"], select')).forEach(input => {
        input.value = '';
        if (input.type === 'checkbox') input.checked = false;
    });
}

function addSegment(formMode = 'create') {
    const containerId = formMode === 'create' ? 'create-segments-container' : 'edit-segments-container';
    const container = document.getElementById(containerId);
    const formPrefix = formMode === 'create' ? 'NewRoute' : 'EditRoute';

    // Always use current number of children as new index
    const index = container.children.length;

    const div = document.createElement('div');
    div.className = 'segment-row';
    div.setAttribute('data-index', index);
    div.innerHTML = `
        <input name="${formPrefix}.Segments[${index}].StartPoint" placeholder="Start Point *" required />
        <input name="${formPrefix}.Segments[${index}].EndPoint" placeholder="End Point *" required />
        <input name="${formPrefix}.Segments[${index}].SegmentDistanceKm" placeholder="Distance (km)" type="number" step="0.1" min="0" />
        <input name="${formPrefix}.Segments[${index}].SegmentEstimatedMinutes" placeholder="Est. Minutes" type="number" min="0" />
        <input name="${formPrefix}.Segments[${index}].SegmentOrder" type="hidden" value="${index + 1}" />
        <button type="button" onclick="removeSegment(this)" class="btn btn-sm btn-delete">✕</button>
    `;
    container.appendChild(div);
}

function reindexSegments(container) {
    // Reindex all children sequentially
    Array.from(container.children).forEach((child, idx) => {
        child.setAttribute('data-index', idx);
        const inputs = child.querySelectorAll('input[name*="Segments"]');
        inputs.forEach(input => {
            // Replace any old index with the new one
            input.name = input.name.replace(/Segments\[\d+\]/, `Segments[${idx}]`);
            if (input.name.includes('SegmentOrder')) input.value = idx + 1;
        });
    });
}

function removeSegment(btn) {
    const row = btn.closest('.segment-row');
    row.remove();
    reindexSegments(row.closest('[id*="segments-container"]'));
}



// ================== ROUTE ACTIONS ==================
function editRoute(routeId) {
    sessionStorage.setItem('activeTab', window.activeTab);
    window.location.href = `?handler=EditRoute&routeId=${routeId}`;
}

function cancelEdit() {
    sessionStorage.setItem('activeTab', 'routes');
    window.location.href = '?';
}

function toggleRouteDetails(routeId) {
    const el = document.getElementById(`route-details-${routeId}`);
    el.style.display = el.style.display === 'none' ? 'block' : 'none';
}

function confirmDelete(routeId, routeName) {
    if (confirm(`Delete route "${routeName}"? This cannot be undone.`)) {
        deleteRoute(routeId);
    }
}

async function deleteRoute(routeId) {
    try {
        const res = await fetch(`?handler=DeleteRoute&routeId=${routeId}`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        });
        const data = await res.json();

        if (data.success) {
            const routeCard = document.querySelector(`.route-card[data-route-id="${routeId}"]`);
            if (routeCard) routeCard.remove();

            window.totalRoutes--;
            window.totalPages = Math.ceil(window.totalRoutes / window.routesPerPage);
            updatePaginationUI();

            alert(data.message || 'Route deleted successfully!');
        } else {
            alert('Error: ' + (data.message || 'Delete failed'));
        }
    } catch (e) {
        console.error(e);
        alert('An error occurred while deleting the route.');
    }
}

// ================== PAGINATION ==================
function changePage(page) {
    if (page < 1 || page > window.totalPages) return;
    window.currentPage = page;
    updatePaginationUI();
}

function updatePaginationUI() {
    const pageNumbersEl = document.getElementById('page-numbers');
    pageNumbersEl.innerHTML = '';

    const maxVisible = 5;
    let startPage = Math.max(1, window.currentPage - Math.floor(maxVisible / 2));
    let endPage = Math.min(window.totalPages, startPage + maxVisible - 1);
    if (endPage - startPage < maxVisible - 1) startPage = Math.max(1, endPage - maxVisible + 1);

    if (startPage > 1) {
        pageNumbersEl.innerHTML += `<span class="page-num" onclick="changePage(1)">1</span>`;
        if (startPage > 2) pageNumbersEl.innerHTML += `<span>...</span>`;
    }

    for (let i = startPage; i <= endPage; i++) {
        pageNumbersEl.innerHTML += `<span class="page-num ${i === window.currentPage ? 'active' : ''}" onclick="changePage(${i})">${i}</span>`;
    }

    if (endPage < window.totalPages) {
        if (endPage < window.totalPages - 1) pageNumbersEl.innerHTML += `<span>...</span>`;
        pageNumbersEl.innerHTML += `<span class="page-num" onclick="changePage(${window.totalPages})">${window.totalPages}</span>`;
    }

    document.getElementById('prev-page').classList.toggle('disabled', window.currentPage <= 1);
    document.getElementById('next-page').classList.toggle('disabled', window.currentPage >= window.totalPages);
    document.getElementById('page-info').textContent = `Page ${window.currentPage} of ${window.totalPages}`;
}

// ================== FORM VALIDATION ==================
function validateRouteForm(mode = 'create') {
    const formId = mode === 'create' ? 'create-form' : 'edit-form';
    const containerId = mode === 'create' ? 'create-segments-container' : 'edit-segments-container';
    const form = document.getElementById(formId);
    const segments = document.getElementById(containerId).querySelectorAll('.segment-row');

    let valid = true;
    let messages = [];

    const routeName = form.querySelector('input[name$=".RouteName"]');
    if (!routeName.value.trim()) {
        valid = false;
        messages.push("Route Name is required.");
        routeName.style.borderColor = 'red';
    } else routeName.style.borderColor = '';

    const routeType = form.querySelector('select[name$=".RouteType"]');
    if (routeType && !routeType.value) {
        valid = false;
        messages.push("Route Type must be selected.");
        routeType.style.borderColor = 'red';
    } else if (routeType) routeType.style.borderColor = '';

    const startLoc = form.querySelector('input[name$=".StartLocation"]');
    const endLoc = form.querySelector('input[name$=".EndLocation"]');
    if (!startLoc.value.trim() || !endLoc.value.trim()) {
        valid = false;
        messages.push("Start and End locations are required.");
        startLoc.style.borderColor = startLoc.value.trim() ? '' : 'red';
        endLoc.style.borderColor = endLoc.value.trim() ? '' : 'red';
    } else { startLoc.style.borderColor = ''; endLoc.style.borderColor = ''; }

    segments.forEach((seg, idx) => {
        const start = seg.querySelector(`input[name*="Segments"][name*="StartPoint"]`);
        const end = seg.querySelector(`input[name*="Segments"][name*="EndPoint"]`);
        if (!start.value.trim() || !end.value.trim()) {
            valid = false;
            messages.push(`Segment ${idx + 1} must have Start and End points.`);
            start.style.borderColor = start.value.trim() ? '' : 'red';
            end.style.borderColor = end.value.trim() ? '' : 'red';
        } else {
            start.style.borderColor = '';
            end.style.borderColor = '';
        }
    });

    if (!valid) alert(messages.join('\n'));
    return valid;
}

// ================== INITIALIZATION ==================
document.addEventListener('DOMContentLoaded', function () {
    const savedTab = sessionStorage.getItem('activeTab');
    if (savedTab) switchTab(savedTab);

    document.getElementById('create-form')?.addEventListener('submit', function (e) {
        if (!validateRouteForm('create')) e.preventDefault();
    });
    document.getElementById('edit-form')?.addEventListener('submit', function (e) {
        if (!validateRouteForm('edit')) e.preventDefault();
    });

    updatePaginationUI();
});
