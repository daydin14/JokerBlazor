#!/usr/bin/env python3
"""Rewrite a published Blazor WebAssembly wwwroot for the /JokerBlazor/ GitHub Pages site."""

from pathlib import Path
import sys

OLD_BASE = '<base href="/" />'
NEW_BASE = '<base href="/JokerBlazor/" />'
REQUIRED = ("index.html", "404.html", ".nojekyll")


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: prepare-github-pages.py <published-wwwroot>", file=sys.stderr)
        return 2

    root = Path(sys.argv[1])
    index = root / "index.html"
    if not index.is_file():
        print(f"missing {index}", file=sys.stderr)
        return 1

    text = index.read_text(encoding="utf-8")
    if OLD_BASE not in text:
        print(f"expected {OLD_BASE} in {index}", file=sys.stderr)
        return 1

    index.write_text(text.replace(OLD_BASE, NEW_BASE, 1), encoding="utf-8")

    missing = [name for name in REQUIRED if not (root / name).exists()]
    if missing:
        print("missing published files: " + ", ".join(missing), file=sys.stderr)
        return 1

    framework = root / "_framework"
    if not framework.is_dir():
        print(f"missing {framework}", file=sys.stderr)
        return 1

    print(f"Prepared {root} with base href /JokerBlazor/")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
