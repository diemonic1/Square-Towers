using System;
using HelpfulScripts;
using ModestTree;
using UnityEngine;
using Zenject;

namespace Services.LocalizationService
{
    public class LocalizationService : ILocalizationService
    {
        public GlobalConstants.ELanguages CurrentLanguage => _currentLanguage;
        public bool IsChangeLanguageDisable  { get; private set; }
        
        private GlobalConstants.ELanguages _currentLanguage = GlobalConstants.ELanguages.Russian;
        
        [Inject] private ConfigsService.ConfigsService _configsService;
        
        public event Action OnLanguageChanged;
        public void ChangeLanguage(GlobalConstants.ELanguages language)
        {
            _currentLanguage = language;
            
            PlayerPrefs.SetString("language", language.ToString());

            OnLanguageChanged?.Invoke();
        }
        
        public string Localize(string key)
        {
            if (key == null)
            {
                Debug.LogError("Localization key is null");
                return "";
            }
            
            if (key.IsEmpty())
            {
                Debug.LogError("Localization key is empty");
                return "";
            }
            
            return _configsService.LocalizationConfig.GetLocalize(key, _currentLanguage.ToString());
        }
        
        public string LocalizeByLanguage(string key, GlobalConstants.ELanguages language)
        {
            if (key == null)
            {
                Debug.LogError("Localization key is null");
                return "";
            }
            
            if (key.IsEmpty())
            {
                Debug.LogError("Localization key is empty");
                return "";
            }

            if (string.IsNullOrEmpty(language.ToString()))
            {
                Debug.LogError("language key is empty");
                return "";
            }

            return _configsService.LocalizationConfig.GetLocalize(key, _currentLanguage.ToString());
        }
    }
}