using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.app.model;
using oop_lab3_cs.app.db;


namespace oop_lab3_cs_tests {

    [TestClass]
    public class TestDB {

        [TestMethod]
        public void test_correct_io() {
            var ceo = new Employee("director", "");
            var mgr1 = new Employee("manager 1", "");
            var mgr2 = new Employee("manager 2", "");
            var wrk11 = new Employee("worker 1-1", "");
            var wrk12 = new Employee("worker 1-2", "");
            var wrk21 = new Employee("worker 2-1", "");
            var wrk22 = new Employee("worker 2-2", "");

            Company company = ceo.CreateCompany("mycompany");

            mgr1.Employ(ceo, mgr1.FirstName, 1);
            mgr2.Employ(ceo, mgr2.FirstName, 2);

            wrk11.Employ(mgr1, wrk11.FirstName, 3);
            wrk12.Employ(mgr1, wrk12.FirstName, 4);
            wrk21.Employ(mgr2, wrk21.FirstName, 5);
            wrk22.Employ(mgr2, wrk22.FirstName, 6);

            var writer = new StringWriter();
            DB.save(company, writer);
            writer.Close();

            var data = writer.ToString();
            
            var reader = new StringReader(data);
            Company deserialized_company = DB.load(reader);

            Assert.AreEqual(company.Name, deserialized_company.Name);
            compare_recursively(ceo, deserialized_company.Director);
        }

        private void compare_recursively(Employee empl1, Employee empl2) {
            Assert.AreEqual(empl1.FirstName, empl2.FirstName);
            Assert.AreEqual(empl1.LastName, empl2.LastName);
            Assert.AreEqual(empl1.Salary, empl2.Salary);
            Assert.AreEqual(empl1.Position, empl2.Position);
            Assert.AreEqual(empl1.Subordinates.Count, empl2.Subordinates.Count);
            for (int i = 0; i < empl1.Subordinates.Count; ++i) {
                compare_recursively(
                    empl1.Subordinates[i],
                    empl2.Subordinates[i]
                );
            }
        }

    };


}
