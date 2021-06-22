using System;
using UnityEngine;

namespace ROOT.RootMath{
    public class Point
    {
        public float x;
        public float y;

        public static Vector2 ToVector2(Point p) => new Vector2(p.x, p.y);

        public Point(float _x,float _y)
        {
            x = _x;
            y = _y;
        }
        
        public Point(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public override string ToString()
        {
            return "(" + x.ToString("F3") + "," + y.ToString("F3") + ")";
        }
    }
    
    public class Circle//(x-a)^2+(y-b)^2-R^2=0;
    {
        public float a;
        public float b;
        public float r;

        public Point Center()
        {
            return new Point(a, b);
        }

        public Circle(Vector2Int center, float _r) : this(new Vector2(center.x, center.y), _r)
        {

        }

        public Circle(Vector2 center, float _r)
        {
            if (_r == 0)
            {
                throw new ArgumentException("Could not create circle for zero as radius");
            }

            a = center.x;
            b = center.y;
            r = _r;
        }
        
        public Circle(float _a, float _b, float _r)
        {
            if (_r == 0)
            {
                throw new ArgumentException("Could not create circle for zero as radius");
            }

            a = _a;
            b = _b;
            r = _r;
        }
    }
    
    public class Line//y=kx+b
    {
        public float k;
        public float b;
        public bool? VorH = null;//null=generic,true=vertical,false=horizontal

        public float SideIndex(Point A)
        {
            return A.y - k * A.x - b;
        }
        
        public Line(float _k, float _b,bool? _vORh=null)
        {
            k = _k;
            b = _b;
            if (!_vORh.HasValue)
            {
                if (k==0)
                {
                    VorH = false;
                }
                if (float.IsPositiveInfinity(k))
                {
                    VorH = true;
                }
            }
            else
            {
                VorH = _vORh;
            }
        }
        
        public Line(Vector2 A, Vector2 B) : this(new Point(A), new Point(B)) { }

        public Line(Point A, Point B)
        {
            if (B.x == A.x && B.y == A.y)
            {
                throw new ArgumentException("Could not create Line for a single point");
            }

            if (B.x == A.x)
            {
                k = float.PositiveInfinity;
                b = B.x;
                VorH = true;
                return;
            }

            if (B.y == A.y)
            {
                VorH = false;
                k = 0;
                b = B.y;
                return;
            }

            k = (B.y - A.y) / (B.x - A.x);
            b = B.y - k * B.x;
        }

        public override string ToString()
        {
            return "y=" + k.ToString("F2") + "x+" + b.ToString("F2");
        }
    }

    public static class RootMath
    {
        public static double Epsilon { get; } = 1e-4;

