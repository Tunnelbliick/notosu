using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace storyboard.scriptslibrary.maniaModCharts.utility
{
    public static class QuickHash
    {

        public static string CreateHash(Vector2[] vectors)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                StringBuilder sb = new StringBuilder();
                foreach (Vector2 vec in vectors)
                {
                    sb.Append(vec.ToString());
                }

                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder hashSb = new StringBuilder();
                foreach (byte b in hash)
                {
                    hashSb.Append(b.ToString("x2"));
                }

                return hashSb.ToString();
            }
        }
    }
}