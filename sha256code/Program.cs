using System;
using System.Text;

class SHA256
{
    static void Main()
    {
        Console.WriteLine("Введите сообщение для шифрования:");
        string input = Console.ReadLine();
        while (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Некорректный ввод!");
            input = Console.ReadLine();
        }
        
        uint[] k = buildAr(true);
        Console.WriteLine("Исходное сообщение:\n" + input);
        string hash = ComputeHash(input, k);
        Console.WriteLine($"\nSHA-256: {hash}");
        Console.ReadKey();
    }
    public static string ComputeHash(string input, uint[] k)
    {
        uint[] H = buildAr();
        string binaryString = StringToBinary(input);
        int dataLen = binaryString.Length;
        int numBlocks = ((dataLen + 64) / 512) + 1;
        binaryString += '1';
        binaryString = binaryString.PadRight((numBlocks * 512) - 64, '0');
        binaryString += Convert.ToString(dataLen, 2).PadLeft(64, '0');

        Console.WriteLine("\nДвоичное представление сообщения:");
        for (int i = 0; i < binaryString.Length; i++)
        {
            Console.Write(binaryString[i]);
            if ((i + 1) % 8==0) Console.Write(" ");
            if ((i + 1) % 32 == 0) Console.Write("\n");
        }

        uint[] paddedData = new uint[numBlocks * 16];
        int l = 0;
        StringBuilder sb = new StringBuilder(binaryString);
        for (int i = 0; i < binaryString.Length / 32; i++)
        {
            string binarySegment = binaryString.Substring(i * 32, 32);
            paddedData[l] = Convert.ToUInt32(binarySegment, 2);
            l++;
        }
        uint[] w = new uint[64];
        for (int i = 0; i < numBlocks; i++)
        {
            Array.Copy(paddedData, i * 16, w, 0, 16);
            for (int t = 16; t < 64; t++)
                w[t] = sigma1(w[t - 2]) + w[t - 7] + sigma0(w[t - 15]) + w[t - 16];
            uint a = H[0];
            uint b = H[1];
            uint c = H[2];
            uint d = H[3];
            uint e = H[4];
            uint f = H[5];
            uint g = H[6];
            uint h = H[7];
            for (int t = 0; t < 64; t++)
            {
                uint T1 = h + Sigma1(e) + Ch(e, f, g) + k[t] + w[t];
                uint T2 = Sigma0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + T1;
                d = c;
                c = b;
                b = a;
                a = T1 + T2;
            }
            H[0] += a;
            H[1] += b;
            H[2] += c;
            H[3] += d;
            H[4] += e;
            H[5] += f;
            H[6] += g;
            H[7] += h;
        }
        return string.Concat(Array.ConvertAll(H, h => h.ToString("x8")));
    }

    static string StringToBinary(string text)
    {
        StringBuilder sb = new StringBuilder();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        foreach (byte b in bytes)
        {
            sb.Append(Convert.ToString(b, 2).PadLeft(8, '0')); 
        }                                              
        return sb.ToString();
    }

    static uint[] buildAr(bool kB = false)
    {
        int i = kB ? 64 : 8;
        uint[] ar = new uint[i];
        int primeCount = 0;
        int num = 2;
        while (primeCount < i)
        {
            if (IsPrime(num))
            {
                double root = Math.Pow(num, 1.0 / (kB ? 3.0 : 2.0));
                double fractionalPart = root - Math.Floor(root);
                ar[primeCount] = (uint)(fractionalPart * Math.Pow(2, 32)); 
                primeCount++;
            }
            num++;
        }
        return ar;
    }

    static bool IsPrime(int num)
    {
        if (num <= 1) return false;
        if (num <= 3) return true;
        if ((num & 1) == 0) return false;  

        int sqrt = (int)Math.Sqrt(num);   
        for (int i = 3; i <= sqrt; i += 2) 
        {
            if (num % i == 0) return false;
        }
        return true;
    }

    static uint ROTR(uint x, int n) => (x >> n) | (x << (32 - n));
    static uint SHR(uint x, int n) => x >> n;

    static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);
    static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);
    static uint Sigma0(uint x) => ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
    static uint Sigma1(uint x) => ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
    static uint sigma0(uint x) => ROTR(x, 7) ^ ROTR(x, 18) ^ SHR(x, 3);
    static uint sigma1(uint x) => ROTR(x, 17) ^ ROTR(x, 19) ^ SHR(x, 10);
}