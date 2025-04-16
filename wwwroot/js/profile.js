if (!window.BASE_URL) {
    window.BASE_URL = window.location.hostname.includes("localhost")
        ? "http://localhost:7044"
        : "https://gameasset-backend-aj1g.onrender.com";
}

window.addEventListener("DOMContentLoaded", async () => {
    const usernameDisplay = document.getElementById("usernameDisplay");

    try {
        const res = await fetch(`${BASE_URL}/api/auth/check-auth`, {
            credentials: "include"
        });

        if (res.ok) {
            const user = await res.json();
            usernameDisplay.textContent = `Welcome, ${user.username}!`;
            loadLikedAssets();
            loadUploadedAssets();
        } else {
            window.location.href = "/login";
        }
    } catch (err) {
        console.error("Auth check failed:", err);
        window.location.href = "/login";
    }

    document.getElementById("editForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const newUsername = document.getElementById("newUsername").value.trim();
        const newPassword = document.getElementById("newPassword").value.trim();
        const message = document.getElementById("editMessage");

        try {
            const res = await fetch(`${BASE_URL}/api/auth/edit-profile`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify({ username: newUsername, password: newPassword })
            });

            const data = await res.json();
            if (res.ok) {
                message.classList.add("success");
                message.textContent = "Profile updated!";
                document.getElementById("usernameDisplay").textContent = `Welcome, ${newUsername}!`;
            } else {
                throw new Error(data.message || "Failed to update.");
            }
        } catch (err) {
            message.classList.add("error");
            message.textContent = err.message;
        }
    });
});

function toggleEditProfile() {
    const form = document.getElementById("editProfileForm");
    form.style.display = form.style.display === "none" ? "block" : "none";
}

async function loadLikedAssets() {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/liked`, {
            credentials: "include"
        });

        const liked = await res.json();
        const container = document.getElementById("likedAssets");
        const counter = document.getElementById("likedCount");

        container.innerHTML = "";
        if (!liked || liked.length === 0) {
            document.getElementById("noLiked").style.display = "block";
            counter.textContent = "(0)";
        } else {
            document.getElementById("noLiked").style.display = "none";
            counter.textContent = `(${liked.length})`;
            liked.forEach(a => container.appendChild(createAssetCard(a)));
        }
    } catch (err) {
        console.error("Failed to load liked assets", err);
    }
}

async function loadUploadedAssets() {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/user`, {
            credentials: "include"
        });

        const assets = await res.json();
        const container = document.getElementById("uploadedAssets");
        const counter = document.getElementById("uploadCount");

        container.innerHTML = "";
        if (!assets || assets.length === 0) {
            document.getElementById("noUploads").style.display = "block";
            counter.textContent = "(0)";
        } else {
            document.getElementById("noUploads").style.display = "none";
            counter.textContent = `(${assets.length})`;
            assets.forEach(a => container.appendChild(createAssetCard(a)));
        }
    } catch (err) {
        console.error("Failed to load uploaded assets", err);
    }
}

function createAssetCard(asset) {
    const card = document.createElement("div");
    card.className = "asset-card";

    const img = document.createElement("img");
    img.src = asset.imageUrl;
    img.alt = asset.title;
    img.onerror = () => {
        img.src = "/assets/placeholder.jpg"; // fallback image
    };

    card.appendChild(img);
    card.innerHTML += `
        <h4>${asset.title}</h4>
        <p>${asset.description}</p>
        <small>Tags: ${asset.tags?.join(", ") || "None"}</small>
    `;
    return card;
}

async function logout() {
    try {
        await fetch(`${BASE_URL}/api/auth/logout`, {
            method: "POST",
            credentials: "include"
        });
        window.location.href = "/login";
    } catch (err) {
        showToast("Logout failed");
    }
}
