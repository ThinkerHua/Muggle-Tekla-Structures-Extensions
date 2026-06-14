using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Tekla.Structures;

namespace Muggle.TsExtensions.Common.WPF.Localization;

public class TranslationService : INotifyPropertyChanged {
    private static TranslationService _instance;
    private readonly Tekla.Structures.Dialog.Localization _localization;

    public static TranslationService Instance => _instance ??= new TranslationService();

    private TranslationService() {
        _localization = new Tekla.Structures.Dialog.Localization();
        
        var xsDataDir = string.Empty;
        TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);
        var path = Path.Combine(xsDataDir, "environments\\common\\extensions\\messages\\Muggle.TsExtensions\\Common.WPF.ail");
        LoadAilFile(path);
    }

    public string this[string key] {
        get {
            var translation = _localization.GetText(key);
            return string.IsNullOrEmpty(translation) ? key : translation;
        }
    }

    public void LoadAilFile(string path) {
        _localization.LoadAilFile(path);
    }

    public void ChangeLanguage(string languageCode) {
        _localization.Language = languageCode;
        OnPropertyChanged(string.Empty);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}