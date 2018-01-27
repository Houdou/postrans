﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station {
    public static int IDCounter = -1;
    
    private GameObject station;
    public readonly int ID;

    public Vector2 Pos {
        get {
            return station.transform.position;
        }
    }

    public readonly bool IsHome;

    public List<Road> roads;
        

    public Station(GameObject obj, bool isHome) {
        station = obj;        
        ID = ++IDCounter;
        IsHome = isHome;
        
        roads = new List<Road>();
    }

    public bool AddRoad(Road road) {
        return true;
    }

    public bool RemoveRoad(Road road) {
        return true;
    }


}

public class StationController : MonoBehaviour {
    public Station model;
    public SpriteRenderer sprite;

    public bool isInitialized;

    private void Awake() {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Initialize(bool isHome) {
        isInitialized = true;
        model = new Station(gameObject, isHome);
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if(!isInitialized) { return; }


    }
}