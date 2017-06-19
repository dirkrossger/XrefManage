using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#region Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Geometry;
#endregion

namespace XrefAdd
{
    public class MyXrefInformation
    {
        private string XrName;
        private string XrPath;
        private string XrFndAtPath = string.Empty;
        private string NewXrName = "";
        private string NewXrPath = "";
        private string[] InsertedAt;
        private string[] _OwnerNames = new string[0];
        private string[] _ChildrenNames = new string[0];
        private Autodesk.AutoCAD.DatabaseServices.XrefStatus XrStatus;
        private string DwgPath;
        private bool Overlay;
        private bool Nested;
        private ObjectId xId;

        public MyXrefInformation() { }

        ~MyXrefInformation() { }

        public string Name
        {
            get { return XrName; }
            set { XrName = value; }
        }
        public string Path
        {
            get { return XrPath; }
            set { XrPath = value; }
        }
        public string FoundAtPath
        {
            get { return XrFndAtPath; }
            set { XrFndAtPath = value; }
        }
        public string[] InsertedWhere
        {
            get { return InsertedAt; }
            set { InsertedAt = value; }
        }
        public Autodesk.AutoCAD.DatabaseServices.XrefStatus Status
        {
            get { return XrStatus; }
            set { XrStatus = value; }
        }
        public string DrawingPath
        {
            get { return DwgPath; }
            set { DwgPath = value; }
        }
        public bool IsOverlay
        {
            get { return Overlay; }
            set { Overlay = value; }
        }
        public bool IsNested
        {
            get { return Nested; }
            set { Nested = value; }
        }
        public string NewName
        {
            get { return NewXrName; }
            set { NewXrName = value; }
        }
        public string NewPath
        {
            get { return NewXrPath; }
            set { NewXrPath = value; }
        }
        public string[] OwnerNames
        {
            get { return _OwnerNames; }
            set { _OwnerNames = value; }
        }
        public string[] ChildrenNames
        {
            get { return _ChildrenNames; }
            set { _ChildrenNames = value; }
        }
        public ObjectId XId
        {
            get
            {
                return xId;
            }

            set
            {
                xId = value;
            }
        }

        public void Remove()
        {
            this.XId = ObjectId.Null;
        }
        public static MyXrefInformation[] FindXrefs(Database db)
        {
            MyXrefInformation[] XrefArray;
            string[] tempStrArray;
            int tempCnt;
            using (Transaction Trans = db.TransactionManager.StartTransaction())
            {
                BlockTable BlkTbl = (BlockTable)Trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                //db.ResolveXrefs(false, true);
                XrefGraph XrGph = db.GetHostDwgXrefGraph(false);
                XrefArray = new MyXrefInformation[XrGph.NumNodes - 1];
                for (int i = 1; i < XrGph.NumNodes; ++i)
                {
                    XrefGraphNode XrNode = XrGph.GetXrefNode(i);
                    BlockTableRecord btr = (BlockTableRecord)Trans.GetObject(XrNode.BlockTableRecordId, OpenMode.ForRead);
                    MyXrefInformation XrInfo = new MyXrefInformation();
                    XrInfo.Name = XrNode.Name;
                    XrInfo.NewName = XrNode.Name;
                    XrInfo.Path = btr.PathName;
                    XrInfo.NewPath = btr.PathName;
                    XrInfo.DrawingPath = db.Filename;
                    XrInfo.XId = XrNode.BlockTableRecordId;

                    string FoundAt = WillLoad(btr.PathName, db);
                    if (XrNode.XrefStatus == XrefStatus.Unresolved)
                    {
                        if (string.IsNullOrEmpty(FoundAt))
                            XrInfo.Status = XrefStatus.Unresolved;
                        else
                            XrInfo.Status = XrefStatus.Resolved;
                    }
                    else
                        XrInfo.Status = XrNode.XrefStatus;
                    if (XrInfo.Status == XrefStatus.Resolved)
                    {
                        XrInfo.FoundAtPath = FoundAt;
                    }
                    XrInfo.IsNested = XrNode.IsNested;
                    XrInfo.IsOverlay = btr.IsFromOverlayReference;
                    ObjectIdCollection ObjIdCol = (ObjectIdCollection)btr.GetBlockReferenceIds(true, true);
                    string[] InsertedAtArray = new string[ObjIdCol.Count];
                    for (int j = 0; j < ObjIdCol.Count; ++j)
                    {
                        ObjectId ObjId = ObjIdCol[j];
                        BlockReference BlkRef = (BlockReference)Trans.GetObject(ObjId, OpenMode.ForRead);
                        BlockTableRecord tempbtr = (BlockTableRecord)Trans.GetObject(BlkRef.OwnerId, OpenMode.ForRead);
                        if (tempbtr.IsLayout)
                        {
                            Layout templo = (Layout)Trans.GetObject(tempbtr.LayoutId, OpenMode.ForRead);
                            InsertedAtArray[j] = "Layout: " + templo.LayoutName;
                        }
                        else InsertedAtArray[j] = "Block: " + tempbtr.Name;
                    }
                    XrInfo.InsertedWhere = InsertedAtArray;
                    if (!XrNode.NumIn.Equals(0))
                    {
                        tempStrArray = new string[XrNode.NumIn];
                        tempCnt = 0;
                        for (int j = 0; j < XrNode.NumIn; j++)
                        {
                            int tempInt = FindGraphLocation(XrNode.In(j));
                            if (tempInt.Equals(-1))
                                continue;
                            tempStrArray[tempCnt] = XrGph.GetXrefNode(tempInt).Name;
                            tempCnt++;
                        }
                        XrInfo.OwnerNames = tempStrArray;
                    }
                    if (!XrNode.NumOut.Equals(0))
                    {
                        tempStrArray = new string[XrNode.NumOut];
                        tempCnt = 0;
                        for (int j = 0; j < XrNode.NumOut; j++)
                        {
                            int tempInt = FindGraphLocation(XrNode.Out(j));
                            if (tempInt.Equals(-1))
                                continue;
                            tempStrArray[tempCnt] = XrGph.GetXrefNode(tempInt).Name;
                            tempCnt++;
                        }
                        XrInfo.ChildrenNames = tempStrArray;
                    }
                    XrefArray[i - 1] = XrInfo;
                }
            }
            return XrefArray;
        }

