using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantShop.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateOfBuying { get; set; }
        public string Gender { get; set; }
        public bool IsActive { get; set; }
        public int TotalPrice { get; set; }
        public string CategoryTitle { get; set; }
        public string ImagePath { get; set; }
    }
}
