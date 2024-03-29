﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;

namespace GK1
{

    public class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();
        public Carbon visuals;
        public double angleToXAxis;

        public GKPolygon(Carbon _visuals)
        {
            visuals = _visuals;
            angleToXAxis = 0;
        }

        public GKPolygon(GKPolygon original)
        {
            visuals = original.visuals;
            for (int i = 0; i < original.vertices.Count; i++)
                vertices.Add(new Vertice(original.vertices[i]));
            for (int i = 0; i < original.vertices.Count - 1; i++)
                edges.Add(new Edge(vertices[i], vertices[i + 1]));
            edges.Add(new Edge(vertices.Last(), vertices.First()));
        }


        public void AddNewVertice(System.Windows.Point location, ApplicationMode mode)
        {
            Vertice newV = new Vertice(location);
            if (mode == ApplicationMode.NewPolyline && vertices.Count >= 1)
            {
                edges.Add(new Edge(vertices.Last(), newV));
            }
            vertices.Add(newV);
        }


        internal void deleteVertice(Vertice target)
        {
            if (vertices.Count <= 3)
            {
                MessageBox.Show("Polygon should have at least 3 vertices!", "Not enough vertices", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int index = vertices.IndexOf(target);

            vertices.RemoveAt(index);
            edges.RemoveAt(index);
            if (index != 0)
            {
                edges.Insert(index, new Edge(vertices[(index - 1 + vertices.Count) % vertices.Count], vertices[index % vertices.Count]));
                edges.RemoveAt((index - 1 + edges.Count) % edges.Count);
            }
            else
            {

                edges.RemoveAt((index - 1 + edges.Count) % edges.Count);

                edges.Add(new Edge(vertices[(index - 1 + vertices.Count) % vertices.Count], vertices[index % vertices.Count]));
            }


        }
        public void PutVerticeInTheMiddle(Edge target)
        {
            int indexVert1 = 0, indexVert2 = 0;
            int indexEdge = 0;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (target.v1 == vertices[i]) indexVert1 = i;
                if (target.v2 == vertices[i]) indexVert2 = i;
                if (target == edges[i]) indexEdge = i;
            }
            System.Windows.Point newVerticeCoords = new System.Windows.Point((target.v1.coords.X + target.v2.coords.X) / 2,
                                                (target.v1.coords.Y + target.v2.coords.Y) / 2);
            Vertice newV = new Vertice(newVerticeCoords);
            if (Math.Abs(indexVert1 - indexVert2) == vertices.Count - 1)
                vertices.Insert(vertices.Count, newV);
            else
                vertices.Insert((indexVert1 + indexVert2) / 2 + 1, newV);
            PopulateEdges();
        }

        internal void PopulateEdges()
        {
            edges = new List<Edge>();
            if (vertices.Count != 0)
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    edges.Add(new Edge(vertices[i], vertices[i + 1]));
                }
                edges.Add(new Edge(vertices.Last(), vertices.First()));
            }
        }

    }

}
