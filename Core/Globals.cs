using MUGENStudio.Graphic;
using MUGENStudio.MugenParser;
using MUGENStudio.MugenParser.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio.Core
{
    /// <summary>
    /// contains globally-accessible settings, project data, etc
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// settings internal to the MUGEN Studio application
        /// </summary>
        public static StudioSettings settingsSingleton = new StudioSettings("mugenstudio.xml");
        /// <summary>
        /// data for all currently-loaded projects
        /// </summary>
        public static List<MugenDefinition> projects = new List<MugenDefinition>();
        /// <summary>
        /// globally-visible editor window
        /// </summary>
        public static CodeEditorWindow editor;
        /// <summary>
        /// validator for statedef and state controllers
        /// </summary>
        public static StateValidator validator = new StateValidator();
    }
}
