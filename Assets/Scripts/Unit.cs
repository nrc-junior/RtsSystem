using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;

public class UnitTimerInfo {
    public float collectCurrent = 0;
    public float collectNext = 0;
}

public class Unit : MonoBehaviour {
    
    public static List<Unit> globalUnits = new List<Unit>();
    public static Dictionary<int, List<Unit>> teamUnits;
    protected static Dictionary<int, List<Collider>> teamColliders;

    Rigidbody rb;

    public int team = -1;
    bool isPlayerOrder = false; 

    public const int unitLayer = 1 << 6;  

    public UnitRoleData role;
    public UnitData data;

    bool isDead = false; 
    public Action<Unit> DIED;
    
    public enum State {Idle, Collecting, Deliverying, Returning }
    public enum Behaviour{TrackPlayerEnemies, TrackResources}
    
    // character caracteristics 
    CharacterRig rig;
    //

    // COMBAT 
    [Space(20)]
    public UnityEvent OnAttack = new UnityEvent();
    public UnityEvent OnReceiveDamage = new UnityEvent();
    public UnityEvent OnDie = new UnityEvent();
    // COMBAT 
    

    public Collider mainCollider {get; private set;}
    Transform transformData;
    GameObject highlight;

    public Vector3 curWorldPosition {get; private set;}
    AIData aiData;

    public Action SELECTED, UNSELECT;

    private Behaviour lastBehaviour;
    private Behaviour currentBehaviour = Behaviour.TrackPlayerEnemies;
    public Behaviour behaviour {get => currentBehaviour; set { lastBehaviour = currentBehaviour; currentBehaviour = value;}}
    
    // COMBAT
    Unit enemy;
    bool isAgroed = false;
    bool isAttacking = false; 
    // COMBAT


    // COLLECT
    public State currentState { get; set; } = State.Idle;
    public KeyValuePair<Resource, int> carrying;
    public ResourceData lastCollected { get; set; }
    public Resource currentResource { get; set; }

    public ResourceData collectedType {get; set;}

    public Building currentWarehouse { get; set; }
    public int collectingIdx {get; set;}
    public int totalCollected {get; set;}
    public UnitTimerInfo timer = new UnitTimerInfo();
    // COLLECT


    public void Awake(){
        SELECTED += OnSelected;
        UNSELECT += OnDeselect;
        
        transformData = transform;
        mainCollider = GetComponentInChildren<Collider>();
        data.ai.nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        GenerateHighlight(); 
        RegisterUnit();    
        OnDie.AddListener(DispatchDieEvent);

        rig = gameObject.GetComponentInChildren<CharacterRig>();
    }

    public void Start(){
        aiData = data.ai;
        
        if(role == null){
            role = new UnitRoleData();
        }
        
        InvokeRepeating("UpdateAutoNavPath", 1, role.refreshRate);
        data.health = role.health;
    }

    void Update(){
        if(isDead) return;

        curWorldPosition = transformData.position;
        
        Vector3 dst2D = data.ai.nav.destination;
        Vector3 cur2D = curWorldPosition;
        dst2D.y = 0;
        cur2D.y = 0;

        data.ai.isMoving = Vector3.Distance(dst2D, cur2D) > 1f;

        if(behaviour == Behaviour.TrackPlayerEnemies){
            if(isAgroed){
                TrackTarget();
            }
            else if (!isPlayerOrder)
            {
                CheckForEnemies();
            }
            else if (isPlayerOrder)
            {
                if(data.ai.nav.remainingDistance < .2f){
                    isPlayerOrder = false;
                    data.ai.nav.destination = curWorldPosition;
                }
            }
        }

        if(rig){
            rig.isCollecting = currentState == State.Collecting;
            rig.isMoving = data.ai.isMoving;
            rig.RefreshParameters();
        }
    }
    
    void UpdateAutoNavPath(){
        if(isDead) return;
        
        if(isAgroed && !isAttacking){
            data.ai.nav.destination = enemy.curWorldPosition;
        }
    }


    void GenerateHighlight(){
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].localBounds;
        for (var i = 1; i < renderers.Length; ++i)
            bounds.Encapsulate(renderers[i].localBounds); // todo: escala n ta funcionando direito
        
        Vector3 unitMarkerSize = new Vector3 ( bounds.max.x / 2 ,  bounds.max.z / 2, 1 );
        highlight = Instantiate(Resources.Load<GameObject>("Unit_Highlight")); 
        
