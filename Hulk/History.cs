namespace Hulk
{
	public class HulkHistory
	{
		public Dictionary<string, FunctionDeclaration> Functions { get; }
		public HulkHistory()
		{
			Functions = new Dictionary<string, FunctionDeclaration>();
		}
		public void AddNewFunction(string key, FunctionDeclaration Val)
		{
			if (!Functions.TryAdd(key, Val))
				throw new RegularError($"Function {key} already exist");
		}
		
	}
}