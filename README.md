# WorldSaveMuzzler

A standalone, 100% client-side quality-of-life mod for Valheim that slaps a muzzle on those obnoxious, screen-stealing world save alerts so they finally shut up and stop screaming across your crosshair while you're fighting for your life.

> [!NOTE]
> **Client-Side Only:** This mod is installed locally on your PC. It does **not** need to be installed on the server and works when playing singleplayer or connecting to any dedicated or community server.

> [!IMPORTANT]
> **Valheim Community Patch Extras Notice:**
> This code was already merged into **Valheim Community Patch Extras**, but is being released standalone for those who do not want the VCP Extra mod installed.
> 
> If Valheim Community Patch Extras is installed, WorldSaveMuzzler will automatically detect it and safely self-disable to prevent duplicate handling.

---

## The Problem

In vanilla Valheim, `Game.UpdateSaving` broadcasts a 30-second autosave warning (`"$msg_worldsavewarning 30s"`) and a save completion alert (`"$msg_worldsaved"`) using `MessageHud.MessageType.Center`. 

This splashes a giant, screen-filling yellow banner directly across your crosshair and combat view. Furthermore, many dedicated servers and admin management scripts broadcast custom countdowns to all players using center-screen toasts.

During intense combat, boss encounters, ocean sailing, or delicate scaffolding construction, having the middle of your screen suddenly blinded by an autosave banner can easily cause mistimed parries, disorientation, accidental falls, or death.

## The Solution

**WorldSaveMuzzler** intercepts `MessageHud.ShowMessage` and provides clean, customizable display options:

- **RelocateToTopLeft (Default)**: Moves world save countdown warnings and save notices into the subtle top-left notification feed. You stay informed of pending saves without having your crosshair or view blocked.
- **Mute**: Silences center-screen world save notifications entirely for an uninterrupted, immersive experience.
- **Native**: Leaves notifications centered across the screen unaltered.

---

## Configuration

Settings can be customized directly in-game using the BepInEx **Configuration Manager** (F1 / F5 depending on configuration) or by editing `BepInEx/config/dreamwraith.WorldSaveMuzzler.cfg`:

| Section | Key | Type | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `1 - General` | `WorldSaveNoticeMode` | `enum` | `RelocateToTopLeft` | Display mode: `RelocateToTopLeft` (subtle top-left corner feed), `Mute` (completely hidden), or `Native` (centered yellow banner). |

---

## Compatibility

- **Valheim Community Patch Extras**: This code was already merged into **Valheim Community Patch Extras**, but is released standalone for those who do not want the VCP Extra mod installed. If VCP Extras is present, WorldSaveMuzzler automatically self-disables to prevent duplicate or conflicting notification handling.
- **100% Client-Side**: Safe to use on any server. The server does not need this mod installed.
- **Smart Detection**: Recognizes native Valheim localization keys (`$msg_worldsavewarning`, `$msg_worldsaved`) as well as plain English countdown broadcasts from common server scripts (e.g., *"World save in 30s"*).
- **Headless Safe**: Inert on headless servers where `MessageHud` is not rendered.

---

## Installation

> [!TIP]
> **Client-Side Only**: Install this on your local PC via your preferred mod manager (Gale / r2modman / Thunderstore) or manual drop. Nothing needs to be installed on the server.

### Mod Manager (Recommended)
1. Install via **Gale**, **Thunderstore Mod Manager**, or **r2modman**.
2. Launch the game and enjoy a clear combat view!

### Manual Installation
1. Ensure **BepInExPack Valheim** is installed.
2. Download and extract the release archive.
3. Place `WorldSaveMuzzler.dll` into your `Valheim/BepInEx/plugins/` directory.
4. Launch Valheim.

---

## Building from Source

The project uses a portable MSBuild configuration that auto-detects standard Steam paths.

```bash
dotnet build -c Release
```

The compiled assembly will be placed in `bin/Release/net48/WorldSaveMuzzler.dll`. Building in `Release` configuration also automatically packages the distribution ZIP to `bin/Publish/WorldSaveMuzzler-<Version>.zip`.

### Custom & CI Paths
For non-standard Steam library locations, copy `WorldSaveMuzzler.csproj.user.example` to `WorldSaveMuzzler.csproj.user` in the project root (this file is git-ignored and automatically loaded by MSBuild):

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <GamePath>D:\SteamLibrary\steamapps\common\Valheim</GamePath>
    <BepInExCorePath>$(UserProfile)\AppData\Roaming\com.kesomannen.gale\valheim\profiles\<ProfileName>\BepInEx\core</BepInExCorePath>
  </PropertyGroup>
</Project>
```

---

## Packaging, Publishing & Releases

All developer automation tools for release management, packaging, and publishing to **Thunderstore** and **Hexium** are organized in the [`.scripts/`](.scripts/) folder:

- **Release Management**: [`release.ps1`](.scripts/release.ps1) compiles in `Release`, creates mod & source archives, extracts changelog notes, and publishes GitHub Releases (Draft by default, or published with `-Publish`) using the `gh` CLI.
- **Packaging & Version Bumping**: [`package.ps1`](.scripts/package.ps1) increments SemVer in `WorldSaveMuzzler.csproj`, updates `manifest.json`, and bundles distribution archives.
- **Portal Publishing**: [`publish.ps1`](.scripts/publish.ps1) uploads directly to Thunderstore and Hexium APIs.
- **CI/CD Workflow**: [`.github/workflows/publish.yml.example`](.github/workflows/publish.yml.example) provides an automated GitHub Actions workflow to publish to Thunderstore and Hexium whenever a GitHub Release is published.

For detailed documentation on flags, workflows, and secret configuration, see [`.scripts/README.md`](.scripts/README.md).

---

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.
