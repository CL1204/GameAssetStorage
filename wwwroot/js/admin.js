const API_BASE_URL = window.location.hostname.includes("localhost")
    ? "http://localhost:7044/api"
    : "https://gameasset-backend-aj1g.onrender.com/api";

// Auth check
(async function init() {
    try {
        const res = await fetch(`${API_BASE_URL}/auth/check-auth`, {
            credentials: "include",
        });
        const auth = await res.json();

        if (!res.ok || !auth.isAdmin) {
            window.location.href = "dashboard.html";
            return;
        }

        document.getElementById("userLabel").textContent = auth.username;
        loadPendingAssets();
    } catch (err) {
        console.error("Auth error:", err);
        window.location.href = "login.html";
    }
})();

// Logout
function logout() {
    fetch(`${API_BASE_URL}/auth/logout`, {
        method: "POST",
        credentials: "include"
    }).then(() => {
        localStorage.clear();
        window.location.href = "login.html";
    });
}

// ✅ Load pending assets from server
async function loadPendingAssets() {
    try {
        const res = await fetch(`${API_BASE_URL}/assets/pending-assets`, {
            credentials: "include"
        });

        const pendingAssets = await res.json();
        renderPendingAssets(pendingAssets);
    } catch (err) {
        console.error("Failed to load pending assets:", err);
    }
}

// ✅ Render assets with image preview
function renderPendingAssets(assets) {
    const tbody = document.getElementById("pendingAssets");
    tbody.innerHTML = "";

    if (!assets.length) {
        tbody.innerHTML = `<tr><td colspan="5">No pending assets</td></tr>`;
        return;
    }

    assets.forEach(asset => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>
                <img src="${asset.imageUrl}" alt="${asset.title}" style="height: 60px; border-radius: 8px; cursor: pointer;" onclick="showAssetModal('${asset.imageUrl}', '${asset.title}', \`${asset.description}\`)">
            </td>
            <td>${asset.category}</td>
            <td>${asset.username || "Unknown"}</td>
            <td>${new Date(asset.createdAt).toLocaleDateString()}</td>
            <td>
                <button class="action-btn approve-btn" onclick="approveAsset(${asset.id})">Approve</button>
                <button class="action-btn reject-btn" onclick="rejectAsset(${asset.id})">Reject</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}


// Modal logic
function showAssetModal(imageUrl, title, description) {
    document.getElementById("modalAssetTitle").textContent = title;
    document.getElementById("modalAssetImage").src = imageUrl;
    document.getElementById("modalAssetDescription").textContent = description;
    document.getElementById("assetModal").style.display = "block";
}

function closeAssetModal() {
    document.getElementById("assetModal").style.display = "none";
}

// Approve asset
async function approveAsset(id) {
    if (!confirm("Approve this asset?")) return;

    const res = await fetch(`${API_BASE_URL}/assets/${id}/approve`, {
        method: "POST",
        credentials: "include"
    });

    if (res.ok) {
        alert("Asset approved!");
        loadPendingAssets();
    } else {
        alert("Failed to approve asset.");
    }
}

// Reject asset
async function rejectAsset(id) {
    if (!confirm("Reject this asset? This cannot be undone.")) return;

    const res = await fetch(`${API_BASE_URL}/assets/${id}/reject`, {
        method: "DELETE",
        credentials: "include"
    });

    if (res.ok) {
        alert("Asset rejected.");
        loadPendingAssets();
    } else {
        alert("Failed to reject asset.");
    }
}

function showAssetModal(imageUrl, title, description) {
    document.getElementById("modalAssetImage").src = imageUrl;
    document.getElementById("modalAssetTitle").textContent = title;
    document.getElementById("modalAssetDescription").textContent = description;
    document.getElementById("assetModal").style.display = "block";
}

function closeAssetModal() {
    document.getElementById("assetModal").style.display = "none";
}
