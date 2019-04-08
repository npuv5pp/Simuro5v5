# Simuro5v5
Simuro5v5 platform.

## Usage
This platform uses a new strategy loading method, but also provides a DLL way compatible with the old platform. The source code for the wrapper for the old platform DLL policy can be found in [this repository](https://github.com/npuv5pp/StrategyServer).

We made some minor modifications to the interface functions of the old platform DLL. See [the repository](https://github.com/npuv5pp/demo_strategydll) for example code.

You can download our compiled and packaged Release version. Compile your strategy into a 32-bit DLL and put it in the root directory. Open Simuro5v5.exe, go to Game -> Strategy, enter your DLL name, click the Load button, and wait until the animation plays, your strategy will load successfully.

## Replay
In the game scene, you can right-click the menu. At any time, you can click on the Replay button to enter the playback scene, from here you can see your most recent game playback.
