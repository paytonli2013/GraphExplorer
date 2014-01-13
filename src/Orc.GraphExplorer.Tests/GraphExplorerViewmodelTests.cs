using GraphX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer.Tests
{
    [TestClass]
    public class GraphExplorerViewmodelTests
    {
        [TestMethod]
        public void GraphExplorerViewmodel_Constructor_Test()
        {
            var graphVM = new GraphExplorerViewmodel();

            Assert.IsNotNull(graphVM.Operations);
            Assert.IsNotNull(graphVM.OperationsRedo);

            Assert.IsFalse(graphVM.HasChange);
            Assert.IsFalse(graphVM.HasRedoable);
            Assert.IsFalse(graphVM.HasUndoable);
        }

        [TestMethod]
        public void GraphExplorerViewmodel_Do_CreateVertex_Operation_Test()
        {
            var graphVM = new GraphExplorerViewmodel();

            Assert.IsNotNull(graphVM.Operations);
            Assert.IsNotNull(graphVM.OperationsRedo);

            Assert.IsFalse(graphVM.HasChange);
            Assert.IsFalse(graphVM.HasRedoable);
            Assert.IsFalse(graphVM.HasUndoable);

            bool createSourceCalled = false;
            bool undoCreateSourceCalled = false;
            var graph = new Model.GraphArea();
            var source = new DataVertex(100) { Title = "Test" };
            VertexControl sourceVC = null;

            var cvoSource = new CreateVertexOperation(graph, source, (sv, svc) =>
            {
                createSourceCalled = true;
                sourceVC = svc;
            },
            (v) =>
            {
                 undoCreateSourceCalled = true;
            });

            graphVM.Do(cvoSource);

            Assert.IsTrue(createSourceCalled);
            Assert.IsNotNull(sourceVC);
            Assert.IsTrue(graph.Graph.Vertices.Any(v => v == source));
            Assert.AreEqual(source, sourceVC.Vertex, "source are not equal");

            Assert.IsTrue(graphVM.HasChange);
            //Assert.IsTrue(graphVM.HasRedoable);
            Assert.IsTrue(graphVM.HasUndoable);

            graphVM.UndoCommand.Execute();

            Assert.IsTrue(undoCreateSourceCalled);
            Assert.IsNotNull(sourceVC);
            Assert.IsFalse(graph.Graph.Vertices.Any(v => v == source));
            Assert.IsNull(sourceVC.Vertex, "source sould be null");

            Assert.IsTrue(graphVM.HasChange);
            Assert.IsTrue(graphVM.HasRedoable);
            //Assert.IsTrue(graphVM.HasUndoable);
        }
    }
}
