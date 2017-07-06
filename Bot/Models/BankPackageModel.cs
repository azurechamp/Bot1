using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    public class BankInfo
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string Term { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
    }

    public class BankPackageModel
    {
        public List<BankInfo> BankInfo { get; set; }
    }
}
