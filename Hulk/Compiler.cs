namespace Hulk
{
    public class Compiler
    {
         public HulkHistory History { get; private set; }

        
        Print Handler;
        
        
        HulkParser Parser;






        public Compiler(Print print)
        {
            History = new();
            Parser = new(History, print);
            Handler = print;
        }
        public void Compile(string input)
        {
            string[] s = MakeToken.GetTokens(input);
            List<string[]> CompInstructions;
            try
            {
                CompInstructions = MakeToken.GetInstructions(s);
            }
            catch (Exception ex)
            {
                Handler(ex.Message);
                return;
            }
            for (int i = 0; i < CompInstructions.Count; i++)
            {
                string[] instruction = CompInstructions[i];
                if (instruction.Length == 0)
                    continue;
                try
                {
                    try
                    {
                        HulkExpression exp = Parser.Parse(instruction);
                        if (exp is FunctionDeclaration Dec)
                        {
                            Dec.AddToHistory(History);
                        }
                        else if(exp is PrintFunc print)
                        {
                            print.GetValue(false);
                            print.GetValue(true);
                        }
                        else
                        {
                            Handler(exp.GetValue(false));
                        }
                    }
                    catch (HulkException ex)
                    {
                        throw new InstrucctionError(ex, i + 1, CompInstructions.Count);
                    }
                }
                catch (Exception ex)
                {
                    Handler(ex.Message);
                }
            }
        }
       
	}
}