        if(highlight){
            Transform hTransform = highlight.transform;
            hTransform.localScale = unitMarkerSize;
            hTransform.SetParent(transform,true);
            Vector3 hPos = bounds.center;
            hPos.y = bounds.min.y +.1f;
            hTransform.localPosition = hPos;
            highlight.SetActive(false);
        }
    }
    
    void RegisterUnit(){
        if(teamUnits == null){
            teamUnits = new Dictionary<int, List<Unit>>();
            teamColliders = new Dictionary<int, List<Collider>>();
        }

        if(!teamUnits.ContainsKey(data.team)){
            teamUnits.Add(data.team, new List<Unit>());
            teamColliders.Add(data.team, new List<Collider>());
        }
        

        globalUnits.Add(this);
        teamUnits[data.team].Add(this);
        teamColliders[data.team].Add(GetComponent<Collider>());
    }

    void CheckForEnemies(){
        Collider[] cols = Physics.OverlapSphere(curWorldPosition, aiData.searchRadius, unitLayer);
        
        foreach (var col in cols){
            if( !teamColliders[data.team].Contains(col)){
                Unit unit = col.GetComponent<Unit>();
                if(unit.data.health <= 0) continue;
                SetTarget(unit); 
                break;
            }
        }
    }

    public void RecivePlayerOrder(){
        Stop();
        
        isAgroed = false;
        isAttacking = false;
        isPlayerOrder = true;

        StopCoroutine(AttackEnemy());
    }

    public void MoveTo(Vector3 position){
        position.y = curWorldPosition.y;
        
        NavMeshPath path = new NavMeshPath();
        bool sucess = data.ai.nav.CalculatePath(position, path);

        if(path.status == NavMeshPathStatus.PathComplete){
            if(isPlayerOrder){
                SpawnEffect.instance.Play(position);
            }

            data.ai.nav.SetPath(path);
            data.ai.nav.isStopped = false;
        }else if (path.status == NavMeshPathStatus.PathInvalid){
            
            // if(isPlayerOrder){
            //     SpawnEffect.instance.Play(position);
            // }
            Debug.LogError("partial");

            data.ai.nav.SetDestination(position);
            data.ai.nav.isStopped = false;
        }
    } 

    public void SetTarget(Unit other){
        isAgroed = true;
        enemy = other;
        
        //todo quando desaparece no fog tbm esquece
        other.OnDie.AddListener(ForgetEnemy);
    }

    void ForgetEnemy(){
        Debug.Log(name + " estava focando " + enemy.name + " mas ele morreu ");

        StopCoroutine(AttackEnemy());
        isAgroed = false;
        isAttacking = false;
        data.ai.nav.destination = curWorldPosition;
    }

    void TrackTarget(){
        if(isAgroed && enemy == null){
            StopCoroutine(AttackEnemy());
            isAgroed = false;
            isAttacking = false;

            data.ai.nav.destination = curWorldPosition;
        }

        float enemyDistance = Vector3.Distance(enemy.curWorldPosition, curWorldPosition);
        
        if(enemyDistance <= aiData.attackRadius){

            if(!isAttacking){
                isAttacking = true;
                StartCoroutine(AttackEnemy());
            }

        } else if(isAttacking) {
            isAttacking = false;
            StopCoroutine(AttackEnemy());
        } 
        
        if(enemyDistance > aiData.searchRadius){
            isAgroed = false;
            isAttacking = false;
            data.ai.nav.destination = curWorldPosition;
        }
    }

    IEnumerator AttackEnemy(){
        while(isAttacking){
            yield return new WaitForSeconds(role.attackSpeed);
            GiveDamage(); 
        }
    }

    void GiveDamage(){
        enemy?.TakeDamage(role.damage);
        OnAttack?.Invoke();
    }

    public void TakeDamage(float damage){
        
        float afterDamage = data.health - damage;
        
        if(afterDamage <= 0 ){
            afterDamage = 0;
        }
        
        data.health = afterDamage;
        OnReceiveDamage?.Invoke();
        
        if(afterDamage == 0){
            isDead = true;
            OnDie?.Invoke();
            

            teamColliders[data.team].Remove(GetComponent<Collider>());
            teamUnits[data.team].Remove(this);
            globalUnits.Remove(this);

            StopAllCoroutines();
            Stop();
            
            
            GetComponent<MeshRenderer>().enabled = false;
            Destroy(gameObject,5);
        }
    }

    void DispatchDieEvent(){
        OnDie.RemoveListener(DispatchDieEvent);
        DIED?.Invoke(this);
    }

    public void Stop(){
        isPlayerOrder = false;
        MoveTo(curWorldPosition);
        data.ai.nav.isStopped = true;
    }

    public void RollbackBehaviour(){
        behaviour = lastBehaviour;
    }

    void OnSelected(){
        if(data.showHighlight) highlight.SetActive(true);
    }

    void OnDeselect(){
        if(data.showHighlight) highlight.SetActive(false);
    }

    protected virtual void OnMouseEnter(){
        if(data.team == team){
            HandleCursor.SetCursor(CursorType.Select);
        }else{
            HandleCursor.SetCursor(CursorType.Attack);
        }
    }
    
    protected virtual void OnMouseExit(){
        HandleCursor.Clear();
    }
    public float DistanceTo(Vector3 target) => Vector3.Distance(curWorldPosition, target);

    void OnDrawGizmos(){
        if(aiData != null)
        Gizmos.DrawWireSphere(curWorldPosition, aiData.searchRadius);
    }
}

[System.Serializable]
public class UnitData {
    public int team = 0;
    public bool showHighlight = true;
    public AIData ai;
    public float health;

    public bool isCollecting {get; set;}
    public bool movingToWarehouse {get; set;}
    public bool reachedJobPoint { get => isCollecting && ai.nav.remainingDistance < 0.2f;}
    

}

[System.Serializable]
public class AIData{
    public float searchRadius = 15;
    public float attackRadius = 1;
    
    public NavMeshAgent nav {get; set;}

    public bool isMoving;
    public bool reachedWarehouse;
}
