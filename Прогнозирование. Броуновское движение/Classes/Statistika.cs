using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Прогнозирование.Броуновское_движение.Classes
{
    /// <summary>
    /// Класс со статистическими свойствами и функциями вычисления этих свойств
    /// </summary>
    class Statistika
    {
        /// <summary>
        /// Данные
        /// </summary>
        public double[] data;
        /// <summary>
        /// Мат.ожидание
        /// </summary>
        public double M;
        /// <summary>
        /// Дисперсия
        /// </summary>
        public double D;
        /// <summary>
        /// Корреляционная функция
        /// </summary>
        public double[] korrellFunction;
        /// <summary>
        /// Нормированная корреляционная функция
        /// </summary>
        public double[] normKorrellFunction;
        /// <summary>
        /// Количество данных
        /// </summary>
        public int n;
        /// <summary>
        /// Си-метрика
        /// </summary>
        public double Ci_metrika;
        /// <summary>
        /// Квадратный корень из дисперсии
        /// </summary>
        public double sigma;

        /// <summary>
        /// Задаём готовый набор данных и сразу вычисляем все статистические свойства
        /// </summary>
        /// <param name="data">Готовый набор данных</param>
        public Statistika(double[] data)
        {
            this.data = data;
            compute_Statistiki();
        }

        /// <summary>
        /// Конструктор по умолчанию. Просто создаёт пустой экземпляр класса. Ни чего более.
        /// </summary>
        public Statistika()
        {

        }

        /// <summary>
        /// Вызывается только после передачи ранее набора. Вычисляем все статистические свойства
        /// </summary>
        public void compute_Statistiki()
        {
            compute_normKorellFunction();  //Вычисляет sigma (Вычисляет D, M и n)
        }

        public void compute_sigma()
        {
            compute_D();

            sigma = Math.Sqrt(D);
        }

        /// <summary>
        /// Вычисление Си-метрики
        /// </summary>
        /// <param name="data2">Набор данных, с которым будет происходить сравнение</param>
        public void compute_Ci_metrika(double[] data2)
        {
            Ci_metrika = Math.Abs(data[0] - data2[0]);

            for (int i = 0; i < data2.Length; i++)
            {
                double tekyshee_otklonenie = Math.Abs(data[i] - data2[i]);

                if (tekyshee_otklonenie > Ci_metrika)
                {
                    Ci_metrika = tekyshee_otklonenie;
                }
            }
        }

        /// <summary>
        /// Вычислить корреляционную функцию
        /// </summary>
        private void compute_korellFunction()
        {
            korrellFunction = new double[data.Length];

            for (int i = 0; i < n; i++)
            {
                double summ = 0;

                for (int k = 0; k < data.Length - i; k++)
                {
                    summ += (data[k] - M) * (data[k + i] - M);
                }

                korrellFunction[i] = summ / n;
            }
        }

        /// <summary>
        /// Вычислить корреляционную функцию и нормированную корреляционную функцию
        /// https://studme.org/290395/matematika_himiya_fizik/normirovannaya_korrelyatsionnaya_funktsiya
        /// </summary>
        private void compute_normKorellFunction()
        {
            compute_sigma();

            normKorrellFunction = new double[data.Length];
            korrellFunction = new double[data.Length];

            for (int i = 0; i < n; i++)
            {
                double summ = 0;
                Statistika t2 = new Statistika();
                t2.data = new double[n - i];

                for (int k = 0; k < n - i; k++)
                {
                    summ += (data[k] - M) * (data[k + i] - M);

                    t2.data[k] = data[k + i];
                }

                korrellFunction[i] = summ / n;
                //Нормировка
                t2.compute_sigma();
                normKorrellFunction[i] = korrellFunction[i] / (sigma * t2.sigma);
            }
        }

        /// <summary>
        /// Вычислить дисперсию
        /// </summary>
        public void compute_D()
        {
            compute_M();
            n = data.Length;

            double summ = 0;

            for (int i = 0; i < n; i++)
            {
                summ += Math.Pow(data[i] - M, 2);
            }

            D = summ / n;
        }

        /// <summary>
        /// Вычислить мат. ожидание
        /// </summary>
        public void compute_M()
        {
            M = data.Average();
        }
    }
}
