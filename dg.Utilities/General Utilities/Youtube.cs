using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace dg.Utilities
{
    public static class Youtube
    {
        /// <summary>
        /// Retrieves the Youtube ID from a youtube URL
        /// </summary>
        public static string GetYoutubeId(string url)
        {
            Match match = Regex.Match(url, "^https?\\:\\/\\/www\\.youtube\\.com\\/(watch\\?v=|v\\/|embed\\/)([^&/]+)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase);
            if (match != null && match.Groups.Count > 2)
            {
                return match.Groups[2].Value;
            }
            return null;
        }

        /// <summary>
        /// Generates the maximum resolution image url for a youtube-id
        /// </summary>
        public static string GetYoutubeMaxResDefaultImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/maxresdefault.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the 1st default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail0ImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/default.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the 2nd default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail1ImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/1.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the 3rd default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail2ImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/2.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the 4th default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail3ImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/3.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the default high-quality image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeHqDefaultImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/hqdefault.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the default medium-quality image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeMqDefaultImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/mqdefault.jpg", youtubeId);
        }

        /// <summary>
        /// Generates the default standard-definition image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeSdDefaultImageUrl(string youtubeId)
        {
            return string.Format(@"http://img.youtube.com/vi/{0}/sddefault.jpg", youtubeId);
        }
    }
}
