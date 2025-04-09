const BASE_URL = window.location.hostname.includes("localhost")
    ? "http://localhost:7044"
    : "https://gameasset-backend-aj1g.onrender.com";

let allAssets = [];
let currentUser = null;

window.onload = async () => {
    try {
        const authRes = await fetch(`${BASE_URL}/api/auth/check-auth`, { credentials: "include" });
        if (!authRes.ok) throw new Error("Unauthorized");
        currentUser = await authRes.json();

        const res = await fetch(`${BASE_URL}/api/assets/approved`);
        if (!res.ok) throw new Error("Failed to load assets");
        allAssets = await res.json();

        displayAssets(allAssets);
        setupSearch(allAssets);
        setupCategoryButtons();
        showCategory("characters");
        highlightCategoryButton("characters");
    } catch (err) {
        console.error("Failed to fetch assets or auth:", err);
    }

    setupUploadHandler();
};

function setupUploadHandler() {
    const uploadForm = document.getElementById("uploadForm");
    uploadForm?.addEventListener("submit", async e => {
        e.preventDefault();
        const file = document.getElementById("assetFile").files[0];
        const title = document.getElementById("assetName").value;
        const category = document.getElementById("assetCategory").value;
        const description = document.getElementById("assetDescription").value;
        const tags = document.getElementById("tagInput").value.split(",").map(t => t.trim()).filter(Boolean);

        if (!file || !title || !category) {
            alert("Please complete the form.");
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
                alert("Upload successful! Awaiting admin approval.");
                uploadForm.reset();
                toggleUploadPanel();
            } else {
                const errorText = await res.text();
                console.error("Upload failed:", errorText);
                alert("Upload failed: " + errorText);
            }

        } catch (error) {
            alert("Upload error: " + error.message);
        }
    });
}

function setupCategoryButtons() {
    document.querySelectorAll(".category-btn").forEach(btn => {
        btn.addEventListener("click", e => {
            e.preventDefault();
            const category = btn.dataset.category;
            showCategory(category);
            highlightCategoryButton(category);
        });
    });
}

function highlightCategoryButton(category) {
    document.querySelectorAll(".category-btn").forEach(btn => {
        if (btn.dataset.category === category) {
            btn.classList.add("active", "clicked");
            setTimeout(() => btn.classList.remove("clicked"), 600);
        } else {
            btn.classList.remove("active");
        }
    });
}

function showCategory(category) {
    document.querySelectorAll(".asset-section").forEach(section => section.classList.remove("active"));
    const section = document.getElementById(`${category}-section`);
    if (section) section.classList.add("active");
    document.getElementById("searchBar").value = "";
    searchAssets(category);
}

function displayAssets(assets) {
    const categories = {
        characters: document.getElementById("characters"),
        environment: document.getElementById("environment"),
        soundtracks: document.getElementById("soundtracks")
    };
    Object.values(categories).forEach(el => el.innerHTML = "");

    assets.forEach(asset => {
        const card = document.createElement("div");
        card.className = "asset-card";
        card.innerHTML = `
            <button class="favorite-btn" onclick="likeAsset(${asset.id}, this)">
              ♥ <span class="favorite-count">${asset.likes}</span>
            </button>
            <img src="${asset.imageUrl}" alt="${asset.title}" onerror="this.src='placeholder.jpg'" />
            <h4>${asset.title}</h4>
            <p>${asset.description}</p>
            <small>Tags: ${asset.tags?.join(", ") || "None"}</small>
            <a class="download-btn" href="#" onclick="downloadAsset(${asset.id}, '${asset.fileUrl}'); return false;">Download</a>
        `;

        if (currentUser?.isAdmin || currentUser?.userId === asset.userId) {
            const deleteBtn = document.createElement("button");
            deleteBtn.textContent = "−";
            deleteBtn.className = "delete-btn";
            deleteBtn.title = "Delete Asset";
            deleteBtn.onclick = () => {
                if (confirm("Are you sure you want to delete this asset?")) {
                    deleteAsset(asset.id);
                }
            };
            card.appendChild(deleteBtn);
        }

        categories[asset.category]?.appendChild(card);
    });
}

function setupSearch(assets) {
    const input = document.getElementById("searchBar");
    input.addEventListener("input", () => {
        const query = input.value.toLowerCase();
        const active = document.querySelector(".asset-section.active");
        const category = active.id.replace("-section", "");

        const filtered = assets.filter(a =>
            a.category === category &&
            (a.title.toLowerCase().includes(query) ||
                a.tags?.some(tag => tag.toLowerCase().includes(query)))
        );

        const container = document.getElementById(category);
        container.innerHTML = filtered.length
            ? ""
            : "<p>No matching assets found.</p>";

        filtered.forEach(asset => {
            const card = document.createElement("div");
            card.className = "asset-card";
            card.innerHTML = `
                <button class="favorite-btn" onclick="likeAsset(${asset.id}, this)">
                  ♥ <span class="favorite-count">${asset.likes}</span>
                </button>
                <img src="${asset.imageUrl}" alt="${asset.title}" onerror="this.src='placeholder.jpg'" />
                <h4>${asset.title}</h4>
                <p>${asset.description}</p>
                <small>Tags: ${asset.tags?.join(", ") || "None"}</small>
                <a class="download-btn" href="#" onclick="downloadAsset(${asset.id}, '${asset.fileUrl}'); return false;">Download</a>
            `;

            if (currentUser?.isAdmin || currentUser?.userId === asset.userId) {
                const deleteBtn = document.createElement("button");
                deleteBtn.textContent = "−";
                deleteBtn.className = "delete-btn";
                deleteBtn.title = "Delete Asset";
                deleteBtn.onclick = () => {
                    if (confirm("Are you sure you want to delete this asset?")) {
                        deleteAsset(asset.id);
                    }
                };
                card.appendChild(deleteBtn);
            }

            container.appendChild(card);
        });
    });
}

async function likeAsset(id, button) {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/${id}/like`, {
            method: "POST",
            credentials: "include"
        });
        if (res.ok) {
            const data = await res.json();
            button.querySelector(".favorite-count").textContent = data.likes;
        } else {
            console.error("Failed to like:", await res.text());
        }
    } catch (err) {
        console.error("Error liking asset:", err);
    }
}

async function downloadAsset(id, fileUrl) {
    try {
        await fetch(`${BASE_URL}/api/assets/${id}/download`, {
            method: "POST",
            credentials: "include"
        });
        window.open(fileUrl, "_blank");
    } catch (err) {
        console.error("Download error:", err);
    }
}

async function deleteAsset(id) {
    try {
        const res = await fetch(`${BASE_URL}/api/assets/${id}`, {
            method: "DELETE",
            credentials: "include"
        });
        if (res.ok) {
            alert("Asset deleted.");
            window.location.reload();
        } else {
            alert("Failed to delete asset.");
        }
    } catch (err) {
        console.error("Error deleting asset:", err);
    }
}

function toggleUploadPanel() {
    const panel = document.getElementById("uploadPanel");
    panel.style.display = panel.style.display === "none" ? "block" : "none";
}
