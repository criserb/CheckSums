using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST1_WPF
{
    static class MakeError
    {
        private static Random _rnd;

        static MakeError()
        {
            _rnd = new Random();
        }

        private static byte[] WithRepeats(byte[] fileData, int errors)
        {
            int count = 0, index;
            byte i;
            do
            {
                index = _rnd.Next(0, fileData.Length);
                i = 1;
                i <<= _rnd.Next(0, 8);
                fileData[index] = ChangeBit(fileData[index], i);
                count++;
            } while (count < errors);
            return fileData;
        }
        public static Task<byte[]> WRepeats(byte[] fileData, int errors)
        {
            return Task.Factory.StartNew(() => WithRepeats(fileData, errors));
        }
        private static byte ChangeBit(byte fileByte, byte bit)
        {
            if ((fileByte & bit) == bit)
            {
                fileByte = Convert.ToByte(fileByte & (~(fileByte & bit)));
            }
            else
            {
                fileByte |= bit;
            }
            return fileByte;
        }
        private static byte[] WithNoRepeats(byte[] fileData, int errors)
        {
            List<Point> listOfIndexes = new List<Point>();
            Point comp = new Point();

            bool duplicateFound;
            int count = 0, index;
            byte i;
            do
            {
                index = _rnd.Next(0, fileData.Length);
                i = 1;
                i <<= _rnd.Next(0, 8);
                comp.X = index;
                comp.Y = i;
                duplicateFound = false;

                foreach (var item in listOfIndexes)
                {
                    if (item.IsTheSame(comp))
                    {
                        duplicateFound = true;
                        break;
                    }
                }
                if (!duplicateFound)
                {
                    fileData[index] = ChangeBit(fileData[index], i);
                    listOfIndexes.Add(new Point(index, i));
                    count++;
                }

            } while (count < errors);
            return fileData;
        }
        public static Task<byte[]> WNRepeats(byte[] fileData, int errors)
        {
            return Task.Factory.StartNew(() => WithNoRepeats(fileData, errors));
        }
    }
}
