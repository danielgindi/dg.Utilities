using System.Collections.Generic;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace dg.Utilities
{
    public static class HttpRequestHelper
    {
        public static void EnableCompressionInResponseToRequest(HttpResponse response, HttpRequest request)
        {
            if ((request.Headers["Accept-Encoding"] ?? "").ToLower().Contains("gzip"))
            {
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                response.AppendHeader("Content-Encoding", "gzip");
            }
        }

        public static bool IsUserAgentFromMobileBrowser(string userAgent)
        {
            return Regex.IsMatch(userAgent, @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        }

        /// <summary>
        /// Find out if the request is from a user browser and not crawler. 
        /// Good to use when need to address users, not for addressing crawlers.
        /// </summary>
        public static bool IsNonCrawlerBrowser(string userAgent)
        {
            return !Regex.IsMatch(userAgent, @"bot|crawler|baiduspider|80legs|ia_archiver|voyager|curl|wget|yahoo! slurp|mediapartners-google", RegexOptions.IgnoreCase);
        }

        private static List<string> CrawlerBotList = null;
        private static List<string> CrawlerNonBotList = null;

        /// <summary>
        /// Find out if the request is from a crawler. 
        /// Good to use when need to address crawlers, not for addressing users.
        /// </summary>
        public static bool IsCrawlerBrowser(string userAgent)
        {
            if (CrawlerBotList == null)
            {
                CrawlerBotList = new List<string>() {
                    "googlebot","bingbot","yandexbot","ahrefsbot","msnbot","linkedinbot","exabot","compspybot",
                    "yesupbot","paperlibot","tweetmemebot","semrushbot","gigabot","voilabot","adsbot-google",
                    "botlink","alkalinebot","araybot","undrip bot","borg-bot","boxseabot","yodaobot","admedia bot",
                    "ezooms.bot","confuzzledbot","coolbot","internet cruiser robot","yolinkbot","diibot","musobot",
                    "dragonbot","elfinbot","wikiobot","twitterbot","contextad bot","hambot","iajabot","news bot",
                    "irobot","socialradarbot","ko_yappo_robot","skimbot","psbot","rixbot","seznambot","careerbot",
                    "simbot","solbot","mail.ru_bot","spiderbot","blekkobot","bitlybot","techbot","void-bot",
                    "vwbot_k","diffbot","friendfeedbot","archive.org_bot","woriobot","crystalsemanticsbot","wepbot",
                    "spbot","tweetedtimes bot","mj12bot","who.is bot","psbot","robot","jbot","bbot","bot"
                };

                CrawlerNonBotList = new List<string>() {
                    "baiduspider","80legs","baidu","yahoo! slurp","ia_archiver","mediapartners-google","lwp-trivial",
                    "nederland.zoek","ahoy","anthill","appie","arale","araneo","ariadne","atn_worldwide","atomz",
                    "bjaaland","ukonline","bspider","calif","christcrawler","combine","cosmos","cusco","cyberspyder",
                    "cydralspider","digger","grabber","downloadexpress","ecollector","ebiness","esculapio","esther",
                    "fastcrawler","felix ide","hamahakki","kit-fireball","fouineur","freecrawl","desertrealm",
                    "gammaspider","gcreep","golem","griffon","gromit","gulliver","gulper","whowhere","portalbspider",
                    "havindex","hotwired","htdig","ingrid","informant","infospiders","inspectorwww","iron33",
                    "jcrawler","teoma","ask jeeves","jeeves","image.kapsi.net","kdd-explorer","label-grabber",
                    "larbin","linkidator","linkwalker","lockon","logo_gif_crawler","marvin","mattie","mediafox",
                    "merzscope","nec-meshexplorer","mindcrawler","udmsearch","moget","motor","muncher","muninn",
                    "muscatferret","mwdsearch","sharp-info-agent","webmechanic","netscoop","newscan-online",
                    "objectssearch","orbsearch","packrat","pageboy","parasite","patric","pegasus","perlcrawler",
                    "phpdig","piltdownman","pimptrain","pjspider","plumtreewebaccessor","getterrobo-plus","raven",
                    "roadrunner","robbie","robocrawl","robofox","webbandit","scooter","search-au","searchprocess",
                    "senrigan","shagseeker","site valet","skymob","slcrawler","slurp","snooper","speedy",
                    "spider_monkey","spiderline","curl_image_client","suke","www.sygol.com","tach_bw","templeton",
                    "titin","topiclink","udmsearch","urlck","valkyrie libwww-perl","verticrawl","victoria",
                    "webscout","voyager","crawlpaper","wapspider","webcatcher","t-h-u-n-d-e-r-s-t-o-n-e",
                    "webmoose","pagesinventory","webquest","webreaper","webspider","webwalker","winona","occam",
                    "robi","fdse","jobo","rhcs","gazz","dwcp","yeti","crawler","fido","wlm","wolp","wwwc","xget",
                    "legs","curl","webs","wget","sift","cmc"
                };
            }

            userAgent = userAgent.ToLowerInvariant();

            if (userAgent.Contains("bot"))
            {
                return CrawlerBotList.Exists(x => userAgent.Contains(x));
            }
            else
            {
                return CrawlerNonBotList.Exists(x => userAgent.Contains(x));
            }
        }

        public static bool IsRequestFromMobileBrowser(HttpRequest request)
        {
            return IsUserAgentFromMobileBrowser(request.UserAgent);
        }

        public static bool IsCurrentRequestFromMobileBrowser()
        {
            return IsRequestFromMobileBrowser(HttpContext.Current.Request);
        }
    }
}
