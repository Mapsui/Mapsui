using Mapsui.UI.WinForms;

namespace Mapsui.Samples.WinForms
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

    
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            Mapsui.Map map1 = new Mapsui.Map();
            Mapsui.Styles.Color color1 = new Mapsui.Styles.Color();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.mapControl1 = new Mapsui.UI.WinForms.MapControl();
            this.SampleList = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(753, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(47, 44);
            this.button1.TabIndex = 1;
            this.button1.Text = "+";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(806, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(46, 44);
            this.button2.TabIndex = 2;
            this.button2.Text = "-";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // mapControl1
            // 
            this.mapControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapControl1.BackColor = System.Drawing.Color.White;
            this.mapControl1.Location = new System.Drawing.Point(199, 0);
            color1.A = 255;
            color1.B = 255;
            color1.G = 255;
            color1.R = 255;
            map1.BackColor = color1;
            map1.CRS = null;
            map1.HoverInfoLayers = ((System.Collections.Generic.IList<Mapsui.Layers.ILayer>)(resources.GetObject("map1.HoverInfoLayers")));
            map1.InfoLayers = ((System.Collections.Generic.IList<Mapsui.Layers.ILayer>)(resources.GetObject("map1.InfoLayers")));
            map1.Lock = false;
            map1.Transformation = null;
            this.mapControl1.Map = map1;
            this.mapControl1.Name = "mapControl1";
            this.mapControl1.Size = new System.Drawing.Size(665, 535);
            this.mapControl1.TabIndex = 3;
            this.mapControl1.Text = "mapControl1";
            // 
            // SampleList
            // 
            this.SampleList.AutoScroll = true;
            this.SampleList.BackColor = System.Drawing.Color.White;
            this.SampleList.Dock = System.Windows.Forms.DockStyle.Left;
            this.SampleList.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.SampleList.Location = new System.Drawing.Point(0, 0);
            this.SampleList.Name = "SampleList";
            this.SampleList.Size = new System.Drawing.Size(200, 535);
            this.SampleList.TabIndex = 5;
            this.SampleList.WrapContents = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 535);
            this.Controls.Add(this.SampleList);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.mapControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

    }

    
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private MapControl mapControl1;
        private System.Windows.Forms.FlowLayoutPanel SampleList;
    }
}