        public static Line GetPerpendicularLine(Line l, Point A)
        {
            if (l.VorH.HasValue)
            {
                if (l.VorH.Value)
                {
                    return new Line(0, A.y, false);
                }
                else
                {
                    return new Line(0, A.x, true);
                }
            }

            Debug.Assert(l.k != 0, "source line should be marked as horizontal!");
            var new_k = -1 / l.k;
            Debug.Assert(!float.IsPositiveInfinity(new_k)&&!float.IsNegativeInfinity(new_k), "source line should be marked as vertical!");
            var new_b = A.y - A.x * new_k;
            return new Line(new_k, new_b);
        }
        public static int SolveQuadraticEquation_RealOnly(float a,float b,float c,out float[] res)
        {
            if (a == 0)
            {
                if (b == 0)
                {
                    res = new[] {-c};
                    return 1;
                }

                res = new[] {-c / b};
                return 1;
            }

            var del = b * b - 4 * a * c;
            if (del<0)
            {
                res = new float[0];
                return 0;
            }

            if (del == 0)
            {
                res = new[] {-b / (2 * a)};
                return 1;
            }

            res = new[] {(-b + Mathf.Sqrt(del)) / (2 * a), (-b - Mathf.Sqrt(del)) / (2 * a)};
            return 2;
        }
        public static int SolveLineIntersectCircle(Circle c,Line l,out Point[] res)
        {
            var A = c.a;
            var B = c.b;
            var R = c.r;
            
            //(x-a)^2+(y-b)^2-R^2=0;
            //(x-a)^2+(y-b)^2=R^2;
            if (l.VorH.HasValue)
            {
                if (l.VorH.Value)
                {
                    //(y-b)^2=R^2-(x-a)^2;
                    //x=x0;
                    var A0 = R * R - (l.b - A) * (l.b - A);
                    if (A0<0)
                    {
                        res = new Point[0];
                        return 0;
                    }
                    if (A0==0)
                    {
                        res = new[] {new Point(l.b, B)};
                        return 1;
                    }

                    var resy0 = Mathf.Sqrt(A0) - c.b;
                    var resy1 = -Mathf.Sqrt(A0) - c.b;
                    
                    res = new[]
                    {
                        new Point(l.b, resy0),
                        new Point(l.b, resy1)
                    };
                    return 2;
                }
                else
                {
                    //(x-a)^2=R^2-(y-b)^2;
                    //y=y0;
                    var A0 = R * R - (l.b - B) * (l.b - B);
                    if (A0<0)
                    {
                        res = new Point[0];
                        return 0;
                    }
                    if (A0==0)
                    {
                        res = new[] {new Point(A, l.b)};
                        return 1;
                    }
                    
                    var resx0 = Mathf.Sqrt(A0) - c.a;
                    var resx1 = -Mathf.Sqrt(A0) - c.a;
                    
                    res = new[]
                    {
                        new Point(resx0, l.b),
                        new Point(resx1, l.b)
                    };
                    return 2;
                }
            }
            
            
            //(x-c_a)^2+(y-c_b)^2-c_R^2=0;
            //y=l_k*x+l_b;
            
            //(x-c_a)^2+(l_k*x+l_b-c_b)^2-c_R^2=0;
            
            //c_a=A,c_b=B,c_R=R,l_k=K,l_b=M;
            
            //[(x-A)^2]+[(K*x+M-B)^2]-R^2=0;
            
            //[x^2-2Ax+A^2]+[K^2*x^2+2*k*(M-B)*x+(M-B)^2]-R^2=0;
            
            //(K^2+1)*x^2+(-2A+2*k*(M-B))*x+(A^2+(M-B)^2-R^2)=0;

            var qa = l.k * l.k + 1;
            var qb = (-2 * c.a + 2 * l.k * (l.b - c.b));
            var qc = c.a * c.a + (l.b - c.b) * (l.b - c.b) - c.r * c.r;

            var count = SolveQuadraticEquation_RealOnly(qa, qb, qc, out var qres);
            res = new Point[0];
            switch (count)
            {
                case 1:
                    res = new[] {new Point(qres[0], qres[0] * l.k + l.b)};
                    break;
                case 2:
                    res = new[]
                    {
                        new Point(qres[0], qres[0] * l.k + l.b),
                        new Point(qres[1], qres[1] * l.k + l.b)
                    };
                    break;
            }
            return count;
        }

        public static Line[] GetOuterCoTangentOf2Circle(Circle A, Circle B)
        {
            return GetOuterCoTangentOf2Circle(A, B, out var resP);
        }

        public static Line[] GetOuterCoTangentOf2Circle(Circle A,Circle B,out Point[] resP)
        {
            //RISK 用应用上不可能，还是想着判断一下A，B不全等。
            var cA = A.Center();
            var cB = B.Center();
            var cA_cB = new Line(cA, cB);
            var pCA = GetPerpendicularLine(cA_cB, cA);
            var pCB = GetPerpendicularLine(cA_cB, cB);
            var countCPA = SolveLineIntersectCircle(A, pCA, out var resAp);
            var countCPB = SolveLineIntersectCircle(B, pCB, out var resBp);
            Debug.Assert(countCPA == 2 && countCPB == 2);
            var sideIndexA0 = cA_cB.SideIndex(resAp[0]);//圆的半径大于零，这两个数就不可能等于0.
            var sideIndexB0 = cA_cB.SideIndex(resBp[0]);
            Debug.Assert(sideIndexA0 != 0 && sideIndexB0 != 0);
            if (sideIndexA0 * sideIndexB0 > 0)
            {
                //resAp[0]和resBp[0]同侧
                resP = new[] {resAp[0], resBp[0], resAp[1], resBp[1]};
                return new[]
                {
                    new Line(resAp[0], resBp[0]),
                    new Line(resAp[1], resBp[1]),
                };
            }
            else
            {
                //resAp[0]和resBp[0]异侧
                resP = new[] {resAp[0], resBp[1], resAp[1], resBp[0]};
                return new[]
                {
                    new Line(resAp[0], resBp[1]),
                    new Line(resAp[1], resBp[0]),
                };
            }
        }
    }
}