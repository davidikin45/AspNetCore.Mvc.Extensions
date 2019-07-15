using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AspNetCore.Base.Localization
{
    public static class CurrencyHelper
    {
        public static IEnumerable<SelectListItem> CurrencyList()
        {
            return CurrenciesList.Select(c => new SelectListItem()
            {
                Value = c.ISOCurrencySymbol,
                Text = $"{c.ISOCurrencySymbol} – {c.CurrencySymbol}",
                //Selected = c.ISOCurrencySymbol == new RegionInfo(CultureInfo.CurrentCulture.LCID).ISOCurrencySymbol
            })
             .OrderBy(s => s.Text);
        }

        private static readonly Dictionary<string, Currency> CurrenciesByCode;
        private static readonly List<Currency> CurrenciesList;

        public static Currency GetCurrency(string ISOCurrencySymbol) { return CurrenciesByCode[ISOCurrencySymbol]; }

        static CurrencyHelper()
        {
            CurrenciesByCode = new Dictionary<string, Currency>();

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                          .Select(x => new RegionInfo(x.LCID));

            foreach (var region in regions)
            {
                if (!CurrenciesByCode.ContainsKey(region.ISOCurrencySymbol.ToUpper()))
                {
                    var currency = new Currency()
                    {
                        ISOCurrencySymbol = region.ISOCurrencySymbol.ToUpper(),
                        CurrencySymbol = region.CurrencySymbol,
                        CurrencyEnglishName = region.CurrencyEnglishName,
                        CurrencyNativeName = region.CurrencyNativeName
                    };

                    CurrenciesByCode.Add(region.ISOCurrencySymbol.ToUpper(), currency);
                }
            }

            CurrenciesList = CurrenciesByCode.Values.OrderBy(v => v.ISOCurrencySymbol).ToList();
        }

        public class Currency
        {
            public string ISOCurrencySymbol { get; set; }
            public string CurrencySymbol { get; set; }
            public string CurrencyEnglishName { get; set; }
            public string CurrencyNativeName { get; set; }

            public string Format(Decimal amount)
            {
                return CurrencyHelper.FormatWithISOCurrencySymbol(amount, ISOCurrencySymbol);
            }
        }

        public static string FormatWithISOCurrencySymbol(Decimal amount, string ISOCurrencySymbol = null)
        {
            NumberFormatInfo numberFormat = null;

            if(ISOCurrencySymbol != null)
            {
                numberFormat = (from c in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                 let r = new RegionInfo(c.LCID)
                 where r != null
                 && r.ISOCurrencySymbol.ToUpper() == ISOCurrencySymbol.ToUpper()
                 select c).First().NumberFormat;
            }
            else
            {
                numberFormat = CultureInfo.CurrentCulture.NumberFormat;
                var specificCulture = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
                ISOCurrencySymbol = new RegionInfo(specificCulture.LCID).ISOCurrencySymbol;
            }

            var numberFormatInfo = (NumberFormatInfo)numberFormat.Clone();
            numberFormatInfo.CurrencySymbol = ISOCurrencySymbol.ToUpper();

            // Add spaces between the figure and the currency 
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?redirectedfrom=MSDN&view=netframework-4.7.2#System_Globalization_NumberFormatInfo_CurrencyPositivePattern
            //n $
            numberFormatInfo.CurrencyPositivePattern = 3;
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencynegativepattern?redirectedfrom=MSDN&view=netframework-4.7.2#System_Globalization_NumberFormatInfo_CurrencyNegativePattern
            //-n $
            numberFormatInfo.CurrencyNegativePattern = 8;

            return amount.ToString("C", numberFormatInfo);
        }    
    }
}
