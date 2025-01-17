using System;
using System.Collections.Generic;
using TimeRelated;
using UnityEngine;
using Random = UnityEngine.Random;

[Obsolete("Encode failed and it is monobehaviour.")]
public class RandomSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs; // Ҫ���ɵ���ƷԤ��������
    public Dictionary<int, GameObject[]> itemSchedule; // �������Ӧ��Ʒ��ӳ���
    public int currentDay; // ��ǰ��������1 = ��һ�죬2 = �ڶ��죬�������ƣ�
    public DayNightEventSystem dayNightSystem;
    public float spawnInterval = 10f; // ���ɼ��������Ϊ��λ��

    private bool isDay; // �Ƿ�Ϊ����
    private float lastSpawnTime; // ��һ�����ɵ�ʱ��

    void Start()
    {
        // ���������Ͷ�Ӧ����Ʒ���ɼƻ�
        itemSchedule = new Dictionary<int, GameObject[]>
        {
            { 1, new[] { itemPrefabs[0] } }, // ��һ�죺���ɿ���
            { 2, new[] { itemPrefabs[1] } }, // �ڶ��죺��������
            { 3, new[] { itemPrefabs[2] } }  // �����죺���Լ������Ӹ�����Ʒ
        };

        lastSpawnTime = Time.time; // ��ʼ����һ������ʱ��
    }

    void Update()
    {
        // �� DayNightEventSystem ͬ������
        currentDay = dayNightSystem.clockUdon.dayCount; // ��ȡ��ǰ��Ϸ�е�����
        isDay = dayNightSystem.isDay; // ��ȡ��ǰ�Ƿ�Ϊ����

        // ����ǰ����ҵ�������ʱ�䣬��������Ʒ
        if (isDay && Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnItemsForCurrentDay(); // ���ɵ����Ӧ����Ʒ
            lastSpawnTime = Time.time; // ������һ������ʱ��
        }
    }

    private void SpawnItemsForCurrentDay()
    {
        if (itemSchedule.ContainsKey(currentDay))
        {
            // ��ȡ������Ҫ���ɵ���Ʒ�б�
            GameObject[] itemsToSpawn = itemSchedule[currentDay];

            foreach (GameObject itemPrefab in itemsToSpawn)
            {
                // �������λ��
                Vector3 randomSpawnPosition = new Vector3(
                    Random.Range(120, 170),
                    10,
                    Random.Range(-15, 15)
                );
                // ʵ������Ʒ
                Instantiate(itemPrefab, randomSpawnPosition, Quaternion.identity);
            }
        }
    }
}