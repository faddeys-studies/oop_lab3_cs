using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oop_lab3_cs.shell.tokens;

namespace oop_lab3_cs_tests {
    [TestClass]
    public class TokenizationIntegrationTests {
        [TestMethod]
        public void TestSuccessfulTokenizing() {
            const string statement = "variable = another = "
                  +"function(arg1=value arg2=subcall(argument=another_value))";
            Token[] expected_tokens = new Token[] {
                new AssignmentToken("variable"),
                new AssignmentToken("another"),
                new NameToken("function"),
                new ParenthesisToken(true),
                new AssignmentToken("arg1"),
                new NameToken("value"),
                new AssignmentToken("arg2"),
                new NameToken("subcall"),
                new ParenthesisToken(true),
                new AssignmentToken("argument"),
                new NameToken("another_value"),
                new ParenthesisToken(false),
                new ParenthesisToken(false)
            };
            List<Token> actual_tokens = Token.ParseAll(statement);
            Assert.AreEqual(expected_tokens.Length, actual_tokens.Count);
            for (int i = 0; i < expected_tokens.Length; ++i) {
                Assert.AreEqual(expected_tokens[i], actual_tokens[i]);
            }
        }

        public void test_failing_tokenizing() {
            const string statement = "variable = another = 1_some wrong = = = tokens";
            try {
                Token.ParseAll(statement);
            } catch (SyntaxError) {
                return;
            }
            Assert.Fail("should fail with SyntaxError");
        }
    }

    [TestClass]
    public class AssignmentTokenTests {
        [TestMethod]
        public void TestComparison() {
            Assert.AreEqual(new AssignmentToken("qwe"), new AssignmentToken("qwe"));
            Assert.AreNotEqual(new AssignmentToken("123"), new AssignmentToken("qwe"));
            Assert.AreNotEqual(new AssignmentToken("qwer"), new AssignmentToken("qwe"));
        }

        [TestMethod]
        public void TestCorrectParsing() {
            var cases = new []{
                new { code=" variable = expression",     result="variable", offset=11},
                new { code="var_1 = expression",         result="var_1",    offset=7},
                new { code=" variable = ",               result="variable", offset=11},
                new { code="variable = two expressions", result="variable", offset=10},
                new { code="v1 =",                       result="v1",       offset=4},
                new { code="dense=nexttoken",            result="dense",    offset=6},
                new { code="sparse   =    nexttoken",    result="sparse",   offset=10},
            };
            foreach (var item in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(AssignmentToken), item.code, out offset);

                Assert.IsNotNull(token, item.code);
                Assert.IsInstanceOfType(token, typeof(AssignmentToken), item.code);
                Assert.AreEqual((AssignmentToken)token, new AssignmentToken(item.result), item.code);
                Assert.AreEqual(offset, item.offset, item.code);
            }
        }

