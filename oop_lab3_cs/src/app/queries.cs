using System;
using System.Collections.Generic;
using System.Linq;
using oop_lab3_cs.app.model;
using oop_lab3_cs.app.hierarchy;


namespace oop_lab3_cs.app.queries {
    
    public class Query<OrderT>: EmployeeVisitor where OrderT: HierarchyIterator {

        private readonly Func<Employee, int, bool> predicate;
        private List<Employee> result;
        OrderT iterator;

        public Query(Func<Employee, int, bool> p) {
            predicate = p;
        }

        List<Employee> run(Company company) {
            result = new List<Employee>();
            iterator = (OrderT)Activator.CreateInstance(
                typeof(OrderT), new object[] { company.Director }
            );
            VisitAll(iterator);
            return result;
        }

        public override void Visit(Employee empl) {
            if (predicate(empl, iterator.GetDepth())) {
                result.Add(empl);
            }
        }

    };

}}


}
