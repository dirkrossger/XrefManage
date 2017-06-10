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

namespace xReference
{
    class xRef
    {
        //http://adndevblog.typepad.com/autocad/2012/06/finding-all-xrefs-in-the-current-database-using-cnet.html
        public static void XrefGraph(Transaction tr, Database db)
        {
            try
            {
                using (tr.TransactionManager.StartTransaction())
                {
                    //Active.Editor.WriteMessage("\n---Resolving the XRefs------------------");
                    //db.ResolveXrefs(true, false);
                    XrefGraph xg = db.GetHostDwgXrefGraph(true);
                    Active.Editor.WriteMessage("\n---XRef's Graph-------------------------");
                    Active.Editor.WriteMessage("\nCURRENT DRAWING");
                    GraphNode root = xg.RootNode;
                    printchildren1(root, "|-------", Active.Editor, tr, db, Active.Document);
                    Active.Editor.WriteMessage("\n----------------------------------------\n");
                }
            }
            catch (System.Exception ex) { }

        }

        // Recursively prints out information about the XRef's hierarchy
        //private static void printChildren(GraphNode i_root, string i_indent, Editor i_ed, Transaction i_Tx)
        //{
        //    for (int o = 0; o < i_root.NumOut; o++)
        //    {
        //        XrefGraphNode child = i_root.Out(o) as XrefGraphNode;
        //        if (child.XrefStatus == XrefStatus.Resolved)
        //        {
        //            BlockTableRecord bl = i_Tx.GetObject(child.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
        //            i_ed.WriteMessage("\n" + i_indent + child.Database.Filename);
        //            // Name of the Xref (found name)
        //            // You can find the original path too:
        //            //if (bl.IsFromExternalReference == true)
        //            // i_ed.WriteMessage("\n" + i_indent + "Xref path name: "
        //            //                      + bl.PathName);
        //            printChildren(child, "| " + i_indent, i_ed, i_Tx);
        //        }
        //    }
        //}

        public static void printchildren1(GraphNode i_root, string i_indent, Editor i_ed, Transaction i_Tx, Database db, Document doc)
        {
            int workingcount = 0;
            for (int r = 0; r < i_root.NumOut; r++)
            {

                XrefGraphNode child = i_root.Out(r) as XrefGraphNode;

                string xref = child.Name + child.XrefStatus.ToString();
                //Global.variables.ed.WriteMessage("\n : " + xref);

                if (child.XrefStatus == XrefStatus.Resolved)
                {
                    BlockTableRecord blk = i_Tx.GetObject(child.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    printchildren1(child, "| " + i_indent, i_ed, i_Tx, db, doc);
                    workingcount += 1;
                    i_ed.WriteMessage("\n" + 
                        " FileName:" + child.Name + 
                        " FileStatus:" + child.XrefStatus.ToString() + 
                        " XrefNode:" + child + 
                        " xrefHandle:" + child.BlockTableRecordId.Handle
                        );
                    //Global.XrefObj.XrefItem item = new Global.XrefObj.XrefItem() { FileName = child.Name, FileStatus = child.XrefStatus.ToString(), XrefNode = child, xrefHandle = child.BlockTableRecordId.Handle };
                    //Global.variables.Xreflist.Add(item);
                    //Global.variables.ed.WriteMessage("\nXREF ID >>: " + blk.ObjectId + " || " + blk.Handle);
                }
                else
                {
                    continue;
                }
                //Global.variables.ed.WriteMessage("\n=======================================================\n");
            }

        }

        public static void GetXrefFromFile(Database db)
        {
           
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                ObjectIdCollection blockIds = new ObjectIdCollection();
                foreach (ObjectId bId in blockTbl)
                    blockIds.Add(bId);
                ObjectIdCollection xrefIds = XrefTools.filterXrefIds(blockIds);
                if (xrefIds.Count != 0)
                {

                    //db.ReloadXrefs(xrefIds);

                }
                    
                tr.Commit();
            }
        }
    }
}
