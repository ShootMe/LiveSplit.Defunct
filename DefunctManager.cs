using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
namespace LiveSplit.Defunct {
	public partial class DefunctManager : Form {
		public SplitterMemory Memory { get; set; }
		private Thread getValuesThread = null;
		public DefunctManager() {
			InitializeComponent();
			Text = "Defunct Manager " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			getValuesThread = new Thread(UpdateLoop);
			getValuesThread.IsBackground = true;
			getValuesThread.Start();
		}

		private void DefunctManager_FormClosing(object sender, FormClosingEventArgs e) {
			e.Cancel = Memory != null;
			if (e.Cancel) {
				if (this.WindowState != FormWindowState.Minimized) {
					this.WindowState = FormWindowState.Minimized;
				}
			} else if (getValuesThread != null) {
				getValuesThread = null;
			}
		}
		private void UpdateLoop() {
			while (getValuesThread != null) {
				try {
					UpdateValues();
					Thread.Sleep(30);
				} catch { }
			}
		}
		public void UpdateValues() {
			if (this.InvokeRequired) {
				this.Invoke((Action)UpdateValues);
			} else if (Memory != null && Memory.HookProcess()) {
				if (!Visible) { this.Show(); }

				string level = Memory.CurrentLevelName();
				Vector cp = Memory.CurrentCP();
				lblCheckpoint.Text = string.IsNullOrEmpty(level) ? "" : Memory.CurrentCPName(level, cp.X, cp.Y) + ": Power(" + cp.M + ")";
				Vector pos = Memory.CurrentPlayerPos();
				lblPos.Text = "Position: (" + pos.X.ToString("0.00") + ", " + pos.Z.ToString("0.00") + ", " + pos.Y.ToString("0.00") + ")";
				lblVelocity.Text = "Velocity: " + Memory.CurrentVelocity().ToString();
				int[] collectibles = Memory.Collectibles();
				lblCollectibles.Text = "Collectibles: (" + collectibles[1] + "/" + collectibles[0] + ") (" + collectibles[2] + "/40)";
			} else if (this.Visible) {
				this.Hide();
			}
		}
	}
}