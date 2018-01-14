using System;
using System.Diagnostics;
namespace LiveSplit.Defunct.Memory {
	public class DefunctMemory {
		private static ProgramPointer levelHandler = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "83C41085C074248BF785FF74158B068B008B40088B400C3D????????0F85????????B8", 35));
		private static ProgramPointer checkPointSystem = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8BD139128B490C4983EC0851503900E8????????83C41083EC0C503900E8", -4));
		private static ProgramPointer preloadLevel = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC83EC188B05????????3D????????741B8B0D????????B8????????89080FB60D", 8));
		private static ProgramPointer playerHandler = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC575681EC????????8B7D088B05????????83EC086A0050E8", 16));
		private static ProgramPointer saveHandler = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC148B05????????83EC086A0050E8????????83C41085C00F84????????83EC0C68", 9));
		private static ProgramPointer arcadeTimer = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC048B7D08B8????????89388B05????????83EC086A0050", 11));
		private float[] junkX = { 992.36f, 725.25f, 540.18f, 1513.69f, 1082.25f, 422.82f, 1494.81f, 692.56f, 1559.98f };
		private float[] junkY = { 6039.27f, 6288.26f, 6407.79f, 6441.79f, 6484.88f, 6676.19f, 6999.13f, 7093.40f, 7325.27f };
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public DefunctMemory() {
			lastHooked = DateTime.MinValue;
		}
		public Vector CurrentCP() {
			//CheckPointSystem.currentActiveCp.midPos.X
			float x = checkPointSystem.Read<float>(Program, -0x8, 0x74);
			//CheckPointSystem.currentActiveCp.midPos.Y
			float y = checkPointSystem.Read<float>(Program, -0x8, 0x78);
			//CheckPointSystem.currentActiveCp.midPos.playerStartStrength
			float strength = checkPointSystem.Read<float>(Program, -0x8, 0x3c);
			return new Vector() { X = x, Y = y, M = strength };
		}
		public void UnlockAllLevels() {
			if (saveHandler.GetPointer(Program) != IntPtr.Zero) {
				//SaveHandler.s_instance.s_mainLevels.m_levelDatas
				IntPtr mainLevels = (IntPtr)saveHandler.Read<uint>(Program, 0x0, 0x10, 0x10);
				int size = Program.Read<int>(mainLevels, 0xc);
				for (int i = 0; i < size; i++) {
					IntPtr level = (IntPtr)Program.Read<uint>(mainLevels, 0x10 + i * 4);
					IntPtr nextLevel = (IntPtr)Program.Read<uint>(mainLevels, 0x10 + (i + 1 < size ? i + 1 : i) * 4);
					bool unlocked = Program.Read<bool>(level, 0x14);
					bool completed = Program.Read<bool>(level, 0x15);
					bool nextUnlocked = Program.Read<bool>(nextLevel, 0x14);
					bool nextCompleted = Program.Read<bool>(nextLevel, 0x15);
					if ((unlocked ^ completed) || (!(unlocked & completed) && (nextUnlocked | nextCompleted))) {
						Program.Write(level, 16843009, 0x14);
					}
				}
			}
		}
		public int PlatinumCount() {
			int count = 0;
			if (saveHandler.GetPointer(Program) != IntPtr.Zero) {
				//SaveHandler.s_instance.s_mainLevels.m_levelDatas
				IntPtr mainLevels = (IntPtr)saveHandler.Read<uint>(Program, 0x0, 0x10, 0x10);
				int size = Program.Read<int>(mainLevels, 0xc);
				for (int i = 0; i < size; i++) {
					//SaveHandler.s_instance.s_mainLevels.m_levelDatas[i].m_medal
					int medal = Program.Read<int>(mainLevels, 0x10 + i * 4, 0xc);
					if (medal == 3) { count++; }
				}

				//SaveHandler.s_instance.s_arcadeLevels.m_levelDatas
				IntPtr arcadeLevels = (IntPtr)saveHandler.Read<uint>(Program, 0x0, 0x14, 0x10);
				size = Program.Read<int>(arcadeLevels, 0xc);
				for (int i = 0; i < size; i++) {
					//SaveHandler.s_instance.s_arcadeLevels.m_levelDatas[i].m_medal
					int medal = Program.Read<int>(arcadeLevels, 0x10 + i * 4, 0xc);
					if (medal == 3) { count++; }
				}
			}
			return count;
		}
		public int[] Collectibles() {
			int[] stats = new int[3] { 0, 0, 0 };
			if (saveHandler.GetPointer(Program) != IntPtr.Zero && preloadLevel.GetPointer(Program) != IntPtr.Zero) {
				//PreloadLevel.s_levelIndex
				int lvlIndex = preloadLevel.Read<int>(Program);

				//SaveHandler.s_instance.s_mainLevels.m_levelDatas
				IntPtr mainLevels = (IntPtr)saveHandler.Read<uint>(Program, 0x0, 0x10, 0x10);
				int size = Program.Read<int>(mainLevels, 0xc);
				int totalCount = 0;

				if (lvlIndex == 5) {
					//CheckPointSystem.currentAreas
					IntPtr areas = (IntPtr)checkPointSystem.Read<uint>(Program);
					int areaSize = Program.Read<int>(areas, 0xc);
					if (areaSize > 0) {
						//CheckPointSystem.currentAreas[areaSize-1].cps.Count
						areaSize = Program.Read<int>(areas, 0x8, 0x10 + (areaSize - 1) * 4, 0xc, 0xc);
						if (areaSize == 9) { lvlIndex = 6; }
					}
				}

				if (lvlIndex >= 0) {
					//LevelHandler.m_mainLevels[lvlIndex].m_collectablesAmount
					stats[0] = levelHandler.Read<int>(Program, 0x0, 0xc, 0x8, 0x10 + (4 * lvlIndex), 0x1c);
				}

				for (int i = 0; i < size; i++) {
					//SaveHandler.s_instance.s_mainLevels.m_levelDatas[i]
					IntPtr level = (IntPtr)Program.Read<uint>(mainLevels, 0x10 + (i * 4));
					//SaveHandler.s_instance.s_mainLevels.m_levelDatas[i].m_collectables.length
					int length = Program.Read<int>(level, 0x8, 0xc);
					int count = 0;
					for (int j = 0; j < length; j++) {
						//SaveHandler.s_instance.s_mainLevels.m_levelDatas[i].m_collectables[j]
						if (Program.Read<bool>(level, 0x8, 0x10 + j)) {
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
		public float CurrentArcadeTime() {
			//ArcadeTimer.s_self.m_time
			return arcadeTimer.Read<float>(Program, 0x0, 0x74);
		}
		public bool TimerOn() {
			//ArcadeTimer.s_self.m_timerIsOn
			return arcadeTimer.Read<bool>(Program, 0x0, 0x7c);
		}
		public void ResetTimer() {
			if (arcadeTimer.GetPointer(Program) != IntPtr.Zero) {
				//ArcadeTimer.s_self.m_time
				//ArcadeTimer.s_self.m_pauseTimer
				arcadeTimer.Write<long>(Program, 0L, 0x0, 0x74);
				//ArcadeTimer.s_self.m_timerIsOn
				arcadeTimer.Write<bool>(Program, false, 0x0, 0x7c);
			}
		}
		public bool IsArcadePlay() {
			//PreloadLevel.s_isArcadePlay
			return preloadLevel.Read<bool>(Program, 0x4);
		}
		public string CurrentCPName(string level, float x, float y) {
			//CheckPointSystem.currentAreas
			IntPtr areas = (IntPtr)checkPointSystem.Read<uint>(Program);
			int listSize = Program.Read<int>(areas, 0xc);
			if (listSize > 0) {
				//CheckPointSystem.currentAreas[size-1].cps.Count
				listSize = Program.Read<int>(areas, 0x8, 0x10 + (listSize - 1) * 4, 0xc, 0xc);
				if (listSize == 9) {
					if (areas != IntPtr.Zero) {
						float minDis = 99999999;
						Vector pos = CurrentPlayerPos();
						for (int i = 0; i < 9; i++) {
							float tX = junkX[i];
							float tY = junkY[i];
							float dis = (float)Math.Sqrt((tX - pos.X) * (tX - pos.X) + (tY - pos.Y) * (tY - pos.Y));
							if (dis < minDis) {
								minDis = dis;
								x = tX;
								y = tY;
							}
						}
					}
				}
			}
			return CheckPointNames.GetCheckpointName(level, x, y);
		}
		public Vector CurrentPlayerPos() {
			//PlayerHandler.self.currentGrPlayerPos
			float x = playerHandler.Read<float>(Program, 0x0, 0x118);
			float y = playerHandler.Read<float>(Program, 0x0, 0x120);
			float z = playerHandler.Read<float>(Program, 0x0, 0x11c);
			return new Vector() { X = x, Y = y, Z = z };
		}
		public Vector CurrentVelocity() {
			if (playerHandler.GetPointer(Program) == IntPtr.Zero) { return default(Vector); }

			//PlayerHandler.self.speedSettings.lastVelocity
			float x = playerHandler.Read<float>(Program, 0x0, 0x34, 0x50);
			float y = playerHandler.Read<float>(Program, 0x0, 0x34, 0x54);
			float z = playerHandler.Read<float>(Program, 0x0, 0x34, 0x58);
			return new Vector() { X = x, Y = y, Z = z, M = (float)Math.Sqrt(x * x + y * y + z * z) };
		}
		public string SceneToLoad() {
			//PreloadLevel.s_sceneToLoad
			return Program.Read((IntPtr)preloadLevel.Read<uint>(Program, -0x8));
		}
		public string CurrentLevelName() {
			if (preloadLevel.GetPointer(Program) != IntPtr.Zero) {
				//PreloadLevel.s_levelIndex
				int index = preloadLevel.Read<int>(Program);
				if (index >= 0) {
					if (!IsArcadePlay()) {
						//LevelHandler.m_mainLevels[index].m_levelName
						return Program.Read((IntPtr)levelHandler.Read<uint>(Program, 0x0, 0xc, 0x8, 0x10 + (4 * index), 0x8));
					} else {
						//LevelHandler.m_arcadeLevels[index].m_levelName
						return Program.Read((IntPtr)levelHandler.Read<uint>(Program, 0x0, 0x10, 0x8, 0x10 + (4 * index), 0x8));
					}
				}
			}
			return string.Empty;
		}
		public string CurrentSceneName() {
			if (preloadLevel.GetPointer(Program) != IntPtr.Zero) {
				//PreloadLevel.s_levelIndex
				int index = preloadLevel.Read<int>(Program);
				if (index >= 0) {
					if (!IsArcadePlay()) {
						//LevelHandler.m_mainLevels[index].m_sceneName
						return Program.Read((IntPtr)levelHandler.Read<uint>(Program, 0x0, 0xc, 0x8, 0x10 + (4 * index), 0xc));
					} else {
						//LevelHandler.m_arcadeLevels[index].m_sceneName
						return Program.Read((IntPtr)levelHandler.Read<uint>(Program, 0x0, 0x10, 0x8, 0x10 + (4 * index), 0xc));
					}

				}
			}
			return string.Empty;
		}

		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Defunctx86");
				Program = processes.Length == 0 ? null : processes[0];
				if (Program != null) {
					MemoryReader.Update64Bit(Program);
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) { Program.Dispose(); }
		}
	}
	public static class CheckPointNames {
		public static string GetCheckpointName(string level, float x, float y) {
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

			return level;
		}
		private static bool Close(float a, float b) {
			return Math.Abs(a - b) < 0.02;
		}
	}
	public enum PointerVersion {
		V1
	}
	public enum AutoDeref {
		None,
		Single,
		Double
	}
	public class ProgramSignature {
		public PointerVersion Version { get; set; }
		public string Signature { get; set; }
		public int Offset { get; set; }
		public ProgramSignature(PointerVersion version, string signature, int offset) {
			Version = version;
			Signature = signature;
			Offset = offset;
		}
		public override string ToString() {
			return Version.ToString() + " - " + Signature;
		}
	}
	public class ProgramPointer {
		private int lastID;
		private DateTime lastTry;
		private ProgramSignature[] signatures;
		private int[] offsets;
		public IntPtr Pointer { get; private set; }
		public PointerVersion Version { get; private set; }
		public AutoDeref AutoDeref { get; private set; }

		public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures) {
			AutoDeref = autoDeref;
			this.signatures = signatures;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}
		public ProgramPointer(AutoDeref autoDeref, params int[] offsets) {
			AutoDeref = autoDeref;
			this.offsets = offsets;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}

		public T Read<T>(Process program, params int[] offsets) where T : struct {
			GetPointer(program);
			return program.Read<T>(Pointer, offsets);
		}
		public string Read(Process program, params int[] offsets) {
			GetPointer(program);
			return program.Read((IntPtr)program.Read<uint>(Pointer, offsets));
		}
		public byte[] ReadBytes(Process program, int length, params int[] offsets) {
			GetPointer(program);
			return program.Read(Pointer, length, offsets);
		}
		public void Write<T>(Process program, T value, params int[] offsets) where T : struct {
			GetPointer(program);
			program.Write<T>(Pointer, value, offsets);
		}
		public void Write(Process program, byte[] value, params int[] offsets) {
			GetPointer(program);
			program.Write(Pointer, value, offsets);
		}
		public IntPtr GetPointer(Process program) {
			if (program == null) {
				Pointer = IntPtr.Zero;
				lastID = -1;
				return Pointer;
			} else if (program.Id != lastID) {
				Pointer = IntPtr.Zero;
				lastID = program.Id;
			}

			if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
				lastTry = DateTime.Now;

				Pointer = GetVersionedFunctionPointer(program);
				if (Pointer != IntPtr.Zero) {
					if (AutoDeref != AutoDeref.None) {
						Pointer = (IntPtr)program.Read<uint>(Pointer);
						if (AutoDeref == AutoDeref.Double) {
							if (MemoryReader.is64Bit) {
								Pointer = (IntPtr)program.Read<ulong>(Pointer);
							} else {
								Pointer = (IntPtr)program.Read<uint>(Pointer);
							}
						}
					}
				}
			}
			return Pointer;
		}
		private IntPtr GetVersionedFunctionPointer(Process program) {
			if (signatures != null) {
				MemorySearcher searcher = new MemorySearcher();
				searcher.MemoryFilter = delegate (MemInfo info) {
					return (info.State & 0x1000) != 0 && (info.Protect & 0x40) != 0 && (info.Protect & 0x100) == 0;
				};
				for (int i = 0; i < signatures.Length; i++) {
					ProgramSignature signature = signatures[i];

					IntPtr ptr = searcher.FindSignature(program, signature.Signature);
					if (ptr != IntPtr.Zero) {
						Version = signature.Version;
						return ptr + signature.Offset;
					}
				}

				return IntPtr.Zero;
			}

			return (IntPtr)program.Read<uint>(program.MainModule.BaseAddress, offsets);
		}
	}
	public struct Vector {
		public float X, Y, Z, M;
		public override string ToString() {
			return "(" + M.ToString("0.00") + ") (" + Math.Abs(X).ToString("0.00") + ", " + Math.Abs(Y).ToString("0.00") + ", " + Math.Abs(Z).ToString("0.00") + ")";
		}
	}
}