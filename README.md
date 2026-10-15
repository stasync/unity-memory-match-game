# Unity Memory Match Game

> **Status: raw prototype.** The core rules, a basic UI and in-memory save/load work end to end. Everything else is placeholder or missing, so expect rough edges.

A card-matching ("concentration") game built in Unity. Flip two cards per turn and clear the board by finding every pair.

## What the prototype covers

**Gameplay**
- Boards from 2×2 up to 6×6. The cell count must be even. The default is 5×6.
- Cards are dealt as shuffled pairs (Fisher–Yates).
- A 3-second preview shows every card face-up before play starts.
- Each turn flips two cards. A match stays face-up. A mismatch flips back after 1 second, or as soon as the next card is clicked.
- Tracks turns, matches, the current combo (consecutive matches) and elapsed time.

**Save / Load**
- Save takes a binary snapshot of the current session (stats and board), and Load restores it.
- Snapshots live in memory only and are lost when the app closes.

**UI**
- uGUI grid board with square cards sized to the board width, and pooled card views.
- Simple scale-based flip animation driven by animation curves.
- Start/Stop, Save and Load buttons, plus a turns/matches/time panel.

**Code**
- Game rules are kept separate from presentation, across the assemblies listed under [Project layout](#project-layout).
- One EditMode test covers the save/load round trip.

## Not done yet / known issues

- No game-over screen. The timer keeps running after the last pair is found.
- The time display only refreshes at the end of each turn.
- Combo is tracked but not shown in the UI.
- Saves are not written to disk.
- Loading a save replays the face-up preview.
- Only 16 card faces exist, so a 6×6 board (18 pairs) can't be played yet. Start logs an error instead.
- Card art is a placeholder. There is no audio.

## Running it

1. Open the project in Unity 6000.4.3f1.
2. Open `Assets/Scenes/BootstrapScene.unity` and press Play.
3. Click **Start**.

To change the board size, edit `Desired Rows` / `Desired Columns` on `Assets/Prefabs/GameViewController.prefab`.

For debugging, turn on `Show Debug Overlay` on the same prefab. It draws the prototype IMGUI panel (top-left), which shows every card's type and has its own Start/Save/Load buttons.

To run the tests, open **Window → General → Test Runner**, select **EditMode** and click **Run All**.

## Project layout

| Assembly | Folder | Responsibility |
| --- | --- | --- |
| `Code.State` | `Assets/Code/State` | Plain board data (`BoardCellState`, `BoardState`). |
| `Code.GameManagement` | `Assets/Code/GameManagement` | Game rules (`GameSession`), session lifetime and save/load (`GameManager`), serialization. |
| `Code.Presentation` | `Assets/Code/Presentation` | uGUI views: board, cards, score panel, buttons. |
| `Code.Bootstrap` | `Assets/Code/Bootstrap` | Entry point. Creates the `GameManager` and the game view. |
| `Code.Tests` | `Assets/Code/Tests` | EditMode tests. |
