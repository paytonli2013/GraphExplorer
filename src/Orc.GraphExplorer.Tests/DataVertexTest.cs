using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer.Tests
{
    [TestClass]
    public class DataVertexTest
    {
        [TestMethod]
        public void DataVertex_Default_Constructor_Test()
        {
            var vertex = new DataVertex();

            Assert.AreEqual(vertex.Id, -1);
            Assert.AreEqual(vertex.Title, "-1");
        }

        [TestMethod]
        public void DataVertex_Constructor_Test() 
        {
            var vertex = new DataVertex(1, "title");

            Assert.AreEqual(vertex.Id, 1);
            Assert.AreEqual(vertex.Title, "title");
        }

        [TestMethod]
        public void DataVertex_AddCommand_Test()
        {
            var vertex = new DataVertex();

            vertex.AddCommand.Execute();

            Assert.AreEqual(vertex.Properties.Count, 1);

            vertex.AddCommand.Execute();

            Assert.AreEqual(vertex.Properties.Count, 2);
        }

    }
}
