// Load the bottom nav HTML
document.addEventListener("DOMContentLoaded", async () => {
    const navHTML = `
    <nav class="bottom-nav" id="bottomNav">
        <button onclick="location.href='dashboard.html'">
            <span>🏠</span>
            <span>Home</span>
        </button>
        <button onclick="location.href='assets.html'">
            <span>📂</span>
            <span>Explore</span>
        </button>
        <button class="plus-btn" onclick="toggleUploadPanel()">+</button>
        <button onclick="location.href='profile.html'">
            <span>👤</span>
            <span>Me</span>
        </button>
        <button id="adminActionBtn" style="display: none;" onclick="location.href='admin.html'">
            <span>🛠️</span>
            <span>Actions</span>
        </button>
    </nav>
    `;
    document.getElementById("bottomNavContainer").innerHTML = navHTML;

    // Highlight active page
    const current = window.location.pathname;
    document.querySelectorAll(".bottom-nav button").forEach(btn => {
        if (btn.innerText.toLowerCase().includes(current.replace(".html", "").replace("/", ""))) {
            btn.classList.add("active");
        }
    });

    // ✅ Always run auth check to reveal admin button if admin
    try {
        const BASE_URL = window.location.hostname.includes("localhost")
            ? "http://localhost:7044"
            : "https://gameasset-backend-aj1g.onrender.com";

        const res = await fetch(`${BASE_URL}/api/auth/check-auth`, {
            credentials: "include",
        });

        if (res.ok) {
            const user = await res.json();
            if (user.isAdmin) {
                document.getElementById("adminActionBtn").style.display = "inline-block";
            }
        }
    } catch (err) {
        console.error("Auth check failed in bottom-nav.js", err);
    }
});

// ✅ Global fallback for toggleUploadPanel
if (typeof toggleUploadPanel !== "function") {
    window.toggleUploadPanel = () => {
        const panel = document.getElementById("uploadPanel");
        if (panel) {
            panel.style.display = panel.style.display === "none" ? "block" : "none";
        } else {
            console.warn("No upload panel found on this page.");
        }
    };
}
