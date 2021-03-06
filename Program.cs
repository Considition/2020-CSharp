﻿using System;
using System.Linq;
using DotNet.models;


namespace DotNet
{
    public static class Program
    {
        private const string ApiKey = "";           // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "training1";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new GameLayer(ApiKey);

        public static void Main(string[] args)
        {
            var gameId = GameLayer.NewGame(Map);
            Console.WriteLine($"Starting game: {GameLayer.GetState().GameId}");
            GameLayer.StartGame(gameId);

            while (GameLayer.GetState().Turn < GameLayer.GetState().MaxTurns)
            {
                take_turn(gameId);
            }
            Console.WriteLine($"Done with game: {GameLayer.GetState().GameId}");
            Console.WriteLine(GameLayer.GetScore(gameId).FinalScore);
        }

        private static void take_turn(string gameId)
        {
            // TODO Implement your artificial intelligence here.
            // TODO Taking one action per turn until the game ends.
            // TODO The following is a short example of how to use the StarterKit
            var x = 0;
            var y = 0;
            var state = GameLayer.GetState();
            if (state.ResidenceBuildings.Count < 1)
            {
                for (var i = 0; i < 10; i++)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (state.Map[i][j] == 0)
                        {
                            x = i;
                            y = j;
                            break;
                        }
                    }
                }

                GameLayer.StartBuild(new Position(x, y), state.AvailableResidenceBuildings[0].BuildingName,
                    gameId);
            }

            else
            {
                var building = state.ResidenceBuildings[0];
                if (building.BuildProgress < 100)
                {
                    GameLayer.Build(building.Position, gameId);
                }
                else if (!building.Effects.Contains(state.AvailableUpgrades[0].Name))
                    GameLayer.BuyUpgrade(building.Position, state.AvailableUpgrades[0].Name, gameId);
                else if (building.Health < 50)
                {
                    GameLayer.Maintenance(building.Position, gameId);
                }

                else if (building.Temperature < 18)
                {
                    var bluePrint = GameLayer.GetResidenceBlueprint(building.BuildingName);
                    var energy = bluePrint.BaseEnergyNeed + (building.Temperature - state.CurrentTemp)
                        * bluePrint.Emissivity / 1 + 0.5 - building.CurrentPop * 0.04;
                    GameLayer.AdjustEnergy(building.Position, energy, gameId);
                }
                else if (building.Temperature > 24)
                {
                    var bluePrint = GameLayer.GetResidenceBlueprint(building.BuildingName);
                    var energy = bluePrint.BaseEnergyNeed + (building.Temperature - state.CurrentTemp)
                        * bluePrint.Emissivity / 1 - 0.5 - building.CurrentPop * 0.04;
                    GameLayer.AdjustEnergy(building.Position, energy, gameId);
                }
                else GameLayer.Wait(gameId);

                foreach (var message in GameLayer.GetState().Messages)
                {
                    Console.WriteLine(message);
                }

                foreach (var error in GameLayer.GetState().Errors)
                {
                    Console.WriteLine("Error: " + error);
                }
            }
        }
    }
}