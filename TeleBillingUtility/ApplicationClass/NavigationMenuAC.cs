using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class NavigationMenuAC
	{
		public int ViewIndex { get;set;}
		public long ModuleId { get; set; }
		public string ModuleName { get; set; }
		public string IconName { get; set; }
		public string RouteLink { get; set; }
		public bool IsSinglePage { get; set; }
		public long ParentId { get; set; }
		public string Title { get; set; }
		public long LinkId { get;set; }
		public long LinkViewIndex { get;set; }
		public bool IsView { get; set; }
		public bool IsViewOnly { get; set; }
		public bool IsAdd { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }
		public bool IsChangeStatus { get; set; }
	}
}
