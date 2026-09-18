# Changelog

All notable changes to **WorldSaveMuzzler** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-18

### Added
- Initial standalone release of **WorldSaveMuzzler**.
- **RelocateToTopLeft** (Default): Moves center-screen autosave warnings and world saved notifications into the subtle top-left corner feed.
- **Mute**: Option to completely silence center-screen world save notifications.
- **Native**: Option to restore native center-screen yellow toasts.
- Full support for Valheim native localization keys (`$msg_worldsavewarning`, `$msg_worldsaved`) and common dedicated server notification scripts (`"world save"`, `"saving world"`).
- Automated self-disabling when **Valheim Community Patch Extras** (> v0.28.0) is detected to prevent duplicate or conflicting notification handling.
- Single source of truth (SSOT) MSBuild versioning and automated Thunderstore / Hexium packaging.
