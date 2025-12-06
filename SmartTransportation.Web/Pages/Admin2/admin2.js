// ==========================
// INITIALIZATION
// ==========================
document.addEventListener("DOMContentLoaded", () => {
    loadDrivers();
    loadRoutes(currentPage);
});

// ==========================
// GLOBALS
// ==========================
let currentPage = 1;
let totalPages = 1;
let currentDriverId = null;

// ==========================
// DRIVER / VEHICLE FUNCTIONS
// ==========================
async function loadDrivers() {
    try {
        const token = localStorage.getItem("jwt");
        const res = await fetch("/api/admin/drivers", {
            headers: { "Authorization": "Bearer " + token }
        });
        if (!res.ok) throw new Error("Failed to fetch drivers");

        const drivers = await res.json();
        const tbody = document.querySelector("#driversTable tbody");
        tbody.innerHTML = "";

        if (!drivers.length) {
            tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;">No drivers found</td></tr>`;
            return;
        }

        drivers.forEach(d => {
            tbody.innerHTML += `
                <tr>
                    <td>${d.fullName}</td>
                    <td>${d.phone}</td>
                    <td>${d.city}</td>
                    <td>${d.driverRating ?? "N/A"}</td>
                    <td>${d.isDriverVerified ? "✔ Verified" : "❌ Not Verified"}</td>
                    <td>
                        <button onclick="viewDriver(${d.driverId})" class="btn btn-primary btn-sm">View</button>
                    </td>
                </tr>
            `;
        });
    } catch (err) {
        console.error(err);
        alert("Error loading drivers: " + err.message);
    }
}

async function viewDriver(driverId) {
    try {
        const token = localStorage.getItem("jwt");
        const res = await fetch(`/api/admin/driver/${driverId}`, {
            headers: { "Authorization": "Bearer " + token }
        });
        if (!res.ok) throw new Error("Driver not found");

        const full = await res.json();
        const d = full.driver;
        const v = full.vehicle;

        document.getElementById("driverDetails").style.display = "block";
        document.getElementById("driverProfile").innerHTML = `
            <p><strong>Name:</strong> ${d.fullName}</p>
            <p><strong>Phone:</strong> ${d.phone}</p>
            <p><strong>City:</strong> ${d.city}</p>
            <p><strong>License:</strong> ${d.driverLicenseNumber ?? "N/A"}</p>
            <p><strong>Rating:</strong> ${d.driverRating ?? "N/A"}</p>
            <p><strong>Verified:</strong> ${d.isDriverVerified ? "✔ Yes" : "❌ No"}</p>
        `;

        document.getElementById("vehicleProfile").innerHTML = `
            <p><strong>Make:</strong> ${v.vehicleMake}</p>
            <p><strong>Model:</strong> ${v.vehicleModel}</p>
            <p><strong>Year:</strong> ${v.vehicleYear}</p>
            <p><strong>Plate:</strong> ${v.plateNumber}</p>
            <p><strong>Seats:</strong> ${v.seatsCount}</p>
            <p><strong>Verified:</strong> ${v.isVerified ? "✔ Yes" : "❌ No"}</p>
        `;

        currentDriverId = driverId;
    } catch (err) {
        console.error(err);
        alert("Error loading driver details: " + err.message);
    }
}

async function verifyDriver(driverId, isVerified = true) {
    try {
        const res = await fetch("/api/admin/driver/verify", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + localStorage.getItem("jwt")
            },
            body: JSON.stringify({ driverId, isVerified })
        });
        if (!res.ok) throw new Error("Failed to verify driver");

        alert(`Driver ${isVerified ? "Verified ✔" : "Unverified ✖"}`);
        loadDrivers();
    } catch (err) {
        console.error(err);
        alert("Error verifying driver: " + err.message);
    }
}

async function verifyVehicle(driverId, isVerified = true) {
    try {
        const res = await fetch("/api/admin/vehicle/verify", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + localStorage.getItem("jwt")
            },
            body: JSON.stringify({ driverId, isVerified })
        });
        if (!res.ok) throw new Error("Failed to verify vehicle");

        alert(`Vehicle ${isVerified ? "Verified ✔" : "Unverified ✖"}`);
        loadDrivers();
    } catch (err) {
        console.error(err);
        alert("Error verifying vehicle: " + err.message);
    }
}

// ==========================
// ROUTE / SEGMENT FUNCTIONS
// ==========================
function toggleRouteForm(mode) {
    const el = document.getElementById(mode + "-route-form");
    if (!el) return;
    el.style.display = el.style.display === "none" || el.style.display === "" ? "block" : "none";
}

