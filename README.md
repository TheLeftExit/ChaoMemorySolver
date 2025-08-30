# Chao Memory solver

This is a result of a spontaneous one-day coding marathon. I'm running Sonic Adventure and Sonic Advance series games in emulators (with GC-GBA link cable emulation), and I need a way to earn rings for the Black Market without grinding or cheating.

This tool automates the interacts with the [Chao Memory](https://chao-island.com/guides/spin-off-games/tiny-chao-garden/mini-games/chao-memory) game in the Tiny Chao Garden. It continuously takes screenshots of the [mGBA](https://mgba.io/) window, calculates most likely positions of different cards, tracks them while they're shuffled, and then simulates an input sequence based on the final card map. There's also a debug picture box that shows the calculated item map in real time, which looks cool.

This probably doesn't feel like cheating because I spent a day coding this, so it's a different kind of grinding effort or whatever.

---

Constraints (which I may or may not fix later):
- Once the loop starts, the only way to stop is to exit the tool,
- Hardcoded input keys (arrow keys, X key as A button),
- The input sequence uses tight timings and doesn't use screen capture, so desync is likely if the emulator stutters,
- mGBA must be scaled at 1x, not resized, and must remain fully visible on-screen (full screen capture is much easier than capturing a specific window),
- ...and much more!