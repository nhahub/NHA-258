document.addEventListener("DOMContentLoaded", () => {
    loadDrivers();
});

async function loadDrivers() {
    const token = localStorage.getItem("jwt");

    const res = await fetch("/api/admin/drivers", {
        headers: { "Authorization": "Bearer " + token }
    });

    const drivers = await res.json();
    const tbody = document.querySelector("#driversTable tbody");
    tbody.innerHTML = "";

    drivers.forEach(d => {
        tbody.innerHTML += `
            <tr>
                <td>${d.fullName}</td>
                <td>${d.phone}</td>
                <td>${d.city}</td>
                <td>${d.driverRating ?? "N/A"}</td>
                <td>${d.isDriverVerified ? "✔ Verified" : "❌ Not Verified"}</td>
                <td><button onclick="viewDriver(${d.driverId})" class="btn btn-primary btn-sm">View</button></td>
            </tr>
        `;
    });
}

async function viewDriver(driverId) {
    const token = localStorage.getItem("jwt");

    const res = await fetch(`/api/admin/driver/${driverId}`, {
        headers: { "Authorization": "Bearer " + token }
    });

    const full = await res.json();

    // Show section
    document.getElementById("driverDetails").style.display = "block";

    // Fill Driver profile
    const d = full.driver;
    document.getElementById("driverProfile").innerHTML = `
        <p><strong>Name:</strong> ${d.fullName}</p>
        <p><strong>Phone:</strong> ${d.phone}</p>
        <p><strong>City:</strong> ${d.city}</p>
        <p><strong>License:</strong> ${d.driverLicenseNumber}</p>
        <p><strong>Rating:</strong> ${d.driverRating ?? "N/A"}</p>
        <p><strong>Verified:</strong> ${d.isDriverVerified}</p>
    `;

    // Fill Vehicle profile
    const v = full.vehicle;
    document.getElementById("vehicleProfile").innerHTML = `
        <p><strong>Make:</strong> ${v.vehicleMake}</p>
        <p><strong>Model:</strong> ${v.vehicleModel}</p>
        <p><strong>Year:</strong> ${v.vehicleYear}</p>
        <p><strong>Plate:</strong> ${v.plateNumber}</p>
        <p><strong>Seats:</strong> ${v.seatsCount}</p>
        <p><strong>Verified:</strong> ${v.isVerified}</p>
    `;

    // Store ID for verify actions
    window.currentDriverId = driverId;
}

document.getElementById("verifyDriverBtn").addEventListener("click", async () => {
    await verifyDriver(window.currentDriverId);
});

document.getElementById("verifyVehicleBtn").addEventListener("click", async () => {
    await verifyVehicle(window.currentDriverId);
});

// ==========================
// VERIFY DRIVER
// ==========================
async function verifyDriver(driverId) {
    await fetch("/api/admin/driver/verify", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + localStorage.getItem("jwt")
        },
        body: JSON.stringify({
            driverId: driverId,
            isDriverVerified: true
        })
    });

    alert("Driver Verified ✔");
    loadDrivers();
}

// ==========================
// VERIFY VEHICLE
// ==========================
async function verifyVehicle(driverId) {
    await fetch("/api/admin/vehicle/verify", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + localStorage.getItem("jwt")
        },
        body: JSON.stringify({
            driverId: driverId,
            isVerified: true
        })
    });

    alert("Vehicle Verified ✔");
    loadDrivers();
}
