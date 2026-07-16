# Turn-Based JRPG Prototype

This repository contains a vertical slice of a 3D Turn-Based RPG, developed as a technical assessment. The project is engineered with a strong emphasis on scalable software architecture, high code readability, and a polished "game feel", fully fulfilling the core requirements of world exploration, interactive dialogues, automated cutscenes, and JRPG-style combat.

## Architecture & Design Patterns
The codebase is structured to meet industry standards, avoiding spaghetti code and ensuring long-term maintainability.
*   **Single Responsibility Principle (SRP):** Core systems are strictly decoupled. Dedicated managers handle specific domains independently (e.g., separating cross-scene data persistence from asynchronous loading logic).
*   **Singleton Pattern:** Implemented robust and safe Singletons for persistent global managers to control game flow and state without data overlapping across scenes.
*   **Finite State Machine (FSM):** A strict FSM governs the game states (Exploration, Dialog, Combat, Cutscene), seamlessly locking/unlocking player inputs and UI prompts to prevent logic bugs during transitions.
*   **Data-Driven Architecture:** Extensively utilizes **Scriptable Objects** to define character stats, skills, and combat actions. This allows game designers to tweak gameplay balancing directly in the Editor without altering the core scripts.

## Core Gameplay & Mechanics
*   **World Exploration:** Smooth WASD/Arrow key movement logic using physics-based Rigidbody and procedural rotation.
*   **Interactive Environment:** Robust trigger-based interaction system (Spacebar) to engage with NPCs and environmental objects.
*   **Turn-Based Combat:** A fully functional JRPG-style battle loop featuring dynamic instancing of Player and Enemy **Prefabs**, turn sequence calculations, and dynamic stat evaluation.
*   **Automated Cutscenes:** Engine-driven in-game sequences where the player avatar moves autonomously to predetermined marks during narrative events.

## Narrative Integration
*   **Fungus Framework:** Seamless integration of the **Fungus** visual scripting tool to drive the dialogue system. The FSM effectively communicates with Fungus flowcharts to trigger battles and post-battle narrative resolutions natively.

## Extra Polish & Game Feel (Bonus Implementations)
*   **Cinematography Hijacking:** A custom camera director built on Cinemachine that dynamically alters blend definitions at runtime—switching from smooth tracking during exploration to instant, high-impact cuts during combat strikes.
*   **Seamless Asynchronous Loading:** Scene transitions run entirely in the background. Combined with a programmatic screen-space overlay fader and strategic Garbage Collection (GC) forcing, it eliminates instantiation lag spikes, ensuring buttery-smooth transitions between the open world and battle scenes.
*   **Draw Call Optimization:** Features a custom editor-time mesh baking utility that collapses heavy nested hierarchies (e.g., hundreds of environment quads) into single static meshes, drastically reducing CPU Transform overhead and VRAM usage.

## Project Structure
Adheres strictly to an **Industry-Standard Folder Structure**, neatly categorizing Assets (Animations, Data, Materials, Models, Prefabs, Scenes, Scripts) to ensure maximum navigability and workspace cleanliness.
