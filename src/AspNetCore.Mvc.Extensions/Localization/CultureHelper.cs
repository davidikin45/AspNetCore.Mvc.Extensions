using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AspNetCore.Base.Localization
{
    public static class CultureHelper
    {
        public static IEnumerable<SelectListItem> NeutralCultureSelectList()
        {
            return NeutralCultureList().Select(c => new SelectListItem()
            {
                Value = c.Name,
                Text = c.DisplayName,
                Selected = c.Name == CultureInfo.CurrentCulture.Name
            })
             .OrderBy(s => s.Text);
        }

        //Neutral > Specific
        //Specific > Specific
        //https://stackoverflow.com/questions/986754/when-to-use-cultureinfo-getcultureinfostring-or-cultureinfo-createspecificcult
        //CultureInfo.CreateSpecificCulture("en")
        public static IEnumerable<Culture> NeutralCultureList()
        {
            return CultureInfo.GetCultures(CultureTypes.NeutralCultures).Select(c => new Culture()
            {
                Name = c.Name,
                DisplayName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en" ? c.DisplayName : $"{c.DisplayName} – {c.EnglishName}"
            })
             .OrderBy(s => s.DisplayName);
        }

        public static IEnumerable<SelectListItem> SpecificCultureSelectList()
        {
            return SpecificCultureList().Select(c => new SelectListItem()
            {
                Value = c.Name,
                Text = c.DisplayName,
                Selected = c.Name == CultureInfo.CurrentCulture.Name
            })
             .OrderBy(s => s.Text);
        }

        public static IEnumerable<Culture> SpecificCultureList()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => new Culture()
            {
                Name = c.Name,
                DisplayName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en" ? c.DisplayName : $"{c.DisplayName} – {c.EnglishName}"
            })
             .OrderBy(s => s.DisplayName);
        }

        public class Culture
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }

            public CultureInfo SpecificCulture
            {
                get
                {
                    return CultureInfo.CreateSpecificCulture(Name);
                }
            }

            public string DefaultISOCurrencySymbol
            {
                get
                {
                    return new RegionInfo(SpecificCulture.LCID).ISOCurrencySymbol;
                }
            }

        }
    }
}
