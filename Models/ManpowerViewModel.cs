using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace ExLibris.JiraExtensions.Models
{
    public class ManpowerViewModel
    {
        public DataTable AllocatedWork { get; set; }
        public DataTable NonAllocatedWork { get; set; }
    }
}