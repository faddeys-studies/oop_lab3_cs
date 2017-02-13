using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using oop_lab3_cs.shell.objects;


namespace oop_lab3_cs.shell.functions {

    [AttributeUsage(AttributeTargets.Method)]
    public class ExposeToShell : Attribute {
        public readonly string HelpText;

        public ExposeToShell(string text) { this.HelpText = text; }
    }


    public class ShFunction {

        private readonly MethodInfo method;
        private readonly string[] arg_names;
        private readonly Type[] arg_types;
        private readonly Type return_type;
        private readonly string help_text;

        public ShFunction(Type holder, string method_name, string help=null) 
            : this(holder.GetMethod(method_name), help) { }

        public ShFunction(MethodInfo method, string help=null) {
            if (!method.IsStatic) throw new ArgumentException("Only static methods are allowed");
            this.help_text = help;
            this.method = method;
            this.return_type = method.ReturnType;
            this.arg_types = (from param in method.GetParameters()
                              select param.ParameterType).ToArray();
            this.arg_names = (from param in method.GetParameters()
                              select param.Name).ToArray();
        }

        public ShObject Call(Dictionary<string, ShObject> arguments) {
            for (int idx = 0; idx < arg_names.Length; idx++) {
                if (!arguments.ContainsKey(arg_names[idx]))
                    throw new ShellError("Missing argument: " + arg_names[idx]);
                if (!arguments[arg_names[idx]].HasType(arg_types[idx])) {
                    throw new ShellError("Type mismatch: " + arg_names[idx]);
                }
            }
            if (arguments.Count > arg_names.Length)
                throw new ShellError("Unexpected arguments");
            var args = Enumerable.Zip(
                arg_names, arg_types,
                (arg_name, arg_type) => arguments[arg_name].Get(arg_type)
            ).ToArray();
            object result;
            try {
                result = method.Invoke(null, args);
            } catch (TargetInvocationException exc) {
                throw exc.InnerException;
            }
            if (return_type == typeof(void)) {
                return ShObject.Empty();
            } else {
                return ShObject.New(return_type, result);
            }
        }

        public static Dictionary<string, ShFunction> Gather(Type cls) {
            var result = new Dictionary<string, ShFunction>();
            foreach(var method in cls.GetMethods()) {
                if (!method.IsStatic) continue;
                var expose_attr = method.GetCustomAttribute<ExposeToShell>();
                if (expose_attr != null) {
                    result[method.Name] = new ShFunction(method, expose_attr.HelpText);
                }
            }
            return result;
        }

        public string HelpText { get { return help_text; } }

    }

}
