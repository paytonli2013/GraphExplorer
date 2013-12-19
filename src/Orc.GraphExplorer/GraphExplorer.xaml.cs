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
using System.Windows.Threading;
using Microsoft.Win32;
using GraphX;

namespace Orc.GraphExplorer
{
    /// <summary>
    /// Interaction logic for GraphExplorer.xaml
    /// </summary>
    public partial class GraphExplorer : UserControl
    {
        private List<VertexControl> _selectedVertices = new List<VertexControl>();

        Queue<NavigateHistoryItem> _navigateHistory = new Queue<NavigateHistoryItem>();

        DataVertex _currentNavItem;

        public GraphExplorer()
        {
            InitializeComponent();

            ApplySetting(zoomctrl, Area);
            ApplySetting(zoomctrlNav, AreaNav);

            Area.VertexDoubleClick += Area_VertexDoubleClick;
            AreaNav.VertexDoubleClick += AreaNav_VertexDoubleClick;
            Area.VertexSelected += Area_VertexSelected;

            this.Loaded += (s, e) =>
            {
                var defaultSvc = GraphExplorerSection.Current.DefaultGraphDataService;

                switch (defaultSvc)
                {
                    case GraphDataServiceEnum.Csv:
                        GraphDataService = new CsvGraphDataService();
                        break;
                    case GraphDataServiceEnum.Factory:
                        break;
                    default:
                        break;
                }
            };
        }

        void Area_VertexSelected(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (DragBehaviour.GetIsDragging(args.VertexControl)) return;

                SelectVertex(args.VertexControl);
            }
            else if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                args.VertexControl.ContextMenu = new System.Windows.Controls.ContextMenu();
                var mi = new System.Windows.Controls.MenuItem() { Header = "Delete item", Tag = args.VertexControl };
                //mi.Click += mi_Click;
                args.VertexControl.ContextMenu.Items.Add(mi);
            }
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

        void AreaNav_VertexDoubleClick(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            //throw new NotImplementedException();
            var vertex = args.VertexControl.DataContext as DataVertex;

            if (vertex == null || vertex == _currentNavItem)
                return;

            _currentNavItem = vertex;

            var degree = Area.Graph.Degree(vertex);

            if (degree < 1)
                return;

            NavigateTo(vertex, Area.Graph);
        }

        void Area_VertexDoubleClick(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            if (tbtnCanEdit.IsChecked.HasValue && tbtnCanEdit.IsChecked.Value)
            {
                return;
            }

            var vertex = args.VertexControl.DataContext as DataVertex;

            if (vertex == null)
                return;

            _currentNavItem = vertex;

            var degree = Area.Graph.Degree(vertex);

            if (degree < 1)
                return;

            NavigateTo(vertex, Area.Graph);

            if (navTab.Visibility != System.Windows.Visibility.Visible)
                navTab.Visibility = System.Windows.Visibility.Visible;

            navTab.IsSelected = true;
        }

        private void NavigateTo(DataVertex dataVertex, QuickGraph.BidirectionalGraph<DataVertex, DataEdge> overrallGraph)
        {
            //overrallGraph.get
            var historyItem = GetHistoryItem(dataVertex, overrallGraph);

            var graph = new Graph();

            foreach (var vertex in historyItem.Vertexes)
            {
                graph.AddVertex(vertex);
            }

            foreach (var edge in historyItem.Edges)
            {
                graph.AddEdge(edge);
            }

            AreaNav.ExternalLayoutAlgorithm = new TopologicalLayoutAlgorithm<DataVertex, DataEdge, QuickGraph.BidirectionalGraph<DataVertex, DataEdge>>(graph);

            AreaNav.GenerateGraph(graph, true, true);

            var dispatcher = AreaNav.Dispatcher;

            FitToBounds(dispatcher, zoomctrl);
        }

