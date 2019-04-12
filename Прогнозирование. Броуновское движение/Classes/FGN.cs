using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Прогнозирование.Броуновское_движение
{
    class FGN
    {
        Random rnd = new Random();

        /// <summary>
        /// Вычислить ФГШ
        /// </summary>
        /// <param name="H">Коэффециент Хёрста</param>
        /// <returns></returns>
        public double[] computeFGN(double H, int n)
        {
            int n_2 = n / 2;

            //Шаг 1:
            double[] pow_spec = fgn_spectrum(H, n_2);

            //Шаг 2: Корректировка используя периодограмму
            double[] zz = new double[n_2 + 1];

            for (int i = 0; i < n_2; i++)
            {
                zz[i] = pow_spec[i] * genexp(1);
            }

            //Шаг 3:
            double[] z = new double[2 * (n_2 + 1)];

            for (int i = 0; i < n_2; i++)
            {
                z[2 * i] = Math.Sqrt(zz[i]);
                z[2 * i + 1] = 2 * Math.PI * rnd.NextDouble();
            }

            z[2 * n_2 + 1] = 0;

            double[] z_e = new double[2 * (n + 1)];

            z_e[0] = 0;
            z_e[1] = 0;

            for (int i = 0; i < n_2; i++)
            {
                z_e[2 * i] = z[2 * i];
                z_e[2 * i + 1] = z[2 * i + 1];
            }

            for (int i = n_2 + 1; i < n - 1; i++)
            {
                z_e[2 * i] = z[2 * (n - i)];
                z_e[2 * i + 1] = -z[2 * (n - i) + 1];
            }

            double re;
            double im;

            for (int i = 0; i < n - 1; i++)
            {
                re = z_e[2 * i] * Math.Cos(z_e[2 * i + 1]);
                im = z_e[2 * i] * Math.Sin(z_e[2 * i + 1]);
                z_e[2 * i] = re;
                z_e[2 * i + 1] = im;
            }

            //Шаг 5:
            fft_rif(z_e, n, -1);

            double[] result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = z_e[i * 2] * 1000;
            }

            return result;
        }

        private void fft_rif(double[] data, int nn, int isign)
        {
            for (int i = 2*nn; i > 0; i--)
            {
                data[i] = data[i - 1];
            }

            data[2] = 0;

            int n = nn * 2;
            int j = 1;
            double tempr1;
            int m;

            for (int i = 0; i < n; i+=2)
            {
                if (j > i)
                {
                    tempr1 = data[j];
                    data[j] = data[i];
                    data[i] = tempr1;

                    tempr1 = data[j + 1];
                    data[j + 1] = data[i + 1];
                    data[i + 1] = tempr1;
                }

                m = n / 2;

                while ((m >= 2) && (j > m))
                {
                    j = j - m;
                    m /= 2;
                }

                j += m;
            }

            int mmax = 2;
            int istep;
            double theta;
            double wtemp;
            double wpr;
            double wpi;
            double wr;
            double wi;
            double tempr;
            double tempi;


            while (n > mmax)
            {
                istep = 2 * mmax;
                theta = -6.28318530717959 / (isign * mmax);
                wtemp= Math.Sin(0.5 * theta);
                wpr= -2.0 * wtemp * wtemp;
                wpi= Math.Sin(theta);
                wr= 1.0;
                wi= 0.0;
                m = 1;

                while (m < mmax)
                {
                    int i = m;

                    while (i < n - mmax)
                    {
                        j = i + mmax;
                        tempr = wr * data[j] - wi * data[j + 1];
                        tempi = wr * data[j + 1] + wi * data[j];
                        data[j] = data[i] - tempr;
                        data[j + 1] = data[i + 1] - tempi;
                        data[i] = data[i] + tempr;
                        data[i + 1] = data[i + 1] + tempi;
                        i = i + istep;
                    }

                    wtemp = wr;
                    wr = wtemp * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                    m = m + 2;
                }

                mmax = istep;
            }

            for (int i = 0; i < 2*nn; i++)
            {
                data[i] = data[i + 1];
            }
        }

        private double[] fgn_spectrum(double h, int n)
        {
            double[] result = new double[n];

            double lambda;
            double fact1;
            double a;
            double b;
            double c;

            double g = 1 / (2 * h + 1);

            for (int nl = 1; nl <= 3000; nl++)
            {
                g *= Math.Pow(1 + 1 / nl, 2 * h + 1) / (1 + (2 * h + 1) / nl);
            }

            fact1 = 2 * Math.Sin(Math.PI * h) * g;

            for (int i = 0; i < n; i++)
            {
                lambda = Math.PI * (i + 1) / n;
                a = fact1 * (1 - Math.Cos(lambda));
                b = Math.Pow(lambda, -2 * h - 1);
                c = fgn_b_est(lambda, h);
                result[i] = a * (b + c);
            }

            return result;
        }

        private double fgn_b_est(double lambda, double h)
        {
            double[] a = new double[5];
            double[] b = new double[5];

            double d = -2 * h - 1;
            double dprime = -2 * h;

            for (int k = 0; k < 4; k++)
            {
                a[k] = 2 * (k + 1) * Math.PI + lambda;
                b[k] = 2 * (k + 1) * Math.PI - lambda;
            }

            double sum1 = 0;

            for (int k = 0; k < 3; k++)
            {
                sum1 += Math.Pow(a[k], d);
                sum1 += Math.Pow(b[k], d);
            }

            double sum2 = 0;

            for (int k = 2; k < 4; k++)
            {
                sum2 += Math.Pow(a[k], dprime);
                sum2 += Math.Pow(b[k], dprime);
            }

            return sum1 + (sum2 / (8 * Math.PI * h));
        }

        private double genexp(double av)
        {
            return sexpo() * av;
        }

        private double sexpo()  //Вроде правильно
        {
            double[] q = new double[8];
            q[0] = 0.6931472;
            q[1] = 0.9333737;
            q[2] = 0.9888778;
            q[3] = 0.9984959;
            q[4] = 0.9998293;
            q[5] = 0.9999833;
            q[6] = 0.9999986;
            q[7] = 1.0;

            double a = 0;
            double u = rnd.NextDouble();
            double ustar;
            double umin;

            int i;

            u *= 2;

            while (u <= 1)
            {
                a += q[0];
                u *= 2;
            }

            u -= 1;

            if (!(q[0] < u))
            {
                return a + u;
            }

            i = 0;
            ustar = rnd.NextDouble();
            umin = ustar;
            ustar = rnd.NextDouble();

            if (ustar < umin)
            {
                umin = ustar;
            }

            while (u > q[i])
            {
                ustar = rnd.NextDouble();

                if (ustar < umin)
                {
                    umin = ustar;
                }

                i++;
            }

            return a + umin * q[0];
        }
    }
}
