# Changelog

All notable changes to this project will be documented in this file.

## [v1.5.0] - 2026-07-23

### Added
- Hemp, dyes, rubber, coffee, salt and saffron to the pool of possible cargo items that will spawn during flotsom encounters. 
- One tea or coffee box will always spawn with a flotsom encounter. 

### Fixed
- A race condition that would sometimes leave whale sounds not loaded resulting in NullReferenceExceptions.

## [v1.4.0] - 2026-06-16

### Changed
- Changed the way encounters are triggered. Encounters now have a configuragble percent chance to occur, default 60%. When an encounter is triggered, a mod from a list of the enabled mods will then be chosen at random. The encounter types have weights, so some will be chosen more often than others. See the readme for the weights.
- The configs have been changed to remove the max roll increase and add the percent chance for an encounter.

## [v1.3.0] - 2026-06-03

### Updated
- Changed the release process so hopefully some linux users will no longer have issues when unzipping the release file.

### Performance Improvements
- Changed the way the dense fog encounter wave/wind sound fade in and out to reduce number of iterations. Minor performance gain.

## [v1.2.5] - 2025-08-31

### Updated
- Increased distance from the player's ship where the wreck and items spawn during a flotsam encounter.
- Unsealed all sealable crates that spawn during a flotsam encounter.

## [v1.2.4] - 2025-08-08

### Fixed
- Not being able to find the assets when using a mod manager.

## [v1.2.3] - 2025-08-06

### Removed
- Testing code accidentally left in.

## [v1.2.2] - 2025-08-06

### Updated
- Flight pattern of seagulls in fishing bonanza so that they don't fly sideways.

### Added
- Fishing bonanza HooksHangMore compatibility.

## [v1.2.1] - 2025-07-14

### Updated
- Flotsam wreck to be an actual small boat instead of a hull.

### Added
- Config option to set the time range for when a encounter chance occurs.
- Config option to increase the max roll number to decrease chance for an encounter.

## [v1.2.0] - 2025-05-08

### Updated
- Complete rework of SeaLifeMod control. Eliminates stutter when spawning whales.
- Increased number of whales in whale encounter to 2 to 5 whales.
- Improved asset loading time for a quicker startup.
- Refactor for improved encapsulation, code clarity, and ease of future changes.

## [v1.1.8] - 2025-04-14

### Fixed
- Error thrown during fishing bonanza encounter if player falls out of their boat

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
