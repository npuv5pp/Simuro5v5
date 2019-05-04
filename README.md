[中文文档](https://github.com/npuv5pp/Simuro5v5/blob/master/README_ZH.md)

# Introduction
This platform uses a new strategy loading method, but also provides a DLL way compatible with the old platform. The wrapper for the old platform DLL strategy can be found in [this repository](https://github.com/npuv5pp/V5DLLAdapter).

We made some modifications to the interface functions of the old platform DLL. See the [this repository](https://github.com/npuv5pp/DLLStrategy) for example code.

# Usage

## Load Strategy

### Using the new strategy interface

The new strategy load uses the C/S architecture: the strategy acts as a strategy server and the platform acts as a strategy client, communicating through the network. Therefore, the strategy of the new platform is not limited to the DLL. As long as the RPC interface specified by [strategy Protocol](https://github.com/npuv5pp/V5RPC) can be implemented, it can be loaded as a strategy to the platform. We strongly recommend over-provisioning the old DLL strategy to the new strategy interface.

### Using DLL wrapper

In order to be temporarily compatible with the DLL loading method, we provide a [wrapper for the DLL strategy](https://github.com/npuv5pp/V5DLLAdapter). You can download our compiled and packaged Release version.

However, your old version of the strategy still needs to make some changes. See the [this repository](https://github.com/npuv5pp/DLLStrategy) for sample code.

Start V5DLLAdapter.exe, then click Browse, load your strategy, fill in the port with 20000 (blue) / 20001 (yellow), and then click Start.

## Run

Open Simuro5v5.exe. Here the right mouse button can open or close the menu, left button to confirm. Go to Game -> Strategy, click the Begin button, and wait until the animation plays, your strategy will load successfully. If you find that there is no response after clicking Begin, maybe your DLL is having problems, please make sure all interface functions are implemented.

During the game, you can use the space bar to pause.

## Replay

In the game scene, you can call the menu at any time by right-clicking or by pressing `ESC`. At any time, you can click the Replay button to enter the playback scene, where you can see your most recent game playback.

You can control replay by keyboard:

| Key       | Function                             |
|-----------|--------------------------------------|
| `↑` `↓`   | Adjust playback speed                |
| `←` `→`   | Control progress                     |
| `SEC`     | Pause or continue                    |
| `1` ~ `5` | Track the blue side robots No. 1~5   |
| `6` ~ `0` | Track the yellow side robots No. 1~5 |
| `-`       | Track the ball                       |
| `K`       | Forward first person perspective     |
| `L`       | Backward first person perspective    |
| `X`       | Switch to top view                   |

You can also control the playback process by mouse control, click the button, or use the mouse wheel to slide the bottom of the screen to control the playback progress. The mouse wheel slides on the rate drop-down box on the right to adjust the rate.

The playback lens can be controlled with the four buttons on the left in conjunction with the mouse. In tracking mode, use the right mouse button to rotate the camera, and slide the mouse wheel to zoom.

The strategy playback data can be exported and imported via the Export/Import button on the right.

# Automatic Referee
This platform adopts a new ruling method, relies on automatic referee judgment, and outputs the ruling result below the platform.

For specific documents, please refer to [referee documents](https://github.com/npuv5pp/Simuro5v5/blob/master/Documents/Referee.md)

# Credits

Copyright (C) Northwestern Polytechnical University V5++ team. all rights reserved.
