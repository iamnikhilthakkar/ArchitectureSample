using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Architecture.Web.Helper
{
    public static class ExtensionHelper
    {
        public static List<SelectListItem> GetDistinctList<T>(this List<T> list, string keyName, string valueName)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var items = list.Select(s => new
            {
                key = s.GetType().GetProperty(keyName).GetValue(s, null).ToString(),
                value = s.GetType().GetProperty(valueName).GetValue(s, null).ToString()
            }).Distinct().ToList();

            listItems = items.Select(x => new SelectListItem
            {
                Text = x.key,
                Value = x.value
            }).ToList();

            return listItems.ToList();
        }
    }
}
