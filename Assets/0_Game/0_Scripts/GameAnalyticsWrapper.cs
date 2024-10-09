using System;
using System.Collections.Generic;
using System.Linq;
using GameAnalyticsSDK;
using UnityEngine;

namespace SCR.SDK.Wrappers
{
    public class GameAnalyticsWrapper : MonoBehaviour, IGameAnalyticsATTListener
    {
        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                GameAnalytics.RequestTrackingAuthorization(this);
            }
            else
            {
                GameAnalytics.Initialize();
            }
        }

        public void GameAnalyticsATTListenerNotDetermined()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerRestricted()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerDenied()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerAuthorized()
        {
            GameAnalytics.Initialize();
        }

        public static void DesignEvent(string name, float eventValue)
        {
            GameAnalytics.NewDesignEvent(name, eventValue);
        }

        public static void ProgressionLvl(GAProgressionStatus progressionStatus, int lvl)
        {
            GameAnalytics.NewProgressionEvent(progressionStatus, "lvl_", lvl);
        }
    }
}