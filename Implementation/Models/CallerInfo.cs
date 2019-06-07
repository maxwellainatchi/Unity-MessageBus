namespace Messaging.Message {
	public class CallerInfo {
		public int line;
		public string file;
		public string funcName;
		public System.DateTime emittedAt;
		public System.DateTime sentAt;

		public override string ToString() {
			return $"<b>{System.IO.Path.GetFileName(file)}@{line} (in {funcName})</b>";
		}
	}
}
