using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.shell.objects;
using oop_lab3_cs.shell.functions;


namespace oop_lab3_cs_tests {

    using Args = Dictionary<string, ShObject>;

    class Dummy {

        public int x;
        public string str;

        public Dummy(int x, string str) {
            this.x = x;
            this.str = str;
        }
    }

    class Dummy2 {

        public int x;

        public Dummy2(int x) {
            this.x = x;
        }
    }

    class DummyApiHolder {
        public static void MyMutate(Dummy arg1) {
            arg1.x += 1;
        }
        public static Dummy MyNoArgs() {
            return new Dummy(10, "some string");
        }
        public static Dummy MyCombine(Dummy arg1, Dummy arg2) {
            return new Dummy(arg1.x + arg2.x, arg1.str + arg2.str);
        }
        public static Dummy MyTwoTypes(Dummy arg1, Dummy2 arg2) {
            return new Dummy(arg1.x * arg2.x, arg1.str);
        }
    }

    [TestClass]
    public class ShellExtensionApiTests {
        [TestMethod]
        public void TestFunctionWithoutReturn() {
            ShFunction mutate = new ShFunction(typeof(DummyApiHolder), "MyMutate");
            ShObject dummy_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));

            ShObject result = mutate.Call(new Args {
                { "arg1", dummy_obj}
            });

            Assert.IsTrue(result.IsEmpty);
            Assert.AreEqual(dummy_obj.Get<Dummy>().x, 101);
        }

        [TestMethod]
        public void TestFunctionWithoutArguments() {
            ShFunction no_args = new ShFunction(typeof(DummyApiHolder), "MyNoArgs");

            ShObject result = no_args.Call(new Args {});

            Assert.IsTrue(result.HasType<Dummy>());
            Assert.AreEqual(result.Get<Dummy>().x, 10);
            Assert.AreEqual(result.Get<Dummy>().str, "some string");
        }

        [TestMethod]
        public void TestFunctionWithArgsAndReturn() {
            ShFunction combine = new ShFunction(typeof(DummyApiHolder), "MyCombine");
            ShObject dummy1_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));
            ShObject dummy2_obj = ShObject.New<Dummy>(new Dummy(250, "asd"));

            ShObject result = combine.Call(new Args {
                { "arg1", dummy1_obj},
                { "arg2", dummy2_obj}
            });

            Assert.IsTrue(result.HasType<Dummy>());
            Assert.AreEqual(result.Get<Dummy>().x, 350);
            Assert.AreEqual(result.Get<Dummy>().str, "xyzasd");
        }

        [TestMethod]
        public void TestFunctionWithDifferentParameterTypes() {
            ShFunction two_types = new ShFunction(typeof(DummyApiHolder), "MyTwoTypes");
            ShObject dummy1_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));
            ShObject dummy2_obj = ShObject.New<Dummy2>(new Dummy2(25));

            ShObject result = two_types.Call(new Args {
                { "arg1", dummy1_obj},
                { "arg2", dummy2_obj}
            });

            Assert.IsTrue(result.HasType<Dummy>());
            Assert.AreEqual(result.Get<Dummy>().x, 2500);
            Assert.AreEqual(result.Get<Dummy>().str, "xyz");
        }

        [TestMethod]
        [ExpectedException(typeof(ShellError))]
        public void TestMissingParameterRaisesTypeError() {
            ShFunction combine = new ShFunction(typeof(DummyApiHolder), "MyCombine");
            ShObject dummy1_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));
            ShObject dummy2_obj = ShObject.New<Dummy>(new Dummy(250, "asd"));

            combine.Call(new Args {
                { "arg1", dummy1_obj}
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ShellError))]
        public void TestWrongParameterNameRaisesTypeError() {
            ShFunction combine = new ShFunction(typeof(DummyApiHolder), "MyCombine");
            ShObject dummy1_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));
            ShObject dummy2_obj = ShObject.New<Dummy>(new Dummy(250, "asd"));

            combine.Call(new Args {
                { "arg1",  dummy1_obj},
                { "wrong", dummy1_obj}
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ShellError))]
        public void TestExtraParameterRaisesTypeError() {
            ShFunction combine = new ShFunction(typeof(DummyApiHolder), "MyCombine");
            ShObject dummy1_obj = ShObject.New<Dummy>(new Dummy(100, "xyz"));
            ShObject dummy2_obj = ShObject.New<Dummy>(new Dummy(250, "asd"));

            combine.Call(new Args {
                { "arg1", dummy1_obj},
                { "arg2", dummy2_obj},
                { "extra", dummy1_obj}
            });
        }

    }
}
