﻿using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Defunct {
	public class SplitterComponent : IComponent {
		public string ComponentName { get { return "Defunct Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private SplitterMemory mem;
		private int currentSplit, state, lastLogCheck, platinumCount;
		private bool hasLog = false;
		private float lastTime = 0;
		internal static string[] keys = { "CurrentSplit", "State", "SceneName", "SceneToLoad", "IsArcade", "PlatniumCount", "ArcadeTimer", "TimerOn", "IsPaused" };
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private DefunctManager manager;

		public SplitterComponent(LiveSplitState state) {
			mem = new SplitterMemory();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
			}

			manager = new DefunctManager();
			manager.Memory = mem;
			manager.Show();
			manager.Visible = false;
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;

			Vector pos = mem.CurrentPlayerPos();
			float y = pos.Y;
			string sceneToLoad = mem.SceneToLoad();
			string currentScene = mem.CurrentSceneName();
			if (currentSplit == 0) {
				if (state == 0 && sceneToLoad == "Menu_RA") {
					state++;
				} else if (state == 1 && currentScene == "Cargo_Ship_01" && y == 0) {
					state++;
				} else if (state == 2 && currentScene == "Cargo_Ship_01" && y < -1400) {
					shouldSplit = true;
				}
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				if (Model.CurrentState.Run.Count <= 11) {
					switch (currentSplit) {
						case 1: shouldSplit = currentScene == "BadGrasslands_01" && y >= 125 && y < 130; break;
						case 2: shouldSplit = currentScene == "GoodGrasslands_01" && y >= 1941 && y < 2000; break;
						case 3: shouldSplit = (currentScene == "GoodGrasslands_01" || currentScene == "Forest_01") && y >= 5886 && y < 5940; break;
						case 4: shouldSplit = currentScene == "Slope_01" && y >= 10078 && y < 10130; break;
						case 5: shouldSplit = currentScene == "Wasteland_01" && y >= -304 && y < -295; break;
						case 6: shouldSplit = y >= 5741; break;
						case 7: shouldSplit = currentScene == "Wasteland_01_Race_01" && y >= 748 && y < 770; break;
						case 8: shouldSplit = currentScene == "Wasteland_01_Oasis_01" && y >= 8150 && y < 8200; break;
						case 9: shouldSplit = currentScene == "Ravine_01" && y >= 137 && y < 190; break;
						case 10: shouldSplit = currentScene == "Finale_AlienShip_02" && y >= 3967 && y < 4010; break;
						case 11: shouldSplit = currentScene == "Finale_AlienShip_02" && y >= 9933; break;
					}
				} else {
					mem.UnlockAllLevels();
					if (sceneToLoad == "Menu_RA") {
						mem.ResetTimer();
					}

					if (currentScene == "Cargo_Ship_01" && currentSplit == 2) {
						mem.AllowPause();
						mem.UnlockLost();
					}

					switch (currentSplit) {
						case 1:
							shouldSplit = pos.X >= 895 && pos.X <= 915 && y >= 1153 && y <= 1165 && pos.Z >= -8 && pos.Z <= 8;
							break;
						default:
							float currentTime = mem.CurrentArcadeTime();
							Model.CurrentState.IsGameTimePaused = currentTime == lastTime || sceneToLoad == "Menu_RA" || !mem.IsArcadePlay() || !mem.TimerOn();

							int currentPlatnium = mem.PlatinumCount();
							shouldSplit = currentPlatnium > 1 && currentPlatnium != platinumCount;
							platinumCount = currentPlatnium;

							lastTime = currentTime;
							break;
					}
				}
			}

			HandleSplit(shouldSplit);
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

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = string.Empty, curr = string.Empty;
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						case "SceneName": curr = mem.CurrentSceneName(); break;
						case "SceneToLoad": curr = mem.SceneToLoad(); break;
						case "IsArcade": curr = mem.IsArcadePlay().ToString(); break;
						case "PlatniumCount": curr = mem.PlatinumCount().ToString(); break;
						case "ArcadeTimer": curr = mem.CurrentArcadeTime().ToString("0"); break;
						case "TimerOn": curr = mem.TimerOn().ToString(); break;
						default: curr = ""; break;
					}

					if (string.IsNullOrEmpty(prev)) { prev = string.Empty; }
					if (string.IsNullOrEmpty(curr)) { curr = string.Empty; }
					if (!prev.Equals(curr)) {
						WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}

		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			//Remove duplicate autosplitter componenets
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			bool hasAutosplitter = false;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplitterComponent) {
					if ((invalidator == null && width == 0 && height == 0) || hasAutosplitter) {
						components.Remove(component);
					}
					hasAutosplitter = true;
				}
			}

			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = 0;
			state = 0;
			platinumCount = 1;
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
			Model.CurrentState.IsGameTimePaused = false;
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
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) + ": CurrentSplit: " + currentSplit.ToString().PadLeft(24, ' '));
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (Console.IsOutputRedirected) {
					using (StreamWriter wr = new StreamWriter("_Defunct.log", true)) {
						wr.WriteLine(data);
					}
				} else {
					Console.WriteLine(data);
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
			if(Model != null) {
				Model.CurrentState.OnReset -= OnReset;
				Model.CurrentState.OnPause -= OnPause;
				Model.CurrentState.OnResume -= OnResume;
				Model.CurrentState.OnStart -= OnStart;
				Model.CurrentState.OnSplit -= OnSplit;
				Model.CurrentState.OnUndoSplit -= OnUndoSplit;
				Model.CurrentState.OnSkipSplit -= OnSkipSplit;
			}
		}
	}
}