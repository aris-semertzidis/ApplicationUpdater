namespace ApplicationUpdater.WinForms;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        label1 = new Label();
        progressBar = new ProgressBar();
        progressLabel = new Label();
        timer1 = new System.Windows.Forms.Timer(components);
        SuspendLayout();
        // 
        // label1
        // 
        label1.Dock = DockStyle.Top;
        label1.Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label1.ForeColor = Color.White;
        label1.Location = new Point(0, 0);
        label1.Name = "label1";
        label1.Size = new Size(800, 68);
        label1.TabIndex = 0;
        label1.Text = "Updating Application";
        label1.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(103, 262);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(632, 16);
        progressBar.TabIndex = 1;
        // 
        // progressLabel
        // 
        progressLabel.Font = new Font("Arial", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
        progressLabel.ForeColor = Color.White;
        progressLabel.Location = new Point(0, 294);
        progressLabel.Name = "progressLabel";
        progressLabel.Size = new Size(800, 68);
        progressLabel.TabIndex = 2;
        progressLabel.Text = "Progress....";
        progressLabel.TextAlign = ContentAlignment.TopCenter;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(64, 64, 64);
        ClientSize = new Size(800, 450);
        Controls.Add(progressLabel);
        Controls.Add(progressBar);
        Controls.Add(label1);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Application Updater";
        ResumeLayout(false);
    }

    #endregion

    private Label label1;
    private ProgressBar progressBar;
    private Label progressLabel;
    private System.Windows.Forms.Timer timer1;
}
