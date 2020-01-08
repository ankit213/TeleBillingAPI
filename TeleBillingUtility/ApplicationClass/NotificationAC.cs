using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class NotificationAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("subject")]
        public string NotificationText { get; set; }

        [JsonProperty("actionusername")]
        public string ActionUserName { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("createddate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("notificationtypeid")]
        public long NotificationTypeId { get; set; }
    }

    public class NotificationObjAC
    {
        [JsonProperty("listofnotification")]
        public List<NotificationAC> listOfNotification { get; set; }

        [JsonProperty("remainingnotificationcount")]
        public long RemainingNotificationCount { get; set; }
    }
}
