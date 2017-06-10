using System;
using System.IO;
using System.Text;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

using ofdFlags = Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags;

[assembly: ExtensionApplication(typeof(xReference.XrefTools))]
[assembly: CommandClass(typeof(xReference.XrefTools))]


namespace xReference
{

    public class XrefTools : IExtensionApplication
    {
        #region ExtensionAppImplementation

        public void Initialize()
        {
            Active.Editor.WriteMessage(
                "\nFollowing routines in: " +
                "\nAttachXrefs: " +
                "\nBindXrefs: " +
                "\nDetachXrefs: " +
                "\nOpenXrefs: " +
                "\nReloadXrefs: " +
                "\nReloadAllXrefs: " +
                "\nUnloadXrefs: "
                );
        }
        public void Terminate() { }

        #endregion

        #region Helpers

        public delegate void ProcessSingleXref(BlockTableRecord btr);

        public delegate void ProcessMultipleXrefs(ObjectIdCollection xrefIds);

        public static void detachXref(BlockTableRecord btr)
        {
            Application.DocumentManager.MdiActiveDocument.Database.DetachXref(btr.ObjectId);
        }

        public static void openXref(BlockTableRecord btr)
        {
            string xrefPath = btr.PathName;
            if (xrefPath.Contains(".\\"))
            {
                string hostPath =
                    Application.DocumentManager.MdiActiveDocument.Database.Filename;
                Directory.SetCurrentDirectory(Path.GetDirectoryName(hostPath));
                xrefPath = Path.GetFullPath(xrefPath);
            }
            if (!File.Exists(xrefPath)) return;
            Document doc = Application.DocumentManager.Open(xrefPath, false);
            if (doc.IsReadOnly)
            {
                System.Windows.Forms.MessageBox.Show(
                    doc.Name + " opened in read-only mode.",
                    "OpenXrefs",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public static void bindXrefs(ObjectIdCollection xrefIds)
        {
            Application.DocumentManager.MdiActiveDocument.Database.BindXrefs(xrefIds, false);
        }

        public static void reloadXrefs(ObjectIdCollection xrefIds)
        {
            Application.DocumentManager.MdiActiveDocument.Database.ReloadXrefs(xrefIds);
        }

        public static void unloadXrefs(ObjectIdCollection xrefIds)
        {
            Application.DocumentManager.MdiActiveDocument.Database.UnloadXrefs(xrefIds);
        }

        public static void processXrefs(string promptMessage, ProcessSingleXref process)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] filterList = { new TypedValue(0, "INSERT") };
            ed.WriteMessage(promptMessage);
            PromptSelectionResult result = ed.GetSelection(new SelectionFilter(filterList));
            if (result.Status != PromptStatus.OK) return;

            ObjectId[] ids = result.Value.GetObjectIds();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection xrefIds = new ObjectIdCollection();
                foreach (ObjectId id in ids)
                {
                    BlockReference blockRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead, false, true);
                    ObjectId bId = blockRef.BlockTableRecord;
                    if (!xrefIds.Contains(bId))
                    {
                        xrefIds.Add(bId);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bId, OpenMode.ForRead);
                        if (btr.IsFromExternalReference)
                            process(btr);
                    }
                }
                tr.Commit();
            }
        }

        public static void processXrefs(string promptMessage, ProcessMultipleXrefs process)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] filterList = { new TypedValue(0, "INSERT") };
            ed.WriteMessage(promptMessage);
            PromptSelectionResult result = ed.GetSelection(new SelectionFilter(filterList));
            if (result.Status != PromptStatus.OK) return;

            ObjectId[] ids = result.Value.GetObjectIds();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection blockIds = new ObjectIdCollection();
                foreach (ObjectId id in ids)
                {
                    BlockReference blockRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead, false, true);
                    blockIds.Add(blockRef.BlockTableRecord);
                }
                ObjectIdCollection xrefIds = filterXrefIds(blockIds);
                if (xrefIds.Count != 0)
                    process(xrefIds);
                tr.Commit();
            }
        }

        public static void attachXrefs(string[] fileNames)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Array.Sort(fileNames);
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            double dimScale = db.Dimscale;
            foreach (string fileName in fileNames)
            {
                PromptPointOptions options = new PromptPointOptions("Pick insertion point for " + fileName + ": ");
                options.AllowNone = false;
                PromptPointResult pt = ed.GetPoint(options);
                if (pt.Status != PromptStatus.OK) continue;

                double xrefScale = getDwgScale(fileName);
                double scaleFactor = dimScale / xrefScale;
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    ObjectId xrefId = db.AttachXref(fileName, Path.GetFileNameWithoutExtension(fileName));
                    BlockReference blockRef = new BlockReference(pt.Value, xrefId);
                    BlockTableRecord layoutBlock = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    blockRef.ScaleFactors = new Scale3d(scaleFactor, scaleFactor, scaleFactor);
                    blockRef.Layer = "0";
                    layoutBlock.AppendEntity(blockRef);
                    tr.AddNewlyCreatedDBObject(blockRef, true);
                    tr.Commit();
                }
            }
        }

        public static double getDwgScale(string fileName)
        {
            using (Database db = new Database(false, true))
            {
                db.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                return db.Dimscale;
            }
        }

        public static ObjectIdCollection filterXrefIds(ObjectIdCollection blockIds)
        {
            ObjectIdCollection xrefIds = new ObjectIdCollection();
            foreach (ObjectId bId in blockIds)
            {
                if (!xrefIds.Contains(bId))
                {
                    BlockTableRecord btr = (BlockTableRecord)bId.GetObject(OpenMode.ForRead);
                    if (btr.IsFromExternalReference)
                        xrefIds.Add(bId);
                }
            }
            return xrefIds;
        }

        #endregion

        #region Commands

        [CommandMethod("XrefTools", "AttachXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefAttach()
        {
            string initFolder = Application.DocumentManager.MdiActiveDocument.Database.Filename.ToUpper();
            if (initFolder.Contains("PLOT"))
            {
                initFolder = initFolder.Replace("-PLOT.DWG", "");
                initFolder = initFolder.Replace("PLOT\\", "");
                initFolder = initFolder.Replace("PLOTS\\", "");
                if (!Directory.Exists(initFolder))
                    initFolder = Application.DocumentManager.MdiActiveDocument.Database.Filename;
            }

            ofdFlags flags = ofdFlags.DefaultIsFolder | ofdFlags.AllowMultiple;
            OpenFileDialog dlg = new OpenFileDialog("Select Drawings to Attach", initFolder, "dwg", "Select Xrefs", flags);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                attachXrefs(dlg.GetFilenames());
        }

        [CommandMethod("XrefTools", "BindXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefBind()
        {
            processXrefs("\nSelect xrefs to bind: ", XrefTools.bindXrefs);
        }

        [CommandMethod("XrefTools", "DetachXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefDetach()
        {
            processXrefs("\nSelect xrefs to detach: ", XrefTools.detachXref);
        }

        [CommandMethod("XrefTools", "OpenXrefs", CommandFlags.Session)]
        public static void XrefOpen()
        {
            processXrefs("\nSelect xrefs to open: ", XrefTools.openXref);
        }

        [CommandMethod("XrefTools", "ReloadXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefReload()
        {
            processXrefs("\nSelect xrefs to reload: ", XrefTools.reloadXrefs);
        }

        [CommandMethod("XrefTools", "ReloadAllXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefReloadAll()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                ObjectIdCollection blockIds = new ObjectIdCollection();
                foreach (ObjectId bId in blockTbl)
                    blockIds.Add(bId);
                ObjectIdCollection xrefIds = filterXrefIds(blockIds);
                if (xrefIds.Count != 0)
                    db.ReloadXrefs(xrefIds);
                tr.Commit();
            }
        }

        [CommandMethod("XrefTools", "UnloadXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefUnload()
        {
            processXrefs("\nSelect xrefs to unload: ", XrefTools.unloadXrefs);
        }

        #endregion

    }
}
