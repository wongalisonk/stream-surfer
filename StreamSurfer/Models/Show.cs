﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class Show
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Picture { get; set; }
        public string Desc { get; set; }

        public List<Synonym> Synonyms { get; set; } 
        public List<ShowGenre> Genres { get; set; }
        public List<ShowService> ShowService { get; set; }
    }
}
