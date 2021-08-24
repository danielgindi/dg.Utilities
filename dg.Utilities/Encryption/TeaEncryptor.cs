using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities.Encryption
{
    public static class TeaEncryptor
    {
        public static string Encrypt(string plaintext, string password)
        {
            if (plaintext.Length == 0) return @"";
            if (password.Length == 0) return plaintext;
            
            var v = BytesToLongs(Encoding.UTF8.GetBytes(plaintext), 2);
            var k = BytesToLongs(Encoding.UTF8.GetBytes(password), 4);
            int n = v.Length;

            UInt32 z = v[n - 1], y, sum = 0, e, DELTA = 0x9e3779b9, mx;
            UInt32 q = (UInt32)(6 + 52 / n);
            
            while (q-- > 0)
            {
                sum += DELTA;
                e = sum >> 2 & 3;
                for (int p = 0; p < n; p++)
                {
                    y = v[(p + 1) % n];
                    mx = ((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4)) ^ (sum ^ y) + (k[(p & 3 ^ e)] ^ z);
                    z = v[p] += mx;
                }
            }
            
            return Convert.ToBase64String(LongsToBytes(v), Base64FormattingOptions.None);
        }

        public static string Decrypt(string ciphertext, string password, bool fallBackToInput = false)
        {
            if (ciphertext.Length == 0) return @"";
            if (password.Length == 0) return ciphertext;
            
            byte[] fromBase64;
            
            try
            {
                fromBase64 = Convert.FromBase64String(ciphertext);
            }
            catch
            {
                if (fallBackToInput)
                    return ciphertext;
                
                throw;
            }
            
            var v = BytesToLongs(fromBase64);
            var k = BytesToLongs(Encoding.UTF8.GetBytes(password), 4);
            int n = v.Length;

            UInt32 z, y = v[0], e, DELTA = 0x9e3779b9, mx;
            UInt32 q = (UInt32)(6 + 52 / n);
            UInt32 sum = q * DELTA;

            while (sum != 0)
            {
                e = sum >> 2 & 3;
                for (var p = n - 1; p >= 0; p--)
                {
                    z = v[p > 0 ? p - 1 : n - 1];
                    mx = ((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4)) ^ (sum ^ y) + (k[(p & 3 ^ e)] ^ z);
                    y = v[p] -= mx;
                }
                sum -= DELTA;
            }

            var decryptedBytes = LongsToBytes(v);
            var decryptedLen = decryptedBytes.Length;
            while (decryptedLen > 0 && decryptedBytes[decryptedLen - 1] == 0)
                decryptedLen--;

            if (decryptedLen == 0)
                return "";
            else return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedLen);
        }

        private static UInt32[] BytesToLongs(byte[] s, int padZeroMinLength = 0)
        {
            var slen = s.Length;
            var len = (int)Math.Ceiling(((double)slen) / 4.0d);
            var l = new UInt32[Math.Max(len, padZeroMinLength)];
            
            for (int i = 0, i4 = 0; i < len; i++, i4 += 4)
            {
                l[i] = ((s[i4])) +
                    ((i4 + 1) >= slen ? (UInt32)0 << 8 : ((UInt32)s[i4 + 1] << 8)) +
                    ((i4 + 2) >= slen ? (UInt32)0 << 16 : ((UInt32)s[i4 + 2] << 16)) +
                    ((i4 + 3) >= slen ? (UInt32)0 << 24 : ((UInt32)s[i4 + 3] << 24));
            }
            
            return l;
        }

        private static byte[] LongsToBytes(UInt32[] l)
        {
            var llen = l.Length;
            var b = new byte[llen * 4];
            
            for (int i = 0, i4 = 0; i < llen; i++, i4 += 4)
            {
                var li = l[i];
                b[i4] = (byte)(li & 0xFF);
                b[i4 + 1] = (byte)(li >> (8 & 0xFF));
                b[i4 + 2] = (byte)(li >> (16 & 0xFF));
                b[i4 + 3] = (byte)(li >> (24 & 0xFF));
            }
            
            return b;
        }
    }
}