using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer.Tests
{
    [TestClass]
    public class CsvGraphDataServiceTests
    {
        [TestMethod]
        public void CsvGraphDataService_Constructor_Test()
        {
            var withDefaultCtor = new CsvGraphDataService();
            Assert.AreEqual(withDefaultCtor.Config,CsvGraphDataServiceConfig.Current);

            var config = new CsvGraphDataServiceConfig()
            {
                 VertexesFilePath = "PATH1",
                 EdgesFilePath = "PATH2"
            };

            var withConfigPassedInCtor = new CsvGraphDataService(config);
            Assert.AreEqual(withConfigPassedInCtor.Config, config);
        }
    }
}
