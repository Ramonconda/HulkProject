using System.Globalization;
using System.Text.RegularExpressions;

namespace Hulk
{
    public class HulkParser
    {
        Stack<HulkExpression> ParsingExp;
        Print PrintHandler;
        public HulkHistory Memoria { get; }
        public HulkParser(HulkHistory Mem, Print printHandler)
        {
            Memoria = Mem;
            ParsingExp = new Stack<HulkExpression>();
            PrintHandler = printHandler;
        }




        public HulkExpression Parse(string[] tokens)
        {
            try
            {
                var result = ParseInternal(tokens, 0, tokens.Length - 1);
                return result;
            }
            catch (Exception)
            {
                ParsingExp.Clear();
                throw;
            }
        }
        private HulkExpression ParseInternal(string[] tokens, int start, int end)
        {
            if (tokens.Length == 0)
                return null;
            HulkExpression expr = null;
            if (tokens[start] == "function")
                expr = ParseFunctionDeclaration(tokens, start, end);
            else if (tokens[start] == "number" || tokens[start] == "boolean" || tokens[start] == "string")
                expr = ParseVarDeclaration(tokens, start, end);
            if (expr is VariableDeclaration)
                throw new RegularError("variables must be declared inside let-in expressions", "declaration");
            expr ??= ParseInner(tokens, start, end);
            if (expr == null)
                throw new RegularError("Invalid Expression, missing semicolon?");
            return expr;
        }
        private HulkExpression ParseInner(string[] tokens, int start, int end)
        {
            if (tokens.Length == 0)
                return null;
            HulkExpression expr = null;
            if (tokens[start] == "let")
                expr = ParseLetInExpression(tokens, start, end);
            else if (tokens[start] == "if")
                expr = ParseIfElseStatement(tokens, start, end);            
            //expr ??= TryAsignment(tokens, start, end);
            expr ??= TryConditionalOr(tokens, start, end);
            expr ??= TryConditionalAnd(tokens, start, end);
            expr ??= TryEquality(tokens, start, end);
            expr ??= TryRelational(tokens, start, end);
            expr ??= TryAdditive(tokens, start, end);
            expr ??= TryMultiplicative(tokens, start, end);
            expr ??= TryPower(tokens, start, end);
            expr ??= TryUnary(tokens, start, end);
            expr ??= TryPrincipal(tokens, start, end);
            if (expr == null)
                throw new RegularError("Invalid Expression, missing semicolon?");
            return expr;
        }
        


        
        private HulkExpression TryConditionalOr(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "|":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Disjunction));
                }
            }
            return null;
        }
        private HulkExpression TryConditionalAnd(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "&":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Conjunction));
                }
            }
            return null;
        }
        private HulkExpression TryEquality(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "==":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Equal));
                    case "!=":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(UnEqual));
                }
            }
            return null;
        }
        private HulkExpression TryRelational(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "<":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(LowerThan));
                    case ">":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(GreaterThan));
                    case "<=":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(LowerEqualThan));
                    case ">=":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(GreaterEqualThan));
                }
            }
            return null;
        }
        private HulkExpression TryAdditive(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "+":
                        return i == start ? null : BinaryFunctionMaker(tokens, start, end, i, typeof(Addition));

                    case "-":
                        return i == start ? null : BinaryFunctionMaker(tokens, start, end, i, typeof(Subtraction));

                }
            }
            return null;
        }
        private HulkExpression TryMultiplicative(string[] tokens, int start, int end)
        {
            for (int i = end; i >= start; i--)
            {
                switch (tokens[i])
                {
                    case ")":
                        {
                            i = MakeToken.GoToPreviousParenthesis(i, start, tokens);
                            break;
                        }
                    case "*":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Multiplication));
                    case "/":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Division));
                    case "%":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Module));
                }
            }
            return null;
        }
        private HulkExpression TryPower(string[] tokens, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                switch (tokens[i])
                {
                    case "(":
                        {
                            i = MakeToken.GoToNextParenthesis(i, end, tokens);
                            break;
                        }
                    case "^":
                        return BinaryFunctionMaker(tokens, start, end, i, typeof(Power));
                }
            }
            return null;
        }
        private HulkExpression TryUnary(string[] tokens, int start, int end)
        {
            switch (tokens[start])
            {
                case "(":
                    {
                        int i = MakeToken.GoToNextParenthesis(start, end, tokens);
                        if (i != end)
                            return null;
                        else
                            return start != end - 1 ? ParseInner(tokens, start + 1, end - 1) : throw new SyntaxError(")", "expression");
                    }
                case "!":
                    return UnaryFunctionMaker(tokens, start, end, start, typeof(Negation));
                case "+":
                    return UnaryFunctionMaker(tokens, start, end, start, typeof(Positive));
                case "-":
                    return UnaryFunctionMaker(tokens, start, end, start, typeof(Negative));
            }
            return null;
        }
        private HulkExpression TryPrincipal(string[] tokens, int start, int end)
        {
            if (start == end)
            {

                if (tokens[start] == "true" || tokens[start] == "false")
                    return new Variable(bool.Parse(tokens[start]));
                if (double.TryParse(tokens[start], NumberStyles.Any, new CultureInfo("en-US"), out double maybeNum))
                    return new Variable(maybeNum);
                if (Regex.Match(tokens[start], @"\u0022(.)*\u0022").Success)
                {
                    string arg = TreatStringRepresentation(tokens[start]);
                    return new Variable(arg);
                }
                return TryVariable(tokens[start]);
            }
            switch (tokens[start])
            {
                case "(":
                    {
                        return start != end - 1 ? ParseInner(tokens, start + 1, end - 1) : throw new SyntaxError(")", "expression");
                    }
                case "sqrt":
                    return FunctionCallMaker(tokens, start, end, typeof(SquaredRoot));
                case "sin":
                    return FunctionCallMaker(tokens, start, end, typeof(Sine));
                case "cos":
                    return FunctionCallMaker(tokens, start, end, typeof(Cosine));
                case "exp":
                    return FunctionCallMaker(tokens, start, end, typeof(SquaredRoot));
                case "log":
                    return FunctionCallMaker(tokens, start, end, typeof(Logarithm));
                case "print":
                    return FunctionCallMaker(tokens, start, end, typeof(PrintFunc));
                default:
                    return TryFunctionCall(tokens, start, end);
            }
        }
    

        private HulkExpression ParseVarDeclaration(string[] tokens, int start, int end)
        {
            HulkExpression result;
            if (tokens[start] != "number" && tokens[start] != "boolean" && tokens[start] != "string")
                throw new LexicalError(tokens[start], "variable type");
            string type = tokens[start];
            int declarationEnd = MakeToken.GetNameLimit(tokens, start + 1, end, "=");
            if (tokens[start + 1] == "=")
                throw new SyntaxError("variable name", "variable declaration");
            List<string> names;
            try
            {
                names = MakeToken.GetCommaSeparatedTokens(tokens, start + 1, declarationEnd);
            }
            catch
            {
                string invalid = "";
                for (int i = start + 1; i <= declarationEnd; i++)
                    invalid += tokens[i] + ", ";
                throw new LexicalError(invalid, "declaration name");
            }
            foreach (string name in names)
            {
                bool correct = HulkInfo.IsCorrectName(name);
                if (!correct)
                    throw new LexicalError(name, "variable name");
            }
            HulkExpression ValueExp = null;
            if (declarationEnd < end - 1)
                ValueExp = ParseInner(tokens, declarationEnd + 2, end);
            else if (declarationEnd == end - 1 || declarationEnd > end - 1) //comentar miembro derecho del or para poder declarar varibales no inicializadas
                throw new SyntaxError("value expression", "variable declaration");
            result = new VariableDeclaration(names, type, ValueExp);
            return result;
        }
        private HulkExpression ParseLetInArgs(string[] tokens, int start, int end)
        {
            VariableDeclaration result;
            string type = null;
            string name;
           
            int declarationEnd;
            if (tokens[start] == "number" || tokens[start] == "boolean" || tokens[start] == "string")
            {
                declarationEnd = MakeToken.GetNameLimit(tokens, start + 1, end, "=");
                type = tokens[start];
                if (tokens[start + 1] == "=")
                    throw new SyntaxError("variable name", "variable declaration");
                name = tokens[start + 1];
            }
            else
            {
                name = tokens[start];
                declarationEnd = MakeToken.GetNameLimit(tokens, start, end, "=");
            }
            List<string> VariableName = new();

            bool correct = HulkInfo.IsCorrectName(name);
            if (!correct)
                throw new LexicalError(name, "variable name");

            VariableName.Add(name);
            HulkExpression ValueExp = null;
            if (declarationEnd < end - 1)
                ValueExp = ParseInner(tokens, declarationEnd + 2, end);
            else if (declarationEnd >= end - 1)
                throw new SyntaxError("value expression", "variable declaration");
            if (type == null)
                return new VariableDeclaration(VariableName, ValueExp);
            return new VariableDeclaration(VariableName, type, ValueExp);
        }


        public HulkExpression ParseFunctionDeclaration(string[] tokens, int start, int end)
        {
            int declarationEnd = MakeToken.GetNameLimit(tokens, start, end, "=>");
            if (tokens[start] != "function")
                throw new LexicalError(tokens[start], "function declaration start");
            if (tokens[start + 1] == "=>")
                throw new SyntaxError("function name", "function declaration");
            FunctionDeclaration result;
            if (declarationEnd >= end - 1)
                throw new SyntaxError("function declaration body", "function declaration");

            else
            {
                string funcName = tokens[start + 1];
                ;
                if (!HulkInfo.IsCorrectName(funcName))
                    throw new LexicalError(funcName, "function name");
                if (tokens[start + 2] != "(")
                    throw new SyntaxError("(", "function declaration after function name");
                if (tokens[declarationEnd] != ")")
                    throw new SyntaxError(")", "function declaration arguments");
                List<string> ArgNames;
                try
                {
                    ArgNames = MakeToken.GetCommaSeparatedTokens(tokens, start + 3, declarationEnd - 1);
                }
                catch
                {
                    string invalid = "";
                    for (int i = start + 3; i <= declarationEnd - 1; i++)
                        invalid += tokens[i] + ", ";
                    throw new LexicalError(invalid, "function arguments");
                }

                foreach (string name in ArgNames)
                {
                    if (!HulkInfo.IsCorrectName(name))
                        throw new LexicalError(name, "variable name");
                }
                result = new FunctionDeclaration(funcName, ArgNames);
                ParsingExp.Push(result);
                HulkExpression DefExpression = ParseInner(tokens, declarationEnd + 2, end);
                ParsingExp.Pop();
                result.Define(DefExpression);
            }
            return result;
        }
        private HulkExpression ParseLetInExpression(string[] tokens, int start, int end)
        {
            LetInStatement result;
            if (tokens[start] != "let")
                return null;
            int declarationEnd = MakeToken.GetNameLimit(tokens, start, end, "in");
            if (declarationEnd >= end - 1)
                throw new SyntaxError("body", "let-in expression");
            List<HulkExpression> Args = GetComaSeparatedDeclarations(tokens, start + 1, declarationEnd);
            Dictionary<string, Variable> LayerVariables = new();
            foreach (HulkExpression arg in Args)
            {
                if (arg is not VariableDeclaration)
                    throw new SemanticError("let-in", "variable declaration", arg.GetType().Name);
                var Vars = arg as VariableDeclaration;
                foreach (string name in Vars.Names)
                {
                    if (Vars.ValExpression is null)
                        throw new SyntaxError("value", "let-in expression argument");
                    Variable LetVariable;
                    if (Vars.ValExpression.IsDependent)
                        LetVariable = new Variable(name, Vars.ValExpression, Vars.Type, Variable.VariableOptions.Dependent);
                    else
                        LetVariable = new Variable(name, Vars.ValExpression.GetValue(false));
                    LayerVariables.Add(name, LetVariable);
                }
            }
            result = new LetInStatement(LayerVariables);
            ParsingExp.Push(result);
            HulkExpression DefExpression = ParseInner(tokens, declarationEnd + 2, end);
            ParsingExp.Pop();
            result.Define(DefExpression);
            return result;
        }
        private HulkExpression ParseIfElseStatement(string[] tokens, int start, int end)
        {
            HulkExpression result;
            if (tokens[start] != "if")
                throw new LexicalError(tokens[start], "if statement token");
            else
            {
                int conditionEnd;
                if (tokens[start + 1] != "(")
                    throw new SyntaxError("(", "if-else condition");
                else
                {
                    conditionEnd = MakeToken.GoToNextParenthesis(start + 1, end, tokens);
                    if (conditionEnd == end)
                        throw new SyntaxError("if instruction", "if-else expression");
                    HulkExpression condition = ParseInner(tokens, start + 1, conditionEnd);
                    int ifDoEnd = MakeToken.GetNameLimit(tokens, start, end, "else");
                    if (ifDoEnd == end - 1)
                        throw new SyntaxError("else instruction", "if-else expression after token \"else\"");
                    HulkExpression IfDo = ParseInner(tokens, conditionEnd + 1, ifDoEnd);
                    HulkExpression ElseDo = null;
                    if (ifDoEnd < end - 1)
                        ElseDo = ParseInner(tokens, ifDoEnd + 2, end);
                    else
                        throw new SyntaxError("else expression", "if-else statement");
                    result = new IfElseStatement(condition, IfDo, ElseDo);
                }
            }
            return result;
        }
    

        private HulkExpression TryFunctionCall(string[] tokens, int start, int end)
        {
            HulkExpression result = null;
            if (tokens[start + 1] == "(")
            {
                if (tokens[end] != ")")
                    throw new SyntaxError(")", "function call");
               
                FunctionDeclaration Definition;

                if (Memoria.Functions.ContainsKey(tokens[start]))
                    Definition = Memoria.Functions[tokens[start]];
                else
                {
                    try
                    {
                        var ExpressionList = ParsingExp.ToList();
                        FunctionDeclaration BaseExp = ExpressionList[^1] as FunctionDeclaration;
                        if (tokens[start] == BaseExp.FunctionName)
                            Definition = BaseExp;
                        else
                            throw new RegularError($"function {tokens[start]} not found", "reference");
                    }
                    catch
                    {
                        throw new RegularError($"function {tokens[start]} not found", "reference");
                    }
                }

                string name = tokens[start];
            
                var Args = GetComaSeparatedExpressions(tokens, start + 2, end - 1);
                result = new FunctionCall(name, Args, Definition);
            }
            return result;
        }
        private HulkExpression TryVariable(string varName)
        {
            switch (varName)
            {
                case "PI":
                    return new Variable(Math.PI);
                case "E":
                    return new Variable(Math.E);
            }
            Stack<HulkExpression> PosibleLocations = new(new Stack<HulkExpression>(ParsingExp));
            Dictionary<string, Variable> Location = new();
            while (PosibleLocations.TryPop(out HulkExpression exp))
            {
                if (exp is FunctionDeclaration Dec)
                    Location = Dec.Arguments;
                else if (exp is LetInStatement Let)
                    Location = Let.StoredVariables;

                if (Location.ContainsKey(varName))
                    return Location[varName];
            }
            if (Location.ContainsKey(varName))
                return Location[varName];
            else
                throw new RegularError($"variable {varName} not found", "reference");
        }
        private HulkExpression BinaryFunctionMaker(string[] tokens, int start, int end, int opPos, Type type)
        {
            HulkExpression left = opPos != start ? ParseInner(tokens, start, opPos - 1) : throw new SyntaxError("left argument", $"\"{tokens[opPos]}\" expression");
            HulkExpression right = opPos != end ? ParseInner(tokens, opPos + 1, end) : throw new SyntaxError("right argument", $"\"{tokens[opPos]}\" expression");

           
            object[] args = new object[] { left, right };
            return (HulkExpression)Activator.CreateInstance(type, args);
            
        }
        private HulkExpression UnaryFunctionMaker(string[] tokens, int start, int end, int opPos, Type type)
        {
            HulkExpression argument = start != end ? ParseInner(tokens, start + 1, end) : throw new SyntaxError("left argument", $"\"{tokens[opPos]}\" expression");
            
            object[] args = new object[] { argument };
            return (HulkExpression)Activator.CreateInstance(type, args);
        }
        private HulkExpression FunctionCallMaker(string[] tokens, int start, int end, Type type)
        {
            if (tokens[start + 1] != "(" || tokens[end] != ")")
                throw new SyntaxError("parenthesis", "function call arguments");
            else
            {
                List<HulkExpression> Args = GetComaSeparatedExpressions(tokens, start + 2, end - 1);
                if (type == typeof(PrintFunc))
                {
                    List<object> printArgs = new List<object>(Args);
                    printArgs.Add(PrintHandler);
                    object[] print = printArgs.ToArray();
                    return (HulkExpression)Activator.CreateInstance(type, print);
                }
                object[] args = Args.ToArray();                
                return (HulkExpression)Activator.CreateInstance(type, args);
            }
        }
        
        private string TreatStringRepresentation(string str)
        {
            //caracteres escapados y elimina las comillas al final del string
            str = str.Replace("\\a","\a");
            str = str.Replace("\\b", "\b"); 
            str = str.Replace("\\f", "\f");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\r", "\r");
            str = str.Replace("\\t", "\t");
            str = str.Replace("\\v", "\v");
            str = str.Replace("\\", "");
            str = str[1..^1];
            return str;
        }
        private List<HulkExpression> GetComaSeparatedExpressions(string[] tokens, int start, int end)
        {
            List<HulkExpression> result = new();
            if (tokens[start] == "," || tokens[end] == ",")
                throw new RegularError("incorrect comma separation");
            int argStart = start;
            for (int i = start; i <= end; i++)
            {
                if (tokens[i] == "(")
                {
                    i = MakeToken.GoToNextParenthesis(i, end, tokens);
                }
                if (tokens[i] == ",")
                {
                    var exp = ParseInner(tokens, argStart, i - 1);
                  
                    result.Add(exp);
                    argStart = i + 1;
                }
                else if (i == end)
                {
                    var exp = ParseInner(tokens, argStart, i);
                    
                    result.Add(exp);
                }
            }
            return result;
        }
        private List<HulkExpression> GetComaSeparatedDeclarations(string[] tokens, int start, int end)
        {
            List<HulkExpression> result = new();
            if (tokens[start] == "," || tokens[end] == ",")
                throw new RegularError("incorrect comma separation");
            int argStart = start;
            for (int i = start; i <= end; i++)
            {
                if (tokens[i] == "(")
                {
                    i = MakeToken.GoToNextParenthesis(i, end, tokens);
                }

                if (tokens[i] == ",")
                {

                    var exp = ParseLetInArgs(tokens, argStart, i - 1);
                    if (exp == null || exp is not VariableDeclaration)
                        throw new SemanticError("Let-in argument", "variable declaration", exp.GetType().Name);
                    result.Add(exp);
                    argStart = i + 1;
                }
                else if (i == end)
                {
                    var exp = ParseLetInArgs(tokens, argStart, i);
                    if (exp == null || exp is not VariableDeclaration)
                        throw new SemanticError("Let-in argument", "variable declaration", exp.GetType().Name);
                    result.Add(exp);
                }
            }
            return result;
        }
      
    }

}