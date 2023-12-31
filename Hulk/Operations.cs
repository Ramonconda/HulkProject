﻿using Microsoft.CSharp.RuntimeBinder;

namespace Hulk
{
    public abstract class UnaryFunction : HulkExpression
    {
        public UnaryFunction(HulkExpression Arg)
        {
            if (Arg.IsDependent)
                IsDependent = true;
            Argument = Arg;
        }
        public override object GetValue(bool execute)
        {
            return Evaluate(Argument.GetValue(execute));
        }
        public HulkExpression Argument { get; protected set; }
        public abstract object Evaluate(object arg);
    }
    // Boolean
    public class Negation : UnaryFunction
    {
        public Negation(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is bool b)
                return !b;
            if (arg == null)
                return default(bool);
            throw new SemanticError("Operator `!`", "boolean", arg.GetHulkTypeAsString());
        }
    }
    // Arithmetic
    public class Positive : UnaryFunction
    {
        public Positive(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return x;
            if (arg == null)
                return 5d;
            throw new SemanticError("Operator `+`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class Negative : UnaryFunction
    {
        public Negative(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return -x;
            if (arg == null)
                return 5d;
            throw new SemanticError("Operator `-`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class SquaredRoot : UnaryFunction
    {
        public SquaredRoot(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return Math.Sqrt(x);
            if (arg == null)
                return 5d;
            throw new SemanticError("Function `sqrt`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class Sine : UnaryFunction
    {
        public Sine(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return Math.Sin(x);
            if (arg == null)
                return 5d;
            throw new SemanticError("Function `sin`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class Cosine : UnaryFunction
    {
        public Cosine(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return Math.Cos(x);
            if (arg == null)
                return 5d;
            throw new SemanticError("Function `cos`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class ERaised : UnaryFunction
    {
        public ERaised(HulkExpression Arg) : base(Arg)
        {
        }
        public override object Evaluate(object arg)
        {
            if (arg is double x)
                return Math.Exp(x);
            if (arg == null)
                return 5d;
            throw new SemanticError("Function `exp`", "number", arg.GetHulkTypeAsString());
        }
    }
    public class Rand : HulkExpression
    {
        public Rand()
        {
        }
        public override object GetValue(bool execute)
        {
            Random random = new();
            return random.NextDouble();
        }
    }





    /////operaciones binarias



    public abstract class BinaryFunction : HulkExpression
    {
        public BinaryFunction(HulkExpression leftArgument, HulkExpression rightArgument)
        {
            if (leftArgument.IsDependent || rightArgument.IsDependent)
                IsDependent = true;
            LeftArgument = leftArgument;
            RightArgument = rightArgument;
        }
        public override object GetValue(bool execute)
        {
            return Evaluate(LeftArgument.GetValue(execute), RightArgument.GetValue(execute));
        }
        protected bool ArgsOk(object left, object right, List<Type> AdmitedTypesName)
        {
            foreach (var type in AdmitedTypesName)
            {
                if (left == null || right == null)
                {
                    if (left == null && right != null)
                    {
                        if (right.GetType() == type)
                            return true;
                    }
                    else if (right == null && left != null)
                    {
                        if (left.GetType() == type)
                            return true;
                    }
                    return true;
                }
                if (left.GetType() == right.GetType() && left.GetType() == type)
                    return true;
            }
            return false;
        }
        public HulkExpression LeftArgument { get; protected set; }
        public HulkExpression RightArgument { get; protected set; }
        public abstract object Evaluate(object left, object right);
    }
   
    public class Conjunction : BinaryFunction
    {
        public Conjunction(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is bool && right is bool))
                return (dynamic)left && (dynamic)right;
            var conflictiveType = left is not bool ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `&&`", "boolean", conflictiveType);
        }
    }
    public class Disjunction : BinaryFunction
    {
        public Disjunction(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is bool && right is bool))
                return (dynamic)left || (dynamic)right;
            var conflictiveType = left is not bool ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `||`", "boolean", conflictiveType);
        }
    }
  
    public class LowerThan : BinaryFunction
    {
        public LowerThan(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is double && right is double))
                return (dynamic)left < (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `<`", "number", conflictiveType);
        }
    }
    public class GreaterThan : BinaryFunction
    {
        public GreaterThan(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is double && right is double))
                return (dynamic)left > (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `>`", "number", conflictiveType);
        }
    }
    public class LowerEqualThan : BinaryFunction
    {
        public LowerEqualThan(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is double && right is double))
                return (dynamic)left <= (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `<=`", "number", conflictiveType);
        }
    }
    public class GreaterEqualThan : BinaryFunction
    {
        public GreaterEqualThan(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return default(bool);
            if ((left is double && right is double))
                return (dynamic)left >= (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `>=`", "number", conflictiveType);
        }
    }
    public class Equal : BinaryFunction
    {
        public Equal(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            try 
            {
                return (dynamic)left == (dynamic)right;
            }
            catch(RuntimeBinderException ex)
            {
                string message = ex.Message;
                message = message.Replace("'", "`");
                message = message.Replace("bool", "boolean");
                message = message.Replace("int", "number");
                message = message.Replace("double", "number");
                throw new RegularError(message, "semantic");
            }
        }
    }
    public class UnEqual : BinaryFunction
    {
        public UnEqual(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }

        public override object Evaluate(object left, object right)
        {
            try
            {
                return (dynamic)left != (dynamic)right;
            }
            catch (RuntimeBinderException ex)
            {
                string message = ex.Message;
                message = message.Replace("'", "`");
                message = message.Replace("bool", "boolean");
                message = message.Replace("int", "number");
                message = message.Replace("double", "number");
                throw new RegularError(message, "semantic");
            }
        }
    }
    
    public class Addition : BinaryFunction
    {
        public Addition(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            string expected = "";
            if (left is null)
            {
                if ((double)right != 0d)
                    left = right;
                else
                    left = 5d;
            }
            if (right is null)
            {
                if ((double)left != 0d)
                    right = left;
                else
                    right = 5d;
            }
            if (left == null && right == null)
                return default;
            if ((left is double && right is double) || (left is string && right is string))
                return (dynamic)left + (dynamic)right;
            throw new OperationSemanticError("+", left.GetHulkTypeAsString(), right.GetHulkTypeAsString(), expected);
        }
    }
    public class Subtraction : BinaryFunction
    {
        public Subtraction(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            if (left is null)
            {
                if ((double)right != 0d)
                    left = right;
                else
                    left = 5d;
            }
            if (right is null)
            {
                if ((double)left != 0d)
                    right = left;
                else
                    right = 5d;
            }
            if (left == null && right == null)
                return 5d;
            if ((left is double && right is double))
                return (dynamic)left - (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `-`", "number", conflictiveType);
        }
    }
    public class Multiplication : BinaryFunction
    {
        public Multiplication(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }

        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return 5d;
            if ((left is double && right is double))
                return (dynamic)left * (dynamic)right;
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `*`", "number", conflictiveType);
        }
    }
    public class Division : BinaryFunction
    {
        public Division(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return 5d;
            if (left is double && right is double divisor)
            {
                if (divisor == 0)
                    throw new RegularError("Atempted to divide by 0", "arithmetic");
                return (dynamic)left / (dynamic)right;
            }
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `/`", "number", conflictiveType);
        }
    }
    public class Module : BinaryFunction
    {
        public Module(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return 5d;
            if (left is double && right is double divisor)
            {
                if (divisor == 0)
                    throw new RegularError("Atempted to divide by 0", "arithmetic");
                return (dynamic)left % (dynamic)right;
            }
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `%`", "number", conflictiveType);
        }
    }
    public class Power : BinaryFunction
    {
        public Power(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return 5d;
            if ((left is double && right is double))
                return Math.Pow((dynamic)left, (dynamic)right);
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Operator `^`", "number", conflictiveType);
        }
    }
    public class Logarithm : BinaryFunction
    {
        public Logarithm(HulkExpression leftArgument, HulkExpression rightArgument) : base(leftArgument, rightArgument)
        {
        }
        public override object Evaluate(object left, object right)
        {
            left ??= right;
            right ??= left;
            if (left == null && right == null)
                return 5d;
            if ((left is double && right is double))
                return Math.Log((dynamic)left, (dynamic)right);
            var conflictiveType = left is not double ? left.GetHulkTypeAsString() : right.GetHulkTypeAsString();
            throw new SemanticError("Function `log`", "number", conflictiveType);
        }
    }

}


