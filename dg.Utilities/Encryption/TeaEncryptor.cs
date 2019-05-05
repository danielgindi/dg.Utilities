using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities.Encryption
{
    public static class TeaEncryptor
    {
        static private List<Int32> BytesToLongs(byte[] s)
        {
            int slen = s.Length;
            int len = (int)Math.Ceiling(((double)slen) / 4.0d);
            List<Int32> l = new List<Int32>(len);
            int ll, lll;
            for (int i = 0; i < len; i++)
            {
                lll = 0;
                ll = i * 4;
                if (ll < slen) lll += s[ll];
                ll = i * 4 + 1;
                if (ll < slen) lll += s[ll] << 8;
                ll = i * 4 + 2;
                if (ll < slen) lll += s[ll] << 16;
                ll = i * 4 + 3;
                if (ll < slen) lll += s[ll] << 24;
                l.Add((Int32)lll);
            }
            return l;
        }
        static private byte[] LongsToBytes(List<Int32> l)
        {
            List<byte> a = new List<byte>(l.Count * 4);
            Int32 ll;
            for (int i = 0; i < l.Count; i++)
            {
                ll = l[i];
                a.Add((byte)(ll & 0xFF));
                a.Add((byte)(ll >> 8 & 0xFF));
                a.Add((byte)(ll >> 16 & 0xFF));
                a.Add((byte)(ll >> 24 & 0xFF));
            }
            return a.ToArray();
        }
        static public string Encrypt(string plaintext, string password)
        {
            if (plaintext.Length == 0) return @"";
            if (password.Length == 0) return plaintext;
            List<Int32> v = BytesToLongs(Encoding.UTF8.GetBytes(plaintext));
            while (v.Count <= 1) v.Add(0);
            List<Int32> k = BytesToLongs(Encoding.UTF8.GetBytes(password));
            while (k.Count < 4) k.Add(0);
            int n = v.Count;

            Int32 z = v[n - 1], y = v[0], sum = 0, e, DELTA = unchecked((Int32)0x9e3779b9), mx;
            Int32 q;
            q = 6 + 52 / n;
            while (q-- > 0)
            {
                sum += DELTA;
                e = sum >> 2 & 3;
                for (int p = 0; p < n; p++)
                {
                    y = v[(p + 1) % n];
                    mx = ((Int32)((UInt32)z >> 5) ^ (Int32)((UInt32)y << 2)) + (Int32)(((UInt32)y >> 3) ^ ((UInt32)z << 4)) ^ ((sum ^ y)) + (k[(p & 3 ^ e)] ^ z);
                    z = v[p] += mx;
                }
            }
            return Convert.ToBase64String(LongsToBytes(v), Base64FormattingOptions.None);
        }
        static public string Decrypt(string ciphertext, string password)
        {
            return Decrypt(ciphertext, password, false);
        }
        static public string Decrypt(string ciphertext, string password, bool fallBackToInput)
        {
            if (ciphertext.Length == 0) return @"";
            if (password.Length == 0) return ciphertext;
            byte[] fromBase64 = null;
            try
            {
                fromBase64 = Convert.FromBase64String(ciphertext);
            }
            catch 
            {
                return ciphertext;
            }
            List<Int32> v = BytesToLongs(fromBase64);
            List<Int32> k = BytesToLongs(Encoding.UTF8.GetBytes(password));
            while (k.Count < 4) k.Add(0);
            int n = v.Count;

            Int32 z = v[n - 1], y = v[0], sum = 0, e, DELTA = unchecked((Int32)0x9e3779b9), mx;
            Int32 q;
            q = 6 + 52 / n;
            sum = q * DELTA;

            while (sum != 0)
            {
                e = sum >> 2 & 3;
                for (var p = n - 1; p >= 0; p--)
                {
                    z = v[p > 0 ? p - 1 : n - 1];
                    mx = ((Int32)((UInt32)z >> 5) ^ (Int32)((UInt32)y << 2)) + (Int32)(((UInt32)y >> 3) ^ ((UInt32)z << 4)) ^ ((sum ^ y)) + (k[(p & 3 ^ e)] ^ z);
                    y = v[p] -= mx;
                }
                sum -= DELTA;
            }

            List<byte> plaintext = new List<byte>(LongsToBytes(v));
            while (plaintext.Count > 0 && plaintext[plaintext.Count - 1] == 0) plaintext.RemoveAt(plaintext.Count - 1);
            if (fallBackToInput && plaintext.Count == 0) return ciphertext;
            else return Encoding.UTF8.GetString(plaintext.ToArray());
        }
    }
}