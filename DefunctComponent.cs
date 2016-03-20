using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Defunct.Memory;
namespace LiveSplit.Defunct {
	public class DefunctComponent : IComponent {
		public string ComponentName { get { return "Defunct Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private DefunctMemory mem;
		private int currentSplit = 0;
		private int state = 0;
		private bool hasLog = false;
		private int lastLogCheck = 0;
		internal static string[] keys = { "CurrentSplit", "State" };
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private DefunctManager manager;

		public DefunctComponent() {
			mem = new DefunctMemory();
			foreach (string key in keys) {
				currentValues[key] = "";
			}
			manager = new DefunctManager();
			manager.Memory = mem;
			manager.Component = this;
			manager.Show();
			manager.Visible = false;
		}

		public void GetValues() {
			if (!mem.HookProcess()) {
				if (manager.Visible) { manager.Invoke((Action)delegate () { manager.Hide(); }); }
				return;
			} else if (!manager.Visible) {
				manager.Invoke((Action)delegate () { manager.Show(); });
			}

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;

			if (currentSplit == 0) {
				shouldSplit = mem.CurrentSceneName() == "Cargo_Ship_01";
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				float y = 0;
				switch (currentSplit) {
					case 1:
						if (state == 0 && mem.CurrentSceneName() == "BadGrasslands_01") {
							state++;
						} else if (state == 1) {
							shouldSplit = mem.CurrentPlayerX() != 0;
						}
						break;
					case 2: shouldSplit = mem.CurrentSceneName() == "GoodGrasslands_01" && mem.CurrentPlayerY() >= 1941; break;
					case 3: shouldSplit = mem.CurrentSceneName() == "Forest_01" && mem.CurrentPlayerY() >= 5886; break;
					case 4: shouldSplit = mem.CurrentSceneName() == "Slope_01" && mem.CurrentPlayerY() >= 10078; break;
					case 5: shouldSplit = mem.CurrentSceneName() == "Wasteland_01" && mem.CurrentPlayerY() >= -304; break;
					case 6: shouldSplit = mem.CurrentPlayerY() >= 5741; break;
					case 7:
						y = mem.CurrentPlayerY();
						shouldSplit = mem.CurrentSceneName() == "Wasteland_01_Race_01" && y >= 748 && y < 770; break;
					case 8: shouldSplit = mem.CurrentSceneName() == "Wasteland_01_Oasis_01" && mem.CurrentPlayerY() >= 8150; break;
					case 9:
						y = mem.CurrentPlayerY();
						shouldSplit = mem.CurrentSceneName() == "Ravine_01" && y >= 137 && y < 190; break;
					case 10: shouldSplit = mem.CurrentSceneName() == "Finale_AlienShip_02" && mem.CurrentPlayerY() >= 3967; break;
					case 11: shouldSplit = mem.CurrentSceneName() == "Finale_AlienShip_02" && mem.CurrentPlayerY() >= 9934; break;
				}
			}

			HandleSplit(shouldSplit, currentSplit > 0 && string.IsNullOrEmpty(mem.CurrentLevelName()));
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (currentSplit > 0 && shouldReset) {
				Model.Reset();
			} else if (shouldSplit) {
				if (currentSplit == 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}

		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists("_Defunct.log");
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog) {
				string prev = "", curr = "";
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						default: curr = ""; break;
					}

					if (!prev.Equals(curr)) {
						WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}

		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			if (Model == null) {
				Model = new TimerModel() { CurrentState = lvstate };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				lvstate.OnReset += OnReset;
				lvstate.OnPause += OnPause;
				lvstate.OnResume += OnResume;
				lvstate.OnStart += OnStart;
				lvstate.OnSplit += OnSplit;
				lvstate.OnUndoSplit += OnUndoSplit;
				lvstate.OnSkipSplit += OnSkipSplit;
			}

			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = 0;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			state = 0;
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) + ": CurrentSplit: " + currentSplit.ToString().PadLeft(24, ' '));
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) + ": CurrentSplit: " + currentSplit.ToString().PadLeft(24, ' '));
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) + ": CurrentSplit: " + currentSplit.ToString().PadLeft(24, ' '));
		}
		private void WriteLog(string data) {
			if (hasLog) {
				Console.WriteLine(data);
				using (StreamWriter wr = new StreamWriter("_Defunct.log", true)) {
					wr.WriteLine(data);
				}
			}
		}

		public Control GetSettingsControl(LayoutMode mode) { return null; }
		public void SetSettings(XmlNode settings) { }
		public XmlNode GetSettings(XmlDocument document) { return document.CreateElement("Settings"); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() {
			manager.Memory = null;
			manager.Close();
			manager.Dispose();
			mem.Dispose();
		}
	}
}