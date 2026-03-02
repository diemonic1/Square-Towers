using Services.SavingService.SavingModels;
using UnityEngine;

namespace Services.SavingService
{
    public class SavingService : ISavingService
    {
        private const string SaveKey = "field_state";

        public void Save(FieldStateForSave data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public FieldStateForSave Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return null;

            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<FieldStateForSave>(json);
        }
    }
}