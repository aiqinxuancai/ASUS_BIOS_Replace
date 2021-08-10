using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ASUS_BIOS_Replace
{
    class Program
    {
        static void Main(string[] args)
        {
            //华硕BIOS魔改前替换部分旧代码到新的BIOS文件中
            Console.WriteLine("Start");
            if (args.Length != 2)
            {
                Console.WriteLine("Error:Parameter error");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Error:File [{args[0]}] not found");
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine($"Error:File [{args[1]}] not found");
                return;
            }

            byte[] sourceData = File.ReadAllBytes(args[0]);
            byte[] newData = File.ReadAllBytes(args[1]);


            //替换区域 ASUSBKP
            Console.WriteLine($"Replace [ASUSBKP] ...");
            try
            {
	            newData = ReplaceData(Encoding.UTF8.GetBytes("ASUSBKP"), 
	                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
	                sourceData, newData);
                Console.WriteLine($"Replace [ASUSBKP] success");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error:{ex}");
            }

            //替换区域 BSA_
            Console.WriteLine($"Replace [BSA_] ...");
            try
            {
	            newData = ReplaceData(Encoding.UTF8.GetBytes("BSA_"),
	                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
	                sourceData, newData);
                Console.WriteLine($"Replace [BSA_] success");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error:{ex}");
            }

            //网卡MAC替换
            Console.WriteLine($"Replace MAC address ...");
            try
            {
                Array.Copy(sourceData, 0x1000, newData, 0x1000, 0x2000);
                Console.WriteLine($"Replace MAC address success");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error:{ex}");
            }

            Console.WriteLine($"Save to Replace.bin");
            File.WriteAllBytes("Replace.bin", newData);

        }



        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="startBytes"></param>
        /// <param name="endBytes"></param>
        /// <param name="sourceData">原始数据</param>
        /// <param name="newData"></param>
        /// <returns></returns>
        static byte[] ReplaceData(byte[] startBytes, byte[] endBytes, byte[] sourceData, byte[] newData)
        {

            int indexStart = FindByte(sourceData, startBytes);
            if (indexStart != -1)
            {
                int indexEnd = FindByte(sourceData, endBytes, indexStart);
                int indexStartReplace = FindByte(newData, startBytes);
                if (indexEnd != -1 && indexStartReplace != -1)
                {
                    Debug.WriteLine($"Change start {indexStart} - {indexEnd}, length { indexEnd - indexStart}");

                    var bytes = new byte[indexEnd - indexStart];
                    Array.Copy(sourceData, indexStart, bytes, 0, indexEnd - indexStart);
                    Console.WriteLine(ByteToHex(bytes));

                    Array.Copy(sourceData, indexStart, newData, indexStartReplace, indexEnd - indexStart);
                    return newData;
                }
            }

            throw new Exception("HeadFindError");

        }

        static string ByteToHex(byte[] source)
        {
            var bytes = source;
            StringBuilder ret = new StringBuilder();
            foreach (byte b in bytes)
            {
                ret.AppendFormat("{0:X2}", b);
            }
            var hex = ret.ToString();
            return hex;
        }


        static int FindByte(byte[] source, byte[] target, int startIndex = 0)
        {
            int result = -1;
            for (int i = startIndex; i < source.Length; i++)
            {
                int successCount = 0;
                for (int x = 0; x < target.Length; x++)
                {
                    if (i + x > source.Length)
                    {
                        break;
                    }
                    if (source[i + x] == target[x])
                    {
                        successCount++;
                    }
                }

                if (successCount == target.Length)
                {
                    
                    result = i;
                    break;
                }
            }
            return result;
        }




    }
}
