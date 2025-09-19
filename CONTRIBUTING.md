# Contributing to OnDisplayOff

Thank you for considering contributing to OnDisplayOff! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows (for testing, as this is a Windows-specific utility)
- Git

### Setting Up the Development Environment

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/yourusername/OnDisplayOff.git
   cd OnDisplayOff
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## How to Contribute

### Reporting Issues
- Use the GitHub issue tracker to report bugs or request features
- Before creating a new issue, please search existing issues to avoid duplicates
- Provide as much detail as possible, including:
  - Steps to reproduce the issue
  - Expected vs actual behavior
  - Your Windows version and .NET version
  - Relevant error messages or logs

### Submitting Changes

1. **Create a branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/issue-description
   ```

2. **Make your changes** following the coding guidelines below

3. **Test your changes** thoroughly:
   - Build the project: `dotnet build`
   - Test the application manually
   - Ensure existing functionality still works

4. **Commit your changes** with descriptive commit messages:
   ```bash
   git commit -m "Add feature: description of what you added"
   # or
   git commit -m "Fix: description of what you fixed"
   ```

5. **Push your branch** and create a pull request:
   ```bash
   git push origin feature/your-feature-name
   ```

## Coding Guidelines

### Code Style
- Follow standard C# conventions and naming patterns
- Use meaningful variable and method names
- Add XML documentation comments for public methods and classes
- Keep methods focused and reasonably sized

### Code Organization
- Place related functionality in appropriate classes
- Use proper access modifiers (public, private, internal)
- Follow the existing project structure

### Error Handling
- Use appropriate exception handling
- Log errors appropriately for debugging
- Provide meaningful error messages to users

### Security
- Never commit sensitive information (keys, passwords, etc.)
- Follow Windows security best practices
- Be cautious with elevated privileges and system calls

## Pull Request Guidelines

### Before Submitting
- Ensure your code builds successfully
- Test your changes on Windows
- Update documentation if necessary
- Rebase your branch on the latest main branch

### Pull Request Description
Please include:
- A clear description of what your changes do
- Why the changes are needed
- Any breaking changes
- Screenshots for UI changes
- Testing steps

### Review Process
- All pull requests require review before merging
- Address any feedback from reviewers
- Keep pull requests focused on a single feature or fix
- Be responsive to comments and suggestions

## Types of Contributions

### Welcome Contributions
- Bug fixes
- Performance improvements
- UI/UX enhancements
- Documentation improvements
- Code refactoring for better maintainability
- New power management features
- Better Windows integration

### Feature Requests
- File an issue first to discuss new features
- Consider backward compatibility
- Ensure features align with the project's goals

## Development Tips

### Testing
- Test on different Windows versions if possible
- Test with various display configurations (single/multiple monitors)
- Verify startup/shutdown behavior
- Test with different power management settings

### Debugging
- Use Visual Studio or VS Code for debugging
- Check Windows Event Viewer for system-level issues
- Test both elevated and non-elevated scenarios

## Getting Help

- Create an issue for questions about the codebase
- Check existing issues and pull requests for similar problems
- Review the README.md for usage and build instructions

## License

By contributing to OnDisplayOff, you agree that your contributions will be licensed under the same MIT License that covers the project.