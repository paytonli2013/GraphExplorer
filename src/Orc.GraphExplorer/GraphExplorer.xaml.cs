using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Orc.GraphExplorer.Model;
using System.Threading.Tasks;
using GraphX.Xceed.Wpf.Toolkit.Zoombox;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Hierarchical;

namespace Orc.GraphExplorer
{
    /// <summary>
    /// Interaction logic for GraphExplorer.xaml
    /// </summary>
    public partial class GraphExplorer : UserControl
    {
        //IGraphDataService _graphDataService;

        //default constructor, no service be injected
        public GraphExplorer()
        {
            InitializeComponent();

            ApplySetting();
        }

        //another constructor for inject IGraphDataService to graph explorer
        public GraphExplorer(IGraphDataService graphDataService)
            : this()
        {
            //load data if graphDataService is provided
            if (graphDataService != null)
                this.Loaded += (s, e) =>
                {
                    GraphDataService = graphDataService;
                };
        }

        void GetVertexes()
        {
            GraphDataService.GetVertexes(OnVertexesLoaded, OnError);
        }

        void ApplySetting()
        {
            Zoombox.SetViewFinderVisibility(zoomctrl, System.Windows.Visibility.Visible);
            zoomctrl.FillToBounds();
            
            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            Area.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            Area.DefaultOverlapRemovalAlgorithmParams = Area.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)Area.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)Area.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            //Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            Area.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            Area.AsyncAlgorithmCompute = true;
        }

        void OnVertexesLoaded(IEnumerable<DataVertex> vertexes)
        {
            Vertexes = vertexes;
            GraphDataService.GetEdges(OnEdgeLoaded, OnError);
        }

        void OnEdgeLoaded(IEnumerable<DataEdge> edges)
        {
            Edges = edges;
            UpdateGraphArea();
            zoomctrl.FitToBounds();
        }

        private void UpdateGraphArea()
        {
            var graph = new Graph();
            foreach (var vertex in Vertexes)
            {
                graph.AddVertex(vertex);
            }

            foreach (var edge in Edges)
            {
                graph.AddEdge(edge);
            }

            Area.ExternalLayoutAlgorithm = new TopologicalLayoutAlgorithm<DataVertex, DataEdge, QuickGraph.BidirectionalGraph<DataVertex, DataEdge>>(graph);

            Area.GenerateGraph(graph, true, true);
        }

        void OnError(Exception ex)
        {

        }

        public IEnumerable<DataVertex> Vertexes
        {
            get { return (IEnumerable<DataVertex>)GetValue(VertexesProperty); }
            set { SetValue(VertexesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Vertexes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VertexesProperty =
            DependencyProperty.Register("Vertexes", typeof(IEnumerable<DataVertex>), typeof(GraphExplorer), new PropertyMetadata(null, VertexesChanged));

        static void VertexesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public IEnumerable<DataEdge> Edges
        {
            get { return (IEnumerable<DataEdge>)GetValue(EdgesProperty); }
            set { SetValue(EdgesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Edges.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EdgesProperty =
            DependencyProperty.Register("Edges", typeof(IEnumerable<DataEdge>), typeof(GraphExplorer), new PropertyMetadata(null, EdgesChanged));

        static void EdgesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public Exception Error
        {
            get { return (Exception)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Error.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register("Error", typeof(Exception), typeof(GraphExplorer), new PropertyMetadata(null));

        public IGraphDataService GraphDataService
        {
            get { return (IGraphDataService)GetValue(GraphDataServiceProperty); }
            set { SetValue(GraphDataServiceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GraphDataService.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphDataServiceProperty =
            DependencyProperty.Register("GraphDataService", typeof(IGraphDataService), typeof(GraphExplorer), new PropertyMetadata(null, GraphDataServiceChanged));

        static void GraphDataServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ((GraphExplorer)d).GetVertexes();
            }
        }

        public GraphExplorerSetting Setting
        {
            get { return (GraphExplorerSetting)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Setting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(GraphExplorerSetting), typeof(GraphExplorer), new PropertyMetadata(null));

        void SettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                ((GraphExplorer)d).ApplySetting();
        }
    }




}
