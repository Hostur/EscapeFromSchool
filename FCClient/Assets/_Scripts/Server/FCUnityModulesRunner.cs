using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client;
using FCData;
using FCData.Attributes;
using FCData.Common;
using FCData.Network.Client;
using FCModules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 649

namespace Server
{
	public class FCUnityModulesRunner : FCBehaviour
	{
		[FCInject] private FCModulesRunner _modulesRunner;
		[FCInject] private IFCNetworkSettings _networkSettings;
		[FCInject] private FCMainThreadActionsQueue _mainThreadActionsQueue;
		[FCInject] private FCClientNetworkData _clientNetworkData;
		[Get] private CanvasScaler _canvasScaler;
		[Get] private GraphicRaycaster _graphicRaycaster;
		[SerializeField] private Color _activeColor;
		[SerializeField] private Color _inactiveColor;
		[SerializeField] private Image _background;
		[SerializeField] private Slider _slider;
		[SerializeField] [Range(0, 30)] private int _frameRate;
		[SerializeField] private Button _pauseButton;
		[SerializeField] private Button _playButton;
		[SerializeField] private Button _nextFrameButton;
		[SerializeField] private TMP_Text _frameResultLabel;
		[SerializeField] private ModuleRecord _moduleRecordPrefab;
		[SerializeField] private List<ModuleRecord> _spawnedModules;
		[SerializeField] private Transform _modulesGrid;

		private bool _running;

		public void EnableDisableModule(ModuleRecord record)
		{
			_modulesRunner.EnableDisableModule(record.Module.ModuleType);
			SpawnRecords();
		}

		private void SpawnRecords()
		{
			ClearRecords();
			var modules = _modulesRunner.GetAllModules();
			foreach (IFCModule fcModule in modules)
			{
				var spawned = Instantiate(_moduleRecordPrefab);
				spawned.transform.SetParent(_modulesGrid);
				spawned.Init(this, fcModule);
				_spawnedModules.Add(spawned);
				spawned.Refresh(fcModule.Enabled, 0, 0);
			}
		}

		private void ClearRecords()
		{
			foreach (var m in _spawnedModules)
			{
				Destroy(m.gameObject);
			}
			_spawnedModules.Clear();
		}

		protected override void OnAwake()
		{
			base.OnAwake();

			_pauseButton.onClick.AddListener(OnPauseButton);
			_playButton.onClick.AddListener(OnPlayButton);
			_nextFrameButton.onClick.AddListener(OnNextFrameButton);
			_slider.onValueChanged.AddListener(OnSliderChanged);
			Application.targetFrameRate = _networkSettings.FrameRate;
			_running = true;
#if !UNITY_EDITOR
			_canvasScaler.scaleFactor = 0;
			_graphicRaycaster.enabled = false;
#endif
			//_modulesRunner.SetFrameRate(_networkSettings.FrameRate);
			//_modulesRunner.Start();
			//InternalStartTicking();
			//RefreshButtons();
			//SpawnRecords();
			//Task.Factory.StartNew(async () => await Start().ConfigureAwait(false));
		}

		private void OnSuccessFullInit()
		{
			_modulesRunner.SetFrameRate(_networkSettings.FrameRate);
			_modulesRunner.Start();
			InternalStartTicking();
			RefreshButtons();
			SpawnRecords();
			Publish(new ModulesRunnerStartedEvent());
		}

		private async Task Start()
		{
			if (_networkSettings.SetupType == SetupType.StandaloneClient)
			{
				Destroy(gameObject);
				return;
			}

			bool result = await _modulesRunner.Init().ConfigureAwait(false);
			if (result)
			{
				_mainThreadActionsQueue.Enqueue(OnSuccessFullInit);
			}
			else
			{
				this.Log("Failed to init modules runner.");
			}
		}

		private void OnSliderChanged(float value)
		{
			int fRate = (int)value;
			_networkSettings.FrameRate = fRate;
			_frameRate = fRate;
			_modulesRunner.SetFrameRate(_frameRate);
			InternalStopTicking();
			InternalStartTicking();
		}

		private void RefreshButtons()
		{
			if (_networkSettings.FrameRate == 0)
			{
				_background.color = _inactiveColor;

				_pauseButton.interactable = false;
				_playButton.interactable = true;
				_nextFrameButton.interactable = true;
			}
			else
			{
				_background.color = _activeColor;

				_pauseButton.interactable = true;
				_playButton.interactable = false;
				_nextFrameButton.interactable = false;
			}
		}

		private void InternalStartTicking()
		{
			if (!_running || _networkSettings.FrameRate < 1) return;
			InvokeRepeating("Tick", 0, 1f / _networkSettings.FrameRate);
		}

		private void InternalStopTicking()
		{
			CancelInvoke("Tick");
		}

		private void OnPauseButton()
		{
			_running = false;
			_modulesRunner.Pause();
			InternalStopTicking();
			RefreshButtons();
		}

		private void OnPlayButton()
		{
			_running = true;
			_modulesRunner.Start();
			_modulesRunner.SetFrameRate(_frameRate);
			InternalStartTicking();
			RefreshButtons();
		}

		private void OnNextFrameButton()
		{
			FCFrameResult result = _modulesRunner.MoveByOneFrameForward().Result;
			_frameResultLabel.text = $"{result.Frame} : {result.FrameExecutionTimeInMilliseconds}";
			RefreshModules(result);
		}

		private void OnNextFrame(float time)
		{
			FCFrameResult result = _modulesRunner.TickAsync(time).Result;
			_frameResultLabel.text = $"{result.Frame} : {result.FrameExecutionTimeInMilliseconds}";
			RefreshModules(result);
		}

		private void RefreshModules(FCFrameResult result)
		{
			foreach (FCModuleFrameResult fcModuleFrameResult in result.Modules)
			{
				var record = _spawnedModules.FirstOrDefault(m => m.Module.ModuleType == fcModuleFrameResult.ModuleType);
				record?.Refresh(true, fcModuleFrameResult.ExecutionTimeInMilliseconds, fcModuleFrameResult.EntitiesHandled);
			}
		}

		private void Tick()
		{
			float time = 1F / _networkSettings.FrameRate;
			OnNextFrame(time);
		}

		[FCRegisterEventHandler(typeof(GameOverEvent))]
		private void OnGameOverEvent(object sender, EventArgs arg)
		{
			GameOverEvent e = arg as GameOverEvent;
			if (e.InternalId == _clientNetworkData.MyId)
			{
				OnPauseButton();
			}
		}

		private void OnApplicationQuit()
		{
			_modulesRunner.Dispose();
		}
	}
}
