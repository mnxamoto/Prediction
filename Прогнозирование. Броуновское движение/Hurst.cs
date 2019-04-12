using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Прогнозирование.Броуновское_движение
{
    class Hurst
    {
        /// <summary>
        /// Вычислить коэффециент Хёрста
        /// </summary>
        /// <param name="data">Набор данных, по которому будет произведено вычисление</param>
        /// <returns></returns>
        public double computeHurst(double[] data)
        {
            int m_min = 10; 
            int m_max = 100; 
            int n = data.Length;

            double m_mean = 0;

            for (int m = m_min; m < m_max; m++)
            {
                m_mean += Math.Log10(m);
            }

            m_mean /= (m_max - m_min + 1);

            double rs_sum = 0;
            double a1u = 0;
            double a1d = 0;

            for (int m = m_min; m < m_max; m++)
            {
                double rs = 0;

                for (int i = 0; i < Convert.ToInt32(n / m - 2); i++)
                {
                    double me = 0;
                    double s = 0;
                    double max = 0;
                    double min = 0;
                    double delta = 0;
                    int jbeg = i * m;
                    int jmax = i * m + m - 1;

                    for (int j = jbeg; j < jmax; j++)
                    {
                        me += data[j];
                    }

                    me /= m;

                    for (int j = jbeg; j < jmax; j++)
                    {
                        s += Math.Sqrt(Math.Abs(data[j] - me));
                        delta += data[j] - me;

                        if (delta > max)
                        {
                            max = delta;
                        }

                        if (delta < min)
                        {
                            min = delta;
                        }
                    }

                    s = Math.Sqrt(s / m);
                    double r = max - min;
                    rs += r / s;
                }

                rs /= Math.Round(n / (double)m - 2) + 1;
                rs_sum += Math.Log10(rs);
                a1u += (Math.Log10(m) - m_mean) * Math.Log10(rs);
                a1d += Math.Sqrt(Math.Log10(m) - m_mean);
                //series7.AddXY(log10(m), log10(rs))  //График (x,y) 
            }

            rs_sum /= m_max - m_min + 1;

            double Hurst = a1u / a1d;  //Хёрст

            return Hurst;
        }
        
        /// <summary>
        /// Вычислить коэффециент Хёрста (старый и не рабочий)
        /// </summary>
        /// <param name="data">Набор данных, по которому будет произведено вычисление</param>
        /// <returns></returns>
        public double computeHurstOld(double[] data)
        {
            int N = data.Length;

            double[] x = new double[N - 1];
            double[] y = new double[N - 1];

            for (int n = 1; n < N; n++)
            {
                double[] nData = new double[n + 1];

                for (int i = 0; i < n + 1; i++)
                {
                    nData[i] = data[i];
                }

                x[n - 1] = Math.Log(n + 1);
                y[n - 1] = Math.Log(RS(nData));
            }

            double H = MNK(x, y);

            return H;
        }

        /// <summary>
        /// МНК - метод наименьших квадратов
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private double MNK(double[] x, double[] y)
        {
            int N = x.Length;

            double c1 = 0;
            double c2 = 0;
            double g1 = 0;
            double g2 = 0;

            for (int i = 0; i < N; i++)
            {
                c1 += x[i] * x[i];
                c1 += x[i];
                g1 += x[i] * y[i];
                g2 += y[i];
            }

            double H = (N * g1 - c2 * g2) / (N * c1 - c2 * c2);

            return H;
        }

        /// <summary>
        /// Вычислить значение RS
        /// </summary>
        /// <param name="data">Текущая выборка</param>
        /// <returns></returns>
        private double RS(double[] data)
        {
            int n = data.Length;

            double[] h = new double[n - 1];

            for (int i = 0; i < n - 1; i++)
            {
                h[i] = Math.Log(data[i + 1] / data[i]);
            }

            double h_average = h.Average();

            double R = this.R(h, h_average);
            double S = this.S(h, h_average);

            double RS = R / S;

            return RS;
        }

        /// <summary>
        /// Вычисление R
        /// </summary>
        /// <param name="h">Логарифмическая доходность</param>
        /// <param name="h_average">Среднее арефметическое от массива h</param>
        /// <returns></returns>
        private double R(double[] h, double h_average)
        {
            double[] summ = new double[h.Length];

            for (int k = 0; k < h.Length; k++)
            {
                summ[k] = 0;

                for (int i = 0; i <= k; i++)
                {
                    summ[k] += h[i] - h_average;
                }
            }

            double R = summ.Max() - summ.Min();

            return R;
        }

        /// <summary>
        /// Вычисление S
        /// </summary>
        /// <param name="h">Логарифмическая доходность</param>
        /// <param name="h_average">Среднее арефметическое от массива h</param>
        /// <returns></returns>
        private double S(double[] h, double h_average)
        {
            double summ = 0;

            for (int i = 0; i < h.Length; i++)
            {
                summ += Math.Pow(h[i] - h_average, 2);
            }

            double S = Math.Sqrt(summ / h.Length);

            return S;
        }
    }
}
