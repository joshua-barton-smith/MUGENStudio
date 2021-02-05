using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Validation
{
    /// <summary>
    /// represents a valid INI section.
    /// </summary>
    public class ValidSection
    {
        // list of properties valid for this section
        private List<ValidProperty> properties;
        /// <summary>
        /// constructs a new valid INI section
        /// </summary>
        /// <param name="name">name of the section, for reference and validation</param>
        /// <param name="type">type of the section, which appears after the name in the INI file ([Section {type}])</param>
        /// <param name="comment">determines whether the section supports a comma-separated comment after the type ([Section {type}, {comment}])</param>
        public ValidSection(string name, ValidProperty.PropType type, bool comment)
        {
            this.Name = name;
            this.Type = type;
            this.Comment = comment;
            this.properties = new List<ValidProperty>();
        }

        /// <summary>
        /// adds a property for this section
        /// </summary>
        /// <param name="prop">property to add</param>
        public void AddProperty(ValidProperty prop)
        {
            this.properties.Add(prop);
        }

        /// <summary>
        /// name for the section
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// type of the section
        /// </summary>
        public ValidProperty.PropType Type { get; }
        /// <summary>
        /// whether the section supports a comment
        /// </summary>
        public bool Comment { get; }

        internal List<ValidProperty> GetProperties()
        {
            return this.properties;
        }
    }
}
