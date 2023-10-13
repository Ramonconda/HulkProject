namespace Hulk
{
    public class Asignment : HulkExpression
    {
         public List<Variable> Variables { get; protected set; }
        public HulkExpression ValueExpression { get; protected set; }
        public Asignment(List<Variable> Vars, HulkExpression ValueExp)
        {
            Variables = Vars;
            CheckValue(ValueExp);
            ValueExpression = ValueExp;
        }
        
        private void CheckValue(HulkExpression ValueExp)
        {
            Types type = default;
            var val = ValueExp.GetValue(false);
            if (val is double)
                type = Types.number;
            else if (val is bool)
                type = Types.boolean;
            else if (val is string)
                type = Types.hstring;
            else if (val is null)
                type = Types.dynamic;
            else if (val is EmptyReturn)
                type = Types.Void;
            foreach (Variable v in Variables)
            {
                if (v.Type != type)
                    throw new RegularError($"Cannot asign value of type `{type}` to `{v.Type}` variable" , "asignment");
            }
        }
        private void ChangeValues()
        {
            var val = ValueExpression.GetValue(false);
            foreach (Variable v in Variables)
            {
                v.Value = val;
                v.Options = Variable.VariableOptions.InitializedVariable;
            }
        }
        public override object GetValue(bool execute)
        {
            if (execute)
                ChangeValues();
            return ValueExpression.GetValue(false);
        }
        
       
    
    }
    public class FunctionCall : HulkExpression
    {
        public string Name { get; protected set; }
        public List<HulkExpression> Arguments { get; protected set; }
        public FunctionDeclaration Definition { get; protected set; }
        public FunctionCall(string name, List<HulkExpression> Args, FunctionDeclaration Def)
        {
            foreach (var arg in Args)
            {
                if (arg.IsDependent)
                    IsDependent = true;
            }
            Name = name;
            CheckArgs(Args);
            Arguments = Args;
            Definition = Def;
        }
        #region Methods
        private void CheckArgs(List<HulkExpression> Args)
        {
            foreach (var arg in Args)
            {
                arg.GetValue(false);
            }
        }
        public override object GetValue(bool execute)
        {
            try
            {
                return Definition.Evaluate(Arguments, execute);
            }
            catch (SemanticError ex)
            {
                throw new SemanticError($"Function `{Name}`", ex.ExpressionExpected, ex.ExpressionReceived);
            }
        }
        #endregion  
       
    }
    public class PrintFunc : HulkExpression
    {
        public PrintFunc(HulkExpression Arg, Print printHandler)
        {
            Argument = Arg;
            PrintHandler = printHandler;
        }
        public override object GetValue(bool execute)
        {
            if (execute)
                PrintHandler(Argument.GetValue(execute));
            return Argument.GetValue(false);
        }
        Print PrintHandler;
        public HulkExpression Argument { get; }
        
    }
    public class Variable : HulkExpression
    { 
        public string? Name { get; protected set; }
        public Types Type { get; protected set; }
        public enum VariableOptions { Value, InitializedVariable, NonInitialized, FunctionArgument, Dependent }
        public VariableOptions Options { get; set; }
        public object Value { get; set; }
        
        //constructor para numeros, valores de verdad o strings
        public Variable(object value)
        {
            Value = value;
            Options = VariableOptions.Value;
            SetType();
        }
        //constructor variable sin tipo
        public Variable(string name, object value)
        {
            Name = name;
            Value = value;
            Options = VariableOptions.InitializedVariable;
            SetType();
        }
        //constructor variable con tipo
        public Variable(string name, object value, Types type, VariableOptions options)
        {
            Name = name;
            Options = options;
            if (options == VariableOptions.Dependent || options == VariableOptions.FunctionArgument)
                IsDependent = true;
            object valueToCheck = value;
            bool matchExp = false;
            if (value is HulkExpression expression)
            {
                valueToCheck = expression.GetValue(false);
                if (valueToCheck == null)
                {
                    if (value is Addition && (type == Types.number || type == Types.hstring))
                        matchExp = true;
                    else if (value is Variable)
                        matchExp = true;
                }
            }
            else if (valueToCheck == null)
                matchExp = true;

            bool matchNumber = valueToCheck is double && type == Types.number;
            bool matchBool = valueToCheck is bool && type == Types.boolean;
            bool matchString = valueToCheck is string && type == Types.hstring;
            if (matchNumber || matchBool || matchString || matchExp || type == Types.dynamic)
            {
                Value = value;
                Type = type;
            }
            else
                throw new SemanticError($"Variable `{Name}`", $"{Type}", value.GetHulkTypeAsString());

        }
       
        public override object GetValue(bool execute)
        {
            switch (Options)
            {
                case VariableOptions.NonInitialized:
                    throw new RegularError("Use of unasigned variable");
                case VariableOptions.FunctionArgument:
                    return Value;
            }
            if (IsDependent)
                return ((HulkExpression)Value).GetValue(execute);
            return Value;
        }        
        private void SetType()
        {
            if (Value is double)
                Type = Types.number;
            else if (Value is bool)
                Type = Types.boolean;
            else if (Value is string)
                Type = Types.hstring;
            
        }
        
    }


     public static class HulkInfo
    {
        //información sobre el lenguaje. Operaciones, palabras clave
        public static string[] KeyWords = { "+", "-", "*", "/", "^", "&&", "&", "|", "||", "=", "(", ")", ",", "function", "number", "boolean", "string", "let", "in", "=>", "else", "if" };
      
        public static bool IsCorrectName(string name)
        {
            foreach (char c in name)
                if (!Char.IsLetterOrDigit(c))
                    return false;
            if (HulkInfo.KeyWords.Contains(name))
                return false;
            if (double.TryParse(name, out _))
                return false;
            if (Char.IsDigit(name[0]))
                return false;
            return true;
        }
       
        public const int StackLimit = 1000; //Limite de llamadas antes deStackOverflowException
    }


    public abstract class HulkExpression
    {
        public abstract object GetValue(bool execute);
        public bool IsDependent { get; protected set; }
    }

 
    public delegate void Print(object input);
    public static class ExtObject
    {
        public static string GetHulkTypeAsString (this Object arg)
        {
            var type = arg.GetType();
            if (type == typeof(double))
                return "number";
            else if (type == typeof(bool))
                return "boolean";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(EmptyReturn))
                return "void";
            else
                return "type";
        }
    }
    public class EmptyReturn 
    {
        public EmptyReturn() { }
    }
    public enum Types { Void, number, boolean, hstring, dynamic }
}
