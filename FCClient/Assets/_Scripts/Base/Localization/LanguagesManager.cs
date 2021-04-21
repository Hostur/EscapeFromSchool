using FCData.Attributes;
using FCData.Events;
using UnityEngine;

namespace Localization
{
  public class LanguagesManager : FCBehaviour
  {
    private const string KEY_GAME_LANGUAGE = "keyGameLanguage";
    public static SystemLanguage CurrentLanguage => SystemLanguage.English;//(SystemLanguage)PlayerPrefs.GetInt(KEY_GAME_LANGUAGE, (int)(Application.systemLanguage == SystemLanguage.Polish ? SystemLanguage.Polish : SystemLanguage.English));
    public static string CurrentLanguageShortString => CurrentLanguage == SystemLanguage.Polish ? "pl" : "en";


    [FCRegisterEventHandler(typeof(SetGameLanguageEvent))]
	private void OnSetGameLanguageEvent(SetGameLanguageEvent e)
    {
      PlayerPrefs.SetInt(KEY_GAME_LANGUAGE, (int)e.Language);
      PlayerPrefs.Save();
      Publish(new AfterLanguageChangedEvent(e.Language));
    }

    public class SetGameLanguageEvent : FCGameEvent
    {
      public SystemLanguage Language { get; }
      public SetGameLanguageEvent(SystemLanguage language) => Language = language;
    }

    public class AfterLanguageChangedEvent : FCGameEvent
    {
      public SystemLanguage Language { get; }
      public AfterLanguageChangedEvent(SystemLanguage language) => Language = language;
    }
  }
}
