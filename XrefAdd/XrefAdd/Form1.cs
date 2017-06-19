using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    public partial class Form1 : Form
    {
        //private static List<MyXrefInformation> XrInfoList;
        private static DocumentCollection DocCol;
        private List<string> RoList;
        private string[] Files, AttFiles;
        private string DwgPathName;

        public MyXrefInformation[] XrInfo;


        public Form1()
        {
            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            
            Autodesk.AutoCAD.Windows.OpenFileDialog Dia =
                new Autodesk.AutoCAD.Windows.OpenFileDialog
                (
                     "Select drawings to manage xref.",
                     "",
                     "dwg",
                     "",
                     Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.AllowMultiple
               );

            if (Dia.ShowDialog() != DialogResult.OK) return;
            Files = Dia.GetFilenames();
            MyStringCompare1 msc1 = new MyStringCompare1();
            Array.Sort(Files, msc1);
            ListFiles(Files);
        }

        void ListFiles(string[] Files)
        {
            RoList = new List<string>();

            foreach (string DwgPath in Files)
            {
                if ((File.GetAttributes(DwgPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    RoList.Add(DwgPath);
                    continue;
                }
                string[] tempStrArray = DwgPath.Split('\\');
                tempStrArray = tempStrArray[tempStrArray.Length - 1].Split('.');
                ListViewItem lvi = new ListViewItem(tempStrArray[0]);
                Database Db;
                DocumentLock tempLock = null;
                Document OpenDoc = null;
                OpenDoc = GetDocumentFrom(DocCol, DwgPath);
                bool DocInEditor = (OpenDoc != null);
                if (DocInEditor)
                {
                    Db = OpenDoc.Database;
                    tempLock = OpenDoc.LockDocument();
                    Db.ResolveXrefs(false, true);
                }
                else
                    Db = new Database(true, false);
                try
                {
                    if (!DocInEditor)
                        Db.ReadDwgFile(DwgPath, System.IO.FileShare.ReadWrite, true, null);
                    XrInfo = MyXrefInformation.FindXrefs(Db);
                    lvi.Tag = XrInfo;
                    if (!XrInfo.Equals(0))
                        DwgListview.Items.Add(lvi);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception AcadEx)
                {
                    MessageBox.Show(AcadEx.Message + "\n\n" + DwgPath + "\n\n" + AcadEx.StackTrace, "AutoCAD error.");
                }
                catch (System.Exception SysEx)
                {
                    MessageBox.Show(SysEx.Message + System.Environment.NewLine + System.Environment.NewLine + SysEx.StackTrace, "System error.");
                }
                if (DocInEditor)
                    tempLock.Dispose();
                else
                    Db.Dispose();
            }
            StringBuilder Sb = new StringBuilder();
            foreach (string str in RoList)
            {
                Sb.AppendLine(str);
            }
            if (!string.IsNullOrEmpty(Sb.ToString()))
                MessageBox.Show(Sb.ToString(), "Read-only files not able to update.");

        }

        void ListFile(string DwgPath)
        {
            RoList = new List<string>();

            if ((File.GetAttributes(DwgPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    RoList.Add(DwgPath);
                }
                string[] tempStrArray = DwgPath.Split('\\');
                tempStrArray = tempStrArray[tempStrArray.Length - 1].Split('.');
                ListViewItem lvi = new ListViewItem(tempStrArray[0]);
                Database Db;
                DocumentLock tempLock = null;
                Document OpenDoc = null;
                OpenDoc = GetDocumentFrom(DocCol, DwgPath);
                bool DocInEditor = (OpenDoc != null);
                if (DocInEditor)
                {
                    Db = OpenDoc.Database;
                    tempLock = OpenDoc.LockDocument();
                    Db.ResolveXrefs(false, true);
                }
                else
                    Db = new Database(true, false);
                try
                {
                    if (!DocInEditor)
                        Db.ReadDwgFile(DwgPath, System.IO.FileShare.ReadWrite, true, null);
                    XrInfo = MyXrefInformation.FindXrefs(Db);
                    lvi.Tag = XrInfo;
                if (!XrInfo.Equals(0))
                {
                    DwgListview.Items[0].Remove();
                    DwgListview.Items.Add(lvi);
                }
                   
                }
                catch (Autodesk.AutoCAD.Runtime.Exception AcadEx)
                {
                    MessageBox.Show(AcadEx.Message + "\n\n" + DwgPath + "\n\n" + AcadEx.StackTrace, "AutoCAD error.");
                }
                catch (System.Exception SysEx)
                {
                    MessageBox.Show(SysEx.Message + System.Environment.NewLine + System.Environment.NewLine + SysEx.StackTrace, "System error.");
                }
                if (DocInEditor)
                    tempLock.Dispose();
                else
                    Db.Dispose();

            StringBuilder Sb = new StringBuilder();
            foreach (string str in RoList)
            {
                Sb.AppendLine(str);
            }
            if (!string.IsNullOrEmpty(Sb.ToString()))
                MessageBox.Show(Sb.ToString(), "Read-only files not able to update."); 

        }


        private Document GetDocumentFrom(DocumentCollection docCol, string dwgPath)
        {
            Document Doc = null;
            foreach (Document doc in docCol)
            {
                if (string.Compare(dwgPath, doc.Name, true) == 0)
                    Doc = doc;
            }
            return Doc;
        }

        void ListXrefs()
        {
            XrefListview.Items.Clear();
            List<ListViewItem> LocList;
            foreach (ListViewItem lvi in DwgListview.SelectedItems)
            {
                foreach (MyXrefInformation xrInfo in lvi.Tag as MyXrefInformation[])
                {
                    string curName = xrInfo.Name;
                    string newName = xrInfo.NewName;
                    string Path = (xrInfo.Path == xrInfo.NewPath ? xrInfo.Path : xrInfo.NewPath);

                    bool DidAdd = false;
                    foreach (ListViewItem xrlvi in XrefListview.Items)
                    {
                        if (xrlvi.Text == curName && xrlvi.SubItems[1].Text == newName && xrlvi.SubItems[2].Text == Path)
                        {
                            LocList = xrlvi.Tag as List<ListViewItem>;
                            LocList.Add(lvi);
                            xrlvi.Tag = LocList;
                            DidAdd = true;
                            break;
                        }
                    }
                    if (DidAdd)
                        continue;
                    LocList = new List<ListViewItem>();
                    ListViewItem tempLvi = new ListViewItem(xrInfo.Name);
                    //tempLvi.SubItems.Add(xrInfo.NewName);
                    tempLvi.SubItems.Add(xrInfo.NewPath);
                    if (xrInfo.IsNested)
                        tempLvi.ForeColor = Color.Crimson;
                    LocList.Add(lvi);
                    tempLvi.Tag = LocList;
                    XrefListview.Items.Add(tempLvi);
                }
            }
            XrefListview.Sort();
        }


        void DrawingSelected(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right) return;
            //if (ViewTogBtn.Text == "Show Per Drawing")
            //{
            //    DwgListview.SelectedItems.Clear();
            //    return;
            //}
            try
            {
                if (DwgListview.SelectedItems[0].Selected)
                {
                    string select = DwgListview.SelectedItems[0].Text;
                    foreach (string file in Files)
                    {
                        if (select == Path.GetFileNameWithoutExtension(file))
                            DwgPathName = file;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Select Drawing!");
            }

            ListXrefs();
        }


        #region Buttons
        private void button2_Detach_Click(object sender, EventArgs e)
        {
            DocCol = AcadApp.DocumentManager;
            //XrInfoList = new List<MyXrefInformation>();
            int index = DwgListview.SelectedIndices[0];

            //List<ListViewItem> LocList;
            foreach (ListViewItem lvi in DwgListview.SelectedItems)
            {
                using (DocumentLock DocLock = DocCol.MdiActiveDocument.LockDocument())
                {
                    foreach (MyXrefInformation xrInfoAr in lvi.Tag as MyXrefInformation[])
                    {
                        string DwgName = xrInfoAr.DrawingPath;
                        Database Db;
                        Document OpenDoc = null;
                        DocumentLock tempLock = null;
                        //bool ShouldSave = false;
                        OpenDoc = GetDocumentFrom(DocCol, DwgName);
                       
                        bool DocInEditor = (OpenDoc != null);
                        if (DocInEditor)
                        {
                            Db = OpenDoc.Database;
                            tempLock = OpenDoc.LockDocument();
                        }
                        else
                            Db = new Database(true, false);

                        using (Transaction Trans = Db.TransactionManager.StartTransaction())
                        {
                            try
                            {
                                //for(int idx = 0; idx < XrefListview.SelectedItems.Count; idx++)
                                //{
                                //ListViewItem xr = XrefListview.SelectedItems[idx];
                                foreach (ListViewItem xr in XrefListview.SelectedItems)
                                {
                                    if (xrInfoAr.Name == xr.Text) // !!! error compare filename from XrefListview_Listbox wrong !!!
                                    {
                                        if (!DocInEditor)
                                            Db.ReadDwgFile(DwgName, System.IO.FileShare.ReadWrite, true, null);

                                        //BlockTable BlkTbl = Trans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        XrefGraph XrGraph = Db.GetHostDwgXrefGraph(true);

                                        XrefGraphNode XrNode = XrGraph.GetXrefNode(xr.Text);

                                        for (int i = 1; i < XrGraph.NumNodes; i++)
                                        {
                                            //XrefGraphNode tempNode = XrGraph.GetXrefNode(i);

                                            //XrNode = tempNode;
                                            ObjectId detachid = XrNode.BlockTableRecordId;
                                            Db.DetachXref(detachid);
                                            Db.SaveAs(DwgName, DwgVersion.Current);

                                            xr.Remove();
                                            ListFile(DwgName);

                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Autodesk.AutoCAD.Runtime.Exception AcadEx)
                            {
                                MessageBox.Show(AcadEx.Message + "\n\n" + AcadEx.StackTrace + "\n\n" + DwgName, "AutoCAD error.");
                            }
                            catch (System.Exception SysEx)
                            {
                                MessageBox.Show(SysEx.Message, "System error.");
                            }
                            Trans.Commit();
                        }

                        if (DocInEditor)
                            tempLock.Dispose();
                        else
                            Db.Dispose();
                    }

                }
            }

            //foreach (ListViewItem lvi in DwgListview.Items)
            //{
            //    DwgListview.Items.Remove(lvi);
            //}
            //ListFiles(Files);
            //DwgListview.Items[index].Selected = true;
        }

        private void button1_Attach_Click(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Windows.OpenFileDialog DiaAttach =
                new Autodesk.AutoCAD.Windows.OpenFileDialog
                (
                     "Select drawings to attach.",
                     "",
                     "dwg",
                     "",
                     Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.AllowMultiple
               );

            if (DiaAttach.ShowDialog() != DialogResult.OK) return;
            AttFiles = DiaAttach.GetFilenames();

            int index = DwgListview.SelectedIndices[0];
            // Get the current database and start a transaction
            Database db;
            db = AcadApp.DocumentManager.MdiActiveDocument.Database;


            using (DocumentLock DocLock = DocCol.MdiActiveDocument.LockDocument())
            {
                Database Db;
                Document OpenDoc = null;
                DocumentLock tempLock = null;
                string DwgName = DwgPathName;
                OpenDoc = GetDocumentFrom(DocCol, DwgName);
                bool DocInEditor = (OpenDoc != null);
                if (DocInEditor)
                {
                    Db = OpenDoc.Database;
                    tempLock = OpenDoc.LockDocument();
                }
                else
                    Db = new Database(true, false);

                bool saveRequired = false;
                using (Transaction acTrans = Db.TransactionManager.StartTransaction())
                {

                    foreach (string file in AttFiles)
                    {
                        ObjectId acXrefId = Db.AttachXref(file, Path.GetFileName(file));

                        // If a valid reference is created then continue
                        if (!acXrefId.IsNull)
                        {
                            // Attach the DWG reference to the current space
                            Point3d insPt = new Point3d(0, 0, 0);
                            using (BlockReference acBlkRef = new BlockReference(insPt, acXrefId))
                            {
                                BlockTableRecord acBlkTblRec;
                                acBlkTblRec = acTrans.GetObject(Db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                                acBlkTblRec.AppendEntity(acBlkRef);
                                acTrans.AddNewlyCreatedDBObject(acBlkRef, true);

                                saveRequired = true;
                            }
                        }
                    }
                    if (saveRequired)
                        Db.SaveAs(DwgPathName, DwgVersion.Current);
                    


                    // Save the new objects to the database
                    acTrans.Commit();

                    // Dispose of the transaction
                }
            }
            foreach (ListViewItem lvi in DwgListview.Items)
            {
                DwgListview.Items.Remove(lvi);
            }
            ListFiles(Files);
            DwgListview.Items[index].Selected = true;
        }

        #endregion Buttons


        [CommandMethod("XrBuild", CommandFlags.Session)]
        public void Main()
        {
            DocCol = AcadApp.DocumentManager;
            //XrInfoList = new List<MyXrefInformation>();
            bool DocInEditor;
            Database Db;

            using (DocumentLock DocLock = DocCol.MdiActiveDocument.LockDocument())
            {
                using (Form1 XrMan = new Form1())
                {
                    AcadApp.ShowModalDialog(XrMan);
                }
            }
        }

        public class XrefAdd : IExtensionApplication
        {
            private static Editor editor =
                AcadApp.DocumentManager.MdiActiveDocument.Editor;

            public void Initialize()
            {
                editor.WriteMessage("\nXrefBuild Start with XrBuild!");
            }

            public void Terminate()
            {
            }
        }

    }
}
