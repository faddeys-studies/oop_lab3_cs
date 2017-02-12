
using System;
using System.Collections.Generic;
using System.Linq;
using oop_lab3_cs.shell.tokens;


namespace oop_lab3_cs.shell.grammar {

    using ArgsMap = Dictionary<string, Expression>;
    using AssignmentsList = List<string>;

    public abstract class ExpressionVisitor {
        public abstract void Visit(FunctionCall fc);
        public abstract void Visit(Variable v);
        public abstract void Visit(NumberLiteral nl);
        public abstract  void Visit(StringLiteral sl);
    }

    public abstract class Expression {

        public abstract void Accept(ExpressionVisitor v);

        public void Traverse(ExpressionVisitor v, bool acceptbefore) {
            if (acceptbefore) Accept(v);
            foreach(Expression child in GetChildren()) {
                child.Traverse(v, acceptbefore);
            }
            if (!acceptbefore) Accept(v);
        }

        public static bool operator==(Expression expr1, Expression expr2) {
            if (Object.ReferenceEquals(expr1, null)) return Object.ReferenceEquals(expr2, null);
            return expr1.Equals(expr2);
        }

        public static bool operator!=(Expression expr1, Expression expr2) {
            return !(expr1 == expr2);
        }

        public abstract List<Expression> GetChildren();
    }

    public class FunctionCall : Expression {

        private string name;
        private ArgsMap arguments;

        public FunctionCall(string name): this(name, new ArgsMap()) { }
        public FunctionCall(string name, ArgsMap args) {
            this.name = name;
            this.arguments = args;
        }

        public bool AddArgument(string name, Expression expr) {
            if (arguments.ContainsKey(name)) {
                return false;
            }
            arguments[name] = expr;
            return true;
        }
        public ArgsMap GetArgs() {
            return arguments;
        }

        public override void Accept(ExpressionVisitor v) { v.Visit(this); }

        public override List<Expression> GetChildren() {
            List<Expression> ret = new List<Expression>();
            foreach (var item in arguments) {
                ret.Add(item.Value);
            }
            return ret;
        }

