namespace atOpticalDecenter
{
    partial class InspectResultForm
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
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.vGridControl1 = new DevExpress.XtraVerticalGrid.VGridControl();
            this.categoryInspectionResult = new DevExpress.XtraVerticalGrid.Rows.CategoryRow();
            this.rowProductSeries = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.rowProductModelName = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.rowLedSpot1BlobSize = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.rowLedSpot2BlobSize = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.rowOpticalEccentricAngle = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.rowInspectResult = new DevExpress.XtraVerticalGrid.Rows.EditorRow();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vGridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.simpleButton1);
            this.layoutControl1.Controls.Add(this.vGridControl1);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;
            this.layoutControl1.Size = new System.Drawing.Size(398, 253);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // vGridControl1
            // 
            this.vGridControl1.Appearance.Category.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vGridControl1.Appearance.Category.Options.UseFont = true;
            this.vGridControl1.Appearance.ReadOnlyRecordValue.Font = new System.Drawing.Font("Tahoma", 14.25F);
            this.vGridControl1.Appearance.ReadOnlyRecordValue.Options.UseFont = true;
            this.vGridControl1.Appearance.ReadOnlyRow.Font = new System.Drawing.Font("Tahoma", 14.25F);
            this.vGridControl1.Appearance.ReadOnlyRow.Options.UseFont = true;
            this.vGridControl1.Appearance.RowHeaderPanel.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vGridControl1.Appearance.RowHeaderPanel.Options.UseFont = true;
            this.vGridControl1.Cursor = System.Windows.Forms.Cursors.SizeNS;
            this.vGridControl1.LayoutStyle = DevExpress.XtraVerticalGrid.LayoutViewStyle.SingleRecordView;
            this.vGridControl1.Location = new System.Drawing.Point(12, 12);
            this.vGridControl1.Name = "vGridControl1";
            this.vGridControl1.RecordWidth = 103;
            this.vGridControl1.RowHeaderWidth = 97;
            this.vGridControl1.Rows.AddRange(new DevExpress.XtraVerticalGrid.Rows.BaseRow[] {
            this.categoryInspectionResult});
            this.vGridControl1.Size = new System.Drawing.Size(374, 203);
            this.vGridControl1.TabIndex = 4;
            // 
            // categoryInspectionResult
            // 
            this.categoryInspectionResult.ChildRows.AddRange(new DevExpress.XtraVerticalGrid.Rows.BaseRow[] {
            this.rowProductSeries,
            this.rowProductModelName,
            this.rowLedSpot1BlobSize,
            this.rowLedSpot2BlobSize,
            this.rowOpticalEccentricAngle,
            this.rowInspectResult});
            this.categoryInspectionResult.Height = 31;
            this.categoryInspectionResult.Name = "categoryInspectionResult";
            this.categoryInspectionResult.Properties.Caption = "투광 검사 결과";
            // 
            // rowProductSeries
            // 
            this.rowProductSeries.Appearance.Options.UseFont = true;
            this.rowProductSeries.Appearance.Options.UseTextOptions = true;
            this.rowProductSeries.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowProductSeries.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowProductSeries.Name = "rowProductSeries";
            this.rowProductSeries.Properties.Caption = "제품";
            this.rowProductSeries.Properties.ReadOnly = true;
            // 
            // rowProductModelName
            // 
            this.rowProductModelName.Appearance.Options.UseFont = true;
            this.rowProductModelName.Appearance.Options.UseTextOptions = true;
            this.rowProductModelName.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowProductModelName.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowProductModelName.Name = "rowProductModelName";
            this.rowProductModelName.Properties.Caption = "제품 모델명";
            this.rowProductModelName.Properties.ReadOnly = true;
            // 
            // rowLedSpot1BlobSize
            // 
            this.rowLedSpot1BlobSize.Appearance.Options.UseFont = true;
            this.rowLedSpot1BlobSize.Appearance.Options.UseTextOptions = true;
            this.rowLedSpot1BlobSize.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowLedSpot1BlobSize.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowLedSpot1BlobSize.Name = "rowLedSpot1BlobSize";
            this.rowLedSpot1BlobSize.Properties.Caption = "Spot1 크기[mm]";
            this.rowLedSpot1BlobSize.Properties.ReadOnly = true;
            // 
            // rowLedSpot2BlobSize
            // 
            this.rowLedSpot2BlobSize.Appearance.Options.UseFont = true;
            this.rowLedSpot2BlobSize.Appearance.Options.UseTextOptions = true;
            this.rowLedSpot2BlobSize.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowLedSpot2BlobSize.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowLedSpot2BlobSize.Name = "rowLedSpot2BlobSize";
            this.rowLedSpot2BlobSize.Properties.Caption = "Spot2 크기[mm]";
            this.rowLedSpot2BlobSize.Properties.ReadOnly = true;
            // 
            // rowOpticalEccentricAngle
            // 
            this.rowOpticalEccentricAngle.Appearance.Options.UseFont = true;
            this.rowOpticalEccentricAngle.Appearance.Options.UseTextOptions = true;
            this.rowOpticalEccentricAngle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowOpticalEccentricAngle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowOpticalEccentricAngle.Name = "rowOpticalEccentricAngle";
            this.rowOpticalEccentricAngle.Properties.Caption = "편심 각도[˚]";
            this.rowOpticalEccentricAngle.Properties.ReadOnly = true;
            // 
            // rowInspectResult
            // 
            this.rowInspectResult.Appearance.Options.UseFont = true;
            this.rowInspectResult.Appearance.Options.UseTextOptions = true;
            this.rowInspectResult.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.rowInspectResult.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.rowInspectResult.Name = "rowInspectResult";
            this.rowInspectResult.Properties.Caption = "검사 결과";
            this.rowInspectResult.Properties.ReadOnly = true;
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(398, 253);
            this.Root.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.vGridControl1;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(378, 207);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // simpleButton1
            // 
            this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.simpleButton1.Location = new System.Drawing.Point(12, 219);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(374, 22);
            this.simpleButton1.StyleController = this.layoutControl1;
            this.simpleButton1.TabIndex = 5;
            this.simpleButton1.Text = "OK";
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.simpleButton1;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 207);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(378, 26);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // InspectResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 253);
            this.Controls.Add(this.layoutControl1);
            this.Name = "InspectResultForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inspection Result";
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vGridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraVerticalGrid.VGridControl vGridControl1;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowProductSeries;
        private DevExpress.XtraVerticalGrid.Rows.CategoryRow categoryInspectionResult;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowProductModelName;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowLedSpot1BlobSize;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowLedSpot2BlobSize;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowOpticalEccentricAngle;
        private DevExpress.XtraVerticalGrid.Rows.EditorRow rowInspectResult;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
    }
}