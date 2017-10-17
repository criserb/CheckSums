using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ST1_WPF
{
    public static class ParityBit
    {
        /// <summary>
        /// 0 - is parity, 1 - isn't parity
        /// </summary>
        public static int SumOf1(byte[] fileData)
        {
            int sum = 0;
            byte k;

            for (int j = 0; j < fileData.Length; j++)
            {
                k = 1;
                for (byte i = 0; i < 8; i++)
                {
                    // Jeśli sprawdzany bit jest jedynką to sum++
                    if ((fileData[j] & k) == k) sum++;
                    k <<= 1;
                }
            }

            //string tmp;
            //foreach (var item in fileData)
            //{
            //    tmp = Convert.ToString(item, 2);
            //    foreach (var item2 in tmp)
            //    {
            //        if (item2 == '1') sum++;
            //    }
            //}

            return sum;
        }
        public static Task<int> Check(byte[] fileData)
        {
            return Task.Factory.StartNew(() => ParityBit.SumOf1(fileData));
        }
    }
}
