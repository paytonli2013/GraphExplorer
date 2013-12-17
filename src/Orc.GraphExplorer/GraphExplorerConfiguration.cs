using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class GraphExplorerSection : ConfigurationSection
    {
        private static GraphExplorerSection _setting;

        public static GraphExplorerSection Current
        {
            get
            {
                if (_setting==null)
                    _setting = ConfigurationManager.GetSection("graphExplorer") as GraphExplorerSection;

                return _setting;
            }
        }

        [ConfigurationProperty("csvGraphDataServiceConfig", IsRequired = false)]
        public CsvGraphDataServiceConfig CsvGraphDataServiceConfig
        {
            get { return (CsvGraphDataServiceConfig)base["csvGraphDataServiceConfig"]; }
            set { base["csvGraphDataServiceConfig"] = value; }
        }

        [ConfigurationProperty("setting",IsRequired=false)]
        public GraphExplorerSetting Setting
        {
            get { return (GraphExplorerSetting)base["setting"]; }
            set { base["setting"] = value; }
        }
    
        [ConfigurationProperty("defaultGraphDataService", DefaultValue = GraphDataServiceEnum.Csv)]
        public GraphDataServiceEnum DefaultGraphDataService
        {
            get { return (GraphDataServiceEnum)base["defaultGraphDataService"]; }
            set { base["defaultGraphDataService"] = value; }
        }

        [ConfigurationProperty("graphDataServiceFactory", IsRequired=false)]
        public string GraphDataServiceFactory
        {
            get { return (string)base["graphDataServiceFactory"]; }
            set { base["graphDataServiceFactory"] = value; }
        }
    }

    public class GraphExplorerSetting : ConfigurationElement
    {
        [ConfigurationProperty("enableNavigation",IsRequired = false, DefaultValue = false)]
        public bool EnableNavigation
        {
            get { return (bool)base["enableNavigation"]; }
            set { base["enableNavigation"] = value; }
        }

        [ConfigurationProperty("navigateToNewTab", IsRequired = false, DefaultValue = true)]
        public bool NavigateToNewTab
        {
            get { return (bool)base["navigateToNewTab"]; }
            set { base["navigateToNewTab"] = value; }
        }
    }
}
