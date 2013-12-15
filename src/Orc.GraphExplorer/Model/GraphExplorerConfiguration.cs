using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer.Model
{
    public class GraphExplorerConfigurationSection : ConfigurationSection
    {
        
    }

    public class GraphExplorerSetting : ConfigurationElement
    {
        [ConfigurationProperty("mode", DefaultValue = "")]
        public DisplayMode Value
        {
            get { return (DisplayMode)base["value"]; }
            set { base["value"] = value; }
        }
    }
}
