window.joker = {
    listenKeys: function (dotNet) {
        this.stopKeys();
        this._keyHandler = function (event) {
            var tag = event.target && event.target.tagName;
            if (tag === "INPUT" || tag === "TEXTAREA" || (event.target && event.target.isContentEditable)) {
                return;
            }

            var key = event.key;
            var shortcut = null;
            if (key === " " || key === "n" || key === "N") shortcut = "next";
            else if (key === "f" || key === "F") shortcut = "favorite";
            else if (key === "r" || key === "R") shortcut = "reveal";
            else if (key === "c" || key === "C") shortcut = "copy";
            else if (key === "?") shortcut = "help";
            if (!shortcut) return;
            event.preventDefault();
            dotNet.invokeMethodAsync("OnShortcut", shortcut);
        };
        window.addEventListener("keydown", this._keyHandler);
    },
    stopKeys: function () {
        if (this._keyHandler) {
            window.removeEventListener("keydown", this._keyHandler);
            this._keyHandler = null;
        }
    },
    copy: async function (text) {
        await navigator.clipboard.writeText(text);
    },
    share: async function (title, text, url) {
        if (typeof navigator.share !== "function") return "unsupported";
        try {
            await navigator.share({ title: title, text: text, url: url });
            return "shared";
        } catch (error) {
            return error && error.name === "AbortError" ? "cancelled" : "unsupported";
        }
    },
    speak: function (text) {
        if (!window.speechSynthesis) return false;
        window.speechSynthesis.cancel();
        var utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "en-US";
        window.speechSynthesis.speak(utterance);
        return true;
    },
    stopSpeak: function () {
        if (window.speechSynthesis) window.speechSynthesis.cancel();
    },
    celebrate: function () {
        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
        if (typeof confetti !== "function") return;
        confetti({ particleCount: 70, spread: 68, origin: { y: 0.68 }, disableForReducedMotion: true });
    },
    downloadCard: function (card) {
        var canvas = document.createElement("canvas");
        canvas.width = 1200;
        canvas.height = 675;
        var ctx = canvas.getContext("2d");
        var dark = !!card.dark;
        ctx.fillStyle = dark ? "#12121a" : "#f3f4fb";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = dark ? "#1c1c28" : "#ffffff";
        roundRect(ctx, 48, 48, 1104, 579, 28);
        ctx.fill();
        ctx.fillStyle = dark ? "#a5b4fc" : "#4f46e5";
        ctx.font = "600 28px 'Source Sans 3', sans-serif";
        ctx.fillText("Joker's Manor", 96, 120);
        if (card.category) {
            ctx.font = "600 22px 'Source Sans 3', sans-serif";
            ctx.fillText(card.category, 96, 164);
        }
        ctx.fillStyle = dark ? "#f4f4f8" : "#1c1b22";
        ctx.font = "560 48px Fraunces, Georgia, serif";
        var y = wrapText(ctx, card.primary || "", 96, 240, 1000, 60);
        if (card.punchline) {
            ctx.fillStyle = dark ? "#c7d2fe" : "#4338ca";
            ctx.font = "italic 560 36px Fraunces, Georgia, serif";
            wrapText(ctx, card.punchline, 96, y + 28, 1000, 48);
        }
        var link = document.createElement("a");
        link.href = canvas.toDataURL("image/png");
        link.download = card.filename || "joke.png";
        link.click();
    }
};

function roundRect(ctx, x, y, width, height, radius) {
    ctx.beginPath();
    ctx.moveTo(x + radius, y);
    ctx.arcTo(x + width, y, x + width, y + height, radius);
    ctx.arcTo(x + width, y + height, x, y + height, radius);
    ctx.arcTo(x, y + height, x, y, radius);
    ctx.arcTo(x, y, x + width, y, radius);
    ctx.closePath();
}

function wrapText(ctx, text, x, y, maxWidth, lineHeight) {
    var words = String(text).split(/\s+/);
    var line = "";
    words.forEach(function (word) {
        var next = line ? line + " " + word : word;
        if (ctx.measureText(next).width > maxWidth && line) {
            ctx.fillText(line, x, y);
            line = word;
            y += lineHeight;
        } else {
            line = next;
        }
    });
    if (line) ctx.fillText(line, x, y);
    return y + lineHeight;
}
