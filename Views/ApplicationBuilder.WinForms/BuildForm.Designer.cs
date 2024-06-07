namespace ApplicationBuilder.WinForms;

partial class BuildForm
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
        buildButton = new Button();
        localBuildTextBox = new RichTextBox();
        label1 = new Label();
        progressBar1 = new ProgressBar();
        statusLabel = new Label();
        label3 = new Label();
        workingDirectoryTextBox = new RichTextBox();
        SuspendLayout();
        // 
        // buildButton
        // 
        buildButton.Location = new Point(344, 146);
        buildButton.Name = "buildButton";
        buildButton.Size = new Size(104, 37);
        buildButton.TabIndex = 0;
        buildButton.Text = "Build";
        buildButton.UseVisualStyleBackColor = true;
        buildButton.Click += buildButton_Click;
        // 
        // localBuildTextBox
        // 
        localBuildTextBox.BackColor = Color.FromArgb(64, 64, 64);
        localBuildTextBox.ForeColor = Color.White;
        localBuildTextBox.Location = new Point(174, 12);
        localBuildTextBox.Name = "localBuildTextBox";
        localBuildTextBox.Size = new Size(543, 45);
        localBuildTextBox.TabIndex = 1;
        localBuildTextBox.Text = "";
        // 
        // label1
        // 
        label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label1.ForeColor = Color.White;
        label1.Location = new Point(12, 15);
        label1.Name = "label1";
        label1.Size = new Size(156, 42);
        label1.TabIndex = 2;
        label1.Text = "Local Build Path";
        // 
        // progressBar1
        // 
        progressBar1.Location = new Point(85, 214);
        progressBar1.Name = "progressBar1";
        progressBar1.Size = new Size(632, 20);
        progressBar1.TabIndex = 3;
        // 
        // statusLabel
        // 
        statusLabel.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
        statusLabel.ForeColor = Color.White;
        statusLabel.Location = new Point(85, 255);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(632, 49);
        statusLabel.TabIndex = 4;
        statusLabel.Text = "Progress...";
        statusLabel.TextAlign = ContentAlignment.TopCenter;
        // 
        // label3
        // 
        label3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label3.ForeColor = Color.White;
        label3.Location = new Point(12, 76);
        label3.Name = "label3";
        label3.Size = new Size(146, 65);
        label3.TabIndex = 5;
        label3.Text = "FTP Working Directory";
        // 
        // workingDirectoryTextBox
        // 
        workingDirectoryTextBox.BackColor = Color.FromArgb(64, 64, 64);
        workingDirectoryTextBox.ForeColor = Color.White;
        workingDirectoryTextBox.Location = new Point(174, 76);
        workingDirectoryTextBox.Name = "workingDirectoryTextBox";
        workingDirectoryTextBox.Size = new Size(543, 45);
        workingDirectoryTextBox.TabIndex = 6;
        workingDirectoryTextBox.Text = "";
        // 
        // BuildForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(64, 64, 64);
        ClientSize = new Size(800, 312);
        Controls.Add(workingDirectoryTextBox);
        Controls.Add(label3);
        Controls.Add(statusLabel);
        Controls.Add(progressBar1);
        Controls.Add(label1);
        Controls.Add(localBuildTextBox);
        Controls.Add(buildButton);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        MaximizeBox = false;
        Name = "BuildForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Application Builder";
        ResumeLayout(false);
    }

    #endregion

    private Button buildButton;
    private RichTextBox localBuildTextBox;
    private Label label1;
    private ProgressBar progressBar1;
    private Label statusLabel;
    private Label label3;
    private RichTextBox workingDirectoryTextBox;
}
