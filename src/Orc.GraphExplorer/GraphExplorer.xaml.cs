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
        private List<int> _selectedVertices = new List<int>();

        Queue<NavigateHistoryItem> _navigateHistory = new Queue<NavigateHistoryItem>();

        DataVertex _currentNavItem;

        public bool IsInEditMode
        {
            get { return tbtnCanEdit.IsChecked.HasValue && tbtnCanEdit.IsChecked.Value; }
        }

        GraphExplorerViewmodel _viewmodel;

        public GraphExplorer()
        {
            InitializeComponent();

            ApplySetting(zoomctrl, Area);
            ApplySetting(zoomctrlNav, AreaNav);

            Area.VertexDoubleClick += Area_VertexDoubleClick;
            AreaNav.VertexDoubleClick += AreaNav_VertexDoubleClick;

            Area.VertexSelected += Area_VertexSelected;
            Area.EdgeSelected += Area_EdgeSelected;

            AreaNav.GenerateGraphFinished += Area_RelayoutFinished;
            Area.GenerateGraphFinished += Area_RelayoutFinished;

            _viewmodel = new GraphExplorerViewmodel();
            _viewmodel.View = this;
            DataContext = _viewmodel;

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

        void Area_RelayoutFinished(object sender, EventArgs e)
        {
            ShowAllEdgesLabels(sender as GraphArea, true);
        }

        private void ShowAllEdgesLabels(GraphArea area, bool show)
        {
            area.ShowAllEdgesLabels(show);
            area.InvalidateVisual();
        }

        private void Area_EdgeSelected(object sender, GraphX.Models.EdgeSelectedEventArgs args)
        {
            if (IsInEditMode)
            {
                args.EdgeControl.ContextMenu = new System.Windows.Controls.ContextMenu();
                var miDeleteLink = new System.Windows.Controls.MenuItem() { Header = "Delete Link", Tag = args.EdgeControl };
                miDeleteLink.Click += miDeleteLink_Click;
                args.EdgeControl.ContextMenu.Items.Add(miDeleteLink);
            }
        }

        void miDeleteLink_Click(object sender, RoutedEventArgs e)
        {
            var eCtrl = (sender as System.Windows.Controls.MenuItem).Tag as EdgeControl;
            if (eCtrl != null)
            {
                var edge = eCtrl.Edge as DataEdge;

                var op = new DeleteEdgeOperation(Area, edge.Source, edge.Target, edge, (ec) => 
                {
                    //do nothing
                },
                (ec) => 
                {
                    //do nothing
                });

                _viewmodel.Do(op);
            }
            //throw new NotImplementedException();
        }

        void Area_VertexSelected(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (DragBehaviour.GetIsDragging(args.VertexControl)) return;

                SelectVertex(args.VertexControl);
            }
            else if (args.MouseArgs.RightButton == MouseButtonState.Pressed && IsInEditMode)
            {
                args.VertexControl.ContextMenu = new System.Windows.Controls.ContextMenu();
                var miDeleteVertex = new System.Windows.Controls.MenuItem() { Header = "Delete", Tag = args.VertexControl };
                miDeleteVertex.Click += miDeleteVertex_Click;
                args.VertexControl.ContextMenu.Items.Add(miDeleteVertex);
            }
        }

        void miDeleteVertex_Click(object sender, RoutedEventArgs e)
        {
            var vCtrl = (sender as System.Windows.Controls.MenuItem).Tag as VertexControl;
            if (vCtrl != null) 
            {
                var op = new DeleteVertexOperation(Area, vCtrl.Vertex as DataVertex, (dv, vc) => 
                {

                }, dv =>
                {
                    Area.RelayoutGraph(true);
                });

                _viewmodel.Do(op);
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

            FitToBounds(dispatcher, zoomctrlNav);
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
            area.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.None;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            area.AsyncAlgorithmCompute = true;
        }

        void OnVertexesLoaded(IEnumerable<DataVertex> vertexes)
        {
            Vertexes = new List<DataVertex>(vertexes);

            CreateGraphArea(Area, Vertexes, Edges);

            FitToBounds(Area.Dispatcher, zoomctrl);
        }

        void OnEdgeLoaded(IEnumerable<DataEdge> edges)
        {
            Edges = edges;
            GraphDataService.GetVertexes(OnVertexesLoaded, OnError);
        }

        private void CreateGraphArea(GraphArea area, IEnumerable<DataVertex> vertexes, IEnumerable<DataEdge> edges)
        {
            area.ClearLayout();

            var graph = new Graph();
            foreach (var vertex in vertexes)
            {
                graph.AddVertex(vertex);
            }

            foreach (var edge in edges)
            {
                graph.AddEdge(edge);
            }

            area.ExternalLayoutAlgorithm = new TopologicalLayoutAlgorithm<DataVertex, DataEdge, QuickGraph.BidirectionalGraph<DataVertex, DataEdge>>(graph);

            area.GenerateGraph(graph, true, true);

            //area.ShowAllEdgesLabels(true);
            //show edage label
            //area.InvalidateVisual();
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
            GetEdges();
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
            foreach (var item in area.VertexList)
            {
                DragBehaviour.SetIsDragEnabled(item.Value, canDrag);

                if (canDrag)
                {
                    item.Value.EventOptions.PositionChangeNotification = true;
                    item.Value.PositionChanged -= Value_PositionChanged;
                    item.Value.PositionChanged += Value_PositionChanged;
                }
            }
            //throw new NotImplementedException();
        }

        void Value_PositionChanged(object sender, GraphX.Models.VertexPositionEventArgs args)
        {
            double offset = 20;
            var zoomtop = zoomctrl.TranslatePoint(new Point(0, 0), Area);
            //dg_Area.UpdateLayout();
            var zoombottom = new Point(Area.ActualWidth, Area.ActualHeight);

            var posOff = args.OffsetPosition;
            var pos = args.Position;

            //if (posOff.X < zoomtop.X)
            //{
            //    GraphAreaBase.SetX(args.VertexControl, zoomtop.X + offset, true);
            //}
            //if (posOff.Y < zoomtop.Y + offset)
            //{
            //    GraphAreaBase.SetY(args.VertexControl, zoomtop.Y + pos.Y, true);
            //}

            //if (posOff.X > zoombottom.X)
            //{
            //    GraphAreaBase.SetX(args.VertexControl, zoombottom.X, true);
            //}
            //if (posOff.Y > zoombottom.Y)
            //{
            //    GraphAreaBase.SetY(args.VertexControl, zoombottom.Y + offset, true);
            //}
        }

        private void SelectVertex(VertexControl vc)
        {
            var v = vc.Vertex as DataVertex;
            if (v == null)
                return;

            if (_selectedVertices.Contains(v.Id))
            {
                _selectedVertices.Remove(v.Id);
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
                _selectedVertices.Add(v.Id);
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

            UpdateIsInEditMode(Area.VertexList, tbtnCanEdit.IsChecked.Value);
            UpdateHighlightBehaviour(true);
        }

        private void UpdateIsInEditMode(IDictionary<DataVertex, VertexControl> dictionary, bool isInEditMode)
        {
            if (dictionary == null)
                return;

            foreach (var v in dictionary)
            {
                v.Key.IsEditing = isInEditMode;

                if (isInEditMode)
                {
                    v.Key.ChangedCommited += Key_ChangedCommited;
                }
                else
                {
                    v.Key.ChangedCommited -= Key_ChangedCommited;
                }
            }
            //throw new NotImplementedException();
        }

        void Key_ChangedCommited(object sender, EventArgs e)
        {
            var data = (DataVertex)sender;

            GraphDataService.UpdateVertex(data, (r, v, ex) =>
            {
                if (ex != null)
                {
                    ShowAlertMessage(ex.ToString());
                }
            });
            //throw new NotImplementedException();
        }

        private void UpdateHighlightBehaviour(bool clearSelectedVertices)
        {
            if (clearSelectedVertices)
                _selectedVertices.Clear();

            if (tbtnCanEdit.IsChecked.Value)
            {
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
            try
            {
                GraphDataService.UpdateEdges(Area.Graph.Edges, (result, error) =>
                {
                    if (!result && error != null)
                        ShowAlertMessage(error.Message);
                });

                GraphDataService.UpdateVertexes(Area.Graph.Vertices, (result, error) =>
                {
                    if (!result && error != null)
                        ShowAlertMessage(error.Message);
                });

                _selectedVertices.Clear();

                //clear dirty flag
                _viewmodel.Commit();

                tbtnCanEdit.IsChecked = false;

                UpdateIsInEditMode(Area.VertexList, tbtnCanEdit.IsChecked.Value);

                UpdateHighlightBehaviour(true);
                //GetEdges();
            }
            catch (Exception ex)
            {
                ShowAlertMessage(ex.Message); ;
            }
        }

        private void tbnNewEdge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedVertices.Count == 2)
                {
                    CreateEdge(_selectedVertices[0], _selectedVertices[1], Area);

                    //FitToBounds(Area.Dispatcher, zoomctrl);
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
            _viewmodel.Do(new CreateVertexOperation(Area, null,
                (v, vc) =>
                {
                    _selectedVertices.Add(v.Id);

                    area.RelayoutGraph(true);

                    UpdateHighlightBehaviour(false);

                    foreach (var selectedV in _selectedVertices)
                    {
                        var localvc = area.VertexList.Where(pair => pair.Key.Id == selectedV).Select(pair => pair.Value).FirstOrDefault();
                        HighlightBehaviour.SetHighlighted(localvc, true);
                    }
                },
                (v) =>
                {
                    _selectedVertices.Remove(v.Id);
                    //on vertex recreated
                }));



            //FitToBounds(area.Dispatcher, zoom);
        }

        private void CreateEdge(int fromId, int toId, GraphArea area)
        {
            var source = area.VertexList.Where(pair => pair.Key.Id == fromId).Select(pair => pair.Key).FirstOrDefault();
            var target = area.VertexList.Where(pair => pair.Key.Id == toId).Select(pair => pair.Key).FirstOrDefault();
            if (source == null || target == null)
                return;

            _viewmodel.Do(new CreateEdgeOperation(Area, source, target,
                (e) =>
                {
                    //on vertex created
                    //_selectedVertices.Add(v.Id);
                    HighlightBehaviour.SetIsHighlightEnabled(e, false);
                    HighlightBehaviour.SetHighlighted(e, false);

                    HighlightBehaviour.SetHighlighted(area.VertexList[source], false);
                    HighlightBehaviour.SetHighlighted(area.VertexList[target], false);

                    UpdateHighlightBehaviour(true);
                },
                (e) =>
                {
                    //_selectedVertices.Remove(v.Id);
                    //on vertex recreated
                }));
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

            var v = vc.Vertex as DataVertex;
            area.Graph.RemoveVertex(v);
            area.RemoveVertex(v);

            if (removeFromSelected && v != null && _selectedVertices.Contains(v.Id))
                _selectedVertices.Remove(v.Id);
        }

        void ShowAlertMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
