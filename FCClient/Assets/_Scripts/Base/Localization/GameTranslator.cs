using System;
using System.Collections.Generic;
using FCData.Attributes;
using FCData.Common;
using FCData.DataUtils;
using FCData.Events;

namespace Localization
{
	[FCRegister(true)]
	public class GameTranslator
	{
		private const char MARKUP = '$';
		private Dictionary<string, string> _dictionary;
		private readonly ScriptableLanguageDictionary _languageDictionary;

		public GameTranslator()
		{
			_languageDictionary =
				UnityEngine.Resources.Load<ScriptableLanguageDictionary>("Balance/LanguageDictionary");
			ReloadDictionary();
			FCGameEventsManager.Subscribe<LanguagesManager.AfterLanguageChangedEvent>(OnReloadLanguageEvent);
		}

		private void ReloadDictionary()
		{
			_dictionary = _languageDictionary.GetDictionary(LanguagesManager.CurrentLanguage);
			FCGameEventsManager.Publish(this, new LanguageReloadedEvent());
		}

		public string Translate(string key)
		{
			int markups = CountTranslationMarkups(ref key);
			if (markups > 0)
				return TranslateWithMarkups(ref key, markups);

			return _dictionary.GetIfExists(key) ?? key;
		}

		private string TranslateWithMarkups(ref string content, int markups)
		{
			for (int i = 0; i < markups; i++)
			{
				// Find index of translatable 
				var indexOfFirstMarkup = content.IndexOf(MARKUP);
				if (indexOfFirstMarkup == -1) return content;

				var tmpIndex = indexOfFirstMarkup;
				int wordLength = 0;
				// incement wordLength until we reach empty character.

				while (++tmpIndex < content.Length && (content[tmpIndex] != ' ' && content[tmpIndex] != ']' &&
				                                       content[tmpIndex] != '[' && content[tmpIndex] != '\n' &&
				                                       content[tmpIndex] != '*'))
					++wordLength;

				var worldToTranslate = content.Substring(indexOfFirstMarkup + 1, wordLength);
				string translatedContent = _dictionary.GetIfExists(worldToTranslate) ?? worldToTranslate;
				content = content.Replace($"${worldToTranslate}", translatedContent);
			}

			return content.Replace("*", "");
		}

		public string Translate(string key, string postfix)
		{
			return Translate(key) + postfix;
		}

		public string Translate(string key, params object[] param)
		{
			try
			{
				if (param != null && param.Length > 0)
				{
					return string.Format(Translate(key), param);
				}

				return key;
			}
			catch (Exception e)
			{
				this.Log($"Translation parameters replacing failed. \n{e}", LogLevel.Error);
				return key;
			}
		}

		private int CountTranslationMarkups(ref string content)
		{
			int result = 0;
			for (int i = 0; i < content.Length; i++)
				if (content[i] == MARKUP)
					++result;

			return result;
		}

		private void OnReloadLanguageEvent(object sender, EventArgs arg)
		{
			ReloadDictionary();
		}

		public class LanguageReloadedEvent : FCGameEvent
		{
		}
	}
}