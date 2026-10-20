(() => {
    const storageKey = "hue-tour-theme";
    const allowedThemes = new Set(["light", "dark"]);

    const applyTheme = (theme) => {
        const selectedTheme = allowedThemes.has(theme) ? theme : "light";
        document.documentElement.dataset.theme = selectedTheme;
        document.documentElement.dataset.bsTheme = selectedTheme;
        return selectedTheme;
    };

    let savedTheme = "light";
    try {
        savedTheme = localStorage.getItem(storageKey) || "light";
    } catch {
        // Storage can be disabled by the browser; the in-memory theme still works.
    }

    applyTheme(savedTheme);

    window.hueTheme = {
        isDark: () => document.documentElement.dataset.theme === "dark",
        toggle: () => {
            const nextTheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
            applyTheme(nextTheme);

            try {
                localStorage.setItem(storageKey, nextTheme);
            } catch {
                // Keep the selected theme for the current page if storage is unavailable.
            }

            return nextTheme === "dark";
        }
    };
})();
