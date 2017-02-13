using System;
using System.Collections.Generic;
using System.Linq;
using oop_lab3_cs.app.model;


namespace oop_lab3_cs.app.hierarchy {

    public abstract class HierarchyIterator { 

        // check if there are any more employees
        public abstract bool HasNext();

        // returns next employee by forwarding the iterator
        public abstract Employee Next();

        // returns the current employee without forwarding
        // calls to this method should be always valid after calling first Next()
        public abstract Employee Get();

        // returns depth of current employee in the company hierarchy
        public abstract int GetDepth();
    }

    public abstract class EmployeeVisitor {
        public abstract void Visit(Employee e);

        public void VisitAll<HIType>(Employee root) where HIType : HierarchyIterator {
            HIType hi = (HIType)Activator.CreateInstance(typeof(HIType), new object[] { root });
            VisitAll(hi);
        }

        public void VisitAll<HIType>(Company comp) where HIType : HierarchyIterator {
            VisitAll<HIType>(comp.Director);
        }

        public void VisitAll(HierarchyIterator hi) {
            while (hi.HasNext()) {
                Visit(hi.Next());
            }
        }
    }

    class Item {
        public int idx;
        public Employee empl;

        public Item(int idx, Employee empl) {
            this.idx = idx;
            this.empl = empl;
        }
    }

    public class ByLevel : HierarchyIterator {
            // first item in pairs is depth
            Queue<Item> q;
            Employee current;
            int current_depth;
        public ByLevel(Employee root) {
            q = new Queue<Item> ();
            q.Enqueue(new Item(0, root));
            current_depth = -1;
        }

        public override bool HasNext() {
            return q.Count > 0;
        }
        public override Employee Next() {
            current_depth = q.First().idx;
            current = q.First().empl;
            foreach(var sub in current.Subordinates) {
                q.Enqueue(new Item(current_depth + 1, sub));
            }
            q.Dequeue();
            return current;
        }
        public override Employee Get() {
            return current;
        }
        public override int GetDepth() {
            return current_depth;
        }
    }

    public class BySubordination : HierarchyIterator {
        // first item is a number of not processed subordinates
        Stack<Item> st;
        // sum of numbers of unprocessed along the stack
        // used to determine whether there are more nodes to process
        int unprocessed_cnt;
        Employee root;

        public BySubordination(Employee root) {
            st = new Stack<Item>();
            this.root = root;
            unprocessed_cnt = -1;
        }

        public override bool HasNext() {
            return unprocessed_cnt > 0 || unprocessed_cnt == -1;
        }
        public override Employee Next() {
            if (unprocessed_cnt == -1) {
                unprocessed_cnt = root.Subordinates.Count;
                st.Push(new Item( unprocessed_cnt, root ));
                return root;
            }
            var curr_item = st.Peek();
            var subordinates = curr_item.empl.Subordinates;
            if (curr_item.idx > 0) {
                var next_empl = subordinates[subordinates.Count - curr_item.idx];
                int n_next_from_here = next_empl.Subordinates.Count;
                st.Push(new Item(n_next_from_here, next_empl));
                curr_item.idx--;
                unprocessed_cnt += (n_next_from_here - 1);
                return Get();
            } else {
                st.Pop();
                return Next();
            }
        }
        public override Employee Get() {
            return st.Peek().empl;
        }
        public override int GetDepth() {
            return st.Count - 1;
        }
    }


}