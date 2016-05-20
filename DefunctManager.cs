using LiveSplit.Defunct.Memory;
using System;
using System.Threading;
using System.Windows.Forms;
namespace LiveSplit.Defunct {
	public partial class DefunctManager : Form {
		public DefunctMemory Memory { get; set; }
		public DefunctComponent Component { get; set; }
		public DefunctManager() {
			InitializeComponent();
			Visible = false;
			Thread t = new Thread(UpdateLoop);
			t.IsBackground = true;
			t.Start();
		}

		private void DefunctManager_FormClosing(object sender, FormClosingEventArgs e) {
			e.Cancel = Memory != null;
			if (e.Cancel && this.WindowState != FormWindowState.Minimized) {
				this.WindowState = FormWindowState.Minimized;
			}
		}

		private void UpdateLoop() {
			try {
				while (true) {
					try {
						UpdateValues();
					} catch { }
					Thread.Sleep(33);
				}
			} catch { }
		}
		public void UpdateValues() {
			if (this.InvokeRequired) {
				this.Invoke((Action)UpdateValues);
			} else if (this.Visible && Memory != null && Memory.HookProcess()) {
				string level = Memory.CurrentLevelName();
				float x = Memory.CurrentCPX();
				float y = Memory.CurrentCPY();
				lblCheckpoint.Text = string.IsNullOrEmpty(level) ? "" : Memory.CurrentCPName(x, y) + ": Power(" + Memory.CurrentCPStartStrength() + ")";
				Vector pos = Memory.CurrentPlayerPos();
				lblPos.Text = "Position: (" + pos.X.ToString("0.00") + ", " + pos.Z.ToString("0.00") + ", " + pos.Y.ToString("0.00") + ")";
				Vector cv = Memory.CurrentVelocity();
				lblVelocity.Text = "Velocity: " + cv.ToString();
				int[] collectibles = Memory.Collectibles();
				lblCollectibles.Text = "Collectibles: (" + collectibles[1] + "/" + collectibles[0] + ") (" + collectibles[2] + "/40)";
			} else if (Memory == null && this.Visible) {
				this.Hide();
			}
		}
	}
}