using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Text;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace Xamrap_Crypto
{

    [Activity(MainLauncher = true)]


    public class MainActivity : Activity
    {
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource

            SetContentView(Resource.Layout.Main);
            
            // Get our button from the layout resource,
            // and attach an event to it
            Button encodeButton = FindViewById<Button>(Resource.Id.EncodeBttn);
            Button decodeButton = FindViewById<Button>(Resource.Id.DecodeBttn);
            Button copyButton = FindViewById<Button>(Resource.Id.copyBttn);
            Button pasteButton = FindViewById<Button>(Resource.Id.pasteBttn);
            Button clearButton = FindViewById<Button>(Resource.Id.clearBttn);
            TextView outputBox = FindViewById<TextView>(Resource.Id.OutputBox);
            TextView messageBox = FindViewById<TextView>(Resource.Id.MessageText);
            TextView keyBox = FindViewById<TextView>(Resource.Id.KeyNumber);
            RadioGroup rbGroup1 = FindViewById<RadioGroup>(Resource.Id.radioGroup1);
            RadioButton rbSimpleSub = FindViewById<RadioButton>(Resource.Id.rbSimpleSub);
            RadioButton rbComplexSub = FindViewById<RadioButton>(Resource.Id.rbComplexSub);
            RadioButton rbRijndael = FindViewById<RadioButton>(Resource.Id.rbRijndael);
            


            rbGroup1.CheckedChange += delegate
            {
                keyBox.Text = "";

                if (rbSimpleSub.Checked)
                    keyBox.InputType = InputTypes.ClassNumber;
                else
                    keyBox.InputType = InputTypes.ClassText;
            };



            encodeButton.Click += delegate
            {
                if (rbSimpleSub.Checked)
                {
                    int key = 0;

                    if (int.TryParse(keyBox.Text, out key))
                    {
                        outputBox.Text = SimpleSubCypherEncode(key, messageBox.Text);
                    }
                    else
                    {
                        Toast.MakeText(this, "Key must be an integer", ToastLength.Long).Show();
                    }
                }
                else if (rbComplexSub.Checked)
                {
                
                    if (keyBox.Text != "" && HasLetters(keyBox.Text))
                        outputBox.Text = ComplexSubCypherEncode(keyBox.Text, messageBox.Text);
                    else
                        Toast.MakeText(this, "Key must contain letters", ToastLength.Long).Show();
                }
                else
                {
                    outputBox.Text = rijndaelEncode(messageBox.Text, keyBox.Text);
                }
            };

            decodeButton.Click += delegate
            {
                if (rbSimpleSub.Checked)
                {
                    int key = 0;

                    if (int.TryParse(keyBox.Text, out key))
                    {
                        outputBox.Text = SimpleSubCypherDecode(key, messageBox.Text);
                    }
                    else
                    {
                        Toast.MakeText(this, "Key must be an integer", ToastLength.Long).Show();
                    }
                }
                else if (rbComplexSub.Checked)
                {

                    if (keyBox.Text != "" && HasLetters(keyBox.Text))
                        outputBox.Text = ComplexSubCypherDecode(keyBox.Text, messageBox.Text);
                    else
                        Toast.MakeText(this, "Key must contain letters", ToastLength.Long).Show();
                }
                else
                {
                    outputBox.Text = rijndaelDecode(messageBox.Text, keyBox.Text);
                }

            };

            copyButton.Click += delegate
            {
                var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                ClipData clip = ClipData.NewPlainText("encoded message", outputBox.Text);
                clipboard.PrimaryClip = clip;
            };

            pasteButton.Click += delegate
            {
                var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);

                

                if (!(clipboard.HasPrimaryClip))
                {
                    // If it does not contain data
                    Toast.MakeText(this, "Clipboard Empty", ToastLength.Long).Show();

                }
                else if (!(clipboard.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain)))
                {

                    // since the clipboard has data but it is not plain text
                    Toast.MakeText(this, "Clipboard does not contain text", ToastLength.Long).Show();

                }
                else
                {
                    //since the clipboard contains plain text.
                    var item = clipboard.PrimaryClip.GetItemAt(0);

                    // Gets the clipboard as text.
                    messageBox.Text = item.Text;
                }

                
            };

            clearButton.Click += delegate
            {
                messageBox.Text = "";
                keyBox.Text = "";
                outputBox.Text = "";

                //var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                //ClipData clip = ClipData.NewPlainText("Clipboard Empty", "");
                //clipboard.PrimaryClip = clip;

                Toast.MakeText(this, "Clearing", ToastLength.Short).Show();

             

            };
        }

        private string SimpleSubCypherEncode(int key, string message)
        {
            Toast.MakeText(this, "Encoding", ToastLength.Long).Show();
            char[] chars = message.ToCharArray();
            int count = 0;
            key = Math.Abs(key);

            //reduce key till it is less than 26
            while (key > 26)
            {
                key -= 26;
            }

            //apply key to each letter
            foreach (char thisChar in chars)
            {
                int intChar = Convert.ToInt16(chars[count]);

                // dont mess with funny chars
                if (intChar >= 32)
                {
                    chars[count] = (char)(intChar + key);
                    int newIntChar = Convert.ToInt16(chars[count]);

                    if (newIntChar > 126)
                        chars[count] = (char)(newIntChar - 95);

                    //insures that letters rollover consistently z => a
                    //if (newIntChar > 122 || (newIntChar > 90 && newIntChar < 97))
                    // chars[count] = (char)(newIntChar - 26);
                }
                count++;
            }
            string codedMessage = new string(chars);
            
            return codedMessage;
        }

        private string SimpleSubCypherDecode(int key, string message)
        {
            Toast.MakeText(this, "Decoding", ToastLength.Long).Show();
            char[] chars = message.ToCharArray();
            int count = 0;
            key = Math.Abs(key);

            //reduce key till it is less than 26
            while (key > 26)
            {
                key -= 26;
            }

            //apply key to each letter
            foreach (char thisChar in chars)
            {
                int intChar = Convert.ToInt16(chars[count]);

                // dont mess with funny chars
                if (intChar >= 32)
                {
                    chars[count] = (char)(intChar - key);
                    int newIntChar = Convert.ToInt16(chars[count]);

                    if (newIntChar < 32)
                        chars[count] = (char)(newIntChar + 95);
                }
                count++;
            }
            string codedMessage = new string(chars);
            return codedMessage;
        }

        public string ComplexSubCypherEncode(string key, string message)
        {
            string codedMessage = "no message";
            try
            {



                Toast.MakeText(this, "Encoding", ToastLength.Short).Show();
                StringBuilder sb = new StringBuilder();
                foreach (char c in key)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        sb.Append(c);
                    }
                }
                key = sb.ToString();

                string formattedKey = key.ToLower();



                char[] messageChars = message.ToCharArray();
                char[] keyChars = formattedKey.ToCharArray();
                int messageCount = 0;
                int keyCount = 0;



               // Toast.MakeText(this, formattedKey, ToastLength.Long).Show();
               // Toast.MakeText(this, keyChars.Length.ToString() + " key length", ToastLength.Long).Show();

                //apply key to each letter
                foreach (char thisChar in messageChars)
                {
                    if (keyCount > keyChars.Length - 1)
                        keyCount = 0;
                    int intChar = Convert.ToInt16(messageChars[messageCount]);
                    int intKey = Convert.ToInt16(keyChars[keyCount])-96;
                    //Toast.MakeText(this, intKey.ToString() + " encoded", ToastLength.Short).Show();

                    // dont mess with funny chars
                    if (intChar >= 32)
                    {
                        messageChars[messageCount] = (char)(intChar + intKey);
                        int newIntChar = Convert.ToInt16(messageChars[messageCount]);

                        if (newIntChar > 126)
                            messageChars[messageCount] = (char)(newIntChar - 95);
                        
                    }
                    messageCount++;
                    keyCount++;
                }
                codedMessage = new string(messageChars);

                return codedMessage;

            }
            catch (Exception ex)
            {
               
                return ex.ToString();
            }
        }

        public string ComplexSubCypherDecode(string key, string message)
        {
            string decodedMessage = "no message";
            try
            {



                Toast.MakeText(this, "Encoding", ToastLength.Short).Show();
                StringBuilder sb = new StringBuilder();
                foreach (char c in key)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        sb.Append(c);
                    }
                }
                key = sb.ToString();

                string formattedKey = key.ToLower();



                char[] messageChars = message.ToCharArray();
                char[] keyChars = formattedKey.ToCharArray();
                int messageCount = 0;
                int keyCount = 0;


                //apply key to each letter
                foreach (char thisChar in messageChars)
                {
                    if (keyCount > keyChars.Length - 1)
                        keyCount = 0;
                    int intChar = Convert.ToInt16(messageChars[messageCount]);
                    int intKey = Convert.ToInt16(keyChars[keyCount]) - 96;
                   
                    // dont mess with funny chars
                    if (intChar >= 32)
                    {
                        messageChars[messageCount] = (char)(intChar - intKey);
                        int newIntChar = Convert.ToInt16(messageChars[messageCount]);

                        if (newIntChar < 32)
                            messageChars[messageCount] = (char)(newIntChar + 95);

                    }
                    messageCount++;
                    keyCount++;
                }
                decodedMessage = new string(messageChars);

                return decodedMessage;

            }
            catch (Exception ex)
            {

                return ex.ToString();
            }
        }
        public static bool HasLetters(string s)
        {
            foreach (char c in s)
            {
                if (Char.IsLetter(c))
                    return true;
            }
            return false;
        }

        private const string initVector = "pewgail9uzpgql88";
        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;
        //Encrypt
        public static string rijndaelEncode(string message, string key)
        {
            try {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(message);

                PasswordDeriveBytes password = new PasswordDeriveBytes(key, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch (Exception Ex)
            {
                return Ex.ToString();
            }
               
        }
        //Decrypt 
        public static string rijndaelDecode(string message, string key)
        {
            try
            {
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] cipherTextBytes = Convert.FromBase64String(message);
                PasswordDeriveBytes password = new PasswordDeriveBytes(key, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            catch(Exception Ex)
            {
                return Ex.ToString();
            }
            
        }

    }


}

