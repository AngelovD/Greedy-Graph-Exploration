// Copyright 2024 MAES
// 
// This file is part of MAES
// 
// MAES is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the
// Free Software Foundation, either version 3 of the License, or (at your option)
// any later version.
// 
// MAES is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General
// Public License for more details.
// 
// You should have received a copy of the GNU General Public License along
// with MAES. If not, see http://www.gnu.org/licenses/.
// 
// Contributors: Rasmus Borrisholt Schmidt, Andreas Sebastian Sørensen, Thor Beregaard, Malte Z. Andreasen, Philip I. Holler and Magnus K. Jensen,
// 
// Original repository: https://github.com/Molitany/MAES

using System;
using Maes.ExplorationAlgorithm.Minotaur;
using System.Collections;
using Maes.ExplorationAlgorithm.TheNextFrontier;
using Maes.Map;
using Maes.Map.MapGen;
using Maes.Robot;
using Maes.Utilities.Files;
using UnityEngine;
using Maes.Robot;
using Maes.ExplorationAlgorithm.Movement;
using System.Collections.Generic;
using Maes.UI;
using UnityEditor;
using System.Linq;
using Maes.ExplorationAlgorithm.Greed;
using Maes.ExplorationAlgorithm.GreedG;

namespace Maes
{
    using Maes.ExplorationAlgorithm.GreedG;
    internal class ExampleProgram : MonoBehaviour
    {
        private Simulator _simulator;
        /*
*/
        private void Start()
        {
            const int randomSeed = 12345;

            var constraintsDict = new Dictionary<string, RobotConstraints>();

            //var constraintsGlobalCommunication = new RobotConstraints(
            constraintsDict["Global"] = new RobotConstraints(
                senseNearbyAgentsRange: 5f,
                senseNearbyAgentsBlockedByWalls: true,
                automaticallyUpdateSlam: true,
                slamUpdateIntervalInTicks: 1,
                slamSynchronizeIntervalInTicks: 10,
                slamPositionInaccuracy: 0.2f,
                distributeSlam: false,
                environmentTagReadRange: 4.0f,
                slamRayTraceRange: 7f,
                relativeMoveSpeed: 1f,
                agentRelativeSize: 0.6f,
                calculateSignalTransmissionProbability: (distanceTravelled, distanceThroughWalls) =>
                {
                    return true;
                }
            );

            //var constraintsMaterials = new RobotConstraints(
            constraintsDict["Material"] = new RobotConstraints(
                senseNearbyAgentsRange: 5f,
                senseNearbyAgentsBlockedByWalls: true,
                automaticallyUpdateSlam: true,
                slamUpdateIntervalInTicks: 1,
                slamSynchronizeIntervalInTicks: 10,
                slamPositionInaccuracy: 0.2f,
                distributeSlam: false,
                environmentTagReadRange: 4.0f,
                slamRayTraceRange: 7f,
                relativeMoveSpeed: 1f,
                agentRelativeSize: 0.6f,
                materialCommunication: true
            );

            //var constraintsLOS = new RobotConstraints(
            constraintsDict["LOS"] = new RobotConstraints(
                senseNearbyAgentsRange: 5f,
                senseNearbyAgentsBlockedByWalls: true,
                automaticallyUpdateSlam: true,
                slamUpdateIntervalInTicks: 1,
                slamSynchronizeIntervalInTicks: 10,
                slamPositionInaccuracy: 0.2f,
                distributeSlam: false,
                environmentTagReadRange: 4.0f,
                slamRayTraceRange: 7f,
                relativeMoveSpeed: 1f,
                agentRelativeSize: 0.6f,
                calculateSignalTransmissionProbability: (distanceTravelled, distanceThroughWalls) =>
                {
                    // Blocked by walls
                    if (distanceThroughWalls > 0)
                    {
                        return false;
                    }
                    return true;
                }
            );

            var simulator = Simulator.GetInstance();
            var random = new System.Random(1234);
            List<int> rand_numbers = new List<int>();
            for (int i = 0; i < 1; i++)
            {
                var val = random.Next(0, 1000000);
                rand_numbers.Add(val);
            }

            var constraintName = "Global";
            var robotConstraints = constraintsDict[constraintName];

            var buildingConfigList50 = new List<BuildingMapConfig>();
            var buildingConfigList100 = new List<BuildingMapConfig>();
            var buildingConfigList200 = new List<BuildingMapConfig>();
            foreach (int val in rand_numbers)
            {
                //buildingConfigList100.Add(new BuildingMapConfig(val, widthInTiles: 100, heightInTiles: 100, maxHallInPercent: 30));
                buildingConfigList200.Add(new BuildingMapConfig(val, widthInTiles: 250, heightInTiles: 250, maxHallInPercent: 30));
            }

            var algorithms = new Dictionary<string, RobotSpawner.CreateAlgorithmDelegate>
                {
                    { "greedG_simple", (seed) => new GreedGAlgorithm(0) },
                    { "greedG_util", (seed) => new GreedGAlgorithm(1) },
                    { "greedG_EuclidSimple", (seed) => new GreedGAlgorithm(2) },
                    { "greedG_EuclidPath", (seed) => new GreedGAlgorithm(3) },
                    { "greedG_ManhattanSimple", (seed) => new GreedGAlgorithm(4) },
                    { "greedG_ManhattanPath", (seed) => new GreedGAlgorithm(5) }
                };
            var buildingMaps = buildingConfigList100.Union(buildingConfigList200);
            foreach (var mapConfig in buildingConfigList200)
            {
                for (var amountOfRobots = 4; amountOfRobots < 10; amountOfRobots += 3)
                {
                    var robotCount = amountOfRobots;

                    foreach (var (algorithmName, algorithm) in algorithms)
                    {


                        //$"{algorithmName}-seed-{mapConfig.RandomSeed}-size-{size}-comms-{constraintName}-robots-{robotCount}-SpawnTogether"
                        simulator.EnqueueScenario(new SimulationScenario(seed: 123,
                                                                         mapSpawner: generator => generator.GenerateMap(mapConfig),
                                                                         robotSpawner: (buildingConfig, spawner) => spawner.SpawnRobotsTogether(
                                                                             buildingConfig,
                                                                             seed: 123,
                                                                             numberOfRobots: robotCount,
                                                                             suggestedStartingPoint: new Vector2Int(random.Next(0, mapConfig.WidthInTiles), random.Next(0, mapConfig.WidthInTiles)),
                                                                             createAlgorithmDelegate: algorithm),
                                                                         statisticsFileName: $"{algorithmName}-seed-{mapConfig.RandomSeed}-size-{mapConfig.WidthInTiles}--robots-{robotCount}",
                                                                         robotConstraints: robotConstraints)
                        );


                    }
                }
            }
        

            var algorithms1 = new Dictionary<string, RobotSpawner.CreateAlgorithmDelegate>
                    {
                        //{ "tnf", seed => new TnfExplorationAlgorithm(1, 10, seed) },
                        { "minotaur", seed => new MinotaurAlgorithm(robotConstraints, seed, 2) },
                        { "greed", seed => new GreedAlgorithm() }
                    };

            foreach (var mapConfig in buildingConfigList200)
            {
                for (var amountOfRobots = 4; amountOfRobots< 10; amountOfRobots += 3)
                {
                    var robotCount = amountOfRobots;

                    foreach (var (algorithmName, algorithm) in algorithms1)
                    {


                        //$"{algorithmName}-seed-{mapConfig.RandomSeed}-size-{size}-comms-{constraintName}-robots-{robotCount}-SpawnTogether"
                        simulator.EnqueueScenario(new SimulationScenario(seed: 123,
                                                                            mapSpawner: generator => generator.GenerateMap(mapConfig),
                                                                            robotSpawner: (buildingConfig, spawner) => spawner.SpawnRobotsTogether(
                                                                                buildingConfig,
                                                                                seed: 123,
                                                                                numberOfRobots: robotCount,
                                                                                suggestedStartingPoint: new Vector2Int(random.Next(0, mapConfig.WidthInTiles), random.Next(0, mapConfig.WidthInTiles)),
                                                                                createAlgorithmDelegate: algorithm),
                                                                            statisticsFileName: $"{algorithmName}-seed-{mapConfig.RandomSeed}-size-{mapConfig.WidthInTiles}--robots-{robotCount}",
                                                                            robotConstraints: robotConstraints)
                        );


                    }
                }
            }
            
        

            //Just code to make sure we don't get too many maps of the last one in the experiment
            var dumpMap = new BuildingMapConfig(-1, widthInTiles: 50, heightInTiles: 50);
            simulator.EnqueueScenario(new SimulationScenario(seed: 123,
                mapSpawner: generator => generator.GenerateMap(dumpMap),
                robotSpawner: (buildingConfig, spawner) => spawner.SpawnRobotsTogether(
                                                                 buildingConfig,
                                                                 seed: 123,
                                                                 numberOfRobots: 5,
                                                                 suggestedStartingPoint: Vector2Int.zero,
                                                                 createAlgorithmDelegate: (seed) => new MinotaurAlgorithm(robotConstraints, seed, 2)),
                statisticsFileName: $"delete-me",
                robotConstraints: robotConstraints));

            simulator.PressPlayButton(); // Instantly enter play mode

            //simulator.GetSimulationManager().AttemptSetPlayState(SimulationPlayState.FastAsPossible);
        }

    }
}
