using System;
using System.Collections.Generic;
using System.IO;
using oop_lab3_cs.app.model;
using oop_lab3_cs.app.hierarchy;
using oop_lab3_cs.app.db;
using oop_lab3_cs.app.queries;
using oop_lab3_cs.shell.functions;


namespace oop_lab3_cs.app.shell_api {

    public class AppShellFunctions {

        [ExposeToShell]
        public static Employee create_employee(string fn, string ln) { return new Employee(fn, ln); }

        [ExposeToShell]
        public static string get_first_name(Employee empl) { return empl.FirstName; }

        [ExposeToShell]
        public static string get_last_name(Employee empl) {return empl.LastName;}

        [ExposeToShell]
        public static Company get_company(Employee empl) {return empl.Company;}

        [ExposeToShell]
        public static string get_position(Employee empl) {return empl.Position;}

        [ExposeToShell]
        public static int get_salary(Employee empl) {return empl.Salary;}

        [ExposeToShell]
        public static Employee get_supervisor(Employee empl) {return empl.Supervisor;}

        [ExposeToShell]
        public static bool is_employed(Employee empl) { return empl.IsEmployed; }

        [ExposeToShell]
        public static List<Employee> get_subordinates(Employee empl) { return empl.Subordinates; }

        [ExposeToShell]
        public static Employee get_subordinate_at(Employee empl, int index) {
            if (0 > index || index >= empl.Subordinates.Count) {
                throw new Exception("index out of bounds");
            }
            return empl.Subordinates[index];
        }

        [ExposeToShell]
        public static bool is_supervisor_of(Employee empl1, Employee empl2) {
            return empl1.IsSupervisorOf(empl2);
        }

        [ExposeToShell]
        public static void employ(Employee empl, Employee sv, string pos, int slr) {
            empl.Employ(sv, pos, slr);
        }

        [ExposeToShell]
        public static void transfer_full(Employee empl, Employee sv, string pos) {
            empl.Transfer(sv, pos);
        }
        [ExposeToShell]
        public static void transfer_sv(Employee empl, Employee supervisor) {empl.Transfer(supervisor);}

        [ExposeToShell]
        public static void transfer_pos(Employee empl, string position) {empl.Transfer(position);}

        [ExposeToShell]
        public static void set_salary(Employee empl, int salary) {empl.Salary = salary;}

        [ExposeToShell]
        public static void leave_company(Employee empl) {empl.LeaveCompany();}

        [ExposeToShell]
        public static Company create_company(Employee empl, string name) {return empl.CreateCompany(name);}

        [ExposeToShell]
        public static Employee get_director(Company c) {return c.Director;}

        [ExposeToShell]
        public static string get_company_name(Company c) {return c.Name;}

        [ExposeToShell]
        public static void save_company(Company cmp, string file) {
            TextWriter writer = File.CreateText(file);
            DB.save(cmp, writer);
            writer.Close();
        }
        [ExposeToShell]
        public static Company load_company(string file) {
            TextReader reader = File.OpenText(file);
            Company comp = DB.load(reader);
            reader.Close();
            return comp;
        }

        [ExposeToShell]
        public static List<Employee> filter_by_salary(Company company, int salary) {
            return new Query<BySubordination>(
                (Employee empl, int depth) => empl.Salary >= salary
            ).run(company);
        }

        [ExposeToShell]
        public static List<Employee> filter_by_position(Company company, string position) {
            return new Query<BySubordination>(
                (Employee empl, int depth) => empl.Position == position
            ).run(company);
        }

        [ExposeToShell]
        public static string hierarchy_to_text(Company company, string mode) {
            HierarchyIterator it;
            if (mode == "level") {
                it = new ByLevel(company.Director);
            } else if (mode == "subord") {
                it = new BySubordination(company.Director);
            } else {
                throw new Exception("Incorrect mode: " + mode
                    + ". Expected: \"level\" or \"subord\"");
            }
            var writer = new StringWriter();
            while (it.HasNext()) {
                Employee empl = it.Next();
                int depth = it.GetDepth();
                for(int i = 0; i<depth; i++) {
                    writer.Write("   ");
                }
                writer.Write(empl.Position);
                writer.Write(": ");
                writer.Write(empl.FirstName);
                writer.Write(" ");
                writer.Write(empl.LastName);
                writer.Write("($");
                writer.Write(empl.Salary);
                writer.WriteLine(")");
            }
            writer.Close();
            return writer.ToString();
        }
    }

}
