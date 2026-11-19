window.jokerTheme = {
    get: function () {
        return localStorage.getItem("joker-theme");
    },
    set: function (mode) {
        localStorage.setItem("joker-theme", mode);
        if (mode === "dark") {
            document.documentElement.setAttribute("data-theme", "dark");
        } else {
            document.documentElement.removeAttribute("data-theme");
        }
    },
    prefersDark: function () {
        return window.matchMedia("(prefers-color-scheme: dark)").matches;
    }
};
