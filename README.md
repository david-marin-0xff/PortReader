# PortReader

PortReader is a Windows-based network utility designed to inspect, log, and analyze active TCP and UDP ports on the local machine. It provides a graphical interface for monitoring listening ports, established connections, associated processes, and related system metadata. The tool is intended for users who want deeper visibility into network activity without relying solely on built-in Windows tools such as netstat.

## Features

- Real-time enumeration of active TCP and UDP ports
- Identification of owning processes and associated executables
- Display of local and remote endpoints
- Automatic refresh and continuous monitoring mode
- Windows Forms graphical interface for ease of use
- Configurable settings for scan intervals and display preferences

## Technical Overview

PortReader is built using C# and .NET, leveraging the System.Net.NetworkInformation namespace to gather network statistics and socket information. The application retrieves process details through the Windows API and uses managed code for data visualization and UI updates.

### Technology Stack
- C# (.NET)
- Windows Forms
- System Diagnostics APIs
- System.Net.NetworkInformation
- Async operations for periodic data refresh

## Running and Building

To build the project:

1. Open the solution in Visual Studio or a compatible IDE.
2. Ensure .NET 8.0 (or the version defined in the project file) is installed.
3. Compile using Build ? Build Solution.
4. The compiled executable will appear inside the bin/Release or bin/Debug directory depending on your build configuration.

To run the compiled application, launch the generated PortReader.exe file from the publish or build output folder.

## Repository Structure

This repository contains only the source code. Build output such as .exe, .dll, and publish artifacts are intentionally excluded using .gitignore, following best practices for .NET development.

## Future Improvements

- Add filtering options for ports, protocols, and processes
- Implement export functionality (CSV or JSON)
- Include historical trend graphs for port activity
- Add logging mode for continuous background monitoring
- Enable dark mode UI theme
- Integrate system notifications for newly opened or closed ports

## License

This project is currently unlicensed and intended for personal use and experimentation. A proper license may be added later depending on development goals.
