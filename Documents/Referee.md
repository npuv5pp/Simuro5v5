# Referee Document

This platform adopts a new ruling method, relies on automatic referee judgment, and outputs the ruling result below the platform.

## Referee Output

When a foul situation occurs during the game, three messages are output below the platform:

- Foul : Refers to the result of the ruling, which may be PlcaeKick , GoalKick , PenaltyKcik , FreeKick .
- Action : Refers to the executor of the ruling result. Which next step is the operation, which may be Bule or Yellow.
- Reason : `Explain specifically the reasons for the offense or defensive foul.`

## Judgment Result

The referee in the platform will judge each beat, and the judgment results have the following five situations:

- PlaceKick. 
- GoalKick. 
- PenaltyKick. 
- FreeKick. 
- NormalMatch. 

The following is an explanation of the above five cases and combined with the referee output.

### 1.PlaceKick 

When one of the goals is scored, the scored party performs the kick-off action.

When Yellow is scored, the output below the platform is : ` Foul: PlaceKick, Yellow team is actor . Reason : Be scored and PlaceKick again`

### 2.GoalKick 

There are three situations that will be judged as GoalKick goal:

1. The attacker hits the defensive side goalkeeper and the defensive side performs the goal kick.
   When Yellow is attacking, Yellow has a player who hits the blue goalkeeper. The output below the platform is : `Foul: GoalKick, Blue team is actor . Reason : Attacker hit the Goalie`
2. The offensive side has two or more players in the defensive side of the restricted area, the defensive side performs the goal kick
   When Yellow is attacking, Huang has two or more players in the blue restricted area. The output below the platform is : `Foul: GoalKick, Blue team is actor. Reason: Attacker have two robots in SmallState`
3. The offensive side has four or more players in the defensive side of the penalty area, the defensive side performs the goal kick
   When Yellow is attacking, Huang has four or more players in the blue restricted area. The output below the platform is : `Foul: GoalKick, Bule team is actor . Reason : Attacker have four robots in BigState`

### 3.PenaltyKick 

There are two situations that will result in PenaltyKick free throws :

1. The defender has two or more players in the small restricted area, and the offensive player performs the free throw action.
   When Yellow is attacking, when the blue side has two or more players defending in their own restricted area, the output below the platform is :` Foul: PenaltyKick, Yellow team is actor . Reason: Defenders have two robots in SmallState`
2. The defender has four or more players in the restricted area, and the offensive player performs free throws.
   When Yellow is attacking, when the blue side has four or more players in their own restricted area, the output below the platform is : `Foul : PenaltyKick , Yellow team is actor . Reason : Defenders have four robots in BigState`

### 4.FreeKick 

If the ball is stationary within ten seconds or the speed of the ball is less than 5, the referee will judge the ball. The referee divides the venue into four areas in the center: it is divided into the upper left area, the lower left area, the upper right area, and the lower right area, which are used for judging the point of the ball.

When the ball is less than 5 in the lower left area for more than ten seconds, the output below the platform is : `Foul : FreeKick , Blue team is actor . Reason : LeftBot Standoff time longer than 10 seconds in game`

### 5.NormalMtack 

If the above four fouls do not occur during the game, the game will proceed normally until the next foul occurs.