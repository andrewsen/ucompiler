using System;
using System.Collections.Generic;
using System.Linq;
using Translator;

namespace uc
{
    public class Operation
    {
        public string View;
        public OperationType Type;
        public Association Association;
        public int Priority;
        public int ArgumentCount;

        // TODO: Will be redunant in future
        public static Operation Cast
        {
            get
            {
                return new Operation
                {
                    Type = OperationType.Cast,
                    Priority = 12,
                };
            }
        }

        private Operation()
        {
            Association = Association.Left;
        }

        public bool Is(params OperationType[] operations)
        {
            return operations.Contains(Type);
        }

        public bool IsUnary
        {
            get
            {
                return new List<OperationType> 
                { 
                    OperationType.Inc, OperationType.Dec, OperationType.PreInc, OperationType.PreDec, OperationType.PostInc, OperationType.PostDec, 
                    OperationType.Not, OperationType.Inv, OperationType.UnaryPlus, OperationType.UnaryMinus
                }.Contains(Type);
            }
        }

        public static Operation From(string view)
        {
            var result = new Operation();

            switch(view)
            {
                case "new":
                    result.Type = OperationType.New;
                    result.Priority = 12; // TODO: Fix
                    break;
                case ".":
                    result.Type = OperationType.MemberAccess;
                    result.Priority = 12; // TODO: Fix
                    break;
                case "+":
                    result.Type = OperationType.Add;
                    result.Priority = 10;
                    break;
				case "-":
					result.Type = OperationType.Sub;
					result.Priority = 10;
					break;
				case "*":
					result.Type = OperationType.Mul;
					result.Priority = 11;
					break;
				case "/":
					result.Type = OperationType.Div;
					result.Priority = 11;
					break;
				case "%":
					result.Type = OperationType.Mod;
					result.Priority = 11;
					break;
				case "|":
					result.Type = OperationType.BinOr;
					result.Priority = 4;
					break;
				case "&":
					result.Type = OperationType.BinAnd;
					result.Priority = 6;
					break;
                case "()":
					result.Type = OperationType.FunctionCall;
					result.Priority = 12;
					break;
				case "!":
					result.Type = OperationType.Not;
					result.Priority = 12;
					break;
				case "~":
                    result.Type = OperationType.Inv;
					result.Priority = 12;
					break;
				case "^":
					result.Type = OperationType.Xor;
					result.Priority = 5;
					break;
                // Enable after Lab4
				//case ":=":
				//	result.Type = OperationType.Assign;
				//	result.Priority = 1;
				//	break;
				case "=":
					result.Type = OperationType.Assign;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "[]":
                    result.Type = OperationType.ArrayGet;
					result.Priority = 13;
					break;
				case "++":
					result.Type = OperationType.Inc;
					result.Priority = 12;
					break;
				case "--":
					result.Type = OperationType.Dec;
					result.Priority = 12;
					break;
				// Enable after Lab 4
				case "==":
				  result.Type = OperationType.Equals;
				  result.Priority = 7;
				  break;
				case "!=":
					result.Type = OperationType.NotEquals;
					result.Priority = 7;
					break;
				case "<=":
					result.Type = OperationType.LowerEquals;
					result.Priority = 8;
					break;
				case ">=":
					result.Type = OperationType.GreaterEquals;
					result.Priority = 8;
					break;
				case "<":
					result.Type = OperationType.Lower;
					result.Priority = 8;
					break;
				case ">":
					result.Type = OperationType.Greater;
					result.Priority = 8;
					break;
				case ">>":
					result.Type = OperationType.ShiftRight;
					result.Priority = 9;
					break;
				case "<<":
					result.Type = OperationType.ShiftLeft;
					result.Priority = 9;
					break;
                // Enable after Lab4
				case "||":
					result.Type = OperationType.Or;
					result.Priority = 2;
					break;
				case "&&":
					result.Type = OperationType.And;
					result.Priority = 3;
					break;
				//case "or":
				//	result.Type = OperationType.Or;
				//	result.Priority = 2;
				//	break;
				//case "and":
				//	result.Type = OperationType.And;
				//	result.Priority = 3;
				//	break;
				case "+=":
					result.Type = OperationType.AssignAdd;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "-=":
					result.Type = OperationType.AssignSub;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "*=":
					result.Type = OperationType.AssignMul;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "/=":
					result.Type = OperationType.AssignDiv;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "|=":
					result.Type = OperationType.AssignBinOr;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "&=":
					result.Type = OperationType.AssignBinAnd;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "^=":
					result.Type = OperationType.AssignXor;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "%=":
					result.Type = OperationType.AssignMod;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case "<<=":
					result.Type = OperationType.AssignShiftLeft;
					result.Priority = 1;
					result.Association = Association.Right;
					break;
				case ">>=":
                    result.Type = OperationType.AssignShiftRight;
                    result.Priority = 1;
                    result.Association = Association.Right;
                    break;
			}

            result.View = view;

            return result;
        }
    }

	public class OperatorPriority
    {
		List<string[]> priorityList;

		public OperatorPriority()
		{
			priorityList = new List<string[]>();
		}

		public void Add(params string[] ops)
		{
			priorityList.Add(ops);
		}

		public int GetPriority(string op)
		{
			for (int i = 0; i < priorityList.Count; ++i)
			{
				var ops = priorityList[i];
				if (ops != null && ops.Contains(op)) return i;
			}
			return -1;
		}

		public int Compare(string op1, string op2)
		{
			var prior1 = GetPriority(op1);
			var prior2 = GetPriority(op2);
			if (prior1 == prior2) return 0;
			else if (prior1 > prior2) return 1;
			else return -1;
		}

		public bool HasBiggerPriority(string op1, string op2)
		{
			var prior1 = GetPriority(op1);
			var prior2 = GetPriority(op2);
			if (prior1 > prior2) return true;
			else return false;
		}

		public bool HasLowerPriority(string op1, string op2)
		{
			var prior1 = GetPriority(op1);
			var prior2 = GetPriority(op2);
			if (prior1 < prior2) return true;
			else return false;
		}

		public bool HasSamePriority(string op1, string op2)
		{
			var prior1 = GetPriority(op1);
			var prior2 = GetPriority(op2);
			if (prior1 == prior2) return true;
			else return false;
		}
	}
}
