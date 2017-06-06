﻿using MathNet.Symbolics;
using Symbolic.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expr = MathNet.Symbolics.Expression;
using PolyLib;

namespace Symbolic.Model.Polynomial
{
    static class Extensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    class Pair
    {
        int power;
        Polynom p;    

        public Pair(int pow, Polynom poly)
        {
            P = poly;
            Power = pow;
        }

        public int Power
        {
            get
            {
                return power;
            }

            set
            {
                power = value;
            }
        }

        internal Polynom P
        {
            get
            {
                return p;
            }

            set
            {
                p = value;
            }
        }
    }

    class Polynom : Base.Function, ICloneable
    {
        /// <summary>
        /// Список мономов
        /// </summary>
        public List<Monom> Monoms;

        #region Свойства

        public Monom LT => Monoms.Count < 1 ? null : Monoms.First();

        public Monom LM => new Monom(1, LT.Powers);

        public double LC => LT.Coef;

        public bool IsNull => Degree == 0;

        public int Degree
        {
            get
            {
                var max = 0;
                foreach (var t in Monoms)
                {
                    var sum = 0;
                    for (var i = 0; i < t.Powers.Count; i++)
                    {
                        sum += t.Powers[i].Item2;
                    }

                    if (sum > max)
                        max = sum;
                }
                return max;
            }
        }

        public List<Tuple<string, int>> Multideg => LT.Powers;

        public new Monom this[int index]
        {
            get { return Monoms[index]; }
            set { Monoms[index] = value; }
        }

        #endregion

        #region Конструкторы

        public Polynom()
        {
            Monoms = new List<Monom>();
        }

        public Polynom(List<Monom> ms)
        {
            Monoms = new List<Monom>(ms);
        }

        #endregion

        #region Производная, интеграл, значение в точке

        /// <summary>
        /// Производная по какой-то переменной
        /// </summary>
        /// <param name="varnum"> Порядок переменной </param>
        /// <returns></returns>
        public override Base.Function Derivative()
        {
            var f = Infix.ParseOrThrow(this.ToString());
            var der = Calculus.Differentiate(Expr.Symbol("x"), f);
            var poly = PolynomParser.Parse(Infix.Format(der));

            return poly;
        }

        /// <summary>
        /// Вычисление значения в точке (x1, x2, ..., xn)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public override double Calc(params double[] val)
        {
            return 0;
        }

        #endregion

        /// <summary>
        /// Копия объекта "Полином"
        /// </summary>
        /// <returns> Полная копия объекта </returns>
        public object Clone()
        {
            return new Polynom
            {
                Monoms = new List<Monom>(Monoms)
            };
        }

        /// <summary>
        /// Упрощение
        /// </summary>
        /// <returns> Упрощенный полином </returns>
        public Polynom SimplifyPolynom()
        {
            var nullCoefs = new List<int>();

            for (var i = 0; i < Monoms.Count; i++)
            {
                //найдем нулевые коэффициенты
                if (Monoms[i].Coef == 0)
                {
                    Monoms.RemoveAt(i);
                    //nullCoefs.Add(i);
                    i--;
                }
            }

            for (var i = 0; i < Monoms.Count; i++)
            {
                Monoms[i].Powers.RemoveAll(x => x.Item2 == 0);
            }

            for (var i = 0; i < Monoms.Count; i++)
            {
                for (var j = i + 1; j < Monoms.Count; j++)
                {
                    if (Monom.AreEqual(Monoms[i], Monoms[j]))
                    {
                        Monoms[i] = Monoms[i] + Monoms[j];
                        Monoms.RemoveAt(j);
                    }
                }
            }

            return new Polynom(Monoms);
        }

        #region Наибольший общий делитель

        /// <summary>
        /// Наибольший общий делитель 2-х полиномов
        /// </summary>
        /// <param name="f"> Полином </param>
        /// <param name="g"> Полином </param>
        /// <returns> НОД - полином </returns>
        public static Polynom GetGCD(Polynom f, Polynom g)
        {
            var h = f.Degree > g.Degree ? (Polynom)f.Clone() : (Polynom)g.Clone();
            var s = f.Degree < g.Degree ? (Polynom)f.Clone() : (Polynom)g.Clone();

            h = LexOrder.CreateOrderedPolynom(h);
            s = LexOrder.CreateOrderedPolynom(s);

            while (!s.IsNull)
            {
                if (h.Degree >= s.Degree)
                {
                    Polynom rem;
                    List<Monom> q;
                    DividePolynoms(h, s, out q, out rem);
                    h = s;
                    s = rem;
                }
                else
                    break;
            }

            return h;
        }

