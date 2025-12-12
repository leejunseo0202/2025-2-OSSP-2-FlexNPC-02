using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using System.Text;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class NeedsAgent : Agent
{
    // 1. 내부 상태 정의
    [Header("Needs (0~1)")]
    public float hunger = 0f;
    public float toilet = 0f;
    public float social = 0f;
    public float hygiene = 0f;
    public float fun = 0f;
    public float energy = 0f;
    public bool randomNeedsStart = true;

    [Header("Need Thresholds")]
    public float needLowThreshold = 0.3f;  // 욕구가 낮다고 판단하는 기준
    public float needHighThreshold = 0.7f; // 욕구가 높다고 판단하는 기준

    public float scanInterval = 0.5f;

    [Header("Debug / Record")]
    public bool recordStat = false;

    private float recordTimer = 0f;
    private float recordInterval = 1.0f;

    private StringBuilder csvBuilder;
    private string csvPath;

    private NavMeshAgent agent;
    private bool isMovingToTarget = false;   // 현재 목표로 이동 중인가?
    private bool isInteracting = false;      // 상호작용 중인가?
    private Transform currentTarget = null;  // 현재 목표 Building
    private float movementTimer = 0f;        // 목표 향해 이동한 시간
    private float maxMoveTime = 15f;          // 목표 미도달 시 실패 처리 기준

    public float detectRadius = 10000f; // 주변 탐지 반경


    private float elapsedTime = 0f;
    public float episodeDuration = 60f; // 에피소드 길이(초 단위)

    public string agentId = "NPC_01";  // SimpleBuilding과 연동될 NPC ID
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // 속도 변경
        agent.speed = 10f;
    }



    // 2. Observation 수집
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. 욕구 상태
        sensor.AddObservation(hunger);
        sensor.AddObservation(toilet);
        sensor.AddObservation(social);
        sensor.AddObservation(hygiene);
        sensor.AddObservation(fun);
        sensor.AddObservation(energy);

        // 2. detectRadius 내 오브젝트 탐지
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);

        int maxObjects = 10;  // 최대 n개까지만 관측 (고정 길이 필요)
        int count = 0;

        foreach (var hit in hits)
        {
            if (count >= maxObjects) break;

            if (hit.gameObject == this.gameObject)
            {
                continue;
            }
            
            Building b = hit.GetComponent<Building>();
            if (b == null) continue;

            Vector3 relPos = hit.transform.position - transform.position;

            // 상대 좌표 (x, z)
            sensor.AddObservation(relPos.x);
            sensor.AddObservation(relPos.z);

            // 태그 타입
            sensor.AddObservation(EncodeBuildingTag(hit.tag));

            count++;
        }

        // Debug.Log($"Observed {count} objects.");
        // 3. 부족한 오브젝트는 padding
        while (count < maxObjects)
        {
            sensor.AddObservation(0f); // relPos.x
            sensor.AddObservation(0f); // relPos.z
            sensor.AddObservation(0f); // tagIndex
            count++;
        }
    }

    int EncodeBuildingTag(string tag)
    {
        return tag switch
        {
            "Hunger" => 1,
            "Toilet" => 2,
            "Social" => 3,
            "Hygiene" => 4,
            "Fun" => 5,
            "Energy" => 6,
            _ => 0,
        };
    }


    // 3. Action 정의
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log("OnActionReceived called");
        if (isInteracting || isMovingToTarget) return;  // 상호작용 또는 이동 중이면 행동 무시

        // 1. Discrete Action으로 목표 선택
        int targetIndex = actions.DiscreteActions[0]; // 0~5 (6개 목표)
        string targetTag = GetTagFromAction(targetIndex);

        // 2. 선택한 목표 태그에서 가장 가까운 목표 찾기
        GameObject nearestTarget = GetNearestTarget(targetTag);
        if (nearestTarget == null) return;

        if(agent == null)
            agent = GetComponent<NavMeshAgent>();
        currentTarget = nearestTarget.transform;
        agent.SetDestination(currentTarget.transform.position);

        isMovingToTarget = true;
        movementTimer = 0f;
    }

    // Action index → 목표 태그 변환
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
            default: return "Hunger";
        }
    }
    //4. 상호작용
    private IEnumerator InteractWithTarget(Transform target)
    {
        isInteracting = true;
        agent.isStopped = true;
        Building building = target.GetComponent<Building>();

        NeedsAgent partner = DetectAgentOnSocialTarget(target);
        if (partner != null)
        {
            // 상대 Agent도 동일 건물과 상호작용 실행
            bool accepted = partner.ForcePartnerInteraction(this, building);
            if (!accepted)
            {
                agent.isStopped = false;
                agent.ResetPath();

                agent.isStopped = false;
                isInteracting = false;
                currentTarget = null;

                yield break;
            }
        }

        float interactDuration = 2.0f; // 상호작용 시간
        float elapsed = 0f;

        while (elapsed < interactDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        

        if (building != null && building.isFunctioning)
        {
            List<NeedModification> effects = building.UseBuilding(agentId, 1f, 0);

            if (effects != null && effects.Count > 0)
            {
                float totalReward = 0f;
                foreach (var eff in effects)
                {
                    float before = GetNeedValue(eff.needTag);
                    ApplyNeedModification(eff);
                    float after = GetNeedValue(eff.needTag);

                    if (eff.amount < 0)
                    {
                        float delta = before - after;  // 감소된 양
                        totalReward += delta;          // 그만큼 보상
                    }

                }

                AddReward(totalReward);
            }
            else
            {
                AddReward(-0.1f);
            }
        }
        else
        {
            AddReward(-0.1f);
        }

        Debug.Log($"{this.name} interacted with {target.name} | Hunger:{hunger:F2}, Toilet:{toilet:F2}, Social:{social:F2}, Hygiene:{hygiene:F2}, Fun:{fun:F2}, Energy:{energy:F2}");

        agent.isStopped = false;
        isInteracting = false;
        currentTarget = null;
    }

    void ApplyNeedModification(NeedModification mod)
    {
        switch (mod.needTag)
        {
            case "Hunger": hunger = Mathf.Clamp01(hunger + mod.amount); break;
            case "Toilet": toilet = Mathf.Clamp01(toilet + mod.amount); break;
            case "Social": social = Mathf.Clamp01(social + mod.amount); break;
            case "Hygiene": hygiene = Mathf.Clamp01(hygiene + mod.amount); break;
            case "Fun": fun = Mathf.Clamp01(fun + mod.amount); break;
            case "Energy": energy = Mathf.Clamp01(energy + mod.amount); break;

            default:
                Debug.LogWarning($"Unknown Need Tag: {mod.needTag}");
                break;
        }
    }

    // 5. 에피소드 랜덤 초기화
    public override void OnEpisodeBegin()
    {
        if (randomNeedsStart)
        {
            hunger = Random.value;
            toilet = Random.value;
            social = Random.value;
            hygiene = Random.value;
            fun = Random.value;
            energy = Random.value;

        }

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
    }

    // 5. 시간 경과에 따른 이동 처리, 욕구 증가
    void Update()
    {
        if (isMovingToTarget && !isInteracting)
        {
            movementTimer += Time.deltaTime;

            // 목표 도착
            if (ReachedTarget(currentTarget))
            {
                StartCoroutine(InteractWithTarget(currentTarget));
                isMovingToTarget = false;
            }
            // 목표 실패
            else if (movementTimer > maxMoveTime)
            {
                AddReward(-0.1f);     // 약한 페널티
                isMovingToTarget = false;
                currentTarget = null;
            }
        }

        UpdateNeeds();

        
        
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= episodeDuration)
        {
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

    private void UpdateNeeds()
    {
        float delta = Time.deltaTime * 0.01f;

        // 1. 욕구 증가
        hunger = Mathf.Clamp01(hunger + delta);
        toilet = Mathf.Clamp01(toilet + delta);
        social = Mathf.Clamp01(social + delta);
        hygiene = Mathf.Clamp01(hygiene + delta);
        fun = Mathf.Clamp01(fun + delta);
        energy = Mathf.Clamp01(energy + delta);

        // 2. 지속적 보상
        if (hunger > needHighThreshold) AddReward(-0.01f * Time.deltaTime);
        if (toilet > needHighThreshold) AddReward(-0.01f * Time.deltaTime);
        if (social > needHighThreshold) AddReward(-0.01f * Time.deltaTime);
        if (hygiene > needHighThreshold) AddReward(-0.01f * Time.deltaTime);
        if (fun > needHighThreshold) AddReward(-0.01f * Time.deltaTime);
        if (energy > needHighThreshold) AddReward(-0.01f * Time.deltaTime);

        if (AllNeedsStable()) AddReward(0.01f * Time.deltaTime);
    }

    GameObject GetNearestTarget(string tag)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(tag) && col.gameObject != this.gameObject)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col.gameObject;
                }
            }
        }
        return nearest;
    }

    public bool ForcePartnerInteraction(NeedsAgent requester, Building building)
    {
        if (isInteracting) return false; // 상호작용 중이면 무시

        StartCoroutine(PartnerInteractionRoutine(requester, building));
        return true;
    }

    private IEnumerator PartnerInteractionRoutine(NeedsAgent requester, Building building)
    {
        isInteracting = true;
        isMovingToTarget = false;
        currentTarget = null;
        if (agent != null) {
            agent.isStopped = true;
            agent.ResetPath();
        }

        float t = 0f;
        float duration = 2f;

        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 건물 효과 동일 적용
        if (building != null && building.isFunctioning)
        {
            List<NeedModification> effects = building.UseBuilding(agentId, 1f, 0);
            ProcessNeedEffects(effects);
        }

        Debug.Log($"{this.name} interacted with {requester.name} | Hunger:{hunger:F2}, Toilet:{toilet:F2}, Social:{social:F2}, Hygiene:{hygiene:F2}, Fun:{fun:F2}, Energy:{energy:F2}");

        if (agent != null)
            agent.isStopped = false;
        isInteracting = false;
    }

    void ProcessNeedEffects(List<NeedModification> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            AddReward(-0.1f);
            return;
        }

        float totalReward = 0f;

        foreach (var eff in effects)
        {
            float before = GetNeedValue(eff.needTag);
            ApplyNeedModification(eff);
            float after = GetNeedValue(eff.needTag);

            if (eff.amount < 0)
                totalReward += before - after;
        }

        AddReward(totalReward);
    }

    void RecordStatLine()
    {
        Vector3 pos = transform.position;
        float reward = GetCumulativeReward();

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

        EndEpisode();
    }

    #region Helper Methods

    bool ReachedTarget(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < 5.0f; // 도착 허용 거리
    }

    float GetNeedValue(string tag)
    {
        switch (tag)
        {
            case "Hunger": return hunger;
            case "Toilet": return toilet;
            case "Social": return social;
            case "Hygiene": return hygiene;
            case "Fun": return fun;
            case "Energy": return energy;
            default: return 0f;
        }
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

    private NeedsAgent DetectAgentOnSocialTarget(Transform target)
    {
        if (target == null) return null;

        // 자기 자신 제외
        if (target.gameObject == this.gameObject)
            return null;

        return target.GetComponent<NeedsAgent>();
    }

    #endregion

}
