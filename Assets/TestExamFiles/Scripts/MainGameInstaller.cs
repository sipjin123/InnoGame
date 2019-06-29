using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class MainGameInstaller : MonoInstaller
{
    [SerializeField]
    private GridHandler _GridHandler;

    [SerializeField]
    private UIBuildingHandler _UIBuildingHandler;

    [SerializeField]
    private BuildingPoolHandler _BuildingPoolHandler;

    [SerializeField]
    private UIResourceHandler _UIResourceHandler;

    [SerializeField]
    private Camera _Camera;

    public override void InstallBindings()
    {
        this.Container.Bind<GridHandler>()
                .FromInstance(_GridHandler)
                .AsSingle()
                .NonLazy();
        this.Container.Bind<BuildingPoolHandler>()
                .FromInstance(_BuildingPoolHandler)
                .AsSingle()
                .NonLazy();
        this.Container.Bind<UIBuildingHandler>()
                .FromInstance(_UIBuildingHandler)
                .AsSingle()
                .NonLazy();
        this.Container.Bind<UIResourceHandler>()
                .FromInstance(_UIResourceHandler)
                .AsSingle()
                .NonLazy();

        this.Container.Bind<Camera>()
                .FromInstance(_Camera)
                .AsSingle()
                .NonLazy();
    }
}
