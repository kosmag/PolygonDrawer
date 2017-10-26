﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GK1
{
    public enum VerticeFix
    {
        Left,
        Right
    }
    public class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();
        public Carbon visuals;

        public GKPolygon(Carbon _visuals)
        {
            visuals = _visuals;
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


        public bool RepairVertices(Edge changedEdge)
        {
            return RepairVertices(vertices.IndexOf(changedEdge.v1)) && RepairVertices(vertices.IndexOf(changedEdge.v2));

        }

        public bool RepairVertices(int changedVerticeIndex)
        {
            bool[] verticeFixed = new bool[vertices.Count];
            int iter = changedVerticeIndex;
            bool first = true;
            // vertices following
            while (first || iter != changedVerticeIndex)
            {
                Vertice v = vertices[iter];
                if (IsCorrectVertice(v, true))
                {
                    if (!first) break;
                }
                else RepairVertice(v, true);


                iter = (iter + 1) % vertices.Count;
                first = false;

            }
            if (!first && iter == changedVerticeIndex) return false;
            int newIter = changedVerticeIndex;
            first = true;
            while (first || newIter != iter)
            {
                Vertice v = vertices[newIter];
                if (IsCorrectVertice(v, false))
                {
                    if (!first) break;
                }
                else RepairVertice(v, false);

                newIter = (newIter - 1 + vertices.Count) % vertices.Count;
                first = false;

            }
            if (!first && iter == newIter) return false;
            return true;
        }

        internal void deleteVertice(Vertice target)
        {
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

        private void RepairVertice(Vertice v, bool front)
        {

            Edge chosen;
            if (front)
                chosen = nextEdge(v);
            else
                chosen = previousEdge(v);


            if (v.fixedAngle)
            {
                ForceAngle(v, v.fixedAngleValue, chosen);
            }
            if (chosen.state == EdgeState.Horizontal)
            {
                chosen.ForceHorizontal();
            }
            if (chosen.state == EdgeState.Vertical)
            {
                chosen.ForceVertical();
            }

        }
        internal bool IsCorrectVertice(Vertice v, bool front)
        {
            Edge tested;
            if (front)
                tested = nextEdge(v);
            else
                tested = previousEdge(v);
            return CorrectAngle(v) && tested.CorrectHorizontal() && tested.CorrectVertical();
        }

        private bool CorrectAngle(Vertice v)
        {
            return !v.fixedAngle || Math.Abs(Global.GetAngle(previousVertice(v), v, nextVertice(v)) - v.fixedAngleValue) < Global.angleEpsilon;

        }
        public Vertice nextVertice(Vertice v)
        {
            int index = vertices.IndexOf(v);
            return vertices[(index + 1) % vertices.Count];
        }
        public Vertice previousVertice(Vertice v)
        {
            int index = vertices.IndexOf(v);
            return vertices[(index - 1 + vertices.Count) % vertices.Count];
        }
        public Edge nextEdge(Vertice v)
        {
            int index = vertices.IndexOf(v);
            return edges[index];
        }
        public Edge previousEdge(Vertice v)
        {
            int index = vertices.IndexOf(v);
            return edges[(index - 1 + vertices.Count) % vertices.Count];
        }
        public void PutVerticeInTheMiddle(Edge target)
        {
            int indexVert = 0;
            int indexEdge = 0;
            target.v1.ClearStatus();
            target.v1.ClearStatus();

            for (int i = 0; i < vertices.Count; i++)
            {
                if (target.v1 == vertices[i] || target.v2 == vertices[i]) indexVert = i;
                if (target == edges[i]) indexEdge = i;
            }
            Point newVerticeCoords = new Point((target.v1.coords.X + target.v2.coords.X) / 2,
                                                (target.v1.coords.Y + target.v2.coords.Y) / 2);
            Vertice newV = new Vertice(newVerticeCoords);
            vertices.Insert(indexVert, newV);
            edges.RemoveAt(indexEdge);
            edges.Insert(indexEdge, new Edge(target.v1, newV));
            edges.Insert(indexEdge + 1, new Edge(newV, target.v2));
        }
        public void ForceAngle(Vertice target, double angle, Edge movable = null)
        {
            Vertice left, right;
            Edge[] pair = new Edge[2];
            int i = 0;
            target.fixedAngle = true;
            target.fixedAngleValue = angle;
            foreach (Edge e in edges)
            {
                if (e.v1 == target || e.v2 == target)
                {
                    pair[i] = e;
                    i++;
                }
            }
            if (pair[0].v1 == target) left = pair[0].v2;
            else left = pair[0].v1;
            if (pair[1].v1 == target) right = pair[1].v2;
            else right = pair[1].v1;

            //double originalAngle = getAngle(left, target, right);
            //if(originalAngle < 0)
            //{
            //    Vertice tmp = left;
            //    left = right;
            //    right = tmp;
            //}

            if (movable == pair[1])
            {
                double originalLeftAngle = Global.AngleAgainstXAxis(target.coords, left.coords);
                Polar pol = new Polar(target.coords, Global.Distance(target.coords, right.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                right.coords = result;
            }
            else
            {
                double originalRightAngle = Global.AngleAgainstXAxis(target.coords, right.coords);

                Polar pol = new Polar(target.coords, Global.Distance(target.coords, left.coords), angle + originalRightAngle);
                Point result = pol.toCartesian();
                left.coords = result;
            }
        }




    }
}