function addSegment(mode) {
    const container = document.getElementById(mode + "-segments-container");
    if (!container) return;

    const index = container.children.length;
    const div = document.createElement("div");
    div.className = "segment-row";
    div.dataset.index = index;
    div.innerHTML = `
        <input name="${mode === 'create' ? 'NewRoute' : 'EditRoute'}.Segments[${index}].StartPoint" placeholder="Start Point *" required />
        <input name="${mode === 'create' ? 'NewRoute' : 'EditRoute'}.Segments[${index}].EndPoint" placeholder="End Point *" required />
        <input name="${mode === 'create' ? 'NewRoute' : 'EditRoute'}.Segments[${index}].SegmentDistanceKm" placeholder="Distance (km)" type="number" step="0.1" min="0" />
        <input name="${mode === 'create' ? 'NewRoute' : 'EditRoute'}.Segments[${index}].SegmentEstimatedMinutes" placeholder="Est. Minutes" type="number" min="0" />
        <input name="${mode === 'create' ? 'NewRoute' : 'EditRoute'}.Segments[${index}].SegmentOrder" type="hidden" value="${index + 1}" />
        <button type="button" onclick="removeSegment(this)" class="btn btn-delete">✕</button>
    `;
    container.appendChild(div);
}

function removeSegment(btn) {
    const row = btn.closest(".segment-row");
    if (row) row.remove();
}

function editRoute(routeId) {
    window.location.href = `/Admin/Admin2?handler=EditRoute&routeId=${routeId}`;
}

function cancelEdit() {
    const form = document.getElementById('edit-route-form');
    if (form) form.style.display = 'none';
}

function confirmDelete(routeId, routeName) {
    if (!confirm(`Are you sure you want to delete route "${routeName}"? This will remove all segments and bookings.`)) return;

    fetch("/Admin/Admin2?handler=DeleteRoute", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": document.getElementById("antiforgery-token").value
        },
        body: JSON.stringify(routeId)
    })
        .then(r => r.json())
        .then(res => {
            alert(res.message);
            if (res.success) location.reload();
        })
        .catch(err => alert("Error: " + err.message));
}

// ==========================
// TAB SWITCH
// ==========================
function showTab(tab) {
    document.getElementById('drivers-tab').style.display = tab === 'drivers' ? 'block' : 'none';
    document.getElementById('routes-tab').style.display = tab === 'routes' ? 'block' : 'none';
}

// ==========================
// PAGINATED ROUTES
// ==========================
async function loadRoutes(page = 1) {
    const size = 5;
    try {
        const token = localStorage.getItem("jwt");
        const res = await fetch(`/api/admin/routes?pageNumber=${page}&pageSize=${size}`, {
            headers: { "Authorization": "Bearer " + token }
        });
        if (!res.ok) throw new Error("Failed to fetch routes");

        const data = await res.json();
        const container = document.getElementById("routesContainer");
        container.innerHTML = "";

        if (!data.items.length) {
            container.innerHTML = `<div style="text-align:center; padding:40px; color:var(--gray-600);">No routes found</div>`;
        } else {
            data.items.forEach(route => {
                const div = document.createElement("div");
                div.className = "route-card";
                div.innerHTML = `
                    <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:12px;">
                        <div style="flex:1;">
                            <div class="route-summary">${route.routeName} <span style="font-size:13px; opacity:0.8;">(${route.isCircular ? "🔄 Circular" : "➡️ Linear"})</span></div>
                            <div style="font-size:13px; color:var(--gray-600);">${route.startLocation} → ${route.endLocation}</div>
                            <div style="font-size:13px; color:var(--gray-600);">📏 ${(route.totalDistanceKm ?? 0).toFixed(1)} km | ⏱️ ${route.estimatedTimeMinutes ?? 0} min</div>
                            <div style="font-size:13px; color:var(--gray-600);">Type: ${route.routeType ?? "Local"}</div>
                        </div>
                        <div>
                            <button class="btn btn-edit" onclick="editRoute(${route.routeId})">Edit</button>
                            <button class="btn btn-delete" onclick="confirmDelete(${route.routeId}, '${route.routeName}')">Delete</button>
                        </div>
                    </div>
                `;
                container.appendChild(div);
            });
        }

        currentPage = data.pageNumber;
        totalPages = Math.ceil(data.totalCount / size);
        document.getElementById("pageInfo").innerText = `Page ${currentPage} of ${totalPages}`;
        document.getElementById("prevPageBtn").disabled = currentPage <= 1;
        document.getElementById("nextPageBtn").disabled = currentPage >= totalPages;

    } catch (err) {
        console.error(err);
    }
}
