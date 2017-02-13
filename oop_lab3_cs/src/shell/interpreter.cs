using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using oop_lab3_cs.shell.objects;
using oop_lab3_cs.shell.functions;
using oop_lab3_cs.shell.grammar;
using oop_lab3_cs.shell.tokens;


namespace oop_lab3_cs.shell.interpreter {

    public class InvalidNameError : Exception {
        private bool _is_function;

        public InvalidNameError(string name, bool is_function) : base(name) {
            this._is_function = is_function;
        }

        public bool is_function() { return _is_function; }
    }

    public class Interrupted : Exception {
        public Interrupted(): base("") {}
    };


    public class World {

        private Dictionary<string, ShFunction> functions;
        private Dictionary<string, ShObject> variables;

        public World(Dictionary<string, ShFunction> functions,
                     Dictionary<string, ShObject> variables) {
            this.functions = functions;
            this.variables = variables;
        }

        public bool HasVariable(string name) { return variables.ContainsKey(name); }
        public bool HasFunction(string name) { return functions.ContainsKey(name); }

        public ShFunction GetFunction(string name) { return functions[name]; }
        public ShObject GetVariable(string name) { return variables[name]; }

        public void SetVariable(string name, ShObject value) { variables[name] = value; }
        public void DeleteVariable(string name) { variables.Remove(name); }

        public ISet<string> GetFunctionNames() {
            ISet<string> result = new HashSet<string>();
            foreach(var name in functions.Keys) {
                result.Add(name);
            }
            return result;
        }
        public ISet<string> GetVariableNames() {
            ISet<string> result = new HashSet<string>();
            foreach (var name in variables.Keys) {
                result.Add(name);
            }
            return result;
        }

    };


    public class Interpreter {

        private World world;
        
        public Interpreter(Dictionary<string, ShFunction> functions,
                    Dictionary<string, ShObject> variables) {
            this.world = new World(functions, variables);
        }
        public Interpreter(World world) {
            this.world = world;
        }

        public string Exec(string input, out bool failbit){
            failbit = false;
            try {
                // empty input - empty output, it's just an empty line
                if (input.Length == 0) { return ""; }

                // print help - list of existing functions
                if (input == "??" || input == "?") {
                    string str = "Defined functions:\n";
                    foreach(string func_name in world.GetFunctionNames()) {
                        str += "\n" + func_name;
                    }
                    return str;
                }

                // print help on specific function
                if (input[0] == '?') {
                    string func_name = input.Substring(1);
                    if (world.HasFunction(func_name)) {
                        var help = world.GetFunction(func_name).HelpText;
                        if (help == null) help = "";
                        return "Help on function " + func_name + ":\n\n" + help;
                    } else {
                        return "Name error: function " + func_name;
                    }
                }

                var tokens = Token.ParseAll(input);
                var statement = Statement.Parse(tokens);
                var expr = statement.GetExpr();

                new NamesValidator(world).Validate(expr);
                var value = new ExprEvaluator(world).Evaluate(expr);

                if (statement.HasAssignments()) {
                    if (value.IsEmpty) {
                        foreach(string name in statement.GetAssignments()) {
                            world.DeleteVariable(name);
                        }
                    } else {
                        foreach(string name in statement.GetAssignments()) {
                            world.SetVariable(name, value);
                        }
                    }
                    return "";
                } else {
                    return value.ToString();
                }

            } catch (SyntaxError err) {
                failbit = true;
                return "Syntax error: " + err.Message;
            } catch (InvalidNameError err) {
                failbit = true;
                return "Name error: "
                       + (err.is_function() ? "function " : "variable ")
                       + err.Message;
            } catch (ShellError err) {
                failbit = true;
                return "Error: " + err.Message;
            } catch (Interrupted err) {
                throw err;
            } catch (Exception err) {
                failbit = true;
                return "Unexpected failure: " + err.Message;
            }
        }

        public void Execfile(string filename){
            var reader = new StreamReader(filename);
            if (reader.EndOfStream) {
                Console.Error.WriteLine("Can't open program file");
                return;
            }
            bool failbit;
            while (!reader.EndOfStream) {
                try {
                    string line = reader.ReadLine();
                    string response = Exec(line, out failbit);
                    if (response.Length > 0 & failbit) {
                        Console.Error.Write(response);
                        break;
                    }
                } catch (Interrupted) { break; }
            }
        }

        public void InteractiveLoop(TextReader in_stream, TextWriter out_stream) {
            bool failbit;
            while (true) {
                try {
                    out_stream.Write(">>> ");
                    string line = in_stream.ReadLine();
                    string response = Exec(line, out failbit);
                    if (response.Length > 0) {
                        out_stream.WriteLine(response);
                    }
                } catch (Interrupted) { break; }
            }
        }

    };

    public class NamesValidator : ExpressionVisitor {
        private ISet<string> function_names;
        private ISet<string> variable_names;

        public NamesValidator(ISet<string> function_names, ISet<string> variable_names) : base() {
            this.function_names = function_names;
            this.variable_names = variable_names;
        }
        public NamesValidator(World world) : base() { 
            this.function_names = world.GetFunctionNames();
            this.variable_names = world.GetVariableNames();
        }

        public void Validate(Expression expr) {
            expr.Accept(this);
        }

        public override void Visit(FunctionCall fc) {
            if (!function_names.Contains(fc.GetName())) {
                throw new InvalidNameError(fc.GetName(), true);
            }
            foreach (var child in fc.GetChildren()) {
                child.Accept(this);
            }
        }
        public override void Visit(Variable v) {
            if (!variable_names.Contains(v.GetName())) {
                throw new InvalidNameError(v.GetName(), false);
            }
        }
        public override void Visit(NumberLiteral nl) { }
        public override void Visit(StringLiteral sl) { }

    };

    public class ExprEvaluator : ExpressionVisitor {

        private World world;
        private ShObject _tmp_value;

        public ExprEvaluator(World world) {
            this.world = world;
        }

        public ShObject Evaluate(Expression expr) {
            expr.Accept(this);
            return _tmp_value;
        }

        public override void Visit(FunctionCall fc) {
            var args = new Dictionary<string, ShObject>();
            foreach (var item in fc.GetArgs()) {
                item.Value.Accept(this);
                args[item.Key] = _tmp_value;
            }
            _tmp_value = world.GetFunction(fc.GetName()).Call(args);
        }
        public override void Visit(Variable v) {
            _tmp_value = world.GetVariable(v.GetName());
        }
        public override void Visit(NumberLiteral nl) {
            _tmp_value = ShObject.New<int>(nl.GetValue());
        }
        public override void Visit(StringLiteral sl) {
            _tmp_value = ShObject.New<string>(sl.GetValue());
        }

    };
}
