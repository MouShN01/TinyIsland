using UnityEngine;

namespace TinyIsland.Core
{
    public enum GameState
    {
        None = 0,
        DayPreparation = 1,
        LowTide = 2,
        MiniTideWarning = 3,
        MiniTide = 4,
        NightTideWarning = 5,
        NightTide = 6,
        Climbing = 7,
        DayComplete = 8,
        GameWon = 9,
        GameLost = 10
    }
}