﻿using System;
using System.Threading;
namespace LiveSplit.Defunct {
	public class DefunctTest {
		private static DefunctComponent comp = new DefunctComponent(null);
		[STAThread]
		public static void Main(string[] args) {
			try {
				Thread test = new Thread(GetVals);
				test.IsBackground = true;
				test.Start();
				System.Windows.Forms.Application.Run();
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		private static void GetVals() {
			try {
				while (true) {
					comp.GetValues();

					Thread.Sleep(10);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
	}
}