using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using oop_lab3_cs.app.model;

namespace oop_lab3_cs.app.db {

    public class DB {
        private static void _hierarchy_to_json(Employee empl, XmlWriter writer) {
            writer.WriteStartElement("employee");
            writer.WriteAttributeString("first_name", empl.FirstName);
            writer.WriteAttributeString("last_name", empl.LastName);
            writer.WriteAttributeString("position", empl.Position);
            writer.WriteAttributeString("salary", empl.Salary.ToString());

            foreach(var sub in empl.Subordinates) {
                _hierarchy_to_json(sub, writer);
            }

            writer.WriteEndElement();
        }

        private static Employee _load_with_subordinates(Employee supervisor, XmlNode xml_node) {
            var attrs = xml_node.Attributes;

            var empl = new Employee(
                attrs["first_name"].Value,
                attrs["last_name"].Value
            );
            empl.Employ(
                supervisor,
                attrs["position"].Value,
                int.Parse(attrs["salary"].Value)
            );
            foreach(XmlNode sub_node in xml_node.ChildNodes) {
                _load_with_subordinates(empl, sub_node);
            }
            return empl;
        }

        public static void save(Company company, XmlWriter writer) {
            writer.WriteStartElement("company");
            writer.WriteAttributeString("company_name", company.Name);
            _hierarchy_to_json(company.Director, writer);
            writer.WriteEndElement();
        }

        public static void save(Company company, TextWriter writer) {
            XmlWriter xml_wrtier = XmlWriter.Create(writer);
            save(company, xml_wrtier);
            xml_wrtier.Close();
        }

        public static Company load(TextReader reader) {
            XmlDocument document = new XmlDocument();
            document.Load(reader);
            var company_node = document.ChildNodes[1];
            var ceo_node = company_node.ChildNodes[0];
            var ceo = new Employee(
                ceo_node.Attributes["first_name"].Value,
                ceo_node.Attributes["last_name"].Value
            );
            var company = ceo.CreateCompany(company_node.Attributes["company_name"].Value);
            ceo.Salary = int.Parse(ceo_node.Attributes["salary"].Value);
            foreach (XmlNode sub_node in ceo_node.ChildNodes) {
                _load_with_subordinates(ceo, sub_node);
            }
            return company;
        }
    }

}

