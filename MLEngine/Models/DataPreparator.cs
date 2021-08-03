using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace MLEngine.Models
{
    class DataPreparator
    {
        string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public string SplitParts { get; set; }
        public bool UseShufle { get; set; }

        private string fname_train;
        private string fname_test;
        private string fname_valid;
        
        private string baseFileName;
        public string BaseFileName { 
            set {
                baseFileName = value;
                fname_train = GetNewFileName(baseFileName, "-train.csv");
                fname_test = GetNewFileName(baseFileName, "-test.csv");
                fname_valid = GetNewFileName(baseFileName, "-valid.csv");
            } }

        private string GetNewFileName(string baseFileName, string postFix)
        {
            List<string> words = baseFileName.Split('\\').ToList();
            string name = words.Last().Split('.').First() + postFix;
            words.Reverse();
            words.Remove(words.First());
            words.Reverse();
            words.Add(name);
            return string.Join("\\", words); 
        }

        public void MakeValidData()
        {
            RestructData(baseFileName, fname_train, fname_test, fname_valid, UseShufle, SplitParts);
        }

        private void RestructData(string fname_data, string fname_train, string fname_test, string fname_valid, bool useShufle, string splitParts)
        {
            try
            {
                CreateSatelliteFiles(fname_train);
                CreateSatelliteFiles(fname_test);
                CreateSatelliteFiles(fname_valid);

                List<string> fileContents = File.ReadAllLines(fname_data).ToList();
                
                for (int i = 0; i < fileContents.Count; i++)
                {
                    fileContents[i] = fileContents[i].Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator).Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                }

                List<string> shuffleContent = new List<string>();

                if (useShufle)
                    shuffleContent = Shuffle(fileContents);
                else
                    shuffleContent = fileContents;


                int splitPoint = Convert.ToInt32(Math.Round(Convert.ToDouble(shuffleContent.Count()) * Convert.ToDouble(splitParts.Replace(".", sep).Replace(",", sep)), 0));

                List<string> trainData = shuffleContent.GetRange(0, shuffleContent.Count() - splitPoint).ToList();
                File.AppendAllLines(fname_train, trainData);
                trainData.Clear();

                List<string> validData = shuffleContent.Skip(fileContents.Count() - splitPoint).ToList();
                validData = validData.GetRange(0, validData.Count() / 2);
                File.AppendAllLines(fname_valid, validData);
                validData.Clear();

                List<string> testData = shuffleContent.Skip(fileContents.Count() - splitPoint).ToList();
                testData = testData.GetRange(testData.Count() / 2, testData.Count() / 2 - 1);
                File.AppendAllLines(fname_test, testData);
                testData.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
               
        }

        private List<string> Shuffle(List<string> list)
        {
            Random rand = new Random();

            for (int i = list.Count - 1; i >= 1; i--)
            {
                int j = rand.Next(i + 1);

                string tmp = list[j];
                list[j] = list[i];
                list[i] = tmp;
            }

            return list;
        }

        void CreateSatelliteFiles(string filename)
        {
            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }
            else if (File.Exists(filename))
            {
                File.Delete(filename);
                File.Create(filename).Close();
            }
        }
    }
}
