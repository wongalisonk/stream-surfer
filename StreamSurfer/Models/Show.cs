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
        public string Poster { get; set; }
        public string Artwork { get; set; }
        public string Desc { get; set; }
        public string Started { get; set; }
        public string Rating { get; set; }
        public string Cast { get; set; }

        public List<Synonym> Synonyms { get; set; } 
        public List<ShowGenre> ShowGenre { get; set; }
        public List<ShowService> ShowService { get; set; }
        public List<MyListShows> MyListShows { get; set; }
    }
}
