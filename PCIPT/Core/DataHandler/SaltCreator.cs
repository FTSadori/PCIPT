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

        public string GetSalt(int length)
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append((char)random.Next(32, 127));
            }
            return stringBuilder.ToString();
        }
    }
}
