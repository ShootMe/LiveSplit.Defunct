using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;
namespace LiveSplit.Defunct {
    public class SplitterFactory : IComponentFactory {
        public string ComponentName { get { return "Defunct Autosplitter v" + this.Version.ToString(); } }
        public string Description { get { return "Autosplitter for Defunct"; } }
        public ComponentCategory Category { get { return ComponentCategory.Control; } }
        public IComponent Create(LiveSplitState state) { return new SplitterComponent(state); }
        public string UpdateName { get { return this.ComponentName; } }
		public string UpdateURL { get { return "https://raw.githubusercontent.com/ShootMe/LiveSplit.Defunct/master/"; } }
		public string XMLURL { get { return this.UpdateURL + "Components/LiveSplit.Defunct.Updates.xml"; } }
		public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
    }
}