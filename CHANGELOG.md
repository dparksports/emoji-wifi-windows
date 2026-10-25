# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions are date-based (calendar versioning, `YYYY.M.D`) starting with 2026.10.17.

## [2026.10.24]

### Fixed
- **Generated passwords no longer contain `: , ;`.** The Wi-Fi QR format backslash-escapes `\ ; , : "` inside passwords, and some QR/phone readers display the escapes literally (e.g. `pa\;ss` instead of `pa;ss`). Generated passwords now avoid those characters entirely, so every reader — including ones that don't unescape — shows the exact password. The special-character pool shrinks from 25 to 22 symbols (under 1% less entropy per character); passwords you type yourself are unaffected and still round-trip through QR import correctly.

## [2026.10.19]

### Added
- **Emoji pool expanded from 62 to 1,364 emojis.** The pool is now the full Unicode 14.0 emoji catalog (fully-qualified emoji only; flags, skin tones, and ZWJ sequences are excluded because they render inconsistently across phones, routers, and Windows).
- New **Emoji Picker** tab: browse emojis by category (smileys, animals, food, travel, games…), search by name, and click any emoji to make it your WiFi name.
- The app now knows which emojis you've **already used** (from your history): they're dimmed in the picker, a checkbox hides them entirely, and random generation prefers unused ones by default ("Skip emojis I've already used").
- **Emoji combinations are auto-generated** from the catalog by theme (2–3 same-category emojis, always within the 32-byte SSID limit), so the ideas list keeps growing beyond the 64 hand-picked classics.
- New **"1,300+ emojis. Zero repeats."** infographic for the Emoji Picker, and the three existing guides refreshed for the bigger pool (all four rebuilt cleanly at 1200px).

### Changed
- Emoji data now ships as an embedded resource generated from Unicode's official `emoji-test.txt` by `tools/generate_emoji_catalog.py` — updating to a future Unicode release is a one-command regeneration.
- The combinations list now renders emojis in color.
- README rewritten around the Emoji Picker, with a new FAQ for "I've used all the good emojis".
- Every catalog emoji and generated combination is covered by unit tests (uniqueness, no flags/skin tones/ZWJ, 32-byte SSID limit).

## [2026.10.17]

### Fixed
- Wi-Fi QR codes now escape `\ ; , : "` in the SSID and password per the Wi-Fi Network Config format, so phones parse every generated password correctly (previously passwords containing `:` or `,` produced mis-parsed codes).
- QR import now round-trips SSIDs and passwords containing special characters (spec-compliant unescaping instead of a naive semicolon split).
- "Connect to Network" no longer fails when the SSID or password contains XML-significant characters (`<`, `>`, `&`, quotes): the WLAN profile is properly escaped, and success is determined from the `netsh` exit code instead of English output text.
- Passwords are now generated with a cryptographic RNG (`RandomNumberGenerator`) and always contain at least one character from every selected character class.
- Copying to the clipboard while another application holds it no longer shows an unhandled-exception dialog.
- History no longer fills with unused entries: configurations are saved when you copy, connect, or import, and consecutive duplicates are collapsed instead of piling up.
- WiFi names longer than the 32-byte SSID limit are rejected with a clear message before connecting.

### Changed
- The download is now a **self-contained single-file build**: one `EmojiWifiWindows.exe` (~70 MB) with the .NET runtime baked in — no runtime to install, nothing to configure, unzip and run. (The download used to be 2 MB but required the .NET 8 Desktop Runtime.)
- Removed all telemetry/analytics (Google Analytics). The app is now fully offline — no data ever leaves your PC — and the Settings page reflects that.
- Restored the semicolon to the special-character set (QR escaping now makes it safe).
- EULA: corrected the product name (it previously referenced a different product) and updated the privacy section to reflect the removal of telemetry.

### Added
- Three illustrated guides (`infographics/`) for non-technical users: the 3-step overview, the setup-and-join journey, and name ideas + tips.
- README rewritten from scratch for everyday users — plain language, honest "paste it into your router" step, FAQ, and a SmartScreen note for the unsigned exe.
- Unit test suite (`EmojiWifiWindows.Tests`) covering QR payload escaping/parsing, password rules, history handling, and WLAN profile XML generation.
- CI workflow (build + test on every push/PR) and a Release workflow that builds the clean zip automatically on `v*` tags.
- This changelog.

### Removed
- Release zips and stray duplicate icon/screenshot files from the repository — binaries now live in [GitHub Releases](https://github.com/dparksports/emoji-wifi-windows/releases) only.

## [1.4.2] - 2026-05-26

- Removed semicolons from password generation (superseded by proper QR escaping in 1.4.3).
- Downgraded target framework to .NET 8.0.

## [1.4.1] - 2026-02-08

- Fixed emoji color rendering in History and Sidebar.
- QR code history delete feature and visibility fix.

## [1.4.0] - 2026-02-08

- Added QR code history (auto-save and reload of configurations).

## [1.3.8] - 2026-02-03

- Updated terms, bumped version, and packaged release.

## [1.3.7] - 2026-02-03

- Added manual WiFi entry.

## [1.3.3] - 2026-02-03

- Added global exception handling.

## Earlier releases

See the [commit history](https://github.com/dparksports/emoji-wifi-windows/commits/master) for changes in v1.2.x–1.3.6 (emoji rendering, window sizing, download packaging).
