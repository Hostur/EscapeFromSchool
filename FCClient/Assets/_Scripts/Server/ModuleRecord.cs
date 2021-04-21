using FCModules;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace Server
{
	public class ModuleRecord : MonoBehaviour
	{
		[SerializeField] private Text _moduleName;
		[SerializeField] private Toggle _turnOnOfButton;
		[SerializeField] private Text _executionTime;
		[SerializeField] private Text _handledEntities;
		[SerializeField] private Image _background;
		private FCUnityModulesRunner _clientModuleRunner;
		public IFCModule Module { get; private set; }

		public void Init(FCUnityModulesRunner modulesRunner, IFCModule module)
		{
			_moduleName.text = module.ModuleType.ToString();
			_clientModuleRunner = modulesRunner;
			Module = module;
			_turnOnOfButton.isOn = module.Enabled;
			_turnOnOfButton.onValueChanged.AddListener(OnToggleChanged);
		}

		private void OnToggleChanged(bool enabled)
		{
			_clientModuleRunner.EnableDisableModule(this);
		}

		public void Refresh(bool active, int executionTime, int touchedEntities)
		{
			if (active)
			{
				_background.color = Color.green;
				_executionTime.text = ((float)executionTime / 1000).ToString();
				_handledEntities.text = touchedEntities.ToString();
			}
			else
			{
				_background.color = Color.red;
				_executionTime.text = string.Empty;
				_handledEntities.text = string.Empty;
			}
		}
	}
}