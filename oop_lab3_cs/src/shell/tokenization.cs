
using System;
using System.Collections.Generic;
using System.Linq;


namespace oop_lab3_cs.shell.tokens {

    public class SyntaxError : Exception {
        public SyntaxError(string message) : base(message) { }
        public SyntaxError() : this("") { }
    }

    public abstract class Token {

        public static List<Token> ParseAll(string code) {
            List<Token> result = new List<Token>();
            while (code.Length > 0) {
                char first_non_space = code.FirstOrDefault(ch => " \t\n".IndexOf(ch) < 0);
                if (first_non_space != '\0') code = code.Remove(0, code.IndexOf(first_non_space));
                Token tok = null;
                foreach (Type impl_type in TokenMeta.Types) {
                    int offset = -1;
                    tok = Token.TryParse(impl_type, code, out offset);
                    if (tok == null) continue;
                    code = code.Remove(0, offset);
                    result.Add(tok);
                    break;
                }
                if (tok == null) {
                    throw new SyntaxError("unknown syntax: " + code.Substring(0, 10));
                }
            }
            return result;
        }

        public static Token TryParse(Type token_type, string code, out int offset) {
            if (!token_type.IsSubclassOf(typeof(Token))) {
                throw new Exception("Wrong token type");
            }
            Token token = (Token)token_type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
            try {
                offset = token.Parse(code);
                if (offset < 0) {
                    throw new SyntaxError();
                }
                return token;
            } catch (SyntaxError err) {
                offset = -1;
                return null;
            }
        }

        public static T TryConvert<T>(Token token) where T : class {
            return null;
        }

        public static bool operator ==(Token first, Token second) {
            if (Object.ReferenceEquals(first, null)) return Object.ReferenceEquals(second, null);
            return first.Equals(second);
        }

        public static bool operator !=(Token first, Token second) {
            return !(first == second);
        }

        public abstract int Parse(string code);

    }

    public class AssignmentToken : Token {

        private string var_name;

        public AssignmentToken(string var_name) {
            this.var_name = var_name;
        }

        public AssignmentToken() : this("") { }

