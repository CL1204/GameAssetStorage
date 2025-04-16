document.addEventListener("DOMContentLoaded", async () => {
    if (!window.BASE_URL) {
        window.BASE_URL = window.location.hostname.includes("localhost")
            ? "http://localhost:7044"
            : "https://gameasset-backend-aj1g.onrender.com";
    }

    const navHTML = `
    <nav class="bottom-nav" id="bottomNav">
        <button onclick="location.href='/dashboard'"><span>🏠</span><span>Home</span></button>
        <button onclick="location.href='/assets/explore'"><span>📂</span><span>Explore</span></button>
        <button class="plus-btn" onclick="toggleUploadPanel()">+</button>
        <button onclick="location.href='/profile'"><span>👤</span><span>Me</span></button>
        <button id="adminActionBtn" style="display:none;" onclick="location.href='/admin'"><span>🛠️</span><span>Actions</span></button>
    </nav>`;
    document.getElementById("bottomNavContainer").innerHTML = navHTML;

    try {
        const res = await fetch(`${BASE_URL}/api/auth/check-auth`, {
            credentials: "include"
        });

        if (res.ok) {
            const user = await res.json();
            window.currentUser = user;

            // Hide login button if logged in
            const loginBtn = document.getElementById("loginBtn");
            if (loginBtn) loginBtn.style.display = "none";

            // Show admin button
            if (user.isAdmin) {
                document.getElementById("adminActionBtn").style.display = "inline-block";
            }
        }
    } catch (err) {
        console.warn("bottom-nav auth check failed", err);
    }
});

if (typeof toggleUploadPanel !== "function") {
    window.toggleUploadPanel = () => {
        const panel = document.getElementById("uploadPanel");
        if (!panel) return console.warn("Upload panel missing");
        panel.style.display = panel.style.display === "none" ? "block" : "none";
    };
}
