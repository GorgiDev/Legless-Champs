using System.Collections.Generic;
using UnityEngine;

public class Tags : MonoBehaviour
{
    public static class Enemies
    {
        public const string Turret = "Turret";
        public const string DroneScrambler = "DroneScrambler";
        public const string Router = "Router";

        private static readonly HashSet<string> All = new()
        {
            Turret, DroneScrambler, Router
        };
        
        public static bool isEnemy(string tag) => All.Contains(tag);
    }

    public static class Friendlies
    {
        public const string Player = "Player";

        private static readonly HashSet<string> All = new()
        {
            Player
        };

        public static bool isFriendly(string tag) => All.Contains(tag);
    }

    public static class Other
    {
        public const string BreakableBox = "BreakableBox";
        public const string ExplosiveBarrel = "ExplosiveBarrel";
        
        private static readonly HashSet<string> All = new()
        {
            BreakableBox, ExplosiveBarrel
        };
    }
}