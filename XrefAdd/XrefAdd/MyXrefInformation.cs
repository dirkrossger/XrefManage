using System;

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
    }
}
