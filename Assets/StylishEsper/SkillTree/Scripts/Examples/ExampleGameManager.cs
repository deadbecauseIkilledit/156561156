//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Example game manager.
//***************************************************************************************

using Esper.SkillTree.Stats;
using Esper.SkillTree.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Esper.SkillTree.Examples
{
    [DefaultExecutionOrder(-1)]
    public class ExampleGameManager : MonoBehaviour
    {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState state;

        /// <summary>
        /// The player's stats.
        /// </summary>
        [SerializeField]
        private UnitStatsProvider player;

        /// <summary>
        /// The player character controller.
        /// </summary>
        public ExamplePlatformerCharacterController playerController;

        [SerializeField]
        private ExampleEnemySpawner spawnerLeft;

        [SerializeField]
        private ExampleEnemySpawner spawnerRight;

        [SerializeField]
        private TextMeshProUGUI waveTextMesh;

        [SerializeField]
        private TextMeshProUGUI timerTextMesh;

        [SerializeField]
        private float waveDelay = 6f;

        [SerializeField]
        private float enemySpawnDelay = 1.5f;

        [SerializeField]
        private int waveEnemyAmountScaling = 3;

        public AudioSource uiAudioSource;

        private List<ExampleEnemyAI> spawnedEnemies = new();

        private float timeRemaining;

        private int wave;

        private int spawnAmountMax;

        private int spawnedAmount;

        /// <summary>
        /// The player status UI.
        /// </summary>
        public ExampleStatusUI statusUI { get; private set; }

        /// <summary>
        /// Game data instance.
        /// </summary>
        public static ExampleGameManager instance { get; private set; }

        /// <summary>
        /// If the game is currently paused.
        /// </summary>
        public bool isPaused { get => state == GameState.Paused; }

        /// <summary>
        /// If any window is enabled.
        /// </summary>
        public bool anyWindowsOpen { get => DefaultSkillTreeWindow.instance.isOpen; }

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
            }

            statusUI = FindFirstObjectByType<ExampleStatusUI>(FindObjectsInactive.Include);

            // Initialize skill tree
            SkillTree.Initialize();
            SkillTree.SetPlayerLevel(1);
            SkillTree.playerStats = player;
        }

        private void OnDestroy()
        {
            SkillTree.ClearData();
        }  

        private void Start()
        {
            // Pause/unpause game when skill tree window opened/closed
            DefaultSkillTreeWindow.instance.onOpen.AddListener(Pause);
            DefaultSkillTreeWindow.instance.onClose.AddListener(Unpause);

            // Set player HP to max
            player.GetStat("HP").ResetValues();

            // Set player exp to 0
            player.SetStat("EXP", 0);

            timeRemaining = waveDelay;
        }

        private void Update()
        {
            // Handle waves
            switch (state)
            {
                // Wait for wave
                case GameState.Waiting:
                    timeRemaining -= Time.deltaTime;
                    timerTextMesh.text = $"{timeRemaining:0.00}s";

                    // Spawn enemies and change state if wave delay complete
                    if (timeRemaining <= 0)
                    {
                        wave++;
                        spawnedAmount = 0;
                        spawnAmountMax = waveEnemyAmountScaling * wave;
                        waveTextMesh.text = $"Wave {wave}";
                        state = GameState.Running;
                        timeRemaining = enemySpawnDelay;
                        timerTextMesh.text = string.Empty;
                    }
                    break;

                case GameState.Running:
                    // Incrementally spawn enemies
                    timeRemaining -= Time.deltaTime;

                    if (timeRemaining <= 0 && spawnedAmount < spawnAmountMax)
                    {
                        // Spawn enemy
                        SpawnEnemies(1);
                        spawnedAmount++;
                        timeRemaining = enemySpawnDelay;
                    }

                    // Check if all enemies have been defeated
                    if (spawnedEnemies.Count == 0 && spawnedAmount == spawnAmountMax)
                    {
                        // If so, go back to waiting
                        state = GameState.Waiting;
                        timeRemaining = waveDelay;
                    }
                    break;

                    // Do nothing if paused
                case GameState.Paused:

                    break;
            }
        }

        private void OnApplicationQuit()
        {
            SkillTree.ClearData();
        }

        /// <summary>
        /// Removes an enemy from the list of spawned enemies.
        /// </summary>
        /// <param name="enemy">The enemy.</param>
        public void RemoveEnemy(ExampleEnemyAI enemy)
        {
            spawnedEnemies.Remove(enemy);
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void Pause()
        {
            state = GameState.Paused;
            playerController.SetRigidbodySimulated(false);

            foreach (var enemy in spawnedEnemies)
            {
                enemy.SetRigidbodySimulated(false);
            }
        }

        /// <summary>
        /// Unpauses the game.
        /// </summary>
        public void Unpause()
        {
            if (spawnedEnemies.Count > 0 || spawnedAmount < spawnAmountMax)
            {
                state = GameState.Running;
            }
            else
            {
                state = GameState.Waiting;
            }

            playerController.SetRigidbodySimulated(true);

            foreach (var enemy in spawnedEnemies)
            {
                enemy.SetRigidbodySimulated(true);
            }
        }

        /// <summary>
        /// Spawns enemies at the left or right randomly.
        /// </summary>
        /// <param name="amount">The amount of enemies to spawn.</param>
        public void SpawnEnemies(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                bool spawnLeft = Random.Range(0, 2) == 1;

                if (spawnLeft)
                {
                    spawnedEnemies.Add(spawnerLeft.SpawnEnemy());
                }
                else
                {
                    spawnedEnemies.Add(spawnerRight.SpawnEnemy());
                }
            }
        }

        /// <summary>
        /// The state of the game for Endless Smash.
        /// </summary>
        public enum GameState
        {
            /// <summary>
            /// Waiting for the next wave.
            /// </summary>
            Waiting,

            /// <summary>
            /// Currently fighting a wave.
            /// </summary>
            Running,

            /// <summary>
            /// Game is paused.
            /// </summary>
            Paused
        }
    }
}