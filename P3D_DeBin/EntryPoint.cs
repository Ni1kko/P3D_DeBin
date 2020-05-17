using System; 
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P3D_DeBin
{  
    internal class EntryPoint
    {
        internal static EntryPoint getInstance() { return new EntryPoint(); }
        internal static Assembly _assembly = typeof(EntryPoint).Assembly;

        #region EntryPoint
        [STAThread]
        private static int Main(string[] args)
        {
            #region Subscribe Assembly Resolver
            AppDomain.CurrentDomain.AssemblyResolve += getInstance().AssemblyResolver;
            #endregion

            Trace.Listeners.Add((TraceListener)new ConsoleTraceListener());
            Console.Title = "P3D DeBin | Nikko#0297";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();
            string message = "P3D DeBin";
            Trace.WriteLine(message + "Loading...");
            Task.Delay(3000);
            try
            {
                switch (args.Length)
                {
                    case 0://no given file or folder. Prompt user to select one
                        var openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "[binarized P3D's] | *.p3d";
                        if (openFileDialog.ShowDialog() == DialogResult.OK) DeBin.convertP3dFile(openFileDialog.FileName, null, false);

                        break;

                    case 1://file or folder given without a output dir.
                        if (File.Exists(args[0]))
                        {
                            if (Path.GetExtension(args[0]) == ".p3d")
                            {
                                DeBin.convertP3dFile(Path.GetFullPath(args[0]), null, false);
                                break;
                            }
                            Trace.WriteLine("The file '{0}' does not have the p3d file extension.", args[0]);
                            break;
                        }
                        if (Directory.Exists(args[0]))
                        {
                            DeBin.convertP3dFiles(Directory.EnumerateFiles(args[0], "*.p3d", SearchOption.TopDirectoryOnly).ToArray(), null, false);
                            break;
                        }
                        Trace.WriteLine("The file or directory '{0}' was not found.", args[0]);
                        break;

                    case 2://file or folder given with a output dir.
                        if (Directory.Exists(args[0]))
                        {
                            if (Directory.Exists(args[1]))
                            {
                                DeBin.convertP3dFiles(Directory.EnumerateFiles(args[0], "*.p3d", SearchOption.TopDirectoryOnly).ToArray(), args[1], false);
                                break;
                            }
                            Trace.WriteLine(string.Format("The folder '{0}' does not exist.", args[1]));
                            break;
                        }
                        Trace.WriteLine(string.Format("The folder '{0}' does not exist.", args[0]));
                        break;

                    default://wow calm down, i only take 2 params
                        Trace.WriteLine(message);//Sigh....
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex.StackTrace);//How? stack track for days.....

                return 1;//boom!
            }
            return 0;//Ting Goess Skrrrr.
        }
        #endregion

        #region Resolve Embedded Assemblies
        internal Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            var askedAssembly = new AssemblyName(args.Name);

            lock (this)
            {
                var stream = _assembly.GetManifestResourceStream($"P3D_DeBin.EmbeddedAssemblies.{askedAssembly.Name}.dll");
                if (stream == null) return null;

                Assembly assembly = null;
                try
                {
                    var assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    assembly = Assembly.Load(assemblyData);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Loading embedded assembly: " + askedAssembly.Name + Environment.NewLine + "Has thrown a unhandled exception: " + e);
                }
                finally
                {
                    if (assembly != null)
                        Trace.WriteLine("Loaded embedded assembly: " + askedAssembly.Name);
                }
                return assembly;
            }
        }
        #endregion
    }
}
