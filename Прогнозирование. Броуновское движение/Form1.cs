using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Прогнозирование.Броуновское_движение.Classes;
using System.Windows.Forms.DataVisualization.Charting;

using System.Configuration;
using System.Collections.Specialized;

namespace Прогнозирование.Броуновское_движение
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        double[] data;
        Random rnd = new Random();
        SettingsCSV settings = new SettingsCSV();

        public void ZagryzkaInfiIzCSV(object a)
        {
            string fileName = (string)a;

            string[] linesCSV = File.ReadAllLines(fileName);
            List<string> listLinesCSV = linesCSV.ToList();

            int countData = linesCSV.Length - 1;
            bool firstRow_Header = settings.getBool("Первая строка исходного файла-заголовок");
            char splitRowValue = settings.getChar("Разделитель столбцов в исходном файле");
            int indexRowDateTime = settings.getInt("Номер столбца с датой");
            int indexRowData = settings.getInt("Номер значащего столбца данных");
            string splitDoubleValue = settings.getString("Разделитель целой и дробной части исходных данных");

            if (firstRow_Header)
            {
                listLinesCSV.RemoveAt(0);
                linesCSV = listLinesCSV.ToArray();
            }

            string[][] dataString = new string[countData][];  //Массив ячеек  //Старое
            string[] row;
            List<double> dataList = new List<double>();

            progressBar1.Invoke((Action)(() => { progressBar1.Maximum = countData; }));

            for (int i = 0; i < countData; i++)
            {
                row = linesCSV[i].Split(splitRowValue);
                row = new string[2]
                {
                        row[indexRowDateTime],
                        row[indexRowData]
                };

                if (row[1] == "")
                {
                    dataList.Add(0);
                }
                else
                {
                    dataList.Add(Convert.ToDouble(row[1].Replace(splitDoubleValue, ",")));
                }

                dataGridView1.Invoke((Action)(() => { dataGridView1.Rows.Add(row); })); //Добавление строки в таблицу
                chart1.Invoke((Action)(() => { chart1.Series[0].Points.AddY(dataList[i]); }));

                label1.Invoke((Action)(() => { label1.Text = "Загружено данных:" + (i + 1) + "/" + countData; }));
                progressBar1.Invoke((Action)(() => { progressBar1.Value = i; }));
            }

            countData = dataList.Count;
            data = new double[countData];

            for (int i = 0; i < countData; i++)
            {
                data[i] = dataList[i];
            }

            dataGridView4.Invoke((Action)(() =>
            {
                dataGridView4.Rows[0].Cells[1].Value = countData;
                dataGridView4.Rows[1].Cells[1].Value = countData;
            }));
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = ".csv";
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataGridView1.Rows.Clear();
                chart1.Series[0].Points.Clear();

                textBox1.Text = openFileDialog1.FileName;

                saveSettings();

                Task.Factory.StartNew(() => { ZagryzkaInfiIzCSV(openFileDialog1.FileName); }); //Создание и запуск нового потока
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("columns_3_headers", "");
            dataGridView2.Rows.Add(4);
            dataGridView2.Rows[0].Cells[0].Value = "Время";
            dataGridView2.Rows[1].Cells[0].Value = "Сред";
            dataGridView2.Rows[2].Cells[0].Value = "Мин";
            dataGridView2.Rows[3].Cells[0].Value = "Макс";

            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();
            chart2.Series.Clear();

            
            string dsa = ConfigurationManager.AppSettings.Get("Параметр1");
            ConfigurationManager.AppSettings.Set("Параметр1", "текст");
            

            int kolData = Convert.ToInt32(dataGridView4.Rows[1].Cells[1].Value);
            int kolShymov = Convert.ToInt32(dataGridView4.Rows[2].Cells[1].Value);
            int kolPprognozov = Convert.ToInt32(dataGridView4.Rows[3].Cells[1].Value);
            int kolLychihShymov = Convert.ToInt32(dataGridView4.Rows[4].Cells[1].Value);

            Algoritm(kolData, kolShymov, kolPprognozov, kolLychihShymov);
        }

        /// <summary>
        /// Броуновское движение
        /// </summary>
        /// <param name="kolData">Количество данных на обработку</param>
        /// <param name="kolShymov">Количество шумов</param>
        /// <param name="kolPprognozov">Количество прогнозов</param>
        /// <param name="kolLychihShymov">Количество лучших шумов</param>
        public void Algoritm(int kolData, int kolShymov, int kolPprognozov, int kolLychihShymov)
        {
            Statistika dataS = new Statistika();
            dataS.data = new double[kolData];

            //Выделение из всех исходных данных необходимого участка
            Series seriesData = new Series("Исходные данные");
            //chart2.Series.Add("Исходные данные");
            seriesData.ChartType = SeriesChartType.Line;
            seriesData.Color = Color.Red;
            seriesData.BorderWidth = 5;

            for (int i = 0; i < kolData; i++)
            {
                dataS.data[i] = data[data.Length - kolData + i];
                seriesData.Points.AddY(dataS.data[i]);
            }
            
            poshagovo(dataS.data, "Исходные данные");

            //Вычислить дисперсию, мат ожидание и корреляционную функцию для исходных данный
            dataS.compute_Statistiki();
            poshagovo(new double[2] { dataS.M, dataS.D }, "Мат ожидание, Дисперсия");
            poshagovo(dataS.korrellFunction, "Корреляционная функция исходных данных");
            poshagovo(dataS.normKorrellFunction, "Нормировання корреляционная функция исходных данных");


            //Вычисление приращения
            Statistika prirashenie = new Statistika();  //Приращение
            prirashenie.data = new double[kolData - 1];
            //double[] prirashenie = new double[countData - 1];  //Приращение
            double prirashenie0 = dataS.data[0];  //Первый элемент приращения (y = x[0] - 0)

            for (int i = 0; i < kolData - 1; i++)
            {
                prirashenie.data[i] = dataS.data[i + 1] - dataS.data[i];
            }

            poshagovo(prirashenie.data, "Приращения");

            //Вычислить дисперсию, мат ожидание и корреляционную функцию для приращения
            prirashenie.compute_Statistiki();
            poshagovo(new double[2] { prirashenie.M, prirashenie.D }, "Мат ожидание, Дисперсия");
            poshagovo(prirashenie.korrellFunction, "Корреляционная функция приращения");
            poshagovo(prirashenie.normKorrellFunction, "Нормированная корреляционная функция приращения");

            //ФГШ
            int kolDataPlusPrognoz = kolData + kolPprognozov;
            //double[][] modeliPrirasheniya = new double[kolShymov][];
            Statistika[] modeliPrirasheniya = new Statistika[kolShymov];

            Hurst hurst = new Hurst();
            double H;

            if (checkBox1.Checked)
            {
                H = hurst.computeHurst(prirashenie.data);
            }
            else
            {
                H = Convert.ToDouble(dataGridView4.Rows[5].Cells[1].Value);
            }

            FGN _FGN = new FGN();
            
            for (int i = 0; i < kolShymov; i++)
            {
                modeliPrirasheniya[i] = new Statistika();
                //Шум
                modeliPrirasheniya[i].data = _FGN.computeFGN(H, kolDataPlusPrognoz);

                //Нормировка
                for (int k = 0; k < kolDataPlusPrognoz; k++)
                {
                    modeliPrirasheniya[i].data[k] = modeliPrirasheniya[i].data[k] * prirashenie.sigma + prirashenie.M;
                }
            }

            poshagovo(modeliPrirasheniya[0].data, "ФГШ 1я выборка");
            poshagovo(modeliPrirasheniya[1].data, "ФГШ 2я выборка");
            poshagovo(modeliPrirasheniya[2].data, "ФГШ 3я выборка");

            //Агрегирование
            Statistika[] modeliIshodProcessa = new Statistika[kolShymov];

            for (int i = 0; i < kolShymov; i++)
            {
                modeliIshodProcessa[i] = new Statistika();
                modeliIshodProcessa[i].data = new double[kolDataPlusPrognoz];
                modeliIshodProcessa[i].data[0] = prirashenie0;

                for (int k = 1; k < kolDataPlusPrognoz; k++)
                {
                    modeliIshodProcessa[i].data[k] = modeliIshodProcessa[i].data[k - 1] + modeliPrirasheniya[i].data[k];
                }

                modeliIshodProcessa[i].compute_Ci_metrika(dataS.data);
            }

            poshagovo(modeliIshodProcessa[0].data, "Модель исходного процесса 1я выборка (после агрегирования)");
            poshagovo(modeliIshodProcessa[1].data, "Модель исходного процесса 2я выборка (после агрегирования)");
            poshagovo(modeliIshodProcessa[2].data, "Модель исходного процесса 3я выборка (после агрегирования)");

            //Сортировка массива по С-метрике по возрастанию
            for (int i = 0; i < kolShymov - 1; i++)
            {
                for (int k = i + 1; k < kolShymov; k++)
                {
                    if (modeliIshodProcessa[k].Ci_metrika < modeliIshodProcessa[i].Ci_metrika)
                    {
                        Statistika obmen = modeliIshodProcessa[i];
                        modeliIshodProcessa[i] = modeliIshodProcessa[k];
                        modeliIshodProcessa[k] = obmen;

                        obmen = modeliPrirasheniya[i];
                        modeliPrirasheniya[i] = modeliPrirasheniya[k];
                        modeliPrirasheniya[k] = obmen;
                    }
                }
            }

            poshagovo(new double[10] {
                modeliIshodProcessa[0].Ci_metrika,
                modeliIshodProcessa[1].Ci_metrika,
                modeliIshodProcessa[2].Ci_metrika,
                modeliIshodProcessa[3].Ci_metrika,
                modeliIshodProcessa[4].Ci_metrika,
                modeliIshodProcessa[5].Ci_metrika,
                modeliIshodProcessa[6].Ci_metrika,
                modeliIshodProcessa[7].Ci_metrika,
                modeliIshodProcessa[8].Ci_metrika,
                modeliIshodProcessa[9].Ci_metrika},
                "Первые 10 С-метрики после ранжирования");

            //Вывод на график точек смоделированного исходного процесса первых 10 моделей после ранжирования
            for (int i = 0; i < kolLychihShymov; i++)
            {
                Series series = chart2.Series.Add("МИП #" + (i + 1));  //МИП - модель исходного процесса
                series.ChartType = SeriesChartType.Point;
                series.Color = Color.Blue;
                series.MarkerStyle = MarkerStyle.Square;

                for (int k = 0; k < kolDataPlusPrognoz; k++)
                {
                    series.Points.AddY(modeliIshodProcessa[i].data[k]);
                }

                poshagovo(modeliIshodProcessa[i].data, "Модель исходного процесса " + (i + 1) + "я выборка (после ранжирования)");
                modeliIshodProcessa[i].compute_Statistiki();
                poshagovo(modeliIshodProcessa[i].normKorrellFunction, "Нормированная корреляционная функция модели исходного процесс " + (i + 1) + "я выборка (после ранжирования)");
                poshagovo(modeliPrirasheniya[i].data, "Модель приращения " + (i + 1) + "я выборка (после ранжирования)");
                modeliPrirasheniya[i].compute_Statistiki();
                poshagovo(modeliPrirasheniya[i].normKorrellFunction, "Нормированная корреляционная функция модели приращения " + (i + 1) + "я выборка (после ранжирования)");
            }

            chart2.Series.Add(seriesData);

            //Вычисление мин, макс, сред для прогнозов
            double[][] result = new double[kolPprognozov][];

            for (int i = 0; i < kolPprognozov; i++)
            {
                double min = modeliIshodProcessa[0].data[kolData + i];
                double max = modeliIshodProcessa[0].data[kolData + i];
                double summ = modeliIshodProcessa[0].data[kolData + i];

                for (int k = 1; k < kolLychihShymov; k++)
                {
                    if (modeliIshodProcessa[k].data[kolData + i] < min)
                    {
                        min = modeliIshodProcessa[k].data[kolData + i];
                    }

                    if (modeliIshodProcessa[k].data[kolData + i] > max)
                    {
                        max = modeliIshodProcessa[k].data[kolData + i];
                    }

                    summ += modeliIshodProcessa[k].data[kolData + i];
                }

                result[i] = new double[3];
                result[i][0] = summ / kolLychihShymov;
                result[i][1] = min;
                result[i][2] = max;
            }

            //Вывод рельтатов
            DateTime dateTime = DateTime.Parse(dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value.ToString());
            DateTime dateTimeEnd_minus1 = DateTime.Parse(dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[0].Value.ToString());
            TimeSpan timeSpan = dateTime - dateTimeEnd_minus1;

            string dateTimeFormat = settings.getString("Вид принимаемого формата даты");

            for (int i = 0; i < kolPprognozov; i++)
            {
                dataGridView2.Columns.Add("columns_2_" + i, "");

                dateTime += timeSpan;

                dataGridView2.Rows[0].Cells[i + 1].Value = dateTime.ToString("dd.MM.yyyy");
                dataGridView2.Rows[1].Cells[i + 1].Value = result[i][0].ToString("F" + 3);
                dataGridView2.Rows[2].Cells[i + 1].Value = result[i][1].ToString("F" + 3);
                dataGridView2.Rows[3].Cells[i + 1].Value = result[i][2].ToString("F" + 3);
            }
        }

        public void poshagovo(double[] row, string opisanieShaga)
        {
            if (!checkBox2.Checked)
            {
                return;
            }

            dataGridView3.Columns.Add("columns3_" + dataGridView3.Columns.Count, opisanieShaga);

            if (row.Length + 1 > dataGridView3.Rows.Count)
            {
                dataGridView3.Rows.Add(row.Length + 1 - dataGridView3.Rows.Count);
            }

            dataGridView3.Rows[0].Cells[dataGridView3.Columns.Count - 1].Value = opisanieShaga;


            for (int i = 0; i < row.Length; i++)
            {
                dataGridView3.Rows[i + 1].Cells[dataGridView3.Columns.Count - 1].Value = row[i];
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string splitRowValue = settings.getString("Разделитель столбцов в результирующем файле");
                string splitDoubleValue = settings.getString("Разделитель целой и дробной части результирующих данных");

                String text = "";

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    for (int k = 0; k < dataGridView2.Columns.Count; k++)
                    {
                        if (i > 0)
                        {
                            text += dataGridView2.Rows[i].Cells[k].Value.ToString().Replace(",", splitDoubleValue) + splitRowValue;
                        }
                        else
                        {
                            text += dataGridView2.Rows[i].Cells[k].Value + splitRowValue;
                        }
                    }

                    text += "\r\n";
                }

                File.WriteAllText(saveFileDialog1.FileName + ".csv", text, Encoding.UTF8);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                dataGridView4.Rows.Add(6);

                dataGridView4.Rows[0].Cells[0].Value = "Кол. данных всего";
                dataGridView4.Rows[1].Cells[0].Value = "Кол. данных на обработку";
                dataGridView4.Rows[2].Cells[0].Value = "Кол. шумов";
                dataGridView4.Rows[3].Cells[0].Value = "Кол. прогнозов";
                dataGridView4.Rows[4].Cells[0].Value = "Кол. лучших прогнозов";
                dataGridView4.Rows[5].Cells[0].Value = "Параметр Херста";

                //dataGridView4.Rows[0].Cells[1].Value = 1000;
                //dataGridView4.Rows[1].Cells[1].Value = 200;
                dataGridView4.Rows[2].Cells[1].Value = settings.getString("Кол. шумов");
                dataGridView4.Rows[3].Cells[1].Value = settings.getString("Кол. прогнозов");
                dataGridView4.Rows[4].Cells[1].Value = settings.getString("Кол. лучших прогнозов");
                dataGridView4.Rows[5].Cells[1].Value = settings.getString("Параметр Херста");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace);
            }
        }

        private void saveSettings()
        {
            settings.set("Кол. шумов", dataGridView4.Rows[2].Cells[1].Value.ToString());
            settings.set("Кол. прогнозов", dataGridView4.Rows[3].Cells[1].Value.ToString());
            settings.set("Кол. лучших прогнозов", dataGridView4.Rows[4].Cells[1].Value.ToString());
            settings.set("Параметр Херста", dataGridView4.Rows[5].Cells[1].Value.ToString());
            settings.saveSettings();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveSettings();
        }
    }
}
