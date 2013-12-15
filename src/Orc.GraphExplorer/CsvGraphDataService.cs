using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class CsvGraphDataService : IGraphDataService
    {

        public void GetVertexes(Action<IEnumerable<DataVertex>> onSuccess, Action<Exception> onFail)
        {
            throw new NotImplementedException();
        }

        public void GetEdges(Action<IEnumerable<DataEdge>> onSuccess, Action<Exception> onFail)
        {
            throw new NotImplementedException();
        }
    }
}
