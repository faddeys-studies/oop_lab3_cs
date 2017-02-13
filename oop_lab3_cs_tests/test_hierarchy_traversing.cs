using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.app.model;
using oop_lab3_cs.app.hierarchy;

namespace oop_lab3_cs_tests {

    [TestClass]
    public class TestHierarchyTraversing {

        [TestMethod]
        public void test_traverse_by_level() {
            var ceo = new Employee("ceo", "");
            var mgr1 = new Employee("mgr1", "");
            var mgr2 = new Employee("mgr2", "");
            var empl1 = new Employee("empl1", "");
            var empl2 = new Employee("empl2", "");
            ceo.CreateCompany("");
            mgr1.Employ(ceo, "", 150);
            mgr2.Employ(ceo, "", 150);
            empl1.Employ(mgr1, "", 150);
            empl2.Employ(mgr1, "", 150);

            ByLevel by_level = new ByLevel(ceo);
            List < string> result_names = new List<string>{ };
            List<int> result_depths = new List<int>{ };
            while (by_level.HasNext()) {
                result_names.Add(by_level.Next().FirstName);
                result_depths.Add(by_level.GetDepth());
            }

            List < string> expected_names = new List<string>{
                "ceo", "mgr1", "mgr2", "empl1", "empl2"
              };
            List<int> expected_depths = new List<int>{
                0, 1, 1, 2, 2
            };

            Assert.AreEqual(result_names.Count, expected_names.Count);
            Assert.AreEqual(result_depths.Count, expected_depths.Count);
            for (int i = 0; i < result_names.Count; ++i) {
                Assert.AreEqual(result_names[i], expected_names[i]);
                Assert.AreEqual(result_depths[i], expected_depths[i]);
            }
        }

        [TestMethod]
        public void test_traverse_by_subordination() {
            var ceo = new Employee("ceo", "");
            var mgr1 = new Employee("mgr1", "");
            var mgr2 = new Employee("mgr2", "");
            var empl1 = new Employee("empl1", "");
            var empl2 = new Employee("empl2", "");
            ceo.CreateCompany("");
            mgr1.Employ(ceo, "", 150);
            mgr2.Employ(ceo, "", 150);
            empl1.Employ(mgr1, "", 150);
            empl2.Employ(mgr1, "", 150);

            BySubordination by_subordination = new BySubordination(ceo);
            List < string> result_names = new List<string> { };
            List<int> result_depths = new List<int> { };
            while (by_subordination.HasNext()) {
                result_names.Add(by_subordination.Next().FirstName);
                result_depths.Add(by_subordination.GetDepth());
            }

            List < string> expected_names = new List<string>{
                "ceo", "mgr1", "empl1", "empl2", "mgr2"
              };
            List<int> expected_depths = new List<int>{
                0, 1, 2, 2, 1
            };

            Assert.AreEqual(result_names.Count, expected_names.Count);
            Assert.AreEqual(result_depths.Count, expected_depths.Count);
            for (int i = 0; i < result_names.Count; ++i) {
                Assert.AreEqual(result_names[i], expected_names[i]);
                Assert.AreEqual(result_depths[i], expected_depths[i]);
            }
        }

    }

}