        private void FitToBounds(System.Windows.Threading.Dispatcher dispatcher, Zoombox zoom)
        {
            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(new Action(()
                    =>
                {
                    zoom.FitToBounds();
                }), DispatcherPriority.Background);
            }
            else
            {
                zoom.FitToBounds();
            }
        }

        private NavigateHistoryItem GetHistoryItem(DataVertex v, QuickGraph.BidirectionalGraph<DataVertex, DataEdge> overrallGraph)
        {
            var hisItem = new NavigateHistoryItem();

            IEnumerable<DataEdge> outs;
            IEnumerable<DataEdge> ins;

            overrallGraph.TryGetInEdges(v, out ins);

            var edges = new List<DataEdge>();

            if (overrallGraph.TryGetOutEdges(v, out outs))
            {
                edges.AddRange(outs);
            }

            if (overrallGraph.TryGetInEdges(v, out ins))
            {
                edges.AddRange(ins);
            }

            if (edges.Count > 0)
            {
                List<DataVertex> vertexes = new List<DataVertex>();
                foreach (var e in edges)
                {
                    if (!vertexes.Contains(e.Source))
                    {
                        vertexes.Add(e.Source);
                    }

                    if (!vertexes.Contains(e.Target))
                    {
                        vertexes.Add(e.Target);
                    }
                }
                hisItem.Edges = edges;
                hisItem.Vertexes = vertexes;
            }

            return hisItem;
        }

        void GetEdges()
        {
            GraphDataService.GetEdges(OnEdgeLoaded, OnError);
        }

        void ApplySetting(Zoombox zoom, GraphArea area)
        {
            Zoombox.SetViewFinderVisibility(zoom, System.Windows.Visibility.Visible);

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            area.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            area.DefaultOverlapRemovalAlgorithmParams = Area.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)area.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)area.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            //Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            area.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            area.AsyncAlgorithmCompute = true;

            //show edage label
            area.ShowAllEdgesLabels(true);
        }

        void OnVertexesLoaded(IEnumerable<DataVertex> vertexes)
        {
            Vertexes = new List<DataVertex>(vertexes);
            UpdateGraphArea();

            FitToBounds(Area.Dispatcher, zoomctrl);
        }

        void OnEdgeLoaded(IEnumerable<DataEdge> edges)
        {
            Edges = edges;
            GraphDataService.GetVertexes(OnVertexesLoaded, OnError);
        }

        private void UpdateGraphArea()
        {
            Area.ClearLayout();

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
            DependencyProperty.Register("Vertexes", typeof(IEnumerable<DataVertex>), typeof(GraphExplorer), new PropertyMetadata(new List<DataVertex>(), VertexesChanged));

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
                ((GraphExplorer)d).GetEdges();
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
            {
                var ge = (GraphExplorer)d;
                ge.ApplySetting(ge.zoomctrl, ge.Area);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (tbtnCanEdit.IsChecked.HasValue && tbtnCanEdit.IsChecked.Value)
            {
                if (MessageBox.Show("Refresh view in edit mode will discard changes you made, will you want to continue?",
                 "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    tbtnCanEdit.IsChecked = false;
                    InnerRefreshGraph();
                }
                else
                {
                    // Do nothing
                }
            }
            else
            {
                InnerRefreshGraph();
            }

        }

        private void InnerRefreshGraph()
        {
            var dispatcher = Area.Dispatcher;

            overrallTab.IsSelected = true;

            int vCount = Vertexes.Count();

            UpdateGraphArea();

            if (vCount > 2)
                FitToBounds(Area.Dispatcher, zoomctrl);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            AreaNav.ClearLayout();

            navTab.Visibility = System.Windows.Visibility.Hidden;

            overrallTab.IsSelected = true;
        }

        private void btnRefresh_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //select overrall tab
            overrallTab.IsSelected = true;

            try
            {
                var dlg = new SaveFileDialog() { Filter = "All files|*.xml", Title = "Select layout file name", FileName = "overrall_layout.xml" };
                if (dlg.ShowDialog() == true)
                {
                    //gg_Area.SaveVisual(dlg.FileName);
                    Area.SaveIntoFile(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            //select overrall tab
            overrallTab.IsSelected = true;

            var dlg = new OpenFileDialog() { Filter = "All files|*.xml", Title = "Select layout file", FileName = "overrall_layout.xml" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    Area.LoadFromFile(dlg.FileName);
                    Area.RelayoutGraph();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Failed to load layout file:\n {0}", ex.ToString()));
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Area.ExportAsPNG();
        }

        private void btnExportNav_Click(object sender, RoutedEventArgs e)
        {
            AreaNav.ExportAsPNG();
        }

        private void settingView_SettingApplied(object sender, SettingAppliedRoutedEventArgs e)
        {
            if (e.NeedRefresh)
            {
                AreaNav.ClearLayout();

                navTab.Visibility = System.Windows.Visibility.Hidden;

                overrallTab.IsSelected = true;

                GraphDataService = new CsvGraphDataService();
            }

            ((SettingView)sender).Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            settingView.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCanDrag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCanDrag(Area, tbtnCanDrag.IsChecked.Value);
        }

        private void UpdateCanDrag(GraphArea area, bool canDrag)
        {
            foreach (var v in area.VertexList)
            {
                DragBehaviour.SetIsDragEnabled(v.Value, canDrag);
            }
            //throw new NotImplementedException();
        }

        private void SelectVertex(VertexControl vc)
        {
            if (_selectedVertices.Contains(vc))
            {
                _selectedVertices.Remove(vc);
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
                _selectedVertices.Add(vc);
                HighlightBehaviour.SetHighlighted(vc, true);
                DragBehaviour.SetIsTagged(vc, true);
            }
        }

        private void tbtnCanEdit_Click(object sender, RoutedEventArgs e)
        {
            if (tbtnCanDrag.IsChecked.HasValue && tbtnCanDrag.IsChecked.Value)
            {
                tbtnCanDrag.IsChecked = false;
                UpdateCanDrag(Area, tbtnCanDrag.IsChecked.Value);
            }

            UpdateHighlightBehaviour();
        }

        private void UpdateHighlightBehaviour()
        {
            if (tbtnCanEdit.IsChecked.Value)
            {
                _selectedVertices.Clear();
                foreach (var v in Area.VertexList)
                {
                    HighlightBehaviour.SetIsHighlightEnabled(v.Value, false);
                    HighlightBehaviour.SetHighlighted(v.Value, false);
                }
                foreach (var edge in Area.EdgesList)
                {
                    HighlightBehaviour.SetIsHighlightEnabled(edge.Value, false);
                    HighlightBehaviour.SetHighlighted(edge.Value, false);
                }
            }
            else
            {
                _selectedVertices.Clear();
                foreach (var v in Area.VertexList)
                {
                    HighlightBehaviour.SetIsHighlightEnabled(v.Value, true);
                    HighlightBehaviour.SetHighlighted(v.Value, false);
                }
                foreach (var edge in Area.EdgesList)
                {
                    HighlightBehaviour.SetIsHighlightEnabled(edge.Value, true);
                    HighlightBehaviour.SetHighlighted(edge.Value, false);
                }
            }
        }

        private void tbnSaveChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbnNewEdge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedVertices.Count == 2)
                {
                    CreateEdge(_selectedVertices[0], _selectedVertices[1], Area);
                }
                else
                {
                    ShowAlertMessage("plase select 2 and only 2 nodes before create a relationship");
                }
            }
            catch (Exception ex)
            {
                ShowAlertMessage(ex.Message);
            }
        }

        private void tbnNewNode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateVertex(Area, zoomctrl);
            }
            catch (Exception ex)
            {
                ShowAlertMessage(ex.Message);
            }
        }

        private void CreateVertex(GraphArea area, Zoombox zoom)
        {
            var vertex = DataVertex.Create();

            Area.Graph.AddVertex(vertex);

            var vCtrl = new VertexControl(vertex);

            HighlightBehaviour.SetIsHighlightEnabled(vCtrl, false);

            HighlightBehaviour.SetHighlighted(vCtrl, true);

            _selectedVertices.Clear();

            _selectedVertices.Add(vCtrl);

            area.AddVertex(vertex, vCtrl);

            area.RelayoutGraph(true);

            zoom.FitToBounds();
            //area.RelayoutFinished += (s, e) => RelayoutFinished(s, e, vCtrl, area, zoom);
        }

        private void CreateEdge(VertexControl from, VertexControl to, GraphArea area)
        {
            if (from == null || to == null)
                return;

            var edge = new DataEdge(from.Vertex as DataVertex, to.Vertex as DataVertex);
            var edgeControl = new EdgeControl(from, to, edge);

            HighlightBehaviour.SetIsHighlightEnabled(edgeControl, false);
            HighlightBehaviour.SetHighlighted(edgeControl, false);
            area.Graph.AddEdge(edge);
            area.AddEdge(edge, edgeControl);
            area.RelayoutGraph(true);

            HighlightBehaviour.SetHighlighted(from, false);
            HighlightBehaviour.SetHighlighted(to, false);
            _selectedVertices.Clear();
        }

        private void SafeRemoveVertex(VertexControl vc, GraphArea area, bool removeFromSelected = false)
        {
            //remove all adjacent edges
            foreach (var item in area.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All))
            {
                var ec = item as EdgeControl;
                area.Graph.RemoveEdge(ec.Edge as DataEdge);
                area.RemoveEdge(ec.Edge as DataEdge);
            }
            area.Graph.RemoveVertex(vc.Vertex as DataVertex);
            area.RemoveVertex(vc.Vertex as DataVertex);
            if (removeFromSelected && _selectedVertices.Contains(vc))
                _selectedVertices.Remove(vc);
        }


        void ShowAlertMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
