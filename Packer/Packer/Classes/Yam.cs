using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

// 히스토리
// 버전1: 초기버전
// 버전2: mask 값을 master_key 값에 따라 바뀌도록 수정.
// 버전3: > 파일별로 적용될 마스크 값이 다르게 적용 될 수 있도록 수정. 
//       (파일 사이즈에 따라서 마스크값을 생성하는 키 인덱스 변경)
// 버전4: > 입력 데이터가 string 인 경우 처리방식 변경.
//       (EncodeFromString, DecodeToString)
// 버전5: 처리 단위를 16바이트로 변경. (마스크 방식에서 3bit 쉬프트 방식으로=ㅂ=)

// NOTE: Encode, Decode 을 여러번 수행하는 경우 기존값이 나올 수 있음.
//       (원본데이터 쉬프트 주기와 XOR 연산 반복 주기 절묘? 하게 맞는 경우)
// NOTE: 데이터의 크기가 변하지 않고, 간소한 암호화가 필요한 경우 사용 할 수 있음.

namespace YamEncrpty
{
    #region PackUtility 클래스

    public static class PackUtility
    {
        static long MakeLength(long value, int bytes)
        {
            long rest = value % bytes;
            return (rest == 0) ? value : value + bytes - rest;
        }

        // 파일을 byte[] array 로 읽어온다. (tea 에서 Encrypt할 수 있도록 byte[] 로 읽어온다. bytes 를 2로 설정해야 함)
        public static byte[] readFile(string filename, int bytes = 0)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException();

            if (!File.Exists(filename))
                throw new FileNotFoundException();

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                long length = reader.BaseStream.Length;
                if (bytes != 0)
                    length = MakeLength(reader.BaseStream.Length, bytes);
                byte[] ret = new byte[length];
                reader.Read(ret, 0, (int)reader.BaseStream.Length);
                reader.Close();
                return ret;
            }
        }

        // 파일을 string 으로 읽어온다. (utf8 인코딩)
        public static string readFileToString(string filename)
        {
            byte[] data = readFile(filename);
            if (data != null)
                return UTF8Encoding.UTF8.GetString(data);
            return null;
        }

        public static uint GetStringHash(string str)
        {
            // DJBHash
            uint hash = 5381;

            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) + hash) + str[i];
            }

            return hash;
        }

        /// <summary> 암호화 해야하는 확장자 검사 </summary>
        public static bool CheckExt(string[] EncodeExt, string ext_)
        {
            foreach (string ext in EncodeExt)
            {
                if (ext.Equals(ext_, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        
    }

    #endregion

	public class Yam
	{
		byte[] yam_master_key = new byte[16];
		uint[] yam_master_table = new uint[4];

		private const string default_key = "YamEncrpty_Ver5";

		public Yam()
		{
			setKey(default_key);
		}

		public Yam(string strKey)
		{
			setKey(strKey);
		}

		public void setKey(string key)
		{
			setMasterKey(MD5Helper.getHash(Encoding.UTF8.GetBytes(key)));
		}

		public void setKey(byte[] key)
		{
			setMasterKey(MD5Helper.getHash(key));
		}

		private void setMasterKey(byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException();
			if (key.Length < 16)
				throw new ArgumentException();

			Buffer.BlockCopy(key, 0, yam_master_key, 0, 16);

			for (int i = 0; i < 16; i += 4)
				yam_master_table[i / 4] = readUInt32(yam_master_key, i);
		}

		private uint shiftRight(uint value)
		{
			return ((value & 0x7) << 29) | (value >> 3);
		}

		private uint shiftLeft(uint value)
		{
			return ((value & 0xE0000000) >> 29) | (value << 3);
		}

		private uint readUInt32(byte[] data, int address)
		{
			return (uint)((data[address] << 24) | (data[address + 1] << 16) | (data[address + 2] << 8) | data[address + 3]);
		}

		private void writeUInt32(byte[] data, int address, uint value)
		{
			data[address+0] = (byte)((value & 0xff000000) >> 24);
			data[address+1] = (byte)((value & 0xff0000) >> 16);
			data[address+2] = (byte)((value & 0xff00) >> 8);
			data[address+3] = (byte)((value & 0xff));
		}

		public byte[] Encode(byte[] data)
		{
			/*
			Stopwatch sw = new Stopwatch();
			sw.Start();
			// */

			int length = data.Length;

			for (int i = 0; i < length; i += 16)
			{
				int left = length - i;
				if (left > 15)
				{
					// 원본 -> r-shift -> xor
					writeUInt32(data, i, shiftRight(readUInt32(data, i)) ^ yam_master_table[0]);
					writeUInt32(data, i + 4, shiftRight(readUInt32(data, i + 4)) ^ yam_master_table[1]);
					writeUInt32(data, i + 8, shiftRight(readUInt32(data, i + 8)) ^ yam_master_table[2]);
					writeUInt32(data, i + 12, shiftRight(readUInt32(data, i + 12)) ^ yam_master_table[3]);
				}
				else
				{
					// 짜투리 부분은 적당히...
					for (int j = 0; j < left; ++j )
						data[i + j] ^= yam_master_key[j];
				}
			}

			/*
			long procTimeMsec = sw.ElapsedMilliseconds;
			sw.Stop();
			Debug.WriteLine(string.Format("Encode: {0} msec [{1}bytes]", procTimeMsec, length));
			// */

			return data;
		}

		public byte[] Decode(byte[] data)
		{
			/*
			Stopwatch sw = new Stopwatch();
			sw.Start();
			// */

			int length = data.Length;

			for (int i = 0; i < length; i += 16)
			{
				int left = length - i;
				if (left > 15)
				{
					// xor -> l-shift -> 원본
					writeUInt32(data, i, shiftLeft(readUInt32(data, i) ^ yam_master_table[0]));
					writeUInt32(data, i + 4, shiftLeft(readUInt32(data, i + 4) ^ yam_master_table[1]));
					writeUInt32(data, i + 8, shiftLeft(readUInt32(data, i + 8) ^ yam_master_table[2]));
					writeUInt32(data, i + 12, shiftLeft(readUInt32(data, i + 12) ^ yam_master_table[3]));
				}
				else
				{
					// 짜투리 부분은 적당히...
					for (int j = 0; j < left; ++j)
						data[i + j] ^= yam_master_key[j];
				}
			}

			/*
			long procTimeMsec = sw.ElapsedMilliseconds;
			sw.Stop();
			Debug.WriteLine(string.Format("Decode: {0} msec [{1}bytes]", procTimeMsec, length));
			// */

			return data;
		}

		public byte[] EncodeFromString(string str)
		{
			return Encode(Encoding.UTF8.GetBytes(str));
		}

		public string DecodeToString(byte[] data)
		{
			return Encoding.UTF8.GetString(Decode(data));
		}
	}

	static public class MD5Helper
    {
        static private MD5 md5_ = MD5.Create();
		static public byte[] getHash(byte[] data)
		{
			return md5_.ComputeHash(data);
		}
    }
}
