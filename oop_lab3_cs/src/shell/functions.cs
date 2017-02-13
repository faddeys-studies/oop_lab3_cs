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

        public ShFunction(Type holder, string method_name) 
            : this(holder.GetMethod(method_name)) { }

        public ShFunction(MethodInfo method) {
            if (!method.IsStatic) throw new ArgumentException("Only static methods are allowed");
            this.method = method;
            this.return_type = method.ReturnType;
            this.arg_types = (from param in method.GetParameters()
                              select param.ParameterType).ToArray();
            this.arg_names = (from param in method.GetParameters()
                              select param.Name).ToArray();
        }

        public ShObject Call(Dictionary<string, ShObject> arguments) {
            foreach (var arg_name in arg_names) {
                if (!arguments.ContainsKey(arg_name))
                    throw new ShellError("Missing argument: " + arg_name);
            }
            if (arguments.Count > arg_names.Length)
                throw new ShellError("Unexpected arguments");
            var args = Enumerable.Zip(
                arg_names, arg_types,
                (arg_name, arg_type) => arguments[arg_name].Get(arg_type)
            ).ToArray();
            object result = method.Invoke(null, args);
            if (return_type == typeof(void)) {
                return ShObject.Empty();
            } else {
                return ShObject.New(return_type, result);
            }
        }

    }

}
