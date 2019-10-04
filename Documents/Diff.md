## 新老平台的差异

### 坐标系统

在新平台中，坐标系统的原点位于场地的中心，场地的右上角为(110, 90)点，场地各个部分的比例，如大小禁区、角落的比例保持不变。

### 接口

新平台的策略-平台通讯使用网络通讯，并不限制策略的编写方式，但是为了方便大家继续使用DLL形式的策略，我们提供了`V5DLLAdapter`，也就是DLL适配器，它帮助DLL实现了网络通讯部分，DLL只需要实现几个类似老平台的接口函数，就可以接入到平台。但是由于新老平台的接口与功能不太一样，因此，需要DLL实现另外一套接口函数，详细说明参见[DLLStrategy](https://github.com/npuv5pp/DLLStrategy/blob/master/README_zh.md)，其中结构体的定义参见[platform.h](<https://github.com/npuv5pp/DLLStrategy/blob/master/DLLStrategy/platform.h>)。

新的接口函数包括：

- `void GetTeamInfo(TeamInfo* teaminfo)`

  用于指定策略信息，目前包含队名字段。

  参数`TeamInfo* teaminfo`**需要策略填充自身的信息**，会返回给平台。

- `void GetInstruction(Field* field)`

  比赛中的每拍被调用，**需要策略指定轮速**，相当于旧接口的Strategy。

  参数`Field* field`为`In/Out`参数，存储当前赛场信息，并允许策略修改己方轮速。

- `void GetPlacement(Field* field)`

  每次自动摆位时被调用，**需要策略指定摆位信息**。

  参数`Field* field`为`In/Out`参数，存储当前赛场信息，并允许策略修改己方位置（和球的位置）。

- `void OnEvent(EventType type, void* argument)`

  事件发生时被调用。

  参数`EventType type`表示事件类型；

  参数`void* argument`表示该事件的参数，如果不含参数，则为NULL。

以下对一些接口细节进行说明：

### 事件

新平台中，不再使用`Environment`结构体存放所有的比赛状态与数据，而是将比赛中的`犯规`、`比赛开始`、`比赛结束`、`切换半场`等相对稳定的状态信息通过事件来通知，每当这些状态发生改变时，DLL中的`OnEvent`函数就会被调用，函数签名为`void OnEvent(EventType type, void* argument)`，通过第一个`type`参数指定事件类型，如果某个事件拥有参数，则通过第二个参数传递。事件及相应参数定义可参考[事件定义](<https://github.com/npuv5pp/V5RPC/blob/master/README.zh-CN.md#%E4%BA%8B%E4%BB%B6%E5%AE%9A%E4%B9%89>)。

#### 阶段切换事件

阶段切换事件及其描述：

| 事件                 | 值   | 描述         |
| -------------------- | ---- | ------------ |
| MatchStart           | 1    | 比赛开始     |
| MatchStop            | 2    | 比赛结束     |
| FirstHalfStart       | 3    | 上半场开始   |
| SecondHalfStart      | 4    | 下半场开始   |
| OvertimeStart        | 5    | 加时赛开始   |
| PenaltyShootoutStart | 6    | 点球大战开始 |


#### 摆位事件

`比赛的摆位信息`通过事件函数来通知。当开球或者有一方犯规时，双方策略就会收到`type`为`JudgeResult`的事件，紧接着会触发摆位(`GetPlacement`函数)。事件拥有参数，DLL中结构体定义为：

```c
struct JudgeResultEvent {
	JudgeType type;
	Team actor;
	wchar_t reason[MAX_STRING_LEN];
};
```

其中`actor`字段指定本次摆位的进攻方，相当于老平台的`Whosball`；`reason`为字符串，描述本次犯规的原因，由平台内的自动裁判发送；`type`字段指定摆位类型，与老平台的`GameState`的对应关系为：

| 新平台           |      | 老平台      |      |
| ---------------- | ---- | ----------- | ---- |
| 类型             | 值   | 类型        | 值   |
| PlaceKick        | 0    | PlaceKick   | 2    |
| GoalKick         | 1    | GoalKick    | 5    |
| PenaltyKick      | 2    | PenaltyKick | 3    |
| FreeKickRightTop | 3    | FreeBall    | 1    |
| FreeKickRightBot | 4    | FreeBall    | 1    |
| FreeKickLeftTop  | 5    | FreeBall    | 1    |
| FreeKickLeftBot  | 6    | FreeBall    | 1    |

新平台中并不包含任意球，因此没有对应与老平台的`FreeKick`类型的摆位。

### 动作

机器人的动作由`GetInstruction`函数指定。比赛运行中的每一拍策略的`GetInstruction`函数都会被调用，传入的参数为`Field`指针，描述了场地中机器人的位置、方向，球的位置等信息，同时，策略需要修改其中的轮速字段以告知平台每个己方机器人的动作。

### 摆位

摆位事件的类型信息由`JudgeResult`事件指出，而实际的摆位动作由`GetPlacement`函数完成。`JudgeResult`事件触发之后，`GetPlacement`函数就会被调用，传入参数为`Field`指针，策略需要更改其中的位置信息来完成摆位。**策略返回的摆位如果不合法，会被自动裁判纠正**。

- 摆位的先后

  门球（GoalKick）和开球（PlaceKick）由进攻方（Actor）先摆位，其他情况由防守方先摆位

- 球的摆位

  门球由进攻方（Actor）摆球，其他情况球固定

- `Field`中的已有信息

  对应先摆的一方，`Field`中的已有数据无意义；对于后摆的一方，`Field`中的敌方数据为先摆的一方的摆位信息（经过自动裁判的修正）。

## 修改建议

由于不清楚大家的策略代码是如何架构的，因此我们无法提供一个完善的兼容层代码。在修改接口时可以参考以下建议：

- 动作接口改动看似较多，实际上只是将之前的`Environment`结构体拆开。因此在之前DLL的基础上改动的话，可以用传入的`Field`与事件函数描述的状态，拼接一个`Environment`结构体，同时做坐标转换，传递给老接口的`Strategy`函数；`Strategy`函数返回之后，再将轮速重新写入`Field`中；
- 自动摆位由于是新功能，因此可能需要实现新的摆位函数；
- `GetTeamInfo`接口目前可以从策略中获取队名，实现之后就可以在平台中看到队伍名称；
- 如果对于接口实现方式有任何疑惑的地方，可以询问我们，或者参考[DLL模板](<https://github.com/npuv5pp/DLLStrategy/>)，