namespace Voodoo.Tune.Debugger.Editor
{
	public interface IDataWidget
	{
		bool CanApply { get; set; }
		bool IsSimulating { get; }
		
		void OnGUI();
		bool Save();
	}
}