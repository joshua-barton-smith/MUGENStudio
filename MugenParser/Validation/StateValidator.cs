using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MUGENStudio.MugenParser.Validation
{
    /// <summary>
    /// validator for statedef, state, controller data
    /// also used in autocompleting in editor tabs
    /// </summary>
    public class StateValidator
    {
        // XDocuments for the validator's input
        private readonly XDocument statedefDoc;
        private readonly XDocument stateDoc;
        private readonly XDocument controllerDoc;

        // represent section validators
        private ValidSection statedef;
        private ValidSection state;
        private Dictionary<string, ValidController> controllers;

        /// <summary>
        /// initialize a validator
        /// </summary>
        public StateValidator(string dir = "TextResources/SyntaxResources")
        {
            // load from XML
            this.statedefDoc = XDocument.Load(dir + "/statedef.xml");
            this.stateDoc = XDocument.Load(dir + "/state.xml");
            this.controllerDoc = XDocument.Load(dir + "/controller.xml");

            // load data about statedefs
            this.ReadStatedefDoc();
            // load data about states
            this.ReadStateDoc();
            // load data about controllers
            this.ReadControllerDoc();
        }

        // sets up the statedef ValidSection based on the XML input
        private void ReadStatedefDoc()
        {
            XElement root = this.statedefDoc.Element("section");
            // verify section name
            if (root.Attribute("name").Value.Equals("statedef"))
            {
                // get type and comment fields
                List<ValidProperty.PropType> type = this.PropTypeListFromString(root.Attribute("type").Value);
                bool comment = Boolean.Parse(root.Attribute("comment").Value);
                // construct state
                this.statedef = new ValidSection("statedef", type.First(), comment);
                // iterate and add properties
                this.AddPropertiesToSection(this.statedef, root);
            }
        }

        // sets up the state ValidSection based on the XML input
        private void ReadStateDoc()
        {
            XElement root = this.stateDoc.Element("section");
            // verify section name
            if (root.Attribute("name").Value.Equals("state"))
            {
                // get type and comment fields
                List<ValidProperty.PropType> type = this.PropTypeListFromString(root.Attribute("type").Value);
                bool comment = Boolean.Parse(root.Attribute("comment").Value);
                // construct statedef
                this.state = new ValidSection("state", type.First(), comment);
                // iterate and add properties
                this.AddPropertiesToSection(this.state, root);
            }
        }

        // sets up the list of ValidController based on the XML input
        private void ReadControllerDoc()
        {
            this.controllers = new Dictionary<string, ValidController>();
            XElement root = this.controllerDoc.Element("controllers");
            // iterate through the controllers
            foreach (XElement ele in root.Elements().Where(e => e.Name.LocalName.Equals("controller")))
            {
                string name = ele.Attribute("name").Value;
                ValidController controller = new ValidController(name);
                this.AddPropertiesToController(controller, ele);
                this.controllers.Add(name, controller);
            }
        }

        // adds properties to a given controller from the given XElement
        private void AddPropertiesToController(ValidController controller, XElement root)
        {
            foreach (XElement ele in root.Elements().Where(e => e.Name.LocalName.Equals("property")))
            {
                // property name
                string propName = ele.Attribute("name").Value;
                // list of valid types
                List<ValidProperty.PropType> propTypes = this.PropTypeListFromString(ele.Attribute("type").Value);
                // is optional
                bool propOptional = Boolean.Parse(ele.Attribute("optional").Value);
                // if propTypes contains ENUM or MULTIENUM, read enum values
                List<string> enumOpts = new List<string>();
                if (propTypes.Contains(ValidProperty.PropType.Enum) || propTypes.Contains(ValidProperty.PropType.MultiEnum))
                {
                    foreach (XElement opt in ele.Elements().Where(e => e.Name.LocalName.Equals("value")))
                    {
                        enumOpts.Add(opt.Attribute("val").Value);
                    }
                }
                // build prop and add
                controller.AddProperty(new ValidProperty(propName, propTypes, propOptional, enumOpts));
            }
        }

        // adds properties to a given section from the given XElement
        private void AddPropertiesToSection(ValidSection section, XElement root)
        {
            foreach (XElement ele in root.Elements().Where(e => e.Name.LocalName.Equals("property")))
            {
                // property name
                string propName = ele.Attribute("name").Value;
                // list of valid types
                List<ValidProperty.PropType> propTypes = this.PropTypeListFromString(ele.Attribute("type").Value);
                // is optional
                bool propOptional = Boolean.Parse(ele.Attribute("optional").Value);
                // if propTypes contains ENUM or MULTIENUM, read enum values
                List<string> enumOpts = new List<string>();
                if (propTypes.Contains(ValidProperty.PropType.Enum) || propTypes.Contains(ValidProperty.PropType.MultiEnum))
                {
                    foreach (XElement opt in ele.Elements().Where(e => e.Name.LocalName.Equals("value")))
                    {
                        enumOpts.Add(opt.Attribute("val").Value);
                    }
                }
                // build prop and add
                section.AddProperty(new ValidProperty(propName, propTypes, propOptional, enumOpts));
            }
        }

        // reads a string and produces a list of PropType for validators
        private List<ValidProperty.PropType> PropTypeListFromString(string value)
        {
            List<ValidProperty.PropType> types = new List<ValidProperty.PropType>();
            if (value.IndexOf(' ') == -1)
            {
                // case for no space (single type)
                types.Add(this.SingleTypeFromString(value));
            } else
            {
                // case for multi-type (e.g. tuple, triple, etc)
                // split all the types
                string[] allTypes = value.Split(' ');
                // get first type
                ValidProperty.PropType first = this.SingleTypeFromString(allTypes[0]);
                // check second type to determine additional types
                switch(allTypes[1].ToLower())
                {
                    // tuple: duplicate
                    case "tuple":
                        types.Add(first);
                        types.Add(first);
                        break;
                    // triple is obvious
                    case "triple":
                        types.Add(first);
                        types.Add(first);
                        types.Add(first);
                        break;
                    // other cases are lists of raw types
                    default:
                        foreach (string x in allTypes)
                        {
                            types.Add(this.SingleTypeFromString(x));
                        }
                        break;
                }
            }
            return types;
        }

        // produces a single PropType from an input string
        private ValidProperty.PropType SingleTypeFromString(string value)
        {
            switch (value.ToLower())
            {
                case "int":
                    return ValidProperty.PropType.Int;
                case "float":
                    return ValidProperty.PropType.Float;
                case "string":
                    return ValidProperty.PropType.String;
                case "bool":
                    return ValidProperty.PropType.Bool;
                case "hitattr":
                    return ValidProperty.PropType.HitAttr;
                case "enum":
                    return ValidProperty.PropType.Enum;
                case "multienum":
                    return ValidProperty.PropType.MultiEnum;
                default:
                    return ValidProperty.PropType.None;
            }
        }

        public List<ValidProperty> GetStatedefProperties()
        {
            return this.statedef.GetProperties();
        }

        public List<ValidProperty> GetStateProperties()
        {
            return this.state.GetProperties();
        }

        public List<ValidProperty> GetControllerProperties(string sctrl)
        {
            if (!controllers.Any(x => x.Key.ToLower().Equals(sctrl))) return new List<ValidProperty>();
            return this.controllers.First(x => x.Key.ToLower().Equals(sctrl)).Value.GetProperties();
        }
    }
}
