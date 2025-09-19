# Changelog

All notable changes to OnDisplayOff will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- Grace period settings now preserve original time unit format instead of converting to seconds
- Excluded legacy GraceSeconds property from JSON serialization

## [1.0.0] - 2025-01-19

### Added
- Initial release of OnDisplayOff
- Automatic power actions when display turns off (Sleep, Hibernate, Shutdown, Restart)
- Configurable grace period with multiple time units (milliseconds, seconds, minutes, hours, days)
- System tray interface with pause/resume functionality
- Windows startup integration via scheduled tasks
- Settings persistence in JSON format
- Event-driven display power state monitoring
- Support for multiple monitor configurations
- Proper privilege handling for shutdown/restart actions
- Modern Standby (S0) device compatibility

### Technical Details
- Built with .NET 8 and Windows Forms
- Uses Windows Power Management APIs
- Single instance application with named mutex
- Settings stored in `%AppData%\OnDisplayOff\settings.json`

## [0.2.0] - Development

### Added
- Grace period settings with flexible time units
- Enhanced settings form with time unit selection
- Improved error handling for power actions

## [0.1.0] - Initial Development

### Added
- Basic power management functionality
- System tray application structure
- Initial settings management