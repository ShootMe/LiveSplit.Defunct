using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace LiveSplit.Defunct.Memory {
	public class DefunctMemory {
		private ProgramPointer currentActiveCp, currentAreas, levelHandler, preloadLevel, playerHandler;
		private float[] junkX = { 992.36f, 725.25f, 540.18f, 1513.69f, 1082.25f, 422.82f, 1494.81f, 692.56f, 1559.98f };
		private float[] junkY = { 6039.27f, 6288.26f, 6407.79f, 6441.79f, 6484.88f, 6676.19f, 6999.13f, 7093.40f, 7325.27f };
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;

		public DefunctMemory() {
			currentActiveCp = new ProgramPointer(this, "CurrentActiveCp") { IsStatic = false };
			currentAreas = new ProgramPointer(this, "CurrentAreas") { IsStatic = false };
			levelHandler = new ProgramPointer(this, "LevelHandler");
			preloadLevel = new ProgramPointer(this, "PreloadLevel") { IsStatic = false };
			playerHandler = new ProgramPointer(this, "PlayerHandler") { IsStatic = false };
		}
		public float CurrentCPX() {
			//CheckPointSystem.instance.currentActiveCp.midPos.X
			return currentActiveCp.Read<float>(0x00, 0x78);
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
		public float CurrentVelocity() {
			if (playerHandler.Value == IntPtr.Zero) { return 0; }

			float x = playerHandler.Read<float>(0x00, 0x38, 0x50);
			float y = playerHandler.Read<float>(0x00, 0x38, 0x54);
			float z = playerHandler.Read<float>(0x00, 0x38, 0x58);
			return (float)Math.Sqrt(x * x + y * y + z * z);
		}
		public void SetMax() {
			MemoryReader.Write<float>(Program, playerHandler.Value, 360, 0x00, 0x38, 0x0c);
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
			int xp = (int)(x * 100);
			int yp = (int)(y * 100);
			if (xp == 92567 && yp == -162117) {
				return "Home 1";
			} else if (xp == 107573 && yp == -92806) {
				return "Home 2";
			} else if (xp == 96702 && yp == -6494) {
				return "Home 3";
			} else if (xp == 96458 && yp == 15611) {
				return "Home 4";
			} else if (xp == 92644 && yp == 90443) {
				return "Home 5";
			} else if (xp == 131035 && yp == 12218) {
				return "Lost 1";
			} else if (xp == 95495 && yp == 45201) {
				return "Lost 2";
			} else if (xp == 122207 && yp == 113675) {
				return "Lost 3";
			} else if (xp == 134499 && yp == 146597) {
				return "Lost 4";
			} else if (xp == 106939 && yp == 203645) {
				return "Over the Hills 1";
			} else if (xp == 111221 && yp == 386520) {
				return "Over the Hills 2";
			} else if (xp == 95007 && yp == 491021) {
				return "Over the Hills 3";
			} else if (xp == 84910 && yp == 599977) {
				return "Into the Woods 1";
			} else if (xp == 76326 && yp == 626499) {
				return "Into the Woods 2";
			} else if (xp == 83216 && yp == 659993) {
				return "Into the Woods 3";
			} else if (xp == 128505 && yp == 689098) {
				return "Into the Woods 4";
			} else if (xp == 150936 && yp == 705223) {
				return "Into the Woods 5";
			} else if (xp == 108500 && yp == 748280) {
				return "Into the Woods 6";
			} else if (xp == 100514 && yp == 785566) {
				return "Into the Woods 7";
			} else if (xp == 89331 && yp == 791774) {
				return "Into the Woods 8";
			} else if (xp == 94671 && yp == 844526) {
				return "Into the Woods 9";
			} else if (xp == 139580 && yp == 900797) {
				return "Into the Woods 10";
			} else if (xp == 110245 && yp == 1018527) {
				return "Valley 1";
			} else if (xp == 124854 && yp == 1130905) {
				return "Valley 2";
			} else if (xp == 131117 && yp == 1139133) {
				return "Valley 3";
			} else if (xp == 106993 && yp == 1212020) {
				return "Valley 4";
			} else if (xp == 105802 && yp == 1401876) {
				return "Valley 5";
			} else if (xp == 107764 && yp == -30369) {
				return "Dawn 1";
			} else if (xp == 107472 && yp == 32788) {
				return "Dawn 2";
			} else if (xp == 135191 && yp == 34789) {
				return "Dawn 3";
			} else if (xp == 106564 && yp == 114115) {
				return "Dawn 4";
			} else if (xp == 128041 && yp == 129487) {
				return "Dawn 5";
			} else if (xp == 113445 && yp == 274020) {
				return "Dawn 6";
			} else if (xp == 106538 && yp == 373301) {
				return "Dawn 7";
			} else if (xp == 113236 && yp == 478433) {
				return "Dawn 8";
			} else if (xp == 99236 && yp == 603927) {
				return "Junkyard 1";
			} else if (xp == 72525 && yp == 628826) {
				return "Junkyard 2";
			} else if (xp == 54018 && yp == 640779) {
				return "Junkyard 3";
			} else if (xp == 151369 && yp == 644179) {
				return "Junkyard 4";
			} else if (xp == 108225 && yp == 648488) {
				return "Junkyard 5";
			} else if (xp == 42282 && yp == 667619) {
				return "Junkyard 6";
			} else if (xp == 149481 && yp == 699913) {
				return "Junkyard 7";
			} else if (xp == 69256 && yp == 709340) {
				return "Junkyard 8";
			} else if (xp == 155998 && yp == 732527) {
				return "Junkyard 9";
			} else if (xp == 100460 && yp == 74569) {
				return "Back on Track 1";
			} else if (xp == 143133 && yp == 215906) {
				return "Back on Track 2";
			} else if (xp == 104442 && yp == 247065) {
				return "Back on Track 3";
			} else if (xp == 71849 && yp == 376140) {
				return "Back on Track 4";
			} else if (xp == 42795 && yp == 439121) {
				return "Back on Track 5";
			} else if (xp == 110138 && yp == 606704) {
				return "Back on Track 6";
			} else if (xp == 108806 && yp == 836194) {
				return "Oasis 1";
			} else if (xp == 87404 && yp == 919658) {
				return "Oasis 2";
			} else if (xp == 56340 && yp == 955417) {
				return "Oasis 3";
			} else if (xp == 79483 && yp == 13081) {
				return "Rock Bottom 1";
			} else if (xp == 63656 && yp == 13805) {
				return "Rock Bottom 2";
			} else if (xp == 80783 && yp == 65729) {
				return "Rock Bottom 3";
			} else if (xp == 70234 && yp == 131892) {
				return "Rock Bottom 4";
			} else if (xp == 102643 && yp == 55908) {
				return "Rock Bottom 5";
			} else if (xp == 88279 && yp == 90204) {
				return "Rock Bottom 6";
			} else if (xp == 72927 && yp == 144883) {
				return "Rock Bottom 7";
			} else if (xp == 60967 && yp == 241518) {
				return "Rock Bottom 8";
			} else if (xp == 101862 && yp == 401403) {
				return "Crash Site 1";
			} else if (xp == 93419 && yp == 578051) {
				return "Crash Site 2";
			} else if (xp == 87846 && yp == 824975) {
				return "Crash Site 3";
			}

			return string.Empty;
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
					{"PlayerHandler",    "558BEC83EC088B05????????83EC086A0050E8????????83C41085C07416B9????????8B4508890183EC0C50E8????????83C410C9C3|-46" }
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
}