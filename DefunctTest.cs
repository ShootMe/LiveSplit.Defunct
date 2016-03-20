using System.Threading;
namespace LiveSplit.Defunct {
	public class DefunctTest {
		private static DefunctComponent comp = new DefunctComponent();
		public static void Main(string[] args) {
			Thread t = new Thread(GetVals);
			t.IsBackground = true;
			t.Start();
			System.Windows.Forms.Application.Run();
		}
		private static void GetVals() {
			while (true) {
				comp.GetValues();

				Thread.Sleep(5);
			}
		}
	}
}