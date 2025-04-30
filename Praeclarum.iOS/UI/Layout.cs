#nullable enable
//
// Copyright (c) 2013-2025 Frank A. Krueger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Foundation;
using UIKit;

namespace Praeclarum.UI
{
	public static class Layout
	{
		/// <summary>
		/// <para>Constrains the layout of subviews according to equations and
		/// inequalities specified in <paramref name="constraints"/> and adds
		/// those constraints to the <paramref name="view"/>. Issue
		/// multiple constraints per call using the &amp;&amp; operator.</para>
		/// <para>e.g. button.Frame.Left &gt;= text.Frame.Right + 22 &amp;&amp;
		/// button.Frame.Width == View.Frame.Width * 0.42f</para>
		/// </summary>
		/// <param name="view">The superview laying out the referenced subviews.</param>
		/// <param name="constraints">Constraint equations and inequalities.</param>
		public static void AddLayoutConstraints (this UIView view, Expression<Func<bool>> constraints)
        {
            var cs = ConstrainLayout (view, constraints, UILayoutPriority.Required);
            view.AddConstraints (cs);
        }

		/// <summary>
		/// <para>Constrains the layout of subviews according to equations and
		/// inequalities specified in <paramref name="constraints"/>.  Issue
		/// multiple constraints per call using the &amp;&amp; operator.</para>
		/// <para>e.g. button.Frame.Left &gt;= text.Frame.Right + 22 &amp;&amp;
		/// button.Frame.Width == View.Frame.Width * 0.42f</para>
		/// </summary>
		/// <param name="view">The superview laying out the referenced subviews.</param>
		/// <param name="constraints">Constraint equations and inequalities.</param>
		public static NSLayoutConstraint[] ConstrainLayout (this UIView view, Expression<Func<bool>> constraints)
		{
			return ConstrainLayout (view, constraints, UILayoutPriority.Required);
		}

		/// <summary>
		/// <para>Constrains the layout of subviews according to equations and
		/// inequalities specified in <paramref name="constraints"/>.  Issue
		/// multiple constraints per call using the &amp;&amp; operator.</para>
		/// <para>e.g. button.Frame.Left &gt;= text.Frame.Right + 22 &amp;&amp;
		/// button.Frame.Width == View.Frame.Width * 0.42f</para>
		/// </summary>
		/// <param name="view">The superview laying out the referenced subviews.</param>
		/// <param name="constraints">Constraint equations and inequalities.</param>
		/// <param name = "priority">The priority of the constraints</param>
		public static NSLayoutConstraint[] ConstrainLayout (this UIView view, Expression<Func<bool>> constraints, UILayoutPriority priority)
		{
			var body = constraints.Body;

			var exprs = new List<BinaryExpression> ();
			FindConstraints (body, exprs);

			var layoutConstraints = exprs.Select (e => CompileConstraint (e, view)).ToArray ();

			if (layoutConstraints.Length > 0) {
				foreach (var c in layoutConstraints) {
					c.Priority = (float)priority;
				}
				view.AddConstraints (layoutConstraints);
			}

			return layoutConstraints;
		}

		static NSLayoutConstraint CompileConstraint (BinaryExpression expr, UIView constrainedView)
		{
			var rel = NSLayoutRelation.Equal;
			rel = expr.NodeType switch
			{
				ExpressionType.Equal => NSLayoutRelation.Equal,
				ExpressionType.LessThanOrEqual => NSLayoutRelation.LessThanOrEqual,
				ExpressionType.GreaterThanOrEqual => NSLayoutRelation.GreaterThanOrEqual,
				_ => throw new NotSupportedException ("Not a valid relationship for a constrain."),
			};

			if (rel == NSLayoutRelation.Equal && IsAnchor (expr.Left) && IsAnchor (expr.Right)) {
                return GetAnchorConstraint (expr);
            }

			var left = GetViewAndAttribute (expr.Left);
			if (left.View != constrainedView && left.View is UIView lview) {
                lview.TranslatesAutoresizingMaskIntoConstraints = false;
            }

			var right = GetRight (expr.Right);

			return NSLayoutConstraint.Create (
				left.View, left.Attribute,
				rel,
				right.View, right.Attribute,
				right.Item3, right.Item4);
		}

