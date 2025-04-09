const BASE_URL = window.location.hostname.includes("localhost")
    ? "http://localhost:7044"
    : "https://gameasset-backend-aj1g.onrender.com";

// On load
window.onload = fetchAssets;

async function fetchAssets() {
    try {
        const res = await fetch(`${BASE_URL}/api/asset/approved`, { credentials: 'include' });
        const data = await res.json();

        const userRes = await fetch(`${BASE_URL}/api/auth/check-auth`, { credentials: 'include' });
        const user = await userRes.json();

        document.getElementById("usernameDisplay").innerText = `@${user.username}`;
        document.getElementById("newUsername").value = user.username;

        const liked = data.filter(asset => asset.likedBy?.includes(user.username));
        const uploaded = data.filter(asset => asset.userId == user.userId);

        const likedContainer = document.getElementById("likedAssets");
        const uploadedContainer = document.getElementById("uploadedAssets");

        if (liked.length === 0) document.getElementById("noLiked").style.display = "block";
        else liked.forEach(a => likedContainer.appendChild(renderCard(a)));

        if (uploaded.length === 0) document.getElementById("noUploads").style.display = "block";
        else uploaded.forEach(a => uploadedContainer.appendChild(renderCard(a)));

        // If admin, add Actions button
        if (user.isAdmin) {
            const btn = document.createElement("button");
            btn.innerHTML = `<span>🛠️</span><span>Actions</span>`;
            btn.onclick = () => location.href = "admin.html";
            document.getElementById("bottomNavActions").appendChild(btn);
        }
    } catch (err) {
        console.error("Failed to fetch profile data:", err);
    }
}

function renderCard(asset) {
    const div = document.createElement("div");
    div.className = "asset-card";
    div.innerHTML = `
        <img src="${asset.imageUrl}" alt="${asset.title}" />
        <h4>${asset.title}</h4>
        <p>${asset.description}</p>
        <a class="download-btn" href="${asset.imageUrl}" download>Download</a>
    `;
    return div;
}

function logout() {
    fetch(`${BASE_URL}/api/auth/logout`, {
        method: "POST",
        credentials: "include"
    }).then(() => {
        window.location.href = "login.html";
    });
}

function toggleUploadPanel() {
    const panel = document.getElementById("uploadPanel");
    if (panel) panel.style.display = panel.style.display === "none" ? "block" : "none";
}

function openEditModal() {
    document.getElementById("editModal").style.display = "block";
}

function closeEditModal() {
    document.getElementById("editModal").style.display = "none";
}

document.getElementById("editForm")?.addEventListener("submit", async (e) => {
    e.preventDefault();
    const username = document.getElementById("newUsername").value.trim();
    const password = document.getElementById("newPassword").value.trim();
    const message = document.getElementById("editMessage");

    try {
        const res = await fetch(`${BASE_URL}/api/auth/edit-profile`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify({ username, password })
        });

        const data = await res.json();
        if (res.ok) {
            message.className = "auth-message success";
            message.textContent = "Profile updated!";
            setTimeout(() => location.reload(), 1200);
        } else {
            message.className = "auth-message error";
            message.textContent = data.message || "Failed to update.";
        }
    } catch (err) {
        message.className = "auth-message error";
        message.textContent = "Something went wrong.";
    }
});
