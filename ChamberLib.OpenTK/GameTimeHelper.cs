﻿using System;

namespace ChamberLib.OpenTK
{
    public static class GameTimeHelper
    {
        public static GameTime ToChamber(this double time)
        {
            var t = TimeSpan.FromSeconds(time);
            return new GameTime(t, t);
        }
    }
}

