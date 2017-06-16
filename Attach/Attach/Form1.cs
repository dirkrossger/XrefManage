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

namespace Attach
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] Files;
        private List<string> RoList;
        private static DocumentCollection DocCol;

        private void button1_Add_Click(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Windows.OpenFileDialog Dia =
                new Autodesk.AutoCAD.Windows.OpenFileDialog
                ("Select drawings to Add.", "", "dwg", "", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.AllowMultiple);
            if (Dia.ShowDialog() != DialogResult.OK) return;
            Files = Dia.GetFilenames();

            Attach();
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


        void Attach()
        {
            // Get the current database and start a transaction
            Database db;
            db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            foreach (ListViewItem lvi in DwgListview.SelectedItems)
            {
                using (DocumentLock DocLock = DocCol.MdiActiveDocument.LockDocument())
                {
                    Database Db;
                    Document OpenDoc = null;
                    DocumentLock tempLock = null;
                    string DwgName = lvi.Text;// Path.GetFileName(lvi.Text);
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

                        foreach (string file in Files)
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
                            Db.SaveAs(@"C:\Users\A480836\Google Drive\XrefManage\1.dwg", DwgVersion.Current);


                        // Save the new objects to the database
                        acTrans.Commit();

                        // Dispose of the transaction
                    }
                }
            }
        }

        void ListFiles()
        {
            RoList = new List<string>();
            DocCol = AcadApp.DocumentManager;

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

        private void Form1_Load(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Windows.OpenFileDialog Dia = 
                new Autodesk.AutoCAD.Windows.OpenFileDialog("Select drawings to manage xref.", "", "dwg", "", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.AllowMultiple);

            if (Dia.ShowDialog() != DialogResult.OK) return;
            Files = Dia.GetFilenames();

            ListFiles();

        }
    }
}
