using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.shell.tokens;
using oop_lab3_cs.shell.grammar;

namespace oop_lab3_cs_tests {

    [TestClass]
    public class StatementParsingTests {

        [TestMethod]
        public void TestSimpleVariable() {
            List<Token> input_tokens = Token.ParseAll(
                "some_variable"
            );
            var expected_statement = new Statement(
                new Variable("some_variable"),
                new List<string>{ }
            );
            var result = Statement.Parse(input_tokens);
            Assert.AreEqual(expected_statement, result);
        }

        [TestMethod]
        public void TestSimpleFunctionCall() {
            List<Token> input_tokens = Token.ParseAll(
                "function(arg=val)"
            );
            var expected_statement = new Statement(
                new FunctionCall(
                    "function", new Dictionary<string, Expression>{
                        {"arg", new Variable("val")}
                    }
                ),
                new List<string> { }
            );
            Assert.AreEqual(expected_statement, Statement.Parse(input_tokens));
        }

        [TestMethod]
        public void TestSimpleAssignment() {
            List<Token> input_tokens = Token.ParseAll(
                "variable = value"
            );
            var expected_statement = new Statement(
                new Variable("value"),
                new List<string> { "variable"}
            );
            Assert.AreEqual(expected_statement, Statement.Parse(input_tokens));
        }

        [TestMethod]
        public void TestMultipleAssignments() {
            List<Token> input_tokens = Token.ParseAll(
                "variable1 = variable2 = variable3 = value"
            );
            var expected_statement = new Statement(
                new Variable("value"),
                new List<string> { "variable1", "variable2", "variable3"}
            );
            Assert.AreEqual(expected_statement, Statement.Parse(input_tokens));
        }

        public void TestComplexFunctionCallWithAssignment() {
            List<Token> input_tokens = Token.ParseAll(
                "result = function1(arg=value param=function2(x=\"literal\" y=1))"
            );
            var expected_statement = new Statement(
                new FunctionCall(
                    "function1", new Dictionary<string, Expression>{
                        {"arg", new Variable("value")},
                        {"param", new FunctionCall(
                            "function2", new Dictionary<string, Expression>{
                                {"x", new StringLiteral("literal")},
                                {"y", new NumberLiteral(1)}
                            }
                        )},
                    }
                ),
                new List<string>{"result"}
            );
            Assert.AreEqual(expected_statement, Statement.Parse(input_tokens));
        }

    }

    public class TestFunctionCall {

        public void test_add_argument() {
            var func_call = new FunctionCall("myfunc");
            Expression subexpr = new Variable("subexpr");
            var args = func_call.GetArgs();

            // add argument "arg1"
            Assert.IsTrue(func_call.AddArgument("arg1", subexpr));
            Assert.AreEqual(args.Count, 1);
            Assert.IsTrue(args.ContainsKey("arg1"));

            // add another argument - "arg2"
            Assert.IsTrue(func_call.AddArgument("arg2", subexpr));
            Assert.AreEqual(args.Count, 2);
            Assert.IsTrue(args.ContainsKey("arg1"));
            Assert.IsTrue(args.ContainsKey("arg2"));

            // add "arg1" again -> fail
            Assert.IsFalse(func_call.AddArgument("arg1", subexpr));
        }

    };


}