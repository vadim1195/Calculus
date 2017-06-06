﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Symbolic.Model.Polynomial
{
    /// <summary>
    /// Класс Моном
    /// </summary>
    class Monom : IComparable<Monom>, ICloneable
    {
        /// <summary>
        /// коэффициент и список степеней
        /// </summary>
        private double _coef;
        public List<Tuple<string, int>> Powers;

        #region Свойства

        public double Coef
        {
            get { return _coef; }
            set { _coef = value; }
        }

        public int Degree
        {
            get
            {
                int sum = 0;
                for (var i = 0; i < Powers.Count; i++)
                {
                    sum += Powers[i].Item2;
                }

                return sum;
            }
        }

        public int GetPower
        {
            get
            {
                if (this.Powers.Find(x => x.Item1.Contains("x")) == null)
                {
                    this.Powers.Add(new Tuple<string, int>("x", 0));
                }
                var power = Powers.Find(x=>x.Item1.Contains("x"));
                    return power.Item2;
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Пустой
        /// </summary>
        public Monom()
        {
            _coef = 1;
            Powers = new List<Tuple<string, int>>();
        }

        /// <summary>
        /// Свободный член
        /// </summary>
        /// <param name="k"> Коэффициент </param>
        public Monom(double k)
        {
            _coef = k;
            Powers = new List<Tuple<string, int>>();
        }

        /// <summary>
        /// Моном + коэффициент
        /// </summary>
        /// <param name="k"> Коэффициент </param>
        /// <param name="pow"> Список степеней </param>
        public Monom(double k, List<Tuple<string, int>> pow)
        {
            _coef = k;
            Powers = pow;
        }

        #endregion

        ///<summary> 
        ///Копия объекта "Моном" 
        ///</summary>
        public object Clone()
        {
            var newList = new List<Tuple<string, int>>();

            this.Powers.ForEach((item) =>
            {
                newList.Add(item);
            });
            return new Monom
            {
                Coef = this.Coef,
                Powers = new List<Tuple<string, int>>(newList)
            };
        }

        ///<summary>
        ///Сравнение мономов 
        ///</summary>
        ///<param name="b"> Аргумент - моном, с которым сравниваем </param>
        ///<returns> 1- сравниваемый больше, 0 - равны, -1 - сравниваемый меньше </returns>
        public virtual int CompareTo(Monom b)
        {
            int compared = 0;
            Monom tempMonom = new Monom();

            //если сравниваем с тем же
            if (Powers == b.Powers)
                return 0;

            //Количество переменных в мономе
            if (Powers.Count > b.Powers.Count)
                b.CompleteMonom(this);
            else if (Powers.Count < b.Powers.Count)
                CompleteMonom(b);
            if (Powers.Count == b.Powers.Count)
            {
                tempMonom = this;
                for (var i = 0; i < tempMonom.Powers.Count; i++)
                {
                    if (String.Compare(Powers[i].Item1, b.Powers[i].Item1, true) == 0)
                    {
                        if (Powers[i].Item2 > b.Powers[i].Item2)
                        {
                            //первый больше
                            compared = 1;
                            break;
                        }
                        else if (Powers[i].Item2 < b.Powers[i].Item2)
                        {
                            //второй больше
                            compared = -1;
                            break;
                        }
                        else if (Powers[i] == b.Powers[i])
                            continue;
                    }
                    if (String.Compare(Powers[i].Item1, b.Powers[i].Item1, true) < 0)
                    {
                        compared = 1;
                        break;
                    }
                    if (String.Compare(Powers[i].Item1, b.Powers[i].Item1, true) > 0)
                    {
                        compared = -1;
                        break;
                    }
                }
            }
            return compared;
        }

        /// <summary>
        /// Get monoms LCM
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Monom GetLCM(Monom a, Monom b)
        {
            var lcm = new Monom();

            //Количество переменных в мономе
            if (a.Powers.Count > b.Powers.Count)
                b.CompleteMonom(b);
            else if (a.Powers.Count < b.Powers.Count)
                a.CompleteMonom(a);
            if (a.Powers.Count == b.Powers.Count)
            {
                for (var i = 0; i < a.Powers.Count; i++)
                {
                    lcm.Powers.Add(a.Powers[i].Item2 > b.Powers[i].Item2 ? a.Powers[i] : b.Powers[i]);
                }
            }
            return lcm;
        }

        ///<summary> 
        ///Дополнение монома переменными 0-й степени 
        ///</summary>
        ///<param name="compared">  Дополнение на столько же переменных, сколько у него </param>
        ///<returns> Дополненный моном </returns>
        public Monom CompleteMonom(Monom compared)
        {
            if (Powers.Count < compared.Powers.Count)
            {
                while (Powers.Count < compared.Powers.Count)
                    Powers.Add(new Tuple<string, int>("x" /*+ ((Powers.Count)+1).ToString()*/, 0));
            }
            else if (Powers.Count > compared.Powers.Count)
            {
                while (Powers.Count > compared.Powers.Count)
                    compared.Powers.Add(new Tuple<string, int>("x" /*+ ((Powers.Count)+1).ToString()*/, 0));
            }

            return this;
        }

        /// <summary>
        /// Проверка мономов на делимость
        /// </summary>
        /// <param name="a">Делимое</param>
        /// <param name="b">Делитель</param>
        /// <returns> да/нет </returns>
        public static bool CanDivide(Monom a, Monom b)
        {
            var canDivide = true;

            if (b._coef == 0)
                canDivide = false;
            else
            {
                if (a.Powers.Count > b.Powers.Count)
                {
                    b.CompleteMonom(a);
                    if (a.Powers.Where((t, i) => t.Item2 < b.Powers[i].Item2).Any())
                    {
                        canDivide = false;
                    }
                }
            }

            return canDivide;
        }

        public Monom OrderVariables()
        {
            Monom temp = this;
            temp.Powers = new List<Tuple<string, int>>(temp.Powers.OrderBy(x => x.Item1));

            return temp;
        }

        /// <summary>
        /// Строковое представление монома
        /// </summary>
        /// <returns> Строковое представление </returns>
        public override string ToString()
        {
            string toShow = "";

            for (int i = 0; i < Powers.Count; i++)
            {
                if (Powers[i].Item2 != 0)
                    toShow += Math.Abs(Powers[i].Item2) > 1
                        ? "*" + Powers[i].Item1 + "^" + (Powers[i].Item2)
                        : "*" + Powers[i].Item1;
            }
            return $"{toShow.Insert(0, Coef.ToString("0.###;-0.###;0"))}";
        }

        /// <summary>
        /// Проверка мономов на подобие
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns> подобны/нет </returns>
        public static bool AreEqual(Monom a, Monom b)
        {
            bool areEqual = false;
            if (a.Powers.Count == b.Powers.Count)
            {
                if (a.Powers.SequenceEqual(b.Powers))
                {
                    areEqual = true;
                }
            }
            return areEqual;
        }

        #region Операторы унарные/бинарные

        public static Monom operator +(Monom a, Monom b)
        {
            Monom sumMonom = null;
            if (AreEqual(a, b))
            {
                sumMonom = new Monom(a._coef + b._coef, a.Powers);
            }
            return sumMonom;
        }

        public static Monom operator -(Monom a, Monom b)
        {
            Monom subMonom = null;
            if (AreEqual(a, b))
            {
                subMonom = new Monom(a._coef - b._coef, a.Powers);
            }
            return subMonom;
        }

        public static Monom operator -(Monom m)
        {
            var tempMonom = m;
            tempMonom._coef *= -1;
            return tempMonom;
        }

        public static Monom operator *(Monom a, Monom b)
        {
            var multMonom = new Monom {_coef = a._coef * b._coef};

            if (a.Powers.Count > b.Powers.Count) b.CompleteMonom(a);
            else if (a.Powers.Count < b.Powers.Count) a.CompleteMonom(b);

            for (var i = 0; i < a.Powers.Count; i++)
            {
                if (a.Powers[i].Item1 == b.Powers[i].Item1)
                    multMonom.Powers.Add(new Tuple<string, int>(a.Powers[i].Item1, a.Powers[i].Item2 + b.Powers[i].Item2));
                else
                {
                    multMonom.Powers.Add(new Tuple<string, int>(a.Powers[i].Item1, a.Powers[i].Item2));
                    multMonom.Powers.Add(new Tuple<string, int>(b.Powers[i].Item1, b.Powers[i].Item2));
                }
            }
            return multMonom;
        }

        public static Monom operator /(Monom a, Monom b)
        {
            var divMonom = new Monom();

            if (CanDivide(a, b))
            {
                divMonom._coef = a._coef / b._coef;
                for (var i = 0; i < a.Powers.Count; i++)
                {
                    divMonom.Powers.Add(new Tuple<string, int>(a.Powers[i].Item1, a.Powers[i].Item2 - b.Powers[i].Item2));
                }
            }
            return divMonom;
        }

        #endregion
    }

}
