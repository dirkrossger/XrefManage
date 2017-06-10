using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Geometry;


namespace Detach
{
    public class Class1
    {
        [CommandMethod("DetachXref")]
        public void detach_xref()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = Doc.Editor;

            string mainDrawingFile = @"C:\Temp\Test.dwg";

            Database db = new Database(false, false);
            using (db)
            {
                try
                {
                    db.ReadDwgFile(mainDrawingFile, System.IO.FileShare.ReadWrite, false, "");

                }
                catch (System.Exception)
                {
                    ed.WriteMessage("\nUnable to read the drawingfile.");
                    return;
                }

                bool saveRequired = false;
                db.ResolveXrefs(true, false);
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    XrefGraph xg = db.GetHostDwgXrefGraph(true);

                    int xrefcount = xg.NumNodes;
                    for (int j = 0; j < xrefcount; j++)
                    {
                        XrefGraphNode xrNode = xg.GetXrefNode(j);
                        String nodeName = xrNode.Name;

                        if (xrNode.XrefStatus == XrefStatus.FileNotFound)
                        {
                            ObjectId detachid = xrNode.BlockTableRecordId;

                            db.DetachXref(detachid);

                            saveRequired = true;
                            ed.WriteMessage("\nDetached successfully");

                            break;
                        }
                    }
                    tr.Commit();
                }

                if (saveRequired)
                    db.SaveAs(mainDrawingFile, DwgVersion.Current);
            }
        }
    }
}
