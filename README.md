# ReboostTV (NakopniTV) v1.0

**1-Click zrychlovač pro Android TV, Google TV a Fire TV.** Vyčistí balast, zablokuje reklamy přes Private DNS, uvolní skrytou cache, udělá rentgen Wi-Fi a RAM, přidá PC ovladač s psaním hesel i živý mizející HUD panel přímo do televize.

> 🇬🇧 English documentation is available in [README.md](README.md).

---

## Hlavní funkce

* **1-Click Boost:** Bezpečné odstranění balastu (Safe Debloat), vyčištění skryté systémové cache, zrychlení animací na `0.5x`, tiché menu a vypnutí automatického usínání.
* **Private DNS (Adblock & Rodinný filtr):** Nastavení šifrovaného DNS jedním klikem (`Adblock` pro blokování reklam a trackerů nebo `Family` pro ochranu dětí).
* **Skóre zdraví TV & Obnova výchozího stavu:** Přehledné skóre televize (Před → Po), automatická záloha do `%AppData%\ReboostTV\` a kulaté zelené tlačítko **Obnovit výchozí**, které vrátí animace, DNS, zvuky i systémové aplikace zpět.
* **Automatický sken domácí Wi-Fi:** Po spuštění prohledá aktivní domácí sítě (`192.168.x.x`, `10.0.x.x`, `172.16.x.x`) a najde televize s povoleným laděním na portu `5555`.
* **Rentgen TV & PC Ovladač:** Kill Switch pro okamžité uvolnění RAM, Wi-Fi Ping test (`1.1.1.1`), stav paměti, vizuální TV ovladač s kruhovým D-Padem a klávesnice z PC pro rychlé psaní hesel.
* **ReboostTV Turbo HUD (`.apk`):** Lehká doprovodná aplikace do televize s plovoucí pilulkou (RAM + stav DNS), která při sledování videa sama zmizí (Auto-Hide), a vestavěným Kill Switchem.
* **4 jazyky & Dark/Light mód:** Okamžité přepínání mezi češtinou, slovenštinou, angličtinou a němčinou + kompletní tmavý a světlý režim.

---

## Bezpečnostní architektura

* **Žádná injektáž příkazů:** Všechny ADB příkazy, povolené balíčky i DNS adresy jsou pevně zapsány v C# kódu (`KnownTargets.cs`, `AdbService.cs`).
* **Přísná validace `config.json`:** Konfigurační soubor přijímá jen definované přepínače a režimy. Jakékoliv pole navíc nebo neplatný formát validátor okamžitě zamítne a použije bezpečný fallback.
* **Přímé spouštění ADB:** Proces `adb.exe` se spouští výhradně přes `ArgumentList` (`UseShellExecute = false`), nikdy přes příkazový řádek Windows.

---

## Rychlý start

1. V televizi zapni **Možnosti pro vývojáře** a povol **Ladění přes USB / síť (ADB)**.
2. Na Windows spusť `ReboostTV.exe` (ve stejné složce měj `adb.exe`, `AdbWinApi.dll`, `AdbWinUsbApi.dll` a `ReboostTV-Turbo.apk`).
3. Vyber televizi ze seznamu, klikni na **Připojit TV**, potvrď hlášku na obrazovce televize a stiskni **SPUSTIT 1-CLICK BOOST**.

---

## Licence

Projekt je šířen pod licencí **ReboostTV Source-Available License** — podrobnosti najdeš v souboru [LICENSE](LICENSE).


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