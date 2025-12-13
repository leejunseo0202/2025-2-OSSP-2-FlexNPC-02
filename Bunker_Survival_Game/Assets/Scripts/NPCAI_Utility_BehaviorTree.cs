using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.LightTransport;

public class NPCAI_Utility_BehaviorTree : MonoBehaviour
{
    [Header("Needs (0~1)")]
    public float hunger  = 0;
    public float toilet  = 0;
    public float social  = 0;
    public float hygiene = 0;
    public float fun     = 0;
    public float energy  = 0;
    public bool randomNeedsStart = true;
    private List<int> criticalNode;
    private List<int> secondaryNode;
    private List<int> idleNode;

    [Header("Need Thresholds")]
    public float needLowThreshold = 0.3f;
    public float needHighThreshold = 0.7f;

    public float detectRadius = 10000f;

    [Header("Debug / Record")]
    public bool recordStat = false;

    private float recordTimer = 0f;
    private float recordInterval = 1.0f;

    private StringBuilder csvBuilder;
    private string csvPath;

    private NavMeshAgent agent;
    private Transform currentTarget = null;
    private bool isInteracting = false;

    public float reward = 0.0f;
    private string targetTag;
    private bool isMovingToTarget = false;   // 현재 목표로 이동 중인가?
    private float movementTimer = 0f;        // 목표 향해 이동한 시간
    private float maxMoveTime = 15f;          // 목표 미도달 시 실패 처리 기준
    List<string> needName = new List<string>();

    private float elapsedTime = 0f;
    public float episodeDuration = 60f; // 에피소드 길이(초 단위)

