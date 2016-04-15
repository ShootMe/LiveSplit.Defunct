using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace LiveSplit.Defunct.Memory {
	public class DefunctMemory {
		private ProgramPointer currentActiveCp, currentAreas, levelHandler, preloadLevel, playerHandler, sceneToLoad, saveMain;
		private float[] junkX = { 992.36f, 725.25f, 540.18f, 1513.69f, 1082.25f, 422.82f, 1494.81f, 692.56f, 1559.98f };
		private float[] junkY = { 6039.27f, 6288.26f, 6407.79f, 6441.79f, 6484.88f, 6676.19f, 6999.13f, 7093.40f, 7325.27f };
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;

		public DefunctMemory() {
			currentActiveCp = new ProgramPointer(this, "CurrentActiveCp") { IsStatic = false };
			currentAreas = new ProgramPointer(this, "CurrentAreas") { IsStatic = false };
			levelHandler = new ProgramPointer(this, "LevelHandler");
			preloadLevel = new ProgramPointer(this, "PreloadLevel") { IsStatic = false };
			sceneToLoad = new ProgramPointer(this, "SceneToLoad") { IsStatic = false };
			playerHandler = new ProgramPointer(this, "PlayerHandler") { IsStatic = false };
			saveMain = new ProgramPointer(this, "SaveHandlerMain") { IsStatic = false };
		}
		public float CurrentCPX() {
			//CheckPointSystem.instance.currentActiveCp.midPos.X
			return currentActiveCp.Read<float>(0x00, 0x78);
		}
		public void UnlockAllLevels() {
			if (saveMain.Value != IntPtr.Zero) {
				int size = saveMain.Read<int>(0x00, 0x10, 0x0c);
				for (int i = 0; i < size; i++) {
					IntPtr level = saveMain.Read<IntPtr>(0x00, 0x10, 0x10 + i * 4);
					IntPtr nextLevel = saveMain.Read<IntPtr>(0x00, 0x10, 0x10 + (i + 1 < size ? i + 1 : i) * 4);
					bool unlocked = Program.Read<bool>(level, 0x14);
					bool completed = Program.Read<bool>(level, 0x15);
					bool nextUnlocked = Program.Read<bool>(nextLevel, 0x14);
					bool nextCompleted = Program.Read<bool>(nextLevel, 0x15);
					if ((unlocked ^ completed) || (!(unlocked & completed) && (nextUnlocked | nextCompleted))) {
						Program.Write<int>(level, 16843009, 0x14);
					}
				}
			}
		}
		public int[] Collectibles() {
			int[] stats = new int[3] { 0, 0, 0 };
			if (saveMain.Value != IntPtr.Zero && preloadLevel.Value != IntPtr.Zero) {
				int lvlIndex = preloadLevel.Read<int>();
				int size = saveMain.Read<int>(0x00, 0x10, 0x0c);
				int totalCount = 0;
				
				if (lvlIndex >= 0) {
					stats[0] = levelHandler.Read<int>(0x10, 0x08, 0x10 + (4 * lvlIndex), 0x1c);
				}

				if (lvlIndex == 5) {
					IntPtr areas = currentAreas.Read<IntPtr>(0x00);
					int areaSize = Program.Read<int>(areas, 0x0c);
					if (areaSize > 0) {
						IntPtr cps = Program.Read<IntPtr>(areas, 0x08, 0x10 + (areaSize - 1) * 4, 0x10);
						areaSize = Program.Read<int>(cps, 0x0c);
						if (areaSize == 9) { lvlIndex = 6; }
					}
				}

				for (int i = 0; i < size; i++) {
					IntPtr level = saveMain.Read<IntPtr>(0x00, 0x10, 0x10 + i * 4);
					int length = Program.Read<int>(level, 0x08, 0x0c);
					int count = 0;
					for (int j = 0; j < length; j++) {
						if (Program.Read<bool>(level, 0x08, 0x10 + j)) {
							count++;
							totalCount++;
						}
					}
					if (i == lvlIndex) {
						stats[1] = count;
					}
				}
				stats[2] = totalCount;
			}
			return stats;
		}
		public float CurrentCPY() {
			//CheckPointSystem.instance.currentActiveCp.midPos.Y
			return currentActiveCp.Read<float>(0x00, 0x7c);
		}
		public float CurrentCPStartStrength() {
			//CheckPointSystem.instance.currentActiveCp.playerStartStrength
			return currentActiveCp.Read<float>(0x00, 0x40);
		}
		public string CurrentCPName(float x, float y) {
			IntPtr areas = currentAreas.Read<IntPtr>(0x00);
			int listSize = Program.Read<int>(areas, 0x0c);
			if (listSize > 0) {
				IntPtr cps = Program.Read<IntPtr>(areas, 0x08, 0x10 + (listSize - 1) * 4, 0x10);
				listSize = Program.Read<int>(cps, 0x0c);
				if (listSize == 9) {
					if (areas != IntPtr.Zero) {
						float minDis = 99999999;
						float pX = CurrentPlayerX();
						float pY = CurrentPlayerY();
						for (int i = 0; i < 9; i++) {
							float tX = junkX[i];
							float tY = junkY[i];
							float dis = (float)Math.Sqrt((tX - pX) * (tX - pX) + (tY - pY) * (tY - pY));
							if (dis < minDis) {
								minDis = dis;
								x = tX;
								y = tY;
							}
						}
					}
				}
			}
			return CheckPointNames.GetCheckpointName(x, y);
		}
		public float CurrentPlayerX() {
			return playerHandler.Read<float>(0x0, 0x118);
		}
		public float CurrentPlayerY() {
			return playerHandler.Read<float>(0x0, 0x120);
		}
		public string SceneToLoad() {
			return sceneToLoad.ReadString(0x0);
		}
		public Vector CurrentVelocity() {
			if (playerHandler.Value == IntPtr.Zero) { return default(Vector); }

			float x = playerHandler.Read<float>(0x00, 0x38, 0x50);
			float y = playerHandler.Read<float>(0x00, 0x38, 0x54);
			float z = playerHandler.Read<float>(0x00, 0x38, 0x58);
			return new Vector() { X = x, Y = y, Z = z, M = (float)Math.Sqrt(x * x + y * y + z * z) };
		}
		public void SetMax(float maxSpeed) {
			MemoryReader.Write<float>(Program, playerHandler.Value, maxSpeed, 0x00, 0x38, 0x0c);
		}
		public string CurrentLevelName() {
			if (preloadLevel.Value != IntPtr.Zero) {
				int index = preloadLevel.Read<int>();
				if (index >= 0) {
					IntPtr lvl = levelHandler.Read<IntPtr>(0x10, 0x08, 0x10 + (4 * index), 0x08);
					return Program.GetString(lvl);
				}
			}
			return string.Empty;
		}
		public string CurrentSceneName() {
			if (preloadLevel.Value != IntPtr.Zero) {
				int index = preloadLevel.Read<int>();
				if (index >= 0) {
					IntPtr lvl = levelHandler.Read<IntPtr>(0x10, 0x08, 0x10 + (4 * index), 0x0c);
					return Program.GetString(lvl);
				}
			}
			return string.Empty;
		}

		public bool HookProcess() {
			if (Program == null || Program.HasExited) {
				Process[] processes = Process.GetProcessesByName("Defunct_x86");
				Program = processes.Length == 0 ? null : processes[0];
				if (processes.Length == 0 || Program.HasExited) {
					IsHooked = false;
					return IsHooked;
				}

				IsHooked = true;
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) { Program.Dispose(); }
		}
	}
	public static class CheckPointNames {
		public static string GetCheckpointName(float x, float y) {
			if (Close(x, 925.67f) && Close(y, -1621.17f)) {
				return "Home 1";
			} else if (Close(x, 1075.73f) && Close(y, -928.06f)) {
				return "Home 2";
			} else if (Close(x, 967.02f) && Close(y, -64.94f)) {
				return "Home 3";
			} else if (Close(x, 964.58f) && Close(y, 156.11f)) {
				return "Home 4";
			} else if (Close(x, 926.44f) && Close(y, 904.43f)) {
				return "Home 5";
			} else if (Close(x, 1310.35f) && Close(y, 122.18f)) {
				return "Lost 1";
			} else if (Close(x, 954.95f) && Close(y, 452.01f)) {
				return "Lost 2";
			} else if (Close(x, 1222.07f) && Close(y, 1136.75f)) {
				return "Lost 3";
			} else if (Close(x, 1344.99f) && Close(y, 1465.97f)) {
				return "Lost 4";
			} else if (Close(x, 1069.39f) && Close(y, 2036.45f)) {
				return "Over the Hills 1";
			} else if (Close(x, 1112.21f) && Close(y, 3865.20f)) {
				return "Over the Hills 2";
			} else if (Close(x, 950.07f) && Close(y, 4910.21f)) {
				return "Over the Hills 3";
			} else if (Close(x, 849.10f) && Close(y, 5999.77f)) {
				return "Into the Woods 1";
			} else if (Close(x, 763.26f) && Close(y, 6264.99f)) {
				return "Into the Woods 2";
			} else if (Close(x, 832.16f) && Close(y, 6599.93f)) {
				return "Into the Woods 3";
			} else if (Close(x, 1285.05f) && Close(y, 6890.98f)) {
				return "Into the Woods 4";
			} else if (Close(x, 1509.36f) && Close(y, 7052.23f)) {
				return "Into the Woods 5";
			} else if (Close(x, 1085.00f) && Close(y, 7482.80f)) {
				return "Into the Woods 6";
			} else if (Close(x, 1005.14f) && Close(y, 7855.66f)) {
				return "Into the Woods 7";
			} else if (Close(x, 893.31f) && Close(y, 7917.74f)) {
				return "Into the Woods 8";
			} else if (Close(x, 946.71f) && Close(y, 8445.26f)) {
				return "Into the Woods 9";
			} else if (Close(x, 1395.80f) && Close(y, 9007.97f)) {
				return "Into the Woods 10";
			} else if (Close(x, 1102.45f) && Close(y, 10185.27f)) {
				return "Valley 1";
			} else if (Close(x, 1248.54f) && Close(y, 11309.05f)) {
				return "Valley 2";
			} else if (Close(x, 1311.17f) && Close(y, 11391.33f)) {
				return "Valley 3";
			} else if (Close(x, 1069.93f) && Close(y, 12120.20f)) {
				return "Valley 4";
			} else if (Close(x, 1058.02f) && Close(y, 14018.76f)) {
				return "Valley 5";
			} else if (Close(x, 1077.64f) && Close(y, -303.69f)) {
				return "Dawn 1";
			} else if (Close(x, 1074.72f) && Close(y, 327.88f)) {
				return "Dawn 2";
			} else if (Close(x, 1351.91f) && Close(y, 347.89f)) {
				return "Dawn 3";
			} else if (Close(x, 1065.64f) && Close(y, 1141.15f)) {
				return "Dawn 4";
			} else if (Close(x, 1280.41f) && Close(y, 1294.87f)) {
				return "Dawn 5";
			} else if (Close(x, 1134.45f) && Close(y, 2740.20f)) {
				return "Dawn 6";
			} else if (Close(x, 1065.38f) && Close(y, 3733.01f)) {
				return "Dawn 7";
			} else if (Close(x, 1132.36f) && Close(y, 4784.33f)) {
				return "Dawn 8";
			} else if (Close(x, 992.36f) && Close(y, 6039.27f)) {
				return "Junkyard 1";
			} else if (Close(x, 725.25f) && Close(y, 6288.26f)) {
				return "Junkyard 2";
			} else if (Close(x, 540.18f) && Close(y, 6407.79f)) {
				return "Junkyard 3";
			} else if (Close(x, 1513.69f) && Close(y, 6441.79f)) {
				return "Junkyard 4";
			} else if (Close(x, 1082.25f) && Close(y, 6484.88f)) {
				return "Junkyard 5";
			} else if (Close(x, 422.82f) && Close(y, 6676.19f)) {
				return "Junkyard 6";
			} else if (Close(x, 1494.81f) && Close(y, 6999.13f)) {
				return "Junkyard 7";
			} else if (Close(x, 692.56f) && Close(y, 7093.40f)) {
				return "Junkyard 8";
			} else if (Close(x, 1559.98f) && Close(y, 7325.27f)) {
				return "Junkyard 9";
			} else if (Close(x, 1004.60f) && Close(y, 745.69f)) {
				return "Back on Track 1";
			} else if (Close(x, 1431.33f) && Close(y, 2159.06f)) {
				return "Back on Track 2";
			} else if (Close(x, 1044.42f) && Close(y, 2470.65f)) {
				return "Back on Track 3";
			} else if (Close(x, 718.49f) && Close(y, 3761.40f)) {
				return "Back on Track 4";
			} else if (Close(x, 427.95f) && Close(y, 4391.21f)) {
				return "Back on Track 5";
			} else if (Close(x, 1101.38f) && Close(y, 6067.04f)) {
				return "Back on Track 6";
			} else if (Close(x, 1088.06f) && Close(y, 8361.94f)) {
				return "Oasis 1";
			} else if (Close(x, 874.04f) && Close(y, 9196.58f)) {
				return "Oasis 2";
			} else if (Close(x, 563.40f) && Close(y, 9554.17f)) {
				return "Oasis 3";
			} else if (Close(x, 794.83f) && Close(y, 130.81f)) {
				return "Rock Bottom 1";
			} else if (Close(x, 636.56f) && Close(y, 138.05f)) {
				return "Rock Bottom 2";
			} else if (Close(x, 807.83f) && Close(y, 657.29f)) {
				return "Rock Bottom 3";
			} else if (Close(x, 702.34f) && Close(y, 1318.92f)) {
				return "Rock Bottom 4";
			} else if (Close(x, 1026.43f) && Close(y, 559.08f)) {
				return "Rock Bottom 5";
			} else if (Close(x, 882.79f) && Close(y, 902.04f)) {
				return "Rock Bottom 6";
			} else if (Close(x, 729.27f) && Close(y, 1448.83f)) {
				return "Rock Bottom 7";
			} else if (Close(x, 609.67f) && Close(y, 2415.18f)) {
				return "Rock Bottom 8";
			} else if (Close(x, 1018.62f) && Close(y, 4014.03f)) {
				return "Crash Site 1";
			} else if (Close(x, 934.19f) && Close(y, 5780.51f)) {
				return "Crash Site 2";
			} else if (Close(x, 878.46f) && Close(y, 8249.75f)) {
				return "Crash Site 3";
			}

			return string.Empty;
		}
		private static bool Close(float a, float b) {
			return Math.Abs(a - b) < 0.02;
		}
	}
	public class ProgramPointer {
		private static string[] versions = new string[1] { "v1.0" };
		private static Dictionary<string, Dictionary<string, string>> funcPatterns = new Dictionary<string, Dictionary<string, string>>() {
			{"v1.0", new Dictionary<string, string>() {
					{"LevelHandler",     "83C4108BF88BC783EC086A0050E8????????83C41085C074248BF785FF74158B068B008B40088B400C3D????????0F85????????B8"},
					{"LevelInfoScreen",  "EC5783EC248B7D08B8????????8938BA????????83EC0C57E8????????83C41089474C8B473483EC0C503900E8????????83C41083EC0C503900|-42" },
					{"LevelIndex",       "57503900E8????????83C4108BC88B45F88B49244183EC046A0051503900E8????????83C4108BF88BC785C00F84????????8B4F0CB8????????89088B4F24B8" },
					{"CurrentActiveCp",  "8BD139128B490C4983EC0851503900E8????????83C41083EC0C503900E8????????83C410EB2C8B05????????83EC086A0050E8????????83C41085C074148B05" },
					{"CurrentAreas",     "8BD139128B490C4983EC0851503900E8????????83C41083EC0C503900E8????????83C410EB2C8B05????????83EC086A0050E8????????83C41085C074148B05|-69" },
					{"PreloadLevel",     "558BEC83EC188B05????????3D????????741B8B0D????????B8????????89080FB60D????????B8????????8808B8????????8B4D088908B8????????0FB64D0C8808B8????????8B4D108908B8????????0FB64D148808B9????????B8????????83EC085150|-95" },
					{"SceneToLoad",      "558BEC83EC188B05????????3D????????741B8B0D????????B8????????89080FB60D????????B8????????8808B8????????8B4D088908B8????????0FB64D0C8808B8????????8B4D108908B8????????0FB64D148808B9????????B8????????83EC085150|-56" },
					{"PlayerHandler",    "558BEC83EC088B05????????83EC086A0050E8????????83C41085C07416B9????????8B4508890183EC0C50E8????????83C410C9C3|-46" },
				    {"SaveHandlerMain",  "????????8B05????????BA????????83EC0C50E8????????83C4108B05????????BA????????83EC0C50E8????????83C4108B05????????BA????????83EC0C50E8|-37" }
			}},
		};
		private IntPtr pointer;
		public DefunctMemory Memory { get; set; }
		public string Name { get; set; }
		public bool IsStatic { get; set; }
		private int lastID;
		private DateTime lastTry;
		public ProgramPointer(DefunctMemory memory, string name) {
			this.Memory = memory;
			this.Name = name;
			this.IsStatic = true;
			lastID = memory.Program == null ? -1 : memory.Program.Id;
			lastTry = DateTime.MinValue;
		}

		public IntPtr Value {
			get {
				if (!Memory.IsHooked) {
					pointer = IntPtr.Zero;
				} else {
					GetPointer(ref pointer, Name);
				}
				return pointer;
			}
		}
		public T Read<T>(params int[] offsets) {
			if (!Memory.IsHooked) { return default(T); }
			return Memory.Program.Read<T>(Value, offsets);
		}
		public string ReadString(params int[] offsets) {
			if (!Memory.IsHooked) { return string.Empty; }
			IntPtr p = Memory.Program.Read<IntPtr>(Value, offsets);
			return Memory.Program.GetString(p);
		}
		private void GetPointer(ref IntPtr ptr, string name) {
			if (Memory.IsHooked) {
				if (Memory.Program.Id != lastID) {
					ptr = IntPtr.Zero;
					lastID = Memory.Program.Id;
				}
				if (ptr == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
					lastTry = DateTime.Now;
					ptr = GetVersionedFunctionPointer(name);
					if (ptr != IntPtr.Zero) {
						if (IsStatic) {
							ptr = Memory.Program.Read<IntPtr>(ptr, 0, 0);
						} else {
							ptr = Memory.Program.Read<IntPtr>(ptr, 0);
						}
					}
				}
			}
		}
		public IntPtr GetVersionedFunctionPointer(string name) {
			foreach (string version in versions) {
				if (funcPatterns[version].ContainsKey(name)) {
					return Memory.Program.FindSignatures(funcPatterns[version][name])[0];
				}
			}
			return IntPtr.Zero;
		}
	}
	public struct Vector {
		public float X, Y, Z, M;
		public override string ToString() {
			return "(" + M.ToString("0.00") + ") (" + Math.Abs(X).ToString("0.00") + ", " + Math.Abs(Y).ToString("0.00") + ", " + Math.Abs(Z).ToString("0.00") + ")";
		}
	}
}