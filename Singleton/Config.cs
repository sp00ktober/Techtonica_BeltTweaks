

using System.Collections.Generic;
using UnityEngine;

namespace Techntonica_BeltTweaks.Singleton
{
    internal class Config
    {
        private static Config instance { get; set; }
        public static Config INSTANCE
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }
        public const int RESETVALUE = -777;
        public int DesiredBeltHeight { get; set; }
        public bool ModificationIsLocked { get; set; }
        public bool Tweak { get; set; }
        private int _lastCountBelts;
        public int LastCountBelts
        {
            get
            {
                return _lastCountBelts;
            }
            set
            {
                if( _lastCountBelts != value )
                {
                    ModificationIsLocked = false;
                }
                _lastCountBelts = value;
            }
        }
        public Dictionary<int, int> DeltaCache { get; set; }
        private Config()
        {
            DesiredBeltHeight = RESETVALUE;
            LastCountBelts = RESETVALUE;
            Tweak = false;
            DeltaCache = new Dictionary<int, int>();
        }
        public void ResetBeltValues()
        {
            DesiredBeltHeight = RESETVALUE;
            LastCountBelts = RESETVALUE;

            if(DeltaCache != null && DeltaCache.Count > 0)
            {
                DeltaCache.Clear();
            }
            DeltaCache = new Dictionary<int, int>();
        }
        public void ClearCacheUntil(int limit)
        {
            List<int> toDelete = new List<int>();
            foreach(KeyValuePair<int, int> kv in DeltaCache)
            {
                if(kv.Key >= limit)
                {
                    toDelete.Add(kv.Key);
                }
            }
            foreach(int key in toDelete)
            {
                DeltaCache.Remove(key);
            }
        }
    }
}
