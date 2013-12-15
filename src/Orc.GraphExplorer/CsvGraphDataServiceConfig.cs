using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class CsvGraphDataServiceConfig : ConfigurationElement
    {
        private static GraphExplorerSection _setting;

        public static CsvGraphDataServiceConfig Current
        {
            get
            {
                if (_setting == null)
                    _setting = ConfigurationManager.GetSection("graphExplorer") as GraphExplorerSection;

                return _setting.CsvGraphDataServiceConfig;
            }
        }

        [ConfigurationProperty("vertexesFilePath", IsRequired = true)]
        public string VertexesFilePath
        {
            get { return (string)base["vertexesFilePath"]; }
            set { base["vertexesFilePath"] = value; }
        }

        [ConfigurationProperty("edgesFilePath", IsRequired = true)]
        public string EdgesFilePath
        {
            get { return (string)base["edgesFilePath"]; }
            set { base["edgesFilePath"] = value; }
        }
    }
}
