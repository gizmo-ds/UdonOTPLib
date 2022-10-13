using UdonSharp;
using UnityEngine;
using System;

public class UdonOTPLib : UdonSharpBehaviour
{
  [SerializeField] private UdonHashLib _hashLibrary;

  public string HOTP(byte[] k, ulong c, int d)
  {
    byte[] hash = hmacsha1(k, ulong2bytes(c));
    int offset = hash[hash.Length - 1] & 0xf;
    byte[] bytes = new byte[4];
    Array.ConstrainedCopy(hash, offset, bytes, 0, 4);
    int code = bytes2int32(bytes);
    code &= 0x7FFFFFFF;
    code %= int32Power(10, d);
    return code.ToString().PadLeft(d, '0');
  }

  public string TOTP(string secret, double t, int digits, int period)
  {
    ulong c = Convert.ToUInt64(Math.Floor(t / period));
    byte[] key = base32Decode(secret);
    return HOTP(key, c, digits);
  }

  private byte[] hmacsha1(byte[] key, byte[] msg)
  {
    const int blocksize = 64;
    byte[] ipad = new byte[blocksize];
    byte[] opad = new byte[blocksize];
    if (key.Length > blocksize) key = hex2bytes(_hashLibrary.SHA1_Bytes(key));
    if (key.Length < blocksize) key = concat(key, new byte[blocksize - key.Length]);
    Array.Copy(key, ipad, blocksize);
    Array.Copy(key, opad, blocksize);
    for (int i = 0; i < blocksize; i++)
    {
      ipad[i] ^= 0x36;
      opad[i] ^= 0x5c;
    }

    // hash(opad + hash(ipad + msg))
    string s = _hashLibrary.SHA1_Bytes(concat(opad, hex2bytes(_hashLibrary.SHA1_Bytes(concat(ipad, msg)))));
    // Debug.Log($"hmacsha1: {s}");
    return hex2bytes(s);
  }

  public double Timestamp()
  {
    return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
  }

  public int Countdown(int period)
  {
    return period - (Convert.ToInt32(Timestamp()) % period);
  }

  private const string RFC4648_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

  private static byte[] base32Decode(string data)
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

  private static byte[] hex2bytes(string hex)
  {
    byte[] arr = new byte[hex.Length >> 1];
    for (int i = 0; i < hex.Length >> 1; ++i)
      arr[i] = (byte)((_hex(hex[i << 1]) << 4) + (_hex(hex[(i << 1) + 1])));
    return arr;
  }

  private static int _hex(char hex)
  {
    int val = (int)hex;
    return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
  }

  private static byte[] concat(byte[] arr1, byte[] arr2)
  {
    byte[] arr = new byte[arr1.Length + arr2.Length];
    arr1.CopyTo(arr, 0);
    arr2.CopyTo(arr, arr1.Length);
    return arr;
  }

  private static byte[] ulong2bytes(ulong v)
  {
    byte[] b = new byte[8];
    b[0] = (byte)((v & 0xff00000000000000) >> 56);
    b[1] = (byte)((v & 0xff000000000000) >> 48);
    b[2] = (byte)((v & 0xff0000000000) >> 40);
    b[3] = (byte)((v & 0xff00000000) >> 32);
    b[4] = (byte)((v & 0xff000000) >> 24);
    b[5] = (byte)((v & 0x00ff0000) >> 16);
    b[6] = (byte)((v & 0x0000ff00) >> 8);
    b[7] = (byte)((v & 0x000000ff));
    return b;
  }

  private static int bytes2int32(byte[] b)
  {
    return b[3] | b[2] << 8 | b[1] << 16 | b[0] << 24;
  }

  private static int int32Power(int v, int exp)
  {
    int r = v;
    for (int i = 0; i < exp - 1; i++) r *= v;
    return r;
  }
}
