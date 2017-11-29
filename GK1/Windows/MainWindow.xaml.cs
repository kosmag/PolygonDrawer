﻿using GK1.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace GK1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public GKPolyline currentPolyline;
        public GKPolygon currentImageState;
        public Carbon visuals;
        Edge currentlyedgeDragged;
        Vector mouseToVertice1;
        Vector mouseToVertice2;
        ApplicationMode mode;
        private Point dragWholePolylineCoords;
        private bool polylineDragged;
        bool dragged = false;
        bool edgeDragged = false;
        int currentlyDragged = 0;
        ImageSource texture;
        Random rnd = new Random(1337);


        public MainWindow()
        {

            InitializeComponent();
            visuals = new Carbon(lineCarbon);

            mode = ApplicationMode.Standard;
            currentPolyline = new GKPolyline(visuals);
            currentImageState = new GKPolygon(visuals);
            //2 wielokąty na ekranie
            OnNewVertice(new Point(100, 200));
            OnNewVertice(new Point(100, 400));
            OnNewVertice(new Point(500, 300));
            OnNewVertice(new Point(400, 200));
            currentPolyline.PopulateEdges();
            visuals.RefreshPolyline(currentPolyline);
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(160000);
            dispatcherTimer.Start();

            visuals.UpdateScreen();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

        }

        private void OnNewVertice(Point location)
        {
            Vertice newV = new Vertice(location);
            if (mode == ApplicationMode.NewPolyline && currentPolyline.vertices.Count >= 1)
            {
                currentPolyline.edges.Add(new Edge(currentPolyline.vertices.Last(), newV));
            }
            currentPolyline.vertices.Add(newV);
            visuals.RefreshPolyline(currentPolyline);
        }
        private void OnMovedPolyline(MouseButtonEventArgs e)
        {
            dragWholePolylineCoords = e.GetPosition(drawingScreen);
            polylineDragged = true;
        }

        private void OnClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            mouseToVertice1 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v1.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v1.coords.Y);
            mouseToVertice2 = new Vector(e.GetPosition(drawingScreen).X - targetEdge.v2.coords.X,
                                         e.GetPosition(drawingScreen).Y - targetEdge.v2.coords.Y);

            edgeDragged = true;
            currentlyedgeDragged = targetEdge;
        }

        private Edge ClickedEdge(Point point)
        {
            int xIndex = (int)point.X * 3;
            int yIndex = (int)point.Y * visuals.rawStride;
            if (xIndex < visuals.width && yIndex < visuals.height && currentPolyline.edges.Contains(visuals.pixelOwner[xIndex + yIndex]))
                return visuals.pixelOwner[xIndex + yIndex];
            else
                return null;
        }



        private void OnClickedVertice(Vertice target)
        {
            if (mode == ApplicationMode.NewPolyline)
            {
                if (target == currentPolyline.vertices.First())
                {
                    currentPolyline.edges.Add(new Edge(currentPolyline.vertices.Last(), currentPolyline.vertices.First()));
                    endNewPolylineMode();
                }

            }

            else if (currentPolyline.vertices.Contains(target))
            {
                dragged = true;
                currentlyDragged = currentPolyline.vertices.IndexOf(target);
            }
            visuals.RefreshPolyline(currentPolyline);
        }

        private void endNewPolylineMode()
        {
            mode = ApplicationMode.Standard;
        }
        private void endAddPolylineMode()
        {
            mode = ApplicationMode.Standard;
        }
        private Vertice ClickedVertice(Point location)
        {

            foreach (Vertice v in currentPolyline.vertices)
            {
                if (Math.Abs(v.coords.X - location.X) <= Global.verticeRadius && Math.Abs(v.coords.Y - location.Y) <= Global.verticeRadius)
                {
                    return v;
                }
            }
            return null;
        }






        private void ClearCanvas()
        {
            drawingScreen.Children.Clear();
            drawingScreen.Children.Add(lineCarbon);
        }


        private void newPolyline_Click(object sender, RoutedEventArgs e)
        {

            ClearCanvas();
            visuals.clear();
            mode = ApplicationMode.Standard;
            dragged = false;
            currentPolyline = new GKPolyline(visuals);
            //2 wielokąty na ekranie
            OnNewVertice(new Point(100, 200));
            OnNewVertice(new Point(100, 400));
            OnNewVertice(new Point(500, 300));
            OnNewVertice(new Point(400, 200));
            currentPolyline.PopulateEdges();

            visuals.RefreshPolyline(currentPolyline);
        }




        private void DraggingPolyline(Point pt)
        {
            Vector displacement = pt - dragWholePolylineCoords;
            foreach (Vertice v in currentPolyline.vertices)
            {
                v.coords += displacement;
            }
            visuals.RefreshPolyline(currentPolyline);
            dragWholePolylineCoords = pt;
        }

        private void drawingScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged)
            {

                currentPolyline.vertices[currentlyDragged].coords.X = Mouse.GetPosition(drawingScreen).X;
                currentPolyline.vertices[currentlyDragged].coords.Y = Mouse.GetPosition(drawingScreen).Y;
                visuals.RefreshPolyline(currentPolyline);
                return;
            }
            if (edgeDragged)
            {

                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                visuals.RefreshPolyline(currentPolyline);

                return;
            }
            if (polylineDragged)
            {
                DraggingPolyline(e.GetPosition(drawingScreen));
                return;
            }

        }

        private void drawingScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged)
            {
                currentPolyline.vertices[currentlyDragged].coords.X = e.GetPosition(drawingScreen).X;
                currentPolyline.vertices[currentlyDragged].coords.Y = e.GetPosition(drawingScreen).Y;
                visuals.RefreshPolyline(currentPolyline);
                dragged = false;
                return;
            }
            if (edgeDragged)
            {

                currentlyedgeDragged.v1.coords.X = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v1.coords.Y = Vector.Add(-mouseToVertice1, Mouse.GetPosition(drawingScreen)).Y;
                currentlyedgeDragged.v2.coords.X = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).X;
                currentlyedgeDragged.v2.coords.Y = Vector.Add(-mouseToVertice2, Mouse.GetPosition(drawingScreen)).Y;
                visuals.RefreshPolyline(currentPolyline);
                edgeDragged = false;
                return;
            }
            if (polylineDragged)
            {
                DraggingPolyline(e.GetPosition(drawingScreen));
                polylineDragged = false;
                return;
            }
        }




        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) OnClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) OnClickedEdge(targetEdge, e);
                else if (mode == ApplicationMode.NewPolyline) OnNewVertice(e.GetPosition(drawingScreen));

                else
                {
                    OnMovedPolyline(e);
                }

            }
        }


        private void drawingScreen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Vertice targetVertice = ClickedVertice(e.GetPosition(drawingScreen));
            if (targetVertice != null) OnRightClickedVertice(targetVertice);
            else
            {
                Edge targetEdge = ClickedEdge(e.GetPosition(drawingScreen));
                if (targetEdge != null) OnRightClickedEdge(targetEdge, e);

            }
        }


        private void OnRightClickedEdge(Edge targetEdge, MouseButtonEventArgs e)
        {
            ShowModifyEdgeWindow(targetEdge);
        }

        private void ShowModifyEdgeWindow(Edge modificationTarget)
        {
            EdgeSettings window = new EdgeSettings(this, modificationTarget);
            window.ShowDialog();
        }

        private void OnRightClickedVertice(Vertice targetVertice)
        {
            ShowSetAngleWindow(targetVertice);
        }

        private void ShowSetAngleWindow(Vertice modificationTarget)
        {
            SetAngle window = new SetAngle(this, modificationTarget);
            window.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseTexture window = new ChooseTexture(this);
            window.ShowDialog();
        }

        private void savePolyline_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "polyline",
                DefaultExt = ".xml",
                Filter = "XML documents (.xml)|*.xml"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                XmlSerializer xs = new XmlSerializer(typeof(Vertice[]));
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    xs.Serialize(fs, currentPolyline.vertices.ToArray());
                }
            }
        }

        private void loadPolyline_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                FileName = "polyline.xml",
                DefaultExt = ".xml",
                Filter = "XML documents (.xml)|*.xml"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                XmlSerializer xs = new XmlSerializer(typeof(Vertice[]));
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    try
                    {
                        Vertice[] verticeArray = (Vertice[])(xs.Deserialize(fs));
                        currentPolyline.vertices = verticeArray.ToList();
                        currentPolyline.PopulateEdges();
                        visuals.RefreshPolyline(currentPolyline);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("This file is not suitable for loading", "Bad file", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void generateVertices_Click(object sender, RoutedEventArgs e)
        {
            int verticeNumber;
            if (!int.TryParse(this.verticeNumber.Text, out verticeNumber) || verticeNumber < 2)
            {
                MessageBox.Show("Please put at least 2 vertices!", "Bad vertice number", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentPolyline = new GKPolyline(visuals);


            for (int i = 0; i < verticeNumber; i++)
            {
                OnNewVertice(new Point(visuals.width * i / verticeNumber + rnd.NextDouble() * visuals.width / verticeNumber, rnd.NextDouble() * visuals.height));

            }
            currentPolyline.PopulateEdges();

            visuals.RefreshPolyline(currentPolyline);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.showPolyline = true;
                visuals.RefreshPolyline(currentPolyline);

            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.showPolyline = false;
                visuals.RefreshPolyline(currentPolyline);

            }
        }

        private void startAnimation_Click(object sender, RoutedEventArgs e)
        {
            visuals.texturePixelData = Global.ImageSourceToBytes(currentImage.Source);

            visuals.texturePixelDataWidth = ((BitmapImage)(currentImage.Source)).PixelWidth;
            visuals.texturePixelDataHeight = ((BitmapImage)(currentImage.Source)).PixelHeight;
            OnNewVertice(new Point(0, 0));
            OnNewVertice(new Point(visuals.texturePixelDataWidth, 0));
            OnNewVertice(new Point(visuals.texturePixelDataWidth, visuals.texturePixelDataHeight));
            OnNewVertice(new Point(0, visuals.texturePixelDataHeight));

        }

        private void stopAnimation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rotationMoving_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.fixedRotation = true;
            }
        }

        private void curveMoving_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.fixedRotation = false;
            }
        }

        private void shearRotation_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.shearRotation = true;
            }
        }

        private void naiveRotation_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.shearRotation = false;
            }
        }

        private void grayscale_Checked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.inGrayscale = true;
            }
        }

        private void grayscale_Unchecked(object sender, RoutedEventArgs e)
        {
            if (visuals != null)
            {
                visuals.inGrayscale = false;
            }
        }
    }

    enum ApplicationMode
    {
        Standard,
        NewPolyline,
        AddPolyline,
        ClipPolyline
    }



}
