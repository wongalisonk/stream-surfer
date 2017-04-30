﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace StreamSurfer.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public DateTime RegisterDate { get; set; }
    }
}
