using System.Collections.Generic;
using System;

using oop_lab3_cs.shell.objects;
using oop_lab3_cs.shell.functions;
using oop_lab3_cs.shell.interpreter;


namespace oop_lab3_cs.shell.builtins {

    public class ShellBuiltins {
        private static World global_world = null;

        public static void SetWorld(World w) {
            global_world = w;
        }

        [ExposeToShell("Returns list of all variables. Return type: List<string>")]
        public static List<string> dir() {
            return new List<string>(global_world.GetVariableNames());
        }


        [ExposeToShell("Concatenate two strings")]
        public static string strcat(string s1, string s2) {
            return s1 + s2;
        }


        [ExposeToShell("Prints value of variable to stdout. Returns nothing.")]
        public static void print(string var_name) {
            if (global_world.HasVariable(var_name)) {
                Console.Out.WriteLine(global_world.GetVariable(var_name).ToString());
            } else {
                throw new InvalidNameError(var_name, false);
            }
        }


        [ExposeToShell("Exits from the shell.")]
        public static void exit() {
            throw new Interrupted();
        }

    }

}

