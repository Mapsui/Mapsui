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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Mapsui.Map map2 = new Mapsui.Map();
        Mapsui.Styles.Color color2 = new Mapsui.Styles.Color();
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.mapControl1 = new Mapsui.Forms.MapControl();
        this.SuspendLayout();
        // 
        // button1
        // 
        this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.button1.Location = new System.Drawing.Point(553, 12);
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
        this.button2.Location = new System.Drawing.Point(606, 12);
        this.button2.Name = "button2";
        this.button2.Size = new System.Drawing.Size(46, 44);
        this.button2.TabIndex = 2;
        this.button2.Text = "-";
        this.button2.UseVisualStyleBackColor = true;
        this.button2.Click += new System.EventHandler(this.button2_Click);
        // 
        // mapControl1
        // 
        this.mapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mapControl1.Location = new System.Drawing.Point(0, 0);
        color2.A = 255;
        color2.B = 255;
        color2.G = 255;
        color2.R = 255;
        this.mapControl1.Name = "mapControl1";
        this.mapControl1.Size = new System.Drawing.Size(664, 457);
        this.mapControl1.TabIndex = 3;
        this.mapControl1.Text = "mapControl1";
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(664, 457);
        this.Controls.Add(this.button2);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.mapControl1);
        this.Name = "Form1";
        this.Text = "Form1";
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private Mapsui.Forms.MapControl mapControl1;
  }
}