    void Start()
    {
        if (gameObject.name.Contains("ML_Agent")) return;

        criticalNode = new List<int>();
        secondaryNode = new List<int>();
        idleNode = new List<int>();

        agent = GetComponent<NavMeshAgent>();
        if (randomNeedsStart) {
            hunger = Random.value;
            toilet = Random.value;
            social = Random.value;
            hygiene = Random.value;
            fun = Random.value;
            energy = Random.value;
        }
        // 속도 변경
        agent.speed = 10f;

        needName.Add("Hunger");
        needName.Add("Toilet");
        needName.Add("Social");
        needName.Add("Hygiene");
        needName.Add("Fun");
        needName.Add("Energy");

        elapsedTime = 0f;

        recordTimer = 0f;

        if (recordStat)
        {
            csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(
                "time,x,z,hunger,toilet,social,hygiene,fun,energy,reward"
            );

            string dir = Application.dataPath + "/StatLogs";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            csvPath =
                $"{dir}/agent_{name}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }
        hunger = 0f;
    }

    void Update()
    {
        if (gameObject.name.Contains("ML_Agent")) return;

        UpdateNeeds();

        if (!isMovingToTarget && currentTarget == null)
        {
            // 1. behavior tree를 Critical Need, Secondary Need, Idle / Patrol 노드로 나눈다.
            UpdateBehaviourTree();

            // 2. behavior tree에서 Node 선택(Utility AI 기반)
            int need = ChooseBehaviorTree();

            // 3. 선택한 목표 태그에서 가장 가까운 목표 찾기
            targetTag = GetTagFromAction(need);
            GameObject nearestTarget = GetNearestTarget(targetTag);
            if (nearestTarget == null) return;

            currentTarget = nearestTarget.transform;
            agent.SetDestination(currentTarget.transform.position);

            isMovingToTarget = true;
            movementTimer = 0f;
        }

        // 4. 목표 지점에 도착하면 상호작용 시작
        MoveNpc(targetTag);


        elapsedTime += Time.deltaTime;

        if (elapsedTime >= episodeDuration)
        {
            elapsedTime = 0f;
            FinishEpisode();
        }

        if (recordStat)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordTimer = 0f;
                RecordStatLine();
            }
        }
    }

    // 1. behavior tree를 Critical Need, Secondary Need, Idle / Patrol 노드로 나눈다.
    private void UpdateBehaviourTree() 
    {
        if(criticalNode != null) criticalNode.Clear();
        if(secondaryNode != null) secondaryNode.Clear();
        if(idleNode != null) idleNode.Clear();

        for (int i = 0; i < 6; i++)
        {
            string needTag = GetTagFromAction(i);
            float needValue = GetNeedValue(i);

            // 1️. Critical Node
            if (needValue >= 0.7f)
                criticalNode.Add(i);
            // 2️. Secondary Node
            else if (needValue >= 0.3f)
                secondaryNode.Add(i);
            // 3️. Idle/patrol Node
            else
            {
                int value = Random.Range(1, 3);
                if(value == 1) idleNode.Add(6);
                else           idleNode.Add(7);
            }
        }
    }

    // 2. behavior tree 실행(Utility AI 기반)
    private int ChooseBehaviorTree()
    {
        int count_critical = criticalNode.Count;
        int count_secondary = secondaryNode.Count;
        int count_idle = idleNode.Count;
        int bestNeed = -1;

        if (count_critical != 0)
            bestNeed = GetHighestUtilityNeed(criticalNode);
        else if (count_secondary != 0)
            bestNeed = GetHighestUtilityNeed(secondaryNode);
        else
            bestNeed = idleNode[0];

        return bestNeed;
    }

    // 2.1 Utility 기반으로 가장 높은 점수의 Need 선택
    private int GetHighestUtilityNeed(List<int> node)
    {
        float bestScore = -1.0f;
        int bestNeed = -1;

        foreach (int need in node)
        {
            float value = GetNeedValue(need);  // 실제 Need 값을 가져옴

            float score =
                value < 0.3f ? value * 0.1f :
                value < 0.7f ? value * 1.0f :
                               value * 2.0f;  // Utility 가중치

            if (score > bestScore)
            {
                bestScore = score;
                bestNeed = need;
            }
        }
        return bestNeed;
    }

    // 3. 선택한 욕구에 맞는 건물 탐색
    GameObject GetNearestTarget(string tag)
    {
        if (tag == "Walk") 
        {
            RandomMove();
            return null;
        } 
        else if (tag == "Idle") return null;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(tag))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col.gameObject;
                }
            }
        }
        if (nearest == null) detectRadius += 2f;
        else detectRadius = 10f;
        return nearest;
    }

    // 3-1. Patrol 랜덤 이동
    private void RandomMove()
    {
        Vector3 randomDirection = Random.insideUnitSphere * detectRadius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, detectRadius, -1);
        agent.SetDestination(navHit.position);
    }

    // 4. 목표 지점에 도착하면 상호작용 시작
    private void MoveNpc(string targetTag) {
        if (isMovingToTarget && !isInteracting)
        {
            movementTimer += Time.deltaTime;

            // 목표 도착
            if (ReachedTarget(currentTarget))
            {
                StartCoroutine(InteractWithTarget(currentTarget, targetTag));
                isMovingToTarget = false;
            }
            // 목표 실패
            else if (movementTimer > maxMoveTime)
            {
                reward -= 0.1f;
                isMovingToTarget = false;
                currentTarget = null;
            }
        }

    }

    //4-1. 상호작용
    private IEnumerator InteractWithTarget(Transform target, string needTag)
    {
        Debug.Log($"npc : {gameObject.name}, action : {needTag}");

        isInteracting = true;
        agent.isStopped = true;

        float interactDuration = 2.0f; // 상호작용 시간
        float elapsed = 0f;

        while (elapsed < interactDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Building building = target.GetComponent<Building>();

        if (building != null && building.isFunctioning)
        {
            List<NeedModification> effects = building.UseBuilding("NPC_01", 5f, 1);

            if (effects != null && effects.Count > 0)
            {
                float totalReward = 0f;
                foreach (var eff in effects)
                {
                    int needindex = needName.IndexOf(eff.needTag);
                    float before = GetNeedValue(needindex);
                    ApplyNeedModification(eff);
                    float after = GetNeedValue(needindex);

                    if (eff.amount < 0)
                    {
                        float delta = before - after;  // 감소된 양
                        totalReward += delta;          // 그만큼 보상
                    }
                }
                reward += totalReward;
            }
            else    reward -= 0.1f;
        }
        else    reward -= 0.1f;

        agent.isStopped = false;
        isInteracting = false;
        currentTarget = null;
    }

    // 4-2. 욕구 변화 적용
    void ApplyNeedModification(NeedModification mod)
    {
        switch (mod.needTag)
        {
            case "Hunger": hunger = Mathf.Clamp01(hunger + mod.amount);     Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;
            case "Toilet": toilet = Mathf.Clamp01(toilet + mod.amount);     Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;
            case "Social": social = Mathf.Clamp01(social + mod.amount);     Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;
            case "Hygiene": hygiene = Mathf.Clamp01(hygiene + mod.amount);  Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;
            case "Fun": fun = Mathf.Clamp01(fun + mod.amount);              Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;
            case "Energy": energy = Mathf.Clamp01(energy + mod.amount);     Debug.Log($"needtag : {mod.needTag}, amount : {mod.amount}"); break;

            default:
                Debug.LogWarning($"Unknown Need Tag: {mod.needTag}");
                break;
        }
    }

    // 시간에 따른 욕구 증가
    void UpdateNeeds()
    {
        float delta = Time.deltaTime * 0.01f;

        hunger = Mathf.Clamp01(hunger + delta);
        toilet = Mathf.Clamp01(toilet + delta);
        social = Mathf.Clamp01(social + delta);
        hygiene = Mathf.Clamp01(hygiene + delta);
        fun = Mathf.Clamp01(fun + delta);
        energy = Mathf.Clamp01(energy + delta);

        // 2. 지속적 보상
        if (hunger > needHighThreshold) reward += (-0.01f * Time.deltaTime);
        if (toilet > needHighThreshold) reward += (-0.01f * Time.deltaTime);
        if (social > needHighThreshold) reward += (-0.01f * Time.deltaTime);
        if (hygiene > needHighThreshold) reward += (-0.01f * Time.deltaTime);
        if (fun > needHighThreshold) reward += (-0.01f * Time.deltaTime);
        if (energy > needHighThreshold) reward += (-0.01f * Time.deltaTime);

        if (AllNeedsStable()) reward += (0.01f * Time.deltaTime);
    }

    void RecordStatLine()
    {
        Vector3 pos = transform.position;

        csvBuilder.AppendLine(
            $"{Time.time:F2}," +
            $"{pos.x:F2},{pos.z:F2}," +
            $"{hunger:F3},{toilet:F3},{social:F3},{hygiene:F3},{fun:F3},{energy:F3}," +
            $"{reward:F3}"
        );
    }
    void FinishEpisode()
    {
        if (recordStat && csvBuilder != null)
        {
            File.WriteAllText(csvPath, csvBuilder.ToString());
            Debug.Log($"[STAT] Saved CSV: {csvPath}");
        }
    }

    #region Helper Methods
    private float GetNeedValue(int i)
    {
        switch (i)
        {
            case 0: return hunger;
            case 1: return toilet;
            case 2: return social;
            case 3: return hygiene;
            case 4: return fun;
            case 5: return energy;
            case 6: return 2;       //Walk
            case 7: return 3;       //idle
            default: return 0f;
        }
    }
    // Action index -> 목표 태그 변환
    string GetTagFromAction(int actionIndex)
    {
        switch (actionIndex)
        {
            case 0: return "Hunger";
            case 1: return "Toilet";
            case 2: return "Social";
            case 3: return "Hygiene";
            case 4: return "Fun";
            case 5: return "Energy";
            case 6: return "Walk";
            case 7: return "Idle";
            default: return "Hunger";
        }
    }
    bool ReachedTarget(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        bool contact = (distance < 5.0f);
        return contact; // 도착 허용 거리
    }

    bool AllNeedsStable()
    {
        return hunger < needLowThreshold &&
               toilet < needLowThreshold &&
               social < needLowThreshold &&
               hygiene < needLowThreshold &&
               fun < needLowThreshold &&
               energy < needLowThreshold;
    }
    #endregion
}