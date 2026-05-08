# Endless Runner Game 🎮
**A 2D infinite runner built with Unity**

A fast-paced 2D endless runner where the player dodges obstacles, collects coins, and survives as long as possible while the game progressively gets harder. Built entirely in Unity using C#.

---

## 📸 Screenshots / Gameplay

> 📁 See the `/screenshots` folder in this repo for gameplay recordings and screenshots.

---

## 🎮 Gameplay

- Run automatically — focus on jumping and dodging
- Obstacles spawn randomly and increase in speed over time
- Collect coins to build your score
- Game ends on collision — try to beat your high score
- Smooth animations and sound effects throughout

---

## ✨ Features

- ♾️ **Infinite procedural level generation** — no two runs are the same
- 📈 **Progressive difficulty** — speed and obstacle frequency increase over time
- 🪙 **Coin collection system** with score tracking
- 💀 **Collision detection** with game over screen
- 🔊 **Sound effects** — jump, coin pickup, death
- 🏆 **High score system** — saves your best run locally
- 📱 Responsive controls — keyboard and mobile touch support

---

## 🛠️ Built With

| Tool | Purpose |
|---|---|
| Unity | Game engine |
| C# | Game logic and scripting |
| Unity Animator | Character and obstacle animations |
| Unity Audio | Sound effects and background music |
| Unity Physics 2D | Collision and movement |
| TextMeshPro | UI text rendering |

---

## 🚀 How to Play

### Play in Browser
> 🔗 *(WebGL build link — add if you export to itch.io)*

### Run Locally
1. Clone this repository
```bash
git clone https://github.com/gulfamasim/endless-runner-game
```
2. Open **Unity Hub**
3. Click **Open Project** → select the cloned folder
4. Wait for Unity to import all assets
5. Open the `Assets/Scenes/GameScene.unity` scene
6. Press the ▶️ **Play** button

> Recommended Unity version: **2021.3 LTS or higher**

---

## 🗂️ Project Structure

```
endless-runner-game/
├── Assets/
│   ├── Scenes/         # Game scenes (Main Menu, Game, Game Over)
│   ├── Scripts/        # All C# game logic
│   ├── Prefabs/        # Player, obstacles, coins
│   ├── Animations/     # Animator controllers and clips
│   ├── Audio/          # Sound effects and music
│   ├── Sprites/        # 2D art assets
│   └── UI/             # Canvas, buttons, score display
├── ProjectSettings/    # Unity project configuration
├── Packages/           # Unity package dependencies
└── screenshots/        # 📸 Gameplay screenshots and recordings
```

---

## 🎯 Core Scripts

| Script | Purpose |
|---|---|
| `PlayerController.cs` | Jump input, animation, death detection |
| `ObstacleSpawner.cs` | Random obstacle generation and pooling |
| `GameManager.cs` | Game state, score, speed progression |
| `CoinCollector.cs` | Coin pickup and score update |
| `GroundScroller.cs` | Infinite background/ground scrolling |
| `UIManager.cs` | Score display, game over screen, restart |

---

## 👨‍💻 Author

**Muhammad Gulfam Asim**
BS Computer Science — Lahore Garrison University (2022–Present)

[![GitHub](https://img.shields.io/badge/GitHub-gulfamasim-181717?logo=github)](https://github.com/gulfamasim)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-gulfamasim402-0A66C2?logo=linkedin)](https://linkedin.com/in/gulfamasim402)
