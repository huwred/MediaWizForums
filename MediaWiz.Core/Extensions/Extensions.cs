using System;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;

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

        [Obsolete("GetDictionaryItemOrDefault is deprecated, please use GetOrCreateDictionaryValue instead.")]
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

        /// <summary>
        /// GetOrCreateDictionaryValue.
        /// Fetches a value from the dictionary or creates a new entry using the supplied value if none exists
        /// </summary>
        /// <param name="localizationService"></param>
        /// <param name="key">Dictionary Key</param>
        /// <param name="defaultValue">Value to use if not in dictionary</param>
        /// <param name="isoCode">Override the CurrentUI language</param>
        /// <returns></returns>
        public static string GetOrCreateDictionaryValue(this ILocalizationService localizationService, string key, string defaultValue,string isoCode = null)
        {
            var languageCode = isoCode ?? System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (languageCode.StartsWith("en_"))
            {
                languageCode = "en";
            }
            var dictionaryItem = localizationService.GetDictionaryItemByKey(key) ?? key.Split('.').Aggregate((IDictionaryItem)null, (item, part) =>
            {
                var partKey = item is null ? part : $"{item.ItemKey}.{part}";
                
                return localizationService.GetDictionaryItemByKey(partKey) ?? localizationService.CreateDictionaryItemWithIdentity(partKey, item?.Key, partKey.Equals(key) ? defaultValue : string.Empty);
            });
            var currentValue = dictionaryItem.Translations?.FirstOrDefault(it => it.Language.IsoCode == languageCode);
            if (!string.IsNullOrWhiteSpace(currentValue?.Value))
                return currentValue.Value;
            return $"[{key}]";
        }

    }
}
