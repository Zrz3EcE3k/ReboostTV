# ReboostTV (NakopniTV) v1.0

**1-Click Android, Google & Fire TV optimizer.** Debloat laggy TVs, block ads via Private DNS, clean hidden system cache, run Wi-Fi/RAM diagnostics, control your TV with a PC remote & keyboard, and monitor performance with a live auto-hiding TV HUD.

> 🇨🇿 Českou verzi dokumentace najdeš v [README.cs.md](README.cs.md).

---

## Key Features

* **1-Click Boost:** Safe debloat of unnecessary preinstalled apps, deep system cache trim, 0.5x animation speed-up, silent menu toggle, and auto-sleep control.
* **Private DNS Adblock & Family Filter:** One-click DNS-over-TLS setup (`dns.adguard-dns.com` or `family.adguard-dns.com`) to block ads, trackers, or inappropriate content across TV apps.
* **TV Health Score & 1-Click Restore:** Instant TV health rating (`Before -> After`), automatic local backup in `%AppData%\ReboostTV\`, and a round **Restore Defaults** button that reverts animations, DNS, sounds, and restored system packages.
* **Auto Network Scanner:** Automatically scans active local LAN/Wi-Fi subnets (`192.168.x.x`, `10.0.x.x`, `172.16.x.x`) for TVs with ADB enabled on port `5555`.
* **X-Ray & PC Remote:** RAM Kill Switch, Wi-Fi Ping test (`1.1.1.1`), live RAM status, visual D-Pad TV remote, and PC-to-TV keyboard for typing passwords effortlessly.
* **ReboostTV Turbo HUD (`.apk`):** Lightweight Android TV companion app with a translucent floating RAM/DNS pill that automatically hides during video playback (Auto-Hide) plus a built-in Kill Switch.
* **4 Languages & Dark/Light Mode:** Instant switching between Czech, Slovak, English, and German, plus full Dark and Light themes.

---

## Security-by-Design Architecture

ReboostTV is built for transparency and safety:
* **No command injection:** All ADB commands, safe package lists, and DNS hostnames are hardcoded in C# (`KnownTargets.cs` and `AdbService.cs`).
* **Strict JSON validation:** Optional `config.json` profiles only accept predefined boolean switches and enum modes. Unknown keys, comments, or oversized files (> 32 KB) are rejected automatically with a safe fallback (`ConfigValidator.cs`).
* **Zero shell execution:** ADB is invoked directly via `ArgumentList` with `UseShellExecute = false`.

---

## Quick Start

1. On your Android/Google/Fire TV, enable **Developer Options** and turn on **USB / Network Debugging (ADB)**.
2. Run `ReboostTV.exe` on Windows (make sure `adb.exe`, `AdbWinApi.dll`, `AdbWinUsbApi.dll`, and `ReboostTV-Turbo.apk` are in the same folder).
3. Select your TV from the auto-scanned list, click **Connect TV**, approve the prompt on your TV screen, and press **RUN 1-CLICK BOOST**.

---

## Building from Source

Automated builds for both Windows (`ReboostTV.exe`) and Android TV (`ReboostTV-Turbo.apk`) run via GitHub Actions on every push to `main`.

* **Windows (.NET 8 SDK):**
  ```bash
  dotnet build pc-app/ReboostTV.sln
  ```
* **Android TV (JDK 17 + Gradle 8.7):**
  ```bash
  cd tv-app/ReboostTV-Turbo
  gradle assembleDebug
  ```

---

## License

This project is licensed under the **ReboostTV Source-Available License** — see the [LICENSE](LICENSE) file for details.