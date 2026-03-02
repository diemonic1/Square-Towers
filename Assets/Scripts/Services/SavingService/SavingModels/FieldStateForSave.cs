using System;
using System.Collections.Generic;

namespace Services.SavingService.SavingModels
{
    [Serializable]
    public class FieldStateForSave
    {
        public List<TowerForSave> Towers = new();
        
        public FieldStateForSave(List<TowerForSave> towers)
        {
            Towers = towers;
        }
    }
}