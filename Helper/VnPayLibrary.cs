using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace WebCinema.Helpers
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData[key] = value;
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData[key] = value;
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();

            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(UrlEncodeVnPay(kv.Key) + "=" + UrlEncodeVnPay(kv.Value) + "&");
                }
            }

            string rawData = data.ToString().TrimEnd('&');
            string secureHash = HmacSHA512(hashSecret, rawData);

            // Debug log
            System.Diagnostics.Debug.WriteLine(">>> rawData: " + rawData);
            System.Diagnostics.Debug.WriteLine(">>> secureHash: " + secureHash);

            return $"{baseUrl}?{rawData}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(string secureHash, string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) &&
                    kv.Key != "vnp_SecureHash" &&
                    kv.Key != "vnp_SecureHashType")
                {
                    data.Append(UrlEncodeVnPay(kv.Key) + "=" + UrlEncodeVnPay(kv.Value) + "&");
                }
            }

            string rawData = data.ToString().TrimEnd('&');
            string myChecksum = HmacSHA512(hashSecret, rawData);

            // Debug log
            System.Diagnostics.Debug.WriteLine(">>> VALIDATE rawData: " + rawData);
            System.Diagnostics.Debug.WriteLine(">>> secureHash server: " + myChecksum);
            System.Diagnostics.Debug.WriteLine(">>> secureHash client: " + secureHash);

            return myChecksum.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
                foreach (byte b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        /// <summary>
        /// Encode theo chuẩn RFC3986
        /// </summary>
        private static string UrlEncodeVnPay(string str)
        {
            return Uri.EscapeDataString(str)
                      .Replace("%20", "+")        // VNPay dùng '+' thay vì '%20'
                      .Replace("(", "%28")
                      .Replace(")", "%29");
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}
