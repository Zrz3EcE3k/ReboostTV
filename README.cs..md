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