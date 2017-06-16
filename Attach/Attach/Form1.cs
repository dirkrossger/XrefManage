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

        private void button1_Add_Click(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Windows.OpenFileDialog Dia =
                new Autodesk.AutoCAD.Windows.OpenFileDialog
                ("Select drawings to Add.", "", "dwg", "", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.AllowMultiple);
            if (Dia.ShowDialog() != DialogResult.OK) return;
            Files = Dia.GetFilenames();

            Attach();
        }

        void Attach()
        {
            // Get the current database and start a transaction
            Database db;
            db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {

                foreach (string file in Files)
                {
                    ObjectId acXrefId = db.AttachXref(file, Path.GetFileName(file));

                    // If a valid reference is created then continue
                    if (!acXrefId.IsNull)
                    {
                        // Attach the DWG reference to the current space
                        Point3d insPt = new Point3d(1, 1, 0);
                        using (BlockReference acBlkRef = new BlockReference(insPt, acXrefId))
                        {
                            BlockTableRecord acBlkTblRec;
                            acBlkTblRec = acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                            acBlkTblRec.AppendEntity(acBlkRef);
                            acTrans.AddNewlyCreatedDBObject(acBlkRef, true);
                        }
                    }
                }


                // Save the new objects to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }
    }
}
