using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace aGame._2019
{
    class Program
    {
        static void Main(string[] args)
        {
            int sampleRate = 180 + 1;//The 1 is because of headers
            Complex32[] samples = new Complex32[sampleRate];
            Complex32[] originalSamples = new Complex32[sampleRate];
            Complex32[] inverseSamples = new Complex32[sampleRate];
            string[] lines = Properties.Resources.CustomerDataTimeSeries.Split(new char[] { '\r', '\n' },StringSplitOptions.RemoveEmptyEntries);
            Dictionary<DateTime, float> parsedData = new Dictionary<DateTime, float>();
            DateTime[] sameDateTimes = new DateTime[sampleRate];
            //----
            IOrderedEnumerable<KeyValuePair<DateTime, float>> orderD = getDataSortedByDate(sampleRate, lines, parsedData);
            int j = 0;
            foreach (KeyValuePair<DateTime, float> keyValuePair in orderD)
            {
                float sum = 0;
                float valueToPut = keyValuePair.Value + sum;
                samples[j] = new Complex32(valueToPut, 0);
                originalSamples[j] = new Complex32(valueToPut, 0);
                sameDateTimes[j] = keyValuePair.Key;
                j++;
            }
            Fourier.Forward(samples, FourierOptions.Matlab);
            applyFilter(sampleRate, samples, inverseSamples);
            Fourier.Inverse(inverseSamples, FourierOptions.Matlab);
            saveDataAsNewCSV(samples, originalSamples, inverseSamples, sameDateTimes);
        }

        private static void saveDataAsNewCSV(Complex32[] samples, Complex32[] originalSamples, Complex32[] inverseSamples, DateTime[] sameDateTimes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < samples.Length; i++)
            {
                sb.AppendLine(i + "," + originalSamples[i].Real + "," + samples[i].Magnitude + "," + inverseSamples[i].Real + "," + sameDateTimes[i]);
            }
            File.WriteAllText("fft.csv", sb.ToString());
        }

        private static void applyFilter(int sampleRate, Complex32[] samples, Complex32[] inverseSamples)
        {
            for (int i = 0; i < sampleRate; i++)
            {
                if ((Math.Abs(i - 26) > 4) && (i > 7))
                {
                    inverseSamples[i] = new Complex32(0, 0);
                }
                else
                {
                    inverseSamples[i] = new Complex32(samples[i].Real, samples[i].Imaginary);
                }
            }
        }

        private static IOrderedEnumerable<KeyValuePair<DateTime, float>> getDataSortedByDate(int sampleRate, string[] lines, Dictionary<DateTime, float> parsedData)
        {
            for (int i = 1; i < sampleRate + 1; i++)
            {
                string[] items = lines[i].Split(new char[] { ',' , '\t'});
                string year = items[0];
                string month = items[1];
                int numberOfMonth = resolveMonth(month);
                string day = items[2];
                DateTime currentDate = new DateTime(int.Parse(year), numberOfMonth, int.Parse(day));
                string value = items[4];//3: Invoice number;
                string value2 = items[3];
                parsedData.Add(currentDate, float.Parse(value));
            }
            var orderD = parsedData.OrderBy(x => x.Key);
            return orderD;
        }

        private static int resolveMonth(string month)
        {
            if (month.StartsWith("Jan", StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }
            if (month.StartsWith("Feb", StringComparison.CurrentCultureIgnoreCase))
            {
                return 2;
            }
            if (month.StartsWith("Mar", StringComparison.CurrentCultureIgnoreCase))
            {
                return 3;
            }
            if (month.StartsWith("Apr", StringComparison.CurrentCultureIgnoreCase))
            {
                return 4;
            }
            if (month.StartsWith("May", StringComparison.CurrentCultureIgnoreCase))
            {
                return 5;
            }
            if (month.StartsWith("Jun", StringComparison.CurrentCultureIgnoreCase))
            {
                return 6;
            }
            throw new NotImplementedException();
        }
    }
}
