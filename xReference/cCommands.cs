using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

[assembly: CommandClass(typeof(xReference.cCommands))]


namespace xReference
{
    public class cCommands
    {
        [CommandMethod("xx")]
        public static void XrefGraph()
        {
            List<string> files = cFile.GetFiles();
            Database workingDB = HostApplicationServices.WorkingDatabase;
            Database db = new Database(false, true);

            try
            {
                db.ReadDwgFile(files[0], System.IO.FileShare.ReadWrite, false, "");
                db.CloseInput(true);
                HostApplicationServices.WorkingDatabase = db;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("\nUnable to open .dwg file : " + ex.StackTrace);
                return;
            }


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                xRef.GetXrefFromFile(db);
                HostApplicationServices.WorkingDatabase = workingDB;

                tr.Commit();
            }
            
                
        }
    }
}
