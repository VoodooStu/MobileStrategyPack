namespace Voodoo.Tune.Network
{
	public class Header
	{
		public string name;
		public string value;

		public Header() { }

		public Header(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}
}