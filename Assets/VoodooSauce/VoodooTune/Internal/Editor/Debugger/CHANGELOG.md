# VoodooTune Editor Debugger changelog

## [1.4.0] - 2022-11-14

### 🔄 Changed
- Update to match the VoodooTuneSDK modifications

## [1.2.1] - 2022-10-19

### 🐛 Fixed
- The ConfigurationClass was silently crashing when a new variable was added to an immutable ABTest.

### 🔄 Changed
- The Server and Status are now managed by `VoodooTunePersistentData`.

## [1.2.0] - 2022-10-18

### ✨ Added
- Package published in the [VoodooSauce 7.0.0][vs-url].

### 🐛 Fixed
- A bug where the data wasn't being updated properly when choosing a different cohort in the same ABTest.

### 🔄 Changed
- The persistent data is updated according to VoodooTuneSDK 1.2.0 `VoodooTunePersistentData`.
- By default, it now uses the saved server instead of `tech`.

## [1.0.0] - 2022-09-26

### ✨ Added
- Tasks are now canceled automatically when:
  - Closing the main window
  - Reloading the app information
- Multi-layer management.
- The configuration modified values are now displayed in orange.
- The configuration default values are now displayed inside their label tooltip.

### 🐛 Fixed
- Now compatible with the VoodooSauce version that will handle multi layer.
- Some errors due to Task cancellation were not being caught.


### 🔄 Changed
- VersionResponse variables `VersionName` -> `Name` & `VersionId`-> `Id`.
- Reorganize Widget folder.
- Field's display calculation is now done once instead of every frame.
- Move the main classes to a `Core` folder.

## [0.7.5] - 2022-08-12
### ✨ Added
- Package published in the [VoodooSauce 6.5.0][vs-6.5.0].
- Initial release

[vs-url]: https://voodoo.zendesk.com/hc/en-us/sections/4408005607954-NEWEST-RELEASE
[vs-6.5.0]: https://voodoo.zendesk.com/hc/en-us/articles/6154066386844-Version-Information-VS-v6-5
[1.4.0]: https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox/compare/1.2.1...1.4.0
[1.2.1]: https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox/compare/1.2.0...1.2.1
[1.2.0]: https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox/compare/1.0.0...1.2.0
[1.0.0]: https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox/compare/0.7.5...1.0.0
[0.7.5]: https://github.com/VoodooTeam/VoodooTune-Unity-Toolbox/releases/tag/0.7.5