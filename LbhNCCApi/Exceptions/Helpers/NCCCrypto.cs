using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NCCSSO
{
    internal class NCCCrypto
    {

        #region Consts
        /// <summary>
        /// Change the Inputkey GUID when you use this code in your own program.
        /// </summary>
        internal const string Inputkey = "";
        #endregion

        #region  Encryption

        /// <summary>
        /// Encrypt the given text and give the byte array back as a BASE64 string
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <param name="salt">The pasword salt</param>
        /// <returns>The encrypted text</returns>
        public static string Encrypt(string text, string salt)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            text = text + "$" + DateTime.Now;

            return ConvertStringToHex(text, Encoding.ASCII);
        }
        #endregion

        #region  Decryption
        /// <summary>
        /// Checks if a string is base64 encoded
        /// </summary>
        /// <param name="base64String">The base64 encoded string</param>
        /// <returns></returns>
        public static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();
            return (base64String.Length % 4 == 0) && Regex.IsMatch(base64String, @"^[a-zA-Z0-9]*={0,3}$", RegexOptions.None);
        }

        /// <summary>
        /// Decrypts the given text
        /// </summary>
        /// <param name="cipherText">The encrypted BASE64 text</param>
        /// <returns>De gedecrypte text</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            return ConvertHexToString(cipherText, Encoding.ASCII);
        }
        #endregion

        #region Instantiate
        /// <summary>
        /// Create a new RijndaelManaged class and initialize it
        /// </summary>
        /// <param name="salt">The pasword salt</param>
        /// <returns></returns>
        private static RijndaelManaged Instantiate(string salt)
        {
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(Inputkey, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }
        #endregion

        public static string ConvertStringToHex(String input, System.Text.Encoding encoding)
        {
            Byte[] stringBytes = encoding.GetBytes(input);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            return sbBytes.ToString();
        }

        public static string ConvertHexToString(String hexInput, System.Text.Encoding encoding)
        {
            int numberChars = hexInput.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
            }
            return encoding.GetString(bytes);
        }
    }
}