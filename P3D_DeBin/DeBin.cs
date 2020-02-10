using BisDll.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P3D_DeBin
{
    internal class Program
    {
        internal static Program getInstance() { return new Program(); }
        internal static Assembly _assembly = typeof(Program).Assembly;
        
        #region EntryPoint
            [STAThread]
            private static int Main(string[] args)
        {
            #region Subscribe Assembly Resolver
                AppDomain.CurrentDomain.AssemblyResolve += getInstance().AssemblyResolver;
            #endregion

            Trace.Listeners.Add((TraceListener) new ConsoleTraceListener());
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
                    case 0:
                        var openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "ArmA P3d (ODOL)|*.p3d";
                        if (openFileDialog.ShowDialog() == DialogResult.OK) convertP3dFile(openFileDialog.FileName, null, false);
                       
                    break;

                    case 1: 
                        if (File.Exists(args[0]))
                        {
                          if (Path.GetExtension(args[0]) == ".p3d")
                          {
                            convertP3dFile(Path.GetFullPath(args[0]), null, false);
                            break;
                          }
                          Trace.WriteLine("The file '{0}' does not have the p3d file extension.", args[0]);
                          break;
                        }
                        if (Directory.Exists(args[0]))
                        {
                          convertP3dFiles(Directory.EnumerateFiles(args[0], "*.p3d", SearchOption.TopDirectoryOnly).ToArray(), null, false);
                          break;
                        }
                        Trace.WriteLine("The file or directory '{0}' was not found.", args[0]);
                    break;

                    case 2: 
                        if (Directory.Exists(args[0]))
                        {
                          if (Directory.Exists(args[1]))
                          {
                            convertP3dFiles(Directory.EnumerateFiles(args[0], "*.p3d", SearchOption.TopDirectoryOnly).ToArray(), args[1], false);
                            break;
                          }
                          Trace.WriteLine(string.Format("The folder '{0}' does not exist.", args[1]));
                          break;
                        }
                        Trace.WriteLine(string.Format("The folder '{0}' does not exist.", args[0]));
                    break;

                    default:
                        Trace.WriteLine(message);
                    break;
                }
            }
            catch (Exception ex)
            { 
                Trace.Write(ex.StackTrace);
                return 1;
            }
            return 0;
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
            Trace.WriteLine("Loading embedded assembly: "+ askedAssembly.Name + Environment.NewLine + "Has thrown a unhandled exception: " + e);
        }
        finally
        {
            if (assembly != null)
                Trace.WriteLine("Loaded embedded assembly: "+ askedAssembly.Name);
        }
        return assembly;
        }
        }
        #endregion

        #region Bis Format Functions
            private static void convertP3dFiles(IEnumerable<string> srcFiles,string dstFolder, bool allowOverwriting = true)
            {
            Trace.WriteLine(string.Format("Start conversion of {0} p3d files:", (object) srcFiles.Count<string>()));
            Trace.Indent();
            int num = 0;
            foreach (string srcFile in srcFiles)
            {
            string fileName = Path.GetFileName(srcFile);
            if (!Program.convertP3dFile(srcFile, dstFolder == null ? (string) null : Path.Combine(dstFolder, fileName), allowOverwriting))
              ++num;
            }
            Trace.Unindent();
            if (num == 0)
            Trace.WriteLine("Conversions finished successfully.");
            else
            Trace.WriteLine(string.Format("{0} was/were not successful.", (object) num));
            }

            private static bool convertP3dFile(string srcPath, string dstPath = null, bool allowOverwriting = false)
    {
    if (!allowOverwriting && srcPath == dstPath)
    {
        Trace.WriteLine("Overwriting the source file is disabled.");
        return false;
    }
    Trace.WriteLine(string.Format("Reading the p3d ('{0}')...", (object) srcPath));
    P3D instance = P3D.GetInstance(srcPath);
    if (instance is BisDll.Model.MLOD.MLOD)
    {
        Trace.WriteLine(string.Format("'{0}' is already in editable MLOD format", (object) srcPath));
    }
    else
    {
        BisDll.Model.ODOL.ODOL odol = instance as BisDll.Model.ODOL.ODOL;
        if (odol != null)
        {
              Trace.WriteLine("ODOL was loaded successfully.");
              Trace.WriteLine("Start conversion...");
              BisDll.Model.MLOD.MLOD mlod = Conversion.ODOL2MLOD(odol);
              Trace.WriteLine("Conversion successful.");
              string withoutExtension = Path.GetFileNameWithoutExtension(srcPath);
              string directoryName = Path.GetDirectoryName(srcPath);
              string file = dstPath ?? Path.Combine(directoryName, withoutExtension + ".p3d");
              Trace.WriteLine("Saving...");
              mlod.writeToFile(file + ".deBin", allowOverwriting);
              
              if (File.Exists(file + ".deBin"))
              {
                  Trace.WriteLine(string.Format("MLOD successfully saved to '{0}'", (object)file));
                  File.Delete(file); 
                  File.Move(file + ".deBin", file);
              } 
                  
              return File.Exists(file);
        }
        Trace.WriteLine(string.Format("'{0}' could not be loaded.", (object) srcPath));
    }
    return false;
    }
        #endregion
    }
}
