using System;
using System.Collections.Generic;
using System.Linq;
using EphysLink;
using TMPro;
using TrajectoryPlanner;
using UnityEngine;
using UnityEngine.Serialization;

namespace Settings
{
    /// <summary>
    ///     Settings menu to connect to the Ephys Link server and manage probe-manipulator bindings.
    /// </summary>
    public class EphysLinkSettings : MonoBehaviour
    {
        #region Variables

        #region Serialized Fields

        // Server connection
        [FormerlySerializedAs("serverConnectedText")] [SerializeField] private TMP_Text _serverConnectedText;
        [FormerlySerializedAs("ipAddressInputField")] [SerializeField] private TMP_InputField _ipAddressInputField;
        [FormerlySerializedAs("portInputField")] [SerializeField] private TMP_InputField _portInputField;
        [FormerlySerializedAs("connectionErrorText")] [SerializeField] private TMP_Text _connectionErrorText;
        [FormerlySerializedAs("connectButtonText")] [SerializeField] private TMP_Text _connectButtonText;

        // Manipulators
        [FormerlySerializedAs("manipulatorList")] [SerializeField] private GameObject _manipulatorList;
        [FormerlySerializedAs("manipulatorConnectionPanelPrefab")] [SerializeField] private GameObject _manipulatorConnectionPanelPrefab;

        // Probes in scene
        [FormerlySerializedAs("probeList")] [SerializeField] private GameObject _probeList;
        [FormerlySerializedAs("probeConnectionPanelPrefab")] [SerializeField] private GameObject _probeConnectionPanelPrefab;

        #endregion

        #region Components

        private CommunicationManager _communicationManager;
        private TrajectoryPlannerManager _trajectoryPlannerManager;
        private TP_QuestionDialogue _questionDialogue;

        #endregion

        #region Session variables

        // private readonly Dictionary<int, Tuple<ManipulatorConnectionSettingsPanel, GameObject>>
        //     _manipulatorIdToManipulatorConnectionSettingsPanel = new();
        private readonly Dictionary<string, (ManipulatorConnectionSettingsPanel manipulatorConnectionSettingsPanel,
                GameObject gameObject)>
            _manipulatorIdToManipulatorConnectionSettingsPanel = new();

        private readonly Dictionary<int, (ProbeConnectionSettingsPanel probeConnectionSettingsPanel, GameObject
                gameObject)>
            _probeIdToProbeConnectionSettingsPanels = new();

        #endregion

        #endregion

        #region Unity

        private void Awake()
        {
            // Get Components
            _communicationManager = GameObject.Find("EphysLink").GetComponent<CommunicationManager>();
            _trajectoryPlannerManager = GameObject.Find("main").GetComponent<TrajectoryPlannerManager>();
            _questionDialogue = GameObject.Find("MainCanvas").transform.Find("QuestionDialoguePanel").gameObject
                .GetComponent<TP_QuestionDialogue>();
        }

        private void FixedUpdate()
        {
            // Update probe panels whenever they change
            if (_trajectoryPlannerManager.GetAllProbes().Count(manager => !manager.IsGhost) !=
                _probeIdToProbeConnectionSettingsPanels.Count)
                UpdateProbePanels();
        }

        private void OnEnable()
        {
            // Update UI elements every time the settings panel is opened
            UpdateProbePanels();
            UpdateConnectionUI();
        }

        #endregion

        #region UI Functions

        /// <summary>
        ///     Populate UI elements with current connection settings.
        /// </summary>
        private void UpdateConnectionUI()
        {
            // Connection UI
            _connectionErrorText.text = "";
            _connectButtonText.text = _communicationManager.IsConnected ? "Disconnect" : "Connect";
            _serverConnectedText.text =
                (_communicationManager.IsConnected ? "Connected" : "Connect") + " to server at";

            // Update available manipulators and their panels
            if (_communicationManager.IsConnected)
            {
                UpdateManipulatorPanelAndSelection();
            }
            else
            {
                // Clear manipulator panels if not connected
                foreach (var manipulatorPanel in
                         _manipulatorIdToManipulatorConnectionSettingsPanel.Values.Select(value => value.gameObject))
                    Destroy(manipulatorPanel);
                _manipulatorIdToManipulatorConnectionSettingsPanel.Clear();
            }
        }

