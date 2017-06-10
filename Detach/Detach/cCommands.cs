using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Geometry;
#endregion



namespace Detach
{
    public class cCommands
    {
        [CommandMethod("DetachXref")]
        public void detach_xref()
        {
            Document Doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = Doc.Editor;

            string mainDrawingFile = @"H:\Google Drive\Test\E20 Vårgårda\100T0201.dwg";

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

    public class Detach : IExtensionApplication
    {
        private static Editor editor =
            Application.DocumentManager.MdiActiveDocument.Editor;

        public void Initialize()
        {
            editor.WriteMessage("\nXrefManage Start with DetachXref");
        }

        public void Terminate()
        {
        }
    }

}
