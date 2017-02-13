using System;
using System.Collections.Generic;
using System.Linq;


namespace oop_lab3_cs.shell.objects {

    public class TypeError : Exception { }

    public abstract class ShObject {

        public static ShObject New<T>(T value) {
            return new ShObjectImpl<T>(value);
        }

        public static ShObject Empty() {
            return new EmptyShObject();
        }

        public bool HasType<T>() {
            if (typeof(T) == typeof(void)) return this is EmptyShObject;
            return this is ShObjectImpl<T>;
        }

        public bool IsEmpty { get { return this is EmptyShObject;  } }

        public abstract bool HasSameType(ShObject other);

        public T Get<T>() {
            if (!HasType<T>()) throw new TypeError();
            return ((ShObjectImpl<T>)this).GetData();
        }

        public abstract override string ToString();

    }


    class ShObjectImpl<T> : ShObject {

        T data;

        public ShObjectImpl(T data) { this.data = data; }

        public override string ToString() {
            return data.ToString();
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

    }

}
