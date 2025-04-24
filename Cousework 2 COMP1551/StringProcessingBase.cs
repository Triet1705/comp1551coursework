using System;
using System.Linq;
using System.Text;

namespace Cousework_2_COMP1551
{
    public abstract class StringProcessingBase
    {
        private string inputString;
        private int inputNumber;
        protected string encodedString;
        private const int maxLength = 40;

        public StringProcessingBase()
        {
        }

        public string InputString
        {
            get { return inputString; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length > maxLength || !value.All(char.IsUpper))
                {
                    throw new ArgumentException($"Input string must not be empty, have a maximum length of {maxLength}, and contain only uppercase letters.");
                }
                inputString = value;
            }
        }

        public int InputNumber
        {
            get { return inputNumber; }
            set
            {
                if (value < -25 || value > 25)
                {
                    throw new ArgumentException($"Input number must be between -25 and 25, Your input is {value}.");
                }
                inputNumber = value;
            }
        }

        protected string EncodedString
        {
            get { return encodedString; }
            set { encodedString = value; }
        }

        public abstract void Encode();

        public virtual string Print()
        {
            return encodedString;
        }

        public int[] InputCode()
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return new int[0];
            }
            return inputString.Select(c => (int)c).ToArray();
        }

        public virtual int[] OutputCode()
        {
            if (string.IsNullOrEmpty(encodedString))
            {
                return new int[0];
            }
            return encodedString.Select(c => (int)c).ToArray();
        }

        public string Sort()
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }
            return new string(inputString.OrderBy(c => c).ToArray());
        }
    }

    public class CaesarProcessor : StringProcessingBase
    {
        public CaesarProcessor() : base()
        {
        }

        public override void Encode()
        {
            if (string.IsNullOrEmpty(InputString))
            {
                EncodedString = "";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in InputString)
            {
                int charCode = (int)c;
                int shiftedCharCode = charCode + InputNumber;

                if (shiftedCharCode > 90)
                {
                    shiftedCharCode = 65 + (shiftedCharCode - 91);
                }
                else if (shiftedCharCode < 65)
                {
                    shiftedCharCode = 90 - (64 - shiftedCharCode);
                }

                sb.Append((char)shiftedCharCode);
            }
            EncodedString = sb.ToString();
        }
    }

    public class AtbashProcessor : StringProcessingBase
    {
        public AtbashProcessor() : base()
        {
        }

        public override void Encode()
        {
            if (string.IsNullOrEmpty(InputString))
            {
                EncodedString = "";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in InputString)
            {
                int shiftedCharCode = 'Z' - (c - 'A');
                sb.Append((char)shiftedCharCode);
            }
            EncodedString = sb.ToString();
        }
    }

    public class AdjacentSwapProcessor : StringProcessingBase
    {
        public AdjacentSwapProcessor() : base()
        {
        }

        public override void Encode()
        {
            if (string.IsNullOrEmpty(InputString))
            {
                EncodedString = "";
                return;
            }

            char[] chars = InputString.ToCharArray();
            for (int i = 0; i < chars.Length - 1; i += 2)
            {
                char temp = chars[i];
                chars[i] = chars[i + 1];
                chars[i + 1] = temp;
            }
            EncodedString = new string(chars);
        }
    }
}