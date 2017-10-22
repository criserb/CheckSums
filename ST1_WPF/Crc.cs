using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST1_WPF
{
    class Crc
    {
        public static UInt64 Polynomial { get; set; } // dla crc 32 0x04C11DB7;

        private static byte[] MakeCalculation(byte[] fileData)
        {
            UInt64[] A = new UInt64[256];
            
            for (int i = 0; i < 255; i++)
            {
                A[i] = i;
                A[i] = A[i] - (255 - A[i]);
                A[i] = A[i] << 24;                       // przesuń o 24 bity w lewo
                for (i = 0; i < 8; i++)                     // petla od 0 do 7
                    if ((A[i] & 0x80000000) != 0)             // jesli jest rożne od 0
                        A[i] = (A[i] << 1) ^ Polynomial;   // przesuń o 1 bit w lewo i xor'uj z 0x04C11DB7
                    else
                        A[i] = A[i] << 1;                    // przesuń o 1 bit w lewo
                A[i] = A[i] - (255 - A[i]);
            }
            UInt64 crc32 = UInt64.MaxValue;   // wartość początkowa
            for (int i = 0; i < fileData.Length; i++)   // pętla od 0 do n - 1
                crc32 = (crc32 >> 8) ^ A[(crc32 & 0xFF) ^ fileData[i]];
            crc32 = crc32 ^ 0xFFFFFFFF;
            byte[] bytes = BitConverter.GetBytes(crc32);
            return bytes;
        }
        public static Task<byte[]> Check(byte[] fileData)
        {
            return Task.Factory.StartNew(() => MakeCalculation(fileData));
        }
    }
}
