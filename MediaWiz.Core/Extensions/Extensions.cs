using System;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace MediaWiz.Forums.Extensions
{
    public static class Extensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static string GetDictionaryItemOrDefault(this ILocalizationService ls, string Key, string Default)
        {
            IDictionaryItem dictionaryItem = ls.GetDictionaryItemByKey("Forums.ForumUrl");
            if (dictionaryItem == null)
            {
                return Default;

            }

            IDictionaryTranslation translation = dictionaryItem.Translations.FirstOrDefault(x => x.LanguageId == 1);
            return translation.Value;
        }
    }
}
