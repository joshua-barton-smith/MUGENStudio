using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Validation
{
    /// <summary>
    /// represents a valid INI property for a given section or controller type
    /// </summary>
    public class ValidProperty
    {
        /// <summary>
        /// name for this property
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// property types for RHS, in some cases this is multiple values
        /// </summary>
        public List<PropType> Types { get; }
        /// <summary>
        /// if the property is optional
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// list of options for ENUM or MULTIENUM types
        /// </summary>
        public List<string> EnumOpts { get; }
        /// <summary>
        /// construct a property validator
        /// </summary>
        /// <param name="name">name of the prop</param>
        /// <param name="types">list of types for RHS</param>
        /// <param name="optional">whether the prop is required or optional</param>
        /// <param name="opts">options for enum types</param>
        public ValidProperty(string name, List<PropType> types, bool optional, List<string> opts)
        {
            this.Name = name;
            this.Types = types;
            this.Optional = optional;
            this.EnumOpts = opts;
        }

        public string GetPropertyDesc()
        {
            string pt = "";
            foreach (ValidProperty.PropType ptype in this.Types)
            {
                pt += ", " + Enum.GetName(typeof(ValidProperty.PropType), ptype);
                if (ptype == ValidProperty.PropType.Enum || ptype == ValidProperty.PropType.MultiEnum)
                {
                    pt += " (";
                    foreach (string opt in this.EnumOpts)
                    {
                        pt += opt + " ";
                    }
                    pt = pt.Substring(0, pt.Length - 1);
                    pt += ")";
                }
            }
            pt = pt.Substring(2);
            if (this.Optional) pt += " (optional)";
            return pt;
        }

        /// <summary>
        /// represents the types assigned to a property
        /// </summary>
        public enum PropType
        {
            /// <summary>
            /// integer type
            /// </summary>
            Int,
            /// <summary>
            /// string type (should be quote-encased)
            /// </summary>
            String,
            /// <summary>
            /// float type
            /// </summary>
            Float,
            /// <summary>
            /// bool type (represented by 0/non-0, so INT behind the scenes)
            /// </summary>
            Bool,
            /// <summary>
            /// complex enum type with multiple segments, complex enough to have its own type
            /// </summary>
            HitAttr,
            /// <summary>
            /// single-value enum
            /// </summary>
            Enum,
            /// <summary>
            /// multi-value enum
            /// </summary>
            MultiEnum,
            /// <summary>
            /// fallback case
            /// </summary>
            None
        }
    }
}
