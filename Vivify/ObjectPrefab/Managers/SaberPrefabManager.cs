using System;
using System.Collections.Generic;
using Heck.ReLoad;
using JetBrains.Annotations;
using SiraUtil.Sabers;
using UnityEngine;
using Vivify.ObjectPrefab.Collections;
using Vivify.ObjectPrefab.Pools;
using Zenject;

namespace Vivify.ObjectPrefab.Managers;

internal class SaberPrefabManager : ILateTickable, IDisposable
{
    private readonly BeatmapObjectPrefabManager _beatmapObjectPrefabManager;
    private readonly ReLoader? _reLoader;
    private readonly Saber _saberA;
    private readonly Saber _saberB;
    private readonly SaberModelManager _saberModelManager;

    private readonly List<Func<bool>> _contracts = [];

    private SaberModelController? _saberModelControllerA;
    private SaberModelController? _saberModelControllerB;

    [UsedImplicitly]
    internal SaberPrefabManager(
        BeatmapObjectPrefabManager beatmapObjectPrefabManager,
        SaberManager saberManager,
        SaberModelManager saberModelManager,
        [InjectOptional] ReLoader? reLoader)
    {
        _beatmapObjectPrefabManager = beatmapObjectPrefabManager;
        _saberModelManager = saberModelManager;
        _saberA = saberManager.SaberForType(SaberType.SaberA);
        _saberB = saberManager.SaberForType(SaberType.SaberB);
        _reLoader = reLoader;
        if (reLoader != null)
        {
            reLoader.Rewinded += OnRewind;
        }

        SaberAPrefabs.Changed += OnSaberAChanged;
        SaberBPrefabs.Changed += OnSaberBChanged;
        SaberATrailMaterials.Changed += OnSaberATrailChanged;
        SaberBTrailMaterials.Changed += OnSaberBTrailChanged;
    }

    internal PrefabList SaberAPrefabs { get; } = new();

    internal TrailList SaberATrailMaterials { get; } = new();

    internal PrefabList SaberBPrefabs { get; } = new();

    internal TrailList SaberBTrailMaterials { get; } = new();

    private SaberModelController? SaberModelControllerA =>
        _saberModelControllerA ??= _saberModelManager.GetSaberModelController(_saberA);

    private SaberModelController? SaberModelControllerB =>
        _saberModelControllerB ??= _saberModelManager.GetSaberModelController(_saberB);

    public void Dispose()
    {
        if (_reLoader != null)
        {
            _reLoader.Rewinded -= OnRewind;
        }

        SaberAPrefabs.Changed -= OnSaberAChanged;
        SaberBPrefabs.Changed -= OnSaberBChanged;
        SaberATrailMaterials.Changed -= OnSaberATrailChanged;
        SaberBTrailMaterials.Changed -= OnSaberBTrailChanged;
    }

    public void LateTick()
    {
        for (int i = _contracts.Count - 1; i >= 0; i--)
        {
            if (!_contracts[i]())
            {
                // if one contract fails, then we can assume all the contracts will fail
                return;
            }

            _contracts.RemoveAt(i);
        }
    }

    private void OnRewind()
    {
        SaberAPrefabs.Clear();
        SaberBPrefabs.Clear();
    }

    private void CreateContract(Func<bool> contract)
    {
        if (!contract())
        {
            _contracts.Add(contract);
        }
    }

    private void OnSaberAChanged(float time)
    {
        CreateContract(() =>
        {
            if (SaberModelControllerA == null)
            {
                return false;
            }

            _beatmapObjectPrefabManager.Despawn(SaberModelControllerA);
            _beatmapObjectPrefabManager.Spawn<PrefabPool, GameObject>(SaberAPrefabs, SaberModelControllerA, time);
            return true;
        });
    }

    private void OnSaberATrailChanged(float time)
    {
        CreateContract(() =>
        {
            if (SaberModelControllerA == null)
            {
                return false;
            }

            _beatmapObjectPrefabManager.Despawn(SaberModelControllerA._saberTrail);
            _beatmapObjectPrefabManager.Spawn<TrailPool, FollowedSaberTrail>(
                SaberATrailMaterials,
                SaberModelControllerA._saberTrail,
                time);
            return true;
        });
    }

    private void OnSaberBChanged(float time)
    {
        CreateContract(() =>
        {
            if (SaberModelControllerB == null)
            {
                return false;
            }

            _beatmapObjectPrefabManager.Despawn(SaberModelControllerB);
            _beatmapObjectPrefabManager.Spawn<PrefabPool, GameObject>(SaberBPrefabs, SaberModelControllerB, time);
            return true;
        });
    }

    private void OnSaberBTrailChanged(float time)
    {
        CreateContract(() =>
        {
            if (SaberModelControllerB == null)
            {
                return false;
            }

            _beatmapObjectPrefabManager.Despawn(SaberModelControllerB._saberTrail);
            _beatmapObjectPrefabManager.Spawn<TrailPool, FollowedSaberTrail>(
                SaberBTrailMaterials,
                SaberModelControllerB._saberTrail,
                time);
            return true;
        });
    }
}
