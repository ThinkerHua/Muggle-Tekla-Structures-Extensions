#pragma warning disable 1633 // Unrecognized #pragma directive
#pragma reference "Tekla.Macros.Akit"
#pragma reference "Tekla.Macros.Wpf.Runtime"
#pragma reference "Tekla.Macros.Runtime"
#pragma warning restore 1633 // Unrecognized #pragma directive

namespace Muggle.TsExtensions.Macros 
{
    public class MainWindowRunner
    {
        [Tekla.Macros.Runtime.MacroEntryPoint]
        public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime)
        {
            var xsDir = System.Environment.GetEnvironmentVariable("XS_DIR");
            var aplicationsPath = System.IO.Path.Combine(xsDir, "environments\\common\\extensions\\Muggle.TsExtensions", "Muggle.TsExtensions.exe");

            if (System.IO.File.Exists(aplicationsPath))
            {
                var process = new System.Diagnostics.Process();
                process.EnableRaisingEvents = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = aplicationsPath;
                process.Start();
                process.Close();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(aplicationsPath + " not found!", "Muggle.TsExtensions", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}