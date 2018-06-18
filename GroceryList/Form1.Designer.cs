namespace GroceryList
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
            this.addBtn = new System.Windows.Forms.Button();
            this.addedGroup = new System.Windows.Forms.GroupBox();
            this.selectableGroup = new System.Windows.Forms.GroupBox();
            this.selectablePanel = new System.Windows.Forms.Panel();
            this.removeBtn = new System.Windows.Forms.Button();
            this.selectableGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // addBtn
            // 
            this.addBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addBtn.Location = new System.Drawing.Point(305, 157);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 1;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            // 
            // addedGroup
            // 
            this.addedGroup.BackColor = System.Drawing.SystemColors.Window;
            this.addedGroup.Location = new System.Drawing.Point(386, 13);
            this.addedGroup.Name = "addedGroup";
            this.addedGroup.Size = new System.Drawing.Size(200, 333);
            this.addedGroup.TabIndex = 2;
            this.addedGroup.TabStop = false;
            this.addedGroup.Text = "My Groceries";
            // 
            // selectableGroup
            // 
            this.selectableGroup.BackColor = System.Drawing.SystemColors.Window;
            this.selectableGroup.Controls.Add(this.selectablePanel);
            this.selectableGroup.Location = new System.Drawing.Point(12, 13);
            this.selectableGroup.Name = "selectableGroup";
            this.selectableGroup.Size = new System.Drawing.Size(287, 333);
            this.selectableGroup.TabIndex = 3;
            this.selectableGroup.TabStop = false;
            this.selectableGroup.Text = "Products";
            // 
            // selectablePanel
            // 
            this.selectablePanel.AutoScroll = true;
            this.selectablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectablePanel.Location = new System.Drawing.Point(3, 16);
            this.selectablePanel.Name = "selectablePanel";
            this.selectablePanel.Size = new System.Drawing.Size(281, 314);
            this.selectablePanel.TabIndex = 0;
            // 
            // removeBtn
            // 
            this.removeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removeBtn.Location = new System.Drawing.Point(305, 186);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(75, 23);
            this.removeBtn.TabIndex = 4;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 358);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.selectableGroup);
            this.Controls.Add(this.addedGroup);
            this.Controls.Add(this.addBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.selectableGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.GroupBox addedGroup;
        private System.Windows.Forms.GroupBox selectableGroup;
        private System.Windows.Forms.Button removeBtn;
        private System.Windows.Forms.Panel selectablePanel;
    }
}

