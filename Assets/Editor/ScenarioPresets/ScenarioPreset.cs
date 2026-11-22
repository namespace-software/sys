#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioTools.Presets
{
    public sealed class ScenarioPreset : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 5)] private string _description;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private List<ScenarioPresetReference> _externalReferences = new();

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public string Description => _description;
        public GameObject Prefab => _prefab;
        public IReadOnlyList<ScenarioPresetReference> ExternalReferences => _externalReferences;

        public void Initialize(string displayName, string description, GameObject prefab,
            List<ScenarioPresetReference> externalReferences)
        {
            _displayName = displayName;
            _description = description ?? string.Empty;
            _prefab = prefab;
            _externalReferences = externalReferences ?? new List<ScenarioPresetReference>();
        }
    }

    [Serializable]
    public sealed class ScenarioPresetReference
    {
        [SerializeField] private string _ownerPath;
        [SerializeField] private string _ownerComponentType;
        [SerializeField] private int _ownerComponentIndex;
        [SerializeField] private string _propertyPath;
        [SerializeField] private string _targetGlobalObjectId;
        [SerializeField] private string _targetScenePath;
        [SerializeField] private string _targetHierarchyPath;
        [SerializeField] private string _targetComponentType;
        [SerializeField] private int _targetComponentIndex;
        [SerializeField] private bool _targetIsGameObject;

        public string OwnerPath => _ownerPath;
        public string OwnerComponentType => _ownerComponentType;
        public int OwnerComponentIndex => _ownerComponentIndex;
        public string PropertyPath => _propertyPath;
        public string TargetGlobalObjectId => _targetGlobalObjectId;
        public string TargetScenePath => _targetScenePath;
        public string TargetHierarchyPath => _targetHierarchyPath;
        public string TargetComponentType => _targetComponentType;
        public int TargetComponentIndex => _targetComponentIndex;
        public bool TargetIsGameObject => _targetIsGameObject;

        public ScenarioPresetReference(string ownerPath, string ownerComponentType, int ownerComponentIndex,
            string propertyPath, string targetGlobalObjectId, string targetScenePath,
            string targetHierarchyPath, string targetComponentType, int targetComponentIndex,
            bool targetIsGameObject)
        {
            _ownerPath = ownerPath;
            _ownerComponentType = ownerComponentType;
            _ownerComponentIndex = ownerComponentIndex;
            _propertyPath = propertyPath;
            _targetGlobalObjectId = targetGlobalObjectId;
            _targetScenePath = targetScenePath;
            _targetHierarchyPath = targetHierarchyPath;
            _targetComponentType = targetComponentType;
            _targetComponentIndex = targetComponentIndex;
            _targetIsGameObject = targetIsGameObject;
        }
    }
}
#endif
