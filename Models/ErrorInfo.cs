using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class ErrorInfoViewModel
    {
        public string ErrorMessage { get; set; }
        public int Refresh { get; set; }

        public ErrorInfoViewModel() 
        {
            // default refresh value to very high (i.e. don't refresh)
            Refresh = 1000;
        }
    }
}