        public override int Parse(string code) {
            int name_start = -1;
            bool name_done = false;
            string buf = "";
            for (int idx = 0; idx < code.Length; idx++) {
                char ch = code[idx];
                if (char.IsLetterOrDigit(ch) || ch == '_') {
                    if (name_start == -1 && char.IsDigit(ch)) {
                        return -1; // name can't start with a digit
                    }
                    if (name_done) return -1; // name parsing done but there is letter again
                    if (name_start == -1) name_start = idx;
                } else if (name_start == -1) {
                    if (!char.IsWhiteSpace(ch)) return -1;
                } else {
                    if (!name_done) {
                        name_done = true;
                        buf = code.Substring(name_start, idx - name_start);
                    }
                    if (ch == '=') {
                        var_name = buf;
                        return idx + 1;
                    } else if (!char.IsWhiteSpace(ch)) {
                        return -1;
                    }
                }
            }
            return -1;
        }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(AssignmentToken)) return false;
            return var_name == ((AssignmentToken)other).var_name;
        }
    }

    public class NameToken : Token {

        private string var_name;

        public NameToken(string var_name) {
            this.var_name = var_name;
        }

        public NameToken() : this("") { }

        public override int Parse(string code) {
            char first_non_space = code.FirstOrDefault(ch => " \t\n".IndexOf(ch) < 0);
            if (first_non_space == '\0') return -1;
            int name_start = code.IndexOf(first_non_space);
            if (code.Length <= name_start) return -1;
            if (!char.IsLetter(code[name_start])) return -1;
            int idx = name_start + 1;
            for (; idx < code.Length; ++idx) {
                char ch = code[idx];
                if (!char.IsLetterOrDigit(ch) && ch != '_') {
                    break;
                }
            }
            var_name = code.Substring(name_start, idx - name_start);
            return idx;
        }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(NameToken)) return false;
            return var_name == ((NameToken)other).var_name;
        }
    }

    public class NumberLiteralToken : Token {

        int value;

        public NumberLiteralToken(int value) {
            this.value = value;
        }

        public NumberLiteralToken() : this(0) { }

        public override int Parse(string code) {
            bool started_parsing = false;
            bool digits_found = false;
            bool is_positive = true;
            int value = 0;
            for (int idx = 0; idx < code.Length; idx++) {
                char ch = code[idx];
                if (char.IsDigit(ch)) {
                    digits_found = true;
                    started_parsing = true;
                    value = (10 * value + (ch - '0'));
                } else if (ch == '-') {
                    if (started_parsing) return -1;
                    started_parsing = true;
                    is_positive = false;
                } else {
                    if (digits_found) {
                        this.value = is_positive ? value : -value;
                        return idx;
                    }
                    if (!char.IsWhiteSpace(ch)) break;
                }
            }
            if (digits_found) {
                this.value = value;
                return code.Length;
            }
            return -1;
        }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(NumberLiteralToken)) return false;
            return value == ((NumberLiteralToken)other).value;
        }
    }

    public class StringLiteralToken : Token {

        private string value;

        public StringLiteralToken(string value) {
            this.value = value;
        }

        public StringLiteralToken() : this("") { }

        public override int Parse(string code) {
            char first_non_space = code.FirstOrDefault(ch => " \t\n".IndexOf(ch) < 0);
            if (first_non_space == '\0') return -1;
            int offset = code.IndexOf(first_non_space);
            if (code.Length <= offset + 2) return -1;  // +2 characters for quotes, at least
            if (code[offset] != '\"') return -1;
            offset++;
            bool is_escaping = false;
            string buf = "";
            for (int idx = offset; idx < code.Length; idx++) {
                char ch = code[idx];
                if (is_escaping) {
                    is_escaping = false;
                    switch (ch) {
                        case 'n': buf += "\n"; break;
                        case 't': buf += "\t"; break;
                        case '\\': buf += "\\"; break;
                        case '\"': buf += "\""; break;
                        default: return -1;
                    }
                } else {
                    if (ch == '\"') {
                        if (idx + 1 < code.Length)
                            if (!char.IsWhiteSpace(code[idx + 1]) && code[idx + 1] != ')')
                                return -1;
                        value = buf;
                        return idx + 1;
                    } else if (ch == '\\') {
                        is_escaping = true;
                        continue;
                    } else {
                        buf += ch;
                    }
                }
            }
            return -1;
        }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(StringLiteralToken)) return false;
            return value == ((StringLiteralToken)other).value;
        }
    }

    public class ParenthesisToken : Token {

        private bool is_open;

        public ParenthesisToken(bool is_open) {
            this.is_open = is_open;
        }

        public ParenthesisToken() : this(true) { }

        public override int Parse(string code) {
            bool started_parsing = false;
            for (int idx = 0; idx < code.Length; ++idx) {
                char ch = code[idx];
                if (ch == '(' || ch == ')') {
                    is_open = (ch == '(');
                    return idx + 1;
                } else if (char.IsWhiteSpace(ch)) {
                    if (started_parsing) return -1;
                    started_parsing = true;
                    continue;
                } else {
                    return -1;
                }
            }
            return -1;
        }

        public override bool Equals(object other) {
            if (Object.ReferenceEquals(other, null)) return false;
            if (other.GetType() != typeof(ParenthesisToken)) return false;
            return is_open == ((ParenthesisToken)other).is_open;
        }
    }

    public class TokenMeta {
        public static readonly List<Type> Types = new List<Type> {
            // ATTENTION: ORDER IS IMPORTANT
            typeof(ParenthesisToken),
            typeof(StringLiteralToken),
            typeof(NumberLiteralToken),
            typeof(AssignmentToken),
            typeof(NameToken),
        };
    }

}
