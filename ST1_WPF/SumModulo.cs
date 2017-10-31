using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST1_WPF
{
    static class SumModulo
    {
        private static byte Sum(byte[] fileData)
        {
            byte temp = fileData[0];

            for (int i = 1; i < fileData.Length; i++)
            {
                temp ^= fileData[i];
            }

            return temp;
        }

        // Uruchamianie funkcji obliczania sumy modulo asynchronicznie
        public static Task<byte> Check(byte[] fileData)
        {
            return Task.Factory.StartNew(() => Sum(fileData));
        }
    }
}

