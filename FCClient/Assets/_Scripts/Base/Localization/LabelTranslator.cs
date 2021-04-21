using System;
using System.Collections.Generic;
using FCData.Attributes;
using FCData.Common;
using TMPro;
using UnityEngine;

#pragma warning disable 649

namespace Localization
{
	public class LabelTranslator : FCBehaviour
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private string _key;
		[SerializeField] private string _postfix;
		[SerializeField] private bool _upperCase;

		[SerializeField] private List<string> _parameters;

		[FCInject]
		private GameTranslator _gameTranslator;

		protected override void OnAwake()
		{
			base.OnAwake();
			Refresh();
		}

		private void Refresh()
		{
			if (!string.IsNullOrEmpty(_postfix))
				_label.text = _upperCase ? _gameTranslator.Translate(_key, _postfix).ToUpper() : _gameTranslator.Translate(_key);
			else
				_label.text = _upperCase ? _gameTranslator.Translate(_key).ToUpper() : _gameTranslator.Translate(_key);

			ReplaceParameters();
		}

		private void ReplaceParameters()
		{
			try
			{
				if (_parameters != null && _parameters.Count > 0)
					_label.text = string.Format(_label.text, _parameters);
			}
			catch (Exception e)
			{
				this.Log($"Translation parameters replacing failed on LabelTranslator attached to {gameObject.name}. \n{e}", LogLevel.Error);
			}
		}



		[FCRegisterEventHandler(typeof(GameTranslator.LanguageReloadedEvent))]
		private void OnLanguageReloadedEvent(object sender, EventArgs arg) => Refresh();
		private void Reset() => _label = GetComponent<TMP_Text>();
	}
}