# PortReader

PortReader is a lightweight Windows application designed to monitor, inspect, and log active TCP and UDP ports in real time.  
It provides a simple WinForms interface for enumerating active connections, checking port states, refreshing results quickly, and exporting data for later analysis.

This project is intended as a learning-oriented tool focusing on networking fundamentals, process-to-port mapping, and Windows socket APIs.  
It demonstrates how to query system-level connection tables, update UI components safely, and structure small desktop utilities for maintainability.

<img width="1902" height="1015" alt="image" src="https://github.com/user-attachments/assets/d4d59bd3-e1a5-4a45-ade4-d733a9d41ea1" />


## Features
- Real-time TCP and UDP port enumeration  
- Displays local/remote addresses and process IDs  
- Auto-refresh option for continuous monitoring  
- Ability to export results for offline review  
- Clean, minimal WinForms UI  
- Written in C# (.NET)

## Future Improvements
- Add filtering (PID, protocol, port ranges)  
- Include a command-line mode  
- Better error handling and logging  
- Optional dark mode UI  
- Performance optimizations for large tables

## Build Requirements
- Windows  
- .NET SDK (6.0 or later)  
- Visual Studio or any compatible C# IDE  

## Repository Structure
/PortReader.csproj
/Form1.cs
/Form1.Designer.cs
/Program.cs
/SettingsForm.cs
/.gitignore

## Badges
![Build Status](https://img.shields.io/badge/build-passing-brightgreen)  
![Platform](https://img.shields.io/badge/platform-windows-blue)  
![Language](https://img.shields.io/badge/language-C%23-green)  
![Framework](https://img.shields.io/badge/.NET-6.0+-blueviolet)



