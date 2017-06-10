namespace XrefAdd
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DwgListview = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.XrefListview = new System.Windows.Forms.ListView();
            this.cXrefName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.XrefPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CanBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button2_Detach = new System.Windows.Forms.Button();
            this.ViewTogBtn = new System.Windows.Forms.Button();
            this.SplitPnl = new System.Windows.Forms.SplitContainer();
            this.button1_Attach = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.SplitPnl)).BeginInit();
            this.SplitPnl.Panel1.SuspendLayout();
            this.SplitPnl.Panel2.SuspendLayout();
            this.SplitPnl.SuspendLayout();
            this.SuspendLayout();
            // 
            // DwgListview
            // 
            this.DwgListview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DwgListview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.DwgListview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.DwgListview.HideSelection = false;
            this.DwgListview.LabelWrap = false;
            this.DwgListview.Location = new System.Drawing.Point(12, 34);
            this.DwgListview.Name = "DwgListview";
            this.DwgListview.Size = new System.Drawing.Size(154, 400);
            this.DwgListview.TabIndex = 0;
            this.DwgListview.UseCompatibleStateImageBehavior = false;
            this.DwgListview.View = System.Windows.Forms.View.Details;
            this.DwgListview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DrawingSelected);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 117;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Drawing names";
            // 
            // XrefListview
            // 
            this.XrefListview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XrefListview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cXrefName,
            this.XrefPath});
            this.XrefListview.GridLines = true;
            this.XrefListview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.XrefListview.HideSelection = false;
            this.XrefListview.Location = new System.Drawing.Point(3, 34);
            this.XrefListview.Name = "XrefListview";
            this.XrefListview.Size = new System.Drawing.Size(905, 306);
            this.XrefListview.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.XrefListview.TabIndex = 1;
            this.XrefListview.UseCompatibleStateImageBehavior = false;
            this.XrefListview.View = System.Windows.Forms.View.Details;
            // 
            // cXrefName
            // 
            this.cXrefName.Text = "Current Xref Name";
            this.cXrefName.Width = 150;
            // 
            // XrefPath
            // 
            this.XrefPath.Text = "Xref Path";
            this.XrefPath.Width = 415;
            // 
            // CanBtn
            // 
            this.CanBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CanBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CanBtn.Location = new System.Drawing.Point(840, 420);
            this.CanBtn.Name = "CanBtn";
            this.CanBtn.Size = new System.Drawing.Size(68, 25);
            this.CanBtn.TabIndex = 3;
            this.CanBtn.Text = "Cancel";
            this.CanBtn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Xref information";
            // 
            // button2_Detach
            // 
            this.button2_Detach.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2_Detach.Location = new System.Drawing.Point(841, 386);
            this.button2_Detach.Name = "button2_Detach";
            this.button2_Detach.Size = new System.Drawing.Size(67, 25);
            this.button2_Detach.TabIndex = 3;
            this.button2_Detach.Text = "Detach";
            this.button2_Detach.UseVisualStyleBackColor = true;
            this.button2_Detach.Click += new System.EventHandler(this.button2_Detach_Click);
            // 
            // ViewTogBtn
            // 
            this.ViewTogBtn.Location = new System.Drawing.Point(102, 4);
            this.ViewTogBtn.Name = "ViewTogBtn";
            this.ViewTogBtn.Size = new System.Drawing.Size(119, 23);
            this.ViewTogBtn.TabIndex = 3;
            this.ViewTogBtn.Text = "Show Per Xref";
            this.ViewTogBtn.UseVisualStyleBackColor = true;
            // 
            // SplitPnl
            // 
            this.SplitPnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitPnl.Location = new System.Drawing.Point(0, 0);
            this.SplitPnl.Name = "SplitPnl";
            // 
            // SplitPnl.Panel1
            // 
            this.SplitPnl.Panel1.Controls.Add(this.DwgListview);
            this.SplitPnl.Panel1.Controls.Add(this.label1);
            // 
            // SplitPnl.Panel2
            // 
            this.SplitPnl.Panel2.Controls.Add(this.CanBtn);
            this.SplitPnl.Panel2.Controls.Add(this.button1_Attach);
            this.SplitPnl.Panel2.Controls.Add(this.label2);
            this.SplitPnl.Panel2.Controls.Add(this.button2_Detach);
            this.SplitPnl.Panel2.Controls.Add(this.ViewTogBtn);
            this.SplitPnl.Panel2.Controls.Add(this.XrefListview);
            this.SplitPnl.Size = new System.Drawing.Size(1092, 453);
            this.SplitPnl.SplitterDistance = 169;
            this.SplitPnl.TabIndex = 6;
            // 
            // button1_Attach
            // 
            this.button1_Attach.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1_Attach.Location = new System.Drawing.Point(840, 354);
            this.button1_Attach.Name = "button1_Attach";
            this.button1_Attach.Size = new System.Drawing.Size(67, 26);
            this.button1_Attach.TabIndex = 14;
            this.button1_Attach.Text = "Attach";
            this.button1_Attach.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 453);
            this.Controls.Add(this.SplitPnl);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SplitPnl.Panel1.ResumeLayout(false);
            this.SplitPnl.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitPnl)).EndInit();
            this.SplitPnl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView DwgListview;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView XrefListview;
        private System.Windows.Forms.ColumnHeader cXrefName;
        private System.Windows.Forms.ColumnHeader XrefPath;
        private System.Windows.Forms.Button CanBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2_Detach;
        private System.Windows.Forms.Button ViewTogBtn;
        private System.Windows.Forms.SplitContainer SplitPnl;
        private System.Windows.Forms.Button button1_Attach;
    }
}