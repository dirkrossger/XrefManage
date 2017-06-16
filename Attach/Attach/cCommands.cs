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

[assembly: CommandClass(typeof(Attach.Commands))]

namespace Attach
{
    public class Commands
    {
        private static DocumentCollection DocCol;

        [CommandMethod("attxref", CommandFlags.Session)]
        public void Main()
        {
            Database Db;
            DocCol = AcadApp.DocumentManager;

            using (DocumentLock DocLock = DocCol.MdiActiveDocument.LockDocument())
            {
                using (Form1 XrMan = new Form1())
                {
                    AcadApp.ShowModalDialog(XrMan);
                }
            }
        }

    }
    public class Attach : IExtensionApplication
    {
        private static Editor editor =
            Application.DocumentManager.MdiActiveDocument.Editor;

        public void Initialize()
        {
            editor.WriteMessage("\nXrefManage Start with attxref");
        }

        public void Terminate()
        {
        }
    }


}
