# Technical Documentation: Turn-Based JRPG Prototype

## 1. Architectural Overview & Technical Highlights

*   **Data-Driven Architecture:** Utilizes `ScriptableObject` classes (`UnitData`, `SkillData`, `CombatAction`) to define character statistics, skill behaviors, and action execution logic. This decouples data from system mechanics, allowing design balancing without code modification.
*   **Separation of Concerns (SRP):** Core systems are modularized into dedicated managers (`SessionManager`, `GameFlowManager`, `CombatManager`, `SceneTransitionManager`), ensuring high maintainability and preventing class bloating.
*   **Finite State Machine (FSM):** Implements a strict FSM via the `GameState` enumerator. This safely isolates execution contexts (Exploration, Dialog, Combat, Cutscene) to prevent input overlaps and state-driven logic bugs.
*   **Asynchronous Scene Management:** Handles scene transitions via background threading (`SceneManager.LoadSceneAsync`). It dynamically generates a screen-space fader and forces Garbage Collection (`System.GC.Collect()`) during the black screen phase to completely mask instantiation frame-drops.
*   **Cinematography Hijacking:** Dynamically overrides the `CinemachineBlendDefinition` at runtime. It enforces a 0-second *Cut* transition for high-impact combat animations and seamlessly restores *Ease In Out* blends for standard tracking.
*   **Editor-Time Mesh Optimization:** Employs a custom `MeshCombiner` utility to recursively bake deep hierarchies of environment quads (e.g., foliage) into singular static meshes. This eliminates massive Transform computation overhead and reduces Draw Calls.
*   **Narrative & State Integration:** Intertwines the Fungus visual scripting framework with the C# FSM, handling automated cutscene movements and prompt suppressions dynamically during narrative execution.

---

## 2. Core System Managers & Scripts

### 2.1 SessionManager.cs
**Responsibility:** Manages cross-scene data persistence, including player progression, runtime stats, and spatial world states.

| Core Variables / Methods | Technical Description |
| :--- | :--- |
| `playerUnitData`, `playerLevel` | Caches base RPG progression metrics and current runtime capacities. |
| `savedPlayerPosition` | Records the exact `Vector3` coordinates before transitioning to the battle instance. |
| `lastInteractedNpcID` | Caches the specific NPC identifier to resume interconnected narrative flow post-battle. |
| `GainExperience()` | Evaluates accumulated EXP and dynamically executes capacity scaling (Level Up). |

### 2.2 SceneTransitionManager.cs
**Responsibility:** Controls UI fading visuals and the asynchronous scene-loading pipeline.

| Core Methods | Technical Description |
| :--- | :--- |
| `CreateAutoFadeCanvas()` | Programmatically generates a persistent Screen-Space Overlay fader at runtime, removing the need for Editor prefabs. |
| `FadeAndLoadRoutine()` | Coroutine that executes visual interpolation, invokes memory cleanup (`Resources.UnloadUnusedAssets()`), and halts rendering visibility for an additional 0.25 seconds to hide Awake/Start processing spikes. |

### 2.3 GameFlowManager.cs
**Responsibility:** The primary FSM Director. Controls input polling allowances, camera states, and interaction listeners.

| Core Methods | Technical Description |
| :--- | :--- |
| `ChangeGameState()` | Modifies the active `GameState` and executes `ProcessStateChange()` to strictly lock/unlock `PlayerController` physics and UI prompt rendering. |
| `StartNPCInteraction()` | Registers target ID, transitions Cinemachine virtual cameras to dialog framings, and dispatches automated pathing commands. |
| `InitializeSceneState()` | Hooked to `Start()`. Evaluates `SessionManager` flags to detect post-battle returns, teleports the player to cached coordinates, and broadcasts Fungus flow triggers. |

### 2.4 CombatManager.cs
**Responsibility:** Orchestrates the turn-based logic loop, damage effect calculations, and dynamic camera direction.

