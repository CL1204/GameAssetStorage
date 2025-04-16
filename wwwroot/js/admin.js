const API_BASE_URL = window.location.hostname.includes("localhost")
    ? "http://localhost:7044"
    : "https://gameasset-backend-aj1g.onrender.com";

document.addEventListener("DOMContentLoaded", async () => {
    try {
        const res = await fetch(`${API_BASE_URL}/api/auth/check-auth`, {
            credentials: "include",
        });
        const auth = await res.json();

        if (!res.ok || !auth.isAdmin) {
            window.location.href = "/dashboard";
            return;
        }

        document.getElementById("userLabel").textContent = auth.username;
        loadPendingAssets();
    } catch (err) {
        console.error("Auth error:", err);
        window.location.href = "/login";
    }
});

function logout() {
    fetch(`${API_BASE_URL}/auth/logout`, {
        method: "POST",
        credentials: "include"
    }).then(() => {
        window.location.href = "/login";
    });
}

async function loadPendingAssets() {
    try {
        const res = await fetch(`${API_BASE_URL}/api/assets/pending-assets`, {
            credentials: "include"
        });

        const pendingAssets = await res.json();
        console.log("Pending Assets:", pendingAssets);

        renderPendingAssets(pendingAssets);
    } catch (err) {
        console.error("Failed to load pending assets:", err);
    }
}

function renderPendingAssets(assets) {
    const tbody = document.getElementById("pendingAssets");
    tbody.innerHTML = "";

    if (!assets.length) {
        tbody.innerHTML = `<tr><td colspan="5">No pending assets</td></tr>`;
        return;
    }

    assets.forEach(asset => {
        const tr = document.createElement("tr");

        const isAudio = asset.category === "soundtracks";
        const assetPreview = isAudio
            ? `<audio controls style="max-width: 160px;">
                   <source src="${asset.fileUrl}" type="audio/mpeg">
                   Your browser does not support the audio element.
               </audio>`
            : `<img src="${asset.imageUrl}" alt="${asset.title}" 
                     style="height: 60px; border-radius: 8px; cursor: pointer;" 
                     onclick="showAssetModal('${asset.imageUrl}', '${escapeHtml(asset.title)}', \`${escapeHtml(asset.description || "No description")}\`)">`;

        tr.innerHTML = `
            <td>${assetPreview}</td>
            <td>${escapeHtml(asset.category)}</td>
            <td>${escapeHtml(asset.username || "Unknown")}</td>
            <td>${new Date(asset.createdAt).toLocaleDateString()}</td>
            <td>
                <button class="action-btn approve-btn" onclick="approveAsset(${asset.id})">Approve</button>
                <button class="action-btn reject-btn" onclick="rejectAsset(${asset.id})">Reject</button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

function escapeHtml(text) {
    const map = {
        '&': "&amp;",
        '<': "&lt;",
        '>': "&gt;",
        '"': "&quot;",
        "'": "&#039;"
    };
    return text?.replace(/[&<>"']/g, m => map[m]) || "";
}

function showAssetModal(imageUrl, title, description) {
    document.getElementById("modalAssetTitle").textContent = title;
    document.getElementById("modalAssetImage").src = imageUrl;
    document.getElementById("modalAssetDescription").textContent = description;
    document.getElementById("assetModal").style.display = "block";
}

function closeAssetModal() {
    document.getElementById("assetModal").style.display = "none";
}

async function approveAsset(id) {
    showConfirm("Approve this asset?", async () => {
        const res = await fetch(`${API_BASE_URL}/api/assets/${id}/approve`, {
            method: "POST",
            credentials: "include"
        });

        if (res.ok) {
            showToast("✅ Asset approved.");
            loadPendingAssets();
        } else {
            showToast("❌ Failed to approve asset.");
        }
    });
}

async function rejectAsset(id) {
    showConfirm("Reject this asset? This will permanently delete the file from storage.", async () => {
        const res = await fetch(`${API_BASE_URL}/api/assets/${id}/reject`, {
            method: "DELETE",
            credentials: "include"
        });

        if (res.ok) {
            showToast("❌ Asset rejected and removed.");
            loadPendingAssets();
        } else {
            const error = await res.text();
            showToast("Failed to reject: " + error);
        }
    });
}

