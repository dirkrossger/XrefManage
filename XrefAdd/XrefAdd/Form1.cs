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
        private static List<MyXrefInformation> XrInfoList;
        private static DocumentCollection DocCol;

        public Form1()
        {
            InitializeComponent();
        }

        public MyXrefInformation[] XrInfo;
        void Form1_Load(object sender, EventArgs e)
        {
            List<string> RoList = new List<string>();
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
            string[] Files = Dia.GetFilenames();
            MyStringCompare1 msc1 = new MyStringCompare1();
            Array.Sort(Files, msc1);
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
                    XrInfo = Utility.FindXrefs(Db);
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

        void DrawingSelected(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            if (ViewTogBtn.Text == "Show Per Drawing")
            {
                DwgListview.SelectedItems.Clear();
                return;
            }
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


        #region Buttons
        private void button2_Detach_Click(object sender, EventArgs e)
        {
            DocCol = AcadApp.DocumentManager;
            XrInfoList = new List<MyXrefInformation>();

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
                                foreach (ListViewItem xr in XrefListview.SelectedItems)
                                {
                                    if (xrInfoAr.Name == xr.Text) // !!! error compare filename from XrefListview_Listbox wrong !!!
                                    {
                                        if (!DocInEditor)
                                            Db.ReadDwgFile(DwgName, System.IO.FileShare.ReadWrite, true, null);

                                        BlockTable BlkTbl = Trans.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                                        XrefGraph XrGraph = Db.GetHostDwgXrefGraph(false);
                                        XrefGraphNode XrNode = XrGraph.GetXrefNode(0);

                                        for (int i = 1; i < XrGraph.NumNodes; i++)
                                        {
                                            XrefGraphNode tempNode = XrGraph.GetXrefNode(i);

                                            XrNode = tempNode;
                                            ObjectId detachid = XrNode.BlockTableRecordId;
                                            Db.DetachXref(detachid);
                                            Db.SaveAs(DwgName, DwgVersion.Current);
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
        }

        #endregion Buttons


        [CommandMethod("AddXref", CommandFlags.Session)]
        public void Main()
        {
            DocCol = AcadApp.DocumentManager;
            XrInfoList = new List<MyXrefInformation>();
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
    }
}
