using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST1_WPF
{
    class Crc
    {
        // zrobić gotowe wielomiany
        // dla crc 32 0x04C11DB7;

        private static UInt64 Checksum(byte[] fileData, UInt64 polynomial)
        {
            UInt64 crc = polynomial ^ fileData[0];

            for (int i = 0; i < 3; i++)
            {
                crc ^= fileData[0];
            }

            for (int i = 1; i < fileData.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (fileData[i] == 0) break;
                    crc ^= fileData[i];
                }
            }

            return crc;
        }

        public static Task<UInt64> Check(byte[] fileData, UInt64 polynomial)
        {
            return Task.Factory.StartNew(() => Checksum(fileData, polynomial));
        }
    }
}