        public string GetName() { return name; }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(FunctionCall)) return false;
            FunctionCall fc = (FunctionCall)other;
            if (name != fc.name) return false;
            if (arguments.Count != fc.arguments.Count) return false;
            foreach(var item in arguments) {
                Expression corresponding = null;
                if (!fc.arguments.TryGetValue(item.Key, out corresponding)) return false;
                if (!corresponding.Equals(item.Value)) return false;
            }
            return true;
        }
    }

    public class NumberLiteral : Expression {
        private int value;

        public NumberLiteral(int value) { this.value = value; }

        public override void Accept(ExpressionVisitor v) { v.Visit(this); }

        public override List<Expression> GetChildren()  {
            return new List<Expression>();
        }

        public int GetValue() { return value; }

        public override bool Equals(object other) {
            if (other is NumberLiteral) {
                var nl = (NumberLiteral)other;
                return this.value == nl.value;
            } else return false;
        }

    }

    public class StringLiteral : Expression {
        private string value;


        public StringLiteral(string value) { this.value = value; }

        public override void Accept(ExpressionVisitor v) { v.Visit(this); }

        public override List<Expression> GetChildren() {
            return new List<Expression>();
        }


        public string GetValue() { return value; }

        public override bool Equals(object other) {
            if (other is StringLiteral) {
                var sl = (StringLiteral)other;
                return this.value == sl.value;
            } else return false;
        }

    }

    public class Variable : Expression {
        private string name;

        public Variable(string name) { this.name = name; }

        public override void Accept(ExpressionVisitor v) { v.Visit(this); }

        public override List<Expression> GetChildren() {
            return new List<Expression>();
        }

        public string GetName() { return name; }
        
        public override bool Equals(object other) {
            if (other is Variable) {
                var v = (Variable)other;
                return this.name == v.name;
            } else return false;
        }

    };


    public class Statement {

        private Expression expr;
        private AssignmentsList assignments;
        
        public Statement(
            Expression expr,
            AssignmentsList assignments
        ) {
            this.expr = expr;
            this.assignments = assignments;
        }

        public bool HasAssignments() { return assignments.Count > 0; }
        public AssignmentsList GetAssignments() { return assignments; }
        public Expression GetExpr() { return expr; }

        public static bool operator ==(Statement first, Statement second) {
            if (Object.ReferenceEquals(first, null)) return Object.ReferenceEquals(second, null);
            return first.Equals(second);
        }

        public static bool operator !=(Statement first, Statement second) {
            return !(first == second);
        }

        public override bool Equals(object obj) {
            if (obj is Statement) {
                var stmt = (Statement)obj;
                bool exprs_ok = expr == stmt.expr
                   , assignments_ok = assignments.SequenceEqual(stmt.assignments);
                return exprs_ok && assignments_ok;
            } else return false;
        }

        public static Statement Parse(List<Token> tokens) {
            int idx = 0;

            // variable assignments should be first tokens is the statement
            List<string> assignments = new List<string>();
            while (idx < tokens.Count && tokens[idx].GetType() == typeof(AssignmentToken)) {
                var t = Token.TryConvert<AssignmentToken>(tokens[idx]);
                assignments.Add(t.Name);
                idx++;
            }

            Expression expr = _parse_expr(tokens, ref idx);
            if (idx<tokens.Count) throw new SyntaxError("unexpected tokens");

            return new Statement(expr, assignments);
        }


        private static Expression _parse_expr(List<Token> tokens, ref int idx) {
            if (idx >= tokens.Count) throw new SyntaxError("expected expression");

            if (tokens[idx] is NameToken) {
                var name_t = Token.TryConvert<NameToken>(tokens[idx]);
                // function call of variable
                ParenthesisToken open_par_tok = null;
                if (idx + 1 < tokens.Count)
                    open_par_tok = Token.TryConvert<ParenthesisToken>(tokens[idx + 1]);
                if (open_par_tok != null && open_par_tok.IsOpen) {
                    // it should be the function call
                    if (idx + 2 >= tokens.Count) throw new SyntaxError("expected function arguments");

                    // ok, we have parsed "function("
                    // now parse the arguments
                    idx += 2; // we parsed 2 tokens
                    var fc = new FunctionCall(name_t.Name);
                    while (tokens[idx] is AssignmentToken) {
                        var assign_tok = Token.TryConvert<AssignmentToken>(tokens[idx]);
                        idx++;
                        Expression sub_expr = _parse_expr(tokens, ref idx);  // <- this changes idx
                        fc.AddArgument(assign_tok.Name, sub_expr);

                        // we need to check for index bounds
                        // otherwise it may cause index error on next while() check
                        if (idx >= tokens.Count) throw new SyntaxError("expected closing parenthesis");
                    }
                    var close_par_tok = Token.TryConvert<ParenthesisToken>(tokens[idx]);
                    if (close_par_tok == null || close_par_tok.IsOpen) {
                        throw new SyntaxError("expected closing parenthesis");
                    }
                    idx++; // closing parenthesis

                    return fc;
                } else {
                    // it is not a function call, so we assume that it is a variable
                    idx++;
                    return new Variable(name_t.Name);
                }
            } else if (tokens[idx] is StringLiteralToken) {
                var slt = Token.TryConvert<StringLiteralToken>(tokens[idx]);
                idx++;
                return new StringLiteral(slt.Value);
            } else if (tokens[idx] is NumberLiteralToken) {
                var nlt = Token.TryConvert<NumberLiteralToken>(tokens[idx]);
                idx++;
                return new NumberLiteral(nlt.Value);
            } else {
                throw new SyntaxError("unexpected tokens");
            }
        }

    }
}

