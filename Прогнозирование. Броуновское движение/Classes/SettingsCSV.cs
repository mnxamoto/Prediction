using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Прогнозирование.Броуновское_движение.Classes
{
    class SettingsCSV
    {
        /// <summary>
        /// Лист параметров
        /// </summary>
        public List<string> settings = new List<string>();
        /// <summary>
        /// Лист названий параметров
        /// </summary>
        public List<string> nameSettings = new List<string>();
        /// <summary>
        /// Путь к файлу с настройками
        /// </summary>
        public string fileName = Environment.CurrentDirectory + "\\Settings.csv";

        public SettingsCSV()
        {
            if (File.Exists(fileName))
            {
                string[] linesCSV = File.ReadAllLines(fileName,Encoding.UTF8);

                for (int i = 0; i < linesCSV.Length; i++)
                {
                    string[] row = linesCSV[i].Split(new char[1] { ';' }, 2);
                    nameSettings.Add(row[0]);
                    settings.Add(row[1]);
                }
            }
            else
            {
                throw new Exception("Файл настроек (Settings.csv) не найден");
            }
        }

        public void saveSettings()
        {
            String text = "";

            for (int i = 0; i < settings.Count; i++)
            {
                text += nameSettings[i] + ";" + settings[i] + "\r\n";
            }

            File.WriteAllText(fileName, text, Encoding.UTF8);
        }

        public void set(string name, string value)
        {
            settings[getIndex(name)] = value;
        }

        public void set(string name, int value)
        {
            set(name, value.ToString());
        }

        public void set(string name, double value)
        {
            set(name, value.ToString());
        }

        private int getIndex(string name)
        {
            return nameSettings.IndexOf(name);
        }

        public string getString(string name)
        {
            return settings[getIndex(name)];
        }

        public double getDouble(string name)
        {
            return Convert.ToDouble(getString(name));
        }

        public int getInt(string name)
        {
            return Convert.ToInt32(getString(name));
        }

        public char getChar(string name)
        {
            return Convert.ToChar(getString(name));
        }

        public bool getBool(string name)
        {
            if (getString(name) == "да")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
