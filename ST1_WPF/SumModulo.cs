using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST1_WPF
{
    class SumModulo
    {
        private static byte Sum(byte[] fileData)
        {
            byte temp = fileData[0];

            for (int i = 1; i < 10; i++)
            {
                temp ^= fileData[i];
            }

            return temp;
        }
        public static Task<byte> Check(byte[] fileData)
        {
            return Task.Factory.StartNew(() => Sum(fileData));
        }
    }
}

