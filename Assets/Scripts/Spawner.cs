using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs; // 要生成的物品预制体数组
    public Dictionary<int, GameObject[]> itemSchedule; // 天数与对应物品的映射表
    public int currentDay; // 当前的天数（1 = 第一天，2 = 第二天，依此类推）
    public DayNightEventSystem dayNightSystem;
    public float spawnInterval = 10f; // 生成间隔（以秒为单位）

    private bool isDay; // 是否为白天
    private float lastSpawnTime; // 上一次生成的时间

    void Start()
    {
        // 定义天数和对应的物品生成计划
        itemSchedule = new Dictionary<int, GameObject[]>
        {
            { 1, new[] { itemPrefabs[0] } }, // 第一天：生成可乐
            { 2, new[] { itemPrefabs[1] } }, // 第二天：生成土豆
            { 3, new[] { itemPrefabs[2] } }  // 第三天：可以继续添加更多物品
        };

        lastSpawnTime = Time.time; // 初始化上一次生成时间
    }

    void Update()
    {
        // 与 DayNightEventSystem 同步数据
        currentDay = dayNightSystem.clockUdon.currentDay; // 获取当前游戏中的天数
        isDay = dayNightSystem.isDay; // 获取当前是否为白天

        // 如果是白天且到达生成时间，则生成物品
        if (isDay && Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnItemsForCurrentDay(); // 生成当天对应的物品
            lastSpawnTime = Time.time; // 更新上一次生成时间
        }
    }

    private void SpawnItemsForCurrentDay()
    {
        if (itemSchedule.ContainsKey(currentDay))
        {
            // 获取当天需要生成的物品列表
            GameObject[] itemsToSpawn = itemSchedule[currentDay];

            foreach (GameObject itemPrefab in itemsToSpawn)
            {
                // 随机生成位置
                Vector3 randomSpawnPosition = new Vector3(
                    Random.Range(120, 170),
                    10,
                    Random.Range(-15, 15)
                );
                // 实例化物品
                Instantiate(itemPrefab, randomSpawnPosition, Quaternion.identity);
            }
        }
    }
}