		static (NSObject? View, NSLayoutAttribute Attribute, nfloat, nfloat) GetRight (Expression expr)
		{
			var r = expr;

			NSObject? view = null;
			NSLayoutAttribute attr = NSLayoutAttribute.NoAttribute;
			nfloat mul = 1;
			nfloat add = 0;
			var pos = true;

			if (r.NodeType == ExpressionType.Add || r.NodeType == ExpressionType.Subtract) {
				var rb = (BinaryExpression)r;
				if (IsConstant (rb.Left)) {
					add = ConstantValue (rb.Left);
					if (r.NodeType == ExpressionType.Subtract) {
						pos = false;
					}
					r = rb.Right;
				}
				else if (IsConstant (rb.Right)) {
					add = ConstantValue (rb.Right);
					if (r.NodeType == ExpressionType.Subtract) {
						add = -add;
					}
					r = rb.Left;
				}
				else {
					throw new NotSupportedException ("Addition only supports constants: " + rb.Right.NodeType);
				}
			}

			if (r.NodeType == ExpressionType.Multiply) {
				var rb = (BinaryExpression)r;
				if (IsConstant (rb.Left)) {
					mul = ConstantValue (rb.Left);
					r = rb.Right;
				}
				else if (IsConstant (rb.Right)) {
					mul = ConstantValue (rb.Right);
					r = rb.Left;
				}
				else {
					throw new NotSupportedException ("Multiplication only supports constants.");
				}
			}

			if (IsConstant (r)) {
				add = ConstantValue (r);
			} else if (r.NodeType == ExpressionType.MemberAccess || r.NodeType == ExpressionType.Call) {
				var t = GetViewAndAttribute (r);
				view = t.View;
				attr = t.Attribute;
			} else {
				throw new NotSupportedException ("Unsupported layout expression node type " + r.NodeType);
			}

			if (!pos)
				mul = -mul;

			return (view, attr, mul, add);
		}

		static bool IsConstant (Expression expr)
		{
			if (expr.NodeType == ExpressionType.Constant)
				return true;

			if (expr.NodeType == ExpressionType.MemberAccess) {
				var mexpr = (MemberExpression)expr;
				var m = mexpr.Member;
				if (m.MemberType == MemberTypes.Field) {
					return true;
				}
				return false;
			}

			if (expr.NodeType == ExpressionType.Convert) {
				var cexpr = (UnaryExpression)expr;
				return IsConstant (cexpr.Operand);
			}

			return false;
		}

		static nfloat ConstantValue (Expression expr)
		{
			var evalConst = Eval (expr);
			if (evalConst is nfloat nf) {
				return nf;
			}
			else if (evalConst is nint ni) {
				return (nfloat)ni;
			}
			else {
				try {
					return (nfloat)Convert.ToDouble (evalConst);
				}
				catch (Exception ex) {
					Log.Error (ex);
					return 0;
				}
			}
		}

		static (NSObject View, NSLayoutAttribute Attribute) GetViewAndAttribute (Expression expr)
		{
			var attr = NSLayoutAttribute.NoAttribute;
			MemberExpression? frameExpr = null;

			var fExpr = expr as MethodCallExpression;
			if (fExpr != null) {
				switch (fExpr.Method.Name) {
				case "GetMidX":
				case "GetCenterX":
					attr = NSLayoutAttribute.CenterX;
					break;
				case "GetMidY":
				case "GetCenterY":
					attr = NSLayoutAttribute.CenterY;
					break;
				case "GetBaseline":
					attr = NSLayoutAttribute.Baseline;
					break;
				default:
					throw new NotSupportedException ("Method " + fExpr.Method.Name + " is not recognized.");
				}

				frameExpr = fExpr.Arguments.FirstOrDefault () as MemberExpression;
			}

			if (attr == NSLayoutAttribute.NoAttribute) {
				var memExpr = expr as MemberExpression;
                if (memExpr == null && expr.NodeType == ExpressionType.Convert && expr is UnaryExpression convert)
                    memExpr = convert.Operand as MemberExpression;
                if (memExpr == null)
                    throw new NotSupportedException ("Left hand side of a relation must be a member expression, instead it is " + expr);

				switch (memExpr.Member.Name) {
					case "Width":
						attr = NSLayoutAttribute.Width;
						break;
					case "Height":
						attr = NSLayoutAttribute.Height;
						break;
					case "Left":
					case "X":
						attr = NSLayoutAttribute.Left;
						break;
					case "Top":
					case "Y":
						attr = NSLayoutAttribute.Top;
						break;
					case "Right":
						attr = NSLayoutAttribute.Right;
						break;
					case "Bottom":
						attr = NSLayoutAttribute.Bottom;
						break;
					default:
						throw new NotSupportedException ("Property " + memExpr.Member.Name + " is not recognized.");
				}

				frameExpr = memExpr.Expression as MemberExpression;
			}

			if (frameExpr == null)
				throw new NotSupportedException ("Constraints should use the Frame or Bounds property of views.");
			var viewExpr = frameExpr.Expression;

			if (viewExpr is null || !(Eval (viewExpr) is UIView view))
                throw new NotSupportedException ("Constraints only apply to views.");

			if (frameExpr.Member.Name == "SafeAreaLayoutGuide") {
                return (view.SafeAreaLayoutGuide, attr);
            }
            else {
                return (view, attr);
            }
		}

