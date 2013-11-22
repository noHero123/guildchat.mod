using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace guildchat.mod
{
    class Cryptomat
    {
        // standart key :D
        private string key = ",2%l8SN5*yFDer3%VcosXF,NUuF9Y45v§mO_g81Xw?knaz!TjO,3G;vBczLbCTX0$Ubf_s:lo-XbIByWgmGG4b(CLSts=kfl*SK5j1hlAAO?uOUNDW-w-u6m%6jr8__$Pc,;l1Ck8jN-6zWwt§!X;m3!=pvtk+,#LQtxb)m(A_IsXi9EvJ+q4,HetSp5b*_!W#XTyY=;:-Qvo*=Fj3DFjZK1M?yC*fEYasqQlE:KVD3guy%Q=1-,PGm,rP(jGFi*Z60MM*FkFZDc3K07Wd(J!JOwrayu3*pkC+j)AdjDVJE+DAaEFdEJsZ0IuLcZoVYD+L§mple(%*§Yu5)6zL8imoQV_xK8($YpVrda!Axc+c.rb=Et)-TH6,cc%+5M9K1T96s.k§mep:;yzf9WK!xa:Oa3hfPTeJK?M§X6NC!zoj0:izM%t=,9u=wNimcV4sZ+mnGT§(r4x_sck:czupQxV8YW$1+$$voUtbt4qeni)giEvq$96:*h06gzD?D;z=+)a$q)J+Dp,N16wbD;GRiloE.rQ,C,eox2lU!ICwNtbA1.cRR$Pd§$jyO(-MS)3f(obK$(._,J_6;0b4QB#;O2m0MIk";
        
        public void setkey(string k)
        {
            this.key=k;
        }

        public string Encrypt(string text)
        {
            if (text.StartsWith("\\g "))
            {
                string txt = text.Substring(3);

                string result= ".g " + EncodeTo64(EncryptOrDecrypt(txt, this.key));
                string result1 = ",g " + Deflate(EncodeTo64(EncryptOrDecrypt(txt, this.key)));
                if(result.Length <= result1.Length)                       
                    return result;
                return result1;
            }
            if (text.StartsWith("/g "))
            {
                string txt = text.Substring(3);
                string result = ".g " + EncodeTo64(EncryptOrDecrypt(txt, this.key));
                string result1 = ",g " + Deflate(EncodeTo64(EncryptOrDecrypt(txt, this.key)));
                if (result.Length <= result1.Length)
                    return result;
                return result1;
            }
            return "";
        }

         public string Decrypt(string text)
        {
            if (text.StartsWith(".g "))
            {
                string txt = text.Substring(3);
                return "" + EncryptOrDecrypt(DecodeFrom64(txt), this.key);
            }
            if (text.StartsWith(",g "))
            {
                string txt = text.Substring(3);
                return "" + EncryptOrDecrypt(Inflate(DecodeFrom64(txt)), this.key);
            }
            return "";

        }

        public string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.UTF8.GetBytes(toEncode);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.UTF8.GetString(encodedDataAsBytes);

            return returnValue;

        }

        string EncryptOrDecrypt(string text, string key)
        {
            var result = new StringBuilder();
            for (int c = 0; c < text.Length; c++)
            {
                result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));
            }

            return result.ToString();
        }


        // make string large to orginal size
        public string Inflate(string input)
        {
            byte[] inputData = System.Convert.FromBase64String(input);
            Inflater inflater = new Inflater(false);
            using (var inputStream = new MemoryStream(inputData))
            using (var ms = new MemoryStream())
            {
                var inputBuffer = new byte[4096];
                var outputBuffer = new byte[4096];

                while (inputStream.Position < inputData.Length)
                {
                    var read = inputStream.Read(inputBuffer, 0, inputBuffer.Length);

                    inflater.SetInput(inputBuffer, 0, read);

                    while (inflater.IsNeedingInput == false)
                    {
                        var written = inflater.Inflate(outputBuffer, 0, outputBuffer.Length);

                        if (written == 0)
                            break;

                        ms.Write(outputBuffer, 0, written);
                    }

                    if (inflater.IsFinished == true)
                        break;
                }

                inflater.Reset();

                return Convert.ToBase64String( ms.ToArray());
            }
        }

        // make string smaller (or try it at least ;)
        public string Deflate(string input)
        {
            byte[] inputData = Convert.FromBase64String(input);
            Deflater deflater = new Deflater(Deflater.BEST_SPEED, false);
            deflater.SetInput(inputData);
            deflater.Finish();

            using (var ms = new MemoryStream())
            {
                var outputBuffer = new byte[65536 * 4];
                while (deflater.IsNeedingInput == false)
                {
                    var read = deflater.Deflate(outputBuffer);
                    ms.Write(outputBuffer, 0, read);

                    if (deflater.IsFinished == true)
                        break;
                }

                deflater.Reset();

                return Convert.ToBase64String( ms.ToArray());
            }
        }



    }
}
