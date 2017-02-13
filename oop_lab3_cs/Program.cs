using System;
using System.Collections.Generic;
using oop_lab3_cs.shell.interpreter;
using oop_lab3_cs.shell.builtins;
using oop_lab3_cs.shell.functions;


namespace oop_lab3_cs {
    class Program {
        static void Main(string[] args) {
            var world = new World(
                ShFunction.Gather(typeof(ShellBuiltins)),
        //merge_dicts(builtins, {
        //    {"create_employee", create_employee},
        //    {"first_name", get_first_name},
        //    {"last_name", get_last_name},
        //    {"company", get_company},
        //    {"position", get_position},
        //    {"salary", get_salary},
        //    {"supervisor", get_supervisor},
        //    {"is_employed", is_employed},
        //    {"subordinates", get_subordinates},
        //    {"subordinate_at", get_subordinate_at},
        //    {"test_relation", is_supervisor_of},
        //    {"employ", employ},
        //    {"transfer", transfer_full},
        //    {"resubordinate", transfer_sv},
        //    {"rename_position", transfer_pos},
        //    {"set_salary", set_salary},
        //    {"leave", leave_company},
        //    {"new_company", create_company},
        //    {"director", get_director},
        //    {"company_name", get_company_name},
        //    {"save_to_file", save_company},
        //    {"load_from_file", load_company},
        //    {"filter_by_salary", filter_by_salary},
        //    {"filter_by_position", filter_by_position},
        //    {"print_hierarchy", hierarchy_to_text},
        //}), 
            new Dictionary<string, shell.objects.ShObject>{ });
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
