// Admin Dashboard - API Integration
// This file handles all API calls and dynamic data loading

class AdminDashboard {
    constructor() {
        this.baseUrl = '/api/Admin';
        this.drivers = [];
        this.vehicles = [];
        this.init();
    }

    async init() {
        await this.loadDashboardData();
        this.attachEventListeners();
    }

    // ========================================
    // API CALLS
    // ========================================

    async fetchDrivers(onlyVerified = null) {
        try {
            const url = onlyVerified !== null 
                ? `${this.baseUrl}/drivers?onlyVerified=${onlyVerified}`
                : `${this.baseUrl}/drivers`;
            
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) throw new Error('Failed to fetch drivers');
            
            const data = await response.json();
            this.drivers = data;
            return data;
        } catch (error) {
            console.error('Error fetching drivers:', error);
            this.showError('Failed to load drivers');
            return [];
        }
    }

    async fetchVehicles(onlyVerified = null) {
        try {
            const url = onlyVerified !== null 
                ? `${this.baseUrl}/vehicles?onlyVerified=${onlyVerified}`
                : `${this.baseUrl}/vehicles`;
            
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) throw new Error('Failed to fetch vehicles');
            
            const data = await response.json();
            this.vehicles = data;
            return data;
        } catch (error) {
            console.error('Error fetching vehicles:', error);
            this.showError('Failed to load vehicles');
            return [];
        }
    }

    async fetchVehiclesByDriver(driverId) {
        try {
            const response = await fetch(`${this.baseUrl}/vehicles/by-driver/${driverId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    return [];
                }
                throw new Error('Failed to fetch driver vehicles');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error fetching driver vehicles:', error);
            return [];
        }
    }

    async verifyDriver(driverId, isVerified) {
        try {
            const response = await fetch(`${this.baseUrl}/driver/verify`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    driverId: driverId,
                    isDriverVerified: isVerified
                })
            });

            if (!response.ok) throw new Error('Failed to verify driver');
            
            this.showSuccess(`Driver ${isVerified ? 'verified' : 'unverified'} successfully`);
            await this.loadDashboardData(); // Refresh data
            return true;
        } catch (error) {
            console.error('Error verifying driver:', error);
            this.showError('Failed to verify driver');
            return false;
        }
    }

    async verifyVehicle(driverId, isVerified) {
        try {
            const response = await fetch(`${this.baseUrl}/vehicle/verify`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    driverId: driverId,
                    isVerified: isVerified
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to verify vehicle');
            }
            
            this.showSuccess(`Vehicle ${isVerified ? 'verified' : 'unverified'} successfully`);
            await this.loadDashboardData(); // Refresh data
            return true;
        } catch (error) {
            console.error('Error verifying vehicle:', error);
            this.showError(error.message || 'Failed to verify vehicle');
            return false;
        }
    }

    // ========================================
    // DATA LOADING & RENDERING
    // ========================================

    async loadDashboardData() {
        this.showLoader();
        
        try {
            // Fetch all data in parallel
            const [drivers, vehicles] = await Promise.all([
                this.fetchDrivers(),
                this.fetchVehicles()
            ]);

            // Update statistics
            this.updateStatistics(drivers, vehicles);
            
            // Render tables
            this.renderDriversTable(drivers);
            this.renderVehiclesTable(vehicles);
            
            this.hideLoader();
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            this.showError('Failed to load dashboard data');
            this.hideLoader();
        }
    }

    updateStatistics(drivers, vehicles) {
        // Calculate statistics
        const totalUsers = drivers.length;
        const activeDrivers = drivers.filter(d => d.isDriverVerified).length;
        const totalVehicles = vehicles.length;
        
        // Calculate users added this month
        const now = new Date();
        const firstDayOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
        const newUsersThisMonth = drivers.filter(d => {
            const createdDate = new Date(d.createdAt);
            return createdDate >= firstDayOfMonth;
        }).length;

        // Update stat boxes
        this.updateStatBox('.stat-box.users .stat-value', totalUsers);
        this.updateStatBox('.stat-box.users .stat-change', `↑ ${newUsersThisMonth} this month`);
        
        this.updateStatBox('.stat-box.routes .stat-value', totalVehicles);
        this.updateStatBox('.stat-box.routes .stat-label', 'Total Vehicles');
        this.updateStatBox('.stat-box.routes .stat-change', `${vehicles.filter(v => v.isVerified).length} verified`);
        
        // Update reports section
        const reportsActiveDrivers = document.querySelector('#reports-tab .stat-box:nth-child(4) .stat-value');
        if (reportsActiveDrivers) {
            reportsActiveDrivers.textContent = activeDrivers;
        }
    }

    updateStatBox(selector, value) {
        const element = document.querySelector(selector);
        if (element) {
            element.textContent = value;
        }
    }

    renderDriversTable(drivers) {
        const tbody = document.querySelector('#users-tab tbody');
        if (!tbody) return;

        if (drivers.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" style="text-align: center; padding: 40px; color: var(--gray-500);">
                        No drivers found. Check your database connection.
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = drivers.map((driver, index) => `
            <tr>
                <td>${index + 1}</td>
                <td>${this.escapeHtml(driver.fullName || 'N/A')}</td>
                <td>${this.escapeHtml(driver.phone || 'N/A')}</td>
                <td>
                    <span class="role-badge">Driver</span>
                </td>
                <td>
                    <span class="status-badge status-${driver.isDriverVerified ? 'verified' : 'pending'}">
                        ${driver.isDriverVerified ? 'Verified' : 'Pending'}
                    </span>
                </td>
                <td>${this.formatDate(driver.createdAt)}</td>
                <td>
                    <div class="driver-rating">
                        ${driver.driverRating ? `⭐ ${driver.driverRating.toFixed(1)}` : 'N/A'}
                    </div>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-icon btn-view" onclick="adminDashboard.viewDriverDetails(${driver.userId})">
                            View
                        </button>
                        <button class="btn-icon ${driver.isDriverVerified ? 'btn-delete' : 'btn-approve'}" 
                                onclick="adminDashboard.toggleDriverVerification(${driver.userId}, ${!driver.isDriverVerified})">
                            ${driver.isDriverVerified ? 'Unverify' : 'Verify'}
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    renderVehiclesTable(vehicles) {
        const tbody = document.querySelector('#routes-tab tbody');
        if (!tbody) return;

        if (vehicles.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" style="text-align: center; padding: 40px; color: var(--gray-500);">
                        No vehicles found. Drivers need to add their vehicles.
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = vehicles.map(vehicle => `
            <tr>
                <td>${vehicle.vehicleId}</td>
                <td>${this.escapeHtml(vehicle.vehicleMake || 'N/A')}</td>
                <td>${this.escapeHtml(vehicle.vehicleModel || 'N/A')}</td>
                <td>${vehicle.vehicleYear || 'N/A'}</td>
                <td>${this.escapeHtml(vehicle.plateNumber || 'N/A')}</td>
                <td>${this.escapeHtml(vehicle.color || 'N/A')}</td>
                <td>
                    <span class="status-badge status-${vehicle.isVerified ? 'verified' : 'pending'}">
                        ${vehicle.isVerified ? 'Verified' : 'Pending'}
                    </span>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="btn-icon btn-view" onclick="adminDashboard.viewVehicleDetails(${vehicle.vehicleId})">
                            View
                        </button>
                        <button class="btn-icon ${vehicle.isVerified ? 'btn-delete' : 'btn-approve'}" 
                                onclick="adminDashboard.toggleVehicleVerification(${vehicle.driverId}, ${!vehicle.isVerified})">
                            ${vehicle.isVerified ? 'Unverify' : 'Verify'}
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }

    // ========================================
    // USER INTERACTIONS
    // ========================================

    async toggleDriverVerification(driverId, shouldVerify) {
        if (confirm(`Are you sure you want to ${shouldVerify ? 'verify' : 'unverify'} this driver?`)) {
            await this.verifyDriver(driverId, shouldVerify);
        }
    }

    async toggleVehicleVerification(driverId, shouldVerify) {
        if (confirm(`Are you sure you want to ${shouldVerify ? 'verify' : 'unverify'} this vehicle?`)) {
            await this.verifyVehicle(driverId, shouldVerify);
        }
    }

    async viewDriverDetails(driverId) {
        const driver = this.drivers.find(d => d.userId === driverId);
        if (!driver) return;

        // Fetch driver's vehicles
        const vehicles = await this.fetchVehiclesByDriver(driverId);

        const vehiclesList = vehicles.length > 0 
            ? vehicles.map(v => `
                <div style="padding: 12px; background: var(--gray-50); border-radius: 8px; margin-bottom: 8px;">
                    <strong>${v.vehicleMake} ${v.vehicleModel}</strong> (${v.vehicleYear})<br>
                    Plate: ${v.plateNumber} | Color: ${v.color} | Seats: ${v.seatsCount}<br>
                    Status: <span class="status-badge status-${v.isVerified ? 'verified' : 'pending'}">
                        ${v.isVerified ? 'Verified' : 'Pending'}
                    </span>
                </div>
            `).join('')
            : '<p style="color: var(--gray-500);">No vehicles registered</p>';

        this.showModal(`
            <h2>${driver.fullName}</h2>
            <div style="margin-top: 20px; line-height: 1.8;">
                <p><strong>Phone:</strong> ${driver.phone || 'N/A'}</p>
                <p><strong>Address:</strong> ${driver.address || 'N/A'}, ${driver.city || ''}, ${driver.country || ''}</p>
                <p><strong>License Number:</strong> ${driver.driverLicenseNumber || 'N/A'}</p>
                <p><strong>License Expiry:</strong> ${this.formatDate(driver.driverLicenseExpiry)}</p>
                <p><strong>Rating:</strong> ${driver.driverRating ? `⭐ ${driver.driverRating.toFixed(1)}` : 'N/A'}</p>
                <p><strong>Status:</strong> 
                    <span class="status-badge status-${driver.isDriverVerified ? 'verified' : 'pending'}">
                        ${driver.isDriverVerified ? 'Verified' : 'Pending Verification'}
                    </span>
                </p>
                <p><strong>Joined:</strong> ${this.formatDate(driver.createdAt)}</p>
                
                <h3 style="margin-top: 24px; margin-bottom: 12px;">Vehicles</h3>
                ${vehiclesList}
            </div>
        `);
    }

    async viewVehicleDetails(vehicleId) {
        const vehicle = this.vehicles.find(v => v.vehicleId === vehicleId);
        if (!vehicle) return;

        this.showModal(`
            <h2>${vehicle.vehicleMake} ${vehicle.vehicleModel}</h2>
            <div style="margin-top: 20px; line-height: 1.8;">
                <p><strong>Vehicle ID:</strong> ${vehicle.vehicleId}</p>
                <p><strong>Year:</strong> ${vehicle.vehicleYear}</p>
                <p><strong>Plate Number:</strong> ${vehicle.plateNumber}</p>
                <p><strong>Color:</strong> ${vehicle.color}</p>
                <p><strong>Seats:</strong> ${vehicle.seatsCount}</p>
                <p><strong>License Number:</strong> ${vehicle.vehicleLicenseNumber || 'N/A'}</p>
                <p><strong>License Expiry:</strong> ${this.formatDate(vehicle.vehicleLicenseExpiry)}</p>
                <p><strong>Status:</strong> 
                    <span class="status-badge status-${vehicle.isVerified ? 'verified' : 'pending'}">
                        ${vehicle.isVerified ? 'Verified' : 'Pending Verification'}
                    </span>
                </p>
                <p><strong>Driver ID:</strong> ${vehicle.driverId}</p>
            </div>
        `);
    }

    attachEventListeners() {
        // Search functionality
        const searchBoxes = document.querySelectorAll('.search-box');
        searchBoxes.forEach(searchBox => {
            searchBox.addEventListener('input', (e) => {
                const searchTerm = e.target.value.toLowerCase();
                const table = e.target.closest('.tab-content').querySelector('tbody');
                const rows = table.querySelectorAll('tr');
                
                rows.forEach(row => {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            });
        });

        // Refresh button (if exists)
        const refreshBtn = document.querySelector('.refresh-btn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.loadDashboardData());
        }
    }

    // ========================================
    // UTILITY FUNCTIONS
    // ========================================

    formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { 
            year: 'numeric', 
            month: 'short', 
            day: 'numeric' 
        });
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showLoader() {
        const loader = document.getElementById('dashboard-loader');
        if (loader) loader.style.display = 'flex';
    }

    hideLoader() {
        const loader = document.getElementById('dashboard-loader');
        if (loader) loader.style.display = 'none';
    }

    showSuccess(message) {
        this.showNotification(message, 'success');
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 16px 24px;
            background: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6'};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    showModal(content) {
        // Create modal
        const modal = document.createElement('div');
        modal.className = 'custom-modal';
        modal.innerHTML = `
            <div class="modal-backdrop" onclick="this.parentElement.remove()"></div>
            <div class="modal-content">
                <button class="modal-close" onclick="this.closest('.custom-modal').remove()">×</button>
                ${content}
            </div>
        `;

        // Add styles if not already added
        if (!document.getElementById('modal-styles')) {
            const style = document.createElement('style');
            style.id = 'modal-styles';
            style.textContent = `
                .custom-modal {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    z-index: 9999;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .modal-backdrop {
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0, 0, 0, 0.5);
                }
                .modal-content {
                    position: relative;
                    background: white;
                    border-radius: 12px;
                    padding: 32px;
                    max-width: 600px;
                    max-height: 80vh;
                    overflow-y: auto;
                    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                }
                .modal-close {
                    position: absolute;
                    top: 16px;
                    right: 16px;
                    background: none;
                    border: none;
                    font-size: 32px;
                    cursor: pointer;
                    color: var(--gray-500);
                    line-height: 1;
                    padding: 0;
                    width: 32px;
                    height: 32px;
                }
                .modal-close:hover {
                    color: var(--gray-700);
                }
                @keyframes slideIn {
                    from { transform: translateX(100%); opacity: 0; }
                    to { transform: translateX(0); opacity: 1; }
                }
                @keyframes slideOut {
                    from { transform: translateX(0); opacity: 1; }
                    to { transform: translateX(100%); opacity: 0; }
                }
                .role-badge {
                    display: inline-block;
                    padding: 4px 12px;
                    background: var(--primary-100);
                    color: var(--primary-700);
                    border-radius: 12px;
                    font-size: 13px;
                    font-weight: 500;
                }
                .status-badge.status-verified {
                    background: #d1fae5;
                    color: #065f46;
                }
                .status-badge.status-pending {
                    background: #fef3c7;
                    color: #92400e;
                }
                .btn-approve {
                    background: #10b981 !important;
                    color: white !important;
                }
                .btn-approve:hover {
                    background: #059669 !important;
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(modal);
    }
}

// Initialize dashboard when DOM is ready
let adminDashboard;
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        adminDashboard = new AdminDashboard();
    });
} else {
    adminDashboard = new AdminDashboard();
}
