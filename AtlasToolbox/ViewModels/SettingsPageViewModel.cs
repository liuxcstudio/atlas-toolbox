using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AtlasToolbox.Models;
using AtlasToolbox.Utils;
using AtlasToolbox.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace AtlasToolbox.ViewModels
{
    public partial class SettingsPageViewModel : INotifyPropertyChanged
    {
        public Language _currentLanguage { get; set; }
        public Language CurrentLanguage 
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                OnPropertyChanged(); // Notifies UI
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            RegistryHelper.SetValue(@"HKLM\SOFTWARE\AtlasOS\Services\Toolbox", "lang", this.CurrentLanguage.Key);
            // 清除语言缓存，确保下次加载新语言
            App.ClearLangCache();
            App.LoadLangString();
            MainWindow mWindows = App.m_window as MainWindow;
        }

        public ObservableCollection<Language> Languages { get; set; }
        private static readonly Dictionary<string, string> _languageMapping = new()
        {
            ["zh-tw"] = "zh_tw",
            ["zh-hk"] = "zh_tw",
            ["zh-mo"] = "zh_tw",
            ["zh-cn"] = "zh_cn",
            ["zh-sg"] = "zh_cn",
            ["es"] = "es_es",
            ["es-es"] = "es_es",
            ["fr"] = "fr_fr",
            ["fr-fr"] = "fr_fr",
            ["pt"] = "pt_pt",
            ["pt-pt"] = "pt_pt",
            ["pt-br"] = "pt_br",
            ["ru"] = "ru_ru",
            ["ru-ru"] = "ru_ru",
            ["sv"] = "sv_se",
            ["sv-se"] = "sv_se"
        };

        public SettingsPageViewModel()
        {
            Languages = new();
            Dictionary<string, string> langs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@$"lang\index.json"));
            foreach (KeyValuePair<string, string> language in langs)
            {
                Languages.Add(new (language.Value, language.Key));
            }
            string lang = (string)RegistryHelper.GetValue(@"HKLM\SOFTWARE\AtlasOS\Services\Toolbox", "lang");
            
            // 如果没有手动选择过语言，则根据系统语言自动选择
            if (string.IsNullOrEmpty(lang))
            {
                lang = GetSystemLanguage();
            }
            
            CurrentLanguage = Languages.FirstOrDefault(item => item.Key == lang);
        }

        private string GetSystemLanguage()
        {
            string systemLang = System.Globalization.CultureInfo.CurrentCulture.Name.ToLower();
            
            // 首先尝试精确匹配
            if (_languageMapping.TryGetValue(systemLang, out string mappedLang))
            {
                return mappedLang;
            }
            
            // 尝试前缀匹配
            foreach (var mapping in _languageMapping)
            {
                if (systemLang.StartsWith(mapping.Key))
                {
                    return mapping.Value;
                }
            }
            
            // 检查中文变体
            if (systemLang.StartsWith("zh-"))
            {
                return Languages.Any(l => l.Key == "zh_tw") ? "zh_tw" : "zh_cn";
            }
            
            // 检查葡萄牙语变体
            if (systemLang.StartsWith("pt-"))
            {
                return systemLang.Contains("br") || systemLang.Contains("brazil") ? "pt_br" : "pt_pt";
            }
            
            // 默认英文
            return "en_us";
        }

        public bool CheckUpdates()
        {
            if (ToolboxUpdateHelper.CheckUpdates())
            {
                App.ContentDialogCaller("newUpdate");
                return false;
            }else
            {
                return true;
            }
        }
    }
}
