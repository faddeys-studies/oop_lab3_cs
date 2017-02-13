using System;
using System.Collections.Generic;
using System.Linq;


namespace oop_lab3_cs.shell.objects {

    public class ShellError : Exception {
        public ShellError() : this("") { }
        public ShellError(string msg) : base(msg) { }
    }

    public abstract class ShObject {

        public static ShObject New<T>(T value) {
            return new ShObjectImpl<T>(value);
        }

        public static ShObject New(Type type, object value) {
            var factory_method = typeof(ShObject)
                .GetMethods()
                .Where(info => info.Name == "New" && info.IsGenericMethodDefinition)
                .First();
            return (ShObject)factory_method
                .MakeGenericMethod(new Type[] { type })
                .Invoke(null, new object[] { value });
        }

        public static ShObject Empty() {
            return new EmptyShObject();
        }

        public bool HasType<T>() {
            if (typeof(T) == typeof(void)) return this is EmptyShObject;
            return this is ShObjectImpl<T>;
        }

        public bool HasType(Type t) {
            return (bool)typeof(ShObject)
                .GetMethod("HasType", new Type[] { })
                .MakeGenericMethod(new Type[] { t })
                .Invoke(this, new object[] { });
        }

        public bool IsEmpty { get { return this is EmptyShObject;  } }

        public abstract bool HasSameType(ShObject other);

        public abstract Type GetDataType();

        public T Get<T>() {
            if (!HasType<T>())
                throw new ShellError(
                    "Expected type " + GetDataType().Name
                    + ", but requested " + typeof(T).Name
                );
            return ((ShObjectImpl<T>)this).GetData();
        }

        public object Get(Type t) {
            return typeof(ShObject)
                .GetMethod("Get", new Type[] { })
                .MakeGenericMethod(new Type[] { t })
                .Invoke(this, new object[] { });
        }

        public abstract override string ToString();

    }


    class ShObjectImpl<T> : ShObject {

        T data;

        public ShObjectImpl(T data) { this.data = data; }

        public override string ToString() {
            return data.ToString();
        }

        public override Type GetDataType() {
            return typeof(T);
        }

        public T GetData() { return data; }

        public override bool HasSameType(ShObject other) {
            return other is ShObjectImpl<T>;
        }

    }

    class EmptyShObject: ShObject {

        public override bool HasSameType(ShObject other) {
            return other is EmptyShObject;
        }

        public override string ToString() {
            return "";
        }

        public override Type GetDataType() {
            throw new ShellError("Empty object has not data type");
        }

    }

}
