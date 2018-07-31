﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLanguage.Interpreter;
using CLanguage.Types;

namespace CLanguage.Syntax
{
    public abstract class Expression
    {
        public Location Location { get; protected set; }

        public bool HasError { get; set; }

        public void Emit(EmitContext ec)
        {
            DoEmit(ec);
        }

        public void EmitPointer (EmitContext ec)
        {
            DoEmitPointer (ec);
        }

		public abstract CType GetEvaluatedCType (EmitContext ec);

        protected abstract void DoEmit(EmitContext ec);
        protected virtual void DoEmitPointer (EmitContext ec) =>
            throw new NotSupportedException ($"Cannot get address of {this.GetType().Name} `{this}`");

		protected static int GetInstructionOffset (CBasicType aType, EmitContext ec)
		{
			var size = aType.GetByteSize (ec);

            if (aType.IsIntegral) {
                switch (size) {
                    case 1:
                        return aType.Signedness == Signedness.Signed ? 0 : 1;
                    case 2:
                        return aType.Signedness == Signedness.Signed ? 2 : 3;
                    case 4:
                        return aType.Signedness == Signedness.Signed ? 4 : 5;
                    case 8:
                        return aType.Signedness == Signedness.Signed ? 6 : 7;
                }
            }
            else {
                switch (size) {
                    case 4:
                        return 8;
                    case 8:
                        return 9;
                }
            }

			throw new NotSupportedException ("Arithmetic on type '" + aType + "'");
		}

		protected static CBasicType GetPromotedType (Expression expr, string op, EmitContext ec)
		{
			var leftType = expr.GetEvaluatedCType (ec);

			var leftBasicType = leftType as CBasicType;

			if (leftBasicType == null) {
				ec.Report.Error (19, "Operator '" + op + "' cannot be applied to operand of type '" + leftType + "'");
				return CBasicType.SignedInt;
			} else {
				return leftBasicType.IntegerPromote (ec);
			}
		}

		protected static CBasicType GetArithmeticType (Expression leftExpr, Expression rightExpr, string op, EmitContext ec)
		{
			var leftType = leftExpr.GetEvaluatedCType (ec);
			var rightType = rightExpr.GetEvaluatedCType (ec);

			var leftBasicType = leftType as CBasicType;
			var rightBasicType = rightType as CBasicType;

			if (leftBasicType == null || rightBasicType == null) {
				ec.Report.Error (19, "Operator '" + op + "' cannot be applied to operands of type '" + leftType + "' and '" + rightType + "'");
				return CBasicType.SignedInt;
			} else {
				return leftBasicType.ArithmeticConvert (rightBasicType, ec);
			}
		}
    }
}
