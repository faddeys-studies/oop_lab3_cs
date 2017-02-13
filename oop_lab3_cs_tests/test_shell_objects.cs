using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.shell.objects;


namespace oop_lab3_cs_tests {

    [TestClass]
    public class DynamicTypesTests {

        [TestMethod]
        public void TestApiWorks() {
            var obj = ShObject.New<int>(10);

            Assert.IsTrue(obj.HasSameType(obj));
            Assert.IsTrue(obj.HasType<int>());
            Assert.IsTrue(obj.HasType(typeof(int)));
            Assert.IsFalse(obj.HasType<string>());
            Assert.IsFalse(obj.HasType(typeof(string)));
            Assert.IsFalse(obj.IsEmpty);
            Assert.AreEqual(obj.Get<int>(), 10);
            Assert.AreEqual(obj.Get(typeof(int)), 10);

            try { obj.Get<float>();
            } catch (ShellError) { return; }
            Assert.Fail("Get<wrong-type>() must throw ShellError");
        }

        [TestMethod]
        public void TestSupportsEmptyValue() {
            var empty_obj = ShObject.Empty();
            Assert.IsTrue(empty_obj.IsEmpty);

            try { empty_obj.Get<float>();
            } catch (ShellError) { return; }
            Assert.Fail("Get<any-type>() on Empties must throw ShellError");
        }

        [TestMethod]
        public void TestTwoObjectsWithSameType() {
            var obj1 = ShObject.New<string>("example1");
            var obj2 = ShObject.New<string>("example2");
            Assert.IsTrue(obj1.HasSameType(obj2));
            Assert.IsTrue(obj2.HasSameType(obj1));
        }

        [TestMethod]
        public void TestTwoObjectsWithDifferentTypes() {
            var obj1 = ShObject.New<string>("example1");
            var obj2 = ShObject.New<float>(1.0f);
            Assert.IsFalse(obj1.HasSameType(obj2));
            Assert.IsFalse(obj2.HasSameType(obj1));
        }

};

}
