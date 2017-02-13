using System;
using System.Linq;
using System.Collections.Generic;
using oop_lab3_cs.shell.interpreter;
using oop_lab3_cs.shell.builtins;
using oop_lab3_cs.shell.objects;
using oop_lab3_cs.shell.functions;
using oop_lab3_cs.app.shell_api;


namespace oop_lab3_cs {
    class Program {

        static void Main(string[] args) {
            var functions = ShFunction.Gather(typeof(ShellBuiltins))
                .Union(ShFunction.Gather(typeof(AppShellFunctions)))
                .ToDictionary(k => k.Key, v => v.Value);
            var world = new World(
                functions, new Dictionary<string, ShObject>{ }
            );
            ShellBuiltins.SetWorld(world);

            if (args.Length == 0) {
                new Interpreter(world).InteractiveLoop(
                    Console.In, Console.Out
                );
            } else {
                new Interpreter(world).Execfile(args[0]);
            }

        }
    }
}
