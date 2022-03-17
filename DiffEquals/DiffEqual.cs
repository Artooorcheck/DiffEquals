using System;
using System.Collections.Generic;
using System.IO;

namespace DiffEquals
{

    enum State
    {
        OK,
        Error
    }

    class Point
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double h { get; private set; }
        public double Eps { get; private set; }

        private State state;

        public bool OnError
        {
            get
            {
                return state != State.Error;
            }
        }

        public string IER
        {
            get
            {
                switch (state)
                {
                    case State.OK:
                        return "Точность достигнута";
                    case State.Error:
                        return "Точность не достигнута";
                    default:
                        return "Неизвестная ошибка";
                }
            }
        }

        public Point(double x, double y, double h, double Eps, State state)
        {
            this.x = x;
            this.y = y;
            this.state = state;
            this.h = h;
            this.Eps = Eps;
        }
    }

    class DiffEqual
    {
        private List<Point> points;

        private int ier;

        public bool OnError
        {
            get
            {
                return ier != 0;
            }
        }

        public string IER
        {
            get
            {
                switch (ier)
                {
                    case 0:
                        return "Ошибок нет";
                    case 1:
                        return "Ошибка входных данных";
                    default:
                        return "Неизвестная ошибка";
                }
            }
        }

        double Infelicity(double ih, double ih2)
        {
            return Math.Abs((ih - ih2) / (0.25 - 1));
        }

        double Y(Func<double, double, double> f, double C, double Yc, double h)
        {
            var K1 = h * f(C, Yc);
            var K2 = h * f(C + h / 2, Yc + K1 / 2);
            var K3 = h * f(C + h / 2, Yc + K1 / 4 + K2 / 4);
            var K4 = h * f(C + h, Yc - K2 + 2 * K3);
            var K5 = h * f(C + 2.0 / 3.0 * h, Yc + 7.0 / 27.0 * K1 + 10.0 / 27.0 * K2 + 1.0 / 27.0 * K4);
            var K6 = h * f(C + h / 5, Yc - 1.0 / 625.0 * (28 * K1 - 125 * K2 + 546 * K3 + 54 * K4 + 378 * K5));
            return Yc + 1.0 / 336.0 * (14 * K1 + 35 * K4 + 162 * K5 + 125 * K6);
        }


        void Solve(Func<double, double, double> f, double A, double B, double y, double x, double h, double hmin, double Eps)
        {
            var startStep = h;

            while (x >= A && x <= B)
            {
                double newY = Y(f, x, y, h);
                double halfY = Y(f, x + h / 2, Y(f, x, y, h / 2), h / 2);
                double curEps = Infelicity(newY, halfY);
                while (curEps > Eps && Math.Abs(h) > Math.Abs(hmin))
                {
                    h /= 2;
                    if (Math.Abs(h) < Math.Abs(hmin))
                        h = (h >= 0) ? Math.Abs(hmin) : -Math.Abs(hmin);
                    newY = Y(f, x, y, h);
                    halfY = Y(f, x + h / 2, Y(f, x, y, h / 2), h / 2);
                    curEps = Infelicity(newY, halfY);
                }
                if (curEps <= Eps)
                {
                    points.Add(new Point(x, y, h, curEps, State.OK));
                }
                if (Math.Abs(h) == Math.Abs(hmin))
                {
                    points.Add(new Point(x, y, h, curEps, State.Error));
                }
                y = newY;
                x += h;
                if (curEps < Eps / 15&&startStep>h) h *= 2;
            }
        }

        private void Print(string res)
        {
            using (var file = new StreamWriter(res, false))
            {
                string result = "X\t\t\tY\t\t\tStep\t\tInfelicity\t\t\tState\n";

                foreach (var i in points)
                    result += $"{Math.Round(i.x, 3)}\t\t\t{Math.Round(i.y, 3)}\t\t\t{Math.Round(i.h, 3)}\t\t\t{Math.Round(i.Eps, 3)}\t\t\t{i.IER}\n";
                file.WriteLine(result);

            }

        }

        private void Read(string data, out double A, out double B, out double C, out double Yc, out double h, out double hmin, out double Eps)
        {
            A = 0; B = 0; C = 0; Yc = 0; h = 0; hmin = 0; Eps = 0;

            string[] paras;
            try
            {
                using (var file = new StreamReader(data))
                {
                    var fileString = file.ReadToEnd();
                    paras = fileString.Split(' ');
                    A = Double.Parse(paras[0]);
                    B = Double.Parse(paras[1]);
                    h = (B - A) / 10;
                    C = paras[2] == "l" ? A : B;
                    h = paras[2] == "l" ? h : -h;
                    Yc = Double.Parse(paras[3]);
                    hmin = Double.Parse(paras[4]);
                    Eps = Double.Parse(paras[5]);

                }
            }
            catch
            {
            }

        }

        public DiffEqual(string data, Func<double, double, double> f, string res)
        {
            Read(data, out double A, out double B, out double x, out double y, out double h, out double hmin, out double Eps);
            points = new List<Point>();
            if (!OnError)
                Solve(f, A, B, y, x, h, hmin, Eps);
            Print(res);
        }
    }
}
