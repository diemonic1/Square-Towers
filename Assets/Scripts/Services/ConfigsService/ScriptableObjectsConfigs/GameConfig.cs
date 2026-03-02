using System;
using System.Collections.Generic;
using System.Drawing;

namespace Services.ConfigsService.ScriptableObjectsConfigs
{
    [Serializable]
    public class GameConfig
    {
        public int HoldingTimeToGetSquareFromScrollInMs;
        
        public String[] SquareColorsHex;
    }
}