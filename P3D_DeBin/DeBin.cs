using BisDll.Model; 
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq; 

namespace P3D_DeBin
{
    internal class DeBin
    {
        #region convertP3D Functions
        internal static void convertP3dFiles(IEnumerable<string> srcFiles,string dstFolder, bool allowOverwriting = true)
            {
                Trace.WriteLine(string.Format("Start conversion of {0} p3d files:", (object) srcFiles.Count<string>()));
                Trace.Indent();
                int num = 0;
                foreach (string srcFile in srcFiles)
                {
                string fileName = Path.GetFileName(srcFile);
                if (!convertP3dFile(srcFile, dstFolder == null ? (string) null : Path.Combine(dstFolder, fileName), allowOverwriting))
                  ++num;
                }
                Trace.Unindent();
                if (num == 0)
                Trace.WriteLine("Conversions finished successfully.");
                else
                Trace.WriteLine(string.Format("{0} was/were not successful.", (object) num));
            }

        internal static bool convertP3dFile(string srcPath, string dstPath = null, bool allowOverwriting = false)
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
