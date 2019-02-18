using UnityEngine;

namespace Simuro5v5
{
    using Simuro5v5.Strategy;
    using Simuro5v5.EventSystem;

    class Watcher
    {
        public Watcher()
        {
        }

        public void RegisterHook()
        {
            Event.Register(Event.EventType1.StrategyBlueLoaded, delegate (object obj)
            {
                var strategy = obj as IStrategy;
                if (strategy != null)
                {
                    Const.DebugLog(string.Format("Blue {0} loaded {1}.", strategy.Description,
                        strategy.IsConnected() ? "succeed" : "failed"));
                }
            });
            Event.Register(Event.EventType1.StrategyYellowLoaded, delegate (object obj)
            {
                var strategy = obj as IStrategy;
                if (strategy != null)
                {
                    Const.DebugLog(string.Format("Yellow {0} loaded {1}.", strategy.Description, 
                        strategy.IsConnected() ? "succeed" : "failed"));
                }
            });
            Event.Register(Event.EventType1.MatchInfoUpdate, delegate (object _matchinfo)
            {
                var matchInfo = _matchinfo as MatchInfo;
                if (matchInfo != null)
                {
                    Const.DebugLog("GlobalMatchInfo updated: ball: " + matchInfo.Ball.pos.x);
                }
            });
            Event.Register(Event.EventType2.WheelSetUpdate, OnWheelUpdate);
            Event.Register(Event.EventType0.MatchStart, delegate ()
            {
                Debug.Log("Match started");
            });
            Event.Register(Event.EventType0.RoundPause, delegate ()
            {
                Debug.Log("Match paused");
            });
            Event.Register(Event.EventType0.RoundResume, delegate ()
            {
                Debug.Log("Match resumed");
            });
        }

        private void OnWheelUpdate(object obj1, object obj2)
        {
            WheelInfo blue = obj1 as WheelInfo;
            WheelInfo yellow = obj2 as WheelInfo;
            if (blue == null || yellow == null)
                return;
            string tmp = "";
            tmp += "Blue: ";
            for (int i = 0; i < 5; i++)
            {
                tmp += (blue.Wheels[i].left.ToString() + "," + blue.Wheels[i].left.ToString() + "    ");
            }
            tmp += ("\n");
            tmp += ("Yellow: ");
            for (int i = 0; i < 5; i++)
            {
                tmp += (yellow.Wheels[i].left.ToString() + "," + yellow.Wheels[i].left.ToString() + "    ");
            }
            tmp += ("\n");
            Const.DebugLog(tmp);
        }
    }
}
