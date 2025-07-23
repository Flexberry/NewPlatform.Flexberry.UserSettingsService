# Flexberry UserSettingsService Changelog
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

### Added
- Added to `FlexberryUserSettingBS` constructor with `ICurrentUser`.
Now it needs resolving by dependency injection system (unity or other)).
If Unity it cam be something like:
```xml
<register type=\"NewPlatform.Flexberry.ORM.CurrentUserService.ICurrentUser, NewPlatform.Flexberry.ORM.CurrentUserService\" mapTo=\"NewPlatform.Flexberry.ORM.CurrentUserService.EmptyCurrentUser, NewPlatform.Flexberry.ORM.CurrentUserService\">
    <constructor />
</register>
```

### Changed
- Updated `NewPlatform.Flexberry.ORM` up to '8.0.0-beta01'.

### Deprecated

### Removed
- Removed `DataServiceWrapper` as base class for `UserSettingsService` (it is removed from `NewPlatform.Flexberry.ORM`).
- Removed static field `IUserSettingsService Current` from `UserSettingsService`.
- Removed constructor of `UserSettingsService` that does not contain parameters.

### Fixed

### Security

## [4.0.0] - 2021-05-30

### Added
- .NET Standard 2.0 implementation. NuGet package contains net45 and netstandard2.0 targets.

### Changed
* csproj format to `Microsoft.NET.Sdk`.
