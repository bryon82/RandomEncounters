# Changelog

All notable changes to this project will be documented in this file.

## [v1.1.7] - 2025-04-11

### Removed
- Constant light wave noise playing throughout dense fog encounter

## [v1.1.6] - 2025-04-11

### Fixed
- Seagulls not showing up for fishing bonanza

## [v1.1.5] - 2025-04-09

### Added
- Intense storm encounter
- Fishing bonanza encounter
- Settings to enable/disable the new encounters
- Settings for the duration of the new encounters

## [v1.1.4] - 2025-02-26

### Added
- Setting to enable or disable dense fog encounter
- Setting to enable or disable flotsam encounter

### Fixed
- Fog duration setting was not being initialized, potential source of infinite fog bug

## [v1.1.3] - 2024-09-21

### Added
- Dense fog encounter

## [v1.1.2] - 2024-09-17

### Added
- Make one of the whales play an animation when spawned for better visibility that a sealife encounter was rolled

### Updated
- The check to see if whales are far enough away to destroy so that is less of a performance hit

### Removed
- Dependency on SailwindModdingHelper

## [v1.1.1] - 2024-09-16

### Updated
- Stop whales spawning from the SeaLifeMod instead of having them spawn and immediately destroyed
- Shifting world is now set as the parent for wreckage

## [v1.1.0] - 2024-09-16

### Added
- Ship wreckage to flotsam encounters

### Updated
- README.md text to clarify requirements for encounter chance

## [v1.0.1] - 2024-09-14

### Updated
- Spawning of whales now has a chance to spawn multiples

## [v1.0.0] - 2024-09-13

### Added
- Random encounter framework
- Flotsam encounters
- Control of SeaLifeMod to generate whale encounters
