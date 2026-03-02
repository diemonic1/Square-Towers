using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Services.ConfigsService.ScriptableObjectsConfigs
{
    public class LocalizationConfig
    {
        private JObject _localizationData;
        
        public LocalizationConfig()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Configs/localizations");

            if (jsonFile == null)
            {
                throw new Exception("Localizations.json file not found in Resources/Configs");
            }

            try
            {
                _localizationData = JObject.Parse(jsonFile.text);
            }
            catch (System.Exception e)
            {
                throw new Exception($"Parsing error JSON: {e.Message}");
            }
        }

        public string GetLocalize(string key, string language)
        {
            try
            {
                return _localizationData[language][key].ToString();
            }
            catch (Exception e)
            {
                Debug.LogError($"Not found key {key} in language {language}");
                return key;
            }
        }
    }
}