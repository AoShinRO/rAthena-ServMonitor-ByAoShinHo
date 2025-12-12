---

# rAthena Server Monitor by AoShinHo

![Platform](https://img.shields.io/badge/platform-windows-blue)
![Framework](https://img.shields.io/badge/.NET-4.8.1-blue)
![WPF](https://img.shields.io/badge/UI-WPF-green)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/AoShinRO/rAthena-ServMonitor-ByAoShinHo)

A Windows desktop application built with **WPF (.NET Framework 4.8.1)** designed to monitor, manage, and interact with rAthena-based game server environments. It provides real-time process control, categorized log viewing, server statistics, and optional developer tools such as build automation and ROBrowser integration.

---

## 📌 Table of Contents

* [Overview](#overview)
* [Key Features](#key-features)
* [System Requirements](#system-requirements)
* [Dependencies](#dependencies)
* [Installation](#installation)
* [Additional Features](#additional-features)
* [Monitoring Tools](#monitoring-tools)
* [Notes](#notes)

---

## Overview

The **rAthena Server Monitor** offers a unified interface to control and observe four core server processes:

* **Login Server**
* **Char Server**
* **Map Server**
* **Web Server**
- Optional:
* **ROBrowser Server**
* **wsProxy Server**
  
It captures and categorizes console output, provides automatic server detection, offers developer-oriented build features, and integrates optional server support through ROBrowser.

---

## Key Features

* Start and stop emulator processes directly from the UI
* Real-time capture of stdout and stderr
* Smart log categorization:

  * Error, Warning, Debug, SQL, Status, Users
* Player online counter
* System tray background monitoring
* Exportable error logs
* Multi-emulator support with automatic detection:
  * **rAthena**
  * **brHades** (with C++20 support)
  * **Hercules**
* Fully asynchronous process and log handling—no UI freezing

---

## System Requirements

### Operating System

* **Windows 7 / 8 / 10 / 11**
  (WPF applications are Windows-only)

### Required Framework

* **.NET Framework 4.8.1**

---

## Dependencies

### 🔧 Build Tools (optional)

You may use either:

* **MSBuild** (default)
* **CMake** with Unix Makefiles (optional)

All of these **must be added to the Windows PATH**.

### 🌐 For ROBrowser Integration

Required only if using ROBrowser client features:

* **Node.js** + **npm**
* **wsproxy**

All of these **must be added to the Windows PATH**.

---

### 📚 Required .NET Libraries (Development Only)

Included via .csproj:

* System.Management
* System.Windows.Forms
* System.Drawing
* PresentationFramework
* Others required by WPF and console monitoring

---

## Installation

### 1. Build the Application

* Open the solution in Visual Studio
  **or**
* Compile via MSBuild

Available configurations:

* **Debug**
* **Release**

Target framework:

```
.NET Framework 4.8.1
```

### 2. Initial Setup

Configure the paths to the server executables:

* `login-server.exe`
* `char-server.exe`
* `map-server.exe`
* `web-server.exe`

Paths are stored automatically once set.

---

## Additional Features

### 🔨 CMake Build Support

* Alternative compilation path using CMake
* Unix Makefiles generation
* Automatic C++20 flag injection for **brHades**

### 🌍 Multiple Emulator Detection

Automatically identifies the emulator by scanning the solution file:

* `rAthena.sln`
* `brHades.sln`
* `Hercules.sln`

### 🌐 ROBrowser Features

* Build via `npm run build -O -T` (optional `-H`)
* Serve via:
  * `npm run serve` (development)
  * `npm run live` (production)
* Configurable WebSocket proxy (default: **5999**)

### 🔄 Pre-Renewal Build Mode

* Automatically defines the **PRERE** compilation constant

### 🛠 Developer Mode

* Build emulators directly from the monitor
* Supports both MSBuild and CMake
* Real-time compilation output window

### 🎨 UI Customization

* Light/Dark Mode toggle (White Mode)
* Customizable font family and size
* "Skip Loading Messages" to improve client UI performance

---

## Monitoring Tools

* Categorized real-time logs
* Error, Warning, SQL and Status counters
* Player online tracking
* System tray integration showing:
  * Server status
  * Player count
* Export logs with filters
* Non-blocking asynchronous log pipeline

---

## Notes

* This application is **Windows-only** due to reliance on WPF.
* ROBrowser, build tools, and proxy components must be installed separately.
* The monitor supports any fork of rAthena/brHades/Hercules as long as traditional executables are maintained.
* Emulator detection is performed automatically based on `.sln` filenames.

---

