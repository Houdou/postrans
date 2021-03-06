﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mail : MonoBehaviour {

    public static int IDCounter = -1;
    public int ID;

    public float prior; // type of the mail
    public int value; // normal return value
    public int bonus; // max return value
    public float AvailableTime;

    public AudioClip mailReceivedAC;
    
    public Station TargetHome;
    public bool IsAccepted;
    public bool IsTravelling;
    public bool IsBurned;

    public bool IsArrived;

    //public float rate; // rate to generate
    public float MoveSpeed = 1f;
    
    public float StartTime;
    public float LivingTime {
        get {
            return Time.time - StartTime;
        }
    }

    private LevelController lc;

    public Station TargetCollectStation;
    public Road TravellingRoad;
    public float RoadProgress;
    public float TargetProgress;

    private void Awake() {
        lc = GameMaster.Instance.GetLevelController();
        ID = ++IDCounter;
    }

    private void Start() {
        UpdateTargetStation();
        StartTime = Time.time;
    }

    private void Update() {
        if(IsBurned) { return; }

        if(!IsAccepted) {
            if(TargetCollectStation != null) {
                MoveMail(TargetCollectStation.Pos);
                CheckAcceptance(TargetCollectStation);
            }

            if(LivingTime > AvailableTime) {
                IsBurned = true;
                GameMaster.Instance.GetLevelController().BurnMail(this);
            }
        }

        if(IsTravelling) { // Collected, on path
            if(TargetCollectStation != null && IsAccepted) { // Not in station
                // TODO: Move along the road
                MoveAloneRoad();
                CheckAcceptance(TargetCollectStation);
            }
        }
    }

    //
    //  1. Get x/y from area
    //  2. Get random type and related value
    //  3. Determine the time left
    //
    public void Initialize(Vector2 init_pos, int targetHome) {
        Vector3 pos = new Vector3(init_pos.x, init_pos.y, 0);
        transform.position = pos;

        TargetHome = lc.HomeList[targetHome];
        transform.Find("Marker").GetComponent<SpriteRenderer>().sprite = lc.Markers[targetHome];

    }

    //
    //  Mail Destroy
    //
    void DestroyMail() {
        Destroy(this);
    }

    //
    //  Move mail to the closest postrans
    //
    public void MoveMail(Vector3 targetPosition) {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
    }

    public void MoveAloneRoad() {
        if(TravellingRoad != null) {
            if((TargetProgress - RoadProgress) * MoveSpeed > 0) {
                RoadProgress += MoveSpeed * Time.deltaTime / TravellingRoad.Length;
                transform.position = TravellingRoad.GetCurvePosByProgress(RoadProgress);
            }            
        }
    }

    public void MoveAroundStation() {
        if(TargetCollectStation != null) {
            MoveMail(TargetCollectStation.Pos + new Vector2(Mathf.Cos(Time.time + ID * 0.1f), -Mathf.Sin(Time.time + ID * 0.1f)) * TargetCollectStation.OrbitRadius);
        }
    }

    public void MoveTowards(Station from, Road road, float speed) {
        transform.position = from.Pos;

        FadeIn();

        TravellingRoad = road;
        TargetCollectStation = road.Next(from);

        RoadProgress = road.StationList[0] == from ? 0.0f : 1.0f;
        TargetProgress = 1 - RoadProgress;
        MoveSpeed = speed * Mathf.Sign(TargetProgress - RoadProgress);

        IsTravelling = true;
    }

    public Station UpdateTargetStation() {
        if(TargetCollectStation == null) {
            float shortestDistance = 9999.9f;
            Station targetStation = null;

            foreach(var station in lc.StationList) {
                if(!station.IsAvailable) continue;

                float distance = Vector2.Distance(station.Pos, transform.position);

                if(distance < station.MailRange && distance < shortestDistance) {
                    shortestDistance = distance;
                    targetStation = station;
                }
            }

            if(targetStation != null) {
                TargetCollectStation = targetStation;
            }
        }
        return TargetCollectStation;
    }

    public void CheckAcceptance(Station station) {
        if(Vector2.Distance(transform.position, station.Pos) < 0.5f) {
            if(station.ID == TargetHome.ID) {
                FadeOut();
                IsAccepted = true;
                IsArrived = true;
                IsTravelling = false;

                // TODO: Add money
                lc.MoneyLeft += value;
                // TODO: Play sound

            } else {
                bool arriveStation = station.AddMail(this);
                
                if(!IsAccepted) {
                    IsAccepted = true;
                }

                if(arriveStation) {
                    FadeOut();
                    IsTravelling = false;
                } else {
                    IsTravelling = true;
                    MoveAroundStation();
                    // Waiting outside
                }
            }
        }
    }

    public void FadeIn() {
        GetComponent<Animator>().SetTrigger("FadeIn");
        transform.Find("Marker").GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public void FadeOut() {
        GetComponent<Animator>().SetTrigger("FadeOut");
        transform.Find("Marker").GetComponent<Animator>().SetTrigger("FadeOut");
        AudioSource.PlayClipAtPoint(mailReceivedAC, transform.localPosition);
    }

    public void PopUp() {
        GetComponent<Animator>().SetTrigger("PopUp");
        transform.Find("Marker").GetComponent<Animator>().SetTrigger("PopUp");
    }
}

