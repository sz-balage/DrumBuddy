<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/sz-balage/DrumBuddy">
    <img src="images/applogo.png" alt="Logo" height="120">
  </a>
  <p align="center">
    A cross-platform app for drummers, allowing them to record, manage, and learn new digital sheets, with real-time feedback.
    <br />
    <strong>Bachelor's Thesis Project by Szabó Balázs</strong>
    <br />
    <br />
    <a href="https://github.com/sz-balage/DrumBuddy/wiki">Explore DrumBuddy wiki »</a>
    <br />
    <a href="https://github.com/baluka1118/DrumBuddy/issues/new?labels=bug&template=bug_report.md">Report Bug</a>
    &middot;
    <a href="https://github.com/baluka1118/DrumBuddy/issues/new?labels=enhancement&template=feature-request.md">Request Feature</a>
  </p>
</div>


---

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>


---

## About The Project

**DrumBuddy** is a cross-platform desktop application built for drummers who want to record, analyze, and improve their rhythm.  
It allows users to record their beats onto a **digital sheet**, get **real-time feedback**, and **store their ideas** for future practice or composition.

One of DrumBuddy’s core features is the ability to **compare two sheets**:
- Use one as a _base sheet_ (your intended rhythm)
- Use another as your _played performance_  
→ The app then provides feedback, helping drummers identify mistakes and improve their timing.

It’s designed to work with **any electronic drum kit**, with **customizable MIDI mappings** for flexible compatibility.
> NOTE: The app has limited drumming functionality, as for now it can only record in 4/4 time signature, and the smallest division unit is a 16th note.

> 🎓 This application also serves as my **Bachelor’s thesis project**

---

### Built With

DrumBuddy is 100% C#, leveraging modern cross-platform and reactive technologies:

* [AvaloniaUI](https://avaloniaui.net/) — for building the cross-platform UI  
* [ReactiveUI](https://reactiveui.net/) & [System.Reactive](https://github.com/dotnet/reactive) — for declarative, reactive programming  
* [SQLite](https://www.sqlite.org/) — lightweight embedded database for sheet persistence  
* [ManagedBass](https://github.com/ManagedBass/ManagedBass) — audio & MIDI library for handling drum input, and metronome audio playback  

---

## Getting Started

### Installation

#### 🪟 Windows
1. Download the latest `DrumBuddy-win-x64.zip` from [Releases](https://github.com/baluka1118/DrumBuddy/releases)
2. Unzip the archive
3. Launch `DrumBuddy.Desktop.exe`
   - Windows Defender may block it — click **“More Info → Run Anyway”**

#### 🐧 Linux
1. Download the latest `DrumBuddy-linux-x64.zip` from [Releases](https://github.com/baluka1118/DrumBuddy/releases)
2. Unzip it
3. Navigate into the unzipped folder, and make the executable runnable:
   ```bash
   cd linux-x64
   chmod +x DrumBuddy.Desktop
4. Now you can run the app via terminal or file manager

#### 🍎 macOS 
For the time being, due to a lack of a paid developer certificate, you can only run DrumBuddy on macOS by manually signing it yourself.
##### Silicon 
1. Download the latest `DrumBuddy-osx-arm64.dmg` from [Releases](https://github.com/baluka1118/DrumBuddy/releases)
2. Run the disk image and drag the app to the Applications folder
3. Open up the terminal, and navigate to your Applications folder
4. Execute the following commands:
   ```bash
   sudo xattr -cr DrumBuddy.app
   sudo xattr -rd com.apple.quarantine DrumBuddy.app
   sudo codesign --force --deep --sign - DrumBuddy.app
5. Now you can run the app
The app will ask for your permission to access the documents folder, in order to store exported app related data.
##### Intel
Use the same commands as above, but with the DrumBuddy-osx-x64.dmg build.

## Usage
After installation:
- Connect, and select your electronic drum kit (or try out the app via keyboard input)
- Configure your MIDI mappings in the Configuration section
- Create new sheets either by recording your beats, or creating them manually
- Compare sheets to see the difference

## Roadmap
- [ ] Multiple time signatures, and note division (1/32, triplets, sextuplets)
- [ ] MIDI, and MusicXML export/import 
- [ ] User management, and cloud sync

Any feature requests, and feedback is welcome and appreciated. (https://github.com/sz-balage/DrumBuddy/issues)

## Contact
Szabó Balázs, szabobazsi11182@gmail.com

## Acknowledgements
* [AvaloniaUI Community](https://github.com/AvaloniaUI)
* [ReactiveUI Team](https://github.com/reactiveui)
* [ManagedBass](https://github.com/ManagedBass/ManagedBass)
* [JetBrains Rider](https://www.jetbrains.com/rider/)

---
### For more info, visit the projects' wiki: [DrumBuddy Wiki](https://github.com/sz-balage/DrumBuddy/wiki)

<p align="right">(<a href="#readme-top">back to top</a>)</p>
