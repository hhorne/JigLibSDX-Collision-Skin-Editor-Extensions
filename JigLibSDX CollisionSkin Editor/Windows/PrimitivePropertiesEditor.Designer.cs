namespace JigLibSDX_CSE
{
    partial class PrimitivePropertiesEditor
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrimitivePropertiesEditor));
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.massTypeCombo = new System.Windows.Forms.ComboBox();
            this.massTypeLabel = new System.Windows.Forms.Label();
            this.massDistributionLabel = new System.Windows.Forms.Label();
            this.massDistributionCombo = new System.Windows.Forms.ComboBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.valueBox = new System.Windows.Forms.TextBox();
            this.separationHelp = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(97, 109);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 22);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(178, 109);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 22);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // massTypeCombo
            // 
            this.massTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.massTypeCombo.FormattingEnabled = true;
            this.massTypeCombo.Location = new System.Drawing.Point(108, 12);
            this.massTypeCombo.Name = "massTypeCombo";
            this.massTypeCombo.Size = new System.Drawing.Size(146, 21);
            this.massTypeCombo.TabIndex = 2;
            this.massTypeCombo.SelectedIndexChanged += new System.EventHandler(this.massTypeCombo_SelectedIndexChanged);
            // 
            // massTypeLabel
            // 
            this.massTypeLabel.AutoSize = true;
            this.massTypeLabel.Location = new System.Drawing.Point(12, 15);
            this.massTypeLabel.Name = "massTypeLabel";
            this.massTypeLabel.Size = new System.Drawing.Size(62, 13);
            this.massTypeLabel.TabIndex = 3;
            this.massTypeLabel.Text = "Mass Type:";
            // 
            // massDistributionLabel
            // 
            this.massDistributionLabel.AutoSize = true;
            this.massDistributionLabel.Location = new System.Drawing.Point(12, 42);
            this.massDistributionLabel.Name = "massDistributionLabel";
            this.massDistributionLabel.Size = new System.Drawing.Size(90, 13);
            this.massDistributionLabel.TabIndex = 5;
            this.massDistributionLabel.Text = "Mass Distribution:";
            // 
            // massDistributionCombo
            // 
            this.massDistributionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.massDistributionCombo.FormattingEnabled = true;
            this.massDistributionCombo.Location = new System.Drawing.Point(108, 39);
            this.massDistributionCombo.Name = "massDistributionCombo";
            this.massDistributionCombo.Size = new System.Drawing.Size(146, 21);
            this.massDistributionCombo.TabIndex = 4;
            this.massDistributionCombo.SelectedIndexChanged += new System.EventHandler(this.massDistributionCombo_SelectedIndexChanged);
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(12, 69);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(37, 13);
            this.valueLabel.TabIndex = 6;
            this.valueLabel.Text = "Value:";
            // 
            // valueBox
            // 
            this.valueBox.Location = new System.Drawing.Point(108, 66);
            this.valueBox.Name = "valueBox";
            this.valueBox.Size = new System.Drawing.Size(146, 20);
            this.valueBox.TabIndex = 7;
            this.valueBox.TextChanged += new System.EventHandler(this.valueBox_TextChanged);
            this.valueBox.Leave += new System.EventHandler(this.valueBox_Leave);
            // 
            // separationHelp
            // 
            this.separationHelp.AutoSize = true;
            this.separationHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.separationHelp.Location = new System.Drawing.Point(109, 85);
            this.separationHelp.Name = "separationHelp";
            this.separationHelp.Size = new System.Drawing.Size(145, 12);
            this.separationHelp.TabIndex = 8;
            this.separationHelp.Text = "Decimal place separator: Comma.";
            // 
            // PrimitivePropertiesEditor
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(265, 140);
            this.Controls.Add(this.valueBox);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.massDistributionLabel);
            this.Controls.Add(this.massDistributionCombo);
            this.Controls.Add(this.massTypeLabel);
            this.Controls.Add(this.massTypeCombo);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.separationHelp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrimitivePropertiesEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Primitive Properties Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ComboBox massTypeCombo;
        private System.Windows.Forms.Label massTypeLabel;
        private System.Windows.Forms.Label massDistributionLabel;
        private System.Windows.Forms.ComboBox massDistributionCombo;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.TextBox valueBox;
        private System.Windows.Forms.Label separationHelp;
    }
}