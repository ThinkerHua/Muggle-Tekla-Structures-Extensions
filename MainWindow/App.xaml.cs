using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Muggle.TsExtensions.MainWindow.Services;
using Muggle.TsExtensions.MainWindow.ViewModels;
using Muggle.TsExtensions.MainWindow.Views;
using Tekla.Structures;
using Events = Tekla.Structures.Model.Events;
using Model = Tekla.Structures.Model.Model;
using TsDialog = Tekla.Structures.Dialog;

namespace Muggle.TsExtensions.MainWindow {
    public partial class App : Application {
        internal const string UserInterrupt = "User interrupt";
        internal const string NotConnected = "Not connected to a model.";

        private string _xsDataDir;
        private string _tsLanguage;
        private Model _model;
        private Events _events;

        private readonly ServiceCollection _servicesBuilder;

        internal TsDialog.Localization Localization { get; private set; }

        public static new App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }

        public App() {
            //  先配置MessageboxService，以便出现异常时可以弹出消息框
            _servicesBuilder = new ServiceCollection();
            _servicesBuilder.AddSingleton<IMessageBoxService, MessageBoxService>();

            Services = _servicesBuilder.BuildServiceProvider();
        }

        private IServiceProvider ConfigureServices() {
            _servicesBuilder.AddSingleton<INavigationService, NavigationService>();

            _servicesBuilder.AddSingleton<MainWindowViewModel>();
            _servicesBuilder.AddSingleton<Views.MainWindow>();

            _servicesBuilder.AddTransient<NormalToolsViewModel>();
            _servicesBuilder.AddTransient<NormalTools>();

            _servicesBuilder.AddTransient<SelectBooleansViewModel>();
            _servicesBuilder.AddTransient<SelectBooleans>();

            _servicesBuilder.AddTransient<ThreeDimensionalRotationViewModel>();
            _servicesBuilder.AddTransient<ThreeDimensionalRotation>();

            _servicesBuilder.AddTransient<PluginsViewModel>();
            _servicesBuilder.AddTransient<Plugins>();

            _servicesBuilder.AddTransient<MoveToElevationViewModel>();
            _servicesBuilder.AddTransient<MoveToElevation>();

            _servicesBuilder.AddTransient<ConnectionStatusFilterViewModel>();
            _servicesBuilder.AddTransient<ConnectionStatusFilter>();

            _servicesBuilder.AddTransient<ExtendBeamViewModel>();
            _servicesBuilder.AddTransient<ExtendBeam>();

            _servicesBuilder.AddTransient<ProjectBeamOntoPlaneViewModel>();
            _servicesBuilder.AddTransient<ProjectBeamOntoPlane>();

            return _servicesBuilder.BuildServiceProvider();
        }

        private void Application_Startup(object sender, StartupEventArgs e) {

            IMessageBoxService messageBoxService = null;

            try {
                messageBoxService = Services.GetRequiredService<IMessageBoxService>();

                _model = new Model();
                if (!_model.GetConnectionStatus()) throw new Exception(NotConnected);

                _events = new Events();
                _events.TeklaStructuresExit += ExitApp;
                _events.Register();
            } catch (Exception ex) {
                messageBoxService?.ShowError(ex.ToString());
                Shutdown();
                return;
            }

            try {
                TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref _tsLanguage);
                _tsLanguage = GetShortLanguage(_tsLanguage);

                TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref _xsDataDir);
#if D2021 || R2021
                var promptsAilFilePath = Path.Combine(_xsDataDir, @"messages\prompts.ail");
#elif D2024 || R2024
                var promptsAilFilePath = Path.Combine(_xsDataDir, @"bin\messages\prompts.ail");
#endif

                Localization = new TsDialog.Localization(promptsAilFilePath, _tsLanguage);
                Localization.LoadAilFile(promptsAilFilePath);
            } catch {
                Localization = new TsDialog.Localization();
            }

            Services = ConfigureServices();

            try {
                var mainWindow = Services.GetRequiredService<Views.MainWindow>();
                mainWindow.Show();
            } catch (Exception ex) {
                messageBoxService?.ShowError(ex.ToString());
                Shutdown();
            }
        }

        private void ExitApp() {
            Dispatcher.Invoke(Shutdown);
            // or do this
            /*new Thread(() => {
                Environment.Exit(0);
            }).Start();*/
        }

        private static string GetShortLanguage(string language) {
            return language switch {
                "ENGLISH" => "enu",
                "DUTCH" => "nld",
                "FRENCH" => "fra",
                "GERMAN" => "deu",
                "ITALIAN" => "ita",
                "SPANISH" => "esp",
                "JAPANESE" => "jpn",
                "CHINESE SIMPLIFIED" => "chs",
                "CHINESE TRADITIONAL" => "cht",
                "CZECH" => "csy",
                "PORTUGUESE BRAZILIAN" => "ptb",
                "HUNGARIAN" => "hun",
                "POLISH" => "plk",
                "RUSSIAN" => "rus",
                _ => "enu",
            };
        }
    }
}
