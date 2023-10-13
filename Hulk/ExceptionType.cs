using System.Globalization;

namespace Hulk
{
	public abstract class HulkException : Exception
	{
		public override string Message { get => (MessageBegin + MessageSpecification + ".").Replace("hstring", "string"); }
		public string MessageBegin { get; protected set; }
		public string MessageSpecification { get; protected set; }
	}
	public class RegularError : HulkException
	{
		public RegularError(string message)
		{
			
			MessageSpecification = message;
			MessageBegin = "!! Error !! : ";
		}
		public RegularError(string message, string errorEspecification)
		{
			MessageSpecification = message;
			errorEspecification = errorEspecification.ToUpper(new CultureInfo("en-US"));
			MessageBegin = $"! {errorEspecification} Error :";
            
		}
	}

	public class InstrucctionError: HulkException
	{
		public InstrucctionError(HulkException ex, int instrucctionNumber, int instrucctionsCount)
		{
			MessageBegin = ex.MessageBegin;
			string messageEnd = instrucctionsCount > 1 ? $" (on instrucction {instrucctionNumber})" : ""; 
			MessageSpecification = ex.MessageSpecification + messageEnd;
		}
	}
	public class SemanticError : HulkException
	{
			public string Expression { get; }
		public string ExpressionReceived { get; }
		public string ExpressionExpected { get; }
		public SemanticError(string expression, string expected, string received)
		{
			MessageBegin = "! SEMANTIC ERROR : ";
			Expression = expression;
			ExpressionReceived = received;
			ExpressionExpected = expected;
			MessageSpecification = $"{Expression} receives `{expected}`, not `{received}`";
		}
	
	}
	public class OperationSemanticError : HulkException
	{
		public string Operation { get; }
		public string LArg { get; }
		public string RArg { get; }
		public string Expected { get; }
		public OperationSemanticError(string operation, string leftarg, string rightarg, string expected)
		{
			MessageBegin = "!! Semantic Error : ";
			Operation = operation;
			LArg = leftarg;
			RArg = rightarg;
			Expected = expected;
			MessageSpecification = $"Operator `{operation}` cannot be applied to operands of type  `{leftarg}` and `{rightarg}`";
		}
		
	}
	public class LexicalError : HulkException
	{
		public string InvalidToken { get; }
		public string ExpectedToken { get; }
		public LexicalError(string invalidToken)
		{
			MessageBegin = "!! Lexical Error : ";
			InvalidToken = invalidToken;
			ExpectedToken = "token";
			MessageSpecification = $"`{InvalidToken}` is not a valid {ExpectedToken}";

		}
		public LexicalError(string invalidToken, string expectedToken)
		{
			MessageBegin = "!! Lexical Error : ";
			InvalidToken = invalidToken;
			ExpectedToken = expectedToken;
			MessageSpecification = $"`{InvalidToken}` is not a valid {ExpectedToken}";
		}
		
	}


	public class SyntaxError : HulkException
	{
		public string MissingPart { get; }
		public string MissingPlace { get; }
		public SyntaxError(string missingPart, string place)
		{
			MessageBegin = "!! Syntax Error : ";
			MissingPart = missingPart;
			MissingPlace = place;
			MessageSpecification = $"Missing {MissingPart} in {MissingPlace}";
		}
		

	}
	
    public class OverFlowError : HulkException
    {
		public string FunctionName { get; }
        public OverFlowError(string FunctionName)
        {
            MessageBegin = "!! Function Error : ";
			FunctionName = FunctionName;
            MessageSpecification = $"Function '{FunctionName}' reached stack limit ( limit is {HulkInfo.StackLimit})";
        }
        
    }
}
