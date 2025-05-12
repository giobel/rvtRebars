namespace rvtRebars
{
	partial class Form1
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.uid = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.btnIsolate = new System.Windows.Forms.Button();
			this.btnRenumber = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(12, 36);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(248, 21);
			this.comboBox1.TabIndex = 0;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1SelectedIndexChanged);
			// 
			// uid
			// 
			this.uid.Location = new System.Drawing.Point(12, 10);
			this.uid.Name = "uid";
			this.uid.Size = new System.Drawing.Size(151, 23);
			this.uid.TabIndex = 1;
			this.uid.Text = "Segment Unique ID";
			this.uid.UseCompatibleTextRendering = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 77);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(151, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Segment Unique ID";
			this.label1.UseCompatibleTextRendering = true;

			// 
			// comboBox2
			// 
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Location = new System.Drawing.Point(12, 103);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(248, 21);
			this.comboBox2.TabIndex = 2;
			// 
			// btnIsolate
			// 
			this.btnIsolate.Location = new System.Drawing.Point(12, 184);
			this.btnIsolate.Name = "btnIsolate";
			this.btnIsolate.Size = new System.Drawing.Size(59, 23);
			this.btnIsolate.TabIndex = 4;
			this.btnIsolate.Text = "Isolate";
			this.btnIsolate.UseCompatibleTextRendering = true;
			this.btnIsolate.UseVisualStyleBackColor = true;
			// 
			// btnRenumber
			// 
			this.btnRenumber.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnRenumber.Location = new System.Drawing.Point(86, 184);
			this.btnRenumber.Name = "btnRenumber";
			this.btnRenumber.Size = new System.Drawing.Size(104, 23);
			this.btnRenumber.TabIndex = 5;
			this.btnRenumber.Text = "Renumber Layers";
			this.btnRenumber.UseCompatibleTextRendering = true;
			this.btnRenumber.UseVisualStyleBackColor = true;
			this.btnRenumber.Click += new System.EventHandler(this.Button1Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(203, 184);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseCompatibleTextRendering = true;
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnRenumber);
			this.Controls.Add(this.btnIsolate);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.uid);
			this.Controls.Add(this.comboBox1);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Invert Slices";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnRenumber;
		private System.Windows.Forms.Button btnIsolate;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label uid;
		private System.Windows.Forms.ComboBox comboBox1;

		
		void Button1Click(object sender, System.EventArgs e)
		{
			
		}

	}
}
