//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Spawns enemies.
//***************************************************************************************

using UnityEngine;

namespace Esper.SkillTree.Examples
{
    public class ExampleEnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private ExampleEnemyAI enemyPrefab;

        /// <summary>
        /// Spawns an enemy in a random location within range.
        /// </summary>
        public ExampleEnemyAI SpawnEnemy()
        {
            // Spawn enemy in random position inside a cube
            var enemy = Instantiate(enemyPrefab);
            enemy.transform.position = GetRandomPosition();
            return enemy;
        }

        /// <summary>
        /// Gets a random position in the spawn area.
        /// </summary>
        /// <returns>A random position.</returns>
        public Vector3 GetRandomPosition()
        {
            var scale = transform.localScale / 2;
            return transform.position + new Vector3(Random.Range(-scale.x, scale.x), Random.Range(-scale.y, scale.y), Random.Range(-scale.z, scale.z));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}