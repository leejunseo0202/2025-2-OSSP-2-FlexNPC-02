using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameDefinitions;
using static UnityEngine.EventSystems.EventTrigger;

public class UI_StatusNPC : MonoBehaviour
{
    public RectTransform[] guage = new RectTransform[6];
    public Image[] guageImage = new Image[6];
    public TMPro.TMP_Text npcNameText;

    public NeedsAgent selectedNPC = null;
    public Button[] npcButton;
    public TMPro.TMP_Text[] npcText;
    public NeedsAgent[] npclist;

    private NavMeshAgent navMesh;

    //UI 활성화 관련 변수
    public Animator animator;
    public Animator animator_right;
    public Button openButton;
    public Button closeButton;
    public GameObject ui_SelectNPCGroup;
    public GameObject ui_ShowGuageGroup;
    public bool isSelectNPC = false;
    private bool ui_Open = false;

    private float maxWidth = 400f;
    private int numberOfNPC = 0;
    private bool shift;

    // 설정 버튼
    public Button settingButton;
    public bool isRight = false;


    public GameObject needsAgentPrefab;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "PlayScene")
        {
            numberOfNPC = PlayerPrefs.GetInt("NumberOfNPC");
            npclist = new NeedsAgent[numberOfNPC + 1];

            // NPC 수에 따라 NPC 생성
            for (int i = 1; i <= numberOfNPC; i++)
            {
                Vector3 spawnPoint = new Vector3((float)i * 1.5f - 10f, 1, 0);

                GameObject npcObj = Instantiate(needsAgentPrefab, spawnPoint, Quaternion.identity);
                npcObj.name = "NPC" + i + "_ML_Agent";
                npclist[i] = npcObj.GetComponent<NeedsAgent>();
                //GameObject npcObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //npcObj.name = "NPC" + i + "_ML_Agent";
                //npcObj.transform.position = spawnPoint;
                //npcObj.tag = "Social";


                //// 2.1 NeedsAgent  스크립트 컴포넌트 추가
                //NeedsAgent newNpc = npcObj.AddComponent<NeedsAgent>();

                //// 2.2 Behavior Parameters 설정
                //var bp = npcObj.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
                //bp.BehaviorName = "NeedsAgentBehavior";
                //bp.BrainParameters.VectorObservationSize = 36;
                //bp.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeDiscrete(6);

                //// 2.3 NavMeshAgent Properties 적용
                //NavMeshAgent navMesh = npcObj.AddComponent<NavMeshAgent>();
                //navMesh.speed = 3f;
                //navMesh.angularSpeed = 120f;
                //navMesh.acceleration = 8f;
                //navMesh.stoppingDistance = 1f;
                //navMesh.autoBraking = true;

                //navMesh.radius = 0.5f;
                //navMesh.height = 1f;
                //navMesh.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                //navMesh.avoidancePriority = 50;

                //// Path Finding
                //navMesh.autoTraverseOffMeshLink = true;
                //navMesh.autoRepath = true;
                //navMesh.areaMask = NavMesh.AllAreas;

                //// 2.4 BoxCollider 적용
                //BoxCollider col = npcObj.AddComponent<BoxCollider>();
                //col.center = Vector3.zero;
                //col.size = new Vector3(1f, 1f, 1f);

                //// 2.5 Rigidbody 추가
                //Rigidbody rb = npcObj.AddComponent<Rigidbody>();
                //rb.mass = 1f;
                //rb.useGravity = true;
                //rb.isKinematic = false;
                //rb.interpolation = RigidbodyInterpolation.None;
                //rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                //rb.centerOfMass = Vector3.zero;

                ////
                //SimpleBuilding building = npcObj.AddComponent<SimpleBuilding>();

                //// 1. 정보 설정
                //building.buildingName = "New Building";
                //building.size = new Vector2Int(1, 1);
                //building.height = 1;

                //// 2. 기능 설정
                //building.requiredResourceTag = "";
                //building.requiresResourceToFunction = false;

                //// 5. 현재 상태
                //building.isFunctioning = true;

                //// 3. Need Effects
                //building.needEffects = new List<NeedModification>();
                //building.needEffects.Add(new NeedModification
                //{
                //    needTag = NeedType.Social,
                //    amount = 10
                //});






                //버튼 활성화
                if (npcButton != null && npcText != null)
                {
                    npcButton[i].gameObject.SetActive(true);
                    npcText[i].gameObject.SetActive(true);
                    npcText[i].text = "NPC" + i;
                }
            }

        }

        if (SceneManager.GetActiveScene().name == "Compare_NPC_AI")
        {
            //버튼 활성화
            npcButton[1].gameObject.SetActive(true);
            npcText[1].gameObject.SetActive(true);
            npcText[1].text = "NPC" + 1;
        }

        for (int i = 0; i < guage.Length; i++)
            if (guageImage[i] == null && guage[i] != null)
                guageImage[i] = guage[i].GetComponent<Image>();
    }

    void Update()
    {
        shift = Keyboard.current.shiftKey.isPressed;
        if (Keyboard.current.digit1Key.wasPressedThisFrame && numberOfNPC >= 1 && !shift) SelectNPC(1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && numberOfNPC >= 2 && !shift) SelectNPC(2);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && numberOfNPC >= 3 && !shift) SelectNPC(3);
        if (Keyboard.current.digit4Key.wasPressedThisFrame && numberOfNPC >= 4 && !shift) SelectNPC(4);

        if (SceneManager.GetActiveScene().name == "Compare_NPC_AI")
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && !shift && !isRight) SelectNPC(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && !shift && !isRight) SelectNPC(2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame && !shift && !isRight) SelectNPC(3);
            if (Keyboard.current.digit4Key.wasPressedThisFrame && !shift && !isRight) SelectNPC(4);

            if (Keyboard.current.digit1Key.wasPressedThisFrame && shift && isRight) SelectNPC(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && shift && isRight) SelectNPC(2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame && shift && isRight) SelectNPC(3);
            if (Keyboard.current.digit4Key.wasPressedThisFrame && shift && isRight) SelectNPC(4);

            // back키
            if (Keyboard.current.bKey.wasPressedThisFrame && ui_ShowGuageGroup.activeSelf && shift && isRight)
            {
                if (ui_SelectNPCGroup != null)
                    ui_SelectNPCGroup.SetActive(true);

                if (ui_ShowGuageGroup != null)
                    ui_ShowGuageGroup.SetActive(false);
            }
        }

            // back키
        if (Keyboard.current.bKey.wasPressedThisFrame && ui_ShowGuageGroup.activeSelf && !isRight)
        {
            if (ui_SelectNPCGroup != null)
                ui_SelectNPCGroup.SetActive(true);

            if (ui_ShowGuageGroup != null)
                ui_ShowGuageGroup.SetActive(false);
        }

        // left 키를 눌러 NPC Status 열기/닫기 토글
        if (!isRight && Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (ui_Open)
            {
                closeButton.onClick.Invoke();
                ui_Open = false;
            }
            else
            {
                openButton.onClick.Invoke();
                ui_Open = true;
            }
        }

        // right 키를 눌러 NPC Status 열기/닫기 토글
        if (isRight && Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (ui_Open)
            {
                closeButton.onClick.Invoke();
                ui_Open = false;
            }
            else
            {
                openButton.onClick.Invoke();
                ui_Open = true;
            }
        }

        // 설정버튼
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            settingButton.onClick.Invoke();

        }

        if (selectedNPC != null)
            UpdateGuage(selectedNPC);
    }

    private void SelectNPC(int index)
    {
        Debug.Log("Select NPC " + index);
        selectedNPC = npclist[index];

        // 제목 텍스트 업데이트
        npcNameText.text = selectedNPC.name;

        // 애니메이션 재생
        if(isRight && animator_right != null && !ui_Open)
        {
            openButton.onClick.Invoke();
            ui_Open = true;
        }
        else if (animator != null && !ui_Open)
        {
            openButton.onClick.Invoke();
            ui_Open = true;
        }

        //버튼 활성화 설정
        if (openButton != null && closeButton != null)
        {
            openButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
        }
        //UI 그룹 활성화 설정
        if (ui_SelectNPCGroup != null && ui_ShowGuageGroup != null)
        {
            ui_SelectNPCGroup.SetActive(false);
            ui_ShowGuageGroup.SetActive(true);
        }
    }

    private void UpdateGuage(NeedsAgent npcStatus)
    {
        Debug.Log("Update");
        float[] statusValue = new float[6];
        statusValue[0] = npcStatus.hunger;
        statusValue[1] = npcStatus.toilet;
        statusValue[2] = npcStatus.social;
        statusValue[3] = npcStatus.hygiene;
        statusValue[4] = npcStatus.fun;
        statusValue[5] = npcStatus.energy;
        for (int i = 0; i < guage.Length; i++)
        {
            float clamped = Mathf.Clamp(statusValue[i], 0.0f, 1.0f);
            float newWidth = (clamped) * maxWidth;

            Vector2 size = guage[i].sizeDelta;
            size.x = Mathf.Lerp(size.x, newWidth, Time.deltaTime * 5f);
            guage[i].sizeDelta = size;

            if (guageImage[i] != null)
                guageImage[i].color = Color.Lerp(Color.red, Color.green, clamped);
        }
    }
}