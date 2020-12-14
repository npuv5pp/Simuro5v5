# 简介

![screenshot](https://raw.githubusercontent.com/npuv5pp/Simuro5v5/c9e93546d63fe9776d34817f0733e41fba437067/Documents/screenshot.png)

这个平台使用了全新的策略加载方式，但同时提供了兼容旧平台的DLL方式。旧平台DLL策略的包装器的可以在[这个仓库](https://github.com/npuv5pp/V5DLLAdapter)找到。

我们对旧平台DLL的接口函数做了一些小的修改，实例代码参见[这个仓库](https://github.com/npuv5pp/DLLStrategy)。

# 用法

## 策略加载

### 使用新的策略接口

新的策略加载使用C/S架构：策略作为策略服务器，平台作为策略客户端，二者通过网络通讯交流。因此新平台的策略不局限DLL的方式，只要能实现[策略协议](https://github.com/npuv5pp/V5RPC)规定的RPC接口，就可以作为策略加载至平台。我们强烈建议将旧的DLL策略过度到新的策略接口上。

### 使用DLL包装器

为了暂时兼容DLL加载方式，我们提供了一个[对DLL策略的包装程序](https://github.com/npuv5pp/V5DLLAdapter)。 你可以下载我们编译打包好的Release版本。

但是你的旧版本的策略仍然需要进行一定的更改，示例代码参见[这个仓库](https://github.com/npuv5pp/DLLStrategy)。

启动V5DLLAdapter.exe，然后点击浏览，加载你的策略，在端口处填入20000(blue)/20001(yellow)，然后点击启动。

## 启动平台

打开Simuro5v5.exe。在这里鼠标右键可以打开或关闭菜单，左键确认。依次进入Game -> Strategy，点击Begin按钮，等到动画播放，你的策略就加载成功了。如果你发现点击Begin后没有反应，也许是你的DLL出了问题，请确保所有的接口函数都已经实现。

比赛过程中，你可以通过<kbd>空格</kbd>键来暂停继续。

## 回放

比赛场景中，你可以随时通过右键或 <kbd>ESC</kbd>键呼出菜单。在任何时候，你都可以点击Replay按钮进入回放场景，从这里你可以看到你最近的一次比赛回放。

键盘操作如下：

| 键盘      | 功能                  |
|-----------|-----------------------|
| <kbd>↑</kbd> <kbd>↓</kbd>   | 调节播放速度          |
| <kbd>←</kbd> <kbd>→</kbd>   | 控制进度              |
| <kbd>空格</kbd>    | 暂停或继续            |
| <kbd>1</kbd> ~ <kbd>5</kbd> | 跟踪蓝方的1~5号机器人 |
| <kbd>6</kbd> ~ <kbd>0</kbd> | 跟踪黄方的1~5号机器人 |
| <kbd>-</kbd>       | 跟踪球                |
| <kbd>K</kbd>       | 向前的第一人称视角    |
| <kbd>L</kbd>       | 向后的第一人称视角    |
| <kbd>X</kbd>       | 切换为俯视图          |

你也可以通过鼠标控制，点击按钮控制播放进程，或者使用鼠标滑轮在屏幕下方滑动控制播放进度，鼠标滑轮在右边的速率下拉框上滑动可以调节速率。

可以通过左面的四个按钮配合鼠标控制回放镜头。直接点击机器人或球可以切换到跟踪视角。在跟踪视角下，用鼠标右键左右滑动可以旋转摄像机，滑动滑轮可以实现缩放。

可以通过右边的Export/Import按钮导入导出策略回放数据。

## 自动裁判

这个平台采用了全新的裁决方式，依靠自动裁判进行判决，并在平台下方输出裁决结果。

具体文档参见[裁判文档](https://github.com/npuv5pp/Simuro5v5/blob/master/Documents/Referee_zh.md)。

## 右攻假设

平台采用右攻假设，即：无论策略加载到哪一方，都可以认为自己在蓝方（场地右侧）。

## 从老平台迁移

如果你准备将策略从老平台迁移到新平台，可以参考[此文档](https://github.com/npuv5pp/Simuro5v5/blob/master/Documents/Diff.md)

## 平台坐标

![Image](https://github.com/ego-0102/Simuro5v5/raw/master/Documents/platform.png)

![Image](https://github.com/ego-0102/Simuro5v5/raw/master/Documents/platform1.png)

## 机器人方向

以黑角朝向方向为正方向

[V5RPC/README.zh-CN.md at master · npuv5pp/V5RPC (github.com)](https://github.com/npuv5pp/V5RPC/blob/master/README.zh-CN.md#robot)



## Credits
版权所有(C) 西北工业大学V5++团队。保留所有权利。
