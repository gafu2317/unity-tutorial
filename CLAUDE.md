# CLAUDE.md

回答は日本語でお願いします

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpellCraft Dungeon is a 2D top-down action game where players create custom magic spells and explore infinite randomly-generated dungeons. Built in Unity 6000.1.15f1.

## Unity Project Structure

- `Assets/Scripts/Data/Character.cs` - Core character data class with stats, equipment, and magic system
- `Assets/Scripts/Managers/GameManager.cs` - Singleton game manager handling player state, XP, and scene management  
- `Assets/Scripts/Managers/SceneController.cs` - Simple scene transition controller
- `Assets/Scenes/` - Game scenes (Title, Main, Dungeon, SpellCraft, Shop, etc.)
- `docs/` - Japanese design documentation and game specifications

## Core Game Architecture

### Character System
The Character class (`Character.cs:10`) manages:
- Player stats (HP, MP, level, experience)
- Equipment system with 4 slots (staff, robe, ring, brooch)
- Magic proficiency system with logarithmic growth
- Equipment bonuses affecting damage reduction, MP regen, range, and principle-specific damage

### Magic System
Magic spells are composed of:
- **Principles** (`PrincipleType:314`) - ThermalControl, KineticControl, StructuralControl, ElectromagneticControl
- **Forms** (`FormType:342`) - Sphere, Spear, Wall, Explosion, Chain
- **Effects** (`EffectType:376`) - Damage, Heal, Movement, ApplyBuff, ApplyDebuff, Generate

Magic proficiency grows through usage with logarithmic scaling (`Magic.cs:257-283`).

### Game Manager
GameManager (`GameManager.cs:9`) uses singleton pattern and handles:
- Player character persistence across scenes
- XP and level progression with exponential scaling
- MP auto-regeneration
- Scene transitions via coroutines

## Development Commands

Since this is a Unity project, use Unity Editor for most operations:
- **Build**: Use Unity Editor Build Settings
- **Test**: Play mode in Unity Editor
- **Debug**: Unity Console and Visual Studio/Rider integration

No package manager scripts (npm/yarn) or CLI build tools are used.

## Key Design Principles

- **Realistic magic mechanics**: Complex spells cost more MP, proficiency reduces costs
- **Equipment as tools**: Each equipment type serves specific magical enhancement purposes
- **Infinite progression**: Logarithmic growth curves prevent power creep while maintaining progression
- **Modular magic**: Spell components combine for emergent gameplay

## Scene Organization

Game flow: Title → Main (hub) → Dungeon/SpellCraft/Shop → GameOver/Clear
All scenes use the persistent GameManager for state management.

## Japanese Documentation

Comprehensive game design documents exist in `docs/` directory in Japanese, covering:
- Game concept and systems (`docs/ゲーム概要.md`, `docs/ゲームシステム.md`)
- Complete technical specification (`docs/00設計書.md`)
- Magic system deep-dive (`docs/魔法システム.md`)
- Equipment and progression systems

## Current Implementation Status

The project has basic character and magic data structures implemented. Core game loop, dungeon generation, and UI systems are not yet implemented.