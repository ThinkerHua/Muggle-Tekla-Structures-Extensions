using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Muggle.TsExtensions.MainWindow.Services;
using Muggle.TsExtensions.MainWindow.ViewModels;
using Tekla.Structures;
using Events = Tekla.Structures.Model.Events;
using Model = Tekla.Structures.Model.Model;
using TSDialog = Tekla.Structures.Dialog;

namespace Muggle.TsExtensions.MainWindow {
    public partial class App : Application {
        internal const string UserInterrupt = "User interrupt";
        internal const string NotConnected = "Not connected to a model.";

        private string _xsDataDir;
        private Model _model;
        private Events _events;

        private readonly ServiceCollection _servicesBuilder;

        internal TSDialog.Localization Localization { get; private set; }

        public static new App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }

        public App() {
            //  先配置MessageboxService，以便出现异常时可以弹出消息框
            _servicesBuilder = new ServiceCollection();
            _servicesBuilder.AddSingleton<IMessageBoxService, MessageBoxService>();

            Services = _servicesBuilder.BuildServiceProvider();
        }

        private IServiceProvider ConfigureServices() {
            _servicesBuilder.AddSingleton<IMessageBoxService, MessageBoxService>();
            _servicesBuilder.AddSingleton<INavigationService, NavigationService>();

            _servicesBuilder.AddSingleton<MainWindowViewModel>();
            _servicesBuilder.AddSingleton<Views.MainWindow>();

            _servicesBuilder.AddTransient<NormalToolsViewModel>();
            _servicesBuilder.AddTransient<Views.NormalTools>();

            _servicesBuilder.AddTransient<SelectBooleansViewModel>();
            _servicesBuilder.AddTransient<Views.SelectBooleans>();

            _servicesBuilder.AddTransient<ThreeDimensionalRotationViewModel>();
            _servicesBuilder.AddTransient<Views.ThreeDimensionalRotation>();

            _servicesBuilder.AddTransient<PluginsViewModel>();
            _servicesBuilder.AddTransient<Views.Plugins>();

            _servicesBuilder.AddTransient<MoveToElevationViewModel>();
            _servicesBuilder.AddTransient<Views.MoveToElevation>();

            _servicesBuilder.AddTransient<ConnectionStatusFilterViewModel>();
            _servicesBuilder.AddTransient<Views.ConnectionStatusFilter>();

            _servicesBuilder.AddTransient<ExtendBeamViewModel>();
            _servicesBuilder.AddTransient<Views.ExtendBeam>();

            _servicesBuilder.AddTransient<ProjectBeamOntoPlaneViewModel>();
            _servicesBuilder.AddTransient<Views.ProjectBeamOntoPlane>();

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
                var language = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
                language = GetShortLanguage(language);

                TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref _xsDataDir);
                var promptsAilFilePath = Path.Combine(_xsDataDir, @"messages\prompts.ail");

                Localization = new TSDialog.Localization(promptsAilFilePath, language);
                Localization.LoadAilFile(promptsAilFilePath);
            } catch {
                Localization = new TSDialog.Localization();
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