        [TestMethod]
        public void TestFailingParsing() {
            var cases = new[] {
                "variable", "", "1variable", "= expr"
            };
            foreach (string str in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(AssignmentToken), str, out offset);
                Assert.IsNull(token, str);
                Assert.AreEqual(offset, -1, str);
            }
        }
    }

    [TestClass]
    public class NameTokenTests {

        [TestMethod]
        public void TestComparison() {
            Assert.AreEqual(new NameToken("qwe"), new NameToken("qwe"));
            Assert.AreNotEqual(new NameToken("123"), new NameToken("qwe"));
            Assert.AreNotEqual(new NameToken("qwer"), new NameToken("qwe"));
        }

        [TestMethod]
        public void TestCorrectParsing() {
            var cases = new[]{
                new { code=" variable = expression", result="variable",   offset=9},
                new { code="var = expression",       result="var",        offset=3},
                new { code="variable_1",             result="variable_1", offset=10},
                new { code="variable(expr)",         result="variable",   offset=8},
                new { code="variable)",              result="variable",   offset=8},
                new { code="numbers123 next",        result="numbers123", offset=10},
            };
            foreach (var item in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(NameToken), item.code, out offset);

                Assert.IsNotNull(token, item.code);
                Assert.IsInstanceOfType(token, typeof(NameToken), item.code);
                Assert.AreEqual((NameToken)token, new NameToken(item.result), item.code);
                Assert.AreEqual(offset, item.offset, item.code);

            }
        }

        [TestMethod]
        public void TestFailingParsing() {
            var cases = new[] {
                "", "1variable", "(variable)", "=var", "?var",
                "&var", "#var", "!var", "-var"
            };
            foreach (string str in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(NameToken), str, out offset);
                Assert.IsNull(token, str);
                Assert.AreEqual(offset, -1, str);
            }
        }

    };

    [TestClass]
    public class NumberLiteralTokenTests {

        [TestMethod]
        public void TestComparison() {
            Assert.AreEqual(new NumberLiteralToken(5), new NumberLiteralToken(5));
            Assert.AreNotEqual(new NumberLiteralToken(10), new NumberLiteralToken(20));
        }

        [TestMethod]
        public void TestCorrectParsing() {
            var cases = new[]{
                new {code="10",      result=10,   offset=2},
                new {code=" 12 ",    result=12,   offset=3},
                new {code="-105 ",   result=-105, offset=4},
                new {code=" - 1 ",   result=-1,   offset=4},
                new {code="10 000",  result=10,   offset=2},
                new {code=" 00000 ", result=0,    offset=6},
                new {code=" 5) ",    result=5,    offset=2},
                new {code="1f",      result=1,    offset=1},
                new {code="2.5",     result=2,    offset=1}
            };
            foreach (var item in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(NumberLiteralToken), item.code, out offset);

                Assert.IsNotNull(token, item.code);
                Assert.IsInstanceOfType(token, typeof(NumberLiteralToken), item.code);
                Assert.AreEqual((NumberLiteralToken)token, new NumberLiteralToken(item.result), item.code);
                Assert.AreEqual(offset, item.offset, item.code);

            }
        }

        [TestMethod]
        public void TestFailingParsing() {
            var cases = new[] {
                "--1", ""
            };
            foreach (string str in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(NumberLiteralToken), str, out offset);
                Assert.IsNull(token, str);
                Assert.AreEqual(offset, -1, str);
            }
        }

    };

    [TestClass]
    public class StringLiteralTokenTests {

        [TestMethod]
        public void TestComparison() {
            Assert.AreEqual(new StringLiteralToken("qwe"), new StringLiteralToken("qwe"));
            Assert.AreNotEqual(new StringLiteralToken("123"), new StringLiteralToken("qwe"));
            Assert.AreNotEqual(new StringLiteralToken("qwer"), new StringLiteralToken("qwe"));
        }

        [TestMethod]
        public void TestCorrectParsing() {
            var cases = new[]{
                new {code="\"\" next tokens",          result="",                 offset=2},
                new {code="\"some data\" next tokens", result="some data",        offset=11},
                new {code="\"\\\"quoted\\\"\"",        result="\"quoted\"",       offset=12},
                new {code="\"with\\nnew\\nlines\"",    result="with\nnew\nlines", offset=18},
                new {code="\"xxx\")",                  result="xxx",              offset=5},
            };
            foreach (var item in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(StringLiteralToken), item.code, out offset);

                Assert.IsNotNull(token, item.code);
                Assert.IsInstanceOfType(token, typeof(StringLiteralToken), item.code);
                Assert.AreEqual((StringLiteralToken)token, new StringLiteralToken(item.result), item.code);
                Assert.AreEqual(offset, item.offset, item.code);

            }
        }

        [TestMethod]
        public void TestFailingParsing() {
            var cases = new[] {
                "\"some data\"justafter", "", "\"not closed", " not opened\""
            };
            foreach (string str in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(StringLiteralToken), str, out offset);
                Assert.IsNull(token, str);
                Assert.AreEqual(offset, -1, str);
            }
        }

    };

    [TestClass]
    public class ParenthesisTokenTests {

        [TestMethod]
        public void TestComparison() {
            Assert.AreEqual(new ParenthesisToken(true), new ParenthesisToken(true));
            Assert.AreNotEqual(new ParenthesisToken(true), new ParenthesisToken(false));
        }

        [TestMethod]
        public void TestCorrectParsing() {
            var cases = new[]{
                new {code=" (",          result=true,  offset=2},
                new {code="(expression", result=true,  offset=1},
                new {code=")",           result=false, offset=1},
                new {code="()",          result=true,  offset=1},
                new {code=")(",          result=false, offset=1},
            };
            foreach (var item in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(ParenthesisToken), item.code, out offset);

                Assert.IsNotNull(token, item.code);
                Assert.IsInstanceOfType(token, typeof(ParenthesisToken), item.code);
                Assert.AreEqual((ParenthesisToken)token, new ParenthesisToken(item.result), item.code);
                Assert.AreEqual(offset, item.offset, item.code);

            }
        }

        [TestMethod]
        public void TestFailingParsing() {
            var cases = new[] {
                "expression(", "", "=("
            };
            foreach (string str in cases) {
                int offset = -1;
                Token token = Token.TryParse(typeof(ParenthesisToken), str, out offset);
                Assert.IsNull(token, str);
                Assert.AreEqual(offset, -1, str);
            }
        }

    };
}