        private void UpdateProbePanels()
        {
            var handledProbeIds = new HashSet<int>();

            // Add any new probes in scene to list
            foreach (var probeManager in _trajectoryPlannerManager.GetAllProbes().Where(manager => !manager.IsGhost))
            {
                var probeId = probeManager.GetID();

                // Create probe connection settings panel if the probe is new
                if (!_probeIdToProbeConnectionSettingsPanels.ContainsKey(probeId))
                {
                    var probeConnectionSettingsPanelGameObject =
                        Instantiate(_probeConnectionPanelPrefab, _probeList.transform);
                    var probeConnectionSettingsPanel =
                        probeConnectionSettingsPanelGameObject.GetComponent<ProbeConnectionSettingsPanel>();

                    probeConnectionSettingsPanel.SetProbeManager(probeManager);
                    probeConnectionSettingsPanel.EphysLinkSettings = this;

                    _probeIdToProbeConnectionSettingsPanels.Add(probeId,
                        new ValueTuple<ProbeConnectionSettingsPanel, GameObject>(probeConnectionSettingsPanel,
                            probeConnectionSettingsPanelGameObject));
                }
                else
                {
                    // Update probeManager in probe connection settings panel
                    _probeIdToProbeConnectionSettingsPanels[probeId].probeConnectionSettingsPanel
                        .SetProbeManager(probeManager);
                }

                handledProbeIds.Add(probeId);
            }

            // Remove any probe that is not in the scene anymore
            foreach (var removedProbeId in _probeIdToProbeConnectionSettingsPanels.Keys.Except(handledProbeIds)
                         .ToList())
            {
                Destroy(_probeIdToProbeConnectionSettingsPanels[removedProbeId].gameObject);
                _probeIdToProbeConnectionSettingsPanels.Remove(removedProbeId);
            }

            UpdateManipulatorPanelAndSelection();
        }

        /// <summary>
        ///     Updates the list of available manipulators to connect to and the selection options for probes.
        /// </summary>
        public void UpdateManipulatorPanelAndSelection()
        {
            _communicationManager.GetManipulators(availableIds =>
            {
                // Update probes with selectable options
                var usedManipulatorIds = _trajectoryPlannerManager.GetAllProbes()
                    .Where(probeManager => probeManager.IsEphysLinkControlled)
                    .Select(probeManager => probeManager.ManipulatorId).ToHashSet();
                foreach (var probeConnectionSettingsPanel in _probeIdToProbeConnectionSettingsPanels.Values.Select(
                             values => values.probeConnectionSettingsPanel))
                {
                    var manipulatorDropdownOptions = new List<string> { "-" };
                    manipulatorDropdownOptions.AddRange(availableIds.Where(id =>
                        id.Equals(probeConnectionSettingsPanel.ProbeManager.ManipulatorId) ||
                        !usedManipulatorIds.Contains(id)));

                    probeConnectionSettingsPanel.SetManipulatorIdDropdownOptions(manipulatorDropdownOptions);
                }

                // Handle manipulator panels
                var handledManipulatorIds = new HashSet<string>();

                // Add any new manipulators in scene to list
                foreach (var manipulatorId in availableIds)
                {
                    // Create new manipulator connection settings panel if the manipulator is new
                    if (!_manipulatorIdToManipulatorConnectionSettingsPanel.ContainsKey(manipulatorId))
                    {
                        var manipulatorConnectionSettingsPanelGameObject =
                            Instantiate(_manipulatorConnectionPanelPrefab, _manipulatorList.transform);
                        var manipulatorConnectionSettingsPanel =
                            manipulatorConnectionSettingsPanelGameObject
                                .GetComponent<ManipulatorConnectionSettingsPanel>();

                        manipulatorConnectionSettingsPanel.SetManipulatorId(manipulatorId);

                        _manipulatorIdToManipulatorConnectionSettingsPanel.Add(manipulatorId,
                            new ValueTuple<ManipulatorConnectionSettingsPanel, GameObject>(
                                manipulatorConnectionSettingsPanel, manipulatorConnectionSettingsPanelGameObject));
                    }

                    handledManipulatorIds.Add(manipulatorId);
                }

                // Remove any manipulators that are not connected anymore
                foreach (var disconnectedManipulator in _manipulatorIdToManipulatorConnectionSettingsPanel.Keys
                             .Except(handledManipulatorIds).ToList())
                {
                    Destroy(_manipulatorIdToManipulatorConnectionSettingsPanel[disconnectedManipulator].gameObject);
                    _manipulatorIdToManipulatorConnectionSettingsPanel.Remove(disconnectedManipulator);
                }
            });
        }

        /// <summary>
        ///     Handle when connect/disconnect button is pressed.
        /// </summary>
        public void OnConnectDisconnectPressed()
        {
            if (!_communicationManager.IsConnected)
            {
                // Attempt to connect to server
                try
                {
                    _serverConnectedText.text = "Connecting to server at";
                    _connectButtonText.text = "Connecting...";
                    _communicationManager.ConnectToServer(_ipAddressInputField.text, int.Parse(_portInputField.text),
                        UpdateConnectionUI, err =>
                        {
                            _serverConnectedText.text = "Connect to server at";
                            _connectionErrorText.text = err;
                            _connectButtonText.text = "Connect";
                        }
                    );
                }
                catch (Exception e)
                {
                    _connectionErrorText.text = e.Message;
                }
            }
            else
            {
                // Disconnect from server
                _questionDialogue.SetYesCallback(() =>
                {
                    foreach (var probeManager in _trajectoryPlannerManager.GetAllProbes()
                                 .Where(probeManager => probeManager.IsEphysLinkControlled))
                        probeManager.SetIsEphysLinkControlled(false);

                    _communicationManager.DisconnectFromServer(UpdateConnectionUI);
                });

                _questionDialogue.NewQuestion(
                    "Are you sure you want to disconnect?\nAll incomplete movements will be canceled.");
            }
        }

        #endregion
    }
}