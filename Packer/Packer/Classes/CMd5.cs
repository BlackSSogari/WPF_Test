using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.IO;

namespace Packer
{
    public class CMd5
    {
        MD5 md5Hash;

        public CMd5()
        {
            md5Hash = MD5.Create();
        }
                
        public string GetMD5HashEncodeASCII(string input)
        {
            StringBuilder MD5Str = new StringBuilder();

            byte[] byteArr = Encoding.ASCII.GetBytes(input);

            byte[] resultArr = (new MD5CryptoServiceProvider()).ComputeHash(byteArr);
            
            //for (int cnti = 1; cnti < resultArr.Length; cnti++) (2010.06.27)

            for (int cnti = 1; cnti < resultArr.Length; cnti++)
            {
                MD5Str.Append(resultArr[cnti].ToString("X2"));
            }

            return MD5Str.ToString();            
        }

        public string GetMd5Hash(string input)
        {
            if (File.Exists(input) == false) return null;

            // Convert the input string to a byte array and compute the hash.
            Stream fileStream = new FileStream(input, FileMode.Open);

            int off = GetType(fileStream as FileStream);

            byte[] data = null;
            if (off == 3)
            {
                long max = fileStream.Length;
                long tempOff = fileStream.Position;

                if (off == tempOff)
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    Byte[] tempData = new byte[max];
                    fileStream.Read(tempData, 0, (int)max);

                    data = md5Hash.ComputeHash(tempData, off, (int)(max-off));
                }
                else
                {
                    data = md5Hash.ComputeHash(fileStream);
                }
            }
            else
            {
                data = md5Hash.ComputeHash(fileStream);
                //byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            fileStream.Close();

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Verify a hash against a string.
        public bool CompareMd5HashValue(string source, string target)
        {            
            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(source, target))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //with BOM 
            int reVal = 0;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
            {
                reVal = 3;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = 3;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = 3;
            }
            //r.Close();
            //r.Dispose();
            fs.Seek(reVal, SeekOrigin.Begin);
            return reVal;
        }

    }
}
