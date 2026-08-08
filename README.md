# SmartRoutine

SmartRoutine is a small Windows desktop app for automating recurring workflows: define a routine once as a sequence of steps — open a website, open a folder, launch an application, open a document — and run the whole thing with a click instead of doing it by hand every time.

![Routine list with two example routines](docs/screenshots/routines-list.png)

![Routine editor showing all four step types, with the "Open folder" step selected](docs/screenshots/routine-editor.png)

<!--
  Both shots above use placeholder example routines/steps ("Morgenroutine",
  "Projekt starten"), not real data. Additional shots worth adding at some
  point:
  1. The execution view (ExecutionForm) mid-routine, showing the step
     counter and navigation buttons.
  2. Optional: the auto-run progress dialog.
-->

## Features

- **Routines** — group any number of steps into a named routine, reorder them by drag & drop, duplicate or disable individual steps without deleting them.
- **Four step types**
  - **Open website** — in the system's default browser, or inside the app via an embedded WebView2 browser.
  - **Open folder** — optionally in a new Explorer window.
  - **Start application** — with arguments and an option to run as administrator.
  - **Open document** — with the file's associated application.
- **Two ways to run a routine**
  - **Guided** — step through manually, with a preview panel and per-step "Run" button.
  - **Auto-run** — every enabled step fires automatically in sequence, with a progress dialog.
- **Per-step behavior** — each step can be shown/hidden, set to start automatically, and set to auto-advance to the next step once it's done.
- **Automatic updates** — SmartRoutine checks GitHub for new releases on startup and can download and apply them itself.

## Download

Grab the latest release from the [Releases page](https://github.com/Erik513/SmartRoutine/releases). Download `SmartRoutine.exe` and run it — no installer needed. From then on, SmartRoutine checks for new versions on startup and offers to update itself.

## Usage

1. Click **+** to create a new routine and give it a name.
2. Click **+ Schritt hinzufügen** to add a step, pick an action type (website / folder / application / document), and fill in its details.
3. Configure whether the step should start automatically and whether it should hand off to the next step on its own.
4. Back in the routine list, use **▶** to step through the routine manually, or **⚡** to run every enabled step automatically.

Routines are stored locally in a small database in `%AppData%\SmartRoutine`.

## Building from source

SmartRoutine targets .NET Framework 4.8.1 (WinForms) and is built with Visual Studio 2022 / MSBuild.

It depends on a sibling repository, [CustomWFUI](https://github.com/Erik513/CustomWFUI) (the shared UI/theming library used across several of this author's apps), referenced as a project reference. Clone both repositories next to each other:

```
repos/
├── SmartRoutine/
└── CustomWFUI/
```

Then open `SmartRoutine.sln` and build — CustomWFUI is built automatically as part of the solution.

Run the test suite (MSTest + Moq) via `SmartRoutine.Tests`.

## Tech stack

- .NET Framework 4.8.1, Windows Forms
- [LiteDB](https://www.litedb.org/) for local storage
- [Microsoft Edge WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) for the in-app browser
- CustomWFUI for the custom dark-themed UI components
