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
				lblLevel.Text = string.IsNullOrEmpty(level) ? "" : level + " - " + Memory.CurrentSceneName();
				float x = Memory.CurrentCPX();
				float y = Memory.CurrentCPY();
				lblCheckpoint.Text = string.IsNullOrEmpty(level) ? "" : Memory.CurrentCPName(x, y) + ": Power(" + Memory.CurrentCPStartStrength() + ")";
				x = Memory.CurrentPlayerX();
				y = Memory.CurrentPlayerY();
				lblPos.Text = "Position: (" + x.ToString("0.00") + ", " + y.ToString("0.00") + ")";
				float cv = Memory.CurrentVelocity();
				lblVelocity.Text = "Velocity: (" + cv.ToString("0.00") + ")";
			} else if (Memory == null && this.Visible) {
				this.Hide();
			}
		}
	}
}