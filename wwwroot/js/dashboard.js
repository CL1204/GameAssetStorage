// FULL FIXED dashboard.js

if (!window.BASE_URL) {
    window.BASE_URL = window.location.hostname.includes("localhost")
        ? "http://localhost:7044"
        : "https://gameasset-backend-aj1g.onrender.com";
}
if (!window.currentUser) window.currentUser = null;

let allAssets = [];
let allUsernames = [];
let currentAssetId = null;

document.addEventListener("DOMContentLoaded", async () => {
    try {
        const auth = await fetch(`${BASE_URL}/api/auth/check-auth`, { credentials: "include" });
        if (auth.ok) {
            const data = await auth.json();
            window.currentUser = data;
            document.getElementById("userLabel").textContent = data.username;
            if (data.isAdmin) document.getElementById("adminBadge").style.display = "inline";
        } else {
            window.currentUser = null;
        }
    } catch (err) {
        console.warn("User not logged in");
        window.currentUser = null;
    }

    await loadAssets();
    await fetchAllUsernames();
});

async function loadAssets() {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/approved`);
        const assets = await res.json();
        allAssets = assets;
        displayTopRated(assets);
        displayDiscover(assets);
    } catch (err) {
        console.error("Error loading assets:", err);
        showToast("Failed to load assets. Please try again later.");
    }
}

function displayTopRated(assets) {
    const top3 = [...assets].sort((a, b) => b.likes - a.likes).slice(0, 3);
    const section = document.getElementById("topRatedSection");
    section.innerHTML = "";
    top3.forEach(asset => section.appendChild(createAssetCard(asset)));
}

function displayDiscover(assets) {
    const shuffled = [...assets].sort(() => 0.5 - Math.random()).slice(0, 10);
    const loopAssets = [...shuffled, ...shuffled];
    const container = document.getElementById("discoverSection");
    container.innerHTML = "";
    loopAssets.forEach(asset => container.appendChild(createAssetCard(asset)));
}

function createAssetCard(asset) {
    const card = document.createElement("div");
    card.className = "asset-card";

    const img = document.createElement("img");
    img.src = asset.imageUrl;
    img.alt = asset.title;
    img.onerror = function () {
        this.onerror = null;
        this.src = "/assets/placeholder.jpg";
    };
    card.appendChild(img);

    const likeBtn = document.createElement("button");
    likeBtn.className = "favorite-btn";
    likeBtn.innerHTML = `♥ <span class="favorite-count">${asset.likes}</span>`;
    likeBtn.onclick = () => {
        if (!window.currentUser) {
            showToast("Please login to like assets.");
        } else {
            likeAsset(asset.id, likeBtn);
        }
    };
    card.appendChild(likeBtn);

    const title = document.createElement("h4");
    title.textContent = asset.title;
    card.appendChild(title);

    const desc = document.createElement("p");
    desc.textContent = asset.description;
    card.appendChild(desc);

    const tags = document.createElement("small");
    tags.textContent = "Tags: " + (asset.tags?.join(", ") || "None");
    card.appendChild(tags);

    if (window.currentUser?.isAdmin || window.currentUser?.userId == asset.userId) {
        const del = document.createElement("button");
        del.textContent = "−";
        del.className = "delete-btn";
        del.onclick = () => {
            showConfirm("Delete this asset?", async () => {
                const res = await fetch(`${BASE_URL}/api/assets/${asset.id}`, {
                    method: "DELETE",
                    credentials: "include"
                });
                if (res.ok) {
                    showToast("Deleted");
                    await loadAssets();
                } else {
                    showToast("Failed to delete.");
                }
            });
        };
        card.appendChild(del);
    }

    const commentBtn = document.createElement("button");
    commentBtn.textContent = "💬";
    commentBtn.className = "action-btn";
    commentBtn.onclick = () => openDiscussion(asset.id);
    card.appendChild(commentBtn);

    return card;
}

async function likeAsset(id, btn) {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/${id}/like`, {
            method: "POST",
            credentials: "include"
        });

        if (res.ok) {
            const result = await res.json();
            btn.querySelector(".favorite-count").textContent = result.likes;
            btn.classList.add("liked");
        } else if (res.status === 400) {
            const result = await res.json();
            showToast(result.message || "Already liked.");
        } else {
            showToast("Failed to like asset.");
        }
    } catch (err) {
        console.error("❌ Like request failed:", err);
    }
}

async function fetchAllUsernames() {
    try {
        const res = await fetch(`${BASE_URL}/admin/users`, { credentials: "include" });
        const users = await res.json();
        allUsernames = users.map(u => u.username.toLowerCase());
    } catch (err) {
        console.error("Fetch users failed:", err);
    }
}

document.getElementById("uploadForm")?.addEventListener("submit", async e => {
    e.preventDefault();
    if (!window.currentUser) {
        return showToast("Please login to upload assets.");
    }

    const file = document.getElementById("assetFile").files[0];
    const title = document.getElementById("assetName").value;
    const category = document.getElementById("assetCategory").value;
    const description = document.getElementById("assetDescription").value;
    const tags = document.getElementById("tagInput").value.split(",").map(t => t.trim()).filter(Boolean);

    if (!file || !title || !category) {
        showToast("Please complete the form.");
        return;
    }

    const formData = new FormData();
    formData.append("file", file);
    formData.append("title", title);
    formData.append("description", description);
    formData.append("category", category);
    tags.forEach(tag => formData.append("tags", tag));

    try {
        const res = await fetch(`${BASE_URL}/api/assets/upload`, {
            method: "POST",
            body: formData,
            credentials: "include"
        });

        if (res.ok) {
            showToast("✅ Upload successful! Awaiting admin approval.");
            e.target.reset();
            toggleUploadPanel();
            await loadAssets();
        }

    } catch (err) {
        showToast("Upload error: " + err.message);
    }
});

function toggleUploadPanel() {
    if (!window.currentUser) {
        showToast("Please login to upload assets.");
        return;
    }
    const panel = document.getElementById("uploadPanel");
    panel.style.display = panel.style.display === "none" ? "block" : "none";
}

window.likeAsset = likeAsset;

function openDiscussion(assetId) {
    currentAssetId = assetId;
    document.getElementById("commentOverlay").style.display = "block";
    loadComments(assetId);

    const asset = allAssets.find(a => a.id === assetId);
    const isAudio = asset.category === "soundtracks";

    const previewContainer = document.getElementById("modalPreview");
    previewContainer.innerHTML = isAudio
        ? `<audio controls src="${asset.fileUrl}" style="width: 100%; max-height: 250px;"></audio>`
        : `<img src="${asset.imageUrl}" alt="${asset.title}" style="max-width: 100%; max-height: 250px;" />`;
}

function closeDiscussion() {
    document.getElementById("commentOverlay").style.display = "none";
    document.getElementById("discussionComments").innerHTML = "";
    document.getElementById("newComment").value = "";
}

async function loadComments(assetId) {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/${assetId}/comments`);
        const comments = await res.json();
        const container = document.getElementById("discussionComments");
        container.innerHTML = "";

        if (comments.length === 0) {
            container.innerHTML = "<p style='color:#ccc;'>No comments yet.</p>";
        } else {
            comments.forEach(c => {
                const isOwner = window.currentUser?.username === c.username;
                const isAdmin = window.currentUser?.isAdmin;
                const div = document.createElement("div");
                div.className = "comment-item";
                div.innerHTML = `
                    <strong>${c.username}</strong>
                    <small>${new Date(c.createdAt).toLocaleString()}</small>
                    ${(isOwner || isAdmin) ? `<button class="delete-comment-btn" onclick="deleteComment(${c.id})">🗑</button>` : ""}
                    <br>${c.content}<hr>`;
                container.appendChild(div);
            });
        }
    } catch (err) {
        console.error("Failed to load comments:", err);
    }
}

async function submitComment() {
    const textarea = document.getElementById("newComment");
    const content = textarea.value.trim();
    if (!content) return showToast("Comment cannot be empty");

    try {
        const res = await fetch(`${BASE_URL}/api/assets/${currentAssetId}/comments`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(content)
        });
        if (res.ok) {
            textarea.value = "";
            await loadComments(currentAssetId);
        } else {
            showToast("Failed to post comment.");
        }
    } catch (err) {
        console.error("Post comment failed:", err);
    }
}

async function deleteComment(commentId) {
    showConfirm("Delete this comment?", async () => {
        try {
            const res = await fetch(`${BASE_URL}/api/assets/comments/${commentId}`, {
                method: "DELETE",
                credentials: "include"
            });
            if (res.ok) {
                showToast("Comment deleted");
                await loadComments(currentAssetId);
            } else {
                showToast("Failed to delete comment.");
            }
        } catch (err) {
            console.error("Delete comment failed:", err);
        }
    });
}

window.deleteComment = deleteComment;