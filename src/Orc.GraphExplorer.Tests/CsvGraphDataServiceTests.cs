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
        [TestInitialize]
        public void Init()
        {

        }

        [TestMethod]
        public void CsvGraphDataService_Constructor_Test()
        {
            var withDefaultCtor = new CsvGraphDataService();
            Assert.AreEqual(withDefaultCtor.Config, CsvGraphDataServiceConfig.Current);

            var config = new CsvGraphDataServiceConfig()
            {
                VertexesFilePath = "PATH1",
                EdgesFilePath = "PATH2"
            };

            var withConfigPassedInCtor = new CsvGraphDataService(config);
            Assert.AreEqual(withConfigPassedInCtor.Config, config);
        }

        [TestMethod]
        public void GetVertexes_From_File_Test()
        {
            var service = new CsvGraphDataService();
            bool callSuccess = false;
            bool callFail = false;

            service.GetVertexes((data) => { callSuccess = true; }, (e) => { callFail = true; });

            Assert.IsFalse(callFail);
            Assert.IsTrue(callSuccess);
        }

        [TestMethod]
        public void PopulateVertexes_Test()
        {
            var listData = new List<CsvGraphDataService.PropertyData>();

            listData.Add(new CsvGraphDataService.PropertyData()
            {
                ID = 1,
                Property = "First Name",
                Value = "ABC11"
            });
            listData.Add(new CsvGraphDataService.PropertyData()
            {
                ID = 1,
                Property = "Last Name",
                Value = "XYZ11"
            });
            listData.Add(new CsvGraphDataService.PropertyData()
            {
                ID = 2,
                Property = "First Name",
                Value = "ABC22"
            });
            listData.Add(new CsvGraphDataService.PropertyData()
            {
                ID = 2,
                Property = "Last Name",
                Value = "XYZ22"
            });

            var vlist = CsvGraphDataService.PopulateVertexes(listData);

            Assert.IsNotNull(vlist);

            Assert.AreEqual(vlist.Count, 2);

            var vertex1 = vlist.First();

            Assert.AreEqual(vertex1.ID, 1);
            Assert.AreEqual(vertex1.Properties.Count, 2);
            Assert.AreEqual(vertex1.Properties["First Name"], "ABC11");
        }

        [TestMethod]
        public void GetEdges_From_File_Test()
        {
            var service = new CsvGraphDataService();
            bool callSuccess = false;
            bool callFail = false;
            List<DataEdge> resultData = null;
            service.GetEdges((data) =>
            {
                callSuccess = true; resultData = new List<DataEdge>(data);
            },
            (e) =>
            {
                callFail = true;
            });

            Assert.IsFalse(callFail);
            Assert.IsTrue(callSuccess);

            Assert.IsNotNull(resultData);

        }
    }
}
