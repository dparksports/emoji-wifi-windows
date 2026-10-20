# 📶 EmojiWifi — give your WiFi a personality

Your WiFi name doesn't have to be `NETGEAR-5G` or `xfinity-1234`. It can be **🍕🍕🍕** — or anything else from **1,364 emojis**.

EmojiWifi is a tiny free app for Windows that helps you dream up a fun emoji WiFi name, get a super-strong password to go with it, and turn it into a **QR code** that friends join by pointing their phone camera at it. No typing, no reading passwords out loud, no "is it a zero or the letter O?"

![Three easy steps: pick a fun name, get a strong password, share your QR code](infographics/infographic-3-steps.png)

---

## ✨ The Emoji Picker: 1,300+ emojis, zero repeats

The heart of the app. Browse the whole emoji keyboard — **smileys, animals, food, travel, games, objects and more** — sorted into 8 tidy categories and searchable by name. Type "pizza", meet 🍕 instantly.

The best part: **it remembers what you've already used.**

- Emojis that appear in any of your past network names get **dimmed**, so you can see at a glance what's new to you
- One checkbox **hides the used ones entirely**
- The Generate button **skips your greatest hits** — every random surprise is actually surprising

![The new Emoji Picker: categories, search, used emojis dimmed, random without reruns](infographics/infographic-emoji-picker.png)

> Used all the classics? With 1,364 emojis and used-tracking, you won't run out for a *very* long time.

## 🚀 How it all works

**Once**, you set things up — it takes about two minutes. **Then**, everyone who visits just scans and they're online.

![Setup: design it, tell your router, post the QR. Guests point their camera and they're in.](infographics/infographic-setup-join.png)

> **The one honest step other tools skip:** your WiFi name lives inside your router, so after you design it here, copy the name and password into your router's app or settings page (the login is usually printed on a sticker under the router). One time, done forever.

## 💡 Need a name? Steal one of these

![Fun WiFi name ideas: Space Station, Pizza Party, Gaming Hub, Love Zone, Cat Zone, Ghost WiFi](infographics/infographic-ideas-tips.png)

The app can also **generate combinations for you** — themed pairs and trios like 🌲🌻🌱 or 🎮👾🤖, always within the length every phone can display.

**Golden rule: keep it to about 8 emoji or fewer** so it shows up nicely on every phone and laptop.

## 🖥️ What the app looks like

![EmojiWifi app screenshot](screenshots/app_screenshot.png)

A clean little window with everything on one screen: the emoji picker, name ideas, your password, your QR code, and a history of setups you've used before (click any entry to bring it back).

## 🆕 New in the 2026.10.19 update

- **Emoji pool grew from 62 to 1,364** — the full Unicode 14.0 catalog, hand-filtered for WiFi use (no flags or skin-tone variants: they show up as broken boxes on some phones and routers)
- The all-new **Emoji Picker** tab with categories, search, and used-emoji dimming
- **"Skip emojis I've already used"** when generating (on by default)
- **Auto-generated themed combinations** — the ideas list is now much bigger than the 64 classics
- Color emoji in the combinations list, and clearer "too long" warnings

Full history in the [CHANGELOG](CHANGELOG.md).

## 📥 Get it (free, ~1 minute)

1. **Download** the zip from the [latest release](https://github.com/dparksports/emoji-wifi-windows/releases/latest). It's about **70 MB** — that's the app carrying its own engine inside, so you never have to install anything else (no ".NET", no runtimes, nothing).
2. **Unzip it** — right-click the file → *Extract All*.
3. **Double-click `EmojiWifiWindows.exe`**. That's the whole install — nothing else gets added to your computer.
4. First time? Windows may show a blue "Windows protected your PC" screen because we're a small free app without an expensive Microsoft certificate. Click **More info → Run anyway**. Our code is open — anyone can read it [right here](https://github.com/dparksports/emoji-wifi-windows).

> The first launch takes a few extra seconds (the app unpacks its engine once). After that it opens instantly.

## ❓ Questions people actually ask

**Does EmojiWifi change my router by itself?**
No. You design your name and password here, then paste them into your router once. We'd rather you know exactly what's happening to your internet.

**Can people join without typing anything?**
Yes — that's the whole point! iPhone and Android cameras understand WiFi QR codes natively. Point, tap the popup, online. 🎉

**I've already used all the good emojis. Now what?**
Open the Emoji Picker. Everything you've used is dimmed, and the Generate button avoids your past names automatically — so what you see is what's new. With 1,364 to choose from, "all the good ones" takes a while. 😄

**Will every emoji show up on every phone?**
We ship the well-established Unicode set (through Emoji 14.0, 2021) and deliberately exclude flags and skin-tone variants, which render inconsistently across devices. Anything in the picker displays reliably on modern phones, laptops, and routers.

**I lost my WiFi password! 😱**
Open EmojiWifi → *Import QR Code* → pick any QR image you saved or screenshotted. Your name and password come right back.

**Is it safe? Where does my data go?**
Nowhere. The app works fully offline — no account, no sign-up, no tracking, nothing sent anywhere. Your names, passwords, and history stay in a little file on your own PC.

**What does it cost?**
Nothing. Free and open source (Apache 2.0 license).

**Windows only?**
For now, yes — Windows 10 or 11.

---

*Made for everyone whose WiFi deserves a better name.* 😊

**For developers:** built with .NET 8 + WPF in C#, tests and CI included — see [CHANGELOG.md](CHANGELOG.md) and the [source](https://github.com/dparksports/emoji-wifi-windows). The emoji catalog is generated from Unicode's official `emoji-test.txt` by `tools/generate_emoji_catalog.py`. Licensed under [Apache 2.0](LICENSE).
