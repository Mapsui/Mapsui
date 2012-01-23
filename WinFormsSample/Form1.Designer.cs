namespace WinFormsSample
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
            this.mapImage1 = new SharpMap.Forms.MapImage();
            ((System.ComponentModel.ISupportInitialize)(this.mapImage1)).BeginInit();
            this.SuspendLayout();
            // 
            // mapImage1
            // 
            this.mapImage1.ActiveTool = SharpMap.Forms.MapImage.Tools.Pan;
            this.mapImage1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.mapImage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapImage1.FineZoomFactor = 10;
            this.mapImage1.Location = new System.Drawing.Point(0, 0);
            this.mapImage1.Name = "mapImage1";
            this.mapImage1.QueryLayerIndex = 0;
            this.mapImage1.Size = new System.Drawing.Size(665, 477);
            this.mapImage1.TabIndex = 0;
            this.mapImage1.TabStop = false;
            this.mapImage1.WheelZoomMagnitude = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 477);
            this.Controls.Add(this.mapImage1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.mapImage1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private SharpMap.Forms.MapImage mapImage1;
    }
}

