using System.Collections.Generic;

namespace dg.Utilities.Apns
{
    /// <summary>
    /// Alert Portion of the Notification Payload
    /// </summary>
    public class NotificationAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationAlert()
        {
            Title = null;
            Body = null;
            ActionLocalizedKey = null;
            TitleLocalizedKey = null;
            TitleLocalizedArgs = new List<object>();
            LocalizedKey = null;
            LocalizedArgs = new List<object>();
        }

        /// <summary>
        /// Title text of the Notification's Alert
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Body text of the Notification's Alert
        /// </summary>
        public string Body
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the launch image to use when app is opened through the remote notification
        /// </summary>
        public string LaunchImage
        {
            get;
            set;
        }

        /// <summary>
        /// Action Button's Localized Key
        /// </summary>
        public string ActionLocalizedKey
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Key
        /// </summary>
        public string LocalizedKey
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Argument List
        /// </summary>
        public List<object> LocalizedArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Key for Title
        /// </summary>
        public string TitleLocalizedKey
        {
            get;
            set;
        }

        /// <summary>
        /// Localized Argument List for Title
        /// </summary>
        public List<object> TitleLocalizedArgs
        {
            get;
            set;
        }

        public void AddLocalizedArgs(params object[] values)
        {
            this.LocalizedArgs.AddRange(values);
        }

        public void AddTitleLocalizedArgs(params object[] values)
        {
            this.TitleLocalizedArgs.AddRange(values);
        }

        /// <summary>
        /// Determines if the Alert is empty and should be excluded from the Notification Payload
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!string.IsNullOrEmpty(Title)
                    || !string.IsNullOrEmpty(TitleLocalizedKey)
                    || !string.IsNullOrEmpty(Body)
                    || !string.IsNullOrEmpty(LocalizedKey)
                    || !string.IsNullOrEmpty(ActionLocalizedKey)
                    || (LocalizedArgs != null && LocalizedArgs.Count > 0)
                    || (TitleLocalizedArgs != null && TitleLocalizedArgs.Count > 0))
                    return false;
                else
                    return true;
            }
        }
    }
}
