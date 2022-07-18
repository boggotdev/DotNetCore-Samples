using BOL;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tech.Models
{
    public class BatchModel
    {
        public int BatchId { get; set; }
        public bool ClosedBatch { get; set; }
        public List<Voucher> batchVouchers { get; set; } 
    }
    public class TicketsModel
    {
        public List<UserTask> Tickets { get; set; }
        public UserTask Ticket { get; set; }
        public OrganizationComplaint OrgComplaint { get; set; } 
        public List<SelectListItem> ComplaintOptions { get; set; }
        public string Message { get; set; }
    }
}
