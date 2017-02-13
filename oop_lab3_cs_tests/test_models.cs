using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.app.model;

namespace oop_lab3_cs_tests {

    [TestClass]
    public class ModelsTests {

        [TestMethod]
        public void test_initial_state() {
            var e = new Employee("name", "surname");
            Assert.AreEqual(e.FirstName, "name");
            Assert.AreEqual(e.LastName, "surname");
            Assert.IsNull(e.Company);
            Assert.IsNull(e.Position);
            Assert.AreEqual(e.Salary, 0);
            Assert.IsNull(e.Supervisor);
            Assert.IsFalse(e.IsEmployed);
            Assert.AreEqual(e.Subordinates.Count, 0);
        }

        [TestMethod]
        public void test_create_company() {
            var e = new Employee("", "");
            var comp = e.CreateCompany("");
            Assert.AreEqual(e.Company, comp);
            Assert.AreEqual(e.Position, "CEO");
            Assert.AreEqual(e.Salary, 0);
            Assert.IsNull(e.Supervisor);
            Assert.IsTrue(e.IsEmployed);
            Assert.AreEqual(e.Subordinates.Count, 0);
            Assert.AreEqual(comp.Director, e);
        }

        [TestMethod]
        public void test_set_salary() {
            // set salary is available only if Employee is employed (or is CEO)
            var ceo = new Employee("", "");
            var empl = new Employee("", "");
            ceo.CreateCompany("");
            ceo.Salary = 500;
            Assert.AreEqual(ceo.Salary, 500);

            empl.Employ(ceo, "Manager", 100);
            Assert.AreEqual(empl.Salary, 100);

            empl.Salary = 250;
            Assert.AreEqual(empl.Salary, 250);
        }

        [TestMethod]
        public void test_employ_as_subordinate_of_ceo() {
            var ceo = new Employee("", "");
            var manager = new Employee("", "");
            var company = ceo.CreateCompany("");

            manager.Employ(ceo, "Manager", 150);
            Assert.AreEqual(manager.Company, company);
            Assert.AreEqual(manager.Position, "Manager");
            Assert.AreEqual(manager.Salary, 150);
            Assert.AreEqual(manager.Supervisor, ceo);
            Assert.IsTrue(manager.IsEmployed);
            Assert.AreEqual(manager.Subordinates.Count, 0);
            Assert.AreEqual(ceo.Subordinates.Count, 1);
            Assert.AreEqual(ceo.Subordinates[0], manager);
            Assert.IsTrue(ceo.IsSupervisorOf(manager));
        }

        [TestMethod]
        public void test_employ_as_subordinate_of_non_ceo() {
            var ceo = new Employee("", "");
            var manager = new Employee("", "");
            var empl = new Employee("", "");
            var company = ceo.CreateCompany("");
            manager.Employ(ceo, "Manager", 150);

            empl.Employ(manager, "Worker1", 100);
            Assert.AreEqual(empl.Company, company);
            Assert.AreEqual(empl.Position, "Worker1");
            Assert.AreEqual(empl.Salary, 100);
            Assert.AreEqual(empl.Supervisor, manager);
            Assert.IsTrue(empl.IsEmployed);
            Assert.AreEqual(empl.Subordinates.Count, 0);
            Assert.AreEqual(manager.Subordinates.Count, 1);
            Assert.AreEqual(manager.Subordinates[0], empl);
            Assert.AreEqual(ceo.Subordinates.Count, 1);
            Assert.AreEqual(ceo.Subordinates[0], manager);
        }

        [TestMethod]
        public void test_employ_with_neighborship() {
            var ceo = new Employee("", "");
            var manager = new Employee("", "");
            var consultant = new Employee("", "");
            var company = ceo.CreateCompany("");
            manager.Employ(ceo, "Manager", 150);

            consultant.Employ(ceo, "Consultant", 300);
            Assert.AreEqual(consultant.Company, company);
            Assert.AreEqual(consultant.Position, "Consultant");
            Assert.AreEqual(consultant.Salary, 300);
            Assert.AreEqual(consultant.Supervisor, ceo);
            Assert.IsTrue(consultant.IsEmployed);
            Assert.AreEqual(consultant.Subordinates.Count, 0);
            Assert.AreEqual(ceo.Subordinates.Count, 2);
            Assert.AreEqual(manager.Subordinates.Count, 0);
            Assert.AreEqual(consultant.Subordinates.Count, 0);
            Assert.AreEqual(ceo.Subordinates[0], manager);
            Assert.AreEqual(ceo.Subordinates[1], consultant);
        }

        [TestMethod]
        public void test_ceo_leaves_empty_company() {
            var e = new Employee("", "");
            var company = e.CreateCompany("");
            e.LeaveCompany();
            Assert.IsNull(e.Company);
            Assert.IsNull(e.Position);
            Assert.IsFalse(e.IsEmployed);
            Assert.IsNull(company.Director);
        }

        [TestMethod]
        public void test_ceo_leaves_non_empty_company() {
            var ceo = new Employee("", "");
            var mgr1 = new Employee("", "");
            var mgr2 = new Employee("", "");
            var empl1 = new Employee("", "");
            var empl2 = new Employee("", "");
            var company = ceo.CreateCompany("");

            mgr1.Employ(ceo, "Manager", 150);
            mgr2.Employ(ceo, "Manager", 150);
            empl1.Employ(mgr1, "Worker", 150);
            empl2.Employ(mgr1, "Worker", 150);

            ceo.LeaveCompany();
            Assert.IsFalse(ceo.IsEmployed);
            Assert.IsFalse(mgr1.IsEmployed);
            Assert.IsFalse(mgr2.IsEmployed);
            Assert.IsFalse(empl1.IsEmployed);
            Assert.IsFalse(empl2.IsEmployed);
            Assert.IsNull(ceo.Company);
            Assert.IsNull(mgr1.Company);
            Assert.IsNull(mgr2.Company);
            Assert.IsNull(empl1.Company);
            Assert.IsNull(empl2.Company);
        }

        [TestMethod]
        public void test_manager_leaves_company() {
            var ceo = new Employee("", "");
            var mgr1 = new Employee("", "");
            var mgr2 = new Employee("", "");
            var empl1 = new Employee("", "");
            var empl2 = new Employee("", "");
            var company = ceo.CreateCompany("");

            mgr1.Employ(ceo, "Manager1", 150);
            mgr2.Employ(ceo, "Manager2", 150);
            empl1.Employ(mgr1, "Worker1", 150);
            empl2.Employ(mgr1, "Worker2", 150);

            mgr1.LeaveCompany();
            Assert.IsFalse(mgr1.IsEmployed);
            Assert.IsTrue(empl1.IsEmployed);
            Assert.IsTrue(empl2.IsEmployed);
            Assert.AreEqual(empl1.Supervisor, ceo);
            Assert.AreEqual(empl2.Supervisor, ceo);
            Assert.AreEqual(ceo.Subordinates.Count, 3);
            Assert.AreEqual(ceo.Subordinates[0], mgr2);
            Assert.AreEqual(ceo.Subordinates[1], empl1);
            Assert.AreEqual(ceo.Subordinates[2], empl2);
        }
    }

}
