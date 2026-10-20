#!/usr/bin/env python3
"""Generate Services/EmojiCatalog.json for EmojiWifiWindows.

Source data is Unicode's official emoji-test.txt (UTS #51 keyboard data).
Using a specific release directory (default 14.0) guarantees every emoji in
the catalog shipped no later than that Unicode version, which keeps the pool
renderable on the phones and routers that will display the SSID.

Filters:
  - keep only fully-qualified emoji (consistent color rendering everywhere)
  - drop the "Flags" group (Windows renders flags as letter pairs, not flags)
  - drop the "Component" group (skin tones, ZWJ roles, hair)
  - drop anything containing ZWJ (U+200D) or skin-tone modifiers
    (U+1F3FB..U+1F3FF): these are multi-glyph sequences that bloat the
    32-byte SSID budget and render as separate emoji on old devices
  - keycaps and VS16 (U+FE0F) forms are kept; they are single visual emoji

Usage:
  python tools/generate_emoji_catalog.py [path-to-emoji-test.txt]

Without an argument the script downloads the 14.0 file from unicode.org.
"""

import json
import re
import sys
import urllib.request
from pathlib import Path

EMOJI_TEST_URL = "https://www.unicode.org/Public/emoji/14.0/emoji-test.txt"
OUTPUT_PATH = Path(__file__).resolve().parent.parent / "Services" / "EmojiCatalog.json"

EXCLUDED_GROUPS = {"Flags", "Component"}
FITZPATRICK = range(0x1F3FB, 0x1F400)  # skin tone modifiers U+1F3FB..U+1F3FF
REGIONAL_INDICATOR = range(0x1F1E6, 0x1F200)
TAG_CHARS = range(0xE0020, 0xE0080)
ZWJ = 0x200D
MAX_SSID_BYTES = 32

ENTRY_RE = re.compile(r"^(?P<codes>[0-9A-F ]+?)\s*;\s*(?P<status>\S+)\s*#\s*(?P<emoji>\S+)\s+(?P<version>E\d+\.\d+)\s+(?P<name>.+)$")


def parse(path: Path) -> list[dict]:
    entries = []
    group = subgroup = ""
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if line.startswith("# group:"):
            group = line[len("# group:"):].strip()
        elif line.startswith("# subgroup:"):
            subgroup = line[len("# subgroup:"):].strip()
        elif line and not line.startswith("#"):
            match = ENTRY_RE.match(line)
            if not match:
                sys.exit(f"Unparseable line: {line!r}")
            if match["status"] != "fully-qualified":
                continue
            if group in EXCLUDED_GROUPS:
                continue
            code_points = [int(cp, 16) for cp in match["codes"].split()]
            if any(cp == ZWJ for cp in code_points):
                continue
            if any(cp in FITZPATRICK for cp in code_points):
                continue
            if any(cp in REGIONAL_INDICATOR for cp in code_points):
                continue
            if any(cp in TAG_CHARS for cp in code_points):
                continue
            emoji = match["emoji"]
            if len(emoji.encode("utf-8")) > MAX_SSID_BYTES:
                continue
            entries.append({
                "Emoji": emoji,
                "Name": match["name"].strip(),
                "Category": group,
                "SubCategory": subgroup,
            })
    return entries


def main() -> None:
    if len(sys.argv) > 1:
        source = Path(sys.argv[1])
    else:
        print(f"Downloading {EMOJI_TEST_URL} ...")
        request = urllib.request.Request(EMOJI_TEST_URL, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(request, timeout=60) as response:
            data = response.read()
        source = Path("emoji-test.txt")
        source.write_bytes(data)

    entries = parse(source)
    seen = set()
    deduped = []
    for entry in entries:
        if entry["Emoji"] in seen:
            continue
        seen.add(entry["Emoji"])
        deduped.append(entry)

    OUTPUT_PATH.write_text(json.dumps(deduped, ensure_ascii=False), encoding="utf-8")

    categories: dict[str, int] = {}
    for entry in deduped:
        categories[entry["Category"]] = categories.get(entry["Category"], 0) + 1
    print(f"Wrote {len(deduped)} emoji to {OUTPUT_PATH} ({OUTPUT_PATH.stat().st_size} bytes)")
    for category, count in categories.items():
        print(f"  {category}: {count}")


if __name__ == "__main__":
    main()
