namespace LiveSplit.Defunct {
	partial class DefunctManager {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefunctManager));
			this.lblCheckpoint = new System.Windows.Forms.Label();
			this.lblVelocity = new System.Windows.Forms.Label();
			this.lblPos = new System.Windows.Forms.Label();
			this.lblCollectibles = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblCheckpoint
			// 
			this.lblCheckpoint.AutoSize = true;
			this.lblCheckpoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCheckpoint.Location = new System.Drawing.Point(3, 2);
			this.lblCheckpoint.Name = "lblCheckpoint";
			this.lblCheckpoint.Size = new System.Drawing.Size(216, 20);
			this.lblCheckpoint.TabIndex = 4;
			this.lblCheckpoint.Text = "Checkpoint 1: 999.99, 999.99";
			// 
			// lblVelocity
			// 
			this.lblVelocity.AutoSize = true;
			this.lblVelocity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblVelocity.Location = new System.Drawing.Point(3, 42);
			this.lblVelocity.Name = "lblVelocity";
			this.lblVelocity.Size = new System.Drawing.Size(236, 20);
			this.lblVelocity.TabIndex = 5;
			this.lblVelocity.Text = "Velocity: (0.00) (0.00, 0.00, 0.00)";
			// 
			// lblPos
			// 
			this.lblPos.AutoSize = true;
			this.lblPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPos.Location = new System.Drawing.Point(3, 22);
			this.lblPos.Name = "lblPos";
			this.lblPos.Size = new System.Drawing.Size(153, 20);
			this.lblPos.TabIndex = 8;
			this.lblPos.Text = "Position: (0.00, 0.00)";
			// 
			// lblCollectibles
			// 
			this.lblCollectibles.AutoSize = true;
			this.lblCollectibles.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCollectibles.Location = new System.Drawing.Point(3, 62);
			this.lblCollectibles.Name = "lblCollectibles";
			this.lblCollectibles.Size = new System.Drawing.Size(174, 20);
			this.lblCollectibles.TabIndex = 9;
			this.lblCollectibles.Text = "Collectibles: (0/2) (0/40)";
			// 
			// DefunctManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(376, 86);
			this.Controls.Add(this.lblCollectibles);
			this.Controls.Add(this.lblPos);
			this.Controls.Add(this.lblVelocity);
			this.Controls.Add(this.lblCheckpoint);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "DefunctManager";
			this.Text = "Defunct Manager";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DefunctManager_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblCheckpoint;
		private System.Windows.Forms.Label lblVelocity;
		private System.Windows.Forms.Label lblPos;
		private System.Windows.Forms.Label lblCollectibles;
	}
}