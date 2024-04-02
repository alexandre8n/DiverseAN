using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efa
{
    public class Afile
    {
        public string filePath;
        public long fileSize;
        public UInt64? checkSum = null;
        //

        public byte[] GetContentBuffer()
        {
            byte[] thisFileBuff = File.ReadAllBytes(filePath);
            if (checkSum == null)
            {
                checkSum = Utl.GetFletcherChecksum4ByteArray(thisFileBuff, 32);
            }
            return thisFileBuff;
        }
        
        public bool IsEqualTo(Afile fileToCompare)
        {
            if (fileSize != fileToCompare.fileSize)
                return false;
            if (this.checkSum != null && fileToCompare.checkSum != null)
            {
                if (checkSum != fileToCompare.checkSum)
                    return false;
            }
            byte[] thisFileBuff = GetContentBuffer();
            byte[] fileToCompareBuff = fileToCompare.GetContentBuffer();
            for (long i = 0; i < fileSize; i++)
            {
                if (thisFileBuff[i] != fileToCompareBuff[i])
                    return false;
            }
            return true;
        }

        public string PrepareOutString()
        {
            string s = string.Format("{0} ({1})\n", filePath, fileSize);
            return s;
        }

        public static long GetCheckSum(byte[] arr)
        {
            return -1;
        }


    }
}
