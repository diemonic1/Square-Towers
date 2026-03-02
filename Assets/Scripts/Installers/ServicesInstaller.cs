using System.Collections;
using System.Collections.Generic;
using Services.ConfigsService;
using Services.ConfigsService.ScriptableObjectsConfigs;
using Services.LocalizationService;
using Services.SavingService;
using UnityEditor;
using UnityEngine;
using Zenject;

public class ServicesInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InstallServices();
    }

    private void InstallServices()
    {
        Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ConfigsService>().AsSingle();
        Container.BindInterfacesAndSelfTo<SavingService>().AsSingle();
    }
}