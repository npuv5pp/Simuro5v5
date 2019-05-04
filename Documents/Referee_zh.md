#裁判文档

这个平台采用了全新的裁决方式，依靠自动裁判进行判决，并在平台下方输出裁决结果。

## 裁判输出

当在比赛过程中，如果出现犯规情况，平台下方会输出三个信息：

* Foul 犯规情况 : 指的是裁决结果，可能是PlcaeKick（开球），GoalKick（门球），PenaltyKcik（罚球），FreeKick（争球）。
* Action 执行方 : 指的是裁决结果的执行方，下一步是哪一方进行操作，可能是Bule或者Yellow。
* Reason 犯规的原因 : 具体解释了进攻方或者防守方犯规的原因。

## 裁决结果

平台中的裁判会对每拍进行判决，判决结果有以下五种情况 ：

* PlaceKick 开球
* GoalKick 门球
* PenaltyKick 罚球
* FreeKick 争球
* NormalMatch 正常比赛

下面对判决出以上五种情况并结合裁判输出进行举例解释说明。

### 1.PlaceKick 开球

有一方进球时，被进球方执行开球动作。

当黄方被进球时，平台下方输出为 : `Foul:PlaceKick , Yellow team is actor . Reason :  Be scored and PlaceKick again             `

### 2.GoalKick  门球

有三种情况会判为GoalKick 门球 ：
1. 进攻方撞击防守方守门员，防守方执行门球动作

   当黄方正在进攻时，黄方有球员撞击蓝方守门员，平台下方输出为 ： `Foul: GoalKick , Blue team is actor . Reason : Attacker hit the Goalie`

2. 进攻方有两个及以上球员在防守方小禁区内，防守方执行门球动作

   当黄方正在进攻时，黄方有两个及以上球员在蓝方小禁区内，平台下方输出为 : `Foul：GoalKick，Blue team is actor. Reason:Attacker have two robots in SmallState`

3. 进攻方有四个及以上球员在防守方大禁区内，防守方执行门球动作

   当黄方正在进攻时，黄方有四个及以上球员在蓝方大禁区内，平台下方输出为 : `Foul: GoalKick , Bule team is actor . Reason : Attacker have four robots in BigState`

###3.PenaltyKick 罚球

有两种情况会判为PenaltyKick 罚球 ：
1. 防守方有两个及以上球员在小禁区内，进攻方执行罚球动作

   当黄方正在进攻时，蓝方有两个及以上球员在己方小禁区内防守时，平台下方输出为 :`Foul : PenaltyKick , Yellow team is actor . Reason: Defenders have two robots in SmallState`

2. 防守方有四个及以上球员在大禁区内，进攻方执行罚球动作

   当黄方正在进攻时，蓝方有四个及以上球员在己方大禁区内方式时，平台下方输出为 ： `Foul : PenaltyKick , Yellow team is actor . Reason : Defenders have four robots in BigState` 

###4.FreeKick 争球

若球在十秒内静止或者球的速度小于5，裁判会判为争球。裁判把场地以中心分为四块区域：分为左上区域，左下区域，右上区域，右下区域，用于争球点的判断。
当球在左下区域内超过十秒速度都小于5，平台下方输出为 ： `Foul : FreeKick , Blue team is actor . Reason : LeftBot Standoff time longer than 10 seconds in game`

###5.NormalMtack 正常比赛

若在比赛过程中没有出现以上四种犯规情况，比赛会正常进行，直到出现下一次犯规。
