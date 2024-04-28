using UdonSharp;
using System;

public class UdonOTPLib : UdonSharpBehaviour
{
    public string HOTP(byte[] k, ulong c, int d)
    {
        byte[] msg = BitConverter.GetBytes(c);
        Array.Reverse(msg);
        byte[] hash = _hmacSha1(k, msg);
        int offset = hash[hash.Length - 1] & 0xf;
        byte[] bytes = new byte[4];
        Array.ConstrainedCopy(hash, offset, bytes, 0, 4);
        int code = _bytes2int32(bytes);
        code &= 0x7FFFFFFF;
        code %= _int32Power(10, d);
        return code.ToString().PadLeft(d, '0');
    }

    public string TOTP(string secret, double t, int digits, int period)
    {
        ulong c = Convert.ToUInt64(Math.Floor(t / period));
        byte[] key = _base32Decode(secret);
        return HOTP(key, c, digits);
    }

    private static byte[] _hmacSha1(byte[] key, byte[] msg)
    {
        const int blocksize = 64;
        byte[] ipad = new byte[blocksize];
        byte[] opad = new byte[blocksize];
        if (key.Length > blocksize) key = SHA1.ComputeHash(key);
        if (key.Length < blocksize) key = _concat(key, new byte[blocksize - key.Length]);
        Array.Copy(key, ipad, blocksize);
        Array.Copy(key, opad, blocksize);
        for (int i = 0; i < blocksize; i++)
        {
            ipad[i] ^= 0x36;
            opad[i] ^= 0x5c;
        }

        return SHA1.ComputeHash(_concat(opad, SHA1.ComputeHash(_concat(ipad, msg))));
    }

    public static double Timestamp()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    public static int Countdown(int period)
    {
        return period - (Convert.ToInt32(Timestamp()) % period);
    }

    private const string RFC4648_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private static byte[] _base32Decode(string data)
    {
        int unpadded_data_length = data.Length;
        for (int i = 1; i < (Math.Min(6, data.Length)) + 1; i++)
        {
            if (data[data.Length - i] != '=') break;
            unpadded_data_length -= 1;
        }

        int output_length = unpadded_data_length * 5 / 8;
        byte[] _output = new byte[output_length];
        char[] bytes = data.ToCharArray();
        int index = 0;
        for (int bitIndex = 0; bitIndex < data.Length * 5; bitIndex += 8)
        {
            int dualbyte = RFC4648_ALPHABET.IndexOf(bytes[bitIndex / 5]) << 10;
            if (bitIndex / 5 + 1 < bytes.Length)
                dualbyte |= RFC4648_ALPHABET.IndexOf(bytes[bitIndex / 5 + 1]) << 5;
            if (bitIndex / 5 + 2 < bytes.Length)
                dualbyte |= RFC4648_ALPHABET.IndexOf(bytes[bitIndex / 5 + 2]);
            dualbyte = 0xff & (dualbyte >> (15 - bitIndex % 5 - 8));
            _output[index] = (byte)(dualbyte);
            index++;
        }

        return _output;
    }

    private static byte[] _concat(byte[] arr1, byte[] arr2)
    {
        byte[] arr = new byte[arr1.Length + arr2.Length];
        arr1.CopyTo(arr, 0);
        arr2.CopyTo(arr, arr1.Length);
        return arr;
    }

    private static int _bytes2int32(byte[] b)
    {
        return b[3] | b[2] << 8 | b[1] << 16 | b[0] << 24;
    }

    private static int _int32Power(int v, int exp)
    {
        int r = v;
        for (int i = 0; i < exp - 1; i++) r *= v;
        return r;
    }
}

/*
 * Implementation of SHA-1 Hashing Algorithm
 * ----------------------------------------
 * WARNING: This SHA-1 implementation is for experimental use only.
 */
public static class SHA1
{
    public static byte[] ComputeHash(byte[] data)
    {
        uint h0 = 0x67452301,
            h1 = 0xEFCDAB89,
            h2 = 0x98BADCFE,
            h3 = 0x10325476,
            h4 = 0xC3D2E1F0;

        byte[] paddedData = _padData(data);

        int blocks = paddedData.Length / 64;
        for (int i = 0; i < blocks; i++)
        {
            uint[] w = new uint[80];
            for (int j = 0; j < 16; j++)
            {
                w[j] = BitConverter.ToUInt32(paddedData, (i * 64) + (j * 4));
                w[j] = _reverseBytes(w[j]);
            }

            for (int j = 16; j < 80; j++)
            {
                w[j] = _leftRotate(w[j - 3] ^ w[j - 8] ^ w[j - 14] ^ w[j - 16], 1);
            }

            uint a = h0, b = h1, c = h2, d = h3, e = h4;

            for (int j = 0; j < 80; j++)
            {
                uint f = 0, k = 0;

                if (j < 20)
                {
                    f = (b & c) | (~b & d);
                    k = 0x5A827999;
                }
                else if (j < 40)
                {
                    f = b ^ c ^ d;
                    k = 0x6ED9EBA1;
                }
                else if (j < 60)
                {
                    f = (b & c) | (b & d) | (c & d);
                    k = 0x8F1BBCDC;
                }
                else if (j >= 60)
                {
                    f = b ^ c ^ d;
                    k = 0xCA62C1D6;
                }

                uint temp = _leftRotate(a, 5) + f + e + k + w[j];
                e = d;
                d = c;
                c = _leftRotate(b, 30);
                b = a;
                a = temp;
            }

            h0 += a;
            h1 += b;
            h2 += c;
            h3 += d;
            h4 += e;
        }

        byte[] hash = new byte[20];
        Array.Copy(BitConverter.GetBytes(_reverseBytes(h0)), 0, hash, 0, 4);
        Array.Copy(BitConverter.GetBytes(_reverseBytes(h1)), 0, hash, 4, 4);
        Array.Copy(BitConverter.GetBytes(_reverseBytes(h2)), 0, hash, 8, 4);
        Array.Copy(BitConverter.GetBytes(_reverseBytes(h3)), 0, hash, 12, 4);
        Array.Copy(BitConverter.GetBytes(_reverseBytes(h4)), 0, hash, 16, 4);

        return hash;
    }

    private static byte[] _padData(byte[] data)
    {
        int originalLength = data.Length;
        ulong paddingLength = (ulong)((56 - (originalLength + 1) % 64) % 64);

        byte[] paddedData = new byte[(ulong)originalLength + 1 + paddingLength + 8];

        Array.Copy(data, paddedData, originalLength);
        paddedData[originalLength] = 0x80;

        long lengthInBits = originalLength * 8;
        byte[] lengthBytes = BitConverter.GetBytes(lengthInBits);
        if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);

        Array.Copy(lengthBytes, 0, paddedData, paddedData.Length - 8, 8);
        return paddedData;
    }

    private static uint _leftRotate(uint value, int bits)
    {
        return (value << bits) | (value >> (32 - bits));
    }

    private static uint _reverseBytes(uint value)
    {
        return ((value & 0x000000FF) << 24) |
               ((value & 0x0000FF00) << 8) |
               ((value & 0x00FF0000) >> 8) |
               ((value & 0xFF000000) >> 24);
    }
}
