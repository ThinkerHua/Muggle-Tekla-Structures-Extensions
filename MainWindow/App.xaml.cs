using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Muggle.TeklaPlugins.MainWindow.Services;
using Muggle.TeklaPlugins.MainWindow.ViewModels;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Events = Tekla.Structures.Model.Events;
using Model = Tekla.Structures.Model.Model;
using TSDialog = Tekla.Structures.Dialog;

namespace Muggle.TeklaPlugins.MainWindow {
    public partial class App : Application {
        internal const string USER_INTERRUPT = "User interrupt";
        internal const string NOT_CONNECTED = "Not connected to a model.";

        private string XSDATADIR;
        private Model model;
        private Events events;

        private readonly ServiceCollection servicesBuilder;

        internal TSDialog.Localization Localization { get; private set; }

        public static new App Current => (App)Application.Current;

        public IServiceProvider Services { get; private set; }

        public App() {
            //  先配置MessageboxService，以便出现异常时可以弹出消息框
            servicesBuilder = new ServiceCollection();
            servicesBuilder.AddSingleton<IMessageBoxService, MessageBoxService>();

            Services = servicesBuilder.BuildServiceProvider();
        }

        private IServiceProvider ConfigureServices() {
            servicesBuilder.AddSingleton<IMessageBoxService, MessageBoxService>();
            servicesBuilder.AddSingleton<INavigationService, NavigationService>();

            servicesBuilder.AddSingleton<MainWindowViewModel>();
            servicesBuilder.AddSingleton<Views.MainWindow>();

            servicesBuilder.AddTransient<NormalToolsViewModel>();
            servicesBuilder.AddTransient<Views.NormalTools>();

            servicesBuilder.AddTransient<SelectBooleansViewModel>();
            servicesBuilder.AddTransient<Views.SelectBooleans>();

            servicesBuilder.AddTransient<ThreeDimensionalRotationViewModel>();
            servicesBuilder.AddTransient<Views.ThreeDimensionalRotation>();

            servicesBuilder.AddTransient<PluginsViewModel>();
            servicesBuilder.AddTransient<Views.Plugins>();

            servicesBuilder.AddTransient<MoveToElevationViewModel>();
            servicesBuilder.AddTransient<Views.MoveToElevation>();

            servicesBuilder.AddTransient<ConnectionStatusFilterViewModel>();
            servicesBuilder.AddTransient<Views.ConnectionStatusFilter>();

            return servicesBuilder.BuildServiceProvider();
        }

        private void Application_Startup(object sender, StartupEventArgs e) {

            IMessageBoxService messageBoxService = null;

            try {
                messageBoxService = Services.GetRequiredService<IMessageBoxService>();

                model = new Model();
                if (!model.GetConnectionStatus()) throw new Exception(NOT_CONNECTED);

                events = new Events();
                events.TeklaStructuresExit += ExitApp;
                events.Register();
            } catch (Exception ex) {
                messageBoxService?.ShowError(ex.ToString());
                ExitApp();
                return;
            }

            try {
                var language = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
                language = GetShortLanguage(language);

                TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref XSDATADIR);
                var promptsAilFilePath = Path.Combine(XSDATADIR, @"messages\prompts.ail");

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
                ExitApp();
            }
        }

        private void ExitApp() {
            events.UnRegister();
            Dispatcher.Invoke(() => {
                Shutdown();
            });
            /*new Thread(() => {
                Environment.Exit(0);
            }).Start();*/
        }

        private static string GetShortLanguage(string Language) {
            return Language switch {
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
