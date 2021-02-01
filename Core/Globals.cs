using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUGENStudio
{
    public static class Globals
    {
        // settings internal to the MUGEN Studio application
        public static StudioSettings settingsSingleton = new StudioSettings("mugenstudio.xml");
        // the current project data
        public static MugenDefinition project;
    }
}
