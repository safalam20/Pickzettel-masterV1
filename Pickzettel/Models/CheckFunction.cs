using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Pickzettel.Models
{
    [Keyless]
    public class CheckFunction
    {
        public string USER_ELStatus { get; set; }
        public string SUser { get; set; }
        public string info { get; set; }

    }
}
