using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Domain.ValueObjects
{
    public class MultiLanguageString : Dictionary<string, string>
    {
        private readonly string _defaultLanguage;
        public MultiLanguageString(string defaultLanguage = "en")
        {
            _defaultLanguage = defaultLanguage;
            this.Add(_defaultLanguage, null);
        }

        public string Value()
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (ContainsKey(culture))
            {
                return this[culture];
            }
            else if (ContainsKey(language))
            {
                return this[language];
            }
            else
            {
                return this[_defaultLanguage];
            }
        }

        public override string ToString()
        {
            return Value();
        }
    }
}
