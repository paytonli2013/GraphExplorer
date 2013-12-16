using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    // Summary:
    //     Represents a service loading data 
    //     from csv file which can be used to generate graph
    public class CsvGraphDataService : IGraphDataService
    {
        // Summary:
        //     Represents data struct in properties
        //     csv file 
        public class PropertyData
        {
            public int ID { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        CsvGraphDataServiceConfig _config;
        List<DataVertex> vlist;
        Dictionary<int, DataVertex> vCache;

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

        // Summary:
        //     get vertexes data by mapping and grouping from csv file 
        //
        // Parameters:
        //   onSuccess:
        //     callback function which will be invoked when 
        //     data successfully loaded and mapped
        //   onFail:
        //     callback function which will be invoked when 
        //     error occured during the processe
        public void GetVertexes(Action<IEnumerable<DataVertex>> onSuccess, Action<Exception> onFail)
        {
            try
            {
                InnerGetVertxes();

                if (onSuccess != null)
                    onSuccess.Invoke(vlist);
            }
            catch (Exception error)
            {
                if (onFail != null)
                    onFail.Invoke(error);
            }
        }

        // Summary:
        //     get vertexes data by mapping and grouping from csv file and generate cache
        private void InnerGetVertxes()
        {
            var vertexesPath = _config.VertexesFilePath;

            if (!File.Exists(vertexesPath))
                throw new Exception("Vertexes data file not found");

            using (var fs = new FileStream(vertexesPath, FileMode.Open))
            using (var reader = new CsvReader(new StreamReader(fs)))
            {
                var records = reader.GetRecords<PropertyData>().ToList();

                vlist = PopulateVertexes(records);
            }

            vCache = UpdateVertexCache(vlist);
        }

        // Summary:
        //   keep & maintain a dictionary for caching DataVertex in case edges creation
        private static Dictionary<int, DataVertex> UpdateVertexCache(List<DataVertex> vlist)
        {
            return vlist.ToDictionary((d) => d.ID, d => d);
        }

        // Summary:
        //   populate vertexes data from properties data loaded from csv file
        public static List<DataVertex> PopulateVertexes(List<PropertyData> records)
        {
            var query = from record in records
                        group record by record.ID into g
                        select new { g.Key, Properties = g.ToDictionary(d => d.Property, d => d.Value) };

            List<DataVertex> vlist = new List<DataVertex>();

            foreach (var result in query)
            {
                var vertex = new DataVertex(result.Key)
                {
                    Properties = result.Properties
                };
                vlist.Add(vertex);
            }

            return vlist;
        }

        // Summary:
        //     get edges data by mapping and grouping from csv file 
        //
        // Parameters:
        //   onSuccess:
        //     callback function which will be invoked when data successfully loaded and mapped
        //   onFail:
        //     callback function which will be invoked when error occured during the processe
        public void GetEdges(Action<IEnumerable<DataEdge>> onSuccess, Action<Exception> onFail)
        {
            try
            {
                if (vlist == null)
                    InnerGetVertxes();

                var edgesFilePath = _config.EdgesFilePath;

                if (!File.Exists(edgesFilePath))
                    throw new Exception("Edges data file not found");

                using (var fs = new FileStream(edgesFilePath, FileMode.Open))
                using (var reader = new CsvReader(new StreamReader(fs)))
                {
                    List<DataEdge> list = new List<DataEdge>();

                    while (reader.Read())
                    {
                        var fromId = reader.GetField<int>(0);
                        var toId = reader.GetField<int>(1);

                        DataVertex from = null;
                        DataVertex to = null;

                        if (vCache.TryGetValue(fromId, out from) && vCache.TryGetValue(toId, out to))
                        {
                            list.Add(new DataEdge(from, to));
                        }
                    }

                    if (onSuccess != null)
                        onSuccess.Invoke(list);
                }
            }
            catch (Exception error)
            {
                if (onFail != null)
                    onFail.Invoke(error);
            }
        }
    }
}
