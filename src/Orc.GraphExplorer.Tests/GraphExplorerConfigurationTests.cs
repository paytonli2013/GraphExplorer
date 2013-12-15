using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer.Tests
{
    [TestClass]
    public class GraphExplorerConfigurationTests
    {
        [TestMethod]
        public void Load_GraphExplorerConfiguration_Test()
        {
            var section = GraphExplorerSection.Current;

            Assert.IsNotNull(section);

            Assert.AreEqual(section.DefaultGraphDataService, GraphDataServiceEnum.Csv);

            var setting = section.Setting;

            Assert.IsNotNull(setting);

            Assert.AreEqual(setting.DisplayMode, DisplayMode.Completed);
        }

        [TestMethod]
        public void Load_CsvGraphDataServiceConfig_Test()
        {
            var config = CsvGraphDataServiceConfig.Current;

            Assert.IsNotNull(config);
            Assert.IsTrue(!string.IsNullOrEmpty(config.VertexesFilePath));
            Assert.IsTrue(!string.IsNullOrEmpty(config.EdgesFilePath));
        }
    }
}