        /// <summary>
        /// Наибольший общий делитель 3 и более полиномов
        /// </summary>
        /// <param name="polynoms"> Список полиномов </param>
        /// <returns></returns>
        public static Polynom GetGCD(params Polynom[] polynoms)
        {
            var h = new Polynom();
            var sortedByDescending = polynoms.OrderBy(p => p.Degree).ToList();

            for (var i = sortedByDescending.Count - 2; i >= 0; i--)
            {
                sortedByDescending[i] = GetGCD(sortedByDescending[i], sortedByDescending[i + 1]);
                sortedByDescending.RemoveAt(i + 1);
            }

            return sortedByDescending.First();
        }

        /// <summary>
        /// Деление полиномов с остатком
        /// </summary>
        /// <param name="f"> Делимое </param>
        /// <param name="g"> Делитель </param>
        /// <param name="q"> Частное </param>
        /// <param name="r"> Остаток </param>
        public static void DividePolynoms(Polynom f, Polynom g, out List<Monom> q, out Polynom r)
        {
            q = new List<Monom>();
            r = (Polynom)f.Clone();

            while (r.Degree >= g.Degree)
            {
                if (!r.IsNull && Monom.CanDivide(r.LT, g.LT))
                {
                    var divLT = r.LT / g.LT;
                    q.Add(divLT);
                    var temp = g * (divLT);
                    r = r - temp;
                    r = r.SimplifyPolynom();
                    r = LexOrder.CreateOrderedPolynom(r);                  
                }
                else
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Строковое представление полинома
        /// </summary>
        /// <returns> Полином-строка </returns>
        public override string ToString()
        {
            var stringPoly = new StringBuilder();

            if (Monoms.Count > 0)
            {
                for (var i = 0; i < Monoms.Count; i++)
                {
                    stringPoly.Append(Monoms[i]);
                    if ((i + 1) < Monoms.Count)
                    {
                        stringPoly.Append(" + ");
                    }
                    else if ((i + 1) == Monoms.Count)
                    {
                        break;
                    }
                }
            }
            else
            {
                stringPoly.Append("0");
            }
            return stringPoly.ToString();
        }

        /// <summary>
        /// S-полином
        /// </summary>
        /// <param name="a"> Полином </param>
        /// <param name="b"> Полином </param>
        /// <returns></returns>
        public static Polynom S_polynom(Polynom a, Polynom b)
        {
            var lcm = Monom.GetLCM(a.LT, b.LT);
            var sp = a * (lcm / a.LT) - b * (lcm / b.LT);
            return sp;
        }

        public static Polynom Resultant(Polynom a, Polynom b)
        {
            List<Pair> polyList = new List<Pair>();
            Polynom tempA = LexOrder.CreateOrderedPolynom(new Polynom(Extensions.Clone(a.Monoms))), 
                    tempB = LexOrder.CreateOrderedPolynom(new Polynom(Extensions.Clone(b.Monoms)));
            var matrixArray = new Polynom[][] { };

            var b1 = LexOrder.CreateOrderedPolynom(b);
            var a1 = LexOrder.CreateOrderedPolynom(a);
            a1.Monoms.RemoveAll(x => x.Coef == 0);
            b1.Monoms.RemoveAll(x => x.Coef == 0);

            int max1 = FindMaxPower(a), max2 = FindMaxPower(b);

            //Сгруппированные выражения по степеням х
            List<Pair> polyList1 = GroupCoefficients(tempA),
                       polyList2 = GroupCoefficients(tempB);
            var filledPairs1 = FillPairList(polyList1);
            var filledPairs2 = FillPairList(polyList2);

            List<List<Polynom>> matrix = new List<List<Polynom>>();
            //Построение части матрицы
            int step = 0;
            for (var i = 0; i < max2; i++)
            {
                step = i;
                var row = new List<Polynom>(new Polynom[a1.LT.GetPower+b1.LT.GetPower]);

                row.RemoveRange(0, filledPairs1.Count);
                row.InsertRange(step, filledPairs1.Select(x => x.P));

                FillPolynom(row);
                matrix.Add(row);
                step++;
            }
            //Построение части матрицы
            step = 0;
            for (var i = 0; i < max1; i++)
            {
                step = i;
                var row = new List<Polynom>(new Polynom[a1.LT.GetPower + b1.LT.GetPower]);
                row.RemoveRange(0, filledPairs2.Count);
                row.InsertRange(step, filledPairs2.Select(x => x.P));

                FillPolynom(row);
                matrix.Add(row);
                step++;
            }
            
            Matrix m = new Matrix(matrix);
            var det = m.Determinant();
            
            return det;   
        }

        public static List<double> FindRoots(Polynom a)
        {
            List<double> roots = new List<double>();

            WolframAlphaNET.WolframAlpha w = new WolframAlphaNET.WolframAlpha("RAG9YE-E5PVQUEEKT");
            var res = w.Query(a.ToString() + " = 0");

            if (res != null)
            {
                var s = res.Pods[1].SubPods.ToList();
                foreach (var s1 in s)
                {
                    if (s1.Plaintext.Contains("x"))
                    {
                        var s2 = s1.Plaintext.Replace("x = ", "");
                        if (s2.Contains("/"))
                        {
                            roots.Add(FromFraction(s2));
                        }
                        else
                        {
                            roots.Add(Convert.ToDouble(s2));
                            var v = new PolyLib.Polynomial(-1, 1).Roots();
                        }
                    }
                }
            }
  
            return roots;
        }

        public static double FromFraction(string str)
        {
            string[] str1 = str.Split('/');
            double d = Convert.ToDouble(str1[0]) / Convert.ToDouble(str1[1]);
            return d;
        }

        public static void FillPolynom(List<Polynom> row)
        {
            for (var k = 0; k < row.Count; k++)
            {
                if (row[k] == null)
                    row[k] = new Polynom(new List<Monom> { new Monom(0) });
            }
        }

        public static List<Pair> GroupCoefficients(Polynom a)
        {
            List<Pair> polyList = new List<Pair>();
            Dictionary<int, Polynom> polyDict = new Dictionary<int, Polynom>();

            //разделение на слагаемые
            var subList = a.Monoms.FindAll(x => x.Powers.FindAll(y => y.Item1.Contains("x")) != null);
            
            //группировка по степеням
            foreach (var item in subList)
            {
                var pow = item.GetPower;
                if (!polyDict.Keys.ToList().Contains(pow))
                {
                    var p = new Polynom();
                    item.Powers.RemoveAll(x => x.Item1.Contains("x"));
                    p.Monoms.Add(item);
                    polyDict.Add(pow, p);
                }
                else
                {
                    item.Powers.RemoveAll(x => x.Item1.Contains("x"));
                    polyDict[pow].Monoms.Add(item);
                }
            }
            polyDict.OrderByDescending(x => x.Key);

            foreach (var kvp in polyDict)
            {
                polyList.Add(new Pair(kvp.Key, kvp.Value));
            }

            return polyList;
        }

        public static int FindMaxPower(Polynom a)
        {
            int max = 0;
            foreach (var m in a.Monoms)
            {
                if (m.Powers.Count > 0)
                {
                    int tempMax = m.GetPower;
                    if (tempMax > max)
                        max = tempMax;
                }
            }
            return max;
        }

        public static List<Pair> FillPairList(List<Pair> pair)
        {
            int maxPow = pair.Max(x => x.Power);
            for (var i = 0; i < maxPow; i++)
            {
                if (pair.Find(x => x.Power == i) == null)
                {
                    pair.Add(new Pair(i, new Polynom()));
                }
            }
            return pair.OrderByDescending(x => x.Power).ToList();
        }

        #region Операторы унарные/бинарные

        public static Polynom operator +(Polynom a, Polynom b)
        {
            Polynom res = new Polynom();
            res.Monoms.AddRange(a.Monoms);
            res.Monoms.AddRange(b.Monoms);
            res.SimplifyPolynom();
            return res;
        }

        public static Polynom operator -(Polynom p)
        {
            Polynom res = (Polynom)p.Clone();
            foreach (Monom t in res.Monoms)
            {
                t.Coef *= -1;
            }
            return res;
        }

        public static Polynom operator -(Polynom a, Polynom b)
        {
            Polynom res = new Polynom();
            res.Monoms.AddRange(a.Monoms);
            res.Monoms.AddRange((-b).Monoms);
            res.SimplifyPolynom();
            return res;
        }

        public static Polynom operator *(Polynom p, Monom m)
        {
            Polynom tempPoly = (Polynom)p.Clone();
            for (int i = 0; i < tempPoly.Monoms.Count; i++)
            {
                tempPoly.Monoms[i] *= m;
            }
            return tempPoly;
        }

        #endregion
    }
}
