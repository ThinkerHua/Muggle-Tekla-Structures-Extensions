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

        private readonly string XSDATADIR;
        private readonly Model model;
        private readonly Events events;

        internal TSDialog.Localization Localization { get; }

        public static new App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        public App() {
            model = new Model();

            events = new Events();
            events.TeklaStructuresExit += ExitApp;
            events.Register();

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

        }

        private static IServiceProvider ConfigureServices() {
            var services = new ServiceCollection();

            services.AddSingleton<IMessageBoxService, MessageBoxService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<Views.MainWindow>();

            services.AddTransient<NormalToolsViewModel>();
            services.AddTransient<Views.NormalTools>();

            services.AddTransient<SelectBooleansViewModel>();
            services.AddTransient<Views.SelectBooleans>();

            services.AddTransient<ThreeDimensionalRotationViewModel>();
            services.AddTransient<Views.ThreeDimensionalRotation>();

            services.AddTransient<PluginsViewModel>();
            services.AddTransient<Views.Plugins>();

            services.AddTransient<MoveToElevationViewModel>();
            services.AddTransient<Views.MoveToElevation>();

            return services.BuildServiceProvider();
        }

        private void Application_Startup(object sender, StartupEventArgs e) {

            IMessageBoxService messageBoxService = null;

            try {
                messageBoxService = Services.GetRequiredService<IMessageBoxService>();

                if (!model.GetConnectionStatus()) throw new Exception(NOT_CONNECTED);

                var mainWindow = Services.GetRequiredService<Views.MainWindow>();
                mainWindow.Show();
            } catch (Exception ex) {
                messageBoxService?.ShowError(ex.ToString());
                ExitApp();
                return;
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
