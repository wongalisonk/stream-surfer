using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StreamSurfer.Models;
using Microsoft.EntityFrameworkCore;
using StreamSurfer.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace StreamSurfer.Controllers
{
    public class ShowController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;

        public ShowController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger)
        {
            this._context = context;
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var show = await _context.Shows
                .Include(m => m.ShowService)
                .Include(m => m.ShowGenre)
                .SingleOrDefaultAsync(m => m.ID == id);
            var loadGenres = await _context.Genres
                .Include(m => m.ShowGenre)
                .ToListAsync();
            var loadServices = await _context.Services
                .Include(m => m.ShowService)
                .ToListAsync();
            //switch cast to string separated by ;
            if (show == null)
            {
                //get show details
                var response = await webRequest.Get(showService.ConvertToDetail(id.Value));
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                // may want to come up with a new way to get these values out.
                // instead of pulling values out manually
                List<Synonym> synonyms = json["alternate_titles"]
                    .Children()
                    .Select(x => new Synonym() { ShowID = id.Value, Title = x.ToString() })
                    .ToList();
                List<Genre> genres = json["genres"]
                    .Children()
                    .Select(x => new Genre() {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Title = (string)JObject.Parse(x.ToString())["title"]
                    })
                    .ToList();
                List<string> cast = json["cast"]
                    .Children()
                    .Select(x => (string)JObject.Parse(x.ToString())["name"])
                    .ToList();
                string castString = "No cast available.";
                if (cast.Count > 0)
                {
                    castString = cast[0];
                    cast.Remove(castString);
                    foreach (var str in cast)
                    {
                        castString = castString + ";" + str;
                    }
                }

                //get service details
                var serviceResponse = await webRequest.Get(showService.ConvertToServices(id.Value));
                if (!serviceResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
                var serviceJson = JObject.Parse(serviceContent);
                // TODO: support more than just web links (such as ios + android)
                List<Service> services = serviceJson["results"]["web"]["episodes"]["all_sources"]
                    .Children()
                    .Select(x => new Service()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Name = (string)JObject.Parse(x.ToString())["display_name"]
                    })
                    .ToList();

                //get episodes
                var episodeResponse = await webRequest.Get(showService.GetEpisodes(id.Value, 1, 0));
                if (!episodeResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var episodeContent = await episodeResponse.Content.ReadAsStringAsync();
                var episodeJson = JObject.Parse(episodeContent);

                Dictionary<string, string> freeWeb = episodeJson["results"][0]["free_web_sources"]
                .Children()
                .ToDictionary(x => (string)JObject.Parse(x.ToString())["display_name"], y => (string)JObject.Parse(y.ToString())["link"]);

                Dictionary<string, string> tvEverywhereWeb = episodeJson["results"][0]["tv_everywhere_web_sources"]
                .Children()
                .ToDictionary(x => (string)JObject.Parse(x.ToString())["display_name"], y => (string)JObject.Parse(y.ToString())["link"]);

                Dictionary<string, string> subscriptionWeb = episodeJson["results"][0]["subscription_web_sources"]
                .Children()
                .ToDictionary(x => (string)JObject.Parse(x.ToString())["display_name"], y => (string)JObject.Parse(y.ToString())["link"]);

                Dictionary<string, string> purchaseWeb = episodeJson["results"][0]["purchase_web_sources"]
                .Children()
                .ToDictionary(x => (string)JObject.Parse(x.ToString())["display_name"], y => (string)JObject.Parse(y.ToString())["link"]);

                List<ShowService> showServices = new List<ShowService>();
                foreach (var service in services)
                {
                    Service getService = _context.Services.SingleOrDefault(s => s.ID == service.ID);
                    if (getService == null)
                    {
                        getService = service;
                    }
                    string link = "";
                    if (freeWeb.ContainsKey(getService.Name))
                    {
                        link = freeWeb[getService.Name];
                    }
                    else if (tvEverywhereWeb.ContainsKey(getService.Name))
                    {
                        link = tvEverywhereWeb[getService.Name];
                    }
                    else if (subscriptionWeb.ContainsKey(getService.Name))
                    {
                        link = subscriptionWeb[getService.Name];
                    }
                    else if (purchaseWeb.ContainsKey(getService.Name))
                    {
                        link = purchaseWeb[getService.Name];
                    }
                    showServices.Add(new ShowService(id.Value, getService.ID, null, getService, link));
                }
                List<ShowGenre> showGenres = new List<ShowGenre>();
                foreach (var genre in genres)
                {
                    Genre getGenre = _context.Genres.SingleOrDefault(g => g.ID == genre.ID);
                    if (getGenre == null)
                    {
                        getGenre = genre;
                    }
                    showGenres.Add(new ShowGenre(id.Value, getGenre.ID, null, getGenre));
                }
                show = new Show()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Picture = (string)json["poster"],
                    Desc = (string)json["overview"],
                    Started = json["first_aired"].ToString().Substring(0, 4),
                    Rating = (string)json["rating"],
                    Cast = castString,
                    Synonyms = synonyms,
                    ShowGenre = showGenres,
                    ShowService = showServices
                };
                _context.Add(show);
                _context.SaveChanges();
            }
            return View(show);
        }
    }
}