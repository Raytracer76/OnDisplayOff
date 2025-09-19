# Security Policy

## Supported Versions

OnDisplayOff follows semantic versioning. Security updates are provided for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of OnDisplayOff seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### How to Report

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please send an email to: [Your email address here]

You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

### What to Include

Please include the following information in your report:

- Type of issue (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit the issue

### Our Commitment

- We will respond to your report within 48 hours with our evaluation of the report and an expected resolution date
- We will handle your report with strict confidentiality and will not pass on your personal details to third parties without your permission
- We will keep you informed of the progress towards resolving the problem
- We may ask for additional information or guidance

## Security Considerations

OnDisplayOff handles several security-sensitive operations:

### System-Level Operations
- **Power Management**: The application can trigger system sleep, hibernate, shutdown, and restart
- **Scheduled Tasks**: Creates Windows scheduled tasks with elevated privileges for shutdown/restart
- **System Event Monitoring**: Registers for Windows power broadcast notifications

### Privilege Requirements
- **Standard Operations**: Sleep and hibernate work with standard user privileges
- **Elevated Operations**: Shutdown and restart require elevated privileges through scheduled tasks
- **Startup Integration**: Creating startup tasks requires administrative privileges initially

### Potential Security Risks
- **Privilege Escalation**: Improper handling of scheduled tasks could lead to privilege escalation
- **Denial of Service**: Malicious triggering of power actions could cause system unavailability
- **Configuration Tampering**: Unauthorized modification of settings could alter system behavior

### Security Measures Implemented
- **Minimal Privileges**: Application runs with minimal required privileges
- **Safe Defaults**: Conservative default settings to minimize unintended behavior
- **Input Validation**: Proper validation of user settings and system inputs
- **Error Handling**: Graceful handling of system errors without exposing sensitive information

## Best Practices for Users

### Installation Security
- Download OnDisplayOff only from official sources (GitHub releases)
- Verify file integrity before installation
- Run with standard user privileges when possible

### Configuration Security
- Review power action settings carefully
- Use appropriate grace periods to prevent accidental shutdowns
- Monitor scheduled tasks created by the application

### System Security
- Keep Windows and .NET runtime updated
- Use standard Windows security practices
- Monitor system logs for unusual power management events

## Disclosure Policy

When we receive a security bug report, we will:

1. Confirm the problem and determine the affected versions
2. Audit code to find any potential similar problems
3. Prepare fixes for all supported releases
4. Release new versions as soon as possible

We will coordinate the timing of the disclosure with you. We prefer to fully disclose the bug as soon as possible once a user mitigation is available.

## Comments on This Policy

If you have suggestions on how this process could be improved, please submit a pull request or create an issue to discuss.