		static object? Eval (Expression expr)
		{
			if (expr.NodeType == ExpressionType.Constant && expr is ConstantExpression constExpr) {
				return constExpr.Value;
			}
			
			if (expr.NodeType == ExpressionType.MemberAccess) {
				var mexpr = (MemberExpression)expr;
				var m = mexpr.Member;
				if (m.MemberType == MemberTypes.Field) {
					var f = (FieldInfo)m;
					var v = f.GetValue (mexpr.Expression != null ? Eval (mexpr.Expression) : null);
					return v;
				}
			}

			if (expr.NodeType == ExpressionType.Convert && expr is UnaryExpression convExpr) {
				var op = Eval (convExpr.Operand);
				if (convExpr.Method != null) {
					return convExpr.Method.Invoke (null, new[]{ op });
				} else {
					return Convert.ChangeType (op, convExpr.Type);
				}
			}

			return Expression.Lambda (expr).Compile ().DynamicInvoke ();
		}

		static void FindConstraints (Expression expr, List<BinaryExpression> constraintExprs)
		{
			var b = expr as BinaryExpression;
			if (b == null)
				return;

			if (b.NodeType == ExpressionType.AndAlso) {
				FindConstraints (b.Left, constraintExprs);
				FindConstraints (b.Right, constraintExprs);
			} else {
				constraintExprs.Add (b);
			}
		}

		static bool IsAnchor (Expression expr)
        {
            return expr is MemberExpression m && m.Member.Name.EndsWith ("Anchor", StringComparison.Ordinal);
        }

        static NSObject? GetAnchor (Expression expr)
        {
            return Eval (expr) as NSObject;
        }

        static NSLayoutConstraint GetAnchorConstraint (BinaryExpression binary)
        {
            var left = GetAnchor (binary.Left);
            var right = GetAnchor (binary.Right);
            if (left != null && right != null) {
                var t = left.GetType ();
                var m = t.GetMethods ().FirstOrDefault (x => x.Name == "ConstraintEqualTo" && x.GetParameters ().Length == 1);
                if (m?.Invoke (left, new object[] { right }) is NSLayoutConstraint r)
					return r;
            }
            throw new Exception ("Failed to get the left and right anchors from " + binary);
        }

		/// <summary>
		/// The baseline of the view whose frame is viewFrame. Use only when defining constraints.
		/// </summary>
		public static nfloat GetBaseline (this CoreGraphics.CGRect viewFrame)
		{
			return 0;
		}

		/// <summary>
		/// The x coordinate of the center of the frame.
		/// </summary>
		public static nfloat GetCenterX (this CoreGraphics.CGRect viewFrame)
		{
			return viewFrame.X + viewFrame.Width / 2;
		}

		/// <summary>
		/// The y coordinate of the center of the frame.
		/// </summary>
		public static nfloat GetCenterY (this CoreGraphics.CGRect viewFrame)
		{
			return viewFrame.Y + viewFrame.Height / 2;
		}
	}
}
