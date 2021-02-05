using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Validation
{
    class ValidController
    {
        /// <summary>
        /// name for the controller
        /// </summary>
        public string Name { get; }
        private List<ValidProperty> properties;

        public ValidController(string name)
        {
            this.Name = name;
            this.properties = new List<ValidProperty>();
        }

        /// <summary>
        /// adds a property for this controller
        /// </summary>
        /// <param name="prop">property to add</param>
        public void AddProperty(ValidProperty prop)
        {
            this.properties.Add(prop);
        }

        internal List<ValidProperty> GetProperties()
        {
            return this.properties;
        }
    }
}
