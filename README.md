# Math Learning Mini-Games

![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue.svg)
![DOTween](https://img.shields.io/badge/DOTween-Integrated-orange.svg)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-green.svg)

## Overview

A focused collection of three educational math mini-games designed to help students practice number recognition and comparison skills. Built with Unity and enhanced with modern UI animations, these games provide an engaging way to learn fundamental mathematical concepts through interactive gameplay.

## 🎮 Featured Mini-Games

### 1. What Number Do You Hear? 🔊

- Audio-based number recognition game
- Text-to-speech system reads numbers aloud
- Students input numbers using an interactive keyboard
- Beautiful animations for correct/incorrect answers

### 2. Compare Numbers ⚖️

- Practice number comparison skills
- Drag and drop comparison symbols (>, <, =)
- Progressive difficulty levels
- Instant visual feedback

### 3. Order Numbers 📊

- Arrange numbers in ascending or descending order
- Drag and drop number sorting
- Multiple difficulty levels
- Animated transitions between numbers

## 🛠️ Technologies Used

### Core

- **Unity 2022.3+**
- **C# Scripts**
- **Unity UI System**

### Animation & Effects

- **DOTween Pro** - For smooth UI animations and transitions
- **Epic Toon FX** - For particle effects and visual feedback
- **UI Kit Pro** - For modern UI elements

### Audio

- **Unity Audio System**
- **Text-to-Speech Integration**

### UI Frameworks

- **TextMeshPro** - For high-quality text rendering
- **UI Particle System** - For UI-based particle effects

## 📁 Project Structure

### Key Scripts

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs         # Main game orchestration
│   ├── AudioManager.cs        # Audio handling and TTS
│   └── SceneLoader.cs        # Dynamic scene loading
│
├── Games/
│   ├── NumberListening/
│   │   ├── AudioGame.cs
│   │   └── NumberReader.cs
│   ├── NumberComparison/
│   │   ├── ComparisonGame.cs
│   │   └── SymbolDragger.cs
│   └── NumberOrdering/
│       ├── OrderingGame.cs
│       └── NumberSorter.cs
│
└── UI/
    ├── UIManager.cs          # UI state management
    ├── AnimationController.cs # DOTween animations
    └── FeedbackSystem.cs     # Visual feedback
```

### Scenes

```
Assets/Scenes/
├── SplashScreen.unity      # Entry point
├── MainMenu.unity         # Game selection
├── ListeningGame.unity    # Number listening game
├── ComparisonGame.unity   # Number comparison game
└── OrderingGame.unity     # Number ordering game
```

## 🚀 Setup Instructions

1. **Project Setup**

   - Clone this repository
   - Open with Unity 2022.3 or higher

2. **Scene Configuration**

   - Open Build Settings (File > Build Settings)
   - Add all scenes to the build profile
   - Set `SplashScreen.unity` as Scene 0
   - Add other scenes in any order (they load dynamically)

3. **Running the Game**
   - Open `SplashScreen.unity`
   - Click Play in Unity Editor
   - For deployment, build to your target platform

> **Note**: Scene order after SplashScreen doesn't matter as the GameManager handles dynamic loading based on teacher configuration.

### Prerequisites

- Unity 2022.3+
- DOTween Pro (import from Asset Store)
- TextMeshPro package
- Basic Unity knowledge

### Required Asset Packages

- DOTween Pro
- Epic Toon FX
- UI Kit Pro - Casual Glossy
- TextMesh Pro

## 🔧 Technical Features

### Animation System

- Smooth UI transitions using DOTween
- Particle effects for feedback
- Custom animation sequences for each game type

### Audio System

- Text-to-speech integration for number pronunciation
- Sound effects for interactions
- Background music management

### UI System

- Responsive UI design
- Touch-friendly interface
- Dynamic scaling for different screen sizes

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add YourFeature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 🤔 Support

For support or questions, please open an issue in the repository.
