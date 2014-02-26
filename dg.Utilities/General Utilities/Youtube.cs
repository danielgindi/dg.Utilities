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
            Match match = Regex.Match(url, @"\/\/www\.youtube\.com\/(watch\?v=|v\/|embed\/)([^&/]+)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase);
            if (match != null && match.Groups.Count > 2)
            {
                return match.Groups[2].Value;
            }
            return null;
        }

        /// <summary>
        /// Generates the maximum resolution image url for a youtube-id
        /// </summary>
        public static string GetYoutubeMaxResDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/maxresdefault.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the 1st default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail0ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/default.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the 2nd default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail1ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/1.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the 3rd default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail2ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/2.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the 4th default low-res image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeThumbnail3ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/3.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the default high-quality image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeHqDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/hqdefault.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the default medium-quality image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeMqDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/mqdefault.jpg", httpRooted ? @"http:" : "", youtubeId);
        }

        /// <summary>
        /// Generates the default standard-definition image thumbnail url for a youtube-id
        /// </summary>
        public static string GetYoutubeSdDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return string.Format(@"{0}//img.youtube.com/vi/{1}/sddefault.jpg", httpRooted ? @"http:" : "", youtubeId);
        }
    }
}
