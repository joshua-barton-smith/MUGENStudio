using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.MugenParser.Validation
{
    
    public class ValidationError
    {
        internal string message;
        internal ValidationSeverity sev;
        public ValidationError(string message, ValidationSeverity sev)
        {
            this.message = message;
            this.sev = sev;
        }
    }

    /// <summary>
    /// severity levels for validation errors.
    /// </summary>
    public enum ValidationSeverity
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }
}
