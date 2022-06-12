using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Security.Cryptography;

namespace CrackedAuth
{
    internal class Auth
    {
        static string ComputeSha256Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        }  
        public static void login()
        {
            string KEY = "";
            if (File.Exists("key.cto"))
            {
                KEY = File.ReadAllText("key.cto");
            }
            else
            {
                Console.WriteLine("Auth Key: ");
                KEY = Console.ReadLine();
            }
            
             
            ManagementObjectCollection mbsList = null;
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select ProcessorID From Win32_processor");
            mbsList = mos.Get();
            string processorId = string.Empty;
            foreach (ManagementBaseObject mo in mbsList)
            {
                processorId = mo["ProcessorID"] as string;
            }

            mos = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            mbsList = mos.Get();
            string systemId = string.Empty;
            foreach (ManagementBaseObject mo in mbsList)
            {
                systemId = mo["UUID"] as string;
            }

            string str = ComputeSha256Hash($"{processorId}{systemId}");
            
            HttpWebRequest httpWebRequest = WebRequest.Create("https://cracked.io/auth.php") as HttpWebRequest;
            httpWebRequest.Proxy = null;
            HttpWebRequest httpWebRequest2 = httpWebRequest;
            httpWebRequest2.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(httpWebRequest2.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object obj, X509Certificate cert, X509Chain ssl, SslPolicyErrors error) => (cert as X509Certificate2).Verify()));
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";
            byte[] bytes = Encoding.UTF8.GetBytes("a=auth&k=" + KEY + "&hwid=" + str);
            httpWebRequest.ContentLength = (long)bytes.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
            string json = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream()).ReadToEnd();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(json);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            if (dictionary.ContainsKey("error"))
            {
                Console.Write(DateTime.Now.ToString("[hh:mm:ss] "));
                Console.Write("| ");
                Console.Write(textInfo.ToTitleCase(dictionary["error"] as string) + ".\n\n");
                Console.ReadKey();
                if (File.Exists("key.cto"))
                {
                    File.Delete("key.cto");
                }
                Thread.Sleep(5000);
                Environment.Exit(0);
                return;
            }
            //Upgraded User Check Starts here.


            //I commented each usergroup id here simply if you wanna make the tool infinity+ for eg remove the premium rank id which is 11 from the list
            //For non upgraded auth please refer to the PlebAuth.cs
            if (!new string[]
            {
                "11", //this is premium id
                "12", // this is supreme id
                "93", // this is infinity id
                "96",
                "97",
                "99",
                "100",
                "101",
                "4",
                "3",
                "6",
                "94",
                "92"
            }.Any((string t) => t == dictionary["group"] as string))
            {
                Process.Start("http://cracked.io/upgrade.php");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("You have to be at least Premium+ to be able to use this tool. Press any key to continue to the forum page");
                if (File.Exists("key.cto"))
                {
                    File.Delete("key.cto");
                }
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            //Upgraded User Check Ends here.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Auth Granted. Welcome " + (dictionary["username"] as string) + "\n\n");
            File.WriteAllText("key.cto", KEY);
            Thread.Sleep(2000);
        }
    }
    public static class Json
    {
        public static object Deserialize(string json)
        {
            if (json == null)
            {
                return null;
            }
            return Json.Parser.Parse(json);
        }

        private sealed class Parser : IDisposable
        {
            public static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || "{}[],:\"".IndexOf(c) != -1;
            }

            private Parser(string jsonString)
            {
                this.json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString)
            {
                object result;
                using (Json.Parser parser = new Json.Parser(jsonString))
                {
                    result = parser.ParseValue();
                }
                return result;
            }

            public void Dispose()
            {
                this.json.Dispose();
                this.json = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                this.json.Read();
                for (; ; )
                {
                    Json.Parser.TOKEN nextToken = this.NextToken;
                    if (nextToken == Json.Parser.TOKEN.NONE)
                    {
                        goto IL_54;
                    }
                    if (nextToken == Json.Parser.TOKEN.CURLY_CLOSE)
                    {
                        break;
                    }
                    if (nextToken != Json.Parser.TOKEN.COMMA)
                    {
                        string text = this.ParseString();
                        if (text == null)
                        {
                            goto IL_56;
                        }
                        if (this.NextToken != Json.Parser.TOKEN.COLON)
                        {
                            goto IL_58;
                        }
                        this.json.Read();
                        dictionary[text] = this.ParseValue();
                    }
                }
                return dictionary;
            IL_54:
                return null;
            IL_56:
                return null;
            IL_58:
                return null;
            }

            private List<object> ParseArray()
            {
                List<object> list = new List<object>();
                this.json.Read();
                bool flag = true;
                while (flag)
                {
                    Json.Parser.TOKEN nextToken = this.NextToken;
                    if (nextToken == Json.Parser.TOKEN.NONE)
                    {
                        return null;
                    }
                    if (nextToken != Json.Parser.TOKEN.SQUARED_CLOSE)
                    {
                        if (nextToken != Json.Parser.TOKEN.COMMA)
                        {
                            object item = this.ParseByToken(nextToken);
                            list.Add(item);
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
                return list;
            }

            private object ParseValue()
            {
                Json.Parser.TOKEN nextToken = this.NextToken;
                return this.ParseByToken(nextToken);
            }

            private object ParseByToken(Json.Parser.TOKEN token)
            {
                switch (token)
                {
                    case Json.Parser.TOKEN.CURLY_OPEN:
                        return this.ParseObject();
                    case Json.Parser.TOKEN.SQUARED_OPEN:
                        return this.ParseArray();
                    case Json.Parser.TOKEN.STRING:
                        return this.ParseString();
                    case Json.Parser.TOKEN.NUMBER:
                        return this.ParseNumber();
                    case Json.Parser.TOKEN.TRUE:
                        return true;
                    case Json.Parser.TOKEN.FALSE:
                        return false;
                    case Json.Parser.TOKEN.NULL:
                        return null;
                }
                return null;
            }

            private string ParseString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                this.json.Read();
                bool flag = true;
                while (flag && this.json.Peek() != -1)
                {
                    char nextChar = this.NextChar;
                    if (nextChar != '"')
                    {
                        if (nextChar != '\\')
                        {
                            stringBuilder.Append(nextChar);
                        }
                        else if (this.json.Peek() == -1)
                        {
                            flag = false;
                        }
                        else
                        {
                            nextChar = this.NextChar;
                            if (nextChar <= '\\')
                            {
                                if (nextChar == '"' || nextChar == '/' || nextChar == '\\')
                                {
                                    stringBuilder.Append(nextChar);
                                }
                            }
                            else if (nextChar <= 'f')
                            {
                                if (nextChar != 'b')
                                {
                                    if (nextChar == 'f')
                                    {
                                        stringBuilder.Append('\f');
                                    }
                                }
                                else
                                {
                                    stringBuilder.Append('\b');
                                }
                            }
                            else if (nextChar != 'n')
                            {
                                switch (nextChar)
                                {
                                    case 'r':
                                        stringBuilder.Append('\r');
                                        break;
                                    case 't':
                                        stringBuilder.Append('\t');
                                        break;
                                    case 'u':
                                        {
                                            char[] array = new char[4];
                                            for (int i = 0; i < 4; i++)
                                            {
                                                array[i] = this.NextChar;
                                            }
                                            stringBuilder.Append((char)Convert.ToInt32(new string(array), 16));
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                stringBuilder.Append('\n');
                            }
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
                return stringBuilder.ToString();
            }

            private object ParseNumber()
            {
                string nextWord = this.NextWord;
                if (nextWord.IndexOf('.') == -1)
                {
                    long num;
                    long.TryParse(nextWord, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
                    return num;
                }
                double num2;
                double.TryParse(nextWord, NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
                return num2;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(this.PeekChar))
                {
                    this.json.Read();
                    if (this.json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private char PeekChar
            {
                get
                {
                    return Convert.ToChar(this.json.Peek());
                }
            }

            private char NextChar
            {
                get
                {
                    return Convert.ToChar(this.json.Read());
                }
            }

            private string NextWord
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    while (!Json.Parser.IsWordBreak(this.PeekChar))
                    {
                        stringBuilder.Append(this.NextChar);
                        if (this.json.Peek() == -1)
                        {
                            break;
                        }
                    }
                    return stringBuilder.ToString();
                }
            }

            private Json.Parser.TOKEN NextToken
            {
                get
                {
                    this.EatWhitespace();
                    if (this.json.Peek() == -1)
                    {
                        return Json.Parser.TOKEN.NONE;
                    }
                    char peekChar = this.PeekChar;
                    if (peekChar <= '[')
                    {
                        switch (peekChar)
                        {
                            case '"':
                                return Json.Parser.TOKEN.STRING;
                            case '#':
                            case '$':
                            case '%':
                            case '&':
                            case '\'':
                            case '(':
                            case ')':
                            case '*':
                            case '+':
                            case '.':
                            case '/':
                                break;
                            case ',':
                                this.json.Read();
                                return Json.Parser.TOKEN.COMMA;
                            case '-':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                return Json.Parser.TOKEN.NUMBER;
                            case ':':
                                return Json.Parser.TOKEN.COLON;
                            default:
                                if (peekChar == '[')
                                {
                                    return Json.Parser.TOKEN.SQUARED_OPEN;
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (peekChar == ']')
                        {
                            this.json.Read();
                            return Json.Parser.TOKEN.SQUARED_CLOSE;
                        }
                        if (peekChar == '{')
                        {
                            return Json.Parser.TOKEN.CURLY_OPEN;
                        }
                        if (peekChar == '}')
                        {
                            this.json.Read();
                            return Json.Parser.TOKEN.CURLY_CLOSE;
                        }
                    }
                    string nextWord = this.NextWord;
                    if (nextWord == "false")
                    {
                        return Json.Parser.TOKEN.FALSE;
                    }
                    if (nextWord == "true")
                    {
                        return Json.Parser.TOKEN.TRUE;
                    }
                    if (!(nextWord == "null"))
                    {
                        return Json.Parser.TOKEN.NONE;
                    }
                    return Json.Parser.TOKEN.NULL;
                }
            }

            private const string WORD_BREAK = "{}[],:\"";

            private StringReader json;

            private enum TOKEN
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }
        }
    }
}
