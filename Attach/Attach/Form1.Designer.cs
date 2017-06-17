namespace Attach
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
            this.button1_Add = new System.Windows.Forms.Button();
            this.DwgListview = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1_Add
            // 
            this.button1_Add.Location = new System.Drawing.Point(269, 32);
            this.button1_Add.Name = "button1_Add";
            this.button1_Add.Size = new System.Drawing.Size(75, 23);
            this.button1_Add.TabIndex = 0;
            this.button1_Add.Text = "Add";
            this.button1_Add.UseVisualStyleBackColor = true;
            this.button1_Add.Click += new System.EventHandler(this.button1_Add_Click);
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
            this.DwgListview.Location = new System.Drawing.Point(12, 32);
            this.DwgListview.MultiSelect = false;
            this.DwgListview.Name = "DwgListview";
            this.DwgListview.Size = new System.Drawing.Size(238, 147);
            this.DwgListview.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.DwgListview.TabIndex = 3;
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
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Drawing names";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 191);
            this.Controls.Add(this.DwgListview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1_Add);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1_Add;
        private System.Windows.Forms.ListView DwgListview;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
    }
}