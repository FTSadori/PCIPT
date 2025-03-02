using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PCIPT.Core.DataHandler
{
    public sealed class SaltCreator
    {
        Random random;

        public SaltCreator(Random random)
        {
            this.random = random;
        }

        public string GetSalt(int length, bool onlyNumsAndDigits = false)
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < length; i++)
            {
                if (onlyNumsAndDigits)
                {
                    char c;
                    do
                    {
                        c = (char)random.Next(48, 122);
                    } while (!Char.IsLetterOrDigit(c));
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append((char)random.Next(32, 127));
                }
            }
            return stringBuilder.ToString();
        }
    }
}
