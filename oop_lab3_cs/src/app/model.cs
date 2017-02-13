using System;
using System.Collections.Generic;
using oop_lab3_cs.app.hierarchy;


namespace oop_lab3_cs.app.model { 

    public class ModelLogicError : Exception {
        public ModelLogicError(string msg) : base(msg) {}
    };

    
    public class Base {
        protected void check(bool condition, string msg= "") {
            if (!condition) throw new ModelLogicError(msg);
        }

    };

    public class Company : Base {

        public readonly string Name;
        public Employee Director;

        public Company(string name, Employee director): base() {
            Director = director;
            Name = name;
        }

    }

    public class Employee : Base {

        // personal info
        public readonly string FirstName;
        public readonly string LastName;

        // employment info
        private Company company;
        private string position;
        private Employee supervisor;
        private List<Employee> subordinates;
        private int salary;

        public Employee(string first_name, string last_name): base() {
            FirstName = first_name;
            LastName = last_name;
            salary = 0;
            subordinates = new List<Employee>();
        }

        public Company Company { get { return company; } }
        public string Position { get { return position; } }
        public int Salary {
            get { return salary; }
            set { this.salary = value; }
        }
        public Employee Supervisor {
            get { return supervisor; }
            set {
                RemoveSupervisor();
                supervisor = value;
                value.subordinates.Add(this);
                company = value.Company;
            }
        }
        public bool IsEmployed { get { return company != null; } }
        public List<Employee> Subordinates { get { return subordinates; } }
        public bool IsSupervisorOf(Employee empl) {
            foreach(var sub in subordinates) {
                if (sub == empl) return true;
            }
            return false;
        }

        public void Employ(Employee supervisor, string position, int salary) {
            check(!IsEmployed);
            this.company = supervisor.Company;
            Transfer(supervisor, position);
            this.salary = salary;
        }
        public void Transfer(Employee supervisor, string position) {
            check(IsEmployed);
            if (supervisor != null) this.Supervisor = supervisor;
            if (position != null) this.position = position;
        }
        public void Transfer(Employee supervisor) { Transfer(supervisor, null); }
        public void Transfer(string position) { Transfer(null, position); }
        public void LeaveCompany() {
            check(IsEmployed);
            if (this == company.Director) {
                // all employees leave the company.
                // we do it in reverse order to avoid transfers
                // after deletion from the non-terminal hierarchy node
                while (Subordinates.Count > 0) {
                    var department = StaffFlattener.GetDepartment<ByLevel>(Subordinates[0]);
                    department.Reverse();
                    foreach(var empl in department) {
                        empl.LeaveCompany();
                    }
                }
            } else {
                while(Subordinates.Count > 0) {
                    Subordinates[0].Supervisor = supervisor;
                }
                RemoveSupervisor();
            }
            company.Director = null;
            company = null;
            position = null;
            salary = 0;
        }
        public Company CreateCompany(string name) {
            check(!IsEmployed);
            company = new Company(name, this);
            position = "CEO";
            salary = 0;
            return company;
        }
        protected void RemoveSupervisor() {
            if (supervisor != null) {
                for (int i = 0; i < supervisor.Subordinates.Count; ++i) {
                    if (this == supervisor.Subordinates[i]) {
                        supervisor.Subordinates.RemoveAt(i);
                    }
                }
                supervisor = null;
            }
        }
    }

    class StaffFlattener : EmployeeVisitor {

        List<Employee> result;

        public override void Visit(Employee empl) {
            result.Add(empl);
        }

        public static List<Employee> GetDepartment<Order>(Employee dpt_head) where Order : HierarchyIterator {
            StaffFlattener sf = new StaffFlattener();
            sf.result = new List<Employee>();
            sf.VisitAll<Order>(dpt_head);
            return sf.result;
        }

    }

}