        public static int FindGraphLocation(GraphNode grNode)
        {
            Graph Gr = grNode.Owner;
            for (int i = 0; i < Gr.NumNodes; i++)
            {
                if (grNode.Equals(Gr.Node(i)))
                    return i;
            }
            return -1;
        }

        public static string WillLoad(string FilePath, Database db)
        {
            string FoundAt = "";
            string[] tempStrAr = FilePath.Split('.');
            string FileExt = tempStrAr[tempStrAr.Length - 1];
            try
            {
                FoundAt = HostApplicationServices.Current.FindFile(FilePath, db, FindFileHint.Default);
            }
            catch { }

            if (!string.Compare(FoundAt, string.Empty).Equals(0))
                return FoundAt;

            if (string.Compare(FilePath.Substring(0, 2), "..").Equals(0) || string.Compare(FilePath.Substring(0, 1), ".").Equals(0))
            {
                string[] XrPathArray = FilePath.Split('\\');
                string PartialPath = "";
                for (int i = 1; i < XrPathArray.Length; ++i)
                {
                    PartialPath = PartialPath + "\\" + XrPathArray[i];
                }
                FileInfo DwgInfo = new FileInfo(db.Filename);
                string tempFilePath = DwgInfo.DirectoryName + PartialPath;
                try { FoundAt = HostApplicationServices.Current.FindFile(tempFilePath, db, FindFileHint.Default); }
                catch { }
                if (!string.Compare(FoundAt, string.Empty).Equals(0))
                    return FoundAt;
            }
            return string.Empty;
        }

        public static ObjectId InsertBlock(Database db, string loName, string blkName, Point3d insPt)
        {
            ObjectId RtnObjId = ObjectId.Null;
            using (Transaction Trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary LoDict = Trans.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                foreach (DictionaryEntry de in LoDict)
                {
                    if (string.Compare((string)de.Key, loName, true).Equals(0))
                    {
                        Layout Lo = Trans.GetObject((ObjectId)de.Value, OpenMode.ForWrite) as Layout;
                        BlockTable BlkTbl = Trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord LoRec = Trans.GetObject(Lo.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                        ObjectId BlkTblRecId = GetNonErasedTableRecordId(BlkTbl.Id, blkName);
                        if (BlkTblRecId.IsNull)
                        {
                            string BlkPath = HostApplicationServices.Current.FindFile(blkName + ".dwg", db, FindFileHint.Default);
                            if (string.IsNullOrEmpty(BlkPath))
                                return RtnObjId;
                            BlkTbl.UpgradeOpen();
                            using (Database tempDb = new Database(false, true))
                            {
                                tempDb.ReadDwgFile(BlkPath, FileShare.Read, true, null);
                                db.Insert(blkName, tempDb, false);
                            }
                            BlkTblRecId = GetNonErasedTableRecordId(BlkTbl.Id, blkName);
                        }
                        LoRec.UpgradeOpen();
                        BlockReference BlkRef = new BlockReference(insPt, BlkTblRecId);
                        LoRec.AppendEntity(BlkRef);
                        Trans.AddNewlyCreatedDBObject(BlkRef, true);
                        BlockTableRecord BlkTblRec = Trans.GetObject(BlkTblRecId, OpenMode.ForRead) as BlockTableRecord;
                        if (BlkTblRec.HasAttributeDefinitions)
                        {
                            foreach (ObjectId objId in BlkTblRec)
                            {
                                AttributeDefinition AttDef = Trans.GetObject(objId, OpenMode.ForRead) as AttributeDefinition;
                                if (AttDef != null)
                                {
                                    AttributeReference AttRef = new AttributeReference();
                                    AttRef.SetAttributeFromBlock(AttDef, BlkRef.BlockTransform);
                                    BlkRef.AttributeCollection.AppendAttribute(AttRef);
                                    Trans.AddNewlyCreatedDBObject(AttRef, true);
                                }
                            }
                        }
                        Trans.Commit();
                    }
                }
            }
            return RtnObjId;
        }

        public static ObjectId GetNonErasedTableRecordId(ObjectId TableId, string Name)
        {
            ObjectId id = ObjectId.Null;
            using (Transaction tr = TableId.Database.TransactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)tr.GetObject(TableId, OpenMode.ForRead);
                if (table.Has(Name))
                {
                    id = table[Name];
                    if (!id.IsErased)
                        return id;
                    foreach (ObjectId recId in table)
                    {
                        if (!recId.IsErased)
                        {
                            SymbolTableRecord rec = (SymbolTableRecord)tr.GetObject(recId, OpenMode.ForRead);
                            if (string.Compare(rec.Name, Name, true) == 0)
                                return recId;
                        }
                    }
                }
            }
            return id;
        }

    }
}
