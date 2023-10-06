# VoodooTune SDK changelog

## [1.4.0] - 2022-11-14

### ✨ Added
- VoodooDebug.cs class
- VoodooTunePersistentData now handles the current configuration and its URL

### 🐛 Fixed
- VoodooConfig file were using Properties instead of Field which mades it impossible to display using reflection on fields.
- TypeUtility & WebRequestUtility were using old VoodooStore guids which has been changed to avoid further conflicts with our users project.

### 🔄 Changed
- Voodootune has been renamed to `VoodooTuneConfigurationManager` and is not a static class anymore. Its purpose has been reoriented around the configuration : `Create` the urls, `Load` the configuration and `Retrieve` the items.
- Namespace from `VoodooTune.XXX` to `Voodoo.Tune.XXX`
- Move ServerDisplayNames & StatusDisplayNames from `VTEDConstants` to `VoodooTunePersistentData`.
- Separate Voodootune & VoodooTuneRequest to keep Voodootune use for SDK users and VoodooTuneRequest for API dialog.
- LoadConfiguration will now return a ConfigResponse instead of multiple parameters
- VoodooTune tokens are now located in VoodooTuneClient
- VoodooSauceVariables will now used interface implementation (IVoodooTuneFilledVariables) instead of reflection to get the required parameters
- Every client model now stores its own Header to prevent the client from calling the url with an inappropriate token (ex: calling tech with a staging token)

### 🗑 Removed
- IClient interface (has been replaced by an abstract class AbstractClient.cs to avoid having access to BaseURl and Header)

## [1.2.1] - 2022-10-19

### 🔄 Changed
- Move Status and Server definition to `VoodootunePersistentData` class.

## [1.2.0] - 2022-10-18

### ✨ Added
- Sandboxes management.
- Layers variable in the `VersionMetadata` endpoint.
- VoodooTune empty constructor with auto-initialization using the server saved in the PlayerPrefs.
- The Status `wip`/`live` is now accessible from the VoodooTune class.
- New data management system for PlayerPrefs `VoodooTunePersistentData`.
- OldMetadata endpoint (VersionMetadata for remote-settings v2).
- `Equals` method for `ABTest`, `Cohort`, `Segment`, `Sandbox` and `Layer`.
- Package published in the [VoodooSauce 7.0.0][vs-url].

### 🐛 Fixed
- `Catalog` was not accessible from the `App` endpoint.
- The backend does not allow sending something different than `wip` anymore for the `Duplicate` endpoint in ABTestClient. Thus, it has been reverted to [0.7.5].
- The `GetDefaultConfigurationURL` was throwing an error when the dictionary was null.
- The WebRequest was keeping the last header in memory.
- The WebRequest was crashing without header defined.


### 🔄 Changed
- Simulation URLs now takes arrays instead of lists.

### 🗑 Removed
- The temporary models made to match the VoodooSauce system.

## [1.0.0] - 2022-09-26

### ✨ Added
- Now handles multi-layer

### 🐛 Fixed
- ABTestClient model was using the enum `Status` instead of `ABTestState` for the UpdateStatus method.
- CohortResponse was trying to retrieve `name` instead of `abTestName` from the API.
- NewCohort wasn't working because of a missing initialization in the default case.
- WebRequest CancelAllTasks is not working properly.
- The `List<T>` were being deserialized by adding values on top of the default ones instead of replacing them.

### 🔄 Changed
- VersionResponse variables `VersionName` -> `Name` & `VersionId`-> `Id`.
- The `Duplicate` endpoint from ABTestClient matches the modifications made to the API.
- We are now using `JsonSerializerSettings` instead of Converters.
- WebRequest's `Timeout` has been renamed to `TimeoutInSeconds` to better reflect what it is.
- WebRequest's `TimeoutInSeconds` has been increased from `5` to `10` seconds.

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