using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;

namespace TeleBillingUtility.Helpers.CommonFunction
{
    public static class CommonFunction
    {

        public static string GetDescriptionFromEnumValue(Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DescriptionAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DescriptionAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }


        public static List<DrpResponseAC> GetFixServiceList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.Mobility), Name = EnumList.ServiceType.Mobility.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.VoiceOnly), Name = EnumList.ServiceType.VoiceOnly.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.InternetService), Name = EnumList.ServiceType.InternetService.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.DataCenterFacility), Name = EnumList.ServiceType.DataCenterFacility.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.ManagedHostingService), Name = EnumList.ServiceType.ManagedHostingService.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.StaticIP), Name = EnumList.ServiceType.StaticIP.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.MOC), Name = EnumList.ServiceType.MOC.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.GeneralServiceMada), Name = EnumList.ServiceType.GeneralServiceMada.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.GeneralserviceKems), Name = EnumList.ServiceType.GeneralserviceKems.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.LandLine), Name = EnumList.ServiceType.LandLine.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.ServiceType.InternetPlanDeviceOffer), Name = EnumList.ServiceType.InternetPlanDeviceOffer.ToString() });
            return lst;
        }


		public static string GetRandomColorFromList()
		{
			var random = new Random();
			var listOfColors = new List<string> { "#8ad876", "#e91e63", "#03a9f4", "ff5722", "d8c30a", "#00800", "#1B3F8B", "#97694F", "#99182C", "#A74CAB", "#DB9EA6", "#E6B426" , "#EE00EE" ,"#EE7621", "#8A8A8A", "#8c7373" };
			int index = random.Next(listOfColors.Count);
			return listOfColors[index];
		}

        #region --> Service Name 
        public static string GetServiceName(long serviceid)
        {
            if (serviceid > 0)
            {
                switch (serviceid)
                {
                    case 1:
                        return "Mobility";
                    case 2:
                        return "Voice Only";
                    case 3:
                        return "Internet Service";
                    case 4:
                        return "Data Center Facility";
                    case 5:
                        return "Managed Hosting Service";
                    case 6:
                        return "Static IP";
                    case 7:
                        return "VOIP";
                    case 8:
                        return "MOC";
                    case 9:
                        return "General Service Mada";
                    case 10:
                        return "General Service Kems";
                    case 11:
                        return "LandLine";
                    case 12:
                        return "Internet Plan Device Offer";
                    default:
                        return "service";
                }

            }
            return string.Empty;
        }
        #endregion
    }
}