| Core Methods | Technical Description |
| :--- | :--- |
| `InitializeDynamicCombatSequence()`| Reads injected `UnitData`, instantiates combatant prefabs, strips exploration components, and binds `CinemachineCamera` tracking targets. |
| `ExecuteTurnSequence()` | Processes the `CombatAction` pipeline, evaluating `EffectCategory` payloads to determine physical/magical damage resolutions and camera framing fallbacks. |
| `SwitchCamera()` | Injects dynamic `CinemachineBlendDefinition.Styles.Cut` parameters for 1 frame based on the `forceCut` flag, reverting to default via a coroutine sequence. |

### 2.5 PlayerController.cs
**Responsibility:** Handles Rigidbody-based physics navigation, trigger detection, and automated sequence pathing.

| Core Methods | Technical Description |
| :--- | :--- |
| `HandleInput()` | Polls axis inputs to apply continuous `Rigidbody.MovePosition` mapping and `Quaternion.Slerp` rotation smoothing. |
| `TriggerAutoMovement()` | Overrides user input to programmatically translate the `Transform` towards predetermined narrative marks via `Vector3.MoveTowards`. |
| `SetMovementState()` | An injected lockdown method that halts physics execution and aggressively clears `PromptVisibility` booleans on active triggers. |

### 2.6 MeshCombiner.cs (Utility)
**Responsibility:** An Editor-only script designed to compress rendering pipelines for heavily populated environment sectors.

| Optimization Execution Flow | Technical Description |
| :--- | :--- |
| **Recursive Extraction** | Crawls the local hierarchy to extract `MeshFilter` and matrix data from all nested child objects. |
| **32-bit Indexing** | Upgrades the `indexFormat` to `UInt32` to safely bypass the standard 65,535 vertex limitation during geometry merging. |
| **Combine & Clean** | Merges sub-meshes via `CombineMeshes()`, followed by a reverse `DestroyImmediate` loop to permanently erase obsolete child `Transform` components. |

---

## 3. System Interaction Flows

### 3.1 Asynchronous Loading Pipeline
1. System invokes `SceneTransitionManager.Instance.LoadSceneWithFade()`.
2. Fader UI interpolates alpha from `0.0` to `1.0` (Black Screen).
3. Memory cleanup is aggressively forced via `GC.Collect()`.
4. Target scene initiates background loading via `SceneManager.LoadSceneAsync`.
5. Upon 100% completion, the system yields for an explicit 0.25-second calibration window to hide subsequent `Awake()` and shader warm-up stutters.
6. Fader UI interpolates back to alpha `0.0`.

### 3.2 NPC Dialogue to Combat Transition
1. Player overlaps NPC `TriggerCollider` -> UI Prompt renders.
2. Input registered -> `PlayerController` defers to `GameFlowManager.StartNPCInteraction()`.
3. FSM alters state to `Cutscene` -> Inputs locked -> UI prompts aggressively suppressed.
4. Player executes `TriggerAutoMovement()` to align with the NPC Stand Mark.
5. Cinemachine transitions to wide framing -> Fungus Flowchart takes execution priority.
6. Dialogue sequence resolves -> Fungus Call Method invokes `TriggerNPCBattle()`.
7. `GameFlowManager` caches spatial coordinates (`Vector3`) and entity identifiers to `SessionManager`.
8. System executes Async Loading Pipeline towards the Battle instance.

### 3.3 Post-Battle Resolution & Narrative Resumption
1. Combat concludes -> System executes Async Loading Pipeline towards the Exploration instance.
2. `GameFlowManager` initializes -> Validates `isReturningFromBattle` flag within `SessionManager`.
3. Player is instantly teleported to the cached spatial coordinates.
4. FSM locks state to `Dialog` -> Locates the exact NPC instance via `lastInteractedNpcID`.
5. System fires `Flowchart.BroadcastFungusMessage("BattleEnded")`.
6. Fungus resumes the narrative sequence -> Fires `TriggerPostBattleCutscene()` upon completion.
7. `PlayableDirector` (Timeline) executes visual effects -> Player triggers automated walk-out logic.
8. FSM restores state to `Exploration` -> Systems return to standard operational polling.
