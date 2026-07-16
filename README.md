## Core Features & Technical Highlights

This project is a vertical slice of a turn-based JRPG, built with a strong emphasis on scalable architecture, performance optimization, and seamless game feel. 

- **Data-Driven Combat System**
  Utilizes a robust data-driven approach via `ScriptableObject` to decouple data from logic. Character stats, skill configurations, and combat action behaviors can be easily authored and balanced directly within the Editor without touching the codebase.
- **Seamless Asynchronous Loading**
  Implements a non-blocking asynchronous scene transition system. It masks the heavy lifting of scene activation, memory allocation, and Garbage Collection (GC) behind a programmatic screen-space overlay fader, eliminating transform overheads and instantiation lag spikes.
- **Dynamic Cinematic Camera**
  Features a custom camera director system built on top of Cinemachine. It dynamically manipulates blend definitions during runtime to seamlessly switch between smooth tracking (Ease In/Out) for exploration and snappy cuts for high-impact combat actions.
- **Integrated Dialogue & State Machine**
  Combines a strict finite state machine (FSM) with a node-based visual scripting tool (Fungus). It handles exclusive player inputs, automated cinematic movements, and UI prompt suppressions dynamically based on the current game state (Exploration, Dialog, Combat, or Cutscene).
- **Environment Rendering Optimization**
  Includes a custom editor-time mesh baking utility. It programmatically traverses nested hierarchies to collapse and combine hundreds of environment meshes (e.g., grasses) into single contiguous chunks, drastically reducing CPU overhead and Draw Calls.
