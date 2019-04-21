# Simuro5v5
Simuro5v5 platform.

## Usage
This platform uses a new strategy loading method, but also provides a DLL way compatible with the old platform. The source code for the wrapper for the old platform DLL policy can be found in [this repository](https://github.com/npuv5pp/StrategyServer).

We made some minor modifications to the interface functions of the old platform DLL. See [the repository](https://github.com/npuv5pp/demo_strategydll) for example code.

You can download our compiled and packaged Release version. Compile your strategy into a 32-bit DLL and put it in the root directory. 

Then open Simuro5v5.exe. Here the right mouse button can open or close the menu, left button to confirm. Go to Game -> Strategy, enter your DLL name, click the Begin button, and wait until the animation plays, your strategy will load successfully. If you find that there is no response after clicking Begin, maybe your DLL has a problem, please make sure that all interface functions have been implemented. For the specific interface functions, see the above repository.

## Replay
In the game scene, you can right-click the menu. At any time, you can click on the Replay button to enter the playback scene, from here you can see your most recent game playback.

You can control the progress through the keyboard or mouse. The up and down direction keys of the keyboard can adjust the playback speed, the left and right direction keys control the progress, and the space bar pauses or continues. You can also control it with your mouse. Click the button to control the playback process, or use the mouse wheel to slide the bottom of the screen to control the playback progress. The mouse wheel slides on the rate drop-down box on the right to adjust the rate.

## Credits
(C) NWPU V5++ Team. ALL RIGHTS RESERVED.
