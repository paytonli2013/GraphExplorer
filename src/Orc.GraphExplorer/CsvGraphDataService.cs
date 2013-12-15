using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class CsvGraphDataService : IGraphDataService
    {
        CsvGraphDataServiceConfig _config;

        public CsvGraphDataServiceConfig Config
        {
            get { return _config; }
        }
        
        public CsvGraphDataService()
        {
            _config = CsvGraphDataServiceConfig.Current;
        }

        public CsvGraphDataService(CsvGraphDataServiceConfig config)
        {
            _config = config;
